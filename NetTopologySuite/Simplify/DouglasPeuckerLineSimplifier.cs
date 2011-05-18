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

        private readonly ICoordinate[] _pts;
        private bool[] _usePt;
        private double _distanceTolerance;       

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        public DouglasPeuckerLineSimplifier(ICoordinate[] pts)
        {
            _pts = pts;
        }

        /// <summary>
        /// 
        /// </summary>
        public double DistanceTolerance
        {
            get
            {
                return _distanceTolerance;
            }
            set
            {
                _distanceTolerance = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ICoordinate[] Simplify()
        {
            _usePt = new bool[_pts.Length];
            for (int i = 0; i < _pts.Length; i++)
                _usePt[i] = true;
            
            SimplifySection(0, _pts.Length - 1);
            CoordinateList coordList = new CoordinateList();
            for (int i = 0; i < _pts.Length; i++)            
                if (_usePt[i])
                    coordList.Add(new Coordinate(_pts[i]));            
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
            seg.P0 = _pts[i];
            seg.P1 = _pts[j];
            double maxDistance = -1.0;
            int maxIndex = i;
            for (int k = i + 1; k < j; k++)
            {
                double distance = seg.Distance(_pts[k]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }
            if (maxDistance <= DistanceTolerance)
                for (int k = i + 1; k < j; k++)                
                    _usePt[k] = false;                            
            else
            {
                SimplifySection(i, maxIndex);
                SimplifySection(maxIndex, j);
            }
        }
    }
}
