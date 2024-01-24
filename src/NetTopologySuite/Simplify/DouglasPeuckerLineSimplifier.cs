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
        /// Simplifies a series of <see cref="Coordinate"/>s. The series' endpoints are preserved.
        /// </summary>
        /// <param name="pts">The series of <c>Coordinate</c>s to simplify</param>
        /// <param name="distanceTolerance">A simplification tolerance distance</param>
        /// <returns>The simplified series of <c>Coordinate</c>s</returns>
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance)
            => Simplify(pts, distanceTolerance, true);

        /// <summary>
        /// Simplifies a series of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="pts">The series of <c>Coordinate</c>s to simplify</param>
        /// <param name="distanceTolerance">A simplification tolerance distance</param>
        /// <param name="isPreserveEndpoint">A flag indicating if the endpoint should be preserved</param>
        /// <returns></returns>
        public static Coordinate[] Simplify(Coordinate[] pts, double distanceTolerance, bool isPreserveEndpoint)
        {
            var simp = new DouglasPeuckerLineSimplifier(pts)
            {
                DistanceTolerance = distanceTolerance,
                PreserveEndpoint = isPreserveEndpoint
            };
            return simp.Simplify();
        }

        private readonly Coordinate[] _pts;
        private bool[] _usePt;

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="pts"/> array of coordinates
        /// </summary>
        /// <param name="pts">An array of coordinates</param>
        public DouglasPeuckerLineSimplifier(Coordinate[] pts)
        {
            _pts = pts;
        }

        /// <summary>
        /// The distance tolerance for the simplification.
        /// </summary>
        public double DistanceTolerance { get; set; }

        /// <summary>
        /// Gets a flag indicating if the endpoint should be preserved
        /// </summary>
        public bool PreserveEndpoint { get; private set; }

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

            var coordList = new CoordinateList(_pts.Length);
            for (int i = 0; i < _pts.Length; i++)
                if (_usePt[i])
                    coordList.Add(_pts[i].Copy());

            if (!PreserveEndpoint && CoordinateArrays.IsRing(_pts))
            {
                SimplifyRingEndpoint(coordList);
            }

            return coordList.ToCoordinateArray();
        }

        private void SimplifyRingEndpoint(CoordinateList pts)
        {
            //-- avoid collapsing triangles
            if (pts.Count < 4)
                return;
            //-- base segment for endpoint
            _seg.P0 = pts[1];
            _seg.P1 = pts[pts.Count - 2];
            double distance = _seg.Distance(pts[0]);
            if (distance <= DistanceTolerance)
            {
                pts.RemoveAt(0);
                pts.RemoveAt(pts.Count - 1);
                pts.CloseRing();
            }
        }

        private readonly LineSegment _seg = new LineSegment();

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
