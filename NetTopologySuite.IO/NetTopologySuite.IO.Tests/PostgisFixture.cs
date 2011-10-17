using System.Configuration;
using GeoAPI.Geometries;
using NUnit.Framework;
using Npgsql;
using NpgsqlTypes;

namespace NetTopologySuite.IO.Tests
{
    [Ignore("Currently all tests fail. Need to investigate what has changed!")]
    public class PostgisFixture : AbstractIOFixture
    {
        protected override bool CreateAppConfigInternal(KeyValueConfigurationCollection kvcc)
        {
            kvcc.Add("PostGisConnectionString", "... your value goes here ...");
            return true;
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            ConnectionString = (string)asr.GetValue("PostGisConnectionString", typeof(string));
        }

        protected override void CreateTestStore()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "DELETE FROM \"geometry_columns\" WHERE \"f_table_name\" = 'nts_io_postgis_2d'; " +
                        "DROP TABLE IF EXISTS \"nts_io_postgis_2d\";"
                        ;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText =
                         "CREATE TABLE \"nts_io_postgis_2d\" (id int primary key, wkt text);" +
                         "SELECT AddGeometryColumn('nts_io_postgis_2d', 'the_geom', " + SRID + ",'GEOMETRY', 2);";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override IGeometry Read(byte[] b)
        {
            var pgReader = new PostGisReader(RandomGeometryHelper.Factory);
            return pgReader.Read(b);
        }

        protected override byte[] Write(IGeometry gIn)
        {
            var pgWriter = new PostGisWriter();
            var b = pgWriter.Write(gIn);
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO \"nts_io_postgis_2d\" VALUES(@P1, @P3, @P2);";
                    var p1 = new NpgsqlParameter("P1", NpgsqlDbType.Integer) {NpgsqlValue = Counter};
                    var p2 = new NpgsqlParameter("P2", NpgsqlDbType.Text) { NpgsqlValue = gIn.AsText() };
                    var p3 = new NpgsqlParameter("P3", NpgsqlDbType.Bytea) { NpgsqlValue = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }

            return b;

        }
    }
}