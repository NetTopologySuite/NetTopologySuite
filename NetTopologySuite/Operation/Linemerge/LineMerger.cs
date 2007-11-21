using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
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
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        // DESIGN_NODE: make into delegate
        private class AnonymousGeometryComponentFilterImpl : IGeometryComponentFilter<TCoordinate>
        {
            private readonly LineMerger<TCoordinate> _container = null;

            public AnonymousGeometryComponentFilterImpl(LineMerger<TCoordinate> container)
            {
                _container = container;
            }

            public void Filter(IGeometry<TCoordinate> component)
            {
                if (component is ILineString<TCoordinate>)
                {
                    _container.add((ILineString<TCoordinate>)component);
                }
            }
        }

        private LineMergeGraph _graph = new LineMergeGraph();
        private IList mergedLineStrings = null;
        private IList edgeStrings = null;
        private IGeometryFactory factory = null;

        /// <summary>
        /// Adds a collection of Geometries to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        public void Add(IList geometries)
        {
            IEnumerator i = geometries.GetEnumerator();

            while (i.MoveNext())
            {
                IGeometry geometry = (IGeometry) i.Current;
                Add(geometry);
            }
        }

        /// <summary>
        /// Returns the LineStrings built by the merging process.
        /// </summary>
        public IList MergedLineStrings
        {
            get
            {
                merge();
                return mergedLineStrings;
            }
        }

        /// <summary>
        /// Adds a Geometry to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        public void Add(IGeometry geometry)
        {
            geometry.Apply(new AnonymousGeometryComponentFilterImpl(this));
        }

        private void add(ILineString lineString)
        {
            if (factory == null)
            {
                factory = lineString.Factory;
            }

            _graph.AddEdge(lineString);
        }

        private void merge()
        {
            if (mergedLineStrings != null)
            {
                return;
            }

            edgeStrings = new ArrayList();
            buildEdgeStringsForObviousStartNodes();
            buildEdgeStringsForIsolatedLoops();
            mergedLineStrings = new ArrayList();

            for (IEnumerator i = edgeStrings.GetEnumerator(); i.MoveNext();)
            {
                EdgeString edgeString = (EdgeString) i.Current;
                mergedLineStrings.Add(edgeString.ToLineString());
            }
        }

        private void buildEdgeStringsForObviousStartNodes()
        {
            buildEdgeStringsForNonDegree2Nodes();
        }

        private void buildEdgeStringsForIsolatedLoops()
        {
            buildEdgeStringsForUnprocessedNodes();
        }

        private void buildEdgeStringsForUnprocessedNodes()
        {
            IEnumerator i = _graph.Nodes.GetEnumerator();

            while (i.MoveNext())
            {
                Node node = (Node) i.Current;

                if (!node.IsMarked)
                {
                    Assert.IsTrue(node.Degree == 2);
                    buildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        private void buildEdgeStringsForNonDegree2Nodes()
        {
            IEnumerator i = _graph.Nodes.GetEnumerator();

            while (i.MoveNext())
            {
                Node node = (Node) i.Current;

                if (node.Degree != 2)
                {
                    buildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        private void buildEdgeStringsStartingAt(Node node)
        {
            IEnumerator i = node.OutEdges.GetEnumerator();

            while (i.MoveNext())
            {
                LineMergeDirectedEdge directedEdge = (LineMergeDirectedEdge) i.Current;

                if (directedEdge.Edge.IsMarked)
                {
                    continue;
                }

                edgeStrings.Add(buildEdgeStringStartingWith(directedEdge));
            }
        }

        private EdgeString buildEdgeStringStartingWith(LineMergeDirectedEdge start)
        {
            EdgeString edgeString = new EdgeString(factory);
            LineMergeDirectedEdge current = start;

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