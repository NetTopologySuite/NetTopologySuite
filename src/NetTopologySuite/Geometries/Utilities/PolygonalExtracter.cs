using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// .
    /// </summary>
    public static class PolygonalExtracter
    {
        /// <summary>
        /// Extracts the <see cref="Polygon"/> and <see cref="MultiPolygon"/> elements from a <see cref="Geometry"/>
        /// and adds them to the provided list.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="list">The list to add the extracted elements to</param>
        public static TCollection GetPolygonals<TCollection>(this Geometry geom, TCollection list)
            where TCollection : ICollection<Geometry>
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (geom is Polygon || geom is MultiPolygon) {
                list.Add(geom);
            }

            else if (geom is GeometryCollection) {
                for (int i = 0; i < geom.NumGeometries; i++)
                {
                    GetPolygonals(geom.GetGeometryN(i), list);
                }
            }
            // skip non-Polygonal elemental geometries 	
            return list;
        }

        /// <summary>
        /// Extracts the <see cref="Polygon"/> and <see cref="MultiPolygon"/> elements from a <see cref="Geometry"/>
        /// and returns them in a list.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        public static TCollection GetPolygonals<TCollection>(this Geometry geom)
            where TCollection : ICollection<Geometry>, new()
        {
            return GetPolygonals(geom, new TCollection());
        }

    }
}
