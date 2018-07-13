using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Polygonizes a set of <see cref="IGeometry"/>s which contain linework that
    /// represents the edges of a planar graph.
    /// </summary>
    /// <remarks>
    /// <para>All types of Geometry are accepted as input;
    /// the constituent linework is extracted as the edges to be polygonized.
    /// The processed edges must be correctly noded; that is, they must only meet
    /// at their endpoints. Polygonization will accept incorrectly noded input
    /// but will not form polygons from non-noded edges,
    /// and reports them as errors.
    /// </para><para>
    /// The Polygonizer reports the follow kinds of errors:
    /// Dangles - edges which have one or both ends which are not incident on another edge endpoint
    /// Cut Edges - edges which are connected at both ends but which do not form part of polygon
    /// Invalid Ring Lines - edges which form rings which are invalid
    /// (e.g. the component lines contain a self-intersection).</para>
    /// <para>
    /// Polygonization supports extracting only polygons which form a valid polygonal geometry.
    /// The set of extracted polygons is guaranteed to be edge-disjoint.
    /// This is useful for situations where it is known that the input lines form a
    /// valid polygonal geometry.</para>
    /// </remarks>
    ///
    public class Polygonizer
    {
        /// <summary>
        /// The default polygonizer output behavior
        /// </summary>
        public const bool AllPolys = false;

        /// <summary>
        /// Adds every linear element in a <see cref="IGeometry"/> into the polygonizer graph.
        /// </summary>
        private class LineStringAdder : IGeometryComponentFilter
        {
            private readonly Polygonizer _container;

            public LineStringAdder(Polygonizer container)
            {
                _container = container;
            }

            /// <summary>
            /// Filters all <see cref="ILineString"/> geometry instances
            /// </summary>
            /// <param name="g">The geometry instance</param>
            public void Filter(IGeometry g)
            {
                var lineString = g as ILineString;
                if (lineString != null)
                    _container.Add(lineString);
            }
        }

        /// <summary>
        /// Default linestring adder.
        /// </summary>
        private readonly LineStringAdder _lineStringAdder;

        private PolygonizeGraph _graph;

        // Initialized with empty collections, in case nothing is computed
        private ICollection<ILineString> _dangles = new List<ILineString>();
        private ICollection<ILineString> _cutEdges = new List<ILineString>();
        private IList<IGeometry> _invalidRingLines = new List<IGeometry>();
        private List<EdgeRing> _holeList;
        private List<EdgeRing> _shellList;
        private ICollection<IGeometry> _polyList;

        private bool _isCheckingRingsValid = true;
        private readonly bool _extractOnlyPolygonal;

        private IGeometryFactory _geomFactory;

        /// <summary>
        /// Allows disabling the valid ring checking,
        /// to optimize situations where invalid rings are not expected.
        /// </summary>
        /// <remarks>The default is <c>true</c></remarks>
        public bool IsCheckingRingsValid
        {
            get => _isCheckingRingsValid;
            set => _isCheckingRingsValid = value;
        }

        /// <summary>
        /// Creates a polygonizer with the same <see cref="IGeometryFactory"/>
        /// as the input <c>Geometry</c>s.
        /// The output mask is <see cref="AllPolys"/>
        /// </summary>
        ///
        public Polygonizer()
            :this(AllPolys)
        {

            _lineStringAdder = new LineStringAdder(this);
        }

        /// <summary>
        /// Creates a polygonizer and allow specifying if only polygons which form a valid polygonal geometry are to be extracted.
        /// </summary>
        /// <param name="extractOnlyPolygonal"><value>true</value> if only polygons which form a valid polygonal geometry are to be extracted</param>
        public Polygonizer(bool extractOnlyPolygonal)
        {
            _extractOnlyPolygonal = extractOnlyPolygonal;
            _lineStringAdder = new LineStringAdder(this);
        }

        /// <summary>
        /// Adds a collection of <see cref="IGeometry"/>s to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used.
        /// </summary>
        /// <param name="geomList">A list of <c>Geometry</c>s with linework to be polygonized.</param>
        public void Add(ICollection<IGeometry> geomList)
        {
            foreach (var geometry in geomList)
                Add(geometry);
        }

        /// <summary>
        /// Adds a <see cref="IGeometry"/> to the linework to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used
        /// </summary>
        /// <param name="g">A <c>Geometry</c> with linework to be polygonized.</param>
        public void Add(IGeometry g)
        {
            g.Apply(_lineStringAdder);
        }

        /// <summary>
        /// Adds a  to the graph of polygon edges.
        /// </summary>
        /// <param name="line">The <see cref="ILineString"/> to add.</param>
        private void Add(ILineString line)
        {
            // record the geometry factory for later use
            _geomFactory = line.Factory;
            // create a new graph using the factory from the input Geometry
            if (_graph == null)
                _graph = new PolygonizeGraph(line.Factory);
            _graph.AddEdge(line);
        }

        /// <summary>
        /// Gets the list of polygons formed by the polygonization.
        /// </summary>
        public ICollection<IGeometry> GetPolygons()
        {
            Polygonize();
            return _polyList;
        }

        /// <summary>
        /// Gets a geometry representing the polygons formed by the polygonization.
        /// If a valid polygonal geometry was extracted the result is a <see cref="IPolygonal"/> geometry.
        /// </summary>
        /// <returns>A geometry containing the polygons</returns>
        public IGeometry GetGeometry()
        {
            if (_geomFactory == null) _geomFactory = new Geometries.GeometryFactory();
            Polygonize();
            if (_extractOnlyPolygonal)
            {
                return _geomFactory.BuildGeometry(_polyList);
            }
            // result may not be valid Polygonal, so return as a GeometryCollection
            return _geomFactory.CreateGeometryCollection(Geometries.GeometryFactory.ToGeometryArray(_polyList));
        }

        /// <summary>
        /// Gets the list of dangling lines found during polygonization.
        /// </summary>
        public ICollection<ILineString> GetDangles()
        {
            Polygonize();
            return _dangles;
        }

        /// <summary>
        /// Gets the list of cut edges found during polygonization.
        /// </summary>
        public ICollection<ILineString> GetCutEdges()
        {
            Polygonize();
            return _cutEdges;
        }

        /// <summary>
        /// Gets the list of lines forming invalid rings found during polygonization.
        /// </summary>
        public IList<IGeometry> GetInvalidRingLines()
        {
            Polygonize();
            return _invalidRingLines;
        }

        /// <summary>
        /// Performs the polygonization, if it has not already been carried out.
        /// </summary>
        private void Polygonize()
        {
            // check if already computed
            if (_polyList != null)
                return;

            _polyList = new List<IGeometry>();

            // if no geometries were supplied it's possible that graph is null
            if (_graph == null)
                return;

            _dangles = _graph.DeleteDangles();
            _cutEdges = _graph.DeleteCutEdges();
            var edgeRingList = _graph.GetEdgeRings();

            var validEdgeRingList = new List<EdgeRing>();
            _invalidRingLines = new List<IGeometry>();
            if (IsCheckingRingsValid)
                 FindValidRings(edgeRingList, validEdgeRingList, _invalidRingLines);
            else validEdgeRingList = (List<EdgeRing>)edgeRingList;

            FindShellsAndHoles(validEdgeRingList);
            AssignHolesToShells(_holeList, _shellList);
            // order the shells to make any subsequent processing deterministic
            _shellList.Sort(new EdgeRing.EnvelopeComparator());

            bool includeAll = true;
            if (_extractOnlyPolygonal)
            {
                FindDisjointShells(_shellList);
                includeAll = false;
            }
            _polyList = ExtractPolygons(_shellList, includeAll);

        }

        private static void FindValidRings(IEnumerable<EdgeRing> edgeRingList, ICollection<EdgeRing> validEdgeRingList, ICollection<IGeometry> invalidRingList)
        {
            foreach (var er in edgeRingList)
            {
                if (er.IsValid)
                     validEdgeRingList.Add(er);
                else invalidRingList.Add(er.LineString);
            }
        }

        private void FindShellsAndHoles(IEnumerable<EdgeRing> edgeRingList)
        {
            _holeList = new List<EdgeRing>();
            _shellList = new List<EdgeRing>();
            foreach (var er in edgeRingList)
            {
                er.ComputeHole();
                if (er.IsHole)
                     _holeList.Add(er);
                else _shellList.Add(er);

            }
        }

        private static void AssignHolesToShells(IEnumerable<EdgeRing> holeList, List<EdgeRing> shellList)
        {
            foreach (var holeEdgeRing in holeList)
            {
                AssignHoleToShell(holeEdgeRing, shellList);
                /*
                if (!holeER.hasShell()) {
                    System.out.println("DEBUG: Outer hole: " + holeER);
                }
                */
            }
        }

        private static void AssignHoleToShell(EdgeRing holeEdgeRing, IList<EdgeRing> shellList)
        {
            var shell = EdgeRing.FindEdgeRingContaining(holeEdgeRing, shellList);
            if (shell != null) {
                shell.AddHole(holeEdgeRing);
            }
        }

        private static void FindDisjointShells(List<EdgeRing> shellList)
        {
            FindOuterShells(shellList);

            bool isMoreToScan;
            do
            {
                isMoreToScan = false;
                foreach(var er in shellList)
                {
                    if (er.IsIncludedSet)
                        continue;
                    er.UpdateIncluded();
                    if (!er.IsIncludedSet)
                    {
                        isMoreToScan = true;
                    }
                }
            } while (isMoreToScan);
        }

        /// <summary>
        /// For each outer hole finds and includes a single outer shell.
        /// This seeds the traversal algorithm for finding only polygonal shells.
        /// </summary>
        /// <param name="shellList">The list of shell EdgeRings</param>
        private static void FindOuterShells(List<EdgeRing> shellList)
        {

            foreach (var er in shellList)
            {
                var outerHoleER = er.OuterHole;
                if (outerHoleER != null && !outerHoleER.IsProcessed)
                {
                    er.IsIncluded = true;
                    outerHoleER.IsProcessed = true;
                }
            }
        }

        private static List<IGeometry> ExtractPolygons(List<EdgeRing> shellList, bool includeAll)
        {
            var polyList = new List<IGeometry>();
            foreach (var er in shellList)
            {
                if (includeAll || er.IsIncluded)
                {
                    polyList.Add(er.Polygon);
                }
            }
            return polyList;
        }

    }
}
