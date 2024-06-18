using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Represents a <see cref="LineString"/> which can be modified to a simplified shape.
    /// This class provides an attribute which specifies the minimum allowable length
    /// for the modified result.
    /// </summary>
    public class TaggedLineString
    {
        private readonly LineString _parentLine;
        private TaggedLineSegment[] _segs;
        private readonly IList<LineSegment> _resultSegs = new List<LineSegment>();
        private readonly int _minimumSize;
        private readonly bool _isRing;

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/>.
        /// The <see cref="MinimumSize"/> is set to <c>2</c> and <see cref="PreserveEndpoint"/> is <c>true</c>.
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        [Obsolete]
        public TaggedLineString(LineString parentLine) : this(parentLine, 2) { }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/> and
        /// <paramref name="minimumSize"/> values. The value for <see cref="IsRing"/> is <c>LineString.IsRing</c>
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        /// <param name="minimumSize">The number of vertices to must be kept.</param>
        [Obsolete]
        public TaggedLineString(LineString parentLine, int minimumSize)
            : this(parentLine, minimumSize, parentLine.IsRing)
        { }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/> and
        /// <paramref name="minimumSize"/> values.
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        /// <param name="minimumSize">The number of vertices to must be kept.</param>
        /// <param name="isRing">A flag indicating if the <paramref name="parentLine"/> forms a ring</param>
        public TaggedLineString(LineString parentLine, int minimumSize, bool isRing)
        {
            _parentLine = parentLine;
            _minimumSize = minimumSize;
            IsRing = isRing;
            Init();
        }

        /// <summary>
        /// Gets a value indicating if the endpoints are to be preserved.
        /// </summary>
        public bool IsRing { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public int MinimumSize => _minimumSize;

        /// <summary>
        ///
        /// </summary>
        public LineString Parent => _parentLine;

        /// <summary>
        ///
        /// </summary>
        public Coordinate[] ParentCoordinates => _parentLine.Coordinates;

        /// <summary>
        ///
        /// </summary>
        public Coordinate[] ResultCoordinates => ExtractCoordinates(_resultSegs);

        /// <summary>
        /// Gets the <paramref name="i"/>'th <c>Coordinate</c> of <see cref="Parent"/> line.
        /// </summary>
        /// <param name="i">The index of the coordinate to get</param>
        /// <returns>The <paramref name="i"/>'th <c>Coordinate</c> of <see cref="Parent"/> line.</returns>
        public Coordinate GetCoordinate(int i)
        {
            return _parentLine.GetCoordinateN(i);
        }

        /// <summary>
        /// Gets a value indicating the number of points of the <see cref="Parent"/> line.
        /// </summary>
        public int Count => _parentLine.NumPoints;

        /// <summary>
        /// Gets a <c>Coordinate</c> of the <see cref="Parent"/> line.
        /// </summary>
        /// <returns>A <c>Coordinate</c> of the <see cref="Parent"/> line.</returns>
        public Coordinate GetComponentPoint()
        {
            return ParentCoordinates[1];
        }
        /// <summary>
        ///
        /// </summary>
        public int ResultSize
        {
            get
            {
                int resultSegsSize = _resultSegs.Count;
                return resultSegsSize == 0 ? 0 : resultSegsSize + 1;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public TaggedLineSegment GetSegment(int i)
        {
            return _segs[i];
        }

        /// <summary>
        /// Gets a segment of the result list.
        /// Negative indexes can be used to retrieve from the end of the list.
        /// </summary>
        /// <param name="i">The segment index to retrieve</param>
        /// <returns>The result segment</returns>
        public LineSegment GetResultSegment(int i)
        {
            int index = i;
            if (i < 0)
            {
                index = _resultSegs.Count + i;
            }
            return _resultSegs[index];
        }

        /// <summary>
        ///
        /// </summary>
        private void Init()
        {
            var pts = _parentLine.Coordinates;
            _segs = new TaggedLineSegment[pts.Length - 1];
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var seg = new TaggedLineSegment(pts[i], pts[i + 1], _parentLine, i);
                _segs[i] = seg;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public TaggedLineSegment[] Segments => _segs;

        /// <summary>
        /// Add a simplified segment to the result.
        /// This assumes simplified segments are computed in the order
        /// they occur in the line.
        /// </summary>
        /// <param name="seg">The result segment to add.</param>
        public void AddToResult(LineSegment seg)
        {
            _resultSegs.Add(seg);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public LineString AsLineString()
        {
            var coordinates = ExtractCoordinates(_resultSegs);
            return _parentLine.Factory.CreateLineString(coordinates);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public LinearRing AsLinearRing()
        {
            var coordinates = ExtractCoordinates(_resultSegs);
            return _parentLine.Factory.CreateLinearRing(coordinates);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        private static Coordinate[] ExtractCoordinates(IList<LineSegment> segs)
        {
            var pts = new Coordinate[segs.Count + 1];
            LineSegment seg = null;
            for (int i = 0; i < segs.Count; i++)
            {
                seg = segs[i];
                pts[i] = seg.P0;
            }
            // add last point
            if (seg != null) pts[pts.Length - 1] = seg.P1;
            return pts;
        }

        [Obsolete("Will be removed in a future version")]
        internal void RemoveRingEndpoint()
        {
            RemoveRingEndpoint(out _);
        }

        internal void RemoveRingEndpoint(out LineSegment item)
        {
            var firstSeg = _resultSegs[0];
            var lastSeg = _resultSegs[_resultSegs.Count - 1];

            firstSeg.P0 = lastSeg.P0;
            _resultSegs.RemoveAt(_resultSegs.Count - 1);

            item = firstSeg;
        }
    }
}
