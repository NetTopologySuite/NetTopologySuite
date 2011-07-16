#region License

/*
 *  The attached / following is part of NetTopologySuite.
 *  
 *  NetTopologySuite is free software ? 2009 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: Felix Obermaier 2009
 *  
 *  This work is a port of JTS' KdNode by David Skea and Martin Davis
 *
 */

#endregion
using System;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Index.KdTree
{
    ///<summary>
    /// A node of a <see cref="KdTree{TCoordinate}"/>, which represents one or more points in the same location.
    ///</summary>
    public class KdNode<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {

        private TCoordinate _p;
        private Object _data;
        private KdNode<TCoordinate> _left;
        private KdNode<TCoordinate> _right;
        private int _count;

        /**
         * Creates a new KdNode.
         * 
         * @param _x coordinate of point
         * @param _y coordinate of point
         * @param data a data objects to associate with this node
         */
        ///<summary>
        /// Creates a new KdNode
        /// </summary>
        /// <param name="factory"><see cref="ICoordinateFactory{TCoordinate}"/> to create Coordinate</param>
        /// <param name="x">x coordinate of point</param>
        /// <param name="y">y coordinate of point</param>
        /// <param name="data">a data object to associate with this node</param>
        public KdNode(ICoordinateFactory<TCoordinate> factory, Double x, Double y, Object data)
        {
            _p = factory.Create(x, y);
            _count = 1;
            _data = data;
        }

        ///<summary>
        /// Creates a new KdNode
        /// </summary>
        /// <param name="p">point location of new node</param>
        /// <param name="data">a data object to associate with this node</param>
        public KdNode(TCoordinate p, Object data)
        {
            _p = p.Clone();
            _count = 1;
            _data = data;
        }

        ///<summary>
        /// Returns the X coordinate of the node
        ///</summary>
        public DoubleComponent X
        {
            get { return _p[Ordinates.X]; }
        }

        ///<summary>
        /// Returns the Y coordinate of the node
        ///</summary>
        public DoubleComponent Y
        {
            get { return _p[Ordinates.Y]; }
        }

        ///<summary>
        /// Returns the location of this node
        ///</summary>
        public TCoordinate Coordinate
        {
            get { return _p; }
        }

        ///<summary>
        /// Gets the user data object associated with this node.
        ///</summary>
        public Object Data
        {
            get { return _data; }
        }

        ///<summary>
        /// Gets/Sets the left node of the tree
        ///</summary>
        public KdNode<TCoordinate> Left
        {
            get { return _left; }
            set { _left = value; }
        }

        ///<summary>
        /// Gets/Sets the right node of the tree
        ///</summary>
        public KdNode<TCoordinate> Right
        {
            get { return _right; }
            set { _right = value; }
        }

        // Increments counts of points at this location
        internal void Increment()
        {
            _count++;
        }

        ///<summary>
        /// Returns the number of inserted points that are coincident at this location.
        ///</summary>
        public Int32 Count
        {
            get {return _count;}
        }

        ///<summary>
        /// Tests whether more than one point with this value have been inserted (up to the tolerance)
        ///</summary>
        public Boolean IsRepeated
        {
            get {return _count > 1;}
        }
    }
}
