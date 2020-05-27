using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Utility methods for overlay processing.
    /// </summary>
    /// <author>Martin Davis</author>
    class OverlayUtility
    {

        private const int SafeEnvExpandFactor = 3;

        static double expandDistance(Envelope env, PrecisionModel pm)
        {
            double envExpandDist;
            if (pm.IsFloating)
            {
                // if PM is FLOAT then there is no scale factor, so add 10%
                double minSize = Math.Min(env.Height, env.Width);
                envExpandDist = 0.1 * minSize;
            }
            else
            {
                // if PM is fixed, add a small multiple of the grid size
                double gridSize = 1.0 / pm.Scale;
                envExpandDist = SafeEnvExpandFactor * gridSize;
            }
            return envExpandDist;
        }

        static Envelope safeOverlapEnv(Envelope env, PrecisionModel pm)
        {
            double envExpandDist = expandDistance(env, pm);
            var safeEnv = env.Copy();
            safeEnv.ExpandBy(envExpandDist);
            return safeEnv;
        }

        internal static Envelope clippingEnvelope(SpatialFunction opCode, InputGeometry inputGeom, PrecisionModel pm)
        {
            Envelope clipEnv = null;
            switch (opCode)
            {
                case OverlayNG.INTERSECTION:
                    var envA = safeOverlapEnv(inputGeom.GetEnvelope(0), pm);
                    var envB = safeOverlapEnv(inputGeom.GetEnvelope(1), pm);
                    clipEnv = envA.Intersection(envB);
                    break;
                case OverlayNG.DIFFERENCE:
                    clipEnv = safeOverlapEnv(inputGeom.GetEnvelope(0), pm);
                    break;
            }
            return clipEnv;
        }

        /**
         * Tests if the result can be determined to be empty
         * based on simple properties of the input geometries
         * (such as whether one or both are empty, 
         * or their envelopes are disjoint).
         * 
         * @param opCode the overlay operation
         * @param inputGeom the input geometries
         * @return true if the overlay result is determined to be empty
         */
        internal static bool isEmptyResult(SpatialFunction opCode, Geometry a, Geometry b, PrecisionModel pm)
        {
            switch (opCode)
            {
                case OverlayNG.INTERSECTION:
                    if (isEnvDisjoint(a, b, pm))
                        return true;
                    break;
                case OverlayNG.DIFFERENCE:
                    if (isEmpty(a))
                        return true;
                    break;
                case OverlayNG.UNION:
                case OverlayNG.SYMDIFFERENCE:
                    if (isEmpty(a) && isEmpty(b))
                        return true;
                    break;
            }
            return false;
        }

        private static bool isEmpty(Geometry geom)
        {
            return geom == null || geom.IsEmpty;
        }

        /**
         * Tests if the geometry envelopes are disjoint, or empty.
         * The disjoint test must take into account the precision model
         * being used, since geometry coordinates may shift under rounding.
         * 
         * @param a a geometry
         * @param b a geometry
         * @param pm the precision model being used
         * @return true if the geometry envelopes are disjoint or empty
         */
        static bool isEnvDisjoint(Geometry a, Geometry b, PrecisionModel pm)
        {
            if (isEmpty(a) || isEmpty(b)) return true;
            if (pm.IsFloating)
            {
                return a.EnvelopeInternal.Disjoint(b.EnvelopeInternal);
            }
            return isDisjoint(a.EnvelopeInternal, b.EnvelopeInternal, pm);
        }

        /**
         * Tests for disjoint envelopes adjusting for rounding 
         * caused by a fixed precision model.
         * Assumes envelopes are non-empty.
         * 
         * @param envA an envelope
         * @param envB an envelope
         * @param pm the precision model
         * @return true if the envelopes are disjoint
         */
        private static bool isDisjoint(Envelope envA, Envelope envB, PrecisionModel pm)
        {
            if (pm.MakePrecise(envB.MinX) > pm.MakePrecise(envA.MaxX)) return true;
            if (pm.MakePrecise(envB.MaxX) < pm.MakePrecise(envA.MinX)) return true;
            if (pm.MakePrecise(envB.MinY) > pm.MakePrecise(envA.MaxY)) return true;
            if (pm.MakePrecise(envB.MaxY) < pm.MakePrecise(envA.MinY)) return true;
            return false;
        }

        /**
         * Creates an empty result geometry of the appropriate dimension,
         * based on the given overlay operation and the dimensions of the inputs.
         * The created geometry is always an atomic geometry, 
         * not a collection.
         * <p>
         * The empty result is constructed using the following rules:
         * <ul>
         * <li>{@link OverlayNG#INTERSECTION} - result has the dimension of the lowest input dimension
         * <li>{@link OverlayNG#UNION} - result has the dimension of the highest input dimension
         * <li>{@link OverlayNG#DIFFERENCE} - result has the dimension of the left-hand input
         * <li>{@link OverlayNG#SYMDIFFERENCE} - result has the dimension of the highest input dimension
         * (since the Symmetric Difference is the Union of the Differences).
         * </ul>
         * 
         * @param overlayOpCode the code for the overlay operation being performed
         * @param a an input geometry
         * @param b an input geometry
         * @param geomFact the geometry factory being used for the operation
         * @return an empty atomic geometry of the appropriate dimension
         */
        internal static Geometry createEmptyResult(Dimension dim, GeometryFactory geomFact)
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
                default:
                    Assert.ShouldNeverReachHere("Unable to determine overlay result geometry dimension");
                    break;
            }
            return result;
        }

        /**
         * Computes the dimension of the result of
         * applying the given operation to inputs
         * with the given dimensions.
         * This assumes that complete collapse does not occur.
         * 
         * @param opCode the overlay operation
         * @param dim0 dimension of the LH input
         * @param dim1 dimension of the RH input
         * @return
         */
        internal static Dimension resultDimension(SpatialFunction opCode, Dimension dim0, Dimension dim1)
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
                    /**
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

        /**
         * Creates an overlay result geometry for homogeneous or mixed components.
         *  
         * @param resultPolyList the list of result polygons (may be empty or null)
         * @param resultLineList the list of result lines (may be empty or null)
         * @param resultPointList the list of result points (may be empty or null)
         * @param geometryFactory the geometry factory to use
         * @return a geometry structured according to the overlay result semantics
         */
        internal static Geometry createResultGeometry(IEnumerable<Polygon> resultPolyList, IEnumerable<LineString> resultLineList, IEnumerable<Point> resultPointList, GeometryFactory geometryFactory)
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

        internal static Geometry toLines(OverlayGraph graph, bool isOutputEdges, GeometryFactory geomFact)
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

        /**
         * Round the key point if precision model is fixed.
         * Note: return value is only copied if rounding is performed.
         * 
         * @param pt the Point to round
         * @return the rounded point coordinate, or null if empty
         */
        public static Coordinate round(Point pt, PrecisionModel pm)
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
