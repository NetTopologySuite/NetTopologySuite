using System;
using System.Data;
using NUnit.Framework;
using Npgsql;

namespace ProjNet.UnitTests.Converters.WKT
{
    [TestFixture]
    public class SpatialRefSysTableParser
    {
        private const string ConnectionString =
            "Host=localhost;Port=5432;Database=postgis2;uid=postgres;pwd=1.Kennwort";
        
        [Test, Ignore("Run only if you have a PostGis server and have corrected the ConnectionString")]
        public void Test()
        {
            
            using (var cn = new NpgsqlConnection(ConnectionString))
            {
                cn.Open();
                var cmd = cn.CreateCommand();
                cmd.CommandText = "SELECT \"srid\", \"srtext\" FROM \"public\".\"spatial_ref_sys\" ORDER BY \"srid\";";

                var counted = 0;
                var failed = 0;
                var tested = 0;
                using (var r = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (r != null)
                    {
                        while (r.Read())
                        {
                            counted++;
                            var srtext = r.GetString(1);
                            if (!string.IsNullOrEmpty(srtext))
                            {
                                tested++;
                                if (!TestParse(r.GetInt32(0), srtext))failed++;
                            }
                        }
                    }
                }

                Console.WriteLine("\n\nTotal number of Tests {0}, failed {1}", tested, failed);
                Assert.IsTrue(failed==0);
            }

        }

        private static bool TestParse(int srid, string srtext)
        {
            try
            {
                ProjNet.Converters.WellKnownText.CoordinateSystemWktReader.Parse(srtext);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Test {0} failed:\n  {1}\n  {2}", srid, srtext, ex.Message);
                return false;
            }
        }

        [Test]
        public void TestSrOrg()
        {
            Assert.IsTrue(TestParse(1,
                                    "PROJCS[\"WGS 84 / Pseudo-Mercator\",GEOGCS[\"Popular Visualisation CRS\",DATUM[\"Popular_Visualisation_Datum\",SPHEROID[\"Popular Visualisation Sphere\",6378137,0,AUTHORITY[\"EPSG\",\"7059\"]],TOWGS84[0,0,0,0,0,0,0],AUTHORITY[\"EPSG\",\"6055\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4055\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Mercator_1SP\"],PARAMETER[\"central_meridian\",0],PARAMETER[\"scale_factor\",1],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],AUTHORITY[\"EPSG\",\"3785\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]"));
        }

        [Test]
        public void TestProjNetIssues()
        {
            Assert.IsTrue(TestParse(1,
                    "PROJCS[\"International_Terrestrial_Reference_Frame_1992Lambert_Conformal_Conic_2SP\","+
                    "GEOGCS[\"GCS_International_Terrestrial_Reference_Frame_1992\","+
                    "DATUM[\"International_Terrestrial_Reference_Frame_1992\","+
                    "SPHEROID[\"GRS_1980\",6378137,298.257222101],"+
                    "TOWGS84[0,0,0,0,0,0,0]],"+
                    "PRIMEM[\"Greenwich\",0],"+
                    "UNIT[\"Degree\",0.0174532925199433]],"+
                    "PROJECTION[\"Lambert_Conformal_Conic_2SP\",AUTHORITY[\"EPSG\",\"9802\"]],"+
                    "PARAMETER[\"Central_Meridian\",-102],"+
                    "PARAMETER[\"Latitude_Of_Origin\",12],"+
                    "PARAMETER[\"False_Easting\",2500000],"+
                    "PARAMETER[\"False_Northing\",0],"+
                    "PARAMETER[\"Standard_Parallel_1\",17.5],"+
                    "PARAMETER[\"Standard_Parallel_2\",29.5],"+
                    "PARAMETER[\"Scale_Factor\",1],"+
                    "UNIT[\"Meter\",1,AUTHORITY[\"EPSG\",\"9001\"]]]"));

            Assert.IsTrue(TestParse(2,
                "PROJCS[\"Google Maps Global Mercator\",GEOGCS[\"WGS 84\",DATUM[\"WGS_1984\",SPHEROID[\"WGS 84\",6378137,298.257223563,AUTHORITY[\"EPSG\",\"7030\"]],AUTHORITY[\"EPSG\",\"6326\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.01745329251994328,AUTHORITY[\"EPSG\",\"9122\"]],AUTHORITY[\"EPSG\",\"4326\"]],PROJECTION[\"Mercator_2SP\"],PARAMETER[\"standard_parallel_1\",0],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",0],PARAMETER[\"false_easting\",0],PARAMETER[\"false_northing\",0],UNIT[\"Meter\",1],EXTENSION[\"PROJ4\",\"+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext  +no_defs\"],AUTHORITY[\"EPSG\",\"900913\"]]"));
        }
    }
}