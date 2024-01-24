using System.Collections.Generic;
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

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/>.
        /// The <see cref="MinimumSize"/> is set to <c>2</c> and <see cref="PreserveEndpoint"/> is <c>true</c>.
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        public TaggedLineString(LineString parentLine) : this(parentLine, 2) { }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/> and
        /// <paramref name="minimumSize"/> values. The value for <see cref="PreserveEndpoint"/> is <c>true</c>
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        /// <param name="minimumSize">The number of vertices to must be kept.</param>
        public TaggedLineString(LineString parentLine, int minimumSize)
            : this(parentLine, minimumSize, true)
        { }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="parentLine"/> and
        /// <paramref name="minimumSize"/> values.
        /// </summary>
        /// <param name="parentLine">The <c>LineString</c> that is to be simplified.</param>
        /// <param name="minimumSize">The number of vertices to must be kept.</param>
        /// <param name="isPreserveEndpoint">A flag indicating if the endpoint should be preserved</param>
        public TaggedLineString(LineString parentLine, int minimumSize, bool isPreserveEndpoint)
        {
            _parentLine = parentLine;
            _minimumSize = minimumSize;
            PreserveEndpoint = isPreserveEndpoint;
            Init();
        }

        /// <summary>
        /// Gets a value indicating if the endpoints are to be preserved.
        /// </summary>
        public bool PreserveEndpoint { get; private set; }

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
        ///
        /// </summary>
        /// <param name="seg"></param>
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

        internal void RemoveRingEndpoint()
        {
            var firstSeg = _resultSegs[0];
            var lastSeg = _resultSegs[_resultSegs.Count - 1];

            firstSeg.P0 = lastSeg.P0;
            _resultSegs.RemoveAt(_resultSegs.Count - 1);
        }

    }
}
