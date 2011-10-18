using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Open.Topology.TestRunner.Functions
{
    public class SelectionFunctions
    {
        public static IGeometry intersects(IGeometry a, IGeometry mask)
        {
            var selected = new List<IGeometry>();
            for (int i = 0; i < a.NumGeometries; i++)
            {
                var g = a.GetGeometryN(i);
                if (mask.Intersects(g))
                {
                    selected.Add(g);
                }
            }
            return a.Factory.BuildGeometry(selected);
        }
    }

    /**
     * Geometry functions which
     * augment the existing methods on {@link Geometry},
     * for use in XML Test files.
     * These should be named differently to the Geometry methods.
     * 
     * @author Martin Davis
     *
     */
}