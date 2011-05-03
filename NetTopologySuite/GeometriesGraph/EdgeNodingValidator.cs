using System.Collections;
using System.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Noding;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of SegmentStrings is correctly noded.
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

        public EdgeNodingValidator(IEnumerable edges)
        {
            _nv = new FastNodingValidator(ToSegmentStrings(edges));
        }

        public void CheckValid()
        {
            _nv.CheckValid();
        }
    }
}
