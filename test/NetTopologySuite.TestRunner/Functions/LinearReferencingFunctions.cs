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
            if (g2.Dimension == Dimension.Curve)
            {
                var line = (LineString)g2.GetGeometryN(0);
                var pStart = line.GetCoordinateN(0);
                var pEnd = line.GetCoordinateN(line.NumPoints - 1);
                double indexStart = ll.Project(pStart);
                double indexEnd = ll.Project(pEnd);
                return ll.ExtractLine(indexStart, indexEnd);
            }
            else
            {
                double index = ll.Project(g2.Coordinate);
                var p = ll.ExtractPoint(index);
                return g.Factory.CreatePoint(p);
            }
        }
        public static double ProjectIndex(Geometry g, Geometry g2)
        {
            var ll = new LengthIndexedLine(g);
            return ll.Project(g2.Coordinate);
        }
    }
}
