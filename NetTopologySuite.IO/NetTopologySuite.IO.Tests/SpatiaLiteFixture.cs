using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO.Tests
{
    public class SpatiaLiteFixture : AbstractIOFixture
    {
        public bool HasZ { get { return (Ordinates & Ordinates.Z) == Ordinates.Z; } }
        public bool HasM { get { return (Ordinates & Ordinates.M) == Ordinates.M; } }
        public bool Compressed { get; set; }

        protected override bool CreateAppConfigInternal(KeyValueConfigurationCollection kvcc)
        {
            kvcc.Add("SpatiaLiteCompressed", "false");
            return false;
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            Compressed = (bool) asr.GetValue("SpatiaLiteCompressed", typeof (bool));
        }

        protected override void CreateTestStore()
        {
            if (File.Exists("SpatiaLite.sqlite"))
                File.Delete("SpatiaLite.sqlite");

            using (var conn = new SQLiteConnection("Data Source=\"SpatiaLite.sqlite\""))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
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
            var b = GaiaGeoWriter.Write(gIn);
            using (var conn = new SQLiteConnection("Data Source=\"SpatiaLite.sqlite\""))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO \"nts_io_spatialite\" VALUES(@P1, @P3, @P2);";
                    var p1 = new SQLiteParameter("P1", DbType.Int32) {Value = Counter};
                    var p2 = new SQLiteParameter("P2", DbType.String) { Value = gIn.AsText() };
                    var p3 = new SQLiteParameter("P3", DbType.Binary) { Value = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }
            return b;
        }
    }
}