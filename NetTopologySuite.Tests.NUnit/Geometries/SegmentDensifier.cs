using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
    * Densifies a LineString
    */
    public class SegmentDensifier
    {
        private LineString inputLine;
        private CoordinateList newCoords;

        public SegmentDensifier(LineString line)
        {
            this.inputLine = line;
        }

        public IGeometry Densify(double segLength)
        {
            newCoords = new CoordinateList();

            ICoordinateSequence seq = inputLine.CoordinateSequence;

            Coordinate p0 = new Coordinate();
            Coordinate p1 = new Coordinate();
            seq.GetCoordinate(0, p0);
            newCoords.Add(new Coordinate(p0));

            for (int i = 0; i < seq.Count - 1; i++)
            {
                seq.GetCoordinate(i, p0);
                seq.GetCoordinate(i + 1, p1);
                Densify(p0, p1, segLength);
            }
            Coordinate[] newPts = newCoords.ToCoordinateArray();
            return inputLine.Factory.CreateLineString(newPts);
        }

        private void Densify(Coordinate p0, Coordinate p1, double segLength)
        {
            double origLen = p1.Distance(p0);
            int nPtsToAdd = (int)Math.Floor(origLen / segLength);

            double delx = p1.X - p0.X;
            double dely = p1.Y - p0.Y;

            double segLenFrac = segLength / origLen;
            for (int i = 0; i <= nPtsToAdd; i++)
            {
                double addedPtFrac = i * segLenFrac;
                Coordinate pt = new Coordinate(p0.X + addedPtFrac * delx,
                                                p0.Y + addedPtFrac * dely);
                newCoords.Add(pt, false);
            }
            newCoords.Add(new Coordinate(p1), false);
        }
    }
}