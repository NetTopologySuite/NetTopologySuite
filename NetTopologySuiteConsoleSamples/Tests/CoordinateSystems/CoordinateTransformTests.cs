using System;
using System.Collections.Generic;
using System.Text;

using SharpMap.Geometries;
using SharpMap.CoordinateSystems;
using SharpMap.CoordinateSystems.Transformations;

using NUnit.Framework;

namespace UnitTests
{
	[TestFixture]
	public class CoordinateTransformTests
	{
		[Test]
		public void TestAlbersProjection()
		{
			CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 6378206.4, 294.9786982138982, LinearUnit.USSurveyFoot);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("longitude_of_center", -96));
			parameters.Add(new ProjectionParameter("latitude_of_center", 23));
			parameters.Add(new ProjectionParameter("standard_parallel_1", 29.5));
			parameters.Add(new ProjectionParameter("standard_parallel_2", 45.5));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Albers Conical Equal Area", "albers", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Albers Conical Equal Area", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(-75, 35);
			SharpMap.Geometries.Point pUtm = trans.MathTransform.Transform(pGeo);
			SharpMap.Geometries.Point pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			SharpMap.Geometries.Point expected = new Point(1885472.7, 1535925);
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.05), String.Format("Albers forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Albers reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestMercator_1SP_Projection()
		{
			CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Bessel 1840", 6377397.155, 299.15281, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Bessel 1840", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Bessel 1840", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 0));
			parameters.Add(new ProjectionParameter("central_meridian", 110));
			parameters.Add(new ProjectionParameter("scale_factor", 0.997));
			parameters.Add(new ProjectionParameter("false_easting", 3900000));
			parameters.Add(new ProjectionParameter("false_northing", 900000));
			IProjection projection = cFac.CreateProjection("Mercator_1SP", "Mercator_1SP", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Makassar / NEIEZ", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(120, -3);
			SharpMap.Geometries.Point pUtm = trans.MathTransform.Transform(pGeo);
			SharpMap.Geometries.Point pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			SharpMap.Geometries.Point expected = new Point(5009726.58, 569150.82);
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Mercator_1SP forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Mercator_1SP reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestMercator_2SP_Projection()
		{
			CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Krassowski 1940", 6378245.0, 298.3, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Krassowski 1940", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Krassowski 1940", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 42));
			parameters.Add(new ProjectionParameter("central_meridian", 51));
			parameters.Add(new ProjectionParameter("false_easting", 0));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Mercator_2SP", "Mercator_2SP", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("Pulkovo 1942 / Mercator Caspian Sea", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(53, 53);
			SharpMap.Geometries.Point pUtm = trans.MathTransform.Transform(pGeo);
			SharpMap.Geometries.Point pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			SharpMap.Geometries.Point expected = new Point(165704.29, 5171848.07);
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("Mercator_2SP forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("Mercator_2SP reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestTransverseMercator_Projection()
		{
			CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Airy 1830", 6377563.396, 299.32496, LinearUnit.Metre);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Airy 1830", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Airy 1830", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 49));
			parameters.Add(new ProjectionParameter("central_meridian", -2));
			parameters.Add(new ProjectionParameter("scale_factor", 0.9996012717));
			parameters.Add(new ProjectionParameter("false_easting", 400000));
			parameters.Add(new ProjectionParameter("false_northing", -100000));
			IProjection projection = cFac.CreateProjection("Transverse Mercator", "Transverse_Mercator", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("OSGB 1936 / British National Grid", gcs, projection, LinearUnit.Metre, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(0.5, 50.5);
			SharpMap.Geometries.Point pUtm = trans.MathTransform.Transform(pGeo);
			SharpMap.Geometries.Point pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			SharpMap.Geometries.Point expected = new Point(577274.99, 69740.50);
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("TransverseMercator forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("TransverseMercator reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));
		}
		[Test]
		public void TestLambertConicConformal2SP_Projection()
		{
			CoordinateSystemFactory cFac = new SharpMap.CoordinateSystems.CoordinateSystemFactory();

			IEllipsoid ellipsoid = cFac.CreateFlattenedSphere("Clarke 1866", 20925832.16, 294.97470, LinearUnit.USSurveyFoot);

			IHorizontalDatum datum = cFac.CreateHorizontalDatum("Clarke 1866", DatumType.HD_Geocentric, ellipsoid, null);
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("Clarke 1866", AngularUnit.Degrees, datum,
				PrimeMeridian.Greenwich, new AxisInfo("Lon", AxisOrientationEnum.East),
				new AxisInfo("Lat", AxisOrientationEnum.North));
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
			parameters.Add(new ProjectionParameter("latitude_of_origin", 27.833333333));
			parameters.Add(new ProjectionParameter("central_meridian", -99));
			parameters.Add(new ProjectionParameter("standard_parallel_1", 28.3833333333));
			parameters.Add(new ProjectionParameter("standard_parallel_2", 30.2833333333));
			parameters.Add(new ProjectionParameter("false_easting", 2000000));
			parameters.Add(new ProjectionParameter("false_northing", 0));
			IProjection projection = cFac.CreateProjection("Lambert Conic Conformal (2SP)", "lambert_conformal_conic_2sp", parameters);

			IProjectedCoordinateSystem coordsys = cFac.CreateProjectedCoordinateSystem("NAD27 / Texas South Central", gcs, projection, LinearUnit.USSurveyFoot, new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));

			ICoordinateTransformation trans = new CoordinateTransformationFactory().CreateFromCoordinateSystems(gcs, coordsys);

			SharpMap.Geometries.Point pGeo = new SharpMap.Geometries.Point(-96, 28.5);
			SharpMap.Geometries.Point pUtm = trans.MathTransform.Transform(pGeo);
			SharpMap.Geometries.Point pGeo2 = trans.MathTransform.Inverse().Transform(pUtm);

			SharpMap.Geometries.Point expected = new Point(2963503.91, 254759.80);
			Assert.IsTrue(ToleranceLessThan(pUtm, expected, 0.02), String.Format("LambertConicConformal2SP forward transformation outside tolerance, Expected {0}, got {1}", expected.ToString(), pUtm.ToString()));
			Assert.IsTrue(ToleranceLessThan(pGeo, pGeo2, 0.0000001), String.Format("LambertConicConformal2SP reverse transformation outside tolerance, Expected {0}, got {1}", pGeo.ToString(), pGeo2.ToString()));

		}

		[Test]
		public void TestGeocentric()
		{			
			CoordinateSystemFactory cFac = new CoordinateSystemFactory();
			IGeographicCoordinateSystem gcs = cFac.CreateGeographicCoordinateSystem("ETRF89 Geographic", AngularUnit.Degrees, HorizontalDatum.ETRF89, PrimeMeridian.Greenwich,
				new AxisInfo("East", AxisOrientationEnum.East), new AxisInfo("North", AxisOrientationEnum.North));
			IGeocentricCoordinateSystem gcenCs = cFac.CreateGeocentricCoordinateSystem("ETRF89 Geocentric", HorizontalDatum.ETRF89, LinearUnit.Metre, PrimeMeridian.Greenwich);
			CoordinateTransformationFactory gtFac = new CoordinateTransformationFactory();
			ICoordinateTransformation ct = gtFac.CreateFromCoordinateSystems(gcs,gcenCs);
			Point pExpected = Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			Point3D pExpected3D = new Point3D(pExpected.X, pExpected.Y, 73.0);
			Point3D p0 = new Point3D(3771793.97, 140253.34, 5124304.35);
			Point3D p1 = ct.MathTransform.Transform(pExpected3D) as Point3D;
			Point3D p2 = ct.MathTransform.Inverse().Transform(p1) as Point3D;
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
			System.Collections.Generic.List<ProjectionParameter> parameters = new System.Collections.Generic.List<ProjectionParameter>(5);
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
			Point3D pGeoCenWGS72 = new Point3D(3657660.66, 255768.55, 5201382.11);			
			ICoordinateTransformation geocen_ed50_2_Wgs84 = ctFac.CreateFromCoordinateSystems(gcenCsWGS72, gcenCsWGS84);
			Point3D pGeoCenWGS84 = geocen_ed50_2_Wgs84.MathTransform.Transform(pGeoCenWGS72) as Point3D;
			//Point3D pGeoCenWGS84 = wgs72.Wgs84Parameters.Apply(pGeoCenWGS72);

			Assert.IsTrue(ToleranceLessThan(new Point3D(3657660.78, 255778.43, 5201387.75), pGeoCenWGS84, 0.01));

			ICoordinateTransformation utm_ed50_2_Wgs84 = ctFac.CreateFromCoordinateSystems(utmED50, utmWGS84);
			Point pUTMED50 = new Point(600000, 6100000);
			Point pUTMWGS84 = utm_ed50_2_Wgs84.MathTransform.Transform(pUTMED50);
			Assert.IsTrue(ToleranceLessThan(new Point(599928.6, 6099790.2), pUTMWGS84, 0.1));
			//Perform reverse
			ICoordinateTransformation utm_Wgs84_2_Ed50 = ctFac.CreateFromCoordinateSystems(utmWGS84, utmED50);
			pUTMED50 = utm_Wgs84_2_Ed50.MathTransform.Transform(pUTMWGS84);
			Assert.IsTrue(ToleranceLessThan(new Point(600000, 6100000), pUTMED50, 0.1));
			//Assert.IsTrue(Math.Abs((pUTMWGS84 as Point3D).Z - 36.35) < 0.5);
			//Point pExpected = Point.FromDMS(2, 7, 46.38, 53, 48, 33.82);
			//ED50_to_WGS84_Denmark: datum.Wgs84Parameters = new Wgs84ConversionInfo(-89.5, -93.8, 127.6, 0, 0, 4.5, 1.2);

		}
		private bool ToleranceLessThan(Point p1, Point p2, double tolerance)
		{
			return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance;
		}
		private bool ToleranceLessThan(Point3D p1, Point3D p2, double tolerance)
		{
			return Math.Abs(p1.X - p2.X) < tolerance && Math.Abs(p1.Y - p2.Y) < tolerance && Math.Abs(p1.Z - p2.Z) < tolerance;
		}
	}
}
