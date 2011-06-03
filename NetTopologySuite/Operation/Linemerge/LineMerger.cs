using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Planargraph;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// Merges a set of fully noded LineStrings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Merging stops at nodes of degree 1
    /// or 3 or more -- the exception is an isolated loop, which only has degree-2 nodes,
    /// in which case a node is simply chosen as a starting point. The direction of each
    /// merged LineString will be that of the majority of the LineStrings from which it
    /// was derived.
    /// </para>
    /// <para>
    /// Any dimension of Geometry is handled -- the constituent linework is extracted to 
    /// form the edges. The edges must be correctly noded; that is, they must only meet
    /// at their endpoints.  The LineMerger will still run on incorrectly noded input
    /// but will not form polygons from incorrected noded edges.
    /// </para>
    /// </remarks>
    public class LineMerger<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /*
         * [codekaizen 2008-01-14]  removed during translation of visitor patterns
         *                          to enumeration / query patterns.
         */

        //private class AnonymousGeometryComponentFilterImpl : IGeometryComponentFilter<TCoordinate>
        //{
        //    private readonly LineMerger<TCoordinate> _container = null;

        //    public AnonymousGeometryComponentFilterImpl(LineMerger<TCoordinate> container)
        //    {
        //        _container = container;
        //    }

        //    public void Filter(IGeometry<TCoordinate> component)
        //    {
        //        if (component is ILineString<TCoordinate>)
        //        {
        //            _container.add((ILineString<TCoordinate>)component);
        //        }
        //    }
        //}

        private readonly LineMergeGraph<TCoordinate> _graph = new LineMergeGraph<TCoordinate>();
        private List<EdgeString<TCoordinate>> _edgeStrings = new List<EdgeString<TCoordinate>>();
        private IGeometryFactory<TCoordinate> _factory;
        private List<ILineString<TCoordinate>> _mergedLineStrings;// = new List<ILineString<TCoordinate>>();

        /// <summary>
        /// Gets the <see cref="ILineString{TCoordinate}"/>s built by the merging process.
        /// </summary>
        public IEnumerable<ILineString<TCoordinate>> MergedLineStrings
        {
            get
            {
                Merge();
                return _mergedLineStrings;
            }
        }

        /// <summary>
        /// Adds a Geometry to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        public void Add(IGeometry<TCoordinate> geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            foreach (ILineString<TCoordinate> line in MergedLineStrings)
                AddLine(line);

            /*
            if (geometry is IHasGeometryComponents<TCoordinate>)
            {
                IHasGeometryComponents<TCoordinate> container
                    = geometry as IHasGeometryComponents<TCoordinate>;

                foreach (ILineString<TCoordinate> s in container.Components)
                {
                    if (s != null)
                    {
                        AddLine(s);
                    }
                }
            }
            else if (geometry is ILineString<TCoordinate>)
            {
                AddLine(geometry as ILineString<TCoordinate>);
            }
             */
        }

        /// <summary>
        /// Adds a collection of Geometries to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        public void Add(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                Add(geometry);
            }
        }

        private void AddLine(ILineString<TCoordinate> lineString)
        {
            if (_factory == null)
            {
                _factory = lineString.Factory;
            }

            _graph.AddEdge(lineString);
        }

        private void Merge()
        {
            if (_mergedLineStrings != null)
            {
                return;
            }

            GraphComponent<TCoordinate>.SetVisited(Caster.Upcast<GraphComponent<TCoordinate>, Node<TCoordinate>>(_graph.Nodes), false);
            GraphComponent<TCoordinate>.SetMarked(Caster.Upcast<GraphComponent<TCoordinate>, Node<TCoordinate>>(_graph.Nodes), false);

            _edgeStrings = new List<EdgeString<TCoordinate>>();
            BuildEdgeStringsForObviousStartNodes();
            BuildEdgeStringsForIsolatedLoops();
            _mergedLineStrings = new List<ILineString<TCoordinate>>();

            foreach (EdgeString<TCoordinate> edgeString in _edgeStrings)
            {
                _mergedLineStrings.Add(edgeString.ToLineString());
            }
        }

        private void BuildEdgeStringsForObviousStartNodes()
        {
            BuildEdgeStringsForNonDegree2Nodes();
        }

        private void BuildEdgeStringsForIsolatedLoops()
        {
            BuildEdgeStringsForUnprocessedNodes();
        }

        private void BuildEdgeStringsForUnprocessedNodes()
        {
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                if (!node.IsMarked)
                {
                    Assert.IsTrue(node.Degree == 2);
                    BuildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        private void BuildEdgeStringsForNonDegree2Nodes()
        {
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                if (node.Degree != 2)
                {
                    BuildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        private void BuildEdgeStringsStartingAt(Node<TCoordinate> node)
        {
            foreach (LineMergeDirectedEdge<TCoordinate> directedEdge in node.OutEdges)
            {
                if (directedEdge.Edge.IsMarked)
                {
                    continue;
                }

                _edgeStrings.Add(BuildEdgeStringStartingWith(directedEdge));
            }
        }

        private EdgeString<TCoordinate> BuildEdgeStringStartingWith(LineMergeDirectedEdge<TCoordinate> start)
        {
            EdgeString<TCoordinate> edgeString = new EdgeString<TCoordinate>(_factory);
            LineMergeDirectedEdge<TCoordinate> current = start;

            do
            {
                edgeString.Add(current);
                current.Edge.Marked = true;
                current = current.Next;
            } while (current != null && current != start);

            return edgeString;
        }
    }
}