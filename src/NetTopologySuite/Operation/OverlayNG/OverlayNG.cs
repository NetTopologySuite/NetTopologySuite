using System;
using System.Collections.Generic;
using System.ComponentModel;

using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Computes the geometric overlay of two <see cref="Geometry"/>s,
    /// using an explicit precision model to allow robust computation.
    /// <para/>
    /// The overlay can be used to determine any of the
    /// following set-theoretic operations (boolean combinations) of the geometries:
    /// <list type="bullet">
    /// <item><term><see cref="SpatialFunction.Intersection"/></term><description>all points which lie in both geometries</description></item>
    /// <item><term><see cref="SpatialFunction.Union"/></term><description>all points which lie in at least one geometry</description></item>
    /// <item><term><see cref="SpatialFunction.Difference"/></term><description>all points which lie in the first geometry but not the second</description></item>
    /// <item><term><see cref="SpatialFunction.SymDifference"/></term><description>all points which lie in one geometry but not both</description></item>
    /// </list>
    /// Input geometries may have different dimension.
    /// Input collections must be homogeneous (all elements must have the same dimension).
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
    /// For robust full-precision overlay see <see cref="OverlayNGRobust"/>.
    /// <para/>
    /// A custom <see cref="INoder"/> can be supplied.
    /// This allows using a more performant noding strategy in specific cases,
    /// for instance in <see cref="CoverageUnion"/>.
    /// <para/>
    /// <b>Note:</b> If a <see cref="SnappingNoder"/> is used
    /// it is best to specify a fairly small snap tolerance,
    /// since the intersection clipping optimization can
    /// interact with the snapping to alter the result.
    /// <para/>
    /// Optionally the overlay computation can process using strict mode
    /// (via <see cref="StrictMode"/> = <c>true</c>).
    /// In strict mode result semantics are:
    /// <list type="bullet">
    /// <item><description>Lines and Points resulting from topology collapses are not included in the result</description></item>
    /// <item><description>Result geometry is homogeneous
    /// for the <see cref="SpatialFunction.Intersection"/> and <see cref="SpatialFunction.Difference"/> operations.</description></item>
    /// <item><description>Result geometry is homogeneous
    /// for the <see cref="SpatialFunction.Union"/> and <see cref="SpatialFunction.SymDifference"/> operations
    /// if the inputs have the same dimension</description></item>
    /// </list>
    /// Strict mode has the following benefits:
    /// <list type="bullet">
    /// <item><description>Results are simpler</description></item>
    /// <item><description>Overlay operations are easily chainable
    /// without needing to remove lower-dimension elements</description></item>
    /// </list>
    /// The original JTS overlay semantics corresponds to non-strict mode.
    /// <para/>
    /// If a robustness error occurs, a <see cref="TopologyException"/> is thrown.
    /// These are usually caused by numerical rounding causing the noding output
    /// to not be fully noded.
    /// For robust computation with full-precision <see cref="OverlayNGRobust"/>
    /// can be used.
    /// </summary>
    public sealed class OverlayNG
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
        /// The default setting for Strict Mode.
        /// <para/>
        /// The original JTS overlay semantics used non-strict result
        /// semantics, including;<br/>
        /// - An Intersection result can be mixed-dimension,
        ///   due to inclusion of intersection components of all dimensions <br/>
        /// - Results can include lines caused by Area topology collapse
        /// </summary>
        internal const bool STRICT_MODE_DEFAULT = false;

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
        private static bool IsResultOfOpPoint(OverlayLabel label, SpatialFunction opCode)
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
        /// using the precision model of the geometry.
        /// and an appropriate noder.
        /// <para/>
        /// The noder is chosen according to the precision model specified.
        /// <list type="bullet">
        /// <item><description>For <see cref="PrecisionModels.Fixed"/>
        /// a snap-rounding noder is used, and the computation is robust.</description></item>
        /// <item><description>For <see cref="PrecisionModels.Floating"/>
        /// a non-snapping noder is used,
        /// and this computation may not be robust.
        /// If errors occur a <see cref="TopologyException"/> is thrown.</description></item>
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
        /// <para/>
        /// The input must be a valid geometry.
        /// Collections must be homogeneous.
        /// <para/>
        /// To union an overlapping set of polygons in a more performant way use <see cref="UnaryUnionNG"/>.
        /// To union a polygonal coverage or linear network in a more performant way, 
        /// use <see cref="CoverageUnion"/>.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The result of the union operation</returns>
        /// <seealso cref="OverlayMixedPoints"/>
        internal static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var ov = new OverlayNG(geom, null, pm, SpatialFunction.Union);
            var geomOv = ov.GetResult();
            return geomOv;
        }

        /// <summary>
        /// Computes a union of a single geometry using a custom noder.
        /// <para/>
        /// The primary use of this is to support coverage union.
        /// Because of this the overlay is performed using strict mode.
        /// </summary>
        /// <param name="geom">The geometry to union</param>
        /// <param name="pm">The precision model to use (maybe be <c>null</c>)</param>
        /// <param name="noder">The noder to use</param>
        /// <returns>the result geometry</returns>
        /// <seealso cref="CoverageUnion"/>
        internal static Geometry Union(Geometry geom, PrecisionModel pm, INoder noder)
        {
            var ov = new OverlayNG(geom, null, pm, SpatialFunction.Union);
            ov.Noder = noder;
            ov.StrictMode = true;
            var geomOv = ov.GetResult();
            return geomOv;
        }

        private readonly SpatialFunction _opCode;
        private readonly InputGeometry _inputGeom;
        private readonly GeometryFactory _geomFact;
        private readonly PrecisionModel _pm;
        private bool _isOutputNodedEdges;

        /// <summary>
        /// Creates an overlay operation on the given geometries,
        /// with a defined precision model.
        /// </summary>
        /// <param name="geom0">The A operand geometry</param>
        /// <param name="geom1">The B operand geometry (may be <c>null</c>)</param>
        /// <param name="pm">The precision model to use</param>
        /// <param name="opCode">The overlay opcode</param>
        public OverlayNG(Geometry geom0, Geometry geom1, PrecisionModel pm, SpatialFunction opCode)
        {
            if (geom0 == null)
            {
                throw new ArgumentNullException(nameof(geom0));
            }

            switch (opCode)
            {
                case SpatialFunction.Intersection:
                case SpatialFunction.Union:
                case SpatialFunction.Difference:
                case SpatialFunction.SymDifference:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(opCode), opCode, "Only Intersection, Union, Difference, and SymDifference are recognized at this time.");
            }

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
        /// <param name="geom1">The B operand geometry (may be <c>null</c>)</param>
        /// <param name="opCode">The overlay opcode</param>
        public OverlayNG(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            : this(geom0, geom1, geom0?.Factory.PrecisionModel, opCode)
        {
        }

        /// <summary>
        /// Creates a union of a single geometry with a given precision model.
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <param name="pm">The precision model to use</param>
        internal OverlayNG(Geometry geom, PrecisionModel pm)
            : this(geom, null, pm, UNION)
        { }

        /// <summary>
        /// Gets or sets whether the overlay results are computed according to strict mode
        /// semantics.
        /// <list type="bullet">
        /// <item><description>Lines resulting from topology collapse are not included</description></item>
        /// <item><description>Result geometry is homogeneous for the
        /// <see cref="SpatialFunction.Intersection"/> and
        /// <see cref="SpatialFunction.Difference"/> operations.</description></item>
        /// <item><description>Result geometry is homogeneous for the
        /// <see cref="SpatialFunction.Union"/> and
        /// <see cref="SpatialFunction.SymDifference"/> operations if the inputs have the same dimension</description></item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        public bool StrictMode { get; set; } = STRICT_MODE_DEFAULT;

        /// <summary>
        /// Gets or sets a value indicating whether overlay processing optimizations are enabled.
        /// <para/>
        /// It may be useful to disable optimizations
        /// for testing purposes.
        /// <para/>
        /// Default is <c>true</c> (optimization enabled).
        /// </summary>
        public bool Optimized { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the result can contain only <see cref="Polygon"/> components.
        /// This is used if it is known that the result must be an (possibly empty) area.
        /// </summary>
        /// <returns><c>true</c> if the result should contain only area components</returns>
        public bool AreaResultOnly { get; set; } = false;

        //------ Testing options -------

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool OutputEdges { get; set; }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool OutputNodedEdges
        {
            get => _isOutputNodedEdges;
            set
            {
                if (value) OutputEdges = true;
                _isOutputNodedEdges = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool OutputResultEdges { get; set; }

        //---------------------------------

        public INoder Noder { get; set; }

        /// <summary>
        /// Gets the result of the overlay operation.
        /// e</summary>
        /// <returns>The result of the overlay operation.</returns>
        /// <exception cref="ArgumentException">Thrown, if the input is not supported (e.g. a mixed-dimension geometry)</exception>
        /// <exception cref="TopologyException">Thrown, if a robustness error occurs</exception>
        public Geometry GetResult()
        {
            // handle empty inputs which determine result
            if (OverlayUtility.IsEmptyResult(_opCode,
            _inputGeom.GetGeometry(0),
                _inputGeom.GetGeometry(1),
                _pm))
            {
                return CreateEmptyResult();
            }

            /*
             * The elevation model is only computed if the input geometries have Z values.
             */
            var elevModel = ElevationModel.Create(_inputGeom.GetGeometry(0), _inputGeom.GetGeometry(1));
            Geometry result;
            if (_inputGeom.IsAllPoints)
            {
                // handle Point-Point inputs
                result = OverlayPoints.Overlay(_opCode, _inputGeom.GetGeometry(0), _inputGeom.GetGeometry(1), _pm);
            }
            else if (!_inputGeom.IsSingle && _inputGeom.HasPoints)
            {
                // handle Point-nonPoint inputs 
                result = OverlayMixedPoints.Overlay(_opCode, _inputGeom.GetGeometry(0), _inputGeom.GetGeometry(1), _pm);
            }
            else
            {
                // handle case where both inputs are formed of edges (Lines and Polygons)
                result = ComputeEdgeOverlay();
            }
            /*
             * This is a no-op if the elevation model was not computed due to Z not present
             */
            elevModel.PopulateZ(result);
            return result;
        }

        private Geometry ComputeEdgeOverlay()
        {
            var edges = NodeEdges();

            var graph = BuildGraph(edges);

            if (_isOutputNodedEdges)
            {
                return OverlayUtility.ToLines(graph, OutputEdges, _geomFact);
            }

            LabelGraph(graph);
            //for (OverlayEdge e : graph.getEdges()) {  Debug.println(e);  }

            if (OutputEdges || OutputResultEdges)
            {
                return OverlayUtility.ToLines(graph, OutputEdges, _geomFact);
            }

            return ExtractResult(_opCode, graph);
        }

        private IList<Edge> NodeEdges()
        {
            /*
             * Node the edges, using whatever noder is being used
             */
            var nodingBuilder = new EdgeNodingBuilder(_pm, Noder);

            /*
             * Optimize Intersection and Difference by clipping to the 
             * result extent, if enabled.
             */
            if (Optimized)
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
            bool isAllowMixedIntResult = !StrictMode;

            //--- Build polygons
            var resultAreaEdges = graph.GetResultAreaEdges();
            var polyBuilder = new PolygonBuilder(resultAreaEdges, _geomFact);
            var resultPolyList = polyBuilder.GetPolygons();
            bool hasResultAreaComponents = resultPolyList.Count > 0;

            List<LineString> resultLineList = null;
            List<Point> resultPointList = null;

            if (!AreaResultOnly)
            {
                //--- Build lines
                bool allowResultLines = !hasResultAreaComponents
                                     || isAllowMixedIntResult
                                     || opCode == SYMDIFFERENCE
                                     || opCode == UNION;
                if (allowResultLines)
                {
                    var lineBuilder = new LineBuilder(_inputGeom, graph, hasResultAreaComponents, opCode, _geomFact);
                    lineBuilder.StrictMode = StrictMode;
                    resultLineList = lineBuilder.GetLines();
                }
                /*
                 * Operations with point inputs are handled elsewhere.
                 * Only an Intersection op can produce point results
                 * from non-point inputs. 
                 */
                bool hasResultComponents = hasResultAreaComponents || resultLineList.Count > 0;
                bool allowResultPoints = !hasResultComponents || isAllowMixedIntResult;
                if (opCode == INTERSECTION && allowResultPoints)
                {
                    var pointBuilder = new IntersectionPointBuilder(graph, _geomFact);
                    pointBuilder.StrictMode = StrictMode;
                    resultPointList = pointBuilder.Points;
                }
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
