namespace NetTopologySuite.IO.Sdo
{
    internal enum SdoEType
    {
        Unknown = -1,
        Coordinate = 1,
        Line = 2,
        Polygon = 3,
        PolygonExterior = 1003,
        PolygonInterior = 2003
    }
}