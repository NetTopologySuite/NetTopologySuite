using System;
using System.Collections.Generic;
using System.Data;
using DotSpatial.Projections;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using NUnit.Framework;
using Npgsql;
using NpgsqlTypes;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.UnitTests
{
    public class ComparisonTest
    {
        private static readonly Random Rnd = new Random();
        private static readonly CoordinateSystemFactory Factory = new CoordinateSystemFactory();
        private static readonly CoordinateTransformationFactory TransformationFactory = new CoordinateTransformationFactory();

        [Test, Ignore]
        public void TestRandomWgs84Points()
        {
            var succededProjNet = 0;
            var failedProjNet = 0;
            var exceptedProjNet = 0;
            var succededDotSpatial = 0;
            var failedDotSpatial = 0;
            var exceptedDotSpatial = 0;

            var cnProj4 = new NpgsqlConnection(Properties.Resources.PgConnectionString);
            var cnSource = new NpgsqlConnection(Properties.Resources.PgConnectionString);
            var cnTarget = new NpgsqlConnection(Properties.Resources.PgConnectionString);
            
                cnProj4.Open();
                cnSource.Open();
                cnTarget.Open();

                using (var cmdProj4 = cnProj4.CreateCommand())
                using (var cmdSource = cnSource.CreateCommand())
                using (var cmdTarget = cnTarget.CreateCommand())
                {
                    cmdProj4.CommandText =
                        "SELECT st_x(tp.point), st_y(tp.point) FROM (SELECT st_transform(st_setsrid(st_makepoint(@px, @py), @psrid), @ptsrid) as point) AS tp;";
                    
                    var p4p = cmdProj4.Parameters;
                    p4p.AddRange(
                        new[]
                            {
                                new NpgsqlParameter("@px", DbType.Double),
                                new NpgsqlParameter("@py", DbType.Double),
                                new NpgsqlParameter("@psrid", DbType.Int32),
                                new NpgsqlParameter("@ptsrid", DbType.Int32),
                            });

                    cmdSource.CommandText = "SELECT \"srid\", \"srtext\", \"proj4text\" FROM \"spatial_ref_sys\";";

                    cmdTarget.CommandText =
                        "SELECT \"srid\", \"srtext\", \"proj4text\" FROM \"spatial_ref_sys\" WHERE \"srid\">@psrid;";
                    cmdTarget.Parameters.Add("@psrid", NpgsqlDbType.Integer);

                    using (var sourceReader = cmdSource.ExecuteReader())
                    {
                        while (sourceReader.Read())
                        {
                            var srid = sourceReader.GetInt32(0);
                            cmdTarget.Parameters[0].Value = cmdProj4.Parameters[2].Value = srid;

                            ICoordinateSystem projNetSource;
                            try
                            {
                                projNetSource =
                                    Factory.CreateFromWkt(sourceReader.GetString(1));
                            }
                            catch
                            {
                                projNetSource = null;
                            }

                            ProjectionInfo DSProjSource;
                            try
                            {
                                DSProjSource =
                                    ProjectionInfo.FromProj4String(sourceReader.GetString(2));
                            }
                            catch
                            {
                                DSProjSource = null;
                            }

                            using (var targetReader = cmdTarget.ExecuteReader(CommandBehavior.Default))
                            {
                                while (targetReader.Read())
                                {
                                    var targetSrid = targetReader.GetInt32(0);

                                    ICoordinateSystem projNetTarget = null;
                                    if (projNetSource != null)
                                    {
                                        try
                                        {
                                            projNetTarget =
                                                Factory.CreateFromWkt(targetReader.GetString(1));
                                        }
                                        catch
                                        {
                                            projNetTarget = null;
                                        }
                                    }

                                    ProjectionInfo DSProjTarget = null;
                                    if (DSProjSource != null)
                                    {
                                        try
                                        {
                                            DSProjTarget =
                                                ProjectionInfo.FromProj4String(targetReader.GetString(2));
                                        }
                                        catch
                                        {
                                            DSProjTarget = null;
                                        }
                                    }

                                    foreach (double[] randomOrdinate in GetRandomOrdinates())
                                    {
                                        //Get source coordinates
                                        p4p[0].Value = randomOrdinate[0];
                                        p4p[1].Value = randomOrdinate[1];
                                        p4p[2].Value = 4326;
                                        p4p[3].Value = srid;

                                        try
                                        {
                                            using (var proj4Reader = cmdProj4.ExecuteReader(CommandBehavior.SingleRow))
                                            {
                                                proj4Reader.Read();
                                                randomOrdinate[0] = proj4Reader.GetDouble(0);
                                                randomOrdinate[1] = proj4Reader.GetDouble(1);
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }

                                        p4p[0].Value = randomOrdinate[0];
                                        p4p[1].Value = randomOrdinate[1];
                                        p4p[2].Value = srid;
                                        p4p[3].Value = targetSrid;

                                        var result = new double[2];
                                        try
                                        {
                                            using (var proj4Reader = cmdProj4.ExecuteReader(CommandBehavior.SingleRow))
                                            {
                                                proj4Reader.Read();
                                                result[0] = proj4Reader.GetDouble(0);
                                                result[1] = proj4Reader.GetDouble(1);
                                            }
                                        }
                                        catch
                                        {
                                            continue;
                                        }


                                        if (projNetSource != null && projNetTarget != null)
                                        {
                                            try
                                            {
                                                var ts = TransformationFactory.CreateFromCoordinateSystems(
                                                    projNetSource,
                                                    projNetTarget);
                                                var projNetResult = TestForwardAndBackProjNet(ts.MathTransform,
                                                                                              randomOrdinate,
                                                                                              ref succededProjNet,
                                                                                              ref failedProjNet,
                                                                                              ref exceptedProjNet);

                                                var dx = projNetResult[0] - result[0];
                                                var dy = projNetResult[1] - result[1];
                                                if (Math.Abs(dx) > 1d || Math.Abs(dy) > 1d)
                                                {
                                                    failedProjNet++;
                                                    Console.WriteLine(
                                                        string.Format(
                                                            "Failed ProjNet    {0} -> {1} for ({2}, {3}). [{4}, {5}]",
                                                            srid, targetSrid,
                                                            randomOrdinate[0], randomOrdinate[1],
                                                            dx, dy));
                                                }
                                            }
                                            catch (Exception)
                                            {
                                                exceptedProjNet++;
                                            }
                                        }
                                        else
                                            exceptedProjNet++;


                                        if (DSProjSource != null && DSProjTarget != null)
                                        {
                                            var dsResult = TestForwardAndBackDotSpatial(DSProjSource, DSProjTarget,
                                                                                        randomOrdinate,
                                                                                        ref succededDotSpatial,
                                                                                        ref failedDotSpatial,
                                                                                        ref exceptedDotSpatial);
                                            var dx = dsResult[0] - result[0];
                                            var dy = dsResult[1] - result[1];
                                            if (Math.Abs(dx) > 1d ||
                                                Math.Abs(dy) > 1d)
                                            {
                                                failedProjNet++;
                                                Console.WriteLine(
                                                    string.Format(
                                                        "Failed DotSpatial {0} -> {1} for ({2}, {3}). [{4}, {5}]",
                                                        srid, targetSrid,
                                                        randomOrdinate[0], randomOrdinate[1],
                                                        dx, dy));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            cnTarget.Dispose();
            cnSource.Dispose();
            cnProj4.Dispose();
        }

        private static IEnumerable<double[]> GetRandomOrdinates()
        {
            yield return new[] { -180 + 180 * Rnd.NextDouble(), 85 * Rnd.NextDouble() };
            yield return new[] { 180 * Rnd.NextDouble(), 85 * Rnd.NextDouble() };
            yield return new[] { -180 + 180 * Rnd.NextDouble(), -85 + 85 * Rnd.NextDouble() };
            yield return new[] { 180 * Rnd.NextDouble(), -85 + 85 * Rnd.NextDouble() };
        }

        private const double Tolerance = 1e-2;
        
        private static double[] TestForwardAndBack(IMathTransform transform, double[] ordinates,
            ref int succeed, ref int failed, ref int exception)
        {
            try
            {
                var forward = transform.Transform(ordinates);

                transform.Invert();
                var back = transform.Transform(ordinates);
                transform.Invert();

                if (Math.Abs(ordinates[0] - back[0]) <= Tolerance &&
                    Math.Abs(ordinates[1] - back[1]) <= Tolerance)
                {
                    succeed++;
                }
                else
                {
                    failed++;
                }

                return forward;

            }
            catch (Exception)
            {
                exception++;
            }
            return null;
        }

        private static double[] TestForwardAndBackProjNet(IMathTransform transform, double[] ordinates,
            ref int succeed, ref int failed, ref int exception)
        {
            try
            {
                var forward = transform.Transform(ordinates);

                transform.Invert();
                var back = transform.Transform(ordinates);
                transform.Invert();

                if (Math.Abs(ordinates[0] - back[0]) <= Tolerance &&
                    Math.Abs(ordinates[1] - back[1]) <= Tolerance)
                {
                    succeed++;
                }
                else
                {
                    failed++;
                }

                return forward;
            }
            catch
            {
                exception++;
            }
            return ordinates;
        }

        private static double[] TestForwardAndBackDotSpatial(
            ProjectionInfo source, ProjectionInfo target, 
            double[] ordinates,ref int succeed, ref int failed, ref int exception)
        {
            try
            {
            var forward = new double[2];
            Buffer.BlockCopy(ordinates, 0, forward, 0, 16);
            Reproject.ReprojectPoints(forward, null, source, target, 0, 1);

            var back = new double[2];
            Buffer.BlockCopy(forward, 0, back, 0, 16);
            Reproject.ReprojectPoints(back, null, target, source, 0, 1);

                if (Math.Abs(ordinates[0] - back[0]) <= Tolerance &&
                    Math.Abs(ordinates[1] - back[1]) <= Tolerance)
                {
                    succeed++;
                }
                else
                {
                    failed++;
                }

                return forward;
            }
            catch
            {
                exception++;
            }
            return ordinates;
        }

    }
}