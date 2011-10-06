using System;
using System.Collections.Generic;
using System.Drawing;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;
using NetTopologySuite.Windows.Forms;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class CreateGeometryFunctions
    {

        private static readonly int DEFAULT_POINTSIZE = 100;

        public static IGeometry fontGlyphSerif(IGeometry g, String text)
        {
            return fontGlyph(g, text, new Font(FontGlyphReader.FontSerif, DEFAULT_POINTSIZE, FontStyle.Regular));
        }

        public static IGeometry fontGlyphSanSerif(IGeometry g, String text)
        {
            return fontGlyph(g, text, new Font(FontGlyphReader.FontSanserif, DEFAULT_POINTSIZE, FontStyle.Regular));
        }

        public static IGeometry fontGlyphMonospaced(IGeometry g, String text)
        {
            return fontGlyph(g, text, new Font(FontGlyphReader.FontMonospaced, DEFAULT_POINTSIZE, FontStyle.Regular));
        }

        private static IGeometry fontGlyph(IGeometry g, String text, Font font)
        {
            IEnvelope env = FunctionsUtil.getEnvelopeOrDefault(g);
            IGeometryFactory geomFact = FunctionsUtil.getFactoryOrDefault(g);

            IGeometry textGeom = FontGlyphReader.Read(text, font, geomFact);
            var envText = textGeom.EnvelopeInternal;

            if (g != null)
            {
                // transform to baseline
                ICoordinate baseText0 = new Coordinate(envText.MinX, envText.MinY);
                ICoordinate baseText1 = new Coordinate(envText.MaxX, envText.MinY);
                ICoordinate baseGeom0 = new Coordinate(env.MinX, env.MinY);
                ICoordinate baseGeom1 = new Coordinate(env.MaxX, env.MinY);
                AffineTransformation trans = AffineTransformationFactory.CreateFromBaseLines(baseText0, baseText1,
                                                                                             baseGeom0, baseGeom1);
                return trans.Transform(textGeom);
            }
            return textGeom;
        }

        public static IGeometry grid(IGeometry g, int nCells)
        {
            var geoms = new List<IGeometry>();

            var env = FunctionsUtil.getEnvelopeOrDefault(g);
            var geomFact = FunctionsUtil.getFactoryOrDefault(g);

            int nCellsOnSide = (int) Math.Sqrt(nCells) + 1;
            double delX = env.Width/nCellsOnSide;
            double delY = env.Height/nCellsOnSide;

            for (int i = 0; i < nCellsOnSide; i++)
            {
                for (int j = 0; j < nCellsOnSide; j++)
                {
                    double x = env.MinX + i*delX;
                    double y = env.MinY + j*delY;

                    var cellEnv = new Envelope(x, x + delX, y, y + delY);
                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }
            return geomFact.BuildGeometry(geoms);
        }

        public static IGeometry supercircle3(IGeometry g, int nPts)
        {
            return supercircle(g, nPts, 3);
        }

        public static IGeometry squircle(IGeometry g, int nPts)
        {
            return supercircle(g, nPts, 4);
        }

        public static IGeometry supercircle5(IGeometry g, int nPts)
        {
            return supercircle(g, nPts, 5);
        }

        public static IGeometry supercirclePoint5(IGeometry g, int nPts)
        {
            return supercircle(g, nPts, 0.5);
        }


        public static IGeometry supercircle(IGeometry g, int nPts, double pow)
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