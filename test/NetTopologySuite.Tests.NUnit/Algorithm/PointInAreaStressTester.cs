using System;
using System.Diagnostics;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class PointInAreaStressTester
    {
        private readonly GeometryFactory _geomFactory;
        private readonly Geometry _area;
        private bool _ignoreBoundaryResults = true;

        private int _numPts = 10000;
        private IPointOnGeometryLocator _pia1;
        private IPointOnGeometryLocator _pia2;
        private readonly int[] _locationCount = new int[3];

        public PointInAreaStressTester(GeometryFactory geomFactory, Geometry area)
        {
            _geomFactory = geomFactory;
            _area = area;

        }

        public int NumPoints
        {
            get => _numPts;
            set => _numPts = value;
        }

        public IPointOnGeometryLocator TestPointInAreaLocator
        {
            get => _pia1;
            set => _pia1 = value;
        }

        public IPointOnGeometryLocator ExpectedPointInAreaLocator
        {
            get => _pia2;
            set => _pia2 = value;
        }

        public bool IgnoreBoundaryResults
        {
            get => _ignoreBoundaryResults;
            set => _ignoreBoundaryResults = value;
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <returns> true if all point locations were computed correctly</returns>
        public bool Run()
        {
            var sw = new Stopwatch();
            sw.Start();
            // default is to use the simple, non-indexed tester
            if (_pia2 == null)
                _pia2 = new SimplePointInAreaLocator(_area);

            int ptGridWidth = (int)Math.Sqrt(_numPts);

            var areaEnv = _area.EnvelopeInternal;
            double xStep = areaEnv.Width / (ptGridWidth - 1);
            double yStep = areaEnv.Height / (ptGridWidth - 1);

            for (int i = 0; i < ptGridWidth; i++)
            {
                for (int j = 0; j < ptGridWidth; j++)
                {

                    // compute test point
                    double x = areaEnv.MinX + i * xStep;
                    double y = areaEnv.MinY + j * yStep;
                    var pt = new Coordinate(x, y);
                    _geomFactory.PrecisionModel.MakePrecise(pt);

                    bool isEqual = TestPointInArea(pt);
                    if (!isEqual)
                        return false;
                }
            }
            sw.Stop();
            Console.WriteLine("Test completed in " + sw.ElapsedMilliseconds);
            PrintStats();
            return true;
        }

        public void PrintStats()
        {
            Console.WriteLine("Location counts: "
                              + "\nBoundary = " + _locationCount[(int)Location.Boundary]
                              + "\nInterior = " + _locationCount[(int)Location.Interior]
                              + "\nExterior = " + _locationCount[(int)Location.Exterior]
                );
        }

        /// <summary>
        /// TestPointInArea
        /// </summary>
        /// <param name="p"></param>
        /// <returns>true if the point location is determined to be the same by both PIA locators</returns>
        private bool TestPointInArea(Coordinate p)
        {
            //Console.WriteLine(WKTWriter.toPoint(p));

            var loc1 = _pia1.Locate(p);
            var loc2 = _pia2.Locate(p);

            _locationCount[(int)loc1]++;

            if ((loc1 == Location.Boundary || loc2 == Location.Boundary)
                && IgnoreBoundaryResults)
                return true;

            return loc1 == loc2;
        }

    }
}