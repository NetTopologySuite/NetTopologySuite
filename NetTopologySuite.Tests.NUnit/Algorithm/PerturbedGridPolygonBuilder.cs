using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    ///<summary>
    /// Creates a perturbed, buffered grid and tests a set
    /// of points against using two PointInArea classes.
    ///</summary>
    /// <author>mbdavis</author>
    public class PerturbedGridPolygonBuilder
    {
        private readonly IGeometryFactory _geomFactory;
        private readonly IPrecisionModel _precisionModel;
        private const double GridWidth = 1000;
        private int _numLines = 10;
        private double _lineWidth = 20;

        private Int32 _seed;
        private Random _rand;

        private IGeometry _grid;

        public bool Verbose { get; set; }

        public PerturbedGridPolygonBuilder(IGeometryFactory geomFactory)
        {
            _geomFactory = geomFactory;
            _precisionModel = geomFactory.PrecisionModel;
            _seed = DateTime.Now.Millisecond;
        }

        public int Seed
        {
            get { return _seed; }
            set { _seed = value; }
        }

        public int NumLines
        {
            get { return _numLines; }
            set {_numLines = value;}
        }

        public double LineWidth
        {
            get { return _lineWidth; }
            set { _lineWidth = value; }
        }

        public IGeometry Geometry
        {
            get
            {
                if (_grid == null)
                    _grid = BuildGrid();
                return _grid;
            }
        }

        private IGeometry BuildGrid()
        {
            var lines = new ILineString[_numLines * 2];
            int index = 0;

            for (int i = 0; i < _numLines; i++)
            {
                Coordinate p0 = new Coordinate(GetRandOrdinate(), 0);
                Coordinate p1 = new Coordinate(GetRandOrdinate(), GridWidth);
                ILineString line = _geomFactory.CreateLineString(new [] { p0, p1 });
                lines[index++] = line;
            }

            for (int i = 0; i < _numLines; i++)
            {
                Coordinate p0 = new Coordinate(0, GetRandOrdinate());
                Coordinate p1 = new Coordinate(GridWidth, GetRandOrdinate());
                ILineString line = _geomFactory.CreateLineString(new [] { p0, p1 });
                lines[index++] = line;
            }

            IMultiLineString ml = _geomFactory.CreateMultiLineString(lines);
            _grid = ml.Buffer(_lineWidth);
            var wktWriter = new WKTWriter(2) {Formatted = true, MaxCoordinatesPerLine = 6};
            if (Verbose)
                Console.WriteLine(wktWriter.Write(_grid));
            return _grid;

        }

        private double GetRand()
        {
            if (_rand == null)
            {
                Console.WriteLine("Seed = " + _seed);
                _rand = new Random(_seed);
            }
            return _rand.NextDouble();
        }


        private double GetRandOrdinate()
        {
            double randNum = GetRand();
            double ord = _precisionModel.MakePrecise(randNum * GridWidth);
            return ord;
        }
    }
}