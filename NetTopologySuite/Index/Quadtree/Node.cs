using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// Represents a node of a <c>Quadtree</c>.  Nodes contain
    /// items which have a spatial extent corresponding to the node's position
    /// in the quadtree.
    /// </summary>
    public class Node : NodeBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static Node CreateNode(IEnvelope env)
        {
            Key key = new Key(env);
            Node node = new Node(key.Envelope, key.Level);
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="addEnv"></param>
        /// <returns></returns>
        public static Node CreateExpanded(Node node, IEnvelope addEnv)
        {
            IEnvelope expandEnv = new Envelope(addEnv);
            if (node != null) 
                expandEnv.ExpandToInclude(node.env);

            Node largerNode = CreateNode(expandEnv);
            if (node != null) 
                largerNode.InsertNode(node);
            return largerNode;
        }

        private IEnvelope env;
        private ICoordinate centre;
        private int level;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="level"></param>
        public Node(IEnvelope env, int level)
        {
            this.env = env;
            this.level = level;
            centre = new Coordinate();
            centre.X = (env.MinX + env.MaxX) / 2;
            centre.Y = (env.MinY + env.MaxY) / 2;
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                return env;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(IEnvelope searchEnv)
        {
            return env.Intersects(searchEnv);
        }

        /// <summary> 
        /// Returns the subquad containing the envelope.
        /// Creates the subquad if
        /// it does not already exist.
        /// </summary>
        /// <param name="searchEnv"></param>
        public Node GetNode(IEnvelope searchEnv)
        {
            int subnodeIndex = GetSubnodeIndex(searchEnv, centre);            
            // if subquadIndex is -1 searchEnv is not contained in a subquad
            if (subnodeIndex != -1) 
            {
                // create the quad if it does not exist
                Node node = GetSubnode(subnodeIndex);
                // recursively search the found/created quad
                return node.GetNode(searchEnv);
            }
            else return this;            
        }

        /// <summary>
        /// Returns the smallest <i>existing</i>
        /// node containing the envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        public NodeBase Find(IEnvelope searchEnv)
        {
            int subnodeIndex = GetSubnodeIndex(searchEnv, centre);
            if (subnodeIndex == -1)
                return this;
            if (subnode[subnodeIndex] != null) 
            {
                // query lies in subquad, so search it
                Node node = subnode[subnodeIndex];
                return node.Find(searchEnv);
            }
            // no existing subquad, so return this one anyway
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void InsertNode(Node node)
        {
            Assert.IsTrue(env == null || env.Contains(node.Envelope));        
            int index = GetSubnodeIndex(node.env, centre);        
            if (node.level == level - 1)             
                subnode[index] = node;                    
            else 
            {
                // the quad is not a direct child, so make a new child quad to contain it
                // and recursively insert the quad
                Node childNode = CreateSubnode(index);
                childNode.InsertNode(node);
                subnode[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subquad for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        /// <param name="index"></param>
        private Node GetSubnode(int index)
        {
            if (subnode[index] == null) 
                subnode[index] = CreateSubnode(index);            
            return subnode[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Node CreateSubnode(int index)
        {
            // create a new subquad in the appropriate quadrant
            double minx = 0.0;
            double maxx = 0.0;
            double miny = 0.0;
            double maxy = 0.0;

            switch (index) 
            {
                case 0:
                    minx = env.MinX;
                    maxx = centre.X;
                    miny = env.MinY;
                    maxy = centre.Y;
                    break;

                case 1:
                    minx = centre.X;
                    maxx = env.MaxX;
                    miny = env.MinY;
                    maxy = centre.Y;
                    break;

                case 2:
                    minx = env.MinX;
                    maxx = centre.X;
                    miny = centre.Y;
                    maxy = env.MaxY;
                    break;

                case 3:
                    minx = centre.X;
                    maxx = env.MaxX;
                    miny = centre.Y;
                    maxy = env.MaxY;
                    break;

	            default:
		            break;
            }
            IEnvelope sqEnv = new Envelope(minx, maxx, miny, maxy);
            Node node = new Node(sqEnv, level - 1);
            return node;
        }
    }
}
