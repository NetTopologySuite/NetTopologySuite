namespace NetTopologySuite.IO.TopoJSON.Geometries
{
    internal class TopoMultiLineString: TopoCurve
    {
        public TopoMultiLineString(string type, int[][][] arcs)
            : base(type, arcs)
        {
        }
    }
}