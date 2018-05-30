using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Polygonize;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.StackOverflow
{
    [System.ComponentModel.Category("Technique")]
    public class MakeValid
    {
        private const string Wkt =
            "MULTIPOLYGON(((6.8016626382085361 52.715309958659567, 6.8016978566354931 52.715300551948957, "+
                           "6.8015610818087193 52.715189826862179, 6.8014185055170415 52.715082765798371, "+
                           "6.8011385751008762 52.714892848902714, 6.80060419093019 52.714522597434573, "+
                           "6.7999630427768381 52.714053843670676, 6.7997671025094686 52.713897979848007, "+
                           "6.7995041016726177 52.713681792796635, 6.8008707444707825 52.71273529869638, "+
                           "6.8018179126154372 52.713377474968908, 6.8019432523746257 52.713500919031091, "+
                           "6.8019712703624577 52.71358956783984, 6.8019625919554079 52.713687641028628, "+
                           "6.801900320844819 52.714051499553385, 6.8018106242715941 52.714620612080388, "+
                           "6.801752965346215 52.714943969273726, 6.8016626382085361 52.715309958659567)))";

        [Test, Ignore("Known to fail")]
        public void TestBuffer0()
        {
            var rdr = new WKTReader();
            var geom = rdr.Read(Wkt);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom.IsValid, Is.False);

            var fixedGeom = geom.Normalized().Buffer(0);
            Assert.That(fixedGeom, Is.Not.Null);
            var hsm = new HausdorffSimilarityMeasure();
            Assert.That(hsm.Measure(fixedGeom, geom), Is.GreaterThan(0.9));
        }

        [Test]
        public void TestValidate()
        {
            var rdr = new WKTReader();
            var geom = rdr.Read(Wkt);
            Assert.That(geom, Is.Not.Null);
            Assert.That(geom.IsValid, Is.False);

            var fixedGeom = geom.Validate();
            Assert.That(fixedGeom, Is.Not.Null);
            var hsm = new HausdorffSimilarityMeasure();
            var m1 = hsm.Measure(fixedGeom, geom);
            var m2 = hsm.Measure(geom, fixedGeom);
            Assert.That(m1, Is.GreaterThan(0.9));
        }
    }

    /// <summary>
    /// Extension method to validate polygons
    /// </summary>
    internal static class ValidateGeometryExtension
    {
        /// <summary>
        /// Get or create a valid version of the geometry given. If the geometry is a
        /// polygon or multi polygon, self intersections or inconsistencies are fixed.
        /// Otherwise the geometry is returned.
        /// </summary>
        /// <param name="geom">The geometry to be fixed</param>
        /// <returns>The fixed geometry</returns>
        public static IGeometry Validate(this IGeometry geom)
        {
            if (geom is IPolygon)
            {
                if (geom.IsValid)
                {
                    geom.Normalize(); // validate does not pick up rings in the wrong order - this will fix that
                    return geom; // If the polygon is valid just return it
                }
                var polygonizer = new Polygonizer();
                AddPolygon((IPolygon)geom, polygonizer);
                return ToPolygonGeometry(polygonizer.GetPolygons(), geom.Factory);
            }

            if (geom is IMultiPolygon)
            {
                if (geom.IsValid)
                {
                    geom.Normalize(); // validate does not pick up rings in the wrong order - this will fix that
                    return geom; // If the multipolygon is valid just return it
                }
                var polygonizer = new Polygonizer();
                for (int n = geom.NumGeometries; n-- > 0;)
                {
                    AddPolygon((IPolygon)geom.GetGeometryN(n), polygonizer);
                }
                return ToPolygonGeometry(polygonizer.GetPolygons(), geom.Factory);
            }

            // ToDo other validations

            // Only care about polygons
            return geom;
        }

        /// <summary>
        /// Add all line strings from the polygon given to the polygonizer given
        /// </summary>
        /// <param name="polygon">The polygon from which to extract the line strings</param>
        /// <param name="polygonizer">The polygonizer</param>
        static void AddPolygon(IPolygon polygon, Polygonizer polygonizer)
        {
            AddLineString(polygon.ExteriorRing, polygonizer);
            for (var n = polygon.NumInteriorRings; n-- > 0;)
            {
                AddLineString(polygon.GetInteriorRingN(n), polygonizer);
            }
        }

        /// <summary>
        /// Add the linestring given to the polygonizer
        /// </summary>
        /// <param name="lineString">The line string</param>
        /// <param name="polygonizer">The polygonizer</param>
        static void AddLineString(ILineString lineString, Polygonizer polygonizer)
        {
            if (lineString is ILinearRing)
            { // LinearRings are treated differently to line strings : we need a LineString NOT a LinearRing
                lineString = lineString.Factory.CreateLineString(lineString.CoordinateSequence);
            }

            // unioning the linestring with the point makes any self intersections explicit.
            var point = lineString.Factory.CreatePoint(lineString.GetCoordinateN(0));
            var toAdd = lineString.Union(point);

            //Add result to polygonizer
            polygonizer.Add(toAdd);
        }

        /// <summary>
        /// Get a geometry from a collection of polygons.
        /// </summary>
        /// <param name="polygons"></param>
        /// <param name="factory">Factory to create <see cref="IMultiPolygon"/>s</param>
        /// <returns><c>null</c> if there were no polygons, the polygon if there was only one, or a MultiPolygon containing all polygons otherwise</returns>
        static IGeometry ToPolygonGeometry(ICollection<IGeometry> polygons, IGeometryFactory factory)
        {
            switch (polygons.Count)
            {
                case 0:
                    return null; // No valid polygons!
                case 1:
                    return polygons.First(); // single polygon - no need to wrap
                default:
                    //polygons may still overlap! Need to sym difference them
                    IGeometry ret = (IPolygon)polygons.First().Copy();
                    foreach (var polygon in polygons.Skip(1))
                        ret = ret.SymmetricDifference(polygon);
                    return ret;
            }
        }
    }
}