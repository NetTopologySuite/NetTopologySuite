using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;

namespace NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Performs an overlay operation using snapping and enhanced precision
    /// to improve the robustness of the result.
    /// This class always uses snapping.
    /// This is less performant than the standard JTS overlay code,
    /// and may even introduce errors which were not present in the original data.
    /// For this reason, this class should only be used
    /// if the standard overlay code fails to produce a correct result.
    /// </summary>
    public class SnapOverlayOp
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public static Geometry Overlay(Geometry g0, Geometry g1, SpatialFunction opCode)
        {
            var op = new SnapOverlayOp(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static Geometry Intersection(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static Geometry Union(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Union);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static Geometry Difference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Difference);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static Geometry SymDifference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.SymDifference);
        }

        private readonly Geometry[] _geom = new Geometry[2];
        private double _snapTolerance;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public SnapOverlayOp(Geometry g1, Geometry g2)
        {
            _geom[0] = g1;
            _geom[1] = g2;
            ComputeSnapTolerance();
        }

        /// <summary>
        ///
        /// </summary>
        private void ComputeSnapTolerance()
        {
            _snapTolerance = GeometrySnapper.ComputeOverlaySnapTolerance(_geom[0], _geom[1]);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public Geometry GetResultGeometry(SpatialFunction opCode)
        {
            var prepGeom = Snap(_geom);
            var result = OverlayOp.Overlay(prepGeom[0], prepGeom[1], opCode);
            return PrepareResult(result);
        }

        private Geometry SelfSnap(Geometry geom)
        {
            var snapper0 = new GeometrySnapper(geom);
            var snapGeom = snapper0.SnapTo(geom, _snapTolerance);
            //System.out.println("Self-snapped: " + snapGeom);
            //System.out.println();
            return snapGeom;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private Geometry[] Snap(Geometry[] geom)
        {
            var remGeom = RemoveCommonBits(geom);

            // MD - testing only
            // Geometry[] remGeom = geom;

            var snapGeom = GeometrySnapper.Snap(remGeom[0], remGeom[1], _snapTolerance);
            // MD - may want to do this at some point, but it adds cycles
            // CheckValid(snapGeom[0]);
            // CheckValid(snapGeom[1]);
            return snapGeom;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        private Geometry PrepareResult(Geometry geom)
        {
            cbr.AddCommonBits(geom);
            return geom;
        }

        private CommonBitsRemover cbr;

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        private Geometry[] RemoveCommonBits(Geometry[] geom)
        {
            cbr = new CommonBitsRemover();
            cbr.Add(geom[0]);
            cbr.Add(geom[1]);
            var remGeom = new Geometry[2];
            remGeom[0] = cbr.RemoveCommonBits((Geometry)geom[0].Copy());
            remGeom[1] = cbr.RemoveCommonBits((Geometry)geom[1].Copy());
            return remGeom;
        }

#if DEBUG
        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(Geometry g)
        {
            if (!g.IsValid)
                Debug.WriteLine("Snapped geometry is invalid");
        }
#endif
    }
}
