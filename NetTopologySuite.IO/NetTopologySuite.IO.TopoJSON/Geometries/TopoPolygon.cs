namespace NetTopologySuite.IO.Geometries
{
    internal class TopoPolygon : TopoCurve
    {
        public TopoPolygon(string type, int[][][] arcs)
            : base(type, arcs) { }
    }
}