using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Utility methods for processing <see cref="ISegmentString"/>s
    /// </summary>
    /// <author>Martin Davis</author>
    public class SegmentStringUtil
    {
        /// <summary>
        /// Extracts all linear components from a given <see cref="Geometry"/>
        /// to <see cref="ISegmentString"/>s.<br/>
        /// The <see cref="ISegmentString"/>'s data item is set to be the source <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry"/> to extract from.</param>
        /// <returns>a list of <see cref="ISegmentString"/>s.</returns>
        public static IList<ISegmentString> ExtractSegmentStrings(Geometry geom)
        {
            return ExtractNodedSegmentStrings(geom);
        }

        /// <summary>
        /// Extracts all linear components from a given <see cref="Geometry"/>
        /// to <see cref="NodedSegmentString"/>s.<br/>
        /// The <see cref="NodedSegmentString"/>'s data item is set to be the source <see cref="Geometry"/>.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry"/> to extract from.</param>
        /// <returns>a list of <see cref="ISegmentString"/>s.</returns>
        public static IList<ISegmentString> ExtractNodedSegmentStrings(Geometry geom)
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
        /// Extracts all linear components from a given <see cref="Geometry"/> to <see cref="BasicSegmentString"/>s.
        /// The <see cref="BasicSegmentString"/>'s data item is set to be the source Geometry.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry"/> to extract from.</param>
        /// <returns>a list of <see cref="ISegmentString"/>s.</returns>
        public static IList<ISegmentString> ExtractBasicSegmentStrings(Geometry geom)
        {
            var segStr = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (var line in lines)
            {
                var pts = line.Coordinates;
                segStr.Add(new BasicSegmentString(pts, geom));
            }
            return segStr;
        }

        /// <summary>
        /// Converts a collection of <see cref="ISegmentString"/>s into a <see cref="Geometry"/>.
        /// The geometry will be either a <see cref="LineString"/>
        /// or a <see cref="MultiLineString"/> (possibly empty).
        /// </summary>
        /// <param name="segStrings">A collection of <see cref="ISegmentString"/>.</param>
        /// <param name="geomFact">A geometry factory</param>
        /// <returns>A <see cref="LineString"/> or a <see cref="MultiLineString"/>.</returns>
        public static Geometry ToGeometry(IList<ISegmentString> segStrings, GeometryFactory geomFact)
        {
            var lines = new LineString[segStrings.Count];
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
