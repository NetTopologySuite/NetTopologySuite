using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace ProjNet.UnitTests
{
	[TestFixture]
	public class CoordinateTransformTests
	{
	    [Test]
	    public void TestCentralMeridianParse()
	    {
            string strSouthPole = "PROJCS[\"South_Pole_Lambert_Azimuthal_Equal_Area\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137,298.257223563]],PRIMEM[\"Greenwich\",0],UNIT[\"Degree\",0.017453292519943295]],PROJECTION[\"Lambert_Azimuthal_Equal_Area\"],PARAMETER[\"False_Easting\",0],PARAMETER[\"False_Northing\",0],PARAMETER[\"Central_Meridian\",-127],PARAMETER[\"Latitude_Of_Origin\",-90],UNIT[\"Meter\",1]]";

            CoordinateSystemFactory pCoordSysFactory = new CoordinateSystemFactory();
            ICoordinateSystem pSouthPole = pCoordSysFactory.CreateFromWkt(strSouthPole);
        }
        
        [Test]
		public void TestAlbersProjection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982, LinearUnit.USSurveyFoot);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("central_meridian", -96));
			parameters.Add(new ProjectionParameter("latitude_of_center", 23));
			parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
			parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Albers Conical Equal Area", "albers", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { -75, 35 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 1885472.7, 1535925 };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), String.Format("Albers forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Albers reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));
		}
		[Test]
		public void TestAlbersProjectionFeet()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982, LinearUnit.USSurveyFoot);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("central_meridian", -96));
			parameters.Add(new ProjectionParameter("latitude_of_center", 23));
			parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
			parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Albers Conical Equal Area", "albers", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Foot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { -75, 35 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 1885472.7 / LinearUnit.Foot.MetersPerUnit, 1535925 / LinearUnit.Foot.MetersPerUnit };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.1), String.Format("Albers forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Albers reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));
		}
		[Test]
		public void TestMercator_1SP_Projection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
			parameters.Add(new ProjectionParameter("central_meridian", 110));
			parameters.Add(new ProjectionParameter("scale_factor", 0.997));
			parameters.Add(new ProjectionParameter("false_easting", 3900000));
			parameters.Add(new ProjectionParameter("false_northing", 900000));
			IProjection projection = cFac.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { 120, -3 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 5009726.58, 569150.82 };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Mercator_1SP forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Mercator_1SP reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));
		}
		[Test]
		public void TestMercator_1SP_Projection_Feet()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
			parameters.Add(new ProjectionParameter("central_meridian", 110));
			parameters.Add(new ProjectionParameter("scale_factor", 0.997));
			parameters.Add(new ProjectionParameter("false_easting", 3900000 / LinearUnit.Foot.MetersPerUnit));
			parameters.Add(new ProjectionParameter("false_northing", 900000 / LinearUnit.Foot.MetersPerUnit));
			IProjection projection = cFac.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Foot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { 120, -3 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 5009726.58 / LinearUnit.Foot.MetersPerUnit, 569150.82 / LinearUnit.Foot.MetersPerUnit };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Mercator_1SP forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0],expected[1], pUtm[0],pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Mercator_1SP reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));
		}
		[Test]
		public void TestMercator_2SP_Projection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Krassowski 1940", 6378245.0, 298.3, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Krassowski 1940", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Krassowski 1940", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 42));
			parameters.Add(new ProjectionParameter("central_meridian", 51));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Mercator_2SP", "Mercator_2SP", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Pulkovo 1942 / Mercator Caspian Sea", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { 53, 53 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 165704.29, 5171848.07 };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Mercator_2SP forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Mercator_2SP reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestTransverseMercator_Projection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Airy 1830", 6377563.396, 299.32496, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Airy 1830", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Airy 1830", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 49));
			parameters.Add(new ProjectionParameter("central_meridian", -2));
			parameters.Add(new ProjectionParameter("scale_factor", 0.9996012717));
			parameters.Add(new ProjectionParameter("false_easting", 400000));
			parameters.Add(new ProjectionParameter("false_northing", -100000));
			IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("OSGB 1936 / British National Grid", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { 0.5, 50.5 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 577274.99, 69740.50 };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("TransverseMercator forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("TransverseMercator reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestLambertConicConformal2SP_Projection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 20925832.16, 294.97470, LinearUnit.USSurveyFoot);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 27.833333333));
			parameters.Add(new ProjectionParameter("central_meridian", -99));
			parameters.Add(new ProjectionParameter("standard_parallel_1", 28.3833333333));
			parameters.Add(new ProjectionParameter("standard_parallel_2", 30.2833333333));
			parameters.Add(new ProjectionParameter("false_easting", 2000000 / LinearUnit.USSurveyFoot.MetersPerUnit));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Lambert Conic Conformal (2SP)", "lambert_conformal_conic_2sp", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("NAD27 / Texas South Central", gcs, projection, LinearUnit.USSurveyFoot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			double[] pGeo = new double[] { -96, 28.5 };
			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			double[] expected = new double[] { 2963503.91 / LinearUnit.USSurveyFoot.MetersPerUnit, 254759.80 / LinearUnit.USSurveyFoot.MetersPerUnit };
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), String.Format("LambertConicConformal2SP forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("LambertConicConformal2SP reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));

		}

		[Test]
		public void TestGeocentric()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("ETRF89 Geographic", AngularUnit.Degrees, HorizontalDatum.ETRF89, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
			IGeocentricCoordinateSystem gcenCs = cFac.CreateGeocentricCoordinateSystem("ETRF89 Geocentric", HorizontalDatum.ETRF89, LinearUnit.Metre, PrimeMeridian.Greenwich);
			CoordinateTransformationFactory gtFac = new CoordinateTransformationFactory();
			ICoordinateTransformation ct = gtFac.CreateFromCoordinateSystems(gcs, gcenCs);
			double[] pExpected = new double[] { 2 + 7.0 / 60 + 46.38 / 3600, 53 + 48.0 / 60 + 33.82/3600 }; // Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			double[] pExpected3D = new double[] { pExpected[0], pExpected[1], 73.0 };
			double[] p0 = new double[] { 3771793.97, 140253.34, 5124304.35 };
			double[] p1 = ct.MathTransform.Transform(pExpected3D) as double[];
			double[] p2 = ct.MathTransform.Inverse().Transform(p1) as double[];
			Assert.IsTrue(ToleranceLessThan(p1, p0, 0.01));
			Assert.IsTrue(ToleranceLessThan(p2, pExpected, 0.00001));
		}

		[Test]
		public void TestDatumTransform()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();
			//Define datums
			HorizontalDatum wgs72 = HorizontalDatum.WGS72;
			HorizontalDatum ed50 = HorizontalDatum.ED50;

			//Define geographic coordinate systems
			IGeographicCoordinateSystem gcsWGS72 = cFac.CreateGeographicCoordinateSystem("WGS72 Geographic", AngularUnit.Degrees, wgs72, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			IGeographicCoordinateSystem gcsWGS84 = cFac.CreateGeographicCoordinateSystem("WGS84 Geographic", AngularUnit.Degrees, HorizontalDatum.WGS84, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			IGeographicCoordinateSystem gcsED50 = cFac.CreateGeographicCoordinateSystem("ED50 Geographic", AngularUnit.Degrees, ed50, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			//Define geocentric coordinate systems
			IGeocentricCoordinateSystem gcenCsWGS72 = cFac.CreateGeocentricCoordinateSystem("WGS72 Geocentric", wgs72, LinearUnit.Metre, PrimeMeridian.Greenwich);
			IGeocentricCoordinateSystem gcenCsWGS84 = cFac.CreateGeocentricCoordinateSystem("WGS84 Geocentric", HorizontalDatum.WGS84, LinearUnit.Metre, PrimeMeridian.Greenwich);
			IGeocentricCoordinateSystem gcenCsED50 = cFac.CreateGeocentricCoordinateSystem("ED50 Geocentric", ed50, LinearUnit.Metre, PrimeMeridian.Greenwich);

			//Define projections
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
			parameters.Add(new ProjectionParameter("central_meridian", 9));
			parameters.Add(new ProjectionParameter("scale_factor", 0.9996));
			parameters.Add(new ProjectionParameter("false_easting", 500000));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);
			IProjectedCoordinateSystem utmED50 = cFac.CreateProjectedCoordinateSystem("ED50 UTM Zone 32N", gcsED50, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
			IProjectedCoordinateSystem utmWGS84 = cFac.CreateProjectedCoordinateSystem("WGS84 UTM Zone 32N", gcsWGS84, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			//Set TOWGS84 parameters
			wgs72.Wgs84Parameters = new Wgs84ConversionInfo(0, 0, 4.5, 0, 0, 0.554, 0.219);
			ed50.Wgs84Parameters = new Wgs84ConversionInfo(-81.0703, -89.3603, -115.7526,
														   -0.48488, -0.02436, -0.41321,
														   -0.540645); //Parameters for Denmark

			//Set up coordinate transformations
			CoordinateTransformationFactory ctFac = new CoordinateTransformationFactory();
			ICoordinateTransformation ctForw = ctFac.CreateFromCoordinateSystems(gcsWGS72, gcenCsWGS72); //Geographic->Geocentric (WGS72)
			ICoordinateTransformation ctWGS84_Gcen2Geo = ctFac.CreateFromCoordinateSystems(gcenCsWGS84, gcsWGS84);  //Geocentric->Geographic (WGS84)
			ICoordinateTransformation ctWGS84_Geo2UTM = ctFac.CreateFromCoordinateSystems(gcsWGS84, utmWGS84);  //UTM ->Geographic (WGS84)
			ICoordinateTransformation ctED50_UTM2Geo = ctFac.CreateFromCoordinateSystems(utmED50, gcsED50);  //UTM ->Geographic (ED50)
			ICoordinateTransformation ctED50_Geo2Gcen = ctFac.CreateFromCoordinateSystems(gcsED50, gcenCsED50); //Geographic->Geocentric (ED50)

			//Test datum-shift from WGS72 to WGS84
			//Point3D pGeoCenWGS72 = ctForw.MathTransform.Transform(pLongLatWGS72) as Point3D;
			double[] pGeoCenWGS72 = new double[] {3657660.66, 255768.55, 5201382.11};
			ICoordinateTransformation geocen_ed50_2_Wgs84 = ctFac.CreateFromCoordinateSystems(gcenCsWGS72, gcenCsWGS84);
			double[] pGeoCenWGS84 = geocen_ed50_2_Wgs84.MathTransform.Transform(pGeoCenWGS72);
			//Point3D pGeoCenWGS84 = wgs72.Wgs84Parameters.Apply(pGeoCenWGS72);

			Assert.IsTrue(ToleranceLessThan(new double[] { 3657660.78, 255778.43, 5201387.75 }, pGeoCenWGS84, 0.01));

			ICoordinateTransformation utm_ed50_2_Wgs84 = ctFac.CreateFromCoordinateSystems(utmED50, utmWGS84);
			double[] pUTMED50 = new double[] {600000, 6100000};
			double[] pUTMWGS84 = utm_ed50_2_Wgs84.MathTransform.Transform(pUTMED50);
			Assert.IsTrue(ToleranceLessThan(new double[] { 599928.6, 6099790.2 }, pUTMWGS84, 0.1));
			//Perform reverse
			ICoordinateTransformation utm_Wgs84_2_Ed50 = ctFac.CreateFromCoordinateSystems(utmWGS84, utmED50);
			pUTMED50 = utm_Wgs84_2_Ed50.MathTransform.Transform(pUTMWGS84);
			Assert.IsTrue(ToleranceLessThan(new double[] {600000, 6100000}, pUTMED50, 0.1));
			//Assert.IsTrue(Math.Abs((pUTMWGS84 as Point3D).Z - 36.35) < 0.5);
			//Point pExpected = Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			//ED50_to_WGS84_Denmark: datum.Wgs84Parameters = new Wgs84ConversionInfo(-89.5, -93.8, 127.6, 0, 0, 4.5, 1.2);

		}

		[Test]
		public void TestKrovak_Projection()
		{
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			List<ProjectionParameter> parameters = new List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_center", 49.5));
			parameters.Add(new ProjectionParameter("longitude_of_center", 42.5));
			parameters.Add(new ProjectionParameter("azimuth", 30.28813972222222));
			parameters.Add(new ProjectionParameter("pseudo_standard_parallel_1", 78.5));
			parameters.Add(new ProjectionParameter("scale_factor", 0.9999));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Krovak", "Krovak", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("WGS 84", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			// test case 1
			double[] pGeo = new double[] { 12, 48 };
			double[] expected = new double[] { -953172.26, -1245573.32 };

			double[] pUtm = trans.MathTransform.Transform(pGeo);
			double[] pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Krovak forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Krovak reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));

			// test case 2
			pGeo = new double[] { 18, 49 };
			expected = new double[] { -499258.06, -1192389.16 };

			pUtm = trans.MathTransform.Transform(pGeo);
			pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Krovak forward transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], pUtm[0], pUtm[1]));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Krovak reverse transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", pGeo[0], pGeo[1], pGeo2[0], pGeo2[1]));
		} 
		private bool ToleranceLessThan(double[] p1, double[] p2, double tolerance)
		{
			if(p1.Length>2 && p2.Length>2)
				return Math.Abs(p1[0] - p2[0]) < tolerance && Math.Abs(p1[1] - p2[1]) < tolerance && Math.Abs(p1[2] - p2[2]) < tolerance;
			else
				return Math.Abs(p1[0] - p2[0]) < tolerance && Math.Abs(p1[1] - p2[1]) < tolerance;
		}

        [Test]
        public void TestUnitTransforms()
        {
			ICoordinateSystem nadUTM = SRIDReader.GetCSbyID(2868); //UTM Arizona Central State Plane using Feet as units
			ICoordinateSystem wgs84GCS = SRIDReader.GetCSbyID(4326); //GCS WGS84
			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84GCS, nadUTM);

			double[] p0 = new double[] { -111.89, 34.165 };
			double[] expected = new double[] { 708066.190579, 1151426.44638 };
			
			double[] p1 = trans.MathTransform.Transform(p0);
			double[] p2 = trans.MathTransform.Inverse().Transform(p1);

			Assert.IsTrue(ToleranceLessThan(p1, expected, 0.013), String.Format("Transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], p1[0],p1[1]));
			//WARNING: This accuracy is too poor!
			Assert.IsTrue(ToleranceLessThan(p0, p2, 0.0000001), String.Format("Transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", p0[0], p0[1], p2[0], p2[1]));
        }

        [Test]
        public void TestPolyconicTransforms()
        {
            ICoordinateSystemFactory csf = new CoordinateSystemFactory();
            ICoordinateSystem sad69 = csf.CreateFromWkt("PROJCS[\"SAD69 / Brazil Polyconic (deprecated)\",GEOGCS[\"SAD69\",DATUM[\"South_American_Datum_1969\",SPHEROID[\"GRS 1967\",6378160,298.247167427,AUTHORITY[\"EPSG\",\"7036\"]],AUTHORITY[\"EPSG\",\"6291\"]],PRIMEM[\"Greenwich\",0,AUTHORITY[\"EPSG\",\"8901\"]],UNIT[\"degree\",0.0174532925199433,AUTHORITY[\"EPSG\",\"9108\"]],AUTHORITY[\"EPSG\",\"4291\"]],UNIT[\"metre\",1,AUTHORITY[\"EPSG\",\"9001\"]],PROJECTION[\"Polyconic\"],PARAMETER[\"latitude_of_origin\",0],PARAMETER[\"central_meridian\",-54],PARAMETER[\"false_easting\",5000000],PARAMETER[\"false_northing\",10000000],AUTHORITY[\"EPSG\",\"29100\"],AXIS[\"X\",EAST],AXIS[\"Y\",NORTH]]");
            ICoordinateSystem wgs84GCS = SRIDReader.GetCSbyID(4326); //GCS WGS84

            ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(wgs84GCS, sad69);

            double[] p0 = new double[] { -43.176377, -22.966494 };
            double[] expected = new double[] { 6108938.17208353, 7418275.67520224 };

            double[] p1 = trans.MathTransform.Transform(p0);
            trans.MathTransform.Invert();
            double[] p2 = trans.MathTransform.Transform(p1);

            Assert.IsTrue(ToleranceLessThan(p1, expected, 0.013), String.Format("Transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", expected[0], expected[1], p1[0], p1[1]));
            //WARNING: This accuracy is too poor!
            Assert.IsTrue(ToleranceLessThan(p0, p2, 0.0000001), String.Format("Transformation outside tolerance, Expected [{0},{1}], got [{2},{3}]", p0[0], p0[1], p2[0], p2[1]));
        }
	}
}
