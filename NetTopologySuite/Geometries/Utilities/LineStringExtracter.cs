using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Extracts all the <see cref="ILineString"/> elements from a <see cref="IGeometry"/>.
    /// </summary>
    ///<see cref="GeometryExtracter"/>
    public class LineStringExtracter : IGeometryFilter
    {
        ///<summary>
        /// Extracts the <see cref="ILineString"/> elements from a single <see cref="IGeometry"/>
        /// and adds them to the<see cref="List{ILineString}"/>.
        ///</summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="lines">The list to add the extracted elements to</param>
        /// <returns>The <paramref name="lines"/> list argument</returns>
        public static ICollection<IGeometry> GetLines(IGeometry geom, ICollection<IGeometry> lines)
        {
            if (geom is ILineString)
            {
                lines.Add(geom);
            }
            else if (geom is IGeometryCollection)
            {
                geom.Apply(new LineStringExtracter(lines));
            }
            // skip non-LineString elemental geometries
            return lines;
        }

        ///<summary>
        /// Extracts the <see cref="ILineString"/> elements from a single <see cref="IGeometry"/>
        /// and returns them in a <see cref="ICollection{ILineString}"/>.
        ///</summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A list containing the linear elements</returns>
        public static ICollection<IGeometry> GetLines(IGeometry geom)
        {
            return GetLines(geom, new List<IGeometry>());
        }

        /// <summary>
        /// Extracts the <see cref="ILineString"/> elements from a single <see cref="IGeometry"/>
        /// and returns them as either a <see cref="ILineString"/> or <see cref="IMultiLineString"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A linear geometry</returns>
        public static IGeometry GetGeometry(IGeometry geom)
        {
            var list = GetLines(geom);
            return geom.Factory.BuildGeometry(list);
        }

        private readonly ICollection<IGeometry> _comps;

        ///<summary>
        /// Constructs a filter with a list in which to store the elements found.
        ///</summary>
        public LineStringExtracter(ICollection<IGeometry> comps)
        {
            _comps = comps;
        }

        public void Filter(IGeometry geom)
        {
            if (geom is ILineString) _comps.Add(geom);
        }
    }
}