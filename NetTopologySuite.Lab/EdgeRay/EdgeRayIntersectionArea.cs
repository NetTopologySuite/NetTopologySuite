using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;

namespace NetTopologySuite.EdgeRay
{
    public class EdgeRayIntersectionArea
    {
        public static double GetArea(IGeometry geom0, IGeometry geom1)
        {
            var area = new EdgeRayIntersectionArea(geom0, geom1);
            return area.Area;
        }

        private IGeometry _geomA;
        private IGeometry _geomB;
        double _area;

        public EdgeRayIntersectionArea(IGeometry geom0, IGeometry geom1)
        {
            _geomA = geom0;
            _geomB = geom1;
        }

        public double Area
        {
            get
            {
                // TODO: for now assume poly is CW and has no holes

                AddIntersections();
                AddResultVertices(_geomA, _geomB);
                AddResultVertices(_geomB, _geomA);
                return _area;
            }
        }

        private void AddIntersections()
        {
            var seqA = GetVertices(_geomA);
            var seqB = GetVertices(_geomB);

            // NTS-specific: these arrays were only written to, never read from.
            ////bool[] isIntersected0 = new bool[seqA.Count - 1];
            ////bool[] isIntersected1 = new bool[seqB.Count - 1];

            bool isCCWA = Orientation.IsCCW(seqA);
            bool isCCWB = Orientation.IsCCW(seqB);

            // Compute rays for all intersections
            var li = new RobustLineIntersector();

            for (int i = 0; i < seqA.Count - 1; i++)
            {
                var a0 = seqA.GetCoordinate(i);
                var a1 = seqA.GetCoordinate(i + 1);

                if (isCCWA)
                {
                    // flip segment orientation
                    (a0, a1) = (a1, a0);
                }

                for (int j = 0; j < seqB.Count - 1; j++)
                {
                    var b0 = seqB.GetCoordinate(j);
                    var b1 = seqB.GetCoordinate(j + 1);

                    if (isCCWB)
                    {
                        // flip segment orientation
                        (b0, b1) = (b1, b0);
                    }

                    li.ComputeIntersection(a0, a1, b0, b1);
                    if (li.HasIntersection)
                    {
                        // NTS-specific: these arrays were only written to, never read from.
                        ////isIntersected0[i] = true;
                        ////isIntersected1[j] = true;

                        /*
                         * With both rings oriented CW (effectively)
                         * There are two situations for segment intersections:
                         * 
                         * 1) A entering B, B exiting A => rays are IP-A1:R, IP-B0:L
                         * 2) A exiting B, B entering A => rays are IP-A0:L, IP-B1:R
                         * (where :L/R indicates result is to the Left or Right).
                         * 
                         * Use full edge to compute direction, for accuracy.
                         */
                        var intPt = li.GetIntersection(0);

                        bool isAenteringB = OrientationIndex.CounterClockwise == Orientation.Index(a0, a1, b1);

                        if (isAenteringB)
                        {
                            _area += EdgeRay.AreaTerm(intPt, a0, a1, true);
                            _area += EdgeRay.AreaTerm(intPt, b1, b0, false);
                        }
                        else
                        {
                            _area += EdgeRay.AreaTerm(intPt, a1, a0, false);
                            _area += EdgeRay.AreaTerm(intPt, b0, b1, true);
                        }
                    }
                }
            }
        }

        private void AddResultVertices(IGeometry geom0, IGeometry geom1)
        {
            /*
             * Compute rays originating at vertices inside the resultant
             * (i.e. A vertices inside B, and B vertices inside A)
             */
            var locator = new IndexedPointInAreaLocator(geom1);
            var seq = GetVertices(geom0);
            bool isCW = !Orientation.IsCCW(seq);
            for (int i = 0; i < seq.Count - 1; i++)
            {
                var vPrev = i == 0 ? seq.GetCoordinate(seq.Count - 2) : seq.GetCoordinate(i - 1);
                var v = seq.GetCoordinate(i);
                var vNext = seq.GetCoordinate(i + 1);
                if (Location.Interior == locator.Locate(v))
                {
                    _area += EdgeRay.AreaTerm(v, vPrev, !isCW);
                    _area += EdgeRay.AreaTerm(v, vNext, isCW);
                }
            }
        }

        private ICoordinateSequence GetVertices(IGeometry geom)
        {
            var poly = (IPolygon)geom;
            var seq = poly.ExteriorRing.CoordinateSequence;
            return seq;
        }
    }
}
