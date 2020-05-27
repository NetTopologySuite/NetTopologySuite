using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Computes the geometric overlay of two {@link Geometry}s, 
     * using an explicit precision model to allow robust computation. 
     * The overlay can be used to determine any of the 
     * following set-theoretic operations (boolean combinations) of the geometries:
     * <ul>
     * <li>{@link INTERSECTION} - all points which lie in both geometries
     * <li>{@link UNION} - all points which lie in at least one geometry
     * <li>{@link DIFFERENCE} - all points which lie in the first geometry but not the second
     * <li>{@link SYMDIFFERENCE} - all points which lie in one geometry but not both
     * </ul>
     * Input geometries may have different dimension.  
     * Collections must be homogeneous
     * (all elements must have the same dimension).
     * <p>
     * The precision model used for the computation can be supplied 
     * independent of the precision model of the input geometry.
     * The main use for this is to allow using a fixed precision 
     * for geometry with a floating precision model.
     * This does two things: ensures robust computation;
     * and forces the output to be validly rounded to the precision model. 
     * <p>
     * For fixed precision models noding is performed using snap-rounding.
     * This provides robust computation (as long as precision is limited to
     * around 13 decimal digits).
     * <p>
     * For floating precision the conventional JTS noder is used. 
     * This is not fully robust, so can sometimes result in 
     * {@link TopologyException}s being thrown. 
     * <p>
     * A custom {@link Noder} can be supplied.
     * This allows using a more performant noding strategy in specific cases, 
     * for instance in {@link CoverageUnion}.
     * 
     * @author mdavis
     *
     */
    public class OverlayNG
    {
        /**
         * The code for the Intersection overlay operation.
         */
        public const SpatialFunction INTERSECTION = SpatialFunction.Intersection;

        /**
         * The code for the Union overlay operation.
         */
        public const SpatialFunction UNION = SpatialFunction.Union;

        /**
         *  The code for the Difference overlay operation.
         */
        public const SpatialFunction DIFFERENCE = SpatialFunction.Difference;

        /**
         *  The code for the Symmetric Difference overlay operation.
         */
        public const SpatialFunction SYMDIFFERENCE = SpatialFunction.SymDifference;

        /**
         * Tests whether a point with a given topological {@link Label}
         * relative to two geometries is contained in 
         * the result of overlaying the geometries using
         * a given overlay operation.
         * <p>
         * The method handles arguments of {@link Location#NONE} correctly
         * 
         * @param label the topological label of the point
         * @param opCode the code for the overlay operation to test
         * @return true if the label locations correspond to the overlayOpCode
         */
        static bool isResultOfOpPoint(OverlayLabel label, SpatialFunction opCode)
        {
            var loc0 = label.GetLocation(0);
            var loc1 = label.GetLocation(1);
            return isResultOfOp(opCode, loc0, loc1);
        }

        /**
         * Tests whether a point with given {@link Location}s
         * relative to two geometries would be contained in 
         * the result of overlaying the geometries using
         * a given overlay operation.
         * This is used to determine whether components
         * computed during the overlay process should be
         * included in the result geometry.
         * <p>
         * The method handles arguments of {@link Location#NONE} correctly.
         * 
         * @param overlayOpCode the code for the overlay operation to test
         * @param loc0 the code for the location in the first geometry 
         * @param loc1 the code for the location in the second geometry 
         *
         * @return true if a point with given locations is in the result of the overlay operation
         */
        internal static bool isResultOfOp(SpatialFunction overlayOpCode, Location loc0, Location loc1)
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

        /**
         * Computes an overlay operation for 
         * the given geometry operands.
         * 
         * @param geom0 the first geometry argument
         * @param geom1 the second geometry argument
         * @param opCode the code for the desired overlay operation
         * @param pm the precision model to use
         * @return the result of the overlay operation
         */
        public static Geometry overlay(Geometry geom0, Geometry geom1,
            SpatialFunction opCode, PrecisionModel pm)
        {
            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            var geomOv = ov.getResult();
            return geomOv;
        }

        /**
         * Computes an overlay operation on the given geometry operands, 
         * using a supplied {@link Noder}.
         * 
         * @param geom0 the first geometry argument
         * @param geom1 the second geometry argument
         * @param opCode the code for the desired overlay operation
         * @param pm the precision model to use (which may be null if the noder does not use one)
         * @param noder the noder to use
         * @return the result of the overlay operation
         */
        public static Geometry overlay(Geometry geom0, Geometry geom1,
            SpatialFunction opCode, PrecisionModel pm, INoder noder)
        {
            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            ov.Noder = noder;
            var geomOv = ov.getResult();
            return geomOv;
        }

        /**
         * Computes an overlay operation on 
         * the given geometry operands,
         * using an automatically-determined fixed precision model
         * which maximises precision while ensuring robust computation.
         * 
         * @param geom0 the first geometry argument
         * @param geom1 the second geometry argument
         * @param opCode the code for the desired overlay operation
         * @return the result of the overlay operation
         */
        public static Geometry overlayFixedPrecision(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            var pm = PrecisionUtil.RobustPM(geom0, geom1);
            //System.out.println("Precision Model: " + pm);

            var ov = new OverlayNG(geom0, geom1, pm, opCode);
            return ov.getResult();
        }

        /**
         * Computes an overlay operation on 
         * the given geometry operands,
         * using the precision model of the geometry.
         * and an appropriate noder.
         * <p>
         * The noder is chosen according to the precision model specified.
         * <ul>
         * <li>For {@link PrecisionModel#FIXED}
         * a snap-rounding noder is used, and the computation is robust.
         * <li>For {@link PrecisionModel#FLOATING}
         * a non-snapping noder is used,
         * and this computation may not be robust.
         * If errors occur a {@link TopologyException} is thrown.
         * </ul>
         * 
         * 
         * @param geom0 the first argument geometry
         * @param geom1 the second argument geometry
         * @param opCode the code for the desired overlay operation
         * @return the result of the overlay operation
         */
        public static Geometry overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            var ov = new OverlayNG(geom0, geom1, opCode);
            return ov.getResult();
        }

        /**
         * Computes a union operation on 
         * the given geometry, with the supplied precision model.
         * The primary use for this is to perform precision reduction
         * (round the geometry to the supplied precision).
         * <p>
         * The input must be a valid geometry.
         * Collections must be homogeneous.
         * <p>
         * To union an overlapping set of polygons in a more performant way use {@link UnaryUnionNG}.
         * To union a polyonal coverage or linear network in a more performant way, 
         * use {@link CoverageUnion}.
         * 
         * @param geom0 the geometry
         * @param pm the precision model to use
         * @return the result of the union operation
         * 
         * @see PrecisionReducer
         * @see UnaryUnionNG
         */
        internal static Geometry Union(Geometry geom, PrecisionModel pm)
        {
            var ov = new OverlayNG(geom, pm);
            var geomOv = ov.getResult();
            return geomOv;
        }

        /**
         * Computes a union of a single geometry using a custom noder.
         * <p>
         * The primary use of this is to support coverage union.
         * 
         * @param geom the geometry to union
         * @param pm the precision model to use (maybe be null)
         * @param noder the noder to use
         * @return the result geometry
         * 
         * @see CoverageUnion
         */
        internal static Geometry Union(Geometry geom, PrecisionModel pm, INoder noder)
        {
            var ov = new OverlayNG(geom, pm);
            ov.Noder = noder;
            var geomOv = ov.getResult();
            return geomOv;
        }

        private readonly SpatialFunction _opCode;
        private readonly InputGeometry _inputGeom;
        private readonly GeometryFactory _geomFact;
        private readonly PrecisionModel _pm;
        private readonly INoder _noder;
        private bool _isOptimized = true;
        private bool _isOutputEdges;
        private bool _isOutputResultEdges;
        private bool _isOutputNodedEdges;

        //private Geometry outputEdges;

        /**
         * Creates an overlay operation on the given geometries,
         * with a defined precision model.
         * 
         * @param geom0 the A operand geometry
         * @param geom1 the B operand geometry
         * @param pm the precision model to use
         * @param opCode the overlay opcode
         */
        public OverlayNG(Geometry geom0, Geometry geom1, PrecisionModel pm, SpatialFunction opCode)
        {
            this._pm = pm;
            this._opCode = opCode;
            _geomFact = geom0.Factory;
            _inputGeom = new InputGeometry(geom0, geom1);
        }

        /**
         * Creates an overlay operation on the given geometries
         * using the precision model of the geometries.
         * <p>
         * The noder is chosen according to the precision model specified.
         * <ul>
         * <li>For {@link PrecisionModel#FIXED}
         * a snap-rounding noder is used, and the computation is robust.
         * <li>For {@link PrecisionModel#FLOATING}
         * a non-snapping noder is used,
         * and this computation may not be robust.
         * If errors occur a {@link TopologyException} is thrown.
         * </ul>
         *  
         * @param geom0 the A operand geometry
         * @param geom1 the B operand geometry
         * @param opCode the overlay opcode
         */
        public OverlayNG(Geometry geom0, Geometry geom1, SpatialFunction opCode)
            : this(geom0, geom1, geom0.Factory.PrecisionModel, opCode)
        {
        }

        private OverlayNG(Geometry geom0, PrecisionModel pm)
        {
            this._pm = pm;
            this._opCode = UNION;
            _geomFact = geom0.Factory;
            _inputGeom = new InputGeometry(geom0, null);
        }

        /**
         * Sets whether overlay processing optimizations are enabled.
         * It may be useful to disable optimizations
         * for testing purposes.
         * Default is TRUE (optimization enabled).
         * 
         * @param isOptimized whether to optimize processing
         */
        [Obsolete("Use Optimized property")]
        public void setOptimized(bool isOptimized)
        {
            this._isOptimized = isOptimized;
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
            get => this._isOptimized;
            set => this._isOptimized = value;
        }

        [Obsolete("Use OutputEdges property")]
        public void setOutputEdges(bool isOutputEdges)
        {
            OutputEdges = isOutputEdges;
        }

        public bool OutputEdges
        {
            get => _isOutputEdges;
            set => _isOutputEdges = value;
        }
        
        [Obsolete("Use OutputNodedEdges property")]
        public void setOutputNodedEdges(bool isOutputNodedEdges)
        {
            OutputNodedEdges = isOutputNodedEdges;
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

        [Obsolete("Use OutputResultEdges property")]
        public void setOutputResultEdges(bool isOutputResultEdges)
        {
            this._isOutputResultEdges = isOutputResultEdges;
        }

        public bool OutputResultEdges
        {
            get => _isOutputResultEdges;
            set => _isOutputResultEdges = value;
        }

        private INoder Noder { get; set; }

        [Obsolete("use Noder property")]
        private void setNoder(INoder noder)
        {
            Noder = noder;
        }

        public Geometry getResult()
        {
            if (OverlayUtility.isEmptyResult(_opCode,
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

            var result = computeEdgeOverlay();

            return result;
        }

        private Geometry computeEdgeOverlay()
        {

            var graph = BuildGraph();

            if (_isOutputNodedEdges)
            {
                return OverlayUtility.toLines(graph, _isOutputEdges, _geomFact);
            }

            LabelGraph(graph);

            if (_isOutputEdges || _isOutputResultEdges)
            {
                return OverlayUtility.toLines(graph, _isOutputEdges, _geomFact);
            }

            return extractResult(_opCode, graph);
        }

        private OverlayGraph BuildGraph()
        {
            /*
             * Node the edges, using whatever noder is being used
             */
            var ovNoder = new OverlayNoder(_pm);

            if (_noder != null) ovNoder.Noder = _noder;

            if (_isOptimized)
            {
                var clipEnv = OverlayUtility.clippingEnvelope(_opCode, _inputGeom, _pm);
                if (clipEnv != null)
                    ovNoder.ClipEnvelope = clipEnv;
            }

            ovNoder.Add(_inputGeom.GetGeometry(0), 0);
            ovNoder.Add(_inputGeom.GetGeometry(1), 1);
            var nodedLines = ovNoder.Node();

            /*
             * Merge the noded edges to eliminate duplicates.
             * Labels will be combined.
             */
            // nodedSegStrings are no longer needed, and will be GCed
            var edges = Edge.CreateEdges(nodedLines);
            var mergedEdges = EdgeMerger.Merge(edges);

            /*
             * Record if an input geometry has collapsed.
             * This is used to avoid trying to locate disconnected edges
             * against a geometry which has collapsed completely.
             */
            _inputGeom.SetCollapsed(0, !ovNoder.HasEdgesFor(0));
            _inputGeom.SetCollapsed(1, !ovNoder.HasEdgesFor(1));

            return new OverlayGraph(mergedEdges);
        }

        private void LabelGraph(OverlayGraph graph)
        {
            var labeller = new OverlayLabeller(graph, _inputGeom);
            labeller.ComputeLabelling();
            labeller.markResultAreaEdges(_opCode);
            labeller.unmarkDuplicateEdgesFromResultArea();
        }

        /**
         * Extracts the result geometry components from the fully labelled topology graph.
         * <p>
         * This method implements the semantic that the result of an 
         * intersection operation is homogeneous with highest dimension.  
         * In other words, 
         * if an intersection has components of a given dimension
         * no lower-dimension components are output.
         * For example, if two polygons intersect in an area, 
         * no linestrings or points are included in the result, 
         * even if portions of the input do meet in lines or points.
         * This semantic choice makes more sense for typical usage, 
         * in which only the highest dimension components are of interest.
         * 
         * @param opCode the overlay operation
         * @param graph the topology graph
         * @return the result geometry
         */
        private Geometry extractResult(SpatialFunction opCode, OverlayGraph graph)
        {

            //--- Build polygons
            var resultAreaEdges = graph.GetResultAreaEdges();
            var polyBuilder = new PolygonBuilder(resultAreaEdges, _geomFact);
            var resultPolyList = polyBuilder.getPolygons();
            bool hasResultComponents = resultPolyList.Count > 0;

            //--- Build lines
            List<LineString> resultLineList = null;
            if (opCode != INTERSECTION || !hasResultComponents)
            {
                var lineBuilder = new LineBuilder(_inputGeom, graph, hasResultComponents, opCode, _geomFact);
                resultLineList = lineBuilder.GetLines();
                hasResultComponents = resultLineList.Count > 0;
            }
            /**
             * Since operations with point inputs are handled elsewhere,
             * this only handles the case where non-point inputs 
             * intersect in points ONLY. 
             */
            List<Point> resultPointList = null;
            if (opCode == INTERSECTION && !hasResultComponents)
            {
                var pointBuilder = new IntersectionPointBuilder(graph, _geomFact);
                resultPointList = pointBuilder.Points;
            }

            if ((resultPolyList == null || resultPolyList.Count == 0)
                && (resultLineList == null || resultLineList.Count == 0)
                && (resultPointList == null || resultPointList.Count == 0))
                return CreateEmptyResult();

            var resultGeom = OverlayUtility.createResultGeometry(resultPolyList, resultLineList, resultPointList, _geomFact);
            return resultGeom;
        }

        private Geometry CreateEmptyResult()
        {
            return OverlayUtility.createEmptyResult(
                OverlayUtility.resultDimension(_opCode,
                    _inputGeom.GetDimension(0),
                    _inputGeom.GetDimension(1)),
                  _geomFact);
        }

    }
}
