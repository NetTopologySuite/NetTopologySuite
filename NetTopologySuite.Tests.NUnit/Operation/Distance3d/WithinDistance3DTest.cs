using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance3D;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance3d
{
public class WithinDistance3DTest
{
	static readonly WKTReader Rdr = new WKTReader();
	
    [TestAttribute]
	public void TestEmpty()
	{
		CheckWithinDistance(	"POINT EMPTY", "POINT EMPTY",	0);
		CheckWithinDistance(	"LINESTRING EMPTY", "POINT (0 0 0)",	1, true);
	}

    [TestAttribute]
    public void TestPointPoint()
    {
		CheckWithinDistance(	"POINT (0 0 0 )",
				"POINT (0 0 1 )",
		1);
		CheckWithinDistance(	"POINT (0 0 0 )",
				"POINT (0 0 1 )",
		0.5, false);
		CheckWithinDistance(	"POINT (10 10 1 )",
				"POINT (11 11 2 )",
				1.733);
		CheckWithinDistance(	"POINT (10 10 0 )",
				"POINT (10 20 10 )",
				14.143);
	}

    [TestAttribute]
    public void TestPointSeg()
    {
		CheckWithinDistance(	"LINESTRING (0 0 0, 10 10 10 )",
				"POINT (5 5 5 )",
				0);
		CheckWithinDistance(	"LINESTRING (10 10 10, 20 20 20 )",
				"POINT (11 11 10 )",
				0.8, false);
	}

    [TestAttribute]
    public void TestCrossSegmentsFlat()
    {
		CheckWithinDistance(	"LINESTRING (0 0 0, 10 10 0 )",
				"LINESTRING (10 0 0, 0 10 0 )",
		0);
		CheckWithinDistance(	"LINESTRING (0 0 10, 30 10 10 )",
				"LINESTRING (10 0 10, 0 10 10 )",
		0);
	}

    [TestAttribute]
    public void TestCrossSegments()
    {
		CheckWithinDistance(	"LINESTRING (0 0 0, 10 10 0 )",
				"LINESTRING (10 0 1, 0 10 1 )",
		1);
		CheckWithinDistance(	"LINESTRING (0 0 0, 20 20 0 )",
				"LINESTRING (10 0 1, 0 10 1 )",
		1);
		CheckWithinDistance(	"LINESTRING (20 10 20, 10 20 10 )",
				"LINESTRING (10 10 20, 20 20 10 )",
		0);
	}

    [TestAttribute]
    public void TestTSegmentsFlat()
    {
		CheckWithinDistance(	"LINESTRING (10 10 0, 10 20 0 )",
						"LINESTRING (20 15 0, 25 15 0 )",
				10);
	}

    [TestAttribute]
    public void TestParallelSegmentsFlat()
    {
		CheckWithinDistance(	"LINESTRING (10 10 0, 20 20 0 )",
						"LINESTRING (10 20 0, 20 30 0 )",
						7.0710678118654755);
	}

    [TestAttribute]
    public void TestParallelSegments()
    {
		CheckWithinDistance(	"LINESTRING (0 0 0, 1 0 0 )",
						"LINESTRING (0 0 1, 1 0 1 )",
						1);
		CheckWithinDistance(	"LINESTRING (10 10 0, 20 10 0 )",
				"LINESTRING (10 20 10, 20 20 10 )",
				14.142135623730951);
		CheckWithinDistance(	"LINESTRING (10 10 0, 20 20 0 )",
				"LINESTRING (10 20 10, 20 30 10 )",
				12.24744871391589 );
				// = distance from LINESTRING (10 10 0, 20 20 0 ) to POINT(10 20 10)
				// = hypotenuse(7.0710678118654755, 10)
	}

    [TestAttribute]
    public void TestLineLine()
	{
		CheckWithinDistance(	"LINESTRING (0 1 2, 1 1 1, 1 0 2 )",
				"LINESTRING (0 0 0.1, .5 .5 0, 1 1 0, 1.5 1.5 0, 2 2 0 )",
				1);		
		CheckWithinDistance(	"LINESTRING (10 10 20, 20 20 30, 20 20 1, 30 30 5 )",
				"LINESTRING (1 80 10, 0 39 5, 39 0 5, 80 1 20)",
				0.7071067811865476);		
	}

    [TestAttribute]
    public void TestPointPolygon()
	{
		// point above poly
		CheckWithinDistance(	"POINT (150 150 10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);	
		// point below poly
		CheckWithinDistance(	"POINT (150 150 -10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);				
		// point right of poly in YZ plane
		CheckWithinDistance(	"POINT (10 150 150)",
				"POLYGON ((0 100 200, 0 200 200, 0 200 100, 0 100 100, 0 100 200))",
				10);				
	}

    [TestAttribute]
    public void TestPointPolygonFlat()
	{
		// inside
		CheckWithinDistance(	"POINT (150 150 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// outside
		CheckWithinDistance(	"POINT (250 250 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				70.71067811865476);				
		// on
		CheckWithinDistance(	"POINT (200 200 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}

    [TestAttribute]
    public void TestLinePolygonFlat()
	{
		// line inside
		CheckWithinDistance(	"LINESTRING (150 150 0, 160 160 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// line outside
		CheckWithinDistance(	"LINESTRING (200 250 0, 260 260 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				50);	
		// line touching
		CheckWithinDistance(	"LINESTRING (200 200 0, 260 260 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}

    [TestAttribute]
    public void TestLinePolygonSimple()
	{
		// line crossing inside
		CheckWithinDistance(	"LINESTRING (150 150 10, 150 150 -10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// vertical line above
		CheckWithinDistance(	"LINESTRING (200 200 10, 260 260 100)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);	
		// vertical line touching
		CheckWithinDistance(	"LINESTRING (200 200 0, 260 260 100)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}
	
	String polyHoleFlat = "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0), (120 180 0, 180 180 0, 180 120 0, 120 120 0, 120 180 0))";

    [TestAttribute]
    public void TestLinePolygonHoleFlat()
	{
		// line crossing hole
		CheckWithinDistance(	"LINESTRING (150 150 10, 150 150 -10)", 	polyHoleFlat, 20, false);	
		// line crossing interior
		CheckWithinDistance(	"LINESTRING (110 110 10, 110 110 -10)",		polyHoleFlat, 0);	
	}

    [TestAttribute]
    public void TestPointPolygonHoleFlat()
	{
		// point above poly hole
		CheckWithinDistance(	"POINT (130 130 10)", 	polyHoleFlat, 14.143);	
		// point below poly hole
		CheckWithinDistance(	"POINT (130 130 -10)", 	polyHoleFlat, 14.143);
		// point above poly
		CheckWithinDistance(	"POINT (110 110 100)", 	polyHoleFlat, 100);
	}

    [TestAttribute]
    public void TestMultiPoint()
	{
		CheckWithinDistance(
				"MULTIPOINT ((0 0 0), (0 0 100), (100 100 100))",
				"MULTIPOINT ((100 100 99), (50 50 50), (25 100 33))",
				1
				);
	}

    [TestAttribute]
    public void TestMultiLineString()
	{
		CheckWithinDistance(
				"MULTILINESTRING ((0 0 0, 10 10 10), (0 0 100, 25 25 25, 40 40 50), (100 100 100, 100 101 102))",
				"MULTILINESTRING ((100 100 99, 100 100 99), (100 102 102, 200 200 20), (25 100 33, 25 100 35))",
				1
				);
	}

	[TestAttribute]
	public void TestMultiPolygon()
	{
		CheckWithinDistance(
				// Polygons parallel to XZ plane
				"MULTIPOLYGON ( ((120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10)), ((120 200 -10, 120 200 190, 180 200 190, 180 200 -10, 120 200 -10)) )",
				// Polygons parallel to XY plane
				"MULTIPOLYGON ( ((100 200 200, 200 200 200, 200 100 200, 100 100 200, 100 200 200)), ((100 200 210, 200 200 210, 200 100 210, 100 100 210, 100 200 210)) )",
				10
				);
	}
	
	
	//==========================================================
	// Convenience methods
	//==========================================================
	
	private void CheckWithinDistance(String wkt1, String wkt2, double distance)
	{
		CheckWithinDistance(wkt1, wkt2, distance, true);
	}
	
	private void CheckWithinDistance(String wkt1, String wkt2, double distance, bool expectedResult)
	{
		IGeometry g1;
		IGeometry g2;
		try {
			g1 = Rdr.Read(wkt1);
        }
        catch (GeoAPI.IO.ParseException e)
        {
			throw new Exception(e.Message);
		}
		try {
			g2 = Rdr.Read(wkt2);
		} catch (GeoAPI.IO.ParseException e) {
            throw new Exception(e.Message);
		}
		// check both orders for arguments
		CheckWithinDistance(g1, g2, distance, expectedResult);
		CheckWithinDistance(g2, g1, distance, expectedResult);
	}

	private static void CheckWithinDistance(IGeometry g1, IGeometry g2, double distance, bool expectedResult)
	{
		var isWithinDist = Distance3DOp.IsWithinDistance(g1, g2, distance);
		Assert.AreEqual(expectedResult, isWithinDist);
	}
	
}}