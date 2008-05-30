using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Performs an overlay operation using snapping and enhanced precision
    /// to improve the robustness of the result.
    /// This class only uses snapping
    /// if an error is detected when running the standard JTS overlay code.
    /// Errors detected include thrown exceptions 
    /// (in particular, <see cref="TopologyException" />)
    /// and invalid overlay computations.
    /// </summary>
    public class SnapIfNeededOverlayOp
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
            SnapIfNeededOverlayOp op = new SnapIfNeededOverlayOp(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry intersection(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry union(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Union);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry difference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Difference);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g0"></param>
        /// <param name="g1"></param>
        /// <returns></returns>
        public static IGeometry symDifference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.SymDifference);
        }

        private IGeometry[] geom = new IGeometry[2];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        public SnapIfNeededOverlayOp(IGeometry g1, IGeometry g2)
        {
            geom[0] = g1;
            geom[1] = g2;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns></returns>
        public IGeometry GetResultGeometry(SpatialFunction opCode)
        {
            IGeometry result = null;
            bool isSuccess = false;
            try
            {
                result = OverlayOp.Overlay(geom[0], geom[1], opCode);
                bool isValid = true;
                // not needed if noding validation is used
                //      boolean isValid = OverlayResultValidator.isValid(geom[0], geom[1], OverlayOp.INTERSECTION, result);
                if (isValid)
                    isSuccess = true;

            }
            catch (Exception ex)
            {
                // Ignore this exception, since the operation will be rerun                
            }
            if (!isSuccess)
            {
                // This may still throw an exception - just let it go if it does
                result = SnapOverlayOp.Overlay(geom[0], geom[1], opCode);
            }
            return result;
        }
    }
}
