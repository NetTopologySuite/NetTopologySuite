namespace NetTopologySuite.IO.TopoJSON.Geometries
{
    internal class TopoPolygon : TopoCurve
    {
        public TopoPolygon(string type, int[][][] arcs) : base(type, arcs)
        {
        }
    }
}