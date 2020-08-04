using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class CreateShapeFunctions
    {
        private static readonly int DEFAULT_POINTSIZE = 100;

        public static Geometry Grid(Geometry g, int nCells)
        {
            var geoms = new List<Geometry>();

            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            int nCellsOnSide = (int) Math.Sqrt(nCells) + 1;
            double cellSizeX = env.Width / nCellsOnSide;
            double cellSizeY = env.Height / nCellsOnSide;

            for (int i = 0; i < nCellsOnSide; i++)
            {
                for (int j = 0; j < nCellsOnSide; j++)
                {
                    double x1 = env.MinX + i * cellSizeX;
                    double y1 = env.MinY + j * cellSizeY;
                    double x2 = env.MinX + (i + 1) * cellSizeX;
                    double y2 = env.MinY + (j + 1) * cellSizeY;
                    var cellEnv = new Envelope(x1, x2, y1, y2);

                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }

            return geomFact.BuildGeometry(geoms);
        }

        public static Geometry GridPoints(Geometry g, int nCells)
        {
            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            int nCellsOnSideY = (int) Math.Sqrt(nCells);
            int nCellsOnSideX = nCells / nCellsOnSideY;

            double cellSizeX = env.Width / (nCellsOnSideX - 1);
            double cellSizeY = env.Height / (nCellsOnSideY - 1);

            var pts = new CoordinateList();

            for (int i = 0; i < nCellsOnSideX; i++)
            {
                for (int j = 0; j < nCellsOnSideY; j++)
                {
                    double x = env.MinX + i * cellSizeX;
                    double y = env.MinY + j * cellSizeY;

                    pts.Add(new Coordinate(x, y));
                }
            }

            return geomFact.CreateMultiPointFromCoords(pts.ToCoordinateArray());
        }

        public static Geometry Supercircle3(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 3);
        }

        public static Geometry Squircle(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 4);
        }

        public static Geometry Supercircle5(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 5);
        }

        public static Geometry SupercirclePoint5(Geometry g, int nPts)
        {
            return Supercircle(g, nPts, 0.5);
        }

        public static Geometry Supercircle(Geometry g, int nPts, double pow)
        {
            var gsf = new GeometricShapeFactory();
            gsf.NumPoints = nPts;
            if (g != null)
                gsf.Envelope = g.EnvelopeInternal;
            else
                gsf.Envelope = new Envelope(0, 1, 0, 1);
            return gsf.CreateSupercircle(pow);
        }

        public static Geometry PointFieldCentroidStar(Geometry ptsGeom)
        {
            var pts = ptsGeom.Coordinates;
            Geometry centroid = ptsGeom.Centroid;
            return PointFieldStar(ptsGeom, centroid);
        }

        public static Geometry PointFieldStar(Geometry ptsGeom, Geometry centrePt)
        {
            var pts = ptsGeom.Coordinates;
            var centre = centrePt.Coordinate;

            var orderedPts = new List<OrderedPoint>();
            foreach (var p in pts)
            {
                double ang = AngleUtility.Angle(centre, p);
                orderedPts.Add(new OrderedPoint(p, ang));
            }

            orderedPts.Sort();
            int n = pts.Length + 1;
            var ring = new Coordinate[n];
            int i = 0;
            foreach (var op in orderedPts)
            {
                ring[i++] = op.Point;
            }
            // close ring
            ring[n - 1] = ring[0].Copy();
            return ptsGeom.Factory.CreatePolygon(ring);
        }

        private class OrderedPoint : IComparable<OrderedPoint>
        {
            readonly double _index;

            public OrderedPoint(Coordinate p, double index)
            {
                Point = p;
                _index = index;
            }

            public Coordinate Point { get; }

            public int CompareTo(OrderedPoint other)
            {
                return _index.CompareTo(other._index);
            }
        }
    }
}
