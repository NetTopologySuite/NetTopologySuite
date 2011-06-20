using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    public class SnapIfNeededOverlayOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static IGeometry<TCoordinate> Overlay(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, SpatialFunctions opCode)
        {
            SnapIfNeededOverlayOp<TCoordinate> op = new SnapIfNeededOverlayOp<TCoordinate>(g0, g1);
            return op.GetResultGeometry(opCode);
        }

        public static IGeometry<TCoordinate> Intersection(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return Overlay(g0, g1, SpatialFunctions.Intersection);
        }

        public static IGeometry<TCoordinate> Union(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return Overlay(g0, g1, SpatialFunctions.Union);
        }

        public static IGeometry<TCoordinate> Difference(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return Overlay(g0, g1, SpatialFunctions.Difference);
        }

        public static IGeometry<TCoordinate> SymDifference(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return Overlay(g0, g1, SpatialFunctions.SymDifference);
        }

        private IGeometry<TCoordinate>[] geom = new IGeometry<TCoordinate>[2];

        public SnapIfNeededOverlayOp(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2)
        {
            geom[0] = g1;
            geom[1] = g2;
        }

        public IGeometry<TCoordinate> GetResultGeometry(SpatialFunctions opCode)
        {
            IGeometry<TCoordinate> result = null;
            Boolean isSuccess = false;
            try
            {
                result = OverlayOp<TCoordinate>.Overlay(geom[0], geom[1], opCode);
                Boolean isValid = true;
                // not needed if noding validation is used
                //      boolean isValid = OverlayResultValidator.isValid(geom[0], geom[1], OverlayOp.INTERSECTION, result);
                if (isValid)
                    isSuccess = true;

            }
            catch (Exception ex)
            {
                // ignore this exception, since the operation will be rerun
                //    	System.out.println(ex.getMessage());
                //    	ex.printStackTrace();
            }
            if (!isSuccess)
            {
                // this may still throw an exception - just let it go if it does
                result = SnapOverlayOp<TCoordinate>.Overlay(geom[0], geom[1], opCode);
            }
            return result;
        }
    }
}
