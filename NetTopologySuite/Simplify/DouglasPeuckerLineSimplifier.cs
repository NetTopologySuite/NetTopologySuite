using GeoAPI.Geometries;
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
        public static ICoordinate[] Simplify(ICoordinate[] pts, double distanceTolerance)
        {
            DouglasPeuckerLineSimplifier simp = new DouglasPeuckerLineSimplifier(pts);
            simp.DistanceTolerance = distanceTolerance;
            return simp.Simplify();
        }

        private ICoordinate[] pts;
        private bool[] usePt;
        private double distanceTolerance;       

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
        public double DistanceTolerance
        {
            get
            {
                return distanceTolerance;
            }
            set
            {
                distanceTolerance = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ICoordinate[] Simplify()
        {
            usePt = new bool[pts.Length];
            for (int i = 0; i < pts.Length; i++)
                usePt[i] = true;
            
            SimplifySection(0, pts.Length - 1);
            CoordinateList coordList = new CoordinateList();
            for (int i = 0; i < pts.Length; i++)            
                if (usePt[i])
                    coordList.Add(new Coordinate(pts[i]));            
            return coordList.ToCoordinateArray();
        }

        private LineSegment seg = new LineSegment();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void SimplifySection(int i, int j)
        {
            if ((i + 1) == j)
                return;            
            seg.P0 = pts[i];
            seg.P1 = pts[j];
            double maxDistance = -1.0;
            int maxIndex = i;
            for (int k = i + 1; k < j; k++)
            {
                double distance = seg.Distance(pts[k]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }
            if (maxDistance <= DistanceTolerance)
                for (int k = i + 1; k < j; k++)                
                    usePt[k] = false;                            
            else
            {
                SimplifySection(i, maxIndex);
                SimplifySection(maxIndex, j);
            }
        }
    }
}
