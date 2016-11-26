using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    ///     Simplifies a line (sequence of points) using
    ///     the standard Douglas-Peucker algorithm.
    /// </summary>
    public class DouglasPeuckerLineSimplifier
    {
        private readonly Coordinate[] _pts;

        private readonly LineSegment _seg = new LineSegment();
        private bool[] _usePt;

        /// <summary>
        /// </summary>
        /// <param name="pts"></param>
        public DouglasPeuckerLineSimplifier(Coordinate[] pts)
        {
            _pts = pts;
        }

        /// <summary>
        /// </summary>
        public double DistanceTolerance { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
        {
            var simp = new DouglasPeuckerLineSimplifier(pts);
            simp.DistanceTolerance = distanceTolerance;
            return simp.Simplify();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public Coordinate[] Simplify()
        {
            _usePt = new bool[_pts.Length];
            for (var i = 0; i < _pts.Length; i++)
                _usePt[i] = true;

            SimplifySection(0, _pts.Length - 1);
            var coordList = new CoordinateList();
            for (var i = 0; i < _pts.Length; i++)
                if (_usePt[i])
                    coordList.Add(new Coordinate(_pts[i]));
            return coordList.ToCoordinateArray();
        }

        /// <summary>
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void SimplifySection(int i, int j)
        {
            if (i + 1 == j)
                return;
            _seg.P0 = _pts[i];
            _seg.P1 = _pts[j];
            var maxDistance = -1.0;
            var maxIndex = i;
            for (var k = i + 1; k < j; k++)
            {
                var distance = _seg.Distance(_pts[k]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    maxIndex = k;
                }
            }
            if (maxDistance <= DistanceTolerance)
                for (var k = i + 1; k < j; k++)
                    _usePt[k] = false;
            else
            {
                SimplifySection(i, maxIndex);
                SimplifySection(maxIndex, j);
            }
        }
    }
}