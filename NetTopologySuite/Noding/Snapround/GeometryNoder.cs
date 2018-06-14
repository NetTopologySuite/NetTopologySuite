using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Nodes the linework in a list of <see cref="IGeometry"/>s using Snap-Rounding
    /// to a given <see cref="IPrecisionModel"/>.
    /// <para>
    /// The input coordinates are expected to be rounded
    /// to the given precision model.
    /// This class does not perform that function.
    /// <c>GeometryPrecisionReducer"</c> may be used to do this.
    /// </para><para>
    /// This class does <b>not</b> dissolve the output linework,
    /// so there may be duplicate linestrings in the output.
    /// Subsequent processing (e.g. polygonization) may require
    /// the linework to be unique.  Using <c>UnaryUnion</c> is one way
    /// to do this (although this is an inefficient approach).
    /// </para></summary>
    public class GeometryNoder
    {
        private IGeometryFactory _geomFact;
        private readonly IPrecisionModel _pm;
        //private bool isValidityChecked = false;

        /// <summary>
        /// Creates a new noder which snap-rounds to a grid specified by the given <see cref="IPrecisionModel"/>
        /// </summary>
        /// <param name="pm">The precision model for the grid to snap-round to.</param>
        public GeometryNoder(IPrecisionModel pm)
        {
            _pm = pm;
        }

        /// <summary>
        /// Gets or sets whether noding validity is checked after noding is performed.
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
            foreach (var g in geoms)
            {
                _geomFact = g.Factory;
                break;
            }

            var segStrings = ToSegmentStrings(ExtractLines(geoms));
            //Noder sr = new SimpleSnapRounder(pm);
            var sr = new MCIndexSnapRounder(_pm);
            sr.ComputeNodes(segStrings);
            var nodedLines = sr.GetNodedSubstrings();

            //TODO: improve this to check for full snap-rounded correctness
            if (IsValidityChecked)
            {
                var nv = new NodingValidator(nodedLines);
                nv.CheckValid();
            }

            return ToLineStrings(nodedLines);
        }

        private IList<ILineString> ToLineStrings(IEnumerable<ISegmentString> segStrings)
        {
            var lines = new List<ILineString>();
            foreach (var ss in segStrings)
            {
                // skip collapsed lines
                if (ss.Count < 2)
                    continue;
                lines.Add(_geomFact.CreateLineString(ss.Coordinates));
            }
            return lines;
        }

        private static IEnumerable<IGeometry> ExtractLines(IEnumerable<IGeometry> geoms)
        {
            var lines = new List<IGeometry>();
            var lce = new LinearComponentExtracter(lines);
            foreach (var geom in geoms)
            {
                geom.Apply(lce);
            }
            return lines;
        }

        private static IList<ISegmentString> ToSegmentStrings(IEnumerable<IGeometry> lines)
        {
            var segStrings = new List<ISegmentString>();
            foreach (ILineString line in lines)
            {
                segStrings.Add(new NodedSegmentString(line.Coordinates, null));
            }
            return segStrings;
        }
    }
}