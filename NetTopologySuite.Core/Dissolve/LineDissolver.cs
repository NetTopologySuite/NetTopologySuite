﻿using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Dissolve
{
    /// <summary>
    /// Dissolves the linear components 
    /// from a collection of <see cref="IGeometry"/>s.
    /// into a set of maximal-length <see cref="ILineString"/>s
    /// in which every unique segment appears once only.
    /// The output linestrings run between node vertices
    /// of the input, which are vertices which have
    /// either degree 1, or degree 3 or greater.
    /// </summary>
    /// <remarks>
    /// Use cases for dissolving linear components
    /// include generalization 
    /// (in particular, simplifying polygonal coverages), 
    /// and visualization 
    /// (in particular, avoiding symbology conflicts when
    /// depicting shared polygon boundaries).
    /// </remarks>
    /// <remarks>
    /// This class does NOT node the input lines.
    /// If there are line segments crossing in the input, 
    /// they will still cross in the output.
    ///</remarks>
    public class LineDissolver
    {
        /// <summary>
        /// Dissolves the linear components in a geometry.
        /// </summary>
        /// <param name="g">the geometry to dissolve</param>
        /// <returns>the dissolved lines</returns>
        public static IGeometry Dissolve(IGeometry g)
        {
            LineDissolver d = new LineDissolver();
            d.Add(g);
            return d.GetResult();
        }

        private IGeometry _result;
        private IGeometryFactory _factory;
        private readonly DissolveEdgeGraph _graph;
        private readonly IList<IGeometry> _lines = new List<IGeometry>();

        public LineDissolver()
        {
            _graph = new DissolveEdgeGraph();
        }

        /// <summary>
        /// Adds a <see cref="IGeometry"/> to be dissolved. 
        /// Any number of geometries may be adde by calling this method multiple times.
        /// Any type of Geometry may be added.  The constituent linework will be
        /// extracted to be dissolved.
        /// </summary>
        /// <param name="geometry">geometry to be line-merged</param>
        public void Add(IGeometry geometry)
        {
            geometry.Apply(new GeometryComponentFilter(c =>
            {
                if (c is ILineString)
                    Add(c as ILineString);
            }));
        }

        /// <summary>
        /// Adds a collection of Geometries to be processed. May be called multiple times.
        /// Any dimension of Geometry may be added; the constituent linework will be
        /// extracted.
        /// </summary>
        /// <param name="geometries">the geometries to be line-merged</param>
        public void Add(IEnumerable<IGeometry> geometries)
        {
            foreach (IGeometry geometry in geometries)
                Add(geometry);
        }

        private void Add(ILineString lineString)
        {
            if (_factory == null)
                _factory = lineString.Factory;
            var doneStart = false;
            var seq = lineString.CoordinateSequence;
            for (int i = 1; i < seq.Count; i++)
            {
                var prev = seq.GetCoordinate(i - 1);
                var curr = seq.GetCoordinate(i);
                var e = (DissolveHalfEdge)_graph.AddEdge(prev, curr);
                //skip zero-length edges
                if (e==null) continue;
                // Record source initial segments, so that they can be reflected in output when needed
                // (i.e. during formation of isolated rings)
                if (!doneStart)
                {
                    e.SetStart();
                    doneStart = true;
                }
            }
        }

        /// <summary>
        /// Gets the dissolved result as a <see cref="IMultiLineString"/>.
        /// </summary>
        /// <returns>the dissolved lines</returns>
        public IGeometry GetResult()
        {
            if (_result == null)
                ComputeResult();
            return _result;
        }

        private void ComputeResult()
        {
            IEnumerable<HalfEdge> edges = _graph.GetVertexEdges();
            foreach (HalfEdge e in edges)
            {
                if (MarkHalfEdge.IsMarked(e))
                    continue;
                Process(e);
            }
            _result = _factory.BuildGeometry(_lines);
        }

        private readonly Stack<HalfEdge> _nodeEdgeStack = new Stack<HalfEdge>();

        private void Process(HalfEdge e)
        {
            HalfEdge eNode = e.PrevNode();
            // if edge is in a ring, just process this edge
            if (eNode == null)
                eNode = e;
            StackEdges(eNode);
            // extract lines from node edges in stack
            BuildLines();
        }

        /// <summary>
        /// For each edge in stack
        /// (which must originate at a node)
        /// extracts the line it initiates.
        /// </summary>
        private void BuildLines()
        {
            while (_nodeEdgeStack.Count > 0)
            {
                HalfEdge e = _nodeEdgeStack.Pop();
                if (MarkHalfEdge.IsMarked(e))
                    continue;
                BuildLine(e);
            }
        }

        private DissolveHalfEdge _ringStartEdge;

        /// <summary>
        /// Updates the tracked ringStartEdge
        /// if the given edge has a lower origin
        /// (using the standard <see cref="Coordinate"/> ordering).
        /// </summary>
        /// <remarks>
        /// Identifying the lowest starting node meets two goals:
        /// * It ensures that isolated input rings are created using the original node and orientation.
        /// * For isolated rings formed from multiple input linestrings, 
        /// it provides a canonical node and orientation for the output
        /// (rather than essentially random, and thus hard to test).
        /// </remarks>
        /// <param name="e"></param>
        private void UpdateRingStartEdge(DissolveHalfEdge e)
        {
            if (!e.IsStart)
            {
                e = (DissolveHalfEdge)e.Sym;
                if (!e.IsStart)
                    return;
            }
            // here e is known to be a start edge
            if (_ringStartEdge == null)
            {
                _ringStartEdge = e;
                return;
            }

            Coordinate eOrig = e.Orig;
            Coordinate rseOrig = _ringStartEdge.Orig;
            int compareTo = eOrig.CompareTo(rseOrig);
            if (compareTo < 0)
                _ringStartEdge = e;
        }

        /// <summary>
        /// Builds a line starting from the given edge.
        /// The start edge origin is a node (valence = 1 or >= 3), 
        /// unless it is part of a pure ring.
        /// </summary>
        /// <remarks>
        /// A pure ring has no other incident lines.
        /// In this case the start edge may occur anywhere on the ring.
        /// </remarks>
        /// <remarks>
        /// The line is built up to the next node encountered,
        /// or until the start edge is re-encountered
        /// (which happens if the edges form a ring).
        /// </remarks>
        /// <param name="eStart"></param>
        private void BuildLine(HalfEdge eStart)
        {
            CoordinateList line = new CoordinateList();
            DissolveHalfEdge e = (DissolveHalfEdge)eStart;
            _ringStartEdge = null;

            MarkHalfEdge.MarkBoth(e);
            Coordinate orig = e.Orig;
            line.Add(orig.Clone(), false);
            // scan along the path until a node is found (if one exists)
            while (e.Sym.Degree() == 2)
            {
                UpdateRingStartEdge(e);
                DissolveHalfEdge eNext = (DissolveHalfEdge)e.Next;
                // check if edges form a ring - if so, we're done
                if (eNext == eStart)
                {
                    BuildRing(_ringStartEdge);
                    return;
                }
                // add point to line, and move to next edge
                orig = eNext.Orig;
                line.Add(orig.Clone(), false);
                e = eNext;
                MarkHalfEdge.MarkBoth(e);
            }
            // add final node
            Coordinate dest = e.Dest;
            line.Add(dest.Clone(), false);

            // queue up the final node edges
            StackEdges(e.Sym);
            // store the scanned line
            AddLine(line);
        }

        private void BuildRing(HalfEdge eStartRing)
        {
            CoordinateList line = new CoordinateList();
            HalfEdge e = eStartRing;

            Coordinate orig = e.Orig;
            line.Add(orig.Clone(), false);
            // scan along the path until a node is found (if one exists)
            while (e.Sym.Degree() == 2)
            {
                HalfEdge eNext = e.Next;
                // check if edges form a ring - if so, we're done
                if (eNext == eStartRing)
                    break;

                // add point to line, and move to next edge
                orig = eNext.Orig;
                line.Add(orig.Clone(), false);
                e = eNext;
            }
            // add final node
            Coordinate dest = e.Dest;
            line.Add(dest.Clone(), false);

            // store the scanned line
            AddLine(line);
        }

        private void AddLine(CoordinateList line)
        {
            Coordinate[] array = line.ToCoordinateArray();
            ILineString ls = _factory.CreateLineString(array);
            _lines.Add(ls);
        }

        /// <summary>
        /// Adds edges around this node to the stack.
        /// </summary>
        /// <param name="node"></param>
        private void StackEdges(HalfEdge node)
        {
            HalfEdge e = node;
            do
            {
                if (!MarkHalfEdge.IsMarked(e))
                    _nodeEdgeStack.Push(e);
                e = e.ONext;
            } while (e != node);
        }
    }
}
