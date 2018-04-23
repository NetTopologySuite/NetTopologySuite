using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Represents a <see cref="ILineString"/> which can be modified to a simplified shape.
    /// This class provides an attribute which specifies the minimum allowable length
    /// for the modified result.
    /// </summary>
    public class TaggedLineString
    {
        private readonly IList<LineSegment> _resultSegs = new List<LineSegment>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        public TaggedLineString(ILineString parentLine) : this(parentLine, 2) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        /// <param name="minimumSize"></param>
        public TaggedLineString(ILineString parentLine, int minimumSize)
        {
            Parent = parentLine;
            MinimumSize = minimumSize;
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public int MinimumSize { get; }

        /// <summary>
        /// 
        /// </summary>
        public ILineString Parent { get; }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate[] ParentCoordinates => Parent.Coordinates;

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
            return Segments[i];
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            Coordinate[] pts = Parent.Coordinates;
            Segments = new TaggedLineSegment[pts.Length - 1];
            for (int i = 0; i < pts.Length - 1; i++)
            {
                TaggedLineSegment seg = new TaggedLineSegment(pts[i], pts[i + 1], Parent, i);
                Segments[i] = seg;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TaggedLineSegment[] Segments { get; private set; }

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
        public ILineString AsLineString()
        {
            Coordinate[] coordinates = ExtractCoordinates(_resultSegs);
            return Parent.Factory.CreateLineString(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILinearRing AsLinearRing()
        {
            Coordinate[] coordinates = ExtractCoordinates(_resultSegs);
            return Parent.Factory.CreateLinearRing(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        private static Coordinate[] ExtractCoordinates(IList<LineSegment> segs)
        {
            Coordinate[] pts = new Coordinate[segs.Count + 1];
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
    }
}
