using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
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
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
        {
            DouglasPeuckerLineSimplifier simp = new DouglasPeuckerLineSimplifier(pts);
            simp.DistanceTolerance = distanceTolerance;
            return simp.Simplify();
        }

        private readonly Coordinate[] _pts;
        private bool[] _usePt;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        public DouglasPeuckerLineSimplifier(Coordinate[] pts)
        {
            _pts = pts;
        }

        /// <summary>
        /// 
        /// </summary>
        public double DistanceTolerance { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Coordinate[] Simplify()
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

        private readonly LineSegment _seg = new LineSegment();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void SimplifySection(int i, int j)
        {
            if ((i + 1) == j)
                return;
            _seg.P0 = _pts[i];
            _seg.P1 = _pts[j];
            double maxDistance = -1.0;
            int maxIndex = i;
            for (int k = i + 1; k < j; k++)
            {
                double distance = _seg.Distance(_pts[k]);
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
