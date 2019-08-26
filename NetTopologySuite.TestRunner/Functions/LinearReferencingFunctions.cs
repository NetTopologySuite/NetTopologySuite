using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;

namespace Open.Topology.TestRunner.Functions
{
    public static class LinearReferencingFunctions
    {
        public static Geometry ExtractPoint(Geometry g, double index)
        {
            var ll = new LengthIndexedLine(g);
            var p = ll.ExtractPoint(index);
            return g.Factory.CreatePoint(p);
        }

        public static Geometry ExtractLine(Geometry g, double start, double end)
        {
            var ll = new LengthIndexedLine(g);
            return ll.ExtractLine(start, end);
        }

        public static Geometry Project(Geometry g, Geometry g2)
        {
            var ll = new LengthIndexedLine(g);
            double index = ll.Project(g2.Coordinate);
            var p = ll.ExtractPoint(index);
            return g.Factory.CreatePoint(p);
        }
        public static double ProjectIndex(Geometry g, Geometry g2)
        {
            var ll = new LengthIndexedLine(g);
            return ll.Project(g2.Coordinate);
        }
    }
}