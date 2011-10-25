using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
public class CreateRandomGeometryFunctions {

    private static Random RND = new Random();

  public static IGeometry randomPointsInGrid(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);

    int nCell = (int) Math.Sqrt(nPts) + 1;

    double xLen = env.Width / nCell;
    double yLen = env.Height / nCell;

    var pts = new List<IPoint>();

    for (int i = 0; i < nCell; i++) {
      for (int j = 0; j < nCell; j++) {
        double x = env.MinX + i * xLen + xLen * RND.NextDouble();
        double y = env.MinY + j * yLen + yLen * RND.NextDouble();
        pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
      }
    }
    return geomFact.BuildGeometry(pts.ToArray());
  }

  public static IGeometry randomPoints(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);
    double xLen = env.Width;
    double yLen = env.Height;

    var pts = new List<IPoint>();

    for (int i = 0; i < nPts; i++) {
      double x = env.MinX + xLen * RND.NextDouble();
      double y = env.MinY + yLen * RND.NextDouble();
      pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
    }
    return geomFact.BuildGeometry(pts.ToArray());
  }

  public static IGeometry randomRadialPoints(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);
    double xLen = env.Width;
    double yLen = env.Height;
    double rMax = Math.Min(xLen, yLen) / 2.0;
    
    double centreX = env.MinX + xLen/2;
    double centreY = env.MinY + yLen/2;
    
    var pts = new List<IPoint>();

    for (int i = 0; i < nPts; i++) {
      double rand = RND.NextDouble();
      double r = rMax * rand * rand;
      double ang = 2 * Math.PI * RND.NextDouble();
      double x = centreX + r * Math.Cos(ang);
      double y = centreY + r * Math.Sin(ang);
      pts.Add(geomFact.CreatePoint(new Coordinate(x, y)));
    }
    return geomFact.BuildGeometry(pts.ToArray());
  }

  public static IGeometry randomSegments(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);
    double xLen = env.Width;
    double yLen = env.Height;

    var lines = new List<IGeometry>();

    for (int i = 0; i < nPts; i++) {
        double x0 = env.MinX + xLen * RND.NextDouble();
        double y0 = env.MinY + yLen * RND.NextDouble();
        double x1 = env.MinX + xLen * RND.NextDouble();
        double y1 = env.MinY + yLen * RND.NextDouble();
      lines.Add(geomFact.CreateLineString(new [] {
          new Coordinate(x0, y0), new Coordinate(x1, y1) }));
    }
    return geomFact.BuildGeometry(lines);
  }

  public static IGeometry randomSegmentsInGrid(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);

    int nCell = (int) Math.Sqrt(nPts) + 1;

    double xLen = env.Width / nCell;
    double yLen = env.Height / nCell;

    var lines = new List<IGeometry>();

    for (int i = 0; i < nCell; i++) {
      for (int j = 0; j < nCell; j++) {
          double x0 = env.MinX + i * xLen + xLen * RND.NextDouble();
          double y0 = env.MinY + j * yLen + yLen * RND.NextDouble();
          double x1 = env.MinX + i * xLen + xLen * RND.NextDouble();
          double y1 = env.MinY + j * yLen + yLen * RND.NextDouble();
        lines.Add(geomFact.CreateLineString(new [] {
            new Coordinate(x0, y0), new Coordinate(x1, y1) }));
      }
    }
    return geomFact.BuildGeometry(lines);
  }

  public static IGeometry randomLineString(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);
    double width = env.Width;
    double hgt = env.Height;

    var pts = new Coordinate[nPts];

    for (int i = 0; i < nPts; i++) {
        double xLen = width * RND.NextDouble();
        double yLen = hgt * RND.NextDouble();
      pts[i] = randomPtAround(env.Centre, xLen, yLen);
    }
    return geomFact.CreateLineString(pts);
  }

  public static IGeometry randomRectilinearWalk(IGeometry g, int nPts) {
    var env = FunctionsUtil.getEnvelopeOrDefault(g);
    var geomFact = FunctionsUtil.getFactoryOrDefault(g);
    double xLen = env.Width;
    double yLen = env.Height;

    var pts = new Coordinate[nPts];

    bool xory = true;
    for (int i = 0; i < nPts; i++) {
      Coordinate pt = null;
      if (i == 0) {
       pt = randomPtAround(env.Centre, xLen, yLen);
      }
      else {
          double dist = xLen * (RND.NextDouble() - 0.5);
        double x = pts[i-1].X;
        double y = pts[i-1].Y;
        if (xory) {
          x += dist;
        }
        else {
          y += dist;
        }
        // switch orientation
        xory = ! xory;
        pt = new Coordinate(x, y);
      }
      pts[i] = pt;
    }
    return geomFact.CreateLineString(pts);
  }

  private static int randomQuadrant(int exclude)
  {
    while (true) {
        int quad = (int)(RND.NextDouble() * 4);
      if (quad > 3) quad = 3;
      if (quad != exclude) return quad;
    }
  }
  
  private static Coordinate randomPtAround(Coordinate basePt, double xLen, double yLen)
  {
      double x0 = basePt.X + xLen * (RND.NextDouble() - 0.5);
      double y0 = basePt.Y + yLen * (RND.NextDouble() - 0.5);
    return new Coordinate(x0, y0);    
  }

}
}