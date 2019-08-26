using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// Sews together a set of fully noded LineStrings.
    /// </summary>
    /// <remarks>
    /// <para> Sewing stops at nodes of degree 1
    /// or 3 or more -- the exception is an isolated loop, which only has degree-2 nodes,
    /// in which case a node is simply chosen as a starting point. The direction of each
    /// merged LineString will be that of the majority of the LineStrings from which it
    /// was derived.</para>
    /// <para>
    /// Any dimension of Geometry is handled -- the constituent linework is extracted to
    /// form the edges. The edges must be correctly noded; that is, they must only meet
    /// at their endpoints.  The LineMerger will still run on incorrectly noded input
    /// but will not form polygons from incorrected noded edges.</para>
    /// <para>
    /// <b>NOTE:</b>once merging has been performed, no more</para>
    /// </remarks>
    public class LineMerger
    {
        /// <summary>
        ///
        /// </summary>
        private class AnonymousGeometryComponentFilterImpl : IGeometryComponentFilter
        {
            private readonly LineMerger _container;

            /// <summary>
            ///
            /// </summary>
            /// <param name="container"></param>
            public AnonymousGeometryComponentFilterImpl(LineMerger container)
            {
                _container = container;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="component"></param>
            public void Filter(Geometry component)
            {
                if (component is LineString)
                    _container.Add((LineString) component);
            }
        }

        private readonly LineMergeGraph _graph = new LineMergeGraph();
        private List<Geometry> _mergedLineStrings;
        private List<EdgeString> _edgeStrings;
        private GeometryFactory _factory;

        /// <summary>
        /// Adds a Geometry to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        /// <param name="geometry"></param>
        public void Add(Geometry geometry)
        {
            geometry.Apply(new AnonymousGeometryComponentFilterImpl(this));
        }

        /// <summary>
        /// Adds a collection of Geometries to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        /// <param name="geometries"></param>
        public void Add(IEnumerable<Geometry> geometries)
        {
            _mergedLineStrings = null;
            foreach (var geometry in geometries)
                Add(geometry);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lineString"></param>
        private void Add(LineString lineString)
        {
            if (_factory == null)
                _factory = lineString.Factory;
            _graph.AddEdge(lineString);
        }

        /// <summary>
        ///
        /// </summary>
        private void Merge()
        {
            if (_mergedLineStrings != null)
                return;

            // reset marks (this allows incremental processing)
            GraphComponent.SetMarked(_graph.GetNodeEnumerator(), false);
            GraphComponent.SetMarked(_graph.GetEdgeEnumerator(), false);

            _edgeStrings = new List<EdgeString>();
            BuildEdgeStringsForObviousStartNodes();
            BuildEdgeStringsForIsolatedLoops();
            _mergedLineStrings = new List<Geometry>();
            foreach (var edgeString in _edgeStrings)
                _mergedLineStrings.Add(edgeString.ToLineString());
        }

        /// <summary>
        ///
        /// </summary>
        private void BuildEdgeStringsForObviousStartNodes()
        {
            BuildEdgeStringsForNonDegree2Nodes();
        }

        /// <summary>
        ///
        /// </summary>
        private void BuildEdgeStringsForIsolatedLoops()
        {
            BuildEdgeStringsForUnprocessedNodes();
        }

        /// <summary>
        ///
        /// </summary>
        private void BuildEdgeStringsForUnprocessedNodes()
        {
            foreach (var node in _graph.Nodes)
            {
                if (!node.IsMarked)
                {
                    Assert.IsTrue(node.Degree == 2);
                    BuildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void BuildEdgeStringsForNonDegree2Nodes()
        {
            foreach (var node in _graph.Nodes)
            {
                if (node.Degree != 2)
                {
                    BuildEdgeStringsStartingAt(node);
                    node.Marked = true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        private void BuildEdgeStringsStartingAt(Node node)
        {
            foreach (LineMergeDirectedEdge directedEdge in node.OutEdges)
            {
                if (directedEdge.Edge.IsMarked)
                    continue;
                _edgeStrings.Add(BuildEdgeStringStartingWith(directedEdge));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        private EdgeString BuildEdgeStringStartingWith(LineMergeDirectedEdge start)
        {
            var edgeString = new EdgeString(_factory);
            var current = start;
            do
            {
                edgeString.Add(current);
                current.Edge.Marked = true;
                current = current.Next;
            }
            while (current != null && current != start);
            return edgeString;
        }

        /// <summary>
        /// Returns the LineStrings built by the merging process.
        /// </summary>
        /// <returns></returns>
        public IList<Geometry> GetMergedLineStrings()
        {
            Merge();
            return _mergedLineStrings;
        }
    }
}
