using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Coordinates.Simple;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /**
     * Creates a perturbed, buffered grid and tests a set
     * of points against using two PointInArea classes.
     * 
     * @author mbdavis
     *
     */
    public class PointInAreaStressTester
    {
        private IGeometryFactory<Coordinate> geomFactory;
        private IGeometry<Coordinate> area;
        private Boolean _ignoreBoundaryResults = true;

        private int numPts = 10000;
        private IPointOnGeometryLocator<Coordinate> pia1;
        private IPointOnGeometryLocator<Coordinate> pia2;
        private int[] locationCount = new int[3];

        public PointInAreaStressTester(IGeometryFactory<Coordinate> geomFactory, IGeometry<Coordinate> area)
        {
            this.geomFactory = geomFactory;
            this.area = area;

        }

        public void SetNumPoints(int numPoints)
        {
            this.numPts = numPoints;
        }


        public void setPIA(IPointOnGeometryLocator<Coordinate> pia)
        {
            this.pia1 = pia;
        }

        public void setExpected(IPointOnGeometryLocator<Coordinate> pia)
        {
            this.pia2 = pia;
        }

        public Boolean IgnoreBoundaryResults
        {
            get { return _ignoreBoundaryResults; }
            set { _ignoreBoundaryResults = value; }
        }

        /**
         * 
         * @return true if all point locations were computed correctly
         */
        public Boolean Run()
	{ 
		Stopwatch sw = new Stopwatch();
		
		// default is to use the simple, non-indexed tester
    if (pia2 == null)
      pia2 = new SimplePointInAreaLocator<Coordinate>(area);
		
		
		int ptGridWidth = (int) Math.Sqrt(numPts);
		
		IExtents<Coordinate> areaEnv = area.Extents;
		double xStep = areaEnv.GetSize(Ordinates.X) / (ptGridWidth - 1);
		double yStep = areaEnv.GetSize(Ordinates.Y) / (ptGridWidth - 1);

		for (int i = 0; i < ptGridWidth; i++) {
			for (int j = 0; j < ptGridWidth; j++) {
				
				// compute test point
				double x = areaEnv.Min[Ordinates.X] +  i * xStep;
				double y = areaEnv.Min[Ordinates.Y] + j * yStep;
				Coordinate pt = geomFactory.CoordinateFactory.Create(x, y);
				pt = geomFactory.PrecisionModel.MakePrecise(pt);
				
				Boolean isEqual = TestPIA(pt);
				if (! isEqual)
					return false;
			}
		}
		Console.WriteLine("Test completed in " + sw.Elapsed.ToString());
		PrintStats();
		return true;
	}

        public void PrintStats()
	{
        Console.WriteLine("Location counts: "
				+ " Boundary = "	+ locationCount[(Int32)Locations.Boundary]
                + " Interior = " + locationCount[(Int32)Locations.Interior]
                + " Exterior = " + locationCount[(Int32)Locations.Exterior]
				                );
	}
        /**
         * 
         * @param p
         * @return true if the point location is determined to be the same by both PIA locaters
         */
        private Boolean TestPIA(Coordinate p)
        {
            //Console.WriteLine(WKTWriter.toPoint(p));

            Locations loc1 = pia1.Locate(p);
            Locations loc2 = pia2.Locate(p);

            locationCount[(Int32)loc1]++;

            if ((loc1 == Locations.Boundary || loc2 == Locations.Boundary)
                && IgnoreBoundaryResults)
                return true;

            return loc1 == loc2;
        }

    }
}