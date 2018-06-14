using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Overlay.Snap
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
        public static IGeometry Overlay(IGeometry g0, IGeometry g1, SpatialFunction opCode)
        {
            var op = new SnapIfNeededOverlayOp(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        public static IGeometry Intersection(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        public static IGeometry Union(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Union);
        }

        public static IGeometry Difference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Difference);
        }

        public static IGeometry SymDifference(IGeometry g0, IGeometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.SymDifference);
        }

        private readonly IGeometry[] _geom = new IGeometry[2];

        public SnapIfNeededOverlayOp(IGeometry g1, IGeometry g2)
        {
            _geom[0] = g1;
            _geom[1] = g2;
        }

        public IGeometry GetResultGeometry(SpatialFunction opCode)
        {
            IGeometry result = null;
            bool isSuccess = false;
            Exception savedException = null;
            try
            {
                // try basic operation with input geometries
                result = OverlayOp.Overlay(_geom[0], _geom[1], opCode);
                bool isValid = true;
                // not needed if noding validation is used
                //      boolean isValid = OverlayResultValidator.isValid(geom[0], geom[1], OverlayOp.INTERSECTION, result);
                // if (isValid)
                isSuccess = true;
            }
            catch (Exception ex)
            {
                savedException = ex;
                // Ignore this exception, since the operation will be rerun
            }
            if (!isSuccess)
            {
                // this may still throw an exception
                // if so, throw the original exception since it has the input coordinates
                try
                {
                    result = SnapOverlayOp.Overlay(_geom[0], _geom[1], opCode);
                }
                catch (Exception)
                {
                    throw savedException;
                }
            }
            return result;
        }
    }
}