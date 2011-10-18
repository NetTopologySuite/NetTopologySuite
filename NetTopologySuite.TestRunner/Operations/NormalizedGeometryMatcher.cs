using GeoAPI.Geometries;

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
            get { return _tolerance; }
            set { _tolerance = value; }
        }

        public bool Match(IGeometry a, IGeometry b)
        {
            var aClone = (IGeometry) a.Clone();
            var bClone = (IGeometry) b.Clone();
            aClone.Normalize();
            bClone.Normalize();
            return aClone.EqualsExact(bClone, _tolerance);
        }

    }
}