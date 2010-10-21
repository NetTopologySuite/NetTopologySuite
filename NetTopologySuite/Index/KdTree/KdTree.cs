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
 *  This work is a port of JTS' KdTree by David Skea
 *
 */

#endregion
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Index.KdTree
{
    ///<summary>
    /// An implementation of a 2-D KD-Tree. KD-trees provide fast range searching on point data.
    /// <para>
    /// This implementation supports detecting and snapping points which are closer than a given
    /// tolerance value. If the same point (up to tolerance) is inserted more than once a new node is
    /// not created but the count of the existing node is incremented.
    /// </para>
    /// 
    ///</summary>
    public class KdTree<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
            private KdNode<TCoordinate> _root;
            private KdNode<TCoordinate> _last;
            private long _numberOfNodes;
            private double _tolerance;

            ///<summary>
            /// Creates a new instance of a KdTree with a snapping tolerance of 0.0.
            /// (I.e. distinct points will <c>not</c> be snapped)
            ///</summary>
            public KdTree()
                :this(0.0)
            {
            }

            ///<summary>
            /// Creates a new instance of a KdTree, specifying a snapping distance tolerance.
            /// Points which lie closer than the tolerance to a point already 
            /// in the tree will be treated as identical to the existing point.
            ///</summary>
            ///<param name="tolerance">the tolerance distance for considering two points equal</param>
            public KdTree(Double tolerance)
            {
                _tolerance = tolerance;
            }

            ///<summary>
            /// Inserts a Range of <see cref="TCoordinate"/>s.
            ///</summary>
            ///<param name="pts">an enumeration of <see cref="TCoordinate"/></param>
            public void InsertRange(IEnumerable<TCoordinate> pts)
            {
                foreach (TCoordinate pt in pts)
                    Insert(pt, null);
            }

            ///<summary>
            /// Inserts a new point in the kd-tree, with no data.
            ///</summary>
            ///<param name="p">the point to insert</param>
            ///<returns>the <see cref="KdTree{TCoordinate}"/> containing the point</returns>
            public KdNode<TCoordinate> Insert(TCoordinate p)
            {
                return Insert(p, null);
            }

            ///<summary>
            /// Inserts a new point into the kd-tree.
            ///</summary>
            ///<param name="p">the point to insert</param>
            ///<param name="data">a data item for the point</param>
            ///<returns>returns a new KdNode if a new point is inserted, else an existing 
            /// node is returned with its counter incremented. This can be checked
            /// by testing returnedNode.getCount() > 1.
            /// </returns>
            public KdNode<TCoordinate> Insert(TCoordinate p, Object data)
            {
                if (_root == null)
                {
                    _root = new KdNode<TCoordinate>(p, data);
                    _numberOfNodes++;
                    return _root;
                }

                KdNode<TCoordinate> currentNode = _root;
                KdNode<TCoordinate> leafNode = _root;
                Boolean isOddLevel = true;
                Boolean isLessThan = true;

                DoubleComponent x, y;
                p.GetComponents( out x, out y );
                // traverse the tree first cutting the plane left-right the top-bottom
                while (currentNode != _last)
                {
                    if (isOddLevel)
                        isLessThan = x.LessThan(currentNode.X);
                    else
                        isLessThan = y.LessThan(currentNode.Y);

                    leafNode = currentNode;
                    if (isLessThan)
                        currentNode = currentNode.Left;
                    else
                        currentNode = currentNode.Right;

                    // test if point is already a node
                    if (currentNode != null)
                    {
                        Boolean isInTolerance = p.Distance(currentNode.Coordinate) <= _tolerance;

                        // if (isInTolerance && ! p.equals2D(currentNode.getCoordinate())) {
                        // System.out.println("KDTree: Snapped!");
                        // System.out.println(WKTWriter.toPoint(p));
                        // }

                        // check if point is already in tree (up to tolerance) and if so simply
                        // return
                        // existing node
                        if (isInTolerance)
                        {
                            currentNode.Increment();
                            return currentNode;
                        }
                    }
                    isOddLevel = !isOddLevel;
                }

                // no node found, add new leaf node to tree
                _numberOfNodes++;
                KdNode<TCoordinate> node = new KdNode<TCoordinate>(p, data)
                                               {Left = _last, Right = _last};
                if (isLessThan)
                    leafNode.Left = node;
                else
                    leafNode.Right = node;
                return node;
            }

            private IEnumerable<KdNode<TCoordinate>> QueryNode(KdNode<TCoordinate> currentNode, KdNode<TCoordinate> bottomNode,
                    IExtents<TCoordinate> queryEnv, Boolean odd)
            {
                if (currentNode == bottomNode)
                    yield break;

                DoubleComponent min;
                DoubleComponent max;
                DoubleComponent discriminant;
                if (odd)
                {
                    min = queryEnv.Min[Ordinates.X];
                    max = queryEnv.Max[Ordinates.X];
                    discriminant = currentNode.X;
                }
                else
                {
                    min = queryEnv.Min[Ordinates.Y];
                    max = queryEnv.Max[Ordinates.Y];
                    discriminant = currentNode.Y;
                }
                Boolean searchLeft = min.LessThan(discriminant);
                Boolean searchRight = discriminant.LessThanOrEqualTo(max);

                if (searchLeft)
                {
                    foreach (KdNode<TCoordinate> kdNode in QueryNode(currentNode.Left, bottomNode, queryEnv, !odd))
                        yield return kdNode;
                }
                if (queryEnv.Contains(currentNode.Coordinate))
                {
                    yield return currentNode;
                }
                if (searchRight)
                {
                    foreach (KdNode<TCoordinate> kdNode in QueryNode(currentNode.Right, bottomNode, queryEnv, !odd))
                        yield return kdNode;
                }

            }

            ///<summary>
            /// Performs a range search of the points in the index.
            ///</summary>
            ///<param name="queryEnv">the range rectangle to query</param>
            ///<returns>an enumeration of the KdNodes found</returns>
            public IEnumerable<KdNode<TCoordinate>> Query(IExtents<TCoordinate> queryEnv)
            {
                return QueryNode(_root, _last, queryEnv, true);
            }

            public Int64 Count
            {
                get { return _numberOfNodes; }
            }

            public Double Tolerance
            {
                get { return _tolerance; }
            }
        }
}
