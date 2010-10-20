using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Triangulate;
using NUnit.Framework;
using coord = NetTopologySuite.Coordinates.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
[TestFixture]
public class VoronoiPerfTest 
{
  [Test]
	public void Run()
	{
		run(10);
		run(100);
		run(1000);
		run(10000);
		run(100000);
        //the following does not work reasonably
		//run(1000000);
	}
	
	
	const double SIDE_LEN = 10.0;
	
	public void run(int nPts)
	{
		Stopwatch sw = new Stopwatch();
        sw.Start();
		DelaunayTriangulationBuilder<coord> builder = new DelaunayTriangulationBuilder<coord>(GeometryUtils.GeometryFactory);
		builder.SetSites(randomPoints(nPts));
		
		IGeometry<coord> g = builder.GetEdges();
		Console.WriteLine("# pts: ~" + builder + "  --  " + sw.ElapsedMilliseconds);
//		System.out.println(g);
	}
	
	IEnumerable<coord> randomPoints(int nPts)
	{
		int nSide = (int) Math.Sqrt(nPts) + 1;
		
        Random r = new Random(0);
		for (int i = 0; i < nSide; i++) {
			for (int j = 0; j < nSide; j++) {
				double x = i * SIDE_LEN + SIDE_LEN * r.NextDouble();
				double y = j * SIDE_LEN + SIDE_LEN * r.NextDouble();
				yield return(GeometryUtils.CoordFac.Create(x, y));
			}
		}
	}
}
}