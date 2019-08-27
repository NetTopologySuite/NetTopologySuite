using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Utility
{
    public static class GeometryDataUtil
    {
        public static void SetComponentDataToIndex(Geometry geom)
        {
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var comp = geom.GetGeometryN(i);
                comp.UserData = "Component # " + i;
            }
        }
    }
}