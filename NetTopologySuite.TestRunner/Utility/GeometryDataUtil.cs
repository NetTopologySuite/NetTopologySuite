using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Utility
{
    public static class GeometryDataUtil
    {
        public static void SetComponentDataToIndex(IGeometry geom)
        {
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                IGeometry comp = geom.GetGeometryN(i);
                comp.UserData = "Component # " + i;
            }
        }
    }
}