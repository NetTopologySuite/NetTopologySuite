using System;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries.Prepared;

namespace Open.Topology.TestRunner.Operations
{
    public class PreparedGeometryTeeOperation : TeeGeometryOperation
    {
        private static bool ContainsProperly(IGeometry g1, IGeometry g2)
        {
            return g1.Relate(g2, "T**FF*FF*");
        }

        /*
        public PreparedGeometryTeeOperation()
        {
        }
         */

        /// <summary>
        /// Creates a new operation which chains to the given <see cref="IGeometryOperation"/>
        /// for non-intercepted methods.
        /// </summary>
        /// <param name="chainOp">The operation to chain to</param>
        public PreparedGeometryTeeOperation(GeometryMethodOperation chainOp)
            : base(chainOp)
        {
        }

        protected override void RunTeeOp(String opName, IGeometry geometry, Object[] args)
        {
            if (args.Length < 1) return;
            var g2 = args[0] as IGeometry;
            if (g2 == null) return;

            if (!geometry.IsValid)
                throw new InvalidOperationException("Input geometry A is not valid");
            if (!g2.IsValid)
                throw new InvalidOperationException("Input geometry B is not valid");

            CheckAllPrepOps(geometry, g2);
            CheckAllPrepOps(g2, geometry);
        }

        private static void CheckAllPrepOps(IGeometry g1, IGeometry g2)
        {
            var prepGeom = PreparedGeometryFactory.Prepare(g1);

            CheckIntersects(prepGeom, g2);
            CheckContains(prepGeom, g2);
            CheckContainsProperly(prepGeom, g2);
            CheckCovers(prepGeom, g2);
        }

        private static void CheckIntersects(IPreparedGeometry pg, IGeometry g2)
        {
            bool pgResult = pg.Intersects(g2);
            bool expected = pg.Geometry.Intersects(g2);

            if (pgResult != expected)
            {
                //			pg.intersects(g2);
                throw new InvalidOperationException("PreparedGeometry.intersects result does not match expected");
            }

            //		System.out.println("Results match!");
        }

        private static void CheckContains(IPreparedGeometry pg, IGeometry g2)
        {
            var pgResult = pg.Contains(g2);
            var expected = pg.Geometry.Contains(g2);

            if (pgResult != expected)
                throw new InvalidOperationException("PreparedGeometry.contains result does not match expected");

            //		System.out.println("Results match!");
        }

        private static void CheckContainsProperly(IPreparedGeometry pg, IGeometry g2)
        {
            var pgResult = pg.ContainsProperly(g2);
            var expected = ContainsProperly(pg.Geometry, g2);

            if (pgResult != expected)
                throw new InvalidOperationException("PreparedGeometry.containsProperly result does not match expected");

            //		System.out.println("Results match!");
        }

        private static void CheckCovers(IPreparedGeometry pg, IGeometry g2)
        {
            var pgResult = pg.Covers(g2);
            var expected = pg.Geometry.Covers(g2);

            if (pgResult != expected)
                throw new InvalidOperationException("PreparedGeometry.covers result does not match expected");

            //		System.out.println("Results match!");
        }

        /*
        static class PreparedGeometryOp
        {
                public static bool Intersects(IGeometry g1, IGeometry g2)
                {
                    var prepGeom = PreparedGeometryFactory.Prepare(g1);
                    return prepGeom.Intersects(g2);
                }

                public static bool Contains(IGeometry g1, IGeometry g2)
                {
                    var prepGeom = PreparedGeometryFactory.Prepare(g1);
                    return prepGeom.Contains(g2);
                }

                public static bool ContainsProperly(IGeometry g1, IGeometry g2)
                {
                    var prepGeom = PreparedGeometryFactory.Prepare(g1);
                    return prepGeom.ContainsProperly(g2);
                }

                public static bool Covers(IGeometry g1, IGeometry g2)
                {
                    var prepGeom = PreparedGeometryFactory.Prepare(g1);
                    return prepGeom.Covers(g2);
                }
        }
        */
    }
}