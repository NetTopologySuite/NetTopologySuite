using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Operations
{
    public class NormalizedGeometryMatcher : IGeometryMatcher
    {
        private double _tolerance;

        /*
        public NormalizedGeometryMatcher()
        {

        }
         */

        public double Tolerance
        {
            get => _tolerance;
            set => _tolerance = value;
        }

        public bool Match(Geometry a, Geometry b)
        {
            var aClone = (Geometry) a.Copy();
            var bClone = (Geometry) b.Copy();
            aClone.Normalize();
            bClone.Normalize();
            return aClone.EqualsExact(bClone, _tolerance);
        }

    }
}