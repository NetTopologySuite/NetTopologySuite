using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class CreateShapeFunctions
    {
        private static readonly int DEFAULT_POINTSIZE = 100;

        public static IGeometry Grid(IGeometry g, int nCells)
        {
            var geoms = new List<IGeometry>();

            var env = FunctionsUtil.GetEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.GetFactoryOrDefault(g);

            var nCellsOnSide = (int) Math.Sqrt(nCells) + 1;
            var cellSizeX = env.Width/nCellsOnSide;
            var cellSizeY = env.Height/nCellsOnSide;

            for (var i = 0; i < nCellsOnSide; i++)
            {
                for (var j = 0; j < nCellsOnSide; j++)
                {
                    var x1 = env.MinX + i * cellSizeX;
                    var y1 = env.MinY + j * cellSizeY;
                    var x2 = env.MinX + (i+1) * cellSizeX;
                    var y2 = env.MinY + (j+1) * cellSizeY;
                    var cellEnv = new Envelope(x1, x2, y1, y2);

                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }
            return geomFact.BuildGeometry(geoms);
        }

        public static IGeometry Supercircle3(IGeometry g, int nPts)
        {
            return Supercircle(g, nPts, 3);
        }

        public static IGeometry Squircle(IGeometry g, int nPts)
        {
            return Supercircle(g, nPts, 4);
        }

        public static IGeometry Supercircle5(IGeometry g, int nPts)
        {
            return Supercircle(g, nPts, 5);
        }

        public static IGeometry SupercirclePoint5(IGeometry g, int nPts)
        {
            return Supercircle(g, nPts, 0.5);
        }

        public static IGeometry Supercircle(IGeometry g, int nPts, double pow)
        {
            var gsf = new GeometricShapeFactory();
            gsf.NumPoints = nPts;
            if (g != null)
                gsf.Envelope = g.EnvelopeInternal;
            else
                gsf.Envelope = new Envelope(0, 1, 0, 1);
            return gsf.CreateSupercircle(pow);
        }
    }
}