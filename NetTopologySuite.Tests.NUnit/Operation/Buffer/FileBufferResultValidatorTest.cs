using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using NetTopologySuite.Operation.Buffer.Validate;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    [TestFixture]
    public class FileBufferResultValidatorTest
    {
        [Test]
public void testAfrica()
  {
//    runTest(TestFiles.DATA_DIR + "world.wkt");
    //RunTest(TestFiles.DATA_DIR + "africa.wkt");
  }
  
  void RunTest(String filename)
  {
      IWktGeometryReader<Coordinate> fileRdr = GeometryUtils.GeometryFactory.WktReader;
    List<IGeometry<Coordinate>> polys = new List<IGeometry<Coordinate>>(fileRdr.ReadAll(new StreamReader(File.OpenRead(filename))));
    
    RunAll(polys, 0.01);
    RunAll(polys, 0.1);
    RunAll(polys, 1.0);
    RunAll(polys, 10.0);
    RunAll(polys, 100.0);
    RunAll(polys, 1000.0);
    
  }
  
  void RunAll(List<IGeometry<Coordinate>> geoms, double dist)
  {
  	Stopwatch sw = new Stopwatch();
    Console.WriteLine("Geom count = " + geoms.Count + "   distance = " + dist);
    foreach (IGeometry<Coordinate> g in geoms ) {
      RunBuffer(g, dist);
      //RunBuffer(g.Reverse(), dist);
      Console.Write(".");
    }
    Console.WriteLine(string.Format(" {0} ms" , sw.ElapsedMilliseconds));

  }
  void RunBuffer(IGeometry<Coordinate> g, double dist)
  {
  	IGeometry<Coordinate> buf = g.Buffer(dist);
    BufferResultValidator<Coordinate> validator = new BufferResultValidator<Coordinate>(g, dist, buf);
    
    if (! validator.IsValid()) {
      String msg = validator.ErrorMessage;

      Console.WriteLine(msg);
      Console.WriteLine(validator.ErrorLocation.ToString());
      Console.WriteLine(g);
    }
  	Assert.IsTrue(validator.IsValid());
  }        
    }
}