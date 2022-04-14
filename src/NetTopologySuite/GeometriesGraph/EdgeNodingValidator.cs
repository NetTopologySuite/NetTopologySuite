using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of <see cref="Edge"/> is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// <remarks>
    /// Uses <see cref="FastNodingValidator"/> to perform the validation.
    /// </remarks>
    /// </summary>
    public class EdgeNodingValidator
    {

        /// <summary>
        /// Checks whether the supplied <see cref="Edge"/>s are correctly noded.
        /// </summary>
        /// <param name="edges">an enumeration of Edges.</param>
        /// <exception cref="TopologyException">If the SegmentStrings are not correctly noded</exception>
        public static void CheckValid(IEnumerable<Edge> edges)
        {
            var validator = new EdgeNodingValidator(edges);
            validator.CheckValid();
        }

        /// <summary>
        /// Builds an enumeration of segment strings from the provided <paramref name="edges"/>.
        /// </summary>
        /// <param name="edges">An enumeration of <c>Edge</c>s</param>
        /// <returns>An enumeration of <c>ISegmentString</c>s</returns>
        public static IEnumerable<ISegmentString> ToSegmentStrings(IEnumerable<Edge> edges)
        {
            // convert Edges to SegmentStrings
            var segStrings = new List<ISegmentString>();
            foreach (var e in edges)
                segStrings.Add(new BasicSegmentString(e.Coordinates, e));
            return segStrings;
        }

        private readonly FastNodingValidator _nv;

       /// <summary>
       /// Creates a new validator for the given collection of <see cref="Edge"/>s.
       /// </summary>
       public EdgeNodingValidator(IEnumerable<Edge> edges)
        {
            _nv = new FastNodingValidator(ToSegmentStrings(edges));
        }

        /// <summary>
        /// Checks whether the supplied edges are correctly noded.
        /// </summary>
       /// <exception cref="TopologyException">If the SegmentStrings are not correctly noded</exception>
        public void CheckValid()
        {
            _nv.CheckValid();
        }
    }
}
