using GeoAPI.Geometries;
using NetTopologySuite.LinearReferencing;

namespace Open.Topology.TestRunner.Functions
{
    public static class LinearReferencingFunctions
    {
        public static IGeometry ExtractPoint(IGeometry g, double index)
        {
            var ll = new LengthIndexedLine(g);
            var p = ll.ExtractPoint(index);
            return g.Factory.CreatePoint(p);
        }

        public static IGeometry ExtractLine(IGeometry g, double start, double end)
        {
            var ll = new LengthIndexedLine(g);
            return ll.ExtractLine(start, end);
        }

        public static IGeometry Project(IGeometry g, IGeometry g2)
        {
            var ll = new LengthIndexedLine(g);
            var index = ll.Project(g2.Coordinate);
            var p = ll.ExtractPoint(index);
            return g.Factory.CreatePoint(p);
        }
        public static double ProjectIndex(IGeometry g, IGeometry g2)
        {
            var ll = new LengthIndexedLine(g);
            return ll.Project(g2.Coordinate);
        }
    }
}