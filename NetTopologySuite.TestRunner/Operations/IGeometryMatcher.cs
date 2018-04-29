using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Operations
{
    /// <summary>
    /// An interface for classes which can determine whether
    /// two geometries match, within a given tolerance.
    /// </summary>
    /// <author>mbdavis</author>
    public interface IGeometryMatcher
    {
        double Tolerance { get; set; }
        bool Match(IGeometry a, IGeometry b);
    }
}
