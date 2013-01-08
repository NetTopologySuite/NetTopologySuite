using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    public class PointInAreaStressTester
    {
        private readonly IGeometryFactory _geomFactory;
        private readonly IGeometry _area;
        private Boolean _ignoreBoundaryResults = true;

        private int _numPts = 10000;
        private IPointOnGeometryLocator _pia1;
        private IPointOnGeometryLocator _pia2;
        private readonly int[] _locationCount = new int[3];

        public PointInAreaStressTester(IGeometryFactory geomFactory, IGeometry area)
        {
            _geomFactory = geomFactory;
            _area = area;

        }

        public int NumPoints
        {
            get { return _numPts; }
            set { _numPts = value;}
        }

        public IPointOnGeometryLocator TestPointInAreaLocator
        {
            get { return _pia1; }
            set { _pia1 = value; }
        }

        public IPointOnGeometryLocator ExpectedPointInAreaLocator
        {
            get { return _pia2; }
            set { _pia2 = value; }
        }

        public Boolean IgnoreBoundaryResults
        {
            get { return _ignoreBoundaryResults; }
            set { _ignoreBoundaryResults = value; }
        }

        /// <summary>
        /// Run
        /// </summary>
        /// <returns> true if all point locations were computed correctly</returns>
        public Boolean Run()
        {
            var sw = new Stopwatch();
            sw.Start();
            // default is to use the simple, non-indexed tester
            if (_pia2 == null)
                _pia2 = new SimplePointInAreaLocator(_area);


            int ptGridWidth = (int)Math.Sqrt(_numPts);

            Envelope areaEnv = _area.EnvelopeInternal;
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

                    Boolean isEqual = TestPointInArea(pt);
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
                              + "\nBoundary = " + _locationCount[(Int32)Location.Boundary]
                              + "\nInterior = " + _locationCount[(Int32)Location.Interior]
                              + "\nExterior = " + _locationCount[(Int32)Location.Exterior]
                );
        }

        /// <summary>
        /// TestPointInArea
        /// </summary>
        /// <param name="p"></param>
        /// <returns>true if the point location is determined to be the same by both PIA locaters</returns>
        private Boolean TestPointInArea(Coordinate p)
        {
            //Console.WriteLine(WKTWriter.toPoint(p));

            Location loc1 = _pia1.Locate(p);
            Location loc2 = _pia2.Locate(p);

            _locationCount[(Int32)loc1]++;

            if ((loc1 == Location.Boundary || loc2 == Location.Boundary)
                && IgnoreBoundaryResults)
                return true;

            return loc1 == loc2;
        }

    }
}