using System;
using System.Collections;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

//TODO: ToddJ - Comeback to how to do stress tests as unit tests

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class MinimumBoundingCircleStressTest 
    {
        //GeometryFactory geomFact = new GeometryFactory();
	

        //void run()
        //{
        //while (true) {
        //int n = (int) ( 10000 * Math.random());
        //run(n);
        //}
        //}
  
        //void run(int nPts)
        //{
        //Coordinate[] randPts = createRandomPoints(nPts);
        //Geometry mp = geomFact.createMultiPoint(randPts);
        //MinimumBoundingCircle mbc = new MinimumBoundingCircle(mp);
        //Coordinate centre = mbc.getCentre();
        //double radius = mbc.getRadius();
        //System.out.println("Testing " + nPts + " random points.  Radius = " + radius);
  	
        //checkWithinCircle(randPts, centre, radius, 0.0001);
        //}
  
        //void checkWithinCircle(Coordinate[] pts, Coordinate centre, double radius, double tolerance)
        //{
        //for (int i = 0; i < pts.Length; i++ ) {
        //Coordinate p = pts[i];
        //double ptRadius = centre.Distance(p);
        //double error = ptRadius - radius;
        //if (error > tolerance) {
        //    Assert.shouldNeverReachHere();
        //}
        //}
        //}
        //Coordinate[] createRandomPoints(int n)
        //{
        //Coordinate[] pts = new Coordinate[n];
        //for(int i = 0; i < n; i++) {
        //double x = 100 * Math.random();
        //double y = 100 * Math.random();
        //pts[i] = new Coordinate(x, y);
        //}
        //return pts;
        //}

    }
}
