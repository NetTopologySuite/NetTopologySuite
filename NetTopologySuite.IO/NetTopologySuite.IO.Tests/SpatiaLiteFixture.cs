namespace NetTopologySuite.IO.Tests
{
    using System.Configuration;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using GeoAPI.Geometries;

    public class SpatiaLiteFixture : AbstractIOFixture
    {
        public bool HasZ
        {
            get { return (this.Ordinates & Ordinates.Z) == Ordinates.Z; }
        }

        public bool HasM 
        { 
            get { return (this.Ordinates & Ordinates.M) == Ordinates.M; }
        }

        public bool Compressed { get; set; }

        protected override void AddAppConfigSpecificItems(KeyValueConfigurationCollection kvcc)
        {
            kvcc.Add("SpatiaLiteCompressed", "false");
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            this.Compressed = (bool)asr.GetValue("SpatiaLiteCompressed", typeof(bool));
        }

        protected override void CreateTestStore()
        {
            if (File.Exists("SpatiaLite.sqlite"))
                File.Delete("SpatiaLite.sqlite");

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=\"SpatiaLite.sqlite\""))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE \"nts_io_spatialite\" (id int primary key, wkt text, the_geom blob);";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override IGeometry Read(byte[] b)
        {
            return GaiaGeoReader.Read(b);
        }

        protected override byte[] Write(IGeometry gIn)
        {
            byte[] b = GaiaGeoWriter.Write(gIn);
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=\"SpatiaLite.sqlite\""))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO \"nts_io_spatialite\" VALUES(@P1, @P3, @P2);";
                    SQLiteParameter p1 = new SQLiteParameter("P1", DbType.Int32) { Value = this.Counter };
                    SQLiteParameter p2 = new SQLiteParameter("P2", DbType.String) { Value = gIn.AsText() };
                    SQLiteParameter p3 = new SQLiteParameter("P3", DbType.Binary) { Value = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }
            return b;
        }
    }
}