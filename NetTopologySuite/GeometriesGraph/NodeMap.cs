using System.Collections;
using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A map of nodes, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap
    {        
        private IDictionary nodeMap = new SortedList();
        private NodeFactory nodeFact;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeFact"></param>
        public NodeMap(NodeFactory nodeFact)
        {
            this.nodeFact = nodeFact;
        }

        /// <summary> 
        /// This method expects that a node has a coordinate value.
        /// </summary>
        /// <param name="coord"></param>
        public Node AddNode(ICoordinate coord)
        {
            Node node = (Node) nodeMap[coord];
            if (node == null) 
            {
                node = nodeFact.CreateNode(coord);
                nodeMap.Add(coord, node);
            }
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node AddNode(Node n)
        {
            Node node = (Node) nodeMap[n.Coordinate];
            if (node == null) 
            {
                nodeMap.Add(n.Coordinate, n);
                return n;
            }
            node.MergeLabel(n);
            return node;
        }

        /// <summary> 
        /// Adds a node for the start point of this EdgeEnd
        /// (if one does not already exist in this map).
        /// Adds the EdgeEnd to the (possibly new) node.
        /// </summary>
        /// <param name="e"></param>
        public void Add(EdgeEnd e)
        {
            ICoordinate p = e.Coordinate;
            Node n = AddNode(p);
            n.Add(e);
        }

        /// <returns> 
        /// The node if found; null otherwise.
        /// </returns>
        /// <param name="coord"></param>
        public Node Find(ICoordinate coord)  
        {
            return (Node) nodeMap[coord];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        public IList Values
        {
            get
            {
                return new ArrayList(nodeMap.Values);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public IList GetBoundaryNodes(int geomIndex)
        {
            IList bdyNodes = new ArrayList();
            for (IEnumerator i = GetEnumerator(); i.MoveNext(); ) 
            {
                Node node = (Node) i.Current;
                if (node.Label.GetLocation(geomIndex) == Locations.Boundary)
                    bdyNodes.Add(node);
            }
            return bdyNodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            for (IEnumerator i = GetEnumerator(); i.MoveNext(); ) 
            {
                Node n = (Node)i.Current;
                n.Write(outstream);
            }
        }
    }
}
