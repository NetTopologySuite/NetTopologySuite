using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Represents a <see cref="ILineString"/> which can be modified to a simplified shape.
    /// Every line segment in the parent LineString is represented as a <see cref="TaggedLineSegment"/>.
    /// This class provides an attribute which specifies the minimum allowable length
    /// for the modified result.
    /// </summary>
    public class TaggedLineString
    {
        private readonly ILineString _parentLine;
        private TaggedLineSegment[] _segs;
        private readonly IList<LineSegment> _resultSegs = new List<LineSegment>();
        private readonly int _minimumSize;

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
            _parentLine = parentLine;
            _minimumSize = minimumSize;
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public int MinimumSize
        {
            get
            {
                return _minimumSize;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILineString Parent
        {
            get
            {
                return _parentLine;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] ParentCoordinates
        {
            get
            {
                return _parentLine.Coordinates;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] ResultCoordinates
        {
            get
            {
                return ExtractCoordinates(_resultSegs);
            }
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
        /// 
        /// </summary>
        private void Init()
        {
            ICoordinate[] pts = _parentLine.Coordinates;
            _segs = new TaggedLineSegment[pts.Length - 1];
            for (int i = 0; i < pts.Length - 1; i++)
            {
                TaggedLineSegment seg = new TaggedLineSegment(pts[i], pts[i + 1], _parentLine, i);
                _segs[i] = seg;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TaggedLineSegment[] Segments
        {
            get
            {
                return _segs;
            }
        }

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
            return _parentLine.Factory.CreateLineString(ExtractCoordinates(_resultSegs));        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILinearRing AsLinearRing()
        {
            return _parentLine.Factory.CreateLinearRing(ExtractCoordinates(_resultSegs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        private static ICoordinate[] ExtractCoordinates(IList<LineSegment> segs)
        {
            ICoordinate[] pts = new ICoordinate[segs.Count + 1];
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
