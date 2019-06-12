using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Extracts all the <see cref="LineString"/> elements from a <see cref="Geometry"/>.
    /// </summary>
    /// <see cref="GeometryExtracter"/>
    public class LineStringExtracter : IGeometryFilter
    {
        /// <summary>
        /// Extracts the <see cref="LineString"/> elements from a single <see cref="Geometry"/>
        /// and adds them to the<see cref="ICollection{LineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="lines">The list to add the extracted elements to</param>
        /// <returns>The <paramref name="lines"/> list argument</returns>
        public static TCollection GetLines<TCollection>(Geometry geom, TCollection lines)
            where TCollection : ICollection<Geometry>
        {
            if (geom is LineString)
            {
                lines.Add(geom);
            }
            else if (geom is GeometryCollection)
            {
                geom.Apply(new LineStringExtracter(lines));
            }
            // skip non-LineString elemental geometries
            return lines;
        }

        /// <summary>
        /// Extracts the <see cref="LineString"/> elements from a single <see cref="Geometry"/>
        /// and returns them in a <see cref="List{LineString}"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A list containing the linear elements</returns>
        public static ReadOnlyCollection<Geometry> GetLines(Geometry geom)
        {
            var lines = GetLines(geom, new List<Geometry>());
            return lines.AsReadOnly();
        }

        /// <summary>
        /// Extracts the <see cref="LineString"/> elements from a single <see cref="Geometry"/>
        /// and returns them as either a <see cref="LineString"/> or <see cref="MultiLineString"/>.
        /// </summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <returns>A linear geometry</returns>
        public static Geometry GetGeometry(Geometry geom)
        {
            var list = GetLines(geom);
            return geom.Factory.BuildGeometry(list);
        }

        private readonly ICollection<Geometry> _comps;

        /// <summary>
        /// Constructs a filter with a list in which to store the elements found.
        /// </summary>
        public LineStringExtracter(ICollection<Geometry> comps)
        {
            _comps = comps;
        }

        public void Filter(Geometry geom)
        {
            if (geom is LineString) _comps.Add(geom);
        }
    }
}
