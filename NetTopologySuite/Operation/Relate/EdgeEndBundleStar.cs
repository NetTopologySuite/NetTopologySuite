using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// An ordered list of <c>EdgeEndBundle</c>s around a <c>RelateNode</c>.
    /// They are maintained in CCW order (starting with the positive x-axis) around the node
    /// for efficient lookup and topology building.
    /// </summary>
    public class EdgeEndBundleStar : EdgeEndStar
    {
        /// <summary>
        /// 
        /// </summary>
        public EdgeEndBundleStar() { }

        /// <summary>
        /// Insert a EdgeEnd in order in the list.
        /// If there is an existing EdgeStubBundle which is parallel, the EdgeEnd is
        /// added to the bundle.  Otherwise, a new EdgeEndBundle is created
        /// to contain the EdgeEnd.
        /// </summary>
        /// <param name="e"></param>
        public override void Insert(EdgeEnd e)
        {
            EdgeEndBundle eb = (EdgeEndBundle) edgeMap[e];
            if (eb == null) 
            {
                eb = new EdgeEndBundle(e);
                InsertEdgeEnd(e, eb);
            }
            else 
                eb.Insert(e);
            
        }

        /// <summary>
        /// Update the IM with the contribution for the EdgeStubs around the node.
        /// </summary>
        /// <param name="im"></param>
        public void UpdateIM(IntersectionMatrix im)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext(); ) 
            {
                EdgeEndBundle esb = (EdgeEndBundle) it.Current;
                esb.UpdateIM(im);
            }
        }
    }
}
