using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index.Quadtree
{
    //public class Node : Node<object>
    //{
    //    public Node(Envelope env, int level) : base(env, level)
    //    {
    //    }
    //}

    /// <summary>
    /// Represents a node of a <c>Quadtree</c>.  Nodes contain
    /// items which have a spatial extent corresponding to the node's position
    /// in the quadtree.
    /// </summary>
    public class Node<T> : NodeBase<T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static Node<T> CreateNode(Envelope env)
        {
            Key key = new Key(env);
            var node = new Node<T>(key.Envelope, key.Level);
            return node;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="addEnv"></param>
        /// <returns></returns>
        public static Node<T> CreateExpanded(Node<T> node, Envelope addEnv)
        {
            Envelope expandEnv = new Envelope(addEnv);
            if (node != null) 
                expandEnv.ExpandToInclude(node._env);

            var largerNode = CreateNode(expandEnv);
            if (node != null) 
                largerNode.InsertNode(node);
            return largerNode;
        }

        private readonly Envelope _env;
        private readonly Coordinate _centre;
        private readonly int _level;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <param name="level"></param>
        public Node(Envelope env, int level)
        {
            _env = env;
            _level = level;
            _centre = new Coordinate();
            _centre.X = (env.MinX + env.MaxX) / 2;
            _centre.Y = (env.MinY + env.MaxY) / 2;
        }

        /// <summary>
        /// 
        /// </summary>
        public Envelope Envelope
        {
            get
            {
                return _env;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchEnv"></param>
        /// <returns></returns>
        protected override bool IsSearchMatch(Envelope searchEnv)
        {
            return _env.Intersects(searchEnv);
        }

        /// <summary> 
        /// Returns the subquad containing the envelope.
        /// Creates the subquad if
        /// it does not already exist.
        /// </summary>
        /// <param name="searchEnv"></param>
        public Node<T> GetNode(Envelope searchEnv)
        {
            int subnodeIndex = GetSubnodeIndex(searchEnv, _centre);            
            // if subquadIndex is -1 searchEnv is not contained in a subquad
            if (subnodeIndex != -1) 
            {
                // create the quad if it does not exist
                var node = GetSubnode(subnodeIndex);
                // recursively search the found/created quad
                return node.GetNode(searchEnv);
            }
            return this;
        }

        /// <summary>
        /// Returns the smallest <i>existing</i>
        /// node containing the envelope.
        /// </summary>
        /// <param name="searchEnv"></param>
        public NodeBase<T> Find(Envelope searchEnv)
        {
            int subnodeIndex = GetSubnodeIndex(searchEnv, _centre);
            if (subnodeIndex == -1)
                return this;
            if (Subnode[subnodeIndex] != null) 
            {
                // query lies in subquad, so search it
                var node = Subnode[subnodeIndex];
                return node.Find(searchEnv);
            }
            // no existing subquad, so return this one anyway
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public void InsertNode(Node<T> node)
        {
            Assert.IsTrue(_env == null || _env.Contains(node.Envelope));        
            int index = GetSubnodeIndex(node._env, _centre);        
            if (node._level == _level - 1)             
                Subnode[index] = node;                    
            else 
            {
                // the quad is not a direct child, so make a new child quad to contain it
                // and recursively insert the quad
                var childNode = CreateSubnode(index);
                childNode.InsertNode(node);
                Subnode[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subquad for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        /// <param name="index"></param>
        private Node<T> GetSubnode(int index)
        {
            if (Subnode[index] == null) 
                Subnode[index] = CreateSubnode(index);            
            return Subnode[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Node<T> CreateSubnode(int index)
        {
            // create a new subquad in the appropriate quadrant
            double minx = 0.0;
            double maxx = 0.0;
            double miny = 0.0;
            double maxy = 0.0;

            switch (index) 
            {
                case 0:
                    minx = _env.MinX;
                    maxx = _centre.X;
                    miny = _env.MinY;
                    maxy = _centre.Y;
                    break;

                case 1:
                    minx = _centre.X;
                    maxx = _env.MaxX;
                    miny = _env.MinY;
                    maxy = _centre.Y;
                    break;

                case 2:
                    minx = _env.MinX;
                    maxx = _centre.X;
                    miny = _centre.Y;
                    maxy = _env.MaxY;
                    break;

                case 3:
                    minx = _centre.X;
                    maxx = _env.MaxX;
                    miny = _centre.Y;
                    maxy = _env.MaxY;
                    break;

	            default:
		            break;
            }
            Envelope sqEnv = new Envelope(minx, maxx, miny, maxy);
            var node = new Node<T>(sqEnv, _level - 1);
            return node;
        }
    }
}
