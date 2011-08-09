using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Nodes a list of <see cref="IGeometry"/>s using Snap Rounding
    /// </summary>
    public class GeometryNoder
    {
        private IGeometryFactory _geomFact;
        private readonly IPrecisionModel _pm;
        //private bool isValidityChecked = false;

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="IPrecisionModel"/>
        /// </summary>
        /// <param name="pm"></param>
        public GeometryNoder(IPrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary>
        /// Gets or sets whether returned noded linestrings are checked for validity
        /// </summary>
        public bool IsValidityChecked { get; set; }

        /// <summary>
        /// Nodes the linework of a set of Geometrys using SnapRounding. 
        /// </summary>
        /// <param name="geoms">A collection of Geometrys of any type</param>
        /// <returns>A list of LineStrings representing the noded linework of the input</returns>
        public IList<ILineString> Node(ICollection<IGeometry> geoms)
        {
            // get geometry factory
            _geomFact = geoms.First().Factory;

            var segStrings = ToSegmentStrings(ExtractLines(geoms));
            //Noder sr = new SimpleSnapRounder(pm);
            INoder sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);
            var nodedLines = sr.GetNodedSubstrings();

            if (IsValidityChecked)
            {
                NodingValidator nv = new NodingValidator(nodedLines);
                nv.CheckValid();
            }

            return ToLineStrings(nodedLines);
        }

        private IList<ILineString> ToLineStrings(IEnumerable<ISegmentString> segStrings)
        {
            var lines = new List<ILineString>();
            foreach (ISegmentString ss in segStrings)
            {
                // skip collapsed lines
                if (ss.Count < 2)
                    continue;
                lines.Add(_geomFact.CreateLineString(ss.Coordinates));
            }
            return lines;
        }

        private IEnumerable<ILineString> ExtractLines(IEnumerable<IGeometry> geoms)
        {
            IList<ILineString> lines = new List<ILineString>();
            var lce = new LinearComponentExtracter(lines);
            foreach (IGeometry geom in geoms)
            {
                geom.Apply(lce);
            }
            return lines;
        }

        private static IList<ISegmentString> ToSegmentStrings(IEnumerable<ILineString> lines)
        {
            IList<ISegmentString> segStrings = new List<ISegmentString>();
            foreach (ILineString line in lines)
            {
                segStrings.Add(new NodedSegmentString(line.Coordinates, null));
            }
            return segStrings;
        }
    }
}