using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// An index which creates <see cref="HotPixel"/>s for provided points,
    /// and allows performing range queries on them.
    /// </summary>
    /// <author>Martin Davis</author>
    class HotPixelIndex
    {
        private readonly PrecisionModel _precModel;
        private readonly double _scaleFactor;

        /*
         * Use a kd-tree to index the pixel centers for optimum performance.
         * Since HotPixels have an extent, queries to the
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
        /// Adds a series of points as non-node pixels
        /// </summary>
        /// <param name="pts">The points to add</param>
        public void Add(IEnumerable<Coordinate> pts)
        {
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
