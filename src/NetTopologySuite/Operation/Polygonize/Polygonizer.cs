using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Polygonize
{
    /// <summary>
    /// Polygonizes a set of <see cref="Geometry"/>s which contain linework that
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
    /// <list type="Table">
    /// <item><term><see cref="GetDangles"/>Dangles</term><description>edges which have one or both ends which are not incident on another edge endpoint</description></item>
    /// <item><term><see cref="GetCutEdges"/></term><description>edges which are connected at both ends but which do not form part of polygon</description></item>
    /// <item><term><see cref="GetInvalidRingLines"/></term><description>edges which form rings which are invalid
    /// (e.g. the component lines contain a self-intersection)</description>
    /// </item></list>
    /// </para>
    /// <para>
    /// The <see cref="Polygonizer(bool)"/> constructor allows
    /// extracting only polygons which form a valid polygonal result.
    /// The set of extracted polygons is guaranteed to be edge-disjoint.
    /// This is useful for situations where it is known that the input lines form a
    /// valid polygonal geometry (which may include holes or nested polygons).</para>
    /// </remarks>
    ///
    public class Polygonizer
    {
        /// <summary>
        /// The default polygonizer output behavior
        /// </summary>
        public const bool AllPolys = false;

        /// <summary>
        /// Adds every linear element in a <see cref="Geometry"/> into the polygonizer graph.
        /// </summary>
        private class LineStringAdder : IGeometryComponentFilter
        {
            private readonly Polygonizer _container;

            public LineStringAdder(Polygonizer container)
            {
                _container = container;
            }

            /// <summary>
            /// Filters all <see cref="LineString"/> geometry instances
            /// </summary>
            /// <param name="g">The geometry instance</param>
            public void Filter(Geometry g)
            {
                var lineString = g as LineString;
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
        private ICollection<LineString> _dangles = new List<LineString>();
        private ICollection<LineString> _cutEdges = new List<LineString>();
        private IList<Geometry> _invalidRingLines = new List<Geometry>();
        private IList<EdgeRing> _holeList;
        private List<EdgeRing> _shellList;
        private ICollection<Geometry> _polyList;

        private bool _isCheckingRingsValid = true;
        private readonly bool _extractOnlyPolygonal;

        private GeometryFactory _geomFactory;

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
        /// Creates a polygonizer that extracts all polygons.
        /// </summary>
        public Polygonizer()
            :this(false)
        {

            _lineStringAdder = new LineStringAdder(this);
        }

        /// <summary>
        /// Creates a polygonizer, specifying whether a valid polygonal geometry must be created.
        /// If the argument is <c>true</c>
        /// then areas may be discarded in order to 
        /// ensure that the extracted geometry is a valid polygonal geometry.
        /// </summary>
        /// <param name="extractOnlyPolygonal"><c>true</c> if a valid polygonal geometry should be extracted</param>
        public Polygonizer(bool extractOnlyPolygonal)
        {
            _extractOnlyPolygonal = extractOnlyPolygonal;
            _lineStringAdder = new LineStringAdder(this);
        }

        /// <summary>
        /// Adds a collection of <see cref="Geometry"/>s to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used.
        /// </summary>
        /// <param name="geomList">A list of <c>Geometry</c>s with linework to be polygonized.</param>
        public void Add(ICollection<Geometry> geomList)
        {
            foreach (var geometry in geomList)
                Add(geometry);
        }

        /// <summary>
        /// Adds a <see cref="Geometry"/> to the linework to be polygonized.
        /// May be called multiple times.
        /// Any dimension of Geometry may be added;
        /// the constituent linework will be extracted and used
        /// </summary>
        /// <param name="g">A <c>Geometry</c> with linework to be polygonized.</param>
        public void Add(Geometry g)
        {
            g.Apply(_lineStringAdder);
        }

        /// <summary>
        /// Adds a  to the graph of polygon edges.
        /// </summary>
        /// <param name="line">The <see cref="LineString"/> to add.</param>
        private void Add(LineString line)
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
        public ICollection<Geometry> GetPolygons()
        {
            Polygonize();
            return _polyList;
        }

        /// <summary>
        /// Gets a geometry representing the polygons formed by the polygonization.
        /// If a valid polygonal geometry was extracted the result is a <see cref="IPolygonal"/> geometry.
        /// </summary>
        /// <returns>A geometry containing the polygons</returns>
        public Geometry GetGeometry()
        {
            if (_geomFactory == null) _geomFactory = new GeometryFactory();
            Polygonize();
            if (_extractOnlyPolygonal)
            {
                return _geomFactory.BuildGeometry(_polyList);
            }
            // result may not be valid Polygonal, so return as a GeometryCollection
            return _geomFactory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(_polyList));
        }

        /// <summary>
        /// Gets the list of dangling lines found during polygonization.
        /// </summary>
        public ICollection<LineString> GetDangles()
        {
            Polygonize();
            return _dangles;
        }

        /// <summary>
        /// Gets the list of cut edges found during polygonization.
        /// </summary>
        public ICollection<LineString> GetCutEdges()
        {
            Polygonize();
            return _cutEdges;
        }

        /// <summary>
        /// Gets the list of lines forming invalid rings found during polygonization.
        /// </summary>
        public IList<Geometry> GetInvalidRingLines()
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

            _polyList = new List<Geometry>();

            // if no geometries were supplied it's possible that graph is null
            if (_graph == null)
                return;

            _dangles = _graph.DeleteDangles();
            _cutEdges = _graph.DeleteCutEdges();
            var edgeRingList = _graph.GetEdgeRings();

            var validEdgeRingList = new List<EdgeRing>();
            var invalidRings = new List<EdgeRing>();
            if (IsCheckingRingsValid)
            {
                FindValidRings(edgeRingList, validEdgeRingList, invalidRings);
                _invalidRingLines = ExtractInvalidLines(invalidRings);
            }
            else validEdgeRingList = (List<EdgeRing>)edgeRingList;

            FindShellsAndHoles(validEdgeRingList);
            HoleAssigner.AssignHolesToShells(_holeList, _shellList);

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

        private static void FindValidRings(IEnumerable<EdgeRing> edgeRingList, ICollection<EdgeRing> validEdgeRingList, ICollection<EdgeRing> invalidRingList)
        {
            foreach (var er in edgeRingList)
            {
                er.ComputeValid();
                if (er.IsValid)
                    validEdgeRingList.Add(er);
                else
                    invalidRingList.Add(er);
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

        /// <summary>Extracts unique lines for invalid rings,
        /// discarding rings which correspond to outer rings and hence contain
        /// duplicate linework.
        /// </summary>
        /// <param name="invalidRings">A list of edge rings</param>
        /// <returns>The list of invalid LineString geometries</returns>
        private IList<Geometry> ExtractInvalidLines(List<EdgeRing> invalidRings)
        {
            /*
             * Sort rings by increasing envelope area.
             * This causes inner rings to be processed before the outer rings
             * containing them, which allows outer invalid rings to be discarded
             * since their linework is already reported in the inner rings.
             */
            invalidRings.Sort(new EdgeRing.EnvelopeAreaComparator());
            /*
             * Scan through rings.  Keep only rings which have an adjacent EdgeRing
             * which is either valid or marked as not processed.  
             * This avoids including outer rings which have linework which is duplicated.
             */
            var invalidLines = new List<Geometry>();
            foreach (var er in invalidRings)
            {
                if (IsIncludedInvalid(er))
                {
                    invalidLines.Add(er.LineString);
                }
                er.IsProcessed = true;
            }
            return invalidLines;
        }

        /// <summary>
        /// Tests if a invalid ring should be included in
        /// the list of reported invalid rings.
        /// <para/>
        /// Rings are included only if they contain
        /// linework which is not already in a valid ring,
        /// or in an already-included ring.
        /// <para/>
        /// Because the invalid rings list is sorted by extent area,
        /// this results in outer rings being discarded,
        /// since all their linework is reported in the rings they contain.
        /// </summary>
        /// <param name="invalidRing">The ring to test</param>
        /// <returns><c>true</c> if the ring should be included</returns>
        private bool IsIncludedInvalid(EdgeRing invalidRing)
        {
            foreach (var de in invalidRing.Edges)
            {
                var deAdj = (PolygonizeDirectedEdge)de.Sym;
                var erAdj = deAdj.Ring;
                /**
                 * 
                 */
                bool isEdgeIncluded = erAdj.IsValid || erAdj.IsProcessed;
                if (!isEdgeIncluded)
                    return true;
            }
            return false;
        }

        private static List<Geometry> ExtractPolygons(List<EdgeRing> shellList, bool includeAll)
        {
            var polyList = new List<Geometry>();
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
