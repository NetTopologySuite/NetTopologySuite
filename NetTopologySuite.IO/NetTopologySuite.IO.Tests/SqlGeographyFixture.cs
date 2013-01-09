using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Tests
{
    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public abstract class SqlServer2008Fixture : AbstractIOFixture
    {
        protected SqlServer2008Fixture()
        {
        }

        protected SqlServer2008Fixture(IGeometryFactory geometryFactory)
            :base(geometryFactory)
        {
        }

        #region Overrides of AbstractIOFixture

        protected override void AddAppConfigSpecificItems(KeyValueConfigurationCollection kvcc)
        {
            // NOTE: insert a valid connection string to a SqlServer 2008 db
            try
            {
                if (kvcc["SqlServer2008ConnectionString"] == null) 
                    kvcc.Add("SqlServer2008ConnectionString", "Data Source=localhost\\SQLEXPRESS;Database=NTSTests;Integrated Security=SSPI;");
            }
            catch
            {
                kvcc.Add("SqlServer2008ConnectionString", "Data Source=localhost\\SQLEXPRESS;Database=NTSTests;Integrated Security=SSPI;");
            }
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            base.ReadAppConfigInternal(asr);
            this.ConnectionString = (string)asr.GetValue("SqlServer2008ConnectionString", typeof(string));
        }

        #endregion
    }

    //[Ignore("Need to come up with some random valid geography objects!")]
    public class SqlGeographyFixture : SqlServer2008Fixture
    {
        public SqlGeographyFixture()
            :base(new OgcCompliantGeometryFactory(new PrecisionModel(10000), 4326))
        {
        }
        #region Overrides of AbstractIOFixture

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            base.ReadAppConfigInternal(asr);
            RandomGeometryHelper.MinX = 50.1;
            RandomGeometryHelper.MaxX = 59.9;
            RandomGeometryHelper.MinY = 50.1;
            RandomGeometryHelper.MaxY = 59.9;

        }

        protected override void CreateTestStore()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using ( var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "IF OBJECT_ID('nts_io_geography') IS NOT NULL " +
                        "DROP TABLE [nts_io_geography];";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        "CREATE TABLE nts_io_geography (id int primary key, wkt text, the_geog geography, wkt2 AS the_geog.STAsText());";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override IGeometry Read(byte[] b)
        {
            var geoReader = new MsSql2008GeographyReader() /*{Factory = RandomGeometryHelper.Factory}*/;
            return geoReader.Read(b);
        }

        protected override void CheckEquality(IGeometry gIn, IGeometry gParsed, WKTWriter writer)
        {
            if (gIn.OgcGeometryType == OgcGeometryType.GeometryCollection)
                Assert.IsTrue(gIn.EqualsNormalized(gParsed));
            else
                Assert.IsTrue(gIn.EqualsTopologically(gParsed));
        }

        protected override byte[] Write(IGeometry gIn)
        {
            var geoWriter = new MsSql2008GeographyWriter();
            var b = geoWriter.WriteGeography(gIn);
            var b2 = geoWriter.Write(gIn);

            using( var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO [nts_io_geography] VALUES(@P1, @P2, @P3);";
                    var p1 = new SqlParameter("P1", SqlDbType.Int) { SqlValue = Counter };
                    var p2 = new SqlParameter("P2", SqlDbType.Text) { SqlValue = gIn.AsText() };
                    var p3 = new SqlParameter("P3", SqlDbType.Udt) { UdtTypeName = "geography", SqlValue = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }

            return b2;
        }

        [Test, Ignore]
        public override void TestGeometryCollection()
        {
            base.TestGeometryCollection();
        }

        [Test, Ignore]
        public override void TestMultiPolygon()
        {
            base.TestMultiPolygon();
        }

        [Test, Ignore]
        public override void TestPolygon()
        {
            base.TestPolygon();
        }

        #endregion
    }
}