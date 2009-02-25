using System.Collections;
using GisSharpBlog.NetTopologySuite.Noding;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of SegmentStrings is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class EdgeNodingValidator
    {        
        private static IList ToSegmentStrings(IEnumerable edges)
        {
            // convert Edges to SegmentStrings
            IList segStrings = new ArrayList();
            for (var i = edges.GetEnumerator(); i.MoveNext(); )
            {
                var e = (Edge)i.Current;
                segStrings.Add(new SegmentString(e.Coordinates, e));
            }
            return segStrings;
        }

        private readonly NodingValidator nv;

        public EdgeNodingValidator(IEnumerable edges)
        {
            nv = new NodingValidator(ToSegmentStrings(edges));
        }

        public void checkValid()
        {
            nv.CheckValid();
        }
    }
}
