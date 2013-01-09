using System.Data;
using System.Data.SqlClient;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Tests
{
    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class SqlGeometryFixture : SqlServer2008Fixture
    {
        #region Overrides of AbstractIOFixture

        protected override void CreateTestStore()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using ( var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "IF OBJECT_ID('nts_io_geometry') IS NOT NULL " +
                        "DROP TABLE [nts_io_geometry];";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
                        "CREATE TABLE nts_io_geometry (id int primary key, wkt text, the_geom geometry, wkt2 as the_geom.STAsText());";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override IGeometry Read(byte[] b)
        {
            var geoReader = new MsSql2008GeometryReader {Factory = RandomGeometryHelper.Factory};
            return geoReader.Read(b);
        }

        protected override byte[] Write(IGeometry gIn)
        {
            var geoWriter = new MsSql2008GeometryWriter();
            var b = geoWriter.WriteGeometry(gIn);
            var b2 = geoWriter.Write(gIn);
            using( var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO [nts_io_geometry] VALUES(@P1, @P2, @P3);";
                    var p1 = new SqlParameter("P1", SqlDbType.Int) { SqlValue = Counter };
                    var p2 = new SqlParameter("P2", SqlDbType.Text) { SqlValue = gIn.AsText() };
                    var p3 = new SqlParameter("P3", SqlDbType.Udt) { UdtTypeName = "geometry", SqlValue = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();

                    /*
                    p1.SqlValue = 100000 + Counter;
                    cmd.Parameters.Remove(p3);
                    p3 = new SqlParameter("P3", SqlDbType.Image) { SqlValue = b };
                    cmd.Parameters.Add(p3);
                    p3.SqlValue = b2;
                    cmd.ExecuteNonQuery();
                     */
                }

            }
            return b2;
        }

        #endregion
    }
}