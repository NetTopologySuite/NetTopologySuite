using System.Reflection;
using GeoAPI.CoordinateSystems.Transformations;
using NUnit.Framework;

namespace ProjNet.UnitTests
{
    public class SharpMapIssues: CoordinateTransformTestsBase
    {
        public SharpMapIssues()
        {
            Verbose = true;
        }

        string wkt2236 = "PROJCS[\"NAD83 / Florida East (ftUS)\", GEOGCS [ \"NAD83\", DATUM [\"North American Datum 1983 (EPSG ID 6269)\", SPHEROID [\"GRS 1980 (EPSG ID 7019)\", 6378137, 298.257222101]], PRIMEM [ \"Greenwich\", 0.000000 ], UNIT [\"Decimal Degree\", 0.01745329251994328]], PROJECTION [\"SPCS83 Florida East zone (US Survey feet) (EPSG OP 15318)\"], PARAMETER [\"Latitude_Of_Origin\", 24.33333333333333333333333333333333333333], PARAMETER [\"Central_Meridian\", -80.9999999999999999999999999999999999999], PARAMETER [\"Scale_Factor\", 0.999941177], PARAMETER [\"False_Easting\", 656166.6669999999999999999999999999999999], PARAMETER [\"False_Northing\", 0], UNIT [\"U.S. Foot\", 0.3048006096012192024384048768097536195072]]";
        string wkt8307 = "GEOGCS [ \"WGS 84\", DATUM [\"World Geodetic System 1984 (EPSG ID 6326)\", SPHEROID [\"WGS 84 (EPSG ID 7030)\", 6378137, 298.257223563]], PRIMEM [ \"Greenwich\", 0.000000 ], UNIT [\"Decimal Degree\", 0.01745329251994328]]";

        [Test, Description("NAD83 (State Plane) projection to the WGS84 (Lat/Long), http://sharpmap.codeplex.com/discussions/435794")] 
        public void TestNad83ToWGS84()
        {
            var src = CoordinateSystemFactory.CreateFromWkt(wkt2236);
            var tgt = CoordinateSystemFactory.CreateFromWkt(wkt8307);//CoordinateSystems.GeographicCoordinateSystem.WGS84;;

            ProjNet.CoordinateSystems.Projections.ProjectionsRegistry.Register("SPCS83 Florida East zone (US Survey feet) (EPSG OP 15318)", 
                ReflectType("ProjNet.CoordinateSystems.Projections.TransverseMercator"));

            ICoordinateTransformation transform = null;
            Assert.DoesNotThrow(() => transform = CoordinateTransformationFactory.CreateFromCoordinateSystems(src, tgt));
            Assert.IsNotNull(transform);
        }

        private System.Type ReflectType(string typeName)
        {
            var asm = Assembly.GetAssembly(typeof (ProjNet.CoordinateSystems.Projections.MapProjection));
            var res = asm.GetType(typeName);
            return res;
        }
    }
}