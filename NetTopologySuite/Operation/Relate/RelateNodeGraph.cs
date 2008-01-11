using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Implements a simple graph of <see cref="Node{TCoordinate}"/>s and 
    /// <see cref="EdgeEnd{TCoordinates}"/>s which is all that is
    /// required to determine topological relationships between Geometries.
    /// Also supports building a topological graph of a single Geometry, to
    /// allow verification of valid topology.    
    /// It is not necessary to create a fully linked
    /// PlanarGraph to determine relationships, since it is sufficient
    /// to know how the Geometries interact locally around the nodes.
    /// In fact, this is not even feasible, since it is not possible to compute
    /// exact intersection points, and hence the topology around those nodes
    /// cannot be computed robustly.
    /// The only Nodes that are created are for improper intersections;
    /// that is, nodes which occur at existing vertices of the Geometries.
    /// Proper intersections (e.g. ones which occur between the interior of line segments)
    /// have their topology determined implicitly, without creating a Node object
    /// to represent them.
    /// </summary>
    public class RelateNodeGraph<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly NodeMap<TCoordinate> _nodes 
            = new NodeMap<TCoordinate>(new RelateNodeFactory<TCoordinate>());

        //public IEnumerator GetNodeEnumerator()
        //{
        //    return _nodes.GetEnumerator();
        //}

        public void Build(GeometryGraph<TCoordinate> geomGraph)
        {
            // compute nodes for intersections between previously noded edges
            ComputeIntersectionNodes(geomGraph, 0);
            /*
            * Copy the labeling for the nodes in the parent Geometry.  These override
            * any labels determined by intersections.
            */
            CopyNodesAndLabels(geomGraph, 0);

            /*
            * Build EdgeEnds for all intersections.
            */
            EdgeEndBuilder<TCoordinate> eeBuilder = new EdgeEndBuilder<TCoordinate>();
            IEnumerable<EdgeEnd<TCoordinate>> eeList = eeBuilder.ComputeEdgeEnds(geomGraph.Edges);
            InsertEdgeEnds(eeList);
        }

        /// <summary>
        /// Insert nodes for all intersections on the edges of a Geometry.
        /// Label the created nodes the same as the edge label if they do not already have a label.
        /// This allows nodes created by either self-intersections or
        /// mutual intersections to be labeled.
        /// Endpoint nodes will already be labeled from when they were inserted.
        /// Precondition: edge intersections have been computed.
        /// </summary>
        public void ComputeIntersectionNodes(GeometryGraph<TCoordinate> geomGraph, Int32 argIndex)
        {
            foreach (Edge<TCoordinate> e in geomGraph.Edges)
            {
                Debug.Assert(e.Label.HasValue);
                Locations eLoc = e.Label.Value[argIndex].On;

                foreach (EdgeIntersection<TCoordinate> ei in e.EdgeIntersectionList)
                {
                    RelateNode<TCoordinate> n = _nodes.AddNode(ei.Coordinate) as RelateNode<TCoordinate>;

                    Debug.Assert(n != null);

                    if (eLoc == Locations.Boundary)
                    {
                        n.SetLabelBoundary(argIndex);
                    }
                    else if (n.Label.Value.IsNull(argIndex))
                    {
                        n.SetLabel(argIndex, Locations.Interior);
                    }
                }
            }
        }

        /// <summary>
        /// Copy all nodes from an arg point into this graph.
        /// The node label in the arg point overrides any previously computed
        /// label for that argIndex.
        /// (E.g. a node may be an intersection node with
        /// a computed label of Boundary,
        /// but in the original arg Geometry it is actually
        /// in the interior due to the Boundary Determination Rule).
        /// </summary>
        public void CopyNodesAndLabels(GeometryGraph<TCoordinate> geomGraph, Int32 argIndex)
        {
            foreach (Node<TCoordinate> node in geomGraph.Nodes)
            {   
                Node<TCoordinate> newNode = _nodes.AddNode(node.Coordinate);
                newNode.SetLabel(argIndex, node.Label.Value[argIndex].On);
            }
        }

        public void InsertEdgeEnds(IEnumerable<EdgeEnd<TCoordinate>> ee)
        {
            foreach (EdgeEnd<TCoordinate> end in ee)
            {
                _nodes.Add(end);   
            }
        }

        public IEnumerable<RelateNode<TCoordinate>> Nodes
        {
            get
            {
                foreach (Node<TCoordinate> node in _nodes)
                {
                    yield return node as RelateNode<TCoordinate>;
                }
            }
        }
    }
}