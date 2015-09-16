using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using NetTopologySuite.Precision;

namespace NetTopologySuite.SnapRound
{
    public static class SnapRoundFunctions
    {
        /// <summary>Reduces precision pointwise, then snap-rounds.
        /// <para/>
        /// Note that output set may not contain non-unique linework
        /// (and thus cannot be used as input to Polygonizer directly).
        /// <c>UnaryUnion</c> is one way to make the linework unique.
        /// </summary>
        /// <param name="geom">A Geometry containing linework to node</param>
        /// <param name="scaleFactor">The precision model scale factor to use</param>
        /// <returns>The noded, snap-rounded linework</returns>
        public static IGeometry SnapRoundLines(
            IGeometry geom, double scaleFactor)
        {
            IPrecisionModel pm = new PrecisionModel(scaleFactor);

            var roundedGeom = GeometryPrecisionReducer.ReducePointwise(geom, pm);

            var geomList = new List<IGeometry>();
            geomList.Add(roundedGeom);

            var noder = new GeometrySnapRounder(pm);
            var lines = noder.Node(geomList);

            return FunctionsUtil.GetFactoryOrDefault(geom).BuildGeometry(lines);
        }

        public static IGeometry SnapRound(
            IGeometry geomA, IGeometry geomB,
            double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var roundedGeomA = GeometryPrecisionReducer.ReducePointwise(geomA, pm);
            var geomRound = roundedGeomA;

            if (geomB != null)
            {
                var roundedGeomB = GeometryPrecisionReducer.ReducePointwise(geomB, pm);
                geomRound = geomA.Factory.CreateGeometryCollection(new [] { roundedGeomA, roundedGeomB });
            }

            var noder = new GeometrySnapRounder(pm);
            IGeometry snapped = noder.Node(geomRound);

            return snapped;
        }


    }
}