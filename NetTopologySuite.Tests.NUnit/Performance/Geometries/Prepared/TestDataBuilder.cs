using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
public class TestDataBuilder 
{
  private IGeometryFactory _geomFact = new GeometryFactory();

	private Coordinate _origin = new Coordinate(0, 0); 
	private double _size = 100.0;
	private int _testDim = 1;
	
	public TestDataBuilder(IGeometryFactory geomFact)
	{
		_geomFact = geomFact;
	}
	

	public void SetExtent(Coordinate origin, double size)
	{
		_origin = origin;
		_size = size;
	}
	
	public int TestDimension { get { return _testDim; } set { _testDim = value; }}
	
	public IGeometry CreateCircle(int nPts) {
		var gsf = new GeometricShapeFactory(_geomFact);
		gsf.Centre = _origin;
		gsf.Size = _size;
		gsf.NumPoints = nPts;
		var circle = gsf.CreateCircle();
		// var gRect = gsf.CreateRectangle();
		// var g = gRect.ExteriorRing);
		return circle;
	}
  
  public IGeometry CreateSineStar(int nPts) {
		var gsf = new SineStarFactory(_geomFact);
		gsf.Centre = _origin;
		gsf.Size = _size;
		gsf.NumPoints = nPts;
		gsf.ArmLengthRatio = 0.1;
		gsf.NumArms = 20;
		var poly = gsf.CreateSineStar();
		return poly;
	}
  
  public IList<IGeometry> CreateTestGeoms(Envelope env, int nItems, double size, int nPts)
  {
    int nCells = (int) Math.Sqrt(nItems);

  	var geoms = new List<IGeometry>();
  	double width = env.Width;
  	double xInc = width / nCells;
  	double yInc = width / nCells;
  	for (int i = 0; i < nCells; i++) {
    	for (int j = 0; j < nCells; j++) {
    		Coordinate @base = new Coordinate(
    				env.MinX + i * xInc,
    				env.MinY + j * yInc);
    		var line = CreateLine(@base, size, nPts);
    		geoms.Add(line);
    	}
  	}
  	return geoms;
  }
  
  public IGeometry CreateLine(Coordinate @base, double size, int nPts)
  {
    var gsf = new GeometricShapeFactory();
    gsf.Centre = _origin;
    gsf.Size = size;
    gsf.NumPoints = nPts;
    var circle = gsf.CreateCircle();
//    System.out.println(circle);
    return circle.Boundary;
  }
  

}
}