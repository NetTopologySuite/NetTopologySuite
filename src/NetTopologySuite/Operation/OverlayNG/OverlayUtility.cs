using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Utility methods for overlay processing.
    /// </summary>
    /// <author>Martin Davis</author>
    internal static class OverlayUtility
    {
        /// <summary>
        /// A null-handling wrapper for <see cref="PrecisionModel.IsFloating"/>
        /// </summary>
        /// <param name="pm">A precision model</param>
        /// <returns><c>true</c> if the provided precision model is floating</returns>
        internal static bool IsFloating(PrecisionModel pm)
        {
            if (pm == null) return true;
            return pm.IsFloating;
        }

        /// <summary>
        /// Computes a clipping envelope for overlay input geometries.
        /// The clipping envelope encloses all geometry line segments which
        /// might participate in the overlay, with a buffer to
        /// account for numerical precision
        /// (in particular, rounding due to a precision model.
        /// The clipping envelope is used in both the <see cref="RingClipper"/>
        /// and in the <see cref="LineLimiter"/>.
        /// <para/>
        /// Some overlay operations (i.e. <see cref="SpatialFunction.Union"/> and <see cref="SpatialFunction.SymDifference"/>)
        /// cannot use clipping as an optimization,
        /// since the result envelope is the full extent of the two input geometries.
        /// In this case the returned
        /// envelope is <c>null</c> to indicate this.
        /// </summary>
        /// <param name="opCode">The overlay op code</param>
        /// <param name="inputGeom">The input geometries</param>
        /// <param name="pm">The precision model being used</param>
        /// <returns>An envelope for clipping and line limiting, or null if no clipping is performed</returns>
        internal static Envelope ClippingEnvelope(SpatialFunction opCode, InputGeometry inputGeom, PrecisionModel pm)
        {
            var resultEnv = ResultEnvelope(opCode, inputGeom, pm);
            if (resultEnv == null)
                return null;

            var clipEnv = RobustClipEnvelopeComputer.GetEnvelope(
                inputGeom.GetGeometry(0),
                inputGeom.GetGeometry(1),
                resultEnv);

            var safeEnv = SafeEnv(clipEnv, pm);
            return safeEnv;
        }

        /// <summary>
        /// Computes an envelope which covers the extent of the result of
        /// a given overlay operation for given inputs.
        /// The operations which have a result envelope smaller than the extent of the inputs
        /// are:
        /// <list type="bullet">
        /// <item><term><see cref="SpatialFunction.Intersection"/></term>
        /// <description>result envelope is the intersection of the input envelopes</description></item>
        /// <item><term><see cref="SpatialFunction.Difference"/></term>
        /// <description>result envelope is the envelope of the A input geometry</description></item>
        /// </list>
        /// Otherwise, <c>null</c> is returned to indicate full extent.
        /// </summary>
        /// <param name="opCode">The overlay op code</param>
        /// <param name="inputGeom">The input geometries</param>
        /// <param name="pm">The precision model being used</param>
        /// <returns>The result envelope, or null if the full extent</returns>
        private static Envelope ResultEnvelope(SpatialFunction opCode, InputGeometry inputGeom, PrecisionModel pm)
        {
            Envelope overlapEnv = null;
            switch (opCode)
            {
                case OverlayNG.INTERSECTION:
                    var envA = SafeEnv(inputGeom.GetEnvelope(0), pm);
                    var envB = SafeEnv(inputGeom.GetEnvelope(1), pm);
                    overlapEnv = envA.Intersection(envB);
                    break;
                case OverlayNG.DIFFERENCE:
                    overlapEnv = SafeEnv(inputGeom.GetEnvelope(0), pm);
                    break;
            }
            // return null for UNION and SYMDIFFERENCE to indicate no clipping
            return overlapEnv;
        }

        /// <summary>
        /// Determines a safe geometry envelope for clipping,
        /// taking into account the precision model being used.
        /// </summary>
        /// <param name="env">A safe geometry envelope for clipping</param>
        /// <param name="pm">The precision model</param>
        /// <returns>A safe envelope to use for clipping</returns>
       private static Envelope SafeEnv(Envelope env, PrecisionModel pm)
        {
            double envExpandDist = SafeExpandDistance(env, pm);
            var safeEnv = env.Copy();
            safeEnv.ExpandBy(envExpandDist);
            return safeEnv;
        }

        private const double SafeEnvBufferFactor = 0.1;
        private const double SafeEnvGridFactor = 3;

        private static double SafeExpandDistance(Envelope env, PrecisionModel pm)
        {
            double envExpandDist;
            if (IsFloating(pm))
            {
                // if PM is FLOAT then there is no scale factor, so add 10%
                double minSize = Math.Min(env.Height, env.Width);
                // heuristic to ensure zero-width envelopes don't cause total clipping
                if (minSize <= 0.0)
                    minSize = Math.Max(env.Height, env.Width);
                envExpandDist = SafeEnvBufferFactor * minSize;
            }
            else
            {
                // if PM is fixed, add a small multiple of the grid size
                double gridSize = 1.0 / pm.Scale;
                envExpandDist = SafeEnvGridFactor * gridSize;
            }
            return envExpandDist;
        }

        /// <summary>
        /// Tests if the result can be determined to be empty
        /// based on simple properties of the input geometries
        /// (such as whether one or both are empty, 
        /// or their envelopes are disjoint).
        /// </summary>
        /// <param name="opCode">The overlay operation</param>
        /// <param name="a">The A operand geometry</param>
        /// <param name="b">The B operand geometry</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns><c>true</c> if the overlay result is determined to be empty</returns>
        internal static bool IsEmptyResult(SpatialFunction opCode, Geometry a, Geometry b, PrecisionModel pm)
        {
            switch (opCode)
            {
                case OverlayNG.INTERSECTION:
                    if (IsEnvDisjoint(a, b, pm))
                        return true;
                    break;
                case OverlayNG.DIFFERENCE:
                    if (IsEmpty(a))
                        return true;
                    break;
                case OverlayNG.UNION:
                case OverlayNG.SYMDIFFERENCE:
                    if (IsEmpty(a) && IsEmpty(b))
                        return true;
                    break;
            }
            return false;
        }

        private static bool IsEmpty(Geometry geom)
        {
            return geom == null || geom.IsEmpty;
        }

        /// <summary>
        /// Tests if the geometry envelopes are disjoint, or empty.
        /// The disjoint test must take into account the precision model
        /// being used, since geometry coordinates may shift under rounding.
        /// </summary>
        /// <param name="a">The A operand geometry</param>
        /// <param name="b">The B operand geometry</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns><c>true</c> if the geometry envelopes are disjoint or empty</returns>
        private static bool IsEnvDisjoint(Geometry a, Geometry b, PrecisionModel pm)
        {
            if (IsEmpty(a) || IsEmpty(b)) return true;
            if (pm == null || pm.IsFloating)
            {
                return a.EnvelopeInternal.Disjoint(b.EnvelopeInternal);
            }
            return IsDisjoint(a.EnvelopeInternal, b.EnvelopeInternal, pm);
        }

        /// <summary>
        /// Tests for disjoint envelopes adjusting for rounding
        /// caused by a fixed precision model.
        /// Assumes envelopes are non-empty.
        /// </summary>
        /// <param name="envA">The A operand envelope</param>
        /// <param name="envB">The B operand envelope</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns><c>true</c> if the envelopes are disjoint</returns>
        private static bool IsDisjoint(Envelope envA, Envelope envB, PrecisionModel pm)
        {
            if (pm.MakePrecise(envB.MinX) > pm.MakePrecise(envA.MaxX)) return true;
            if (pm.MakePrecise(envB.MaxX) < pm.MakePrecise(envA.MinX)) return true;
            if (pm.MakePrecise(envB.MinY) > pm.MakePrecise(envA.MaxY)) return true;
            if (pm.MakePrecise(envB.MaxY) < pm.MakePrecise(envA.MinY)) return true;
            return false;
        }

        /// <summary>
        /// Creates an empty result geometry of the appropriate dimension,
        /// based on the given overlay operation and the dimensions of the inputs.
        /// The created geometry is an atomic geometry,
        /// not a collection(unless the dimension is <see cref="Dimension.Unknown"/>,
        /// in which case a <c>GEOMETRYCOLLECTION EMPTY</c> is created.
        /// </summary>
        /// <param name="dim">The dimension of the empty geometry</param>
        /// <param name="geomFact">The geometry factory being used for the operation</param>
        /// <returns>An empty atomic geometry of the appropriate dimension</returns>
        internal static Geometry CreateEmptyResult(Dimension dim, GeometryFactory geomFact)
        {
            Geometry result = null;
            switch (dim)
            {
                case Dimension.Point:
                    result = geomFact.CreatePoint();
                    break;
                case Dimension.Curve:
                    result = geomFact.CreateLineString();
                    break;
                case Dimension.Surface:
                    result = geomFact.CreatePolygon();
                    break;
                case Dimension.Unknown:
                    result = geomFact.CreateGeometryCollection();
                    break;
                default:
                    Assert.ShouldNeverReachHere("Unable to determine overlay result geometry dimension");
                    break;
            }
            return result;
        }


        /// <summary>
        /// Computes the dimension of the result of
        /// applying the given operation to inputs
        /// with the given dimensions.
        /// This assumes that complete collapse does not occur.
        /// <para/>
        /// The result dimension is computed using the following rules:
        /// <list type="bullet">
        /// <item><term><see cref="SpatialFunction.Intersection"/></term><description>result has the dimension of the lowest input dimension</description></item>
        /// <item><term><see cref="SpatialFunction.Union"/></term><description>result has the dimension of the highest input dimension</description></item>
        /// <item><term><see cref="SpatialFunction.Difference"/></term><description>result has the dimension of the left-hand input</description></item>
        /// <item><term><see cref="SpatialFunction.SymDifference"/></term><description>result has the dimension of the highest input dimension
        /// (since the Symmetric Difference is the Union of the Differences).</description></item>
        /// </list>
        /// </summary>
        /// <param name="opCode">The overlay operation</param>
        /// <param name="dim0">Dimension of the LH input</param>
        /// <param name="dim1">Dimension of the RH input</param>
        /// <returns></returns>
        internal static Dimension ResultDimension(SpatialFunction opCode, Dimension dim0, Dimension dim1)
        {
            var resultDimension = Dimension.False;
            switch (opCode)
            {
                case OverlayNG.INTERSECTION:
                    resultDimension = (Dimension)Math.Min((int)dim0, (int)dim1);
                    break;
                case OverlayNG.UNION:
                    resultDimension = (Dimension)Math.Max((int)dim0, (int)dim1);
                    break;
                case OverlayNG.DIFFERENCE:
                    resultDimension = dim0;
                    break;
                case OverlayNG.SYMDIFFERENCE:
                    /*
                     * This result is chosen because
                     * <pre>
                     * SymDiff = Union( Diff(A, B), Diff(B, A) )
                     * </pre>
                     * and Union has the dimension of the highest-dimension argument.
                     */
                    resultDimension = (Dimension)Math.Max((int)dim0, (int)dim1);
                    break;
            }
            return resultDimension;
        }

        /// <summary>
        /// Creates an overlay result geometry for homogeneous or mixed components.
        /// </summary>
        /// <param name="resultPolyList">An enumeration of result polygons (may be empty or <c>null</c>)</param>
        /// <param name="resultLineList">An enumeration of result lines (may be empty or <c>null</c>)</param>
        /// <param name="resultPointList">An enumeration of result points (may be empty or <c>null</c>)</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        /// <returns>A geometry structured according to the overlay result semantics</returns>
        internal static Geometry CreateResultGeometry(IEnumerable<Polygon> resultPolyList, IEnumerable<LineString> resultLineList, IEnumerable<Point> resultPointList, GeometryFactory geometryFactory)
        {
            var geomList = new List<Geometry>();

            // TODO: for mixed dimension, return collection of Multigeom for each dimension (breaking change)

            // element geometries of the result are always in the order A,L,P
            if (resultPolyList != null) geomList.AddRange(resultPolyList);
            if (resultLineList != null) geomList.AddRange(resultLineList);
            if (resultPointList != null) geomList.AddRange(resultPointList);

            // build the most specific geometry possible
            // TODO: perhaps do this internally to give more control?
            return geometryFactory.BuildGeometry(geomList);
        }

        internal static Geometry ToLines(OverlayGraph graph, bool isOutputEdges, GeometryFactory geomFact)
        {
            var lines = new List<LineString>();
            foreach (var edge in graph.Edges)
            {
                bool includeEdge = isOutputEdges || edge.IsInResultArea;
                if (!includeEdge) continue;
                //Coordinate[] pts = getCoords(nss);
                var pts = edge.CoordinatesOriented;
                var line = geomFact.CreateLineString(pts);
                line.UserData = LabelForResult(edge);
                lines.Add(line);
            }
            return geomFact.BuildGeometry(lines);
        }

        private static string LabelForResult(OverlayEdge edge)
        {
            return edge.Label.ToString(edge.IsForward)
                + (edge.IsInResultArea ? " Res" : "");
        }

        /// <summary>
        /// Round the key point if precision model is fixed.
        /// Note: return value is only copied if rounding is performed.
        /// </summary>
        /// <param name="pt">The point to round</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The rounded point coordinate, or null if empty</returns>
        public static Coordinate Round(Point pt, PrecisionModel pm)
        {
            if (pt.IsEmpty) return null;
            var p = pt.Coordinate.Copy();
            if (!pm.IsFloating)
                pm.MakePrecise(p);
            return p;
        }

        /*
        private void checkSanity(Geometry result) {
          // for Union, area should be greater than largest of inputs
          double areaA = inputGeom.getGeometry(0).getArea();
          double areaB = inputGeom.getGeometry(1).getArea();
          double area = result.getArea();

          // if result is empty probably had a complete collapse, so can't use this check
          if (area == 0) return;

          if (opCode == UNION) {
            double minAreaLimit = 0.5 * Math.max(areaA, areaB);
            if (area < minAreaLimit ) {
              throw new TopologyException("Result area sanity issue");
            }
          }
        }
      */

    }

}
