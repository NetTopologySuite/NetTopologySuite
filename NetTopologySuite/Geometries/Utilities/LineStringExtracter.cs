using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Extracts all the <see cref="ILineString"/> elements from a <see cref="IGeometry"/>.</summary>
    ///<see cref="GeometryExtracter"/>
    public class LineStringExtracter : IGeometryFilter
    {
        ///<summary>
        /// Extracts the <see cref="ILineString"/> elements from a single <see cref="IGeometry"/> and adds them to the<see cref="List{ILineString}"/>.
        ///</summary>
        /// <param name="geom">The geometry from which to extract</param>
        /// <param name="lines">The list to add the extracted elements to</param>
        /// <returns>The <see cref="List{ILineString}"/></returns>
        public static IList<ILineString> GetLines(IGeometry geom, IList<ILineString> lines)
        {
            if (geom is ILineString)
            {
                lines.Add((ILineString)geom);
            }
            else if (geom is IGeometryCollection)
            {
                geom.Apply(new LineStringExtracter(lines));
            }
            // skip non-LineString elemental geometries

            return lines;
        }

        ///<summary>
        /// Extracts the <see cref="ILineString"/> elements from a single <see cref="IGeometry"/> and returns them in a <see cref="List{ILineString}"/>.
        ///</summary>
        /// <param name="geom"></param>
        /// <returns>The <see cref="List{ILineString}"/></returns>
        public static IList<ILineString> GetLines(IGeometry geom)
        {
            return GetLines(geom, new List<ILineString>());
        }

        private readonly IList<ILineString> _comps;

        ///<summary>
        /// Constructs a filter with a list in which to store the elements found.
        ///</summary>
        public LineStringExtracter(IList<ILineString> comps)
        {
            _comps = comps;
        }

        public void Filter(IGeometry geom)
        {
            if (geom is ILineString) _comps.Add((ILineString)geom);
        }
    }
}