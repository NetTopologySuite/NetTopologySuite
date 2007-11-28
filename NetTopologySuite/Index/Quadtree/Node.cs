using System;
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
        public static Node CreateNode(IExtents env)
        {
            Key key = new Key(env);
            Node node = new Node(key.Envelope, key.Level);
            return node;
        }

        public static Node CreateExpanded(Node node, IExtents addEnv)
        {
            IExtents expandEnv = new Extents(addEnv);
            if (node != null)
            {
                expandEnv.ExpandToInclude(node.env);
            }

            Node largerNode = CreateNode(expandEnv);
            if (node != null)
            {
                largerNode.InsertNode(node);
            }
            return largerNode;
        }

        private IExtents env;
        private ICoordinate center;
        private Int32 level;

        public Node(IExtents env, Int32 level)
        {
            this.env = env;
            this.level = level;
            center = new Coordinate();
            center.X = (env.MinX + env.MaxX)/2;
            center.Y = (env.MinY + env.MaxY)/2;
        }

        public IExtents Envelope
        {
            get { return env; }
        }

        protected override Boolean IsSearchMatch(IExtents searchEnv)
        {
            return env.Intersects(searchEnv);
        }

        /// <summary> 
        /// Returns the subquad containing the envelope.
        /// Creates the subquad if
        /// it does not already exist.
        /// </summary>
        public Node GetNode(IExtents searchEnv)
        {
            Int32 subnodeIndex = GetSubnodeIndex(searchEnv, center);
            // if subquadIndex is -1 searchEnv is not contained in a subquad
            if (subnodeIndex != -1)
            {
                // create the quad if it does not exist
                Node node = GetSubnode(subnodeIndex);
                // recursively search the found/created quad
                return node.GetNode(searchEnv);
            }
            else
            {
                return this;
            }
        }

        /// <summary>
        /// Returns the smallest <i>existing</i>
        /// node containing the envelope.
        /// </summary>
        public NodeBase Find(IExtents searchEnv)
        {
            Int32 subnodeIndex = GetSubnodeIndex(searchEnv, center);
            if (subnodeIndex == -1)
            {
                return this;
            }
            if (subnode[subnodeIndex] != null)
            {
                // query lies in subquad, so search it
                Node node = subnode[subnodeIndex];
                return node.Find(searchEnv);
            }
            // no existing subquad, so return this one anyway
            return this;
        }

        public void InsertNode(Node node)
        {
            Assert.IsTrue(env == null || env.Contains(node.Envelope));
            Int32 index = GetSubnodeIndex(node.env, center);
            if (node.level == level - 1)
            {
                subnode[index] = node;
            }
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
        private Node GetSubnode(Int32 index)
        {
            if (subnode[index] == null)
            {
                subnode[index] = CreateSubnode(index);
            }
            return subnode[index];
        }

        private Node CreateSubnode(Int32 index)
        {
            // create a new subquad in the appropriate quadrant
            Double minx = 0.0;
            Double maxx = 0.0;
            Double miny = 0.0;
            Double maxy = 0.0;

            switch (index)
            {
                case 0:
                    minx = env.MinX;
                    maxx = center.X;
                    miny = env.MinY;
                    maxy = center.Y;
                    break;

                case 1:
                    minx = center.X;
                    maxx = env.MaxX;
                    miny = env.MinY;
                    maxy = center.Y;
                    break;

                case 2:
                    minx = env.MinX;
                    maxx = center.X;
                    miny = center.Y;
                    maxy = env.MaxY;
                    break;

                case 3:
                    minx = center.X;
                    maxx = env.MaxX;
                    miny = center.Y;
                    maxy = env.MaxY;
                    break;

                default:
                    break;
            }
            IExtents sqEnv = new Extents(minx, maxx, miny, maxy);
            Node node = new Node(sqEnv, level - 1);
            return node;
        }
    }
}