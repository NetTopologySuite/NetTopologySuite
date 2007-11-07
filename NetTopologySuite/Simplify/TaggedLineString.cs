using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// 
    /// </summary>
    public class TaggedLineString
    {
        private ILineString parentLine;
        private TaggedLineSegment[] segs;
        private IList resultSegs = new ArrayList();
        private Int32 minimumSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        public TaggedLineString(ILineString parentLine) : this(parentLine, 2) {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentLine"></param>
        /// <param name="minimumSize"></param>
        public TaggedLineString(ILineString parentLine, Int32 minimumSize)
        {
            this.parentLine = parentLine;
            this.minimumSize = minimumSize;
            Init();
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 MinimumSize
        {
            get { return minimumSize; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILineString Parent
        {
            get { return parentLine; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] ParentCoordinates
        {
            get { return parentLine.Coordinates; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate[] ResultCoordinates
        {
            get { return ExtractCoordinates(resultSegs); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 ResultSize
        {
            get
            {
                Int32 resultSegsSize = resultSegs.Count;
                return resultSegsSize == 0 ? 0 : resultSegsSize + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public TaggedLineSegment GetSegment(Int32 i)
        {
            return segs[i];
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            ICoordinate[] pts = parentLine.Coordinates;
            segs = new TaggedLineSegment[pts.Length - 1];
            for (Int32 i = 0; i < pts.Length - 1; i++)
            {
                TaggedLineSegment seg = new TaggedLineSegment(pts[i], pts[i + 1], parentLine, i);
                segs[i] = seg;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TaggedLineSegment[] Segments
        {
            get { return segs; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seg"></param>
        public void AddToResult(LineSegment seg)
        {
            resultSegs.Add(seg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILineString AsLineString()
        {
            return parentLine.Factory.CreateLineString(ExtractCoordinates(resultSegs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ILinearRing AsLinearRing()
        {
            return parentLine.Factory.CreateLinearRing(ExtractCoordinates(resultSegs));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segs"></param>
        /// <returns></returns>
        private static ICoordinate[] ExtractCoordinates(IList segs)
        {
            ICoordinate[] pts = new ICoordinate[segs.Count + 1];
            LineSegment seg = null;
            for (Int32 i = 0; i < segs.Count; i++)
            {
                seg = (LineSegment) segs[i];
                pts[i] = seg.P0;
            }
            // add last point
            pts[pts.Length - 1] = seg.P1;
            return pts;
        }
    }
}