using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Precision;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
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
        public static IGeometry Overlay(IGeometry g0, IGeometry g1, SpatialFunction opCode)
        {
            SnapOverlayOp op = new SnapOverlayOp(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry Intersection(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry Union(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Union);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry Difference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Difference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry SymDifference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.SymDifference);
        }


        private IGeometry[] geom = new IGeometry[2];
        private double tolerance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public SnapOverlayOp(IGeometry g1, IGeometry g2)
        {
            geom[0] = g1;
            geom[1] = g2;
            ComputeSnapTolerance();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ComputeSnapTolerance()
        {
            tolerance = GeometrySnapper.ComputeOverlaySnapTolerance(geom[0], geom[1]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public IGeometry GetResultGeometry(SpatialFunction opCode)
        {
            IGeometry[] prepGeom = Snap();
            IGeometry result = OverlayOp.Overlay(prepGeom[0], prepGeom[1], opCode);
            return PrepareResult(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IGeometry[] Snap()
        {
            IGeometry[] remGeom = RemoveCommonBits(geom);

            // MD - testing only
            // IGeometry[] remGeom = geom;

            IGeometry[] snapGeom = GeometrySnapper.Snap(remGeom[0], remGeom[1], tolerance);
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
        private IGeometry PrepareResult(IGeometry geom)
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
        private IGeometry[] RemoveCommonBits(IGeometry[] geom)
        {
            cbr = new CommonBitsRemover();
            cbr.Add(geom[0]);
            cbr.Add(geom[1]);
            IGeometry[] remGeom = new IGeometry[2];
            remGeom[0] = cbr.RemoveCommonBits((IGeometry) geom[0].Clone());
            remGeom[1] = cbr.RemoveCommonBits((IGeometry) geom[1].Clone());
            return remGeom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"></param>
        private void CheckValid(IGeometry g)
        {
  	        if (! g.IsValid) 
  		        Trace.WriteLine("Snapped geometry is invalid");
          }
    }
}
