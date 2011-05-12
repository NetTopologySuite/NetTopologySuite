using System.Collections;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Noding;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of <see cref="Edge"/> is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class EdgeNodingValidator
    {        
        private static IEnumerable<ISegmentString> ToSegmentStrings(IEnumerable edges)
        {
            // convert Edges to SegmentStrings
            IList<ISegmentString> segStrings = new List<ISegmentString>();
            foreach (Edge e in edges)
                segStrings.Add(new BasicSegmentString(e.Coordinates, e));
            return segStrings;
        }

        private readonly FastNodingValidator _nv;

       ///<summary>
       /// Creates a new validator for the given collection of <see cref="Edge"/>s.
       /// </summary> 
       public EdgeNodingValidator(IEnumerable edges)
        {
            _nv = new FastNodingValidator(ToSegmentStrings(edges));
        }

        /// <summary>
        /// Checks whether the supplied edges
        /// are correctly noded.  Throws an exception if they are not.
        /// </summary>
        public void CheckValid()
        {
            _nv.CheckValid();
        }
    }
}
