using System;
using System.Collections.Generic;
using System.Text;

using GisSharpBlog.NetTopologySuite.CoordinateReferenceSystems;

using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.nunut_tests
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class CoordinateSystemBugTest
    {

        private String wkt = null;

        /// <summary>
        /// 
        /// </summary>
        public CoordinateSystemBugTest()
        {
            wkt = "PROJCS[\"UTM_Zone_10N\", "
                + "GEOGCS[\"WGS84\", "
                + "DATUM[\"WGS84\", "
                + "SPHEROID[\"WGS84\", 6378137.0, 298.257223563]], "
                + "PRIMEM[\"Greenwich\", 0.0], "
                + "UNIT[\"degree\",0.017453292519943295], "
                + "AXIS[\"Longitude\",EAST], "
                + "AXIS[\"Latitude\",NORTH]], "
                + "PROJECTION[\"Transverse_Mercator\"], "
                + "PARAMETER[\"semi_major\", 6378137.0], "
                + "PARAMETER[\"semi_minor\", 6356752.314245179], "
                + "PARAMETER[\"central_meridian\", -123.0], "
                + "PARAMETER[\"latitude_of_origin\", 0.0], "
                + "PARAMETER[\"scale_factor\", 0.9996], "
                + "PARAMETER[\"false_easting\", 500000.0], "
                + "PARAMETER[\"false_northing\", 0.0], "
                + "UNIT[\"metre\",1.0], "
                + "AXIS[\"x\",EAST], "
                + "AXIS[\"y\",NORTH]]";            
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestCoordinateParametersRead()
        {
            CoordinateSystemFactory cfact = new CoordinateSystemFactory();
            ProjectedCoordinateSystem psystem = (ProjectedCoordinateSystem)cfact.CreateFromWKT(wkt);
        }
    }
}
