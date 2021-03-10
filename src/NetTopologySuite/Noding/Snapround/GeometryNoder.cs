using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Nodes the linework in a list of <see cref="Geometry"/>s using Snap-Rounding
    /// to a given <see cref="PrecisionModel"/>.
    /// <para>
    /// The input coordinates do not need to be rounded to the
    /// precision model.
    /// All output coordinates are rounded to the precision model.
    /// </para><para>
    /// This class does <b>not</b> dissolve the output linework,
    /// so there may be duplicate linestrings in the output.
    /// Subsequent processing (e.g. polygonization) may require
    /// the linework to be unique.  Using <c>UnaryUnion</c> is one way
    /// to do this (although this is an inefficient approach).
    /// </para></summary>
    public class GeometryNoder
    {
        private GeometryFactory _geomFact;
        private readonly PrecisionModel _pm;
        //private bool isValidityChecked = false;

        /// <summary>
        /// Creates a new noder which snap-rounds to a grid specified by the given <see cref="PrecisionModel"/>
        /// </summary>
        /// <param name="pm">The precision model for the grid to snap-round to.</param>
        public GeometryNoder(PrecisionModel pm)
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
        public ReadOnlyCollection<LineString> Node(IEnumerable<Geometry> geoms)
        {
            // get geometry factory
            // DEVIATION: JTS uses an "ExtractLines" helper, but by inlining it,
            // we can make the parameter any ol' IEnumerable<Geometry> without
            // iterating over it multiple times.
            var lines = new List<Geometry>();
            var lce = new LinearComponentExtracter(lines);
            bool first = true;
            foreach (var g in geoms)
            {
                if (first)
                {
                    _geomFact = g.Factory;
                    first = false;
                }

                g.Apply(lce);
            }

            var segStrings = ToSegmentStrings(lines);
            var sr = new SnapRoundingNoder(_pm);
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

        private ReadOnlyCollection<LineString> ToLineStrings(IEnumerable<ISegmentString> segStrings)
        {
            var lines = new List<LineString>();
            foreach (var ss in segStrings)
            {
                // skip collapsed lines
                if (ss.Count < 2)
                    continue;
                lines.Add(_geomFact.CreateLineString(ss.Coordinates));
            }
            return lines.AsReadOnly();
        }

        private static List<ISegmentString> ToSegmentStrings(List<Geometry> lines)
        {
            var segStrings = new List<ISegmentString>(lines.Count);
            foreach (LineString line in lines)
            {
                segStrings.Add(new NodedSegmentString(line.Coordinates, null));
            }
            return segStrings;
        }
    }
}
