using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// Represents a node of a <c>Quadtree</c>.  Nodes contain
    /// items which have a spatial extent corresponding to the node's position
    /// in the quadtree.
    /// </summary>
    public class Node<TCoordinate, TItem> : NodeBase<TCoordinate, TItem>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static Node<TCoordinate, TItem> CreateNode(IExtents<TCoordinate> extents)
        {
            Key<TCoordinate> key = new Key<TCoordinate>(extents);
            Node<TCoordinate, TItem> node = new Node<TCoordinate, TItem>(key.Extents, key.Level);
            return node;
        }

        public static Node<TCoordinate, TItem> CreateExpanded(Node<TCoordinate, TItem> node, IExtents<TCoordinate> addEnv)
        {
            IExtents<TCoordinate> expandEnv = new Extents<TCoordinate>(addEnv);

            if (node != null)
            {
                expandEnv.ExpandToInclude(node.Extents);
            }

            Node<TCoordinate, TItem> largerNode = CreateNode(expandEnv);

            if (node != null)
            {
                largerNode.InsertNode(node);
            }

            return largerNode;
        }

        private readonly IExtents<TCoordinate> _extents;
        private readonly TCoordinate _center;
        private readonly Int32 _level;

        public Node(IExtents<TCoordinate> extents, Int32 level)
        {
            _extents = extents;
            _level = level;
            _center = _extents.Center;
        }

        public IExtents Extents
        {
            get { return _extents; }
        }

        protected override Boolean IsSearchMatch(IExtents<TCoordinate> query)
        {
            return _extents.Intersects(query);
        }

        /// <summary> 
        /// Returns the subquad containing the envelope.
        /// Creates the subquad if it does not already exist.
        /// </summary>
        public Node<TCoordinate, TItem> GetNode(IExtents<TCoordinate> query)
        {
            Int32 subnodeIndex = GetSubnodeIndex(query, _center);

            // if subquadIndex is -1 searchEnv is not contained in a subquad
            if (subnodeIndex != -1)
            {
                // create the quad if it does not exist
                Node<TCoordinate, TItem> node = getSubnode(subnodeIndex);
                // recursively search the found/created quad
                return node.GetNode(query);
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
        public NodeBase<TCoordinate, TItem> Find(IExtents<TCoordinate> query)
        {
            Int32 subnodeIndex = GetSubnodeIndex(query, _center);

            if (subnodeIndex == -1)
            {
                return this;
            }

            if (SubNodes[subnodeIndex] != null)
            {
                // query lies in subquad, so search it
                Node<TCoordinate, TItem> node = SubNodes[subnodeIndex];
                return node.Find(query);
            }

            // no existing subquad, so return this one anyway
            return this;
        }

        public void InsertNode(Node<TCoordinate, TItem> node)
        {
            Assert.IsTrue(_extents == null || _extents.Contains(node.Extents));
            Int32 index = GetSubnodeIndex(node._extents, _center);
            if (node._level == _level - 1)
            {
                SubNodes[index] = node;
            }
            else
            {
                // the quad is not a direct child, so make a new child quad to contain it
                // and recursively insert the quad
                Node<TCoordinate, TItem> childNode = createSubnode(index);
                childNode.InsertNode(node);
                SubNodes[index] = childNode;
            }
        }

        /// <summary>
        /// Get the subquad for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        private Node<TCoordinate, TItem> getSubnode(Int32 index)
        {
            if (SubNodes[index] == null)
            {
                SubNodes[index] = createSubnode(index);
            }

            return SubNodes[index];
        }

        private Node<TCoordinate, TItem> createSubnode(Int32 index)
        {
            // create a new subquad in the appropriate quadrant
            Double minx = 0.0;
            Double maxx = 0.0;
            Double miny = 0.0;
            Double maxy = 0.0;

            switch (index)
            {
                case 0:
                    minx = _extents.GetMin(Ordinates.X);
                    maxx = _center[Ordinates.X];
                    miny = _extents.GetMin(Ordinates.Y);
                    maxy = _center[Ordinates.Y];
                    break;

                case 1:
                    minx = _center[Ordinates.X];
                    maxx = _extents.GetMax(Ordinates.X);
                    miny = _extents.GetMin(Ordinates.Y);
                    maxy = _center[Ordinates.Y];
                    break;

                case 2:
                    minx = _extents.GetMin(Ordinates.X);
                    maxx = _center[Ordinates.X];
                    miny = _center[Ordinates.Y];
                    maxy = _extents.GetMax(Ordinates.Y);
                    break;

                case 3:
                    minx = _center[Ordinates.X];
                    maxx = _extents.GetMax(Ordinates.X);
                    miny = _center[Ordinates.Y];
                    maxy = _extents.GetMax(Ordinates.Y);
                    break;

                default:
                    break;
            }

            IExtents<TCoordinate> sqEnv = new Extents<TCoordinate>(minx, maxx, miny, maxy);
            Node<TCoordinate, TItem> node = new Node<TCoordinate, TItem>(sqEnv, _level - 1);
            return node;
        }
    }
}