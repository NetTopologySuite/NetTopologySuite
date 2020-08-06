using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    public static class NodingTestUtility
    {
        public static MultiLineString ToLines(ICollection<ISegmentString> nodedList,
            GeometryFactory geomFact)
        {
            var lines = new LineString[nodedList.Count];
            int i = 0;
            foreach (NodedSegmentString nss in nodedList)
            {
                var pts = nss.Coordinates;
                var line = geomFact.CreateLineString(pts);
                lines[i++] = line;
            }
            return geomFact.CreateMultiLineString(lines);
        }

        public static IList<ISegmentString> ToSegmentStrings(IEnumerable<Geometry> lines)
        {
            var nssList = new List<ISegmentString>();
            foreach (LineString line in lines)
            {
                var nss = new NodedSegmentString(line.Coordinates, line);
                nssList.Add(nss);
            }
            return nssList;
        }

        /**
         * Runs a noder on one or two sets of input geometries
         * and validates that the result is fully noded.
         * 
         * @param geom1 a geometry
         * @param geom2 a geometry, which may be null
         * @param noder the noder to use
         * @return the fully noded linework
         * 
         * @throws TopologyException
         */
        public static Geometry NodeValidated(Geometry geom1, Geometry geom2, INoder noder)
        {
            var lines = new List<Geometry>(LineStringExtracter.GetLines(geom1));
            if (geom2 != null)
            {
                lines.AddRange(LineStringExtracter.GetLines(geom2));
            }
            var ssList = ToSegmentStrings(lines);

            var noderValid = new ValidatingNoder(noder);
            noderValid.ComputeNodes(ssList);
            var nodedList = noder.GetNodedSubstrings();

            var result = ToLines(nodedList, geom1.Factory);
            return result;
        }
    }
}
