using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Distance3D;
//using ParseException = GeoAPI.IO.ParseException;

namespace NetTopologySuite.Tests.NUnit.Operation.Distance3d
{
public class Distance3DOpTest
{
	static readonly WKTReader Rdr = new WKTReader();
	
	/*
	public void testTest()
	{
		checkDistance(	"LINESTRING (250 250 0, 260 260 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				70.71067811865476);	
		
		testLinePolygonFlat();
	}
	*/
	
    [TestAttribute]
	public void TestEmpty()
	{
		CheckDistance(	"POINT EMPTY", "POINT EMPTY",	0);
		CheckDistance(	"LINESTRING EMPTY", "POINT (0 0 0)",	0);
		CheckDistance(	"MULTILINESTRING EMPTY", "POLYGON EMPTY",	0);
		CheckDistance(	"MULTIPOLYGON EMPTY", "POINT (0 0 0)",	0);
	}
	
    [TestAttribute]
	public void TestPartiallyEmpty()
	{
		CheckDistance(	"GEOMETRYCOLLECTION( MULTIPOINT (0 0 0), POLYGON EMPTY)", "POINT (0 1 0)",	1);
		CheckDistance(	"GEOMETRYCOLLECTION( MULTIPOINT (11 11 0), POLYGON EMPTY)", 
				"GEOMETRYCOLLECTION( MULTIPOINT EMPTY, LINESTRING (10 10 0, 10 20 0 ))",	
				1);
	}
	
	[TestAttribute]
    public void TestPointPointFlat() {
		CheckDistance(	"POINT (10 10 0 )",
				"POINT (20 20 0 )",
				14.1421356);
		CheckDistance(	"POINT (10 10 0 )",
				"POINT (20 20 0 )",
				14.1421356);
	}
	
    [TestAttribute]
	public void TestPointPoint() {
		CheckDistance(	"POINT (0 0 0 )",
						"POINT (0 0 1 )",
				1);
		CheckDistance(	"POINT (10 10 1 )",
				"POINT (11 11 2 )",
				1.7320508075688772);
		CheckDistance(	"POINT (10 10 0 )",
				"POINT (10 20 10 )",
				14.142135623730951);
	}
	
    [TestAttribute]
	public void TestPointSegFlat() {
		CheckDistance(	"LINESTRING (10 10 0, 10 20 0 )",
				"POINT (20 15 0 )",
				10);
	}
	
    [TestAttribute]
	public void TestPointSeg() {
		CheckDistance(	"LINESTRING (0 0 0, 10 10 10 )",
				"POINT (5 5 5 )",
				0);
		CheckDistance(	"LINESTRING (10 10 10, 20 20 20 )",
				"POINT (11 11 10 )",
				0.816496580927726);
	}
	
    [TestAttribute]
	public void TestPointSegRobust() {
		CheckDistance(	"LINESTRING (0 0 0, 10000000 10000000 1 )",
				"POINT (9999999 9999999 .9999999 )",
				0 );
		CheckDistance(	"LINESTRING (0 0 0, 10000000 10000000 1 )",
				"POINT (5000000 5000000 .5 )",
				0 );
	}
	
    [TestAttribute]
	public void TestCrossSegmentsFlat() {
		CheckDistance(	"LINESTRING (0 0 0, 10 10 0 )",
				"LINESTRING (10 0 0, 0 10 0 )",
		0);
		CheckDistance(	"LINESTRING (0 0 10, 30 10 10 )",
				"LINESTRING (10 0 10, 0 10 10 )",
		0);
	}
	
    [TestAttribute]
	public void TestCrossSegments() {
		CheckDistance(	"LINESTRING (0 0 0, 10 10 0 )",
				"LINESTRING (10 0 1, 0 10 1 )",
		1);
		CheckDistance(	"LINESTRING (0 0 0, 20 20 0 )",
				"LINESTRING (10 0 1, 0 10 1 )",
		1);
		CheckDistance(	"LINESTRING (20 10 20, 10 20 10 )",
				"LINESTRING (10 10 20, 20 20 10 )",
		0);
	}
	
	/**
	 * Many of these tests exhibit robustness errors 
	 * due to numerical roundoff in the distance algorithm mathematics.
	 * This happens when computing nearly-coincident lines 
	 * with very large ordinate values
	 */
    [TestAttribute]
	public void TestCrossSegmentsRobust() {
		CheckDistance(	"LINESTRING (0 0 0, 10000000 10000000 1 )",
				"LINESTRING (0 0 1, 10000000 10000000 0 )",
				0, 0.001);  // expected is 0, but actual is larger
		
		CheckDistance(	"LINESTRING (-10000 -10000 0, 10000 10000 1 )",
				"LINESTRING (-10000 -10000 1, 10000 10000 0 )",
				0);
		
		// previous case with X,Y scaled by 1000 - exposes robustness issue
		CheckDistance(	"LINESTRING (-10000000 -10000000 0, 10000000 10000000 1 )",
				"LINESTRING (-10000000 -10000000 1, 10000000 10000000 0 )",
				0, 0.02);  // expected is 0, but actual is larger
		
		// works because lines are orthogonal, so doesn't hit roundoff problems
		CheckDistance(	"LINESTRING (20000000 10000000 20, 10000000 20000000 10 )",
				"LINESTRING (10000000 10000000 20, 20000000 20000000 10 )",
				0);
	}
	
    [TestAttribute]
	public void TestTSegmentsFlat() {
		CheckDistance(	"LINESTRING (10 10 0, 10 20 0 )",
						"LINESTRING (20 15 0, 25 15 0 )",
				10);
	}
	
    [TestAttribute]
	public void TestParallelSegmentsFlat() {
		CheckDistance(	"LINESTRING (10 10 0, 20 20 0 )",
						"LINESTRING (10 20 0, 20 30 0 )",
						7.0710678118654755);
	}
	
    [TestAttribute]
	public void TestParallelSegments() {
		CheckDistance(	"LINESTRING (0 0 0, 1 0 0 )",
						"LINESTRING (0 0 1, 1 0 1 )",
						1);
		CheckDistance(	"LINESTRING (10 10 0, 20 10 0 )",
				"LINESTRING (10 20 10, 20 20 10 )",
				14.142135623730951);
		CheckDistance(	"LINESTRING (10 10 0, 20 20 0 )",
				"LINESTRING (10 20 10, 20 30 10 )",
				12.24744871391589 );
				// = distance from LINESTRING (10 10 0, 20 20 0 ) to POINT(10 20 10)
				// = hypotenuse(7.0710678118654755, 10)
	}
	
    [TestAttribute]
	public void TestLineLine()
	{
		CheckDistance(	"LINESTRING (0 1 2, 1 1 1, 1 0 2 )",
				"LINESTRING (0 0 0.1, .5 .5 0, 1 1 0, 1.5 1.5 0, 2 2 0 )",
				1);		
		CheckDistance(	"LINESTRING (10 10 20, 20 20 30, 20 20 1, 30 30 5 )",
				"LINESTRING (1 80 10, 0 39 5, 39 0 5, 80 1 20)",
				0.7071067811865476);		
	}
	
    [TestAttribute]
	public void TestPointPolygon()
	{
		// point above poly
		CheckDistance(	"POINT (150 150 10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);	
		// point below poly
		CheckDistance(	"POINT (150 150 -10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);				
		// point right of poly in YZ plane
		CheckDistance(	"POINT (10 150 150)",
				"POLYGON ((0 100 200, 0 200 200, 0 200 100, 0 100 100, 0 100 200))",
				10);				
	}
	
    [TestAttribute]
	public void TestPointPolygonFlat()
	{
		// inside
		CheckDistance(	"POINT (150 150 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// outside
		CheckDistance(	"POINT (250 250 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				70.71067811865476);				
		// on
		CheckDistance(	"POINT (200 200 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}
	
    [TestAttribute]
	public void TestLinePolygonFlat()
	{
		// line inside
		CheckDistance(	"LINESTRING (150 150 0, 160 160 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// line outside
		CheckDistance(	"LINESTRING (200 250 0, 260 260 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				50);	
		// line touching
		CheckDistance(	"LINESTRING (200 200 0, 260 260 0)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}
	
    [TestAttribute]
	public void TestLinePolygonSimple()
	{
		// line crossing inside
		CheckDistance(	"LINESTRING (150 150 10, 150 150 -10)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);	
		// vertical line above
		CheckDistance(	"LINESTRING (200 200 10, 260 260 100)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				10);	
		// vertical line touching
		CheckDistance(	"LINESTRING (200 200 0, 260 260 100)",
				"POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0))",
				0);				
	}
	
	const String PolyHoleFlat = "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0), (120 180 0, 180 180 0, 180 120 0, 120 120 0, 120 180 0))";

    [TestAttribute]
	public void TestLinePolygonHoleFlat()
	{
		// line crossing hole
		CheckDistance(	"LINESTRING (150 150 10, 150 150 -10)", 	PolyHoleFlat, 30);	
		// line crossing interior
		CheckDistance(	"LINESTRING (110 110 10, 110 110 -10)",		PolyHoleFlat, 0);	
		// vertical line above hole
		CheckDistance(	"LINESTRING (130 130 10, 150 150 100)",		PolyHoleFlat, 14.14213562373095);	
		// vertical line touching hole
		CheckDistance(	"LINESTRING (120 180 0, 120 180 100)",		PolyHoleFlat, 0);				
	}
	
    [TestAttribute]
	public void TestPointPolygonHoleFlat()
	{
		// point above poly hole
		CheckDistance(	"POINT (130 130 10)", 	PolyHoleFlat, 14.14213562373095);	
		// point below poly hole
		CheckDistance(	"POINT (130 130 -10)", 	PolyHoleFlat, 14.14213562373095);
		// point above poly
		CheckDistance(	"POINT (110 110 100)", 	PolyHoleFlat, 100);
	}
	
	const String Poly2HoleFlat = "POLYGON ((100 200 0, 200 200 0, 200 100 0, 100 100 0, 100 200 0), (110 110 0, 110 130 0, 130 130 0, 130 110 0, 110 110 0), (190 110 0, 170 110 0, 170 130 0, 190 130 0, 190 110 0))";	

	/**
	 * A case proving that polygon/polygon distance requires 
	 * computing distance between all rings, not just the shells.
	 */
    [TestAttribute]
	public void TestPolygonPolygonLinkedThruHoles()
	{
		// note distance is zero!
		CheckDistance(	
				// polygon with two holes
				Poly2HoleFlat,
				// polygon parallel to XZ plane with shell passing through holes in other polygon
				"POLYGON ((120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10))", 
				0);	
		
		// confirm that distance of simple poly boundary is non-zero
		CheckDistance(	
				// polygon with two holes
				Poly2HoleFlat,
				// boundary of polygon parallel to XZ plane with shell passing through holes
				"LINESTRING (120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10)", 
				10);	
	}

	
    [TestAttribute]
	public void TestMultiPoint()
	{
		CheckDistance(
				"MULTIPOINT ((0 0 0), (0 0 100), (100 100 100))",
				"MULTIPOINT ((100 100 99), (50 50 50), (25 100 33))",
				1
				);
	}
	
    [TestAttribute]
	public void TestMultiLineString()
	{
		CheckDistance(
				"MULTILINESTRING ((0 0 0, 10 10 10), (0 0 100, 25 25 25, 40 40 50), (100 100 100, 100 101 102))",
				"MULTILINESTRING ((100 100 99, 100 100 99), (100 102 102, 200 200 20), (25 100 33, 25 100 35))",
				1
				);
	}
	
    [TestAttribute]
	public void TestMultiPolygon()
	{
		CheckDistance(
				// Polygons parallel to XZ plane
				"MULTIPOLYGON ( ((120 120 -10, 120 120 100, 180 120 100, 180 120 -10, 120 120 -10)), ((120 200 -10, 120 200 190, 180 200 190, 180 200 -10, 120 200 -10)) )",
				// Polygons parallel to XY plane
				"MULTIPOLYGON ( ((100 200 200, 200 200 200, 200 100 200, 100 100 200, 100 200 200)), ((100 200 210, 200 200 210, 200 100 210, 100 100 210, 100 200 210)) )",
				10
				);
	}
	
    [TestAttribute]
	public void TestMultiMixed()
	{
		CheckDistance(
				"MULTILINESTRING ((0 0 0, 10 10 10), (0 0 100, 25 25 25, 40 40 50), (100 100 100, 100 101 101))",
				"MULTIPOINT ((100 100 99), (50 50 50), (25 100 33))",
				1
				);
	}
	
	//==========================================================
	// Convenience methods
	//==========================================================

    private const double DistanceTolerance = 0.00001;

    private void CheckDistance(String wkt1, String wkt2, double expectedDistance)
	{
		CheckDistance(wkt1, wkt2, expectedDistance, DistanceTolerance);
	}

	private void CheckDistance(String wkt1, String wkt2, double expectedDistance, double tolerance)
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
        }
        catch (GeoAPI.IO.ParseException e)
        {
			throw new Exception(e.Message);
		}
		// check both orders for arguments
		CheckDistance(g1, g2, expectedDistance, tolerance);
		CheckDistance(g2, g1, expectedDistance, tolerance);
	}

	private void CheckDistance(IGeometry g1, IGeometry g2, double expectedDistance, double tolerance)
	{
		var distOp = new Distance3DOp(g1, g2);
		var dist = distOp.Distance();
		Assert.AreEqual(expectedDistance, dist, tolerance);
	}
	
}}