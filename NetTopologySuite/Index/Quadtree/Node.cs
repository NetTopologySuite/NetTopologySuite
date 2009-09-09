using System;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary>
    /// Represents a node of a <c>Quadtree</c>.  Nodes contain
    /// items which have a spatial extent corresponding to the node's position
    /// in the quadtree.
    /// </summary>
    public class Node<TCoordinate, TItem> : BaseQuadNode<TCoordinate, TItem>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
        where TItem : IBoundable<IExtents<TCoordinate>>
    {
        private readonly TCoordinate _center;
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly Int32 _level;

        public Node(IExtents<TCoordinate> extents, Int32 level)
            : base(extents)
        {
            _level = level;
            _boundsSet = true;
            _center = Bounds.Center;
            _geoFactory = extents.Factory;
        }

        public static Node<TCoordinate, TItem> CreateNode(IExtents<TCoordinate> extents)
        {
            if (extents == null)
            {
                throw new ArgumentNullException("extents");
            }

            QuadTreeNodeKey<TCoordinate> key = new QuadTreeNodeKey<TCoordinate>(extents);
            Node<TCoordinate, TItem> node = new Node<TCoordinate, TItem>(key.Bounds, key.Level);
            return node;
        }

        public static Node<TCoordinate, TItem> CreateExpanded(
            IGeometryFactory<TCoordinate> geoFactory,
            Node<TCoordinate, TItem> node,
            IExtents<TCoordinate> addEnv)
        {
            IExtents<TCoordinate> expandExtents = new Extents<TCoordinate>(geoFactory, addEnv);

            if (node != null)
            {
                expandExtents.ExpandToInclude(node.Bounds);
            }

            Node<TCoordinate, TItem> largerNode = CreateNode(expandExtents);

            if (node != null)
            {
                largerNode.InsertNode(node);
            }

            return largerNode;
        }

        protected override Boolean IsSearchMatch(IExtents<TCoordinate> query)
        {
            return Intersects(query);
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
        /// Returns the smallest <i>existing</i> node containing the envelope.
        /// </summary>
        public BaseQuadNode<TCoordinate, TItem> Find(IExtents<TCoordinate> query)
        {
            Int32 subnodeIndex = GetSubnodeIndex(query, _center);

            if (subnodeIndex == -1)
            {
                return this;
            }

            if (SubNodesInternal[subnodeIndex] != null)
            {
                // query lies in subquad, so search it
                Node<TCoordinate, TItem> node = SubNodesInternal[subnodeIndex] as Node<TCoordinate, TItem>;
                return node.Find(query);
            }

            // no existing subquad, so return this one anyway
            return this;
        }

        public void InsertNode(Node<TCoordinate, TItem> node)
        {
            Assert.IsTrue(Bounds == null || Bounds.Contains(node.Bounds));
            Int32 index = GetChildSubnodeIndex(node.Bounds, _center);

            if (node._level == _level - 1)
            {
                SubNodesInternal[index] = node;
            }
            else
            {
                // the quad is not a direct child, so make a new child quad to contain it
                // and recursively insert the quad
                Node<TCoordinate, TItem> childNode = createSubnode(index);
                childNode.InsertNode(node);
                SubNodesInternal[index] = childNode;
            }
        }

        public override Boolean Intersects(IExtents<TCoordinate> bounds)
        {
            return Bounds.Intersects(bounds);
        }

        protected override IExtents<TCoordinate> ComputeBounds()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the subquad for the index.
        /// If it doesn't exist, create it.
        /// </summary>
        private Node<TCoordinate, TItem> getSubnode(Int32 index)
        {
            if (SubNodesInternal[index] == null)
            {
                SubNodesInternal[index] = createSubnode(index);
            }

            return SubNodesInternal[index] as Node<TCoordinate, TItem>;
        }

        private Node<TCoordinate, TItem> createSubnode(Int32 index)
        {
            // create a new subquad in the appropriate quadrant
            Double minx = 0.0;
            Double maxx = 0.0;
            Double miny = 0.0;
            Double maxy = 0.0;

            DoubleComponent cx, cy, bx, by;
            _center.GetComponents(out cx, out cy);

            switch (index)
            {
                case 0:
                    Bounds.Min.GetComponents(out bx, out by);
                    minx = (Double)bx;
                    maxx = (Double)cx;
                    miny = (Double)by;
                    maxy = (Double)cy;
                    //minx = Bounds.GetMin(Ordinates.X);
                    //maxx = _center[Ordinates.X];
                    //miny = Bounds.GetMin(Ordinates.Y);
                    //maxy = _center[Ordinates.Y];
                    break;

                case 1:
                    minx = (Double)cx;
                    //minx = _center[Ordinates.X];
                    maxx = Bounds.Max[Ordinates.X];
                    miny = Bounds.Min[Ordinates.Y];
                    maxy = (Double)cy;
                    //maxy = _center[Ordinates.Y];
                    break;

                case 2:
                    minx = Bounds.Min[Ordinates.X];
                    maxx = (Double)cx;
                    miny = (Double)cy;
                    //maxx = _center[Ordinates.X];
                    //miny = _center[Ordinates.Y];
                    maxy = Bounds.Max[Ordinates.Y];
                    break;

                case 3:
                    Bounds.Max.GetComponents(out bx, out by);
                    minx = (Double)cx;
                    maxx = (Double)bx;
                    miny = (Double)cy;
                    maxy = (Double)by;
                    //minx = _center[Ordinates.X];
                    //maxx = Bounds.GetMax(Ordinates.X);
                    //miny = _center[Ordinates.Y];
                    //maxy = Bounds.GetMax(Ordinates.Y);
                    break;

                default:
                    break;
            }

            IExtents<TCoordinate> sqEnv = new Extents<TCoordinate>(_geoFactory, minx, maxx, miny, maxy);
            Node<TCoordinate, TItem> node = new Node<TCoordinate, TItem>(sqEnv, _level - 1);
            return node;
        }
    }
}