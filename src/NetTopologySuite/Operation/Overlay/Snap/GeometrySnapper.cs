using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Snaps the vertices and segments of a <see cref="Geometry"/>
    /// to another Geometry's vertices.
    /// A snap distance tolerance is used to control where snapping is performed.
    /// Snapping one geometry to another can improve
    /// robustness for overlay operations by eliminating
    /// nearly-coincident edges
    /// (which cause problems during noding and intersection calculation).
    /// It can also be used to eliminate artifacts such as narrow slivers, spikes and gores.
    /// Too much snapping can result in invalid topology
    /// beging created, so the number and location of snapped vertices
    /// is decided using heuristics to determine when it
    /// is safe to snap.
    /// This can result in some potential snaps being omitted, however.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometrySnapper
    {
        private const double SnapPrexisionFactor = 1E-9;

        /// <summary>
        /// Estimates the snap tolerance for a Geometry, taking into account its precision model.
        /// </summary>
        /// <param name="g"></param>
        /// <returns>The estimated snap tolerance</returns>
        public static double ComputeOverlaySnapTolerance(Geometry g)
        {
            double snapTolerance = ComputeSizeBasedSnapTolerance(g);

            /*
             * Overlay is carried out in the precision model
             * of the two inputs.
             * If this precision model is of type FIXED, then the snap tolerance
             * must reflect the precision grid size.
             * Specifically, the snap tolerance should be at least
             * the distance from a corner of a precision grid cell
             * to the centre point of the cell.
             */
            var pm = g.PrecisionModel;
            if (pm.PrecisionModelType == PrecisionModels.Fixed)
            {
                double fixedSnapTol = (1 / pm.Scale) * 2 / 1.415;
                if (fixedSnapTol > snapTolerance)
                    snapTolerance = fixedSnapTol;
            }
            return snapTolerance;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static double ComputeSizeBasedSnapTolerance(Geometry g)
        {
            var env = g.EnvelopeInternal;
            double minDimension = Math.Min(env.Height, env.Width);
            double snapTol = minDimension * SnapPrexisionFactor;
            return snapTol;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static double ComputeOverlaySnapTolerance(Geometry g0, Geometry g1)
        {
            return Math.Min(ComputeOverlaySnapTolerance(g0), ComputeOverlaySnapTolerance(g1));
        }

        /// <summary>
        /// Snaps two geometries together with a given tolerance.
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="snapTolerance"></param>
        /// <returns></returns>
        public static Geometry[] Snap(Geometry g0, Geometry g1, double snapTolerance)
        {
            var snapGeom = new Geometry[2];

            var snapper0 = new GeometrySnapper(g0);
            snapGeom[0] = snapper0.SnapTo(g1, snapTolerance);

            /*
             * Snap the second geometry to the snapped first geometry
             * (this strategy minimizes the number of possible different points in the result)
             */
            var snapper1 = new GeometrySnapper(g1);
            snapGeom[1] = snapper1.SnapTo(snapGeom[0], snapTolerance);
            return snapGeom;
        }

        /// <summary>
        /// Snaps a geometry to itself.
        /// Allows optionally cleaning the result to ensure it is topologically valid
        /// (which fixes issues such as topology collapses in polygonal inputs).
        /// Snapping a geometry to itself can remove artifacts such as very narrow slivers, gores and spikes.
        /// </summary>
        /// <param name="geom">the geometry to snap</param>
        /// <param name="snapTolerance">the snapping tolerance</param>
        /// <param name="cleanResult">whether the result should be made valid</param>
        /// <returns>a new snapped <see cref="Geometry"/></returns>
        public static Geometry SnapToSelf(Geometry geom, double snapTolerance, bool cleanResult)
        {
            var snapper0 = new GeometrySnapper(geom);
            return snapper0.SnapToSelf(snapTolerance, cleanResult);
        }

        private readonly Geometry _srcGeom;

        /// <summary>
        /// Creates a new snapper acting on the given geometry
        /// </summary>
        /// <param name="g">the geometry to snap</param>
        public GeometrySnapper(Geometry g)
        {
            _srcGeom = g;
        }

        /// <summary>
        ///  Snaps the vertices in the component <see cref="LineString" />s
        ///  of the source geometry to the vertices of the given snap geometry.
        /// </summary>
        /// <param name="g">a geometry to snap the source to</param>
        /// <param name="tolerance"></param>
        /// <returns>a new snapped Geometry</returns>
        public Geometry SnapTo(Geometry g, double tolerance)
        {
            var snapPts = ExtractTargetCoordinates(g);

            var snapTrans = new SnapTransformer(tolerance, snapPts);
            return snapTrans.Transform(_srcGeom);
        }

        /// Snaps the vertices in the component <see cref="LineString" />s
        /// of the source geometry to the vertices of the same geometry.
        /// Allows optionally cleaning the result to ensure it is topologically valid
        /// (which fixes issues such as topology collapses in polygonal inputs).
        /// <param name="snapTolerance">The snapping tolerance</param>
        /// <param name="cleanResult">Whether the result should be made valid</param>
        /// <returns>The geometry snapped to itself</returns>
        public Geometry SnapToSelf(double snapTolerance, bool cleanResult)
        {
            var snapPts = ExtractTargetCoordinates(_srcGeom);

            var snapTrans = new SnapTransformer(snapTolerance, snapPts, true);
            var snappedGeom = snapTrans.Transform(_srcGeom);
            var result = snappedGeom;
            if (cleanResult && result is IPolygonal)
            {
                // TODO: use better cleaning approach
                result = snappedGeom.Buffer(0);
            }
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        private Coordinate[] ExtractTargetCoordinates(Geometry g)
        {
            // TODO: should do this more efficiently.  Use CoordSeq filter to get points, KDTree for uniqueness & queries
            var ptSet = new HashSet<Coordinate>(g.Coordinates);
            var result = new Coordinate[ptSet.Count];
            ptSet.CopyTo(result, 0);
            Array.Sort(result);
            return result;
        }

        /// <summary>
        /// Computes the snap tolerance based on the input geometries.
        /// </summary>
        private static double ComputeSnapTolerance(Coordinate[] ringPts)
        {
            double minSegLen = ComputeMinimumSegmentLength(ringPts);
            // use a small percentage of this to be safe
            double snapTol = minSegLen / 10;
            return snapTol;
        }

        private static double ComputeMinimumSegmentLength(Coordinate[] pts)
        {
            double minSegLen = double.MaxValue;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                double segLen = pts[i].Distance(pts[i + 1]);
                if (segLen < minSegLen)
                    minSegLen = segLen;
            }
            return minSegLen;
        }
    }

    /// <summary>
    ///
    /// </summary>
    internal class SnapTransformer : GeometryTransformer
    {
        private readonly double _snapTolerance;
        private readonly Coordinate[] _snapPts;
        private readonly bool _isSelfSnap;

        /// <summary>
        ///
        /// </summary>
        /// <param name="snapTolerance"></param>
        /// <param name="snapPts"></param>
        public SnapTransformer(double snapTolerance, Coordinate[] snapPts)
        {
            _snapTolerance = snapTolerance;
            _snapPts = snapPts;
        }

        public SnapTransformer(double snapTolerance, Coordinate[] snapPts, bool isSelfSnap)
            : this(snapTolerance, snapPts)
        {
            _isSelfSnap = isSelfSnap;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override CoordinateSequence TransformCoordinates(CoordinateSequence coords, Geometry parent)
        {
            var srcPts = coords.ToCoordinateArray();
            var newPts = SnapLine(srcPts, _snapPts);
            return Factory.CoordinateSequenceFactory.Create(newPts);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="srcPts"></param>
        /// <param name="snapPts"></param>
        /// <returns></returns>
        private Coordinate[] SnapLine(Coordinate[] srcPts, Coordinate[] snapPts)
        {
            var snapper = new LineStringSnapper(srcPts, _snapTolerance);
            snapper.AllowSnappingToSourceVertices = _isSelfSnap;
            return snapper.SnapTo(snapPts);
        }
    }
}