using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.KdTree
{
    /// <summary>
    /// A node of a <see cref="KdTree.KdTree{T}"/>, which represents one or more points in the same location.
    /// </summary>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <author>dskea</author>
    public class KdNode<T>
        where T : class
    {

        private readonly Coordinate _p;
        private readonly T _data;
        private int _count;

        /// <summary>
        /// Creates a new KdNode.
        /// </summary>
        /// <param name="x">coordinate of point</param>
        /// <param name="y">coordinate of point</param>
        /// <param name="data">A data objects to associate with this node</param>
        public KdNode(double x, double y, T data)
        {
            _p = new Coordinate(x, y);
            Left = null;
            Right = null;
            _count = 1;
            _data = data;
        }

        /// <summary>
        /// Creates a new KdNode.
        /// </summary>
        /// <param name="p">The point location of new node</param>
        /// <param name="data">A data objects to associate with this node</param>
        public KdNode(Coordinate p, T data)
        {
            _p = p.Copy();
            Left = null;
            Right = null;
            _count = 1;
            _data = data;
        }

        /// <summary>
        /// Gets x-ordinate of this node
        /// </summary>
        /// <returns>The <c>x</c>-ordinate</returns>
        public double X => _p.X;

        /// <summary>
        /// Gets y-ordinate of this node
        /// </summary>
        /// <returns>The <c>y</c>-ordinate</returns>
        public double Y => _p.Y;

        /// <summary>
        /// Gets the split value at a node, depending on
        /// whether the node splits on X or Y.
        /// The X (or Y) ordinates of all points in the left subtree
        /// are less than the split value, and those
        /// in the right subtree are greater than or equal to the split value.
        /// </summary>
        /// <param name="isSplitOnX">A flag whether the node splits a X or Y</param>
        /// <returns>The splitting value</returns>
        public double SplitValue(bool isSplitOnX)
        {
            if (isSplitOnX)
            {
                return _p.X;
            }
            return _p.Y;
        }

        /// <summary>
        /// Gets the location of this node
        /// </summary>
        /// <returns>The <c>Coordinate</c></returns>
        public Coordinate Coordinate => _p;

        /// <summary>
        /// Gets the user data object associated with this node.
        /// </summary>
        /// <returns>The user data</returns>
        public T Data => _data;

        /// <summary>
        /// Gets or sets the left node of the tree
        /// </summary>
        /// <returns>The left node</returns>
        public KdNode<T> Left { get; set; }

        /// <summary>
        /// Gets or sets the right node of the tree
        /// </summary>
        /// <returns>The right node</returns>
        public KdNode<T> Right { get; set; }

        // Increments counts of points at this location
        internal void Increment()
        {
            _count = _count + 1;
        }

        /// <summary>
        /// Gets the number of inserted points that are coincident at this location.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Gets whether more than one point with this value have been inserted (up to the tolerance)
        /// </summary>
        /// <returns></returns>
        public bool IsRepeated => _count > 1;

        /// <summary>
        /// Tests whether the node's left subtree may contain values
        /// in a given range envelope.
        /// </summary>
        /// <param name="isSplitOnX">A flag whether the node splits on  X or Y</param>
        /// <param name="env">The range envelope</param>
        /// <returns><c>true</c> if the left subtree is in range</returns>
        internal bool IsRangeOverLeft(bool isSplitOnX, Envelope env)
        {
            double envMin;
            if (isSplitOnX)
            {
                envMin = env.MinX;
            }
            else
            {
                envMin = env.MinY;
            }
            double splitValue = SplitValue(isSplitOnX);
            bool isInRange = envMin < splitValue;
            return isInRange;
        }

        /// <summary>
        /// Tests whether the node's right subtree may contain values
        /// in a given range envelope.
        /// </summary>
        /// <param name="isSplitOnX">A flag whether the node splits on  X or Y</param>
        /// <param name="env">The range envelope</param>
        /// <returns><c>true</c>if the right subtree is in range</returns>
        internal bool IsRangeOverRight(bool isSplitOnX, Envelope env)
        {
            double envMax;
            if (isSplitOnX)
            {
                envMax = env.MaxX;
            }
            else
            {
                envMax = env.MaxY;
            }
            double splitValue = SplitValue(isSplitOnX);
            bool isInRange = splitValue <= envMax;
            return isInRange;
        }

        /// <summary>
        /// Tests whether a point is strictly to the left
        /// of the splitting plane for this node.
        /// If so it may be in the left subtree of this node,
        /// Otherwise, the point may be in the right subtree.
        /// The point is to the left if its X (or Y) ordinate
        /// is less than the split value.
        /// </summary>
        /// <param name="isSplitOnX">A flag whether the node splits on  X or Y</param>
        /// <param name="pt">The query point</param>
        /// <returns><c>true</c> if the point is strictly to the left.</returns>
        /// <seealso cref="SplitValue(bool)"/>
        internal bool IsPointOnLeft(bool isSplitOnX, Coordinate pt)
        {
            double ptOrdinate;
            if (isSplitOnX)
            {
                ptOrdinate = pt.X;
            }
            else
            {
                ptOrdinate = pt.Y;
            }
            double splitValue = SplitValue(isSplitOnX);
            bool isInRange = (ptOrdinate < splitValue);
            return isInRange;
        }


    }
}
