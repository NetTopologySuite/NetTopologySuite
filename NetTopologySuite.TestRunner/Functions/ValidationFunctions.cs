using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Operation.Valid;

namespace Open.Topology.TestRunner.Functions
{
    public class ValidationFunctions
    {
        /// <summary>
        /// Validates all geometries in a collection independently.
        /// Errors are returned as points at the invalid location
        /// </summary>
        /// <param name="g"></param>
        /// <returns>the invalid locations, if any</returns>
        public static IGeometry InvalidLocations(IGeometry g)
        {
            var invalidLoc = new List<IPoint>();
            for (var i = 0; i < g.NumGeometries; i++)
            {
                var geom = g.GetGeometryN(i);
                var ivop = new IsValidOp(geom);
                var err = ivop.ValidationError;
                if (err != null)
                {
                    invalidLoc.Add(g.Factory.CreatePoint(err.Coordinate));
                }
            }
            return g.Factory.BuildGeometry(invalidLoc.ToArray());
        }

        public static IGeometry InvalidGeoms(IGeometry g)
        {
            var invalidGeoms = new List<IGeometry>();
            for (var i = 0; i < g.NumGeometries; i++)
            {
                var geom = g.GetGeometryN(i);
                var ivop = new IsValidOp(geom);
                var err = ivop.ValidationError;
                if (err != null)
                {
                    invalidGeoms.Add(geom);
                }
            }
            return g.Factory.BuildGeometry(invalidGeoms);
        }
    }
}