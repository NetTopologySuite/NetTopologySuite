using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Utility methods for processing <see cref="ISegmentString"/>s
    ///</summary>
    /// <author>Martin Davis</author>
    public class SegmentStringUtil
    {
        ///<summary>
        /// Extracts all linear components from a given <see cref="IGeometry"/> to <see cref="ISegmentString"/>s.
        ///</summary>
        /// <param name="geom">the geometry to extract from</param>
        /// <returns>a List of SegmentStrings
        /// </returns>
        public static IList<ISegmentString> ExtractSegmentStrings(IGeometry geom)
        {
            IList<ISegmentString> segStr = new List<ISegmentString>();
            var lines = LinearComponentExtracter.GetLines(geom);
            foreach (ILineString line in lines)
            {
                Coordinate[] pts = line.Coordinates;
                segStr.Add(new NodedSegmentString(pts, geom));
            }
            return segStr;
        }

        public static string ToString(IEnumerable<ISegmentString> segStrings)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ISegmentString segStr in segStrings)
                sb.AppendFormat("{0}\n", segStr);
            return sb.ToString();
        }
    }
}