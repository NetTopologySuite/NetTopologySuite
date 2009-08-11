using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Precision;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    /**
     * Performs an overlay operation using snapping and enhanced precision
     * to improve the robustness of the result.
     * This class <i>always</i> uses snapping.  
     * This is less performant than the standard JTS overlay code, 
     * and may even introduce errors which were not present in the original data.
     * For this reason, this class should only be used 
     * if the standard overlay code fails to produce a correct result. 
     *  
     * @author Martin Davis
     * @version 1.7
     */
    ///<summary>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class SnapOverlayOp<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        public static IGeometry<TCoordinate> Overlay(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1, SpatialFunctions opCode)
        {
            SnapOverlayOp<TCoordinate> op = new SnapOverlayOp<TCoordinate>(g0, g1);
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


        private IGeometry<TCoordinate>[] _geom = new IGeometry<TCoordinate>[2];
        private Double _snapTolerance;

        public SnapOverlayOp(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2)
        {
            _geom[0] = g1;
            _geom[1] = g2;
            computeSnapTolerance();
        }

        private void computeSnapTolerance()
        {
            _snapTolerance = GeometrySnapper<TCoordinate>.ComputeOverlaySnapTolerance(_geom[0], _geom[1]);

            // System.out.println("Snap tol = " + snapTolerance);
        }

        public IGeometry<TCoordinate> GetResultGeometry(SpatialFunctions opCode)
        {
            IGeometry<TCoordinate>[] prepGeom = Snap();
            IGeometry<TCoordinate> result = OverlayOp<TCoordinate>.Overlay(prepGeom[0], prepGeom[1], opCode);
            return PrepareResult(result);
        }

        private IGeometry<TCoordinate>[] Snap()
        {
            IGeometry<TCoordinate>[] remGeom = RemoveCommonBits(_geom);

            // MD - testing only
            //  	IGeometry<TCoordinate>[] remGeom = geom;

            IGeometry<TCoordinate>[] snapGeom = GeometrySnapper<TCoordinate>.Snap(remGeom[0], remGeom[1], _snapTolerance);
            // MD - may want to do this at some point, but it adds cycles
            //    checkValid(snapGeom[0]);
            //    checkValid(snapGeom[1]);

            /*
            System.out.println("Snapped geoms: ");
            System.out.println(snapGeom[0]);
            System.out.println(snapGeom[1]);
            */
            return snapGeom;
        }

        private IGeometry<TCoordinate> PrepareResult(IGeometry<TCoordinate> geom)
        {
            return _cbr.AddCommonBits(geom);
        }


        private CommonBitsRemover<TCoordinate> _cbr;

        private IGeometry<TCoordinate>[] RemoveCommonBits(IGeometry<TCoordinate>[] geom)
        {
            _cbr = new CommonBitsRemover<TCoordinate>(geom[0].Factory);
            _cbr.Add(geom[0]);
            _cbr.Add(geom[1]);
            IGeometry<TCoordinate>[] remGeom = {
                _cbr.RemoveCommonBits(geom[0].Clone()),
                _cbr.RemoveCommonBits(geom[1].Clone()) };

            return remGeom;
        }

        private void checkValid(IGeometry<TCoordinate> g)
        {
            if (! g.IsValid) {
                System.Diagnostics.Trace.WriteLine("Snapped geometry is invalid");
        }
        }
    }
}
