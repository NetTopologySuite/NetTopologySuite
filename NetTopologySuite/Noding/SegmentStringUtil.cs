using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Utility methods for processing <see cref="ISegmentString"/>s
    ///</summary>
    /// <author>Martin Davis</author>
    public class SegmentStringUtil
    {
        ///<summary>
        /// Extracts all linear components from a given <see cref="IGeometry"/>
        /// to <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/> data item is set to be the source <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="geom">The <see cref="IGeometry"/> to extract from.</param>
        /// <returns>a list of <see cref="ISegmentString"/>s.</returns>
        public static IList<ISegmentString> ExtractSegmentStrings(IGeometry geom)
        {
            return ExtractNodedSegmentStrings(geom);
        }

        ///<summary>
        /// Extracts all linear components from a given <see cref="IGeometry"/>
        /// to <see cref="ISegmentString"/>s.
        /// The <see cref="ISegmentString"/> data item is set to be the source <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="geom">The <see cref="IGeometry"/> to extract from.</param>
        /// <returns>a list of <see cref="ISegmentString"/>s.</returns>
        public static IList<ISegmentString> ExtractNodedSegmentStrings(IGeometry geom)
        {
            var segStr = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (var line in lines)
            {
                var pts = line.Coordinates;
                segStr.Add(new NodedSegmentString(pts, geom));
            }
            return segStr;
        }

        /// <summary>
        /// Converts a collection of <see cref="ISegmentString"/>s into a <see cref="IGeometry"/>.
        /// The geometry will be either a <see cref="ILineString"/>
        /// or a <see cref="IMultiLineString"/> (possibly empty).
        /// </summary>
        /// <param name="segStrings">A collection of <see cref="ISegmentString"/>.</param>
        /// <param name="geomFact">A geometry factory</param>
        /// <returns>A <see cref="ILineString"/> or a <see cref="IMultiLineString"/>.</returns>
        public static IGeometry ToGeometry(IList<ISegmentString> segStrings, IGeometryFactory geomFact)
        {
            var lines = new ILineString[segStrings.Count];
            int index = 0;

            foreach (var ss in segStrings)
            {
                var line = geomFact.CreateLineString(ss.Coordinates);
                lines[index++] = line;
            }
            if (lines.Length == 1)
                return lines[0];
            return geomFact.CreateMultiLineString(lines);
        }

        public static string ToString(IEnumerable<ISegmentString> segStrings)
        {
            var sb = new StringBuilder();
            foreach (var segStr in segStrings)
                sb.AppendFormat("{0}\n", segStr);
            return sb.ToString();
        }
    }
}
