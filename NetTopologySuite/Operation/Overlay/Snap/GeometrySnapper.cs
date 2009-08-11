using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    ///<summary>
    ///Snaps the vertices and segments of a <see cref="IGeometry{TCoordinate}"/> to another Geometry's vertices.
    ///Improves robustness for overlay operations, by eliminating
    ///nearly parallel edges (which cause problems during noding and intersection calculation).
    ///
    ///@author Martin Davis
    ///@version 1.7
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class GeometrySnapper<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        private const double SnapPrecisionFactor = 10e-10;

        ///<summary>
        /// Estimates the snap tolerance for a Geometry, taking into account its precision model.
        ///</summary>
        ///<param name="g">a Geometry</param>
        ///<returns>the estimated snap tolerance</returns>
        public static double ComputeOverlaySnapTolerance(IGeometry<TCoordinate> g)
        {
            double snapTolerance = ComputeSizeBasedSnapTolerance(g);

            /**
             * Overlay is carried out in the precision model 
             * of the two inputs.  
             * If this precision model is of type 'Fixed', then the snap tolerance
             * must reflect the precision grid size.  
             * Specifically, the snap tolerance should be at least 
             * the distance from a corner of a precision grid cell
             * to the centre point of the cell.  
             */
            IPrecisionModel pm = g.Factory.PrecisionModel;
            if (pm.PrecisionModelType == PrecisionModelType.Fixed)
            {
                double fixedSnapTol = (1d / pm.Scale) * 2d / 1.415d;
                if (fixedSnapTol > snapTolerance)
                    snapTolerance = fixedSnapTol;
            }
            return snapTolerance;
        }

        public static double ComputeSizeBasedSnapTolerance(IGeometry<TCoordinate> g)
        {
            IExtents<TCoordinate> env = g.Extents;
            Double minDimension = Math.Min((env.GetMax(Ordinates.X) - env.GetMin(Ordinates.X)),
                                           (env.GetMax(Ordinates.Y) - env.GetMin(Ordinates.Y)));
            Double snapTol = minDimension * SnapPrecisionFactor;
            return snapTol;
        }

        public static double ComputeOverlaySnapTolerance(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return Math.Min(ComputeOverlaySnapTolerance(g0), ComputeOverlaySnapTolerance(g1));
        }

        ///<summary>
        /// Snaps two geometries together with a given tolerance.
        ///</summary>
        ///<param name="g0">a <see cref="IGeometry{TCoordinate}"/></param> to snap</param>
        ///<param name="g1">a <see cref="IGeometry{TCoordinate}"/></param> to snap</param>
        ///<param name="snapTolerance"></param>
        ///<returns>the snapped geometries</returns>
        public static IGeometry<TCoordinate>[] Snap(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, Double snapTolerance)
        {
            IGeometry<TCoordinate>[] snapGeom = new IGeometry<TCoordinate>[2];
            GeometrySnapper<TCoordinate> snapper0 = new GeometrySnapper<TCoordinate>(g0);
            snapGeom[0] = snapper0.SnapTo(g1, snapTolerance);

            GeometrySnapper<TCoordinate> snapper1 = new GeometrySnapper<TCoordinate>(g1);
            /**
             * Snap the second geometry to the snapped first geometry
             * (this strategy minimizes the number of possible different points in the result)
             */
            snapGeom[1] = snapper1.SnapTo(snapGeom[0], snapTolerance);

            //    System.out.println(snap[0]);
            //    System.out.println(snap[1]);
            return snapGeom;
        }

        private IGeometry<TCoordinate> srcGeom;

        ///<summary>
        /// Creates a new snapper acting on the given geometry
        ///</summary>
        ///<param name="srcGeom">the geometry to snap</param>
        public GeometrySnapper(IGeometry<TCoordinate> srcGeom)
        {
            this.srcGeom = srcGeom;
        }

        ///<summary>
        /// Computes the snap tolerance based on the input geometries.
        ///</summary>
        ///<param name="ringPts"></param>
        /// <returns></returns>
        private Double ComputeSnapTolerance(ICoordinateSequence<TCoordinate> ringPts)
        {
            double minSegLen = ComputeMinimumSegmentLength(ringPts);
            // use a small percentage of this to be safe
            double snapTol = minSegLen / 10;
            return snapTol;
        }

        private Double ComputeMinimumSegmentLength(ICoordinateSequence<TCoordinate> pts)
        {
            Double minSegLen = Double.MaxValue;
            foreach (Pair<TCoordinate> pt in Slice.GetOverlappingPairs(pts))
            {
                Double segLen = pt.First.Distance(pt.Second);
                if (segLen < minSegLen)
                    minSegLen = segLen;
            }
            return minSegLen;
        }

        /**
         * Snaps the vertices in the component {@link LineString}s
         * of the source geometry
         * to the vertices of the given snap geometry.
         *
         * @param snapGeom a geometry to snap the source to
         * @return a new snapped Geometry
         */
        public IGeometry<TCoordinate> SnapTo(IGeometry<TCoordinate> snapGeom, Double snapTolerance)
        {
            ICoordinateSequence<TCoordinate> snapPts = ExtractTargetCoordinates(snapGeom);

            SnapTransformer snapTrans = new SnapTransformer(snapTolerance, snapPts);
            return snapTrans.Transform(srcGeom);
        }

        public ICoordinateSequence<TCoordinate> ExtractTargetCoordinates(IGeometry<TCoordinate> g)
        {
            return g.Coordinates.WithoutRepeatedPoints();
            //// TODO: should do this more efficiently.  Use CoordSeq filter to get points, KDTree for uniqueness & queries
            //Set ptSet = new TreeSet();
            //Coordinate[] pts = g.getCoordinates();
            //for (int i = 0; i < pts.length; i++)
            //{
            //    ptSet.add(pts[i]);
            //}
            //return (Coordinate[]) ptSet.toArray(new Coordinate[0]);
        }

        private class SnapTransformer : GeometryTransformer<TCoordinate>
        {
            Double _snapTolerance;
            ICoordinateSequence<TCoordinate> _snapPts;

            public SnapTransformer(Double snapTolerance, ICoordinateSequence<TCoordinate> snapPts)
            {
                _snapTolerance = snapTolerance;
                _snapPts = snapPts;
            }

            protected override ICoordinateSequence<TCoordinate> TransformCoordinates(ICoordinateSequence<TCoordinate> coords, IGeometry<TCoordinate> parent)
            {
                ICoordinateSequence<TCoordinate> srcPts = coords;
                ICoordinateSequence<TCoordinate> newPts = SnapLine(srcPts, _snapPts);
                return newPts;
            }

            private ICoordinateSequence<TCoordinate> SnapLine(ICoordinateSequence<TCoordinate> srcPts, ICoordinateSequence<TCoordinate> snapPts)
            {
                LineStringSnapper<TCoordinate> snapper = new LineStringSnapper<TCoordinate>(srcPts, _snapTolerance);
                return snapper.SnapTo(_snapPts);
            }
        }

    }
}
