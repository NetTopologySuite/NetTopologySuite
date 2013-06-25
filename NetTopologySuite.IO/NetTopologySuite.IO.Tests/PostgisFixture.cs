namespace NetTopologySuite.IO.Tests
{
    using System.Configuration;
    using GeoAPI.Geometries;
    using Npgsql;
    using NpgsqlTypes;

    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class PostgisFixture : AbstractIOFixture
    {
        protected override void AddAppConfigSpecificItems(KeyValueConfigurationCollection kvcc)
        {
            // NOTE: insert a valid connection string to a postgis db
            if (kvcc["PostGisConnectionString"] == null)
                kvcc.Add("PostGisConnectionString", "Server=ivv-sqlt3.ivv-aachen.de;Port=5432;Database=obe;Integrated Security=SSPI;");            
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            this.ConnectionString = (string)asr.GetValue("PostGisConnectionString", typeof(string));
        }

        private string PostGisVersion()
        {
            using (var conn = new NpgsqlConnection(this.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT postgis_version();";
                    var res = cmd.ExecuteScalar();
                    return res.ToString();
                }
            }
        }

        protected override void CreateTestStore()
        {
            using (var conn = new NpgsqlConnection(this.ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    var version = PostGisVersion();
                    if (version.StartsWith("1."))
                    {
                        cmd.CommandText = "DELETE FROM \"geometry_columns\" WHERE \"f_table_name\" = 'nts_io_postgis_2d'; ";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "DROP TABLE IF EXISTS \"nts_io_postgis_2d\";";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = 
                        "CREATE TABLE \"nts_io_postgis_2d\" (id int primary key, wkt text);" 
                      + "SELECT AddGeometryColumn('nts_io_postgis_2d', 'the_geom', " + 4326 + ",'GEOMETRY', 2);"                        
                        ;
                    cmd.ExecuteNonQuery();
                }
            }
            RandomGeometryHelper.Ordinates = Ordinates.XY;
        }

        protected override IGeometry Read(byte[] b)
        {
            var pgReader = new PostGisReader(this.RandomGeometryHelper.Factory);
            return pgReader.Read(b);
        }

        protected override byte[] Write(IGeometry gIn)
        {
            PostGisWriter pgWriter = new PostGisWriter();
            byte[] b = pgWriter.Write(gIn);
            using (NpgsqlConnection cn = new NpgsqlConnection(this.ConnectionString))
            {
                cn.Open();
                using (NpgsqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO \"nts_io_postgis_2d\" VALUES(@P1, @P2, @P3);";
                    NpgsqlParameter p1 = new NpgsqlParameter("P1", NpgsqlDbType.Integer) {NpgsqlValue = this.Counter};
                    NpgsqlParameter p2 = new NpgsqlParameter("P2", NpgsqlDbType.Text) { NpgsqlValue = gIn.AsText() };
                    NpgsqlParameter p3 = new NpgsqlParameter("P3", NpgsqlDbType.Bytea) { NpgsqlValue = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }

            return b;
        }
    }
}