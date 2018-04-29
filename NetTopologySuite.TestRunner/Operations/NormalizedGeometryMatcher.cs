using GeoAPI.Geometries;
namespace Open.Topology.TestRunner.Operations
{
    public class NormalizedGeometryMatcher : IGeometryMatcher
    {
        /*
        public NormalizedGeometryMatcher()
        {
        }
         */
        public double Tolerance { get; set; }
        public bool Match(IGeometry a, IGeometry b)
        {
            var aClone = (IGeometry) a.Copy();
            var bClone = (IGeometry) b.Copy();
            aClone.Normalize();
            bClone.Normalize();
            return aClone.EqualsExact(bClone, Tolerance);
        }
    }
}
