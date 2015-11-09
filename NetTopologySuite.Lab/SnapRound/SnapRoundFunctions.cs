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
            var pm = new PrecisionModel(scaleFactor);
            var gsr = new GeometrySnapRounder(pm);
            gsr.LineworkOnly =true;
            var snapped = gsr.Execute(geom);
            return snapped;
        }

        public static IGeometry SnapRound(
            IGeometry geomA, IGeometry geomB,
            double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var geom = geomA;

            if (geomB != null)
            {
                geom = geomA.Factory.CreateGeometryCollection(new IGeometry[] { geomA, geomB });
            }

            GeometrySnapRounder gsr = new GeometrySnapRounder(pm);
            var snapped = gsr.Execute(geom);
            return snapped;
        }


    }
}