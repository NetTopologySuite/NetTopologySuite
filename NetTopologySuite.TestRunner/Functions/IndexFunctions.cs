using GeoAPI.Geometries;
using NetTopologySuite.Index.KdTree;

namespace Open.Topology.TestRunner.Functions
{
    public class IndexFunctions
    {
        public static IGeometry KdTree(IGeometry pts, IGeometry query, double tolerance)
        {
            var index = Build(pts, tolerance);
            var result = index.Query(query.EnvelopeInternal);
            var resultCoords = KdTree<object>.ToCoordinates(result);
            return pts.Factory.CreateMultiPoint(resultCoords);
        }

        private static KdTree<object> Build(IGeometry geom, double tolerance)
        {
            var index = new KdTree<object>(tolerance);
            var pt = geom.Coordinates;
            for (int i = 0; i < pt.Length; i++)
            {
                index.Insert(pt[i]);
            }
            return index;
        }
    }
}