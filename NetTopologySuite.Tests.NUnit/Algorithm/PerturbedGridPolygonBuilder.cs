using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Coordinates;
using Coord = NetTopologySuite.Coordinates.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class PerturbedGridPolygonBuilder
    {
	private readonly IGeometryFactory<Coord> _geomFactory;
        private readonly ICoordinateFactory<Coord> _coordFactory;
	private const double GridWidth = 1000;
	private int _numLines = 10;
	private double _lineWidth = 20;
		
	private Int32 _seed = 0; 
	private Random _rand;
	
	private IGeometry<Coord> _grid;
	
	public PerturbedGridPolygonBuilder(IGeometryFactory<Coord> geomFactory)
	{
		_geomFactory = geomFactory;
	    _coordFactory = geomFactory.CoordinateFactory;
		_seed = DateTime.Now.Millisecond;
	}
	
	public void SetSeed(Int32 seed)
	{
		this._seed = seed;
	}
	
	public void SetNumLines(int numLines)
	{
		this._numLines = numLines;
	}
	
	public void SetLineWidth(double lineWidth)
	{
		this._lineWidth = lineWidth;
	}
	
	public IGeometry<Coord> Geometry
	{
		get {
        if (_grid == null)
			_grid = buildGrid();
		return _grid;}
	}
	
	private IGeometry<Coord> buildGrid()
	{
		ILineString<Coord>[] lines = new ILineString<Coord>[_numLines * 2];
		int index = 0;
		
		for (int i = 0; i < _numLines; i++) {
			Coord p0 = _coordFactory.Create(GetRandOrdinate(), 0);
			Coord p1 = _coordFactory.Create(GetRandOrdinate(), GridWidth);
			ILineString<Coord> line = _geomFactory.CreateLineString(
					new Coordinate[] { p0, p1 });
			lines[index++] = line;
		}
		
		for (int i = 0; i < _numLines; i++) {
			Coord p0 = _coordFactory.Create(0, GetRandOrdinate());
			Coord p1 = _coordFactory.Create(GridWidth, GetRandOrdinate());
			ILineString<Coordinate> line = _geomFactory.CreateLineString(
					new Coordinate[] { p0, p1 });
			lines[index++] = line;
		}
		
		IMultiLineString<Coordinate> ml = _geomFactory.CreateMultiLineString(lines);
		IGeometry<Coordinate> _grid = ml.Buffer(_lineWidth);
		Console.WriteLine(_grid);
		return _grid;
		
	}
	
	private double getRand()
	{
		if (_rand == null) {
			Console.WriteLine("Seed = " + _seed);
			_rand = new Random(_seed);
		}
		return _rand.NextDouble();
	}
	

	private double GetRandOrdinate()
	{
		double randNum = getRand();
	    double ord = _coordFactory.PrecisionModel.MakePrecise(randNum*GridWidth);
		return ord;
	}
    }
}