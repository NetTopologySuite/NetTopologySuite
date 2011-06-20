using System;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.UnitTests.Converters.WKT
{
	[TestFixture]
	public class WKTCoordSysParserTests
	{
		/// <summary>
		/// Parses a coordinate system WKTs
		/// </summary>
		/// <remarks><code>
		/// PROJCS["NAD83(HARN) / Texas Central (ftUS)",
		/// 	GEOGCS[
		/// 		"NAD83(HARN)",
		/// 		DATUM[
		/// 			"NAD83_High_Accuracy_Regional_Network",
		/// 			SPHEROID[
		/// 				"GRS 1980",
		/// 				6378137,
		/// 				298.257222101,
		/// 				AUTHORITY["EPSG","7019"]
		/// 			],
		///				TOWGS84[725,685,536,0,0,0,0],
		/// 			AUTHORITY["EPSG","6152"]
		/// 		],
		/// 		PRIMEM[
		/// 			"Greenwich",
		/// 			0,
		/// 			AUTHORITY["EPSG","8901"]
		/// 		],
		/// 		UNIT[
		/// 			"degree",
		/// 			0.01745329251994328,
		/// 			AUTHORITY["EPSG","9122"]
		/// 		],
		/// 		AUTHORITY["EPSG","4152"]
		/// 	],
		/// 	PROJECTION["Lambert_Conformal_Conic_2SP"],
		/// 	PARAMETER["standard_parallel_1",31.88333333333333],
		/// 	PARAMETER["standard_parallel_2",30.11666666666667],
		/// 	PARAMETER["latitude_of_origin",29.66666666666667],
		/// 	PARAMETER["central_meridian",-100.3333333333333],
		/// 	PARAMETER["false_easting",2296583.333],
		/// 	PARAMETER["false_northing",9842500.000000002],
		/// 	UNIT[
		/// 		"US survey foot",
		/// 		0.3048006096012192,
		/// 		AUTHORITY["EPSG","9003"]
		/// 	],
		/// 	AUTHORITY["EPSG","2918"]
		/// ]
		/// </code></remarks>
		[Test]
		public void ParseCoordSys()
		{
			CoordinateSystemFactory fac = new CoordinateSystemFactory();
            string wkt = "PROJCS[\"NAD83(HARN) / Texas Central (ftUS)\", GEOGCS[\"NAD83(HARN)\", DATUM[\"NAD83_High_Accuracy_Regional_Network\", SPHEROID[\"GRS 1980\", 6378137, 298.257222101, AUTHORITY[\"EPSG\", \"7019\"]], TOWGS84[725, 685, 536, 0, 0, 0, 0], AUTHORITY[\"EPSG\", \"6152\"]], PRIMEM[\"Greenwich\", 0, AUTHORITY[\"EPSG\", \"8901\"]], UNIT[\"degree\", 0.0174532925199433, AUTHORITY[\"EPSG\", \"9122\"]], AUTHORITY[\"EPSG\", \"4152\"]], UNIT[\"US survey foot\", 0.304800609601219, AUTHORITY[\"EPSG\", \"9003\"]], PROJECTION[\"Lambert_Conformal_Conic_2SP\"], PARAMETER[\"standard_parallel_1\", 31.883333333333], PARAMETER[\"standard_parallel_2\", 30.1166666667], PARAMETER[\"latitude_of_origin\", 29.6666666667], PARAMETER[\"central_meridian\", -100.333333333333], PARAMETER[\"false_easting\", 2296583.333], PARAMETER[\"false_northing\", 9842500], AUTHORITY[\"EPSG\", \"2918\"]]";
			ProjectedCoordinateSystem pcs = CoordinateSystemWktReader.Parse(wkt) as ProjectedCoordinateSystem;
			Assert.IsNotNull(pcs, "Could not parse WKT: " + wkt);

			Assert.AreEqual("NAD83(HARN) / Texas Central (ftUS)", pcs.Name);
			Assert.AreEqual("NAD83(HARN)", pcs.GeographicCoordinateSystem.Name);
			Assert.AreEqual("NAD83_High_Accuracy_Regional_Network", pcs.GeographicCoordinateSystem.HorizontalDatum.Name);
			Assert.AreEqual("GRS 1980", pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.Name);
			Assert.AreEqual(6378137, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.SemiMajorAxis);
			Assert.AreEqual(298.257222101, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.InverseFlattening);
			Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.Authority);
			Assert.AreEqual(7019, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.AuthorityCode);
			Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.HorizontalDatum.Authority);
			Assert.AreEqual(6152, pcs.GeographicCoordinateSystem.HorizontalDatum.AuthorityCode);
			Assert.AreEqual(new Wgs84ConversionInfo(725, 685, 536, 0, 0, 0, 0), pcs.GeographicCoordinateSystem.HorizontalDatum.Wgs84Parameters);
			Assert.AreEqual("Greenwich", pcs.GeographicCoordinateSystem.PrimeMeridian.Name);
			Assert.AreEqual(0, pcs.GeographicCoordinateSystem.PrimeMeridian.Longitude);
			Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.PrimeMeridian.Authority);
			Assert.AreEqual(8901, pcs.GeographicCoordinateSystem.PrimeMeridian.AuthorityCode, 8901);
			Assert.AreEqual("degree", pcs.GeographicCoordinateSystem.AngularUnit.Name);
			Assert.AreEqual(0.0174532925199433, pcs.GeographicCoordinateSystem.AngularUnit.RadiansPerUnit);
			Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.AngularUnit.Authority);
			Assert.AreEqual(9122, pcs.GeographicCoordinateSystem.AngularUnit.AuthorityCode);
			Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.Authority);
			Assert.AreEqual(4152, pcs.GeographicCoordinateSystem.AuthorityCode, 4152);
			Assert.AreEqual("Lambert_Conformal_Conic_2SP", pcs.Projection.ClassName, "Projection Classname");

			ProjectionParameter latitude_of_origin = pcs.Projection.GetParameter("latitude_of_origin");
			Assert.IsNotNull(latitude_of_origin);
			Assert.AreEqual(29.6666666667, latitude_of_origin.Value);
			ProjectionParameter central_meridian = pcs.Projection.GetParameter("central_meridian");
			Assert.IsNotNull(central_meridian);
			Assert.AreEqual(-100.333333333333, central_meridian.Value);
			ProjectionParameter standard_parallel_1 = pcs.Projection.GetParameter("standard_parallel_1");
			Assert.IsNotNull(standard_parallel_1);
			Assert.AreEqual(31.883333333333, standard_parallel_1.Value);
			ProjectionParameter standard_parallel_2 = pcs.Projection.GetParameter("standard_parallel_2");
			Assert.IsNotNull(standard_parallel_2);
			Assert.AreEqual(30.1166666667, standard_parallel_2.Value);
			ProjectionParameter false_easting = pcs.Projection.GetParameter("false_easting");
			Assert.IsNotNull(false_easting);
			Assert.AreEqual(2296583.333, false_easting.Value);
			ProjectionParameter false_northing = pcs.Projection.GetParameter("false_northing");
			Assert.IsNotNull(false_northing);
			Assert.AreEqual(9842500, false_northing.Value);

			Assert.AreEqual("US survey foot", pcs.LinearUnit.Name);
			Assert.AreEqual(0.304800609601219, pcs.LinearUnit.MetersPerUnit);
			Assert.AreEqual("EPSG", pcs.LinearUnit.Authority);
			Assert.AreEqual(9003, pcs.LinearUnit.AuthorityCode);
			Assert.AreEqual("EPSG", pcs.Authority);
			Assert.AreEqual(2918, pcs.AuthorityCode);
			Assert.AreEqual(wkt, pcs.WKT);
		}
		/// <summary>
		/// This test reads in a file with 2671 pre-defined coordinate systems and projections,
		/// and tries to parse them.
		/// </summary>
		[Test]
		public void ParseAllWKTs()
		{
			CoordinateSystemFactory fac = new CoordinateSystemFactory();
			int parsecount = 0;
            foreach (SRIDReader.WKTstring wkt in SRIDReader.GetSRIDs())
            {
                ICoordinateSystem cs = CoordinateSystemWktReader.Parse(wkt.WKT) as ICoordinateSystem;
                Assert.IsNotNull(cs, "Could not parse WKT: " + wkt);
                parsecount++;
            }
			Assert.AreEqual(parsecount, 2671, "Not all WKT was parsed");
		}

		/// <summary>
		/// This test reads in a file with 2671 pre-defined coordinate systems and projections,
		/// and tries to create a transformation with them.
		/// </summary>
		[Test]
		public void TestTransformAllWKTs()
		{
			//GeographicCoordinateSystem.WGS84
			CoordinateTransformationFactory fact = new CoordinateTransformationFactory();
			CoordinateSystemFactory fac = new CoordinateSystemFactory();
			int parsecount = 0;
			StreamReader sr = File.OpenText(@"..\..\SRID.csv");
			string line = "";
			while (!sr.EndOfStream)
			{
				line = sr.ReadLine();
				int split = line.IndexOf(';');
				if (split > -1)
				{
					string srid = line.Substring(0, split);
					string wkt = line.Substring(split + 1);
					ICoordinateSystem cs = CoordinateSystemWktReader.Parse(wkt) as ICoordinateSystem;
					if (cs == null) continue; //We check this in another test.
					if (cs is IProjectedCoordinateSystem)
					{
						switch ((cs as IProjectedCoordinateSystem).Projection.ClassName)
						{
							//Skip not supported projections
							case "Oblique_Stereographic": 
							case "Transverse_Mercator_South_Orientated":
							case "Hotine_Oblique_Mercator":
							case "Lambert_Conformal_Conic_1SP":
							case "Krovak":
							case "Cassini_Soldner":
							case "Lambert_Azimuthal_Equal_Area":
							case "Tunisia_Mining_Grid":
							case "New_Zealand_Map_Grid":
							case "Polyconic":
							case "Lambert_Conformal_Conic_2SP_Belgium":
							case "Polar_Stereographic":
								continue;
							default: break;
						}
					}
					try
					{
						ICoordinateTransformation trans = fact.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, cs);
					}
					catch (Exception ex)
					{
						if (cs is IProjectedCoordinateSystem)
							Assert.Fail("Could not create transformation from:\r\n" + wkt + "\r\n" + ex.Message + "\r\nClass name:" + (cs as IProjectedCoordinateSystem).Projection.ClassName);
						else
							Assert.Fail("Could not create transformation from:\r\n" + wkt + "\r\n" + ex.Message);						
					}
					parsecount++;
				}
			}
			sr.Close();
			Assert.AreEqual(parsecount, 2536, "Not all WKT was processed");
		}
        [Test]
        public void TestUnitBeforeProjection()
        {
            var fac = new CoordinateSystemFactory();
            var wkt = "PROJCS[\"OSGB 1936 / British National Grid\"," +
                 "GEOGCS[\"OSGB 1936\"," +
                 "DATUM[\"OSGB_1936\"," +
                     "SPHEROID[\"Airy 1830\",6377563.396,299.3249646,AUTHORITY[\"EPSG\",\"7001\"]]," +
                     "AUTHORITY[\"EPSG\",\"6277\"]]," +
                     "PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]]," +
                     "UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9122\"]]," +
                     "AUTHORITY[\"EPSG\",\"4277\"]]," +
                 "UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]]," +
                 "PROJECTION[\"Transverse_Mercator\"]," +
                 "PARAMETER[\"latitude_of_origin\",49]," +
                 "PARAMETER[\"central_meridian\",-2]," +
                 "PARAMETER[\"scale_factor\",0.9996012717]," +
                 "PARAMETER[\"false_easting\",400000]," +
                 "PARAMETER[\"false_northing\",-100000]," +
                 "AUTHORITY[\"EPSG\",\"27700\"]," +
                 "AXIS[\"Easting\",EAST]," +
                 "AXIS[\"Northing\",NORTH]]";
            ProjectedCoordinateSystem pcs = fac.CreateFromWkt(wkt) as ProjectedCoordinateSystem;

            Assert.IsNotNull(pcs);

            Assert.AreEqual("OSGB 1936 / British National Grid", pcs.Name);
            Assert.AreEqual("OSGB 1936", pcs.GeographicCoordinateSystem.Name);
            Assert.AreEqual("OSGB_1936", pcs.GeographicCoordinateSystem.HorizontalDatum.Name);
            Assert.AreEqual("Airy 1830", pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.Name);
            Assert.AreEqual(6377563.396, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.SemiMajorAxis);
            Assert.AreEqual(299.3249646, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.InverseFlattening);
            Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.Authority);
            Assert.AreEqual(7001, pcs.GeographicCoordinateSystem.HorizontalDatum.Ellipsoid.AuthorityCode);
            Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.HorizontalDatum.Authority);
            Assert.AreEqual(6277, pcs.GeographicCoordinateSystem.HorizontalDatum.AuthorityCode);
            Assert.AreEqual("Greenwich", pcs.GeographicCoordinateSystem.PrimeMeridian.Name);
            Assert.AreEqual(0, pcs.GeographicCoordinateSystem.PrimeMeridian.Longitude);
            Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.PrimeMeridian.Authority);
            Assert.AreEqual(8901, pcs.GeographicCoordinateSystem.PrimeMeridian.AuthorityCode, 8901);
            Assert.AreEqual("degree", pcs.GeographicCoordinateSystem.AngularUnit.Name);
            Assert.AreEqual(0.0174532925199433, pcs.GeographicCoordinateSystem.AngularUnit.RadiansPerUnit);
            Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.AngularUnit.Authority);
            Assert.AreEqual(9122, pcs.GeographicCoordinateSystem.AngularUnit.AuthorityCode);
            Assert.AreEqual("EPSG", pcs.GeographicCoordinateSystem.Authority);
            Assert.AreEqual(4277, pcs.GeographicCoordinateSystem.AuthorityCode, 4277);

            Assert.AreEqual("Transverse_Mercator", pcs.Projection.ClassName, "Projection Classname");

            ProjectionParameter latitude_of_origin = pcs.Projection.GetParameter("latitude_of_origin");
            Assert.IsNotNull(latitude_of_origin);
            Assert.AreEqual(49, latitude_of_origin.Value);
            ProjectionParameter central_meridian = pcs.Projection.GetParameter("central_meridian");
            Assert.IsNotNull(central_meridian);
            Assert.AreEqual(-2, central_meridian.Value);
            ProjectionParameter scale_factor = pcs.Projection.GetParameter("scale_factor");
            Assert.IsNotNull(scale_factor);
            Assert.AreEqual(0.9996012717, scale_factor.Value);
            ProjectionParameter false_easting = pcs.Projection.GetParameter("false_easting");
            Assert.IsNotNull(false_easting);
            Assert.AreEqual(400000, false_easting.Value);
            ProjectionParameter false_northing = pcs.Projection.GetParameter("false_northing");
            Assert.IsNotNull(false_northing);
            Assert.AreEqual(-100000, false_northing.Value);

            Assert.AreEqual("metre", pcs.LinearUnit.Name);
            Assert.AreEqual(1, pcs.LinearUnit.MetersPerUnit);
            Assert.AreEqual("EPSG", pcs.LinearUnit.Authority);
            Assert.AreEqual(9001, pcs.LinearUnit.AuthorityCode);
            Assert.AreEqual("EPSG", pcs.Authority);
            Assert.AreEqual(27700, pcs.AuthorityCode);

            string newWkt = pcs.WKT.Replace(", ", ",");
            Assert.AreEqual(wkt, newWkt);

        }
	}
}
