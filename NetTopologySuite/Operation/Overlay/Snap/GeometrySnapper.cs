using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using Iesi_NTS.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Snaps the vertices and segments of a <see cref="IGeometry"/> to another Geometry's vertices.
    /// Improves robustness for overlay operations, by eliminating
    /// nearly parallel edges (which cause problems during noding and intersection calculation).
    /// </summary>
    public class GeometrySnapper
    {
        private const double SnapPrexisionFactor = 10E-10;

        /// <summary>
        /// Estimates the snap tolerance for a Geometry, taking into account its precision model.
        /// </summary>
        /// <param name="g"></param>
        /// <returns>The estimated snap tolerance</returns>
        public static double ComputeOverlaySnapTolerance(IGeometry g)
        {
            double snapTolerance = ComputeSizeBasedSnapTolerance(g);

            /**
             * Overlay is carried out in most precise precision model 
             * of inputs.  
             * If this precision model is fixed, then the snap tolerance
             * must reflect the grid size.  
             * Precisely, the snap tolerance should be at least 
             * the distance from a corner of a precision grid cell
             * to the centre point of the cell.  
             */
            IPrecisionModel pm = g.PrecisionModel;
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
        public static double ComputeSizeBasedSnapTolerance(IGeometry g)
        {
            IEnvelope env = g.EnvelopeInternal;
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
        public static double ComputeOverlaySnapTolerance(IGeometry g0, IGeometry g1)
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
        public static IGeometry[] Snap(IGeometry g0, IGeometry g1, double snapTolerance)
        {
            IGeometry[] snapGeom = new IGeometry[2];
            GeometrySnapper snapper0 = new GeometrySnapper(g0);
            snapGeom[0] = snapper0.SnapTo(g1, snapTolerance);

            GeometrySnapper snapper1 = new GeometrySnapper(g1);
            /**
             * Snap the second geometry to the snapped first geometry
             * (this strategy minimizes the number of possible different points in the result)
             */
            snapGeom[1] = snapper1.SnapTo(snapGeom[0], snapTolerance);
            return snapGeom;
        }

        private IGeometry srcGeom;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        public GeometrySnapper(IGeometry g)
        {
            srcGeom = g;
        }

        /// <summary>
        /// Computes the snap tolerance based on the input geometries.
        /// </summary>
        /// <param name="ringPts"></param>
        /// <returns></returns>
        private double ComputeSnapTolerance(ICoordinate[] ringPts)
        {
            double minSegLen = ComputeMinimumSegmentLength(ringPts);
            // Use a small percentage of this to be safe
            double snapTol = minSegLen / 10;
            return snapTol;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private double ComputeMinimumSegmentLength(ICoordinate[] pts)
        {
            double minSegLen = Double.MaxValue;
            for (int i = 0; i < pts.Length - 1; i++) 
            {
                double segLen = pts[i].Distance(pts[i + 1]);
                if (segLen < minSegLen)
                    minSegLen = segLen;
            }
            return minSegLen;
        }

        /// <summary>
        ///  Snaps the vertices in the component <see cref="ILineString" />s
        ///  of the source geometry
        ///  to the vertices of the given geometry.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public IGeometry SnapTo(IGeometry g, double tolerance)
        {
            ICoordinate[] snapPts = ExtractTargetCoordinates(g);

            SnapTransformer snapTrans = new SnapTransformer(tolerance, snapPts);
            return snapTrans.Transform(srcGeom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public ICoordinate[] ExtractTargetCoordinates(IGeometry g)
        {
            // TODO: should do this more efficiently.  Use CoordSeq filter to get points, KDTree for uniqueness & queries
            ListSet<ICoordinate> ptSet = new ListSet<ICoordinate>(g.Coordinates);
            ICoordinate[] result = new ICoordinate[ptSet.Count];
            ptSet.CopyTo(result, 0);
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class SnapTransformer : GeometryTransformer
    {
        private double snapTolerance;
        private ICoordinate[] snapPts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snapTolerance"></param>
        /// <param name="snapPts"></param>
        public SnapTransformer(double snapTolerance, ICoordinate[] snapPts)
        {
            this.snapTolerance = snapTolerance;
            this.snapPts = snapPts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
        {
            ICoordinate[] srcPts = coords.ToCoordinateArray();
            ICoordinate[] newPts = SnapLine(srcPts, snapPts);
            return factory.CoordinateSequenceFactory.Create(newPts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcPts"></param>
        /// <param name="snapPts"></param>
        /// <returns></returns>
        private ICoordinate[] SnapLine(ICoordinate[] srcPts, ICoordinate[] snapPts)
        {
            LineStringSnapper snapper = new LineStringSnapper(srcPts, snapTolerance);
            return snapper.SnapTo(snapPts);
        }
    }
}
