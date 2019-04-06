namespace GeoAPI.Geometries
{
    public interface IPolygon : ISurface, IPolygonal
    {
        ILineString ExteriorRing { get; }

        ILinearRing Shell { get; }

        int NumInteriorRings { get; }

        ILineString[] InteriorRings { get; }

        ILineString GetInteriorRingN(int n);

        ILinearRing[] Holes { get; }  
    }
}
