using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Computes the geometric overlay of two <see cref="Geometry"/>s,
    /// using an explicit precision model to allow robust computation.
    /// The overlay can be used to determine any of the
    /// following set-theoretic operations (boolean combinations) of the geometries:
    /// <list type="bullet">
    /// <item><term><see cref="SpatialFunction.Intersection"/></term><description>all points which lie in both geometries</description></item>
    /// <item><term><see cref="SpatialFunction.Union"/></term><description>all points which lie in at least one geometry</description></item>
    /// <item><term><see cref="SpatialFunction.Difference"/></term><description>all points which lie in the first geometry but not the second</description></item>
    /// <item><term><see cref="SpatialFunction.SymDifference"/></term><description>all points which lie in one geometry but not both</description></item>
    /// </list>
    /// Input geometries may have different dimension.
    /// Collections must be homogeneous
    /// (all elements must have the same dimension).
    /// <para/>
    /// The precision model used for the computation can be supplied
    /// independent of the precision model of the input geometry.
    /// The main use for this is to allow using a fixed precision
    /// for geometry with a floating precision model.
    /// This does two things: ensures robust computation;
    /// and forces the output to be validly rounded to the precision model.
    /// <para/>
    /// For fixed precision models noding is performed using a <see cref="SnapRoundingNoder"/>.
    /// This provides robust computation(as long as precision is limited to
    /// around 13 decimal digits).
    /// <para/>
    /// For floating precision an <see cref="MCIndexNoder"/> is used.
    /// This is not fully robust, so can sometimes result in 
    /// <see cref="TopologyException"/>s being thrown.
    /// For robust full-precision overlay see <see cref="OverlayNGSnapIfNeeded"/>.
    /// <para/>
    /// A custom <see cref="INoder"/> can be supplied.
    /// This allows using a more performant noding strategy in specific cases,
    /// for instance in <see cref="CoverageUnion"/>.
    /// <para/>
    /// <b>Note:</b> If a <see cref="SnappingNoder"/> is used
    /// it is best to specify a fairly small snap tolerance,
    /// since the intersection clipping optimization can
    /// interact with the snapping to alter the result.
    /// </summary>
    public class OverlayNG
    {
        /// <summary>
        /// The code for the Intersection overlay operation.
        /// </summary>
        public const SpatialFunction INTERSECTION = SpatialFunction.Intersection;

        /// <summary>
        /// The code for the Union overlay operation.
        /// </summary>
        public const SpatialFunction UNION = SpatialFunction.Union;

        /// <summary>
        /// The code for the Difference overlay operation.
        /// </summary>
        public const SpatialFunction DIFFERENCE = SpatialFunction.Difference;

        /// <summary>
        /// The code for the Symmetric Difference overlay operation.
        /// </summary>
        public const SpatialFunction SYMDIFFERENCE = SpatialFunction.SymDifference;

        /// <summary>
        /// Indicates whether intersections are allowed to produce
        /// heterogeneous results.
        /// <c>true</c> provides the classic JTS semantics
        /// (for proper boundary touches only -
        /// touching along collapses are not output).
        /// </summary>
        internal const bool ALLOW_INT_MIXED_INT_RESULT = true;


        /// <summary>
        /// Tests whether a point with a given topological <see cref="Label"/>
        /// relative to two geometries is contained in 
        /// the result of overlaying the geometries using
        /// a given overlay operation.
        /// <para/>
        /// The method handles arguments of <see cref="Location.Null"/> correctly
        /// </summary>
        /// <param name="label">The topological label of the point</param>
        /// <param name="opCode">The code for the overlay operation to test</param>
        /// <returns><c>true</c> if the label locations correspond to the overlay <paramref name="opCode"/></returns>
        static bool IsResultOfOpPoint(OverlayLabel label, SpatialFunction opCode)
        {
            var loc0 = label.GetLocation(0);
            var loc1 = label.GetLocation(1);
            return IsResultOfOp(opCode, loc0, loc1);
        }

        /// <summary>
        /// Tests whether a point with given <see cref="Location"/>s
        /// relative to two geometries would be contained in 
        /// the result of overlaying the geometries using
        /// a given overlay operation.
        /// This is used to determine whether components
        /// computed during the overlay process should be
        /// included in the result geometry.
        /// <para/>
        /// The method handles arguments of <see cref="Location.Null"/> correctly.
        /// </summary>
        /// <param name="overlayOpCode">The code for the overlay operation to test</param>
        /// <param name="loc0">The code for the location in the first geometry</param>
        /// <param name="loc1">The code for the location in the second geometry</param>
        /// <returns><c>true</c> if a point with given locations is in the result of the overlay operation</returns>
        internal static bool IsResultOfOp(SpatialFunction overlayOpCode, Location loc0, Location loc1)
        {
            if (loc0 == Location.Boundary) loc0 = Location.Interior;
            if (loc1 == Location.Boundary) loc1 = Location.Interior;
            switch (overlayOpCode)
            {
                case INTERSECTION:
                    return loc0 == Location.Interior
                        && loc1 == Location.Interior;
                case UNION:
                    return loc0 == Location.Interior
                        || loc1 == Location.Interior;
                case DIFFERENCE:
                    return loc0 == Location.Interior
                        && loc1 != Location.Interior;
                case SYMDIFFERENCE:
                    return (loc0 == Location.Interior && loc1 != Location.Interior)
                          || (loc0 != Location.Interior && loc1 == Location.Interior);
            }
            return false;
        }

        /// <summary>
        /// Computes an overlay operation for 
        /// the given geometry operands, with the
        /// noding strategy determined by the precision model.
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry Overlay(Geometry geom0, Geometry geom1,
            SpatialFunction opCode, PrecisionModel pm)
        {
            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            var geomOv = ov.GetResult();
            return geomOv;
        }

        /// <summary>
        /// Computes an overlay operation for 
        /// the given geometry operands, using a supplied <see cref="INoder"/>.
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <param name="pm">The precision model to use (which may be null if the noder does not use one)</param>
        /// <param name="noder">The noder to use</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry Overlay(Geometry geom0, Geometry geom1,
            SpatialFunction opCode, PrecisionModel pm, INoder noder)
        {
            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            ov.Noder = noder;
            var geomOv = ov.GetResult();
            return geomOv;
        }

        /// <summary>
        /// Computes an overlay operation on the given geometry operands,
        /// using a supplied <see cref="INoder"/>.
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <param name="noder">The noder to use</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry Overlay(Geometry geom0, Geometry geom1,
            SpatialFunction opCode, INoder noder)
        {
            var ov = new OverlayNG(geom0, geom1, null, opCode);
            ov.Noder = noder;
            var geomOv = ov.GetResult();
            return geomOv;
        }
        /// <summary>
        /// Computes an overlay operation on 
        /// the given geometry operands,
        /// using an automatically-determined fixed precision model
        /// which maximises precision while ensuring robust computation.
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry OverlayFixedPrecision(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            var pm = PrecisionUtility.RobustPM(geom0, geom1);
            //System.out.println("Precision Model: " + pm);

            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            return ov.GetResult();
        }

        /// <summary>
        /// Computes an overlay operation on 
        /// the given geometry operands,
        /// using the precision model of the geometry.
        /// and an appropriate noder.
        /// <para/>
        /// The noder is chosen according to the precision model specified.
        /// <list type="bullet">
        /// <item><description>For {@link PrecisionModel#FIXED}
        /// a snap-rounding noder is used, and the computation is robust.</description></item>
        /// <item><description>For {@link PrecisionModel#FLOATING}
        /// a non-snapping noder is used,
        /// and this computation may not be robust.
        /// If errors occur a {@link TopologyException} is thrown.</description></item>
        /// </list>
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <returns>The result of the overlay operation</returns>
        public static Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            var ov = new OverlayNG(geom0, geom1, opCode);
            return ov.GetResult();
        }

        /// <summary>
        /// Computes a union operation on 
        /// the given geometry, with the supplied precision model.
        /// The primary use for this is to perform precision reduction
        /// (round the geometry to the supplied precision).
        /// <para/>
        /// The input must be a valid geometry.
        /// Collections must be homogeneous.
        /// <para/>
        /// To union an overlapping set of polygons in a more performant way use <see cref="UnaryUnionNG"/>.
        /// To union a polyonal coverage or linear network in a more performant way, 
        /// use <see cref="CoverageUnion"/>.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The result of the union operation</returns>
        /// <seealso cref="OverlayMixedPoints"/>
        /// <seealso cref="PrecisionReducer"/>
        /// <seealso cref="UnaryUnionNG"/>
        /// <seealso cref="CoverageUnion"/>
        internal static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var ov = new OverlayNG(geom, pm);
            var geomOv = ov.GetResult();
            return geomOv;
        }

        /// <summary>
        /// Computes a union of a single geometry using a custom noder.
        /// <para/>
        /// The primary use of this is to support coverage union.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <param name="pm">The precision model to use (maybe be <c>null</c>)</param>
        /// <param name="noder">The noder to use</param>
        /// <returns>the result geometry</returns>
        /// <seealso cref="CoverageUnion"/>
        internal static Geometry Union(Geometry geom, PrecisionModel pm, INoder noder)
        {
            var ov = new OverlayNG(geom, pm);
            ov.Noder = noder;
            var geomOv = ov.GetResult();
            return geomOv;
        }

        private readonly SpatialFunction _opCode;
        private readonly InputGeometry _inputGeom;
        private readonly GeometryFactory _geomFact;
        private readonly PrecisionModel _pm;
        private INoder _noder;
        private bool _isOptimized = true;
        private bool _isOutputEdges;
        private bool _isOutputResultEdges;
        private bool _isOutputNodedEdges;

        //private Geometry outputEdges;

        /// <summary>
        /// Creates an overlay operation on the given geometries,
        /// with a defined precision model.
        /// </summary>
        /// <param name="geom0">The A operand geometry</param>
        /// <param name="geom1">The B operand geometry</param>
        /// <param name="pm">The precision model to use</param>
        /// <param name="opCode">The overlay opcode</param>
        public OverlayNG(Geometry geom0, Geometry geom1, PrecisionModel pm, SpatialFunction opCode)
        {
            _pm = pm;
            _opCode = opCode;
            _geomFact = geom0.Factory;
            _inputGeom = new InputGeometry(geom0, geom1);
        }

        /// <summary>
        /// Creates an overlay operation on the given geometries
        /// using the precision model of the geometries.
        /// <para/>
        /// The noder is chosen according to the precision model specified.
        /// <list type="bullet">
        /// <item><description>For <see cref="PrecisionModels.Fixed"/>
        /// a snap - rounding noder is used, and the computation is robust.</description></item>
        /// <item><description>For <see cref="PrecisionModels.Floating"/> a non - snapping noder is used,
        /// and this computation may not be robust.</description></item>
        /// If errors occur a <see cref="TopologyException"/> is thrown.
        /// </list>
        /// </summary>
        /// <param name="geom0">The A operand geometry</param>
        /// <param name="geom1">The B operand geometry</param>
        /// <param name="opCode">The overlay opcode</param>
        public OverlayNG(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            : this(geom0, geom1, geom0.Factory.PrecisionModel, opCode)
        {
        }

        private OverlayNG(Geometry geom0, PrecisionModel pm)
        {
            _pm = pm;
            _opCode = UNION;
            _geomFact = geom0.Factory;
            _inputGeom = new InputGeometry(geom0, null);
        }

        /// <summary>
        /// Gets or sets a value indicating whether overlay processing optimizations are enabled.
        /// <para/>
        /// It may be useful to disable optimizations
        /// for testing purposes.
        /// <para/>
        /// Default is <c>true</c> (optimization enabled).
        /// </summary>
        public bool Optimized
        {
            get => _isOptimized;
            set => _isOptimized = value;
        }

        public bool OutputEdges
        {
            get => _isOutputEdges;
            set => _isOutputEdges = value;
        }
        
        public bool OutputNodedEdges
        {
            get => _isOutputNodedEdges;
            set
            {
                if (value) OutputEdges = true;
                _isOutputNodedEdges = value;
            }
        }

        public bool OutputResultEdges
        {
            get => _isOutputResultEdges;
            set => _isOutputResultEdges = value;
        }

        private INoder Noder { get => _noder; set => _noder = value; }

        public Geometry GetResult()
        {
            if (OverlayUtility.IsEmptyResult(_opCode,
                _inputGeom.GetGeometry(0),
                _inputGeom.GetGeometry(1),
                _pm))
            {
                return CreateEmptyResult();
            }

            // special logic for Point-Point inputs
            if (_inputGeom.IsAllPoints)
            {
                return OverlayPoints.Overlay(_opCode, _inputGeom.GetGeometry(0), _inputGeom.GetGeometry(1), _pm);
            }

            // special logic for Point-nonPoint inputs 
            if (!_inputGeom.IsSingle && _inputGeom.HasPoints)
            {
                return OverlayMixedPoints.Overlay(_opCode, _inputGeom.GetGeometry(0), _inputGeom.GetGeometry(1), _pm);
            }

            var result = ComputeEdgeOverlay();

            return result;
        }

        private Geometry ComputeEdgeOverlay()
        {
            var edges = NodeEdges();

            var graph = BuildGraph(edges);

            if (_isOutputNodedEdges)
            {
                return OverlayUtility.ToLines(graph, _isOutputEdges, _geomFact);
            }

            LabelGraph(graph);
            //for (OverlayEdge e : graph.getEdges()) {  Debug.println(e);  }

            if (_isOutputEdges || _isOutputResultEdges)
            {
                return OverlayUtility.ToLines(graph, _isOutputEdges, _geomFact);
            }

            return ExtractResult(_opCode, graph);
        }

        private IList<Edge> NodeEdges()
        {
            /*
             * Node the edges, using whatever noder is being used
             */
            var nodingBuilder = new EdgeNodingBuilder(_pm, _noder);

            if (_isOptimized)
            {
                var clipEnv = OverlayUtility.ClippingEnvelope(_opCode, _inputGeom, _pm);
                if (clipEnv != null)
                    nodingBuilder.ClipEnvelope = clipEnv;
            }

            var mergedEdges = nodingBuilder.Build(
                _inputGeom.GetGeometry(0),
                _inputGeom.GetGeometry(1));

            /*
             * Record if an input geometry has collapsed.
             * This is used to avoid trying to locate disconnected edges
             * against a geometry which has collapsed completely.
             */
            _inputGeom.SetCollapsed(0, !nodingBuilder.HasEdgesFor(0));
            _inputGeom.SetCollapsed(1, !nodingBuilder.HasEdgesFor(1));

            return mergedEdges;
        }

        private OverlayGraph BuildGraph(IEnumerable<Edge> edges)
        {
            var graph = new OverlayGraph();
            foreach (var e in edges)
            {
                graph.AddEdge(e.Coordinates, e.CreateLabel());
            }
            return graph;
        }

        private void LabelGraph(OverlayGraph graph)
        {
            var labeller = new OverlayLabeller(graph, _inputGeom);
            labeller.ComputeLabelling();
            labeller.MarkResultAreaEdges(_opCode);
            labeller.UnmarkDuplicateEdgesFromResultArea();
        }

        /// <summary>
        /// Extracts the result geometry components from the fully labelled topology graph.
        /// <para/>
        /// This method implements the semantic that the result of an 
        /// intersection operation is homogeneous with highest dimension.  
        /// In other words, 
        /// if an intersection has components of a given dimension
        /// no lower-dimension components are output.
        /// For example, if two polygons intersect in an area, 
        /// no linestrings or points are included in the result, 
        /// even if portions of the input do meet in lines or points.
        /// This semantic choice makes more sense for typical usage, 
        /// in which only the highest dimension components are of interest.
        /// </summary>
        /// <param name="opCode">The overlay operation</param>
        /// <param name="graph">The topology graph</param>
        /// <returns>The result geometry</returns>
        private Geometry ExtractResult(SpatialFunction opCode, OverlayGraph graph)
        {

            //--- Build polygons
            var resultAreaEdges = graph.GetResultAreaEdges();
            var polyBuilder = new PolygonBuilder(resultAreaEdges, _geomFact);
            var resultPolyList = polyBuilder.GetPolygons();
            bool hasResultComponents = resultPolyList.Count > 0;

            //--- Build lines
            List<LineString> resultLineList = null;
            bool allowMixedIntResult = !hasResultComponents || ALLOW_INT_MIXED_INT_RESULT;
            if (opCode != INTERSECTION || allowMixedIntResult)
            {
                var lineBuilder = new LineBuilder(_inputGeom, graph, hasResultComponents, opCode, _geomFact);
                resultLineList = lineBuilder.GetLines();
            }
            hasResultComponents = hasResultComponents || resultLineList.Count > 0;
            /*
             * Since operations with point inputs are handled elsewhere,
             * this only handles the case where non-point inputs 
             * intersect in points. 
             */
            List<Point> resultPointList = null;
            allowMixedIntResult = !hasResultComponents || ALLOW_INT_MIXED_INT_RESULT;
            if (opCode == INTERSECTION && allowMixedIntResult)
            //if (opCode == INTERSECTION)
            {
                var pointBuilder = new IntersectionPointBuilder(graph, _geomFact);
                resultPointList = pointBuilder.Points;
            }

            if (IsEmpty(resultPolyList)
                && IsEmpty(resultLineList)
                && IsEmpty(resultPointList))
                return CreateEmptyResult();

            var resultGeom = OverlayUtility.CreateResultGeometry(resultPolyList, resultLineList, resultPointList, _geomFact);
            return resultGeom;
        }

        static bool IsEmpty<T>(IReadOnlyCollection<T> self)
        {
            return self == null || self.Count == 0;
        }

        private Geometry CreateEmptyResult()
        {
            return OverlayUtility.CreateEmptyResult(
                OverlayUtility.ResultDimension(_opCode,
                    _inputGeom.GetDimension(0),
                    _inputGeom.GetDimension(1)),
                  _geomFact);
        }
    }
}
