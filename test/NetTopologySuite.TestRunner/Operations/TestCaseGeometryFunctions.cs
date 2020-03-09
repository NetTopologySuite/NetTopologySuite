using System.Collections.ObjectModel;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Precision;

namespace Open.Topology.TestRunner.Operations
{
    public class TestCaseGeometryFunctions
    {
        public static Geometry bufferMitredJoin(Geometry g, double distance)
        {
            var bufParams = new BufferParameters();
            bufParams.JoinStyle = JoinStyle.Mitre;

            return BufferOp.Buffer(g, distance, bufParams);
        }

        public static Geometry densify(Geometry g, double distance)
        {
            return Densifier.Densify(g, distance);
        }

        public static double minClearance(Geometry g)
        {
            return MinimumClearance.GetDistance(g);
        }

        public static Geometry minClearanceLine(Geometry g)
        {
            return MinimumClearance.GetLine(g);
        }

        private static Geometry polygonize(Geometry g, bool extractOnlyPolygonal)
        {
            var lines = LinearComponentExtracter.GetLines(g);
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(lines);
            return polygonizer.GetGeometry();
        }

        public static Geometry polygonize(Geometry g)
        {
            return polygonize(g, false);
        }

        public static Geometry polygonizeValidArea(Geometry g)
        {
            return polygonize(g, true);
        }
    }
}
