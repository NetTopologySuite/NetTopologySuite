using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    ///  An index which creates unique <see cref="HotPixel"/>s for provided points,
    /// and performs range queries on them.
    /// The points passed to the index do not needed to be
    /// rounded to the specified scale factor; this is done internally
    /// when creating the HotPixels for them.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class HotPixelIndex
    {
        private readonly PrecisionModel _precModel;
        private readonly double _scaleFactor;

        /*
         * Use a kd-tree to index the pixel centers for optimum performance.
         * Since HotPixels have an extent, range queries to the
         * index must enlarge the query range by a suitable value 
         * (using the pixel width is safest).
         */
        private readonly KdTree<HotPixel> _index = new KdTree<HotPixel>();

        /// <summary>
        /// Creates a new hot pixel index using the provided <see cref="PrecisionModel"/>.
        /// </summary>
        /// <param name="pm">The precision model</param>
        public HotPixelIndex(PrecisionModel pm)
        {
            _precModel = pm;
            _scaleFactor = pm.Scale;
        }


        /// <summary>
        /// Utility class to enumerate through a shuffled array of
        /// <see cref="Coordinate"/>s using the
        /// <a href="https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle">Fisher-Yates shuffle algorithm</a>
        /// </summary>
        private sealed class ShuffledCoordinates : IEnumerable<Coordinate>
        {
            private readonly Coordinate[] _pts;

            /// <summary>
            /// Creates an instance of this class using the provided <see cref="Coordinate"/>s.
            /// </summary>
            /// <param name="pts">An array of coordinates</param>
            public ShuffledCoordinates(Coordinate[] pts)
            {
                _pts = pts;
            }

            private sealed class ShuffledCoordinatesEnumerator : IEnumerator<Coordinate>
            {
                private readonly Random _rnd = new Random(13);
                private readonly Coordinate[] _pts;
                private readonly int[] _indices;
                private int _index;

                /// <summary>
                /// Creates an instance of this class using the provided <see cref="Coordinate"/>s.
                /// </summary>
                /// <param name="pts">An array of coordinates</param>
                public ShuffledCoordinatesEnumerator(Coordinate[] pts)
                {
                    _pts = pts;
                    _indices = ArrayPool<int>.Shared.Rent(pts?.Length ?? 0);
                    Reset();
                }

                /// <inheritdoc cref="IEnumerator.MoveNext()"/>
                public bool MoveNext()
                {
                    if (_index < 1)
                    {
                        Current = null;
                        return false;
                    }

                    int j = _rnd.Next(_index);
                    Current = _pts[_indices[j]];
                    _indices[j] = _indices[--_index];
                    return true;
                }

                /// <inheritdoc cref="IEnumerator.Reset()"/>
                public void Reset()
                {
                    for (int i = 0; i < _indices.Length; i++)
                        _indices[i] = i;
                    _index = _pts?.Length ?? 0;
                    Current = null;
                    // Initialize Rnd?
                }

                /// <inheritdoc cref="IEnumerator{T}.Current"/>
                public Coordinate Current
                {
                    get;
                    private set;
                }

                object IEnumerator.Current => Current;

                /// <inheritdoc cref="IDisposable.Dispose()"/>
                public void Dispose()
                {
                    ArrayPool<int>.Shared.Return(_indices);
                }
            }

            /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
            public IEnumerator<Coordinate> GetEnumerator()
            {
                return new ShuffledCoordinatesEnumerator(_pts);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        /// <summary>
        /// Adds a series of points as non-node pixels
        /// </summary>
        /// <param name="pts">The points to add</param>
        public void Add(IEnumerable<Coordinate> pts)
        {
            /*
             * Attempt to shuffle the points before adding.
             * This avoids having long monotonic runs of points
             * causing an unbalanced KD-tree, which would create
             * performance and robustness issues.
             */
            if (pts is Coordinate[] ptsA)
                pts = new ShuffledCoordinates(ptsA);

            foreach (var pt in pts)
            {
                Add(pt);
            }
        }

        /// <summary>
        /// Adds a list of points as node pixels.
        /// </summary>
        /// <param name="pts">The points to add</param>
        public void AddNodes(IEnumerable<Coordinate> pts)
        {
            /*
             * Node points are not shuffled, since they are
             * added after the vertex points, and hence the KD-tree should 
             * be reasonably balanced already.
             */
            foreach (var pt in pts)
            {
                var hp = Add(pt);
                hp.IsNode = true;
            }
        }

        /// <summary>
        /// Adds a point as a Hot Pixel. <br/>
        /// If the point has been added already, it is marked as a node.
        /// </summary>
        /// <param name="p">The point to add</param>
        /// <returns>The hot-pixel for the point</returns>
        public HotPixel Add(Coordinate p)
        {
            // TODO: is there a faster way of doing this?
            var pRound = Round(p);

            var hp = Find(pRound);
            /*
             * Hot Pixels which are added more than once 
             * must have more than one vertex in them
             * and thus must be nodes.
             */
            if (hp != null)
            {
                hp.IsNode = true;
                return hp;
            }

            /*
             * A pixel containing the point was not found, so create a new one.
             * It is initially set to NOT be a node
             * (but may become one later on).
             */
            hp = new HotPixel(pRound, _scaleFactor);
            _index.Insert(hp.Coordinate, hp);
            return hp;
        }

        private HotPixel Find(Coordinate pixelPt)
        {
            var kdNode = _index.Query(pixelPt);
            if (kdNode == null)
                return null;
            return kdNode.Data;
        }

        private Coordinate Round(Coordinate pt)
        {
            var p2 = pt.Copy();
            _precModel.MakePrecise(p2);
            return p2;
        }

        /// <summary>
        /// Visits all the hot pixels which may intersect a segment (p0-p1).
        /// The visitor must determine whether each hot pixel actually intersects
        /// the segment.
        /// </summary>
        /// <param name="p0">The segment start point</param>
        /// <param name="p1">The segment end point</param>
        /// <param name="visitor">The visitor to apply</param>
        public void Query(Coordinate p0, Coordinate p1, IKdNodeVisitor<HotPixel> visitor)
        {
            var queryEnv = new Envelope(p0, p1);
            // expand query range to account for HotPixel extent
            // expand by full width of one pixel to be safe
            queryEnv.ExpandBy(1.0 / _scaleFactor);
            _index.Query(queryEnv, visitor);
        }
    }
}
