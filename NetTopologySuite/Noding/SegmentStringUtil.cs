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
        public static string ToString(IEnumerable<ISegmentString> segStrings)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ISegmentString segStr in segStrings)
                sb.AppendFormat("{0}\n", segStr);
            return sb.ToString();
        }
        
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
            IList<ISegmentString> segStr = new List<ISegmentString>();
            IEnumerable<IGeometry> lines = LinearComponentExtracter.GetLines(geom);
            foreach (IGeometry line in lines)
            {
                Coordinate[] pts = line.Coordinates;
                segStr.Add(new NodedSegmentString(pts, geom));
            }
            return segStr;
        }
    }
}
