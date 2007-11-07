using System;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a line (sequence of points) using
    /// the standard Douglas-Peucker algorithm.
    /// </summary>
    public class DouglasPeuckerLineSimplifier
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static ICoordinate[] Simplify(ICoordinate[] pts, Double distanceTolerance)
        {
            DouglasPeuckerLineSimplifier simp = new DouglasPeuckerLineSimplifier(pts);
            simp.DistanceTolerance = distanceTolerance;
            return simp.Simplify();
        }

        private ICoordinate[] pts;
        private Boolean[] usePt;
        private Double distanceTolerance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        public DouglasPeuckerLineSimplifier(ICoordinate[] pts)
        {
            this.pts = pts;
        }

        /// <summary>
        /// 
        /// </summary>
        public Double DistanceTolerance
        {
            get { return distanceTolerance; }
            set { distanceTolerance = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ICoordinate[] Simplify()
        {
            usePt = new Boolean[pts.Length];
            for (Int32 i = 0; i < pts.Length; i++)
            {
                usePt[i] = true;
            }

            SimplifySection(0, pts.Length - 1);
            CoordinateList coordList = new CoordinateList();
            for (Int32 i = 0; i < pts.Length; i++)
            {
                if (usePt[i])
                {
                    coordList.Add(new Coordinate(pts[i]));
                }
            }
            return coordList.ToCoordinateArray();
        }

        private LineSegment seg = new LineSegment();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void SimplifySection(Int32 i, Int32 j)
        {
            if ((i + 1) == j)
            {
                return;
            }
            seg.P0 = pts[i];
            seg.P1 = pts[j];
            Double maxDistance = -1.0;
            Int32 maxIndex = i;
            for (Int32 k = i + 1; k < j; k++)
            {
                Double distance = seg.Distance(pts[k]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }
            if (maxDistance <= DistanceTolerance)
            {
                for (Int32 k = i + 1; k < j; k++)
                {
                    usePt[k] = false;
                }
            }
            else
            {
                SimplifySection(i, maxIndex);
                SimplifySection(maxIndex, j);
            }
        }
    }
}