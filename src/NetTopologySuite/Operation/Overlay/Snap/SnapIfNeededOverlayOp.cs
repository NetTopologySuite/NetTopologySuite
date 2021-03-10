using System;
using System.Runtime.ExceptionServices;
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
        public static Geometry Overlay(Geometry g0, Geometry g1, SpatialFunction opCode)
        {
            var op = new SnapIfNeededOverlayOp(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        public static Geometry Intersection(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Intersection);
        }

        public static Geometry Union(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Union);
        }

        public static Geometry Difference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.Difference);
        }

        public static Geometry SymDifference(Geometry g0, Geometry g1)
        {
            return Overlay(g0, g1, SpatialFunction.SymDifference);
        }

        private readonly Geometry[] _geom = new Geometry[2];

        public SnapIfNeededOverlayOp(Geometry g1, Geometry g2)
        {
            _geom[0] = g1;
            _geom[1] = g2;
        }

        public Geometry GetResultGeometry(SpatialFunction opCode)
        {
            Geometry result = null;
            bool isSuccess = false;
            ExceptionDispatchInfo savedException = null;
            try
            {
                // try basic operation with input geometries
                result = OverlayOp.Overlay(_geom[0], _geom[1], opCode);
                //bool isValid = true;
                // not needed if noding validation is used
                //      boolean isValid = OverlayResultValidator.isValid(geom[0], geom[1], OverlayOp.INTERSECTION, result);
                // if (isValid)
                isSuccess = true;
            }
            catch (Exception ex)
            {
                savedException = ExceptionDispatchInfo.Capture(ex);
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
                    savedException.Throw();
                }
            }
            return result;
        }
    }
}
