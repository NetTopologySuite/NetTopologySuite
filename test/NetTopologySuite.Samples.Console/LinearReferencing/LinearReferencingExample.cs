using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.LinearReferencing;
using NUnit.Framework;

namespace NetTopologySuite.Samples.LinearReferencing
{
    /// <summary>
    /// Examples of Linear Referencing
    /// </summary>
    public class LinearReferencingExample
    {
        private static WKTReader rdr = new WKTReader(new NtsGeometryServices(new PrecisionModel(PrecisionModels.Fixed)));

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearReferencingExample"/> class.
        /// </summary>
        public LinearReferencingExample() { }

        /// <summary>
        ///
        /// </summary>
        [Test]
        public void Run()
        {
            RunExtractedLine("LINESTRING (0 0, 10 10, 20 20)", 1, 10);
            RunExtractedLine("MULTILINESTRING ((0 0, 10 10), (20 20, 25 25, 30 40))", 1, 20);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="wkt"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void RunExtractedLine(string wkt, double start, double end)
        {
            Console.WriteLine("=========================");
            var g1 = rdr.Read(wkt);
            Console.WriteLine("Input Geometry: " + g1);
            Console.WriteLine("Indices to extract: " + start + " " + end);

            var indexedLine = new LengthIndexedLine(g1);

            var subLine = indexedLine.ExtractLine(start, end);
            Console.WriteLine("Extracted Line: " + subLine);

            double[] index = indexedLine.IndicesOf(subLine);
            Console.WriteLine("Indices of extracted line: " + index[0] + " " + index[1]);

            var midpt = indexedLine.ExtractPoint((index[0] + index[1]) / 2);
            Console.WriteLine("Midpoint of extracted line: " + midpt);
        }

        [Test]
        public void TestIndexedPointOnLine()
        {
            var reader = new WKTReader();
            var geom = reader.Read("LINESTRING (0 0, 0 10, 10 10)");

            geom = InsertPoint(geom, new Coordinate(1, 5));
            Assert.AreEqual("LINESTRING (0 0, 1 5, 0 10, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(6, 9));
            Assert.AreEqual("LINESTRING (0 0, 1 5, 0 10, 6 9, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(0, 0));
            Assert.AreEqual("LINESTRING (0 0, 1 5, 0 10, 6 9, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(10, 10));
            Assert.AreEqual("LINESTRING (0 0, 1 5, 0 10, 6 9, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(1, 5));
            Assert.AreEqual("LINESTRING (0 0, 1 5, 0 10, 6 9, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(0, -2));
            Assert.AreEqual("LINESTRING (0 -2, 0 0, 1 5, 0 10, 6 9, 10 10)", geom.AsText());
            geom = InsertPoint(geom, new Coordinate(10, 13));
            Assert.AreEqual("LINESTRING (0 -2, 0 0, 1 5, 0 10, 6 9, 10 10, 10 13)", geom.AsText());

        }

Geometry InsertPoint(Geometry geom, Coordinate point)
{
    if (!(geom is ILineal))
        throw new InvalidOperationException();

    var lil = new LocationIndexedLine(geom);
    var ll = lil.Project(point);

    var element = (LineString) geom.GetGeometryN(ll.ComponentIndex);
    var oldSeq = element.CoordinateSequence;
    var newSeq = element.Factory.CoordinateSequenceFactory.Create(
        oldSeq.Count + 1, oldSeq.Dimension, oldSeq.Measures);

            int j = 0;
    if (ll.SegmentIndex == 0 && ll.SegmentFraction == 0)
    {
        if (ll.GetSegment(element).P0.Distance(point) == 0) return geom;
        newSeq.SetCoordinate(0, point);
        CoordinateSequences.Copy(oldSeq, 0, newSeq, 1, oldSeq.Count);
    }
    else if (ll.SegmentIndex == oldSeq.Count - 1 && ll.SegmentFraction == 0)
    {
        if (ll.GetSegment(element).P1.Distance(point) == 0) return geom;
        CoordinateSequences.Copy(oldSeq, 0, newSeq, 0, oldSeq.Count);
        newSeq.SetCoordinate(oldSeq.Count, point);
    }
    else
    {
        if (ll.IsVertex) return geom;

        CoordinateSequences.Copy(oldSeq, 0, newSeq, 0, ll.SegmentIndex + 1);
        newSeq.SetCoordinate(ll.SegmentIndex + 1, point);
        CoordinateSequences.Copy(oldSeq, ll.SegmentIndex + 1, newSeq, ll.SegmentIndex + 2, newSeq.Count - 2 - ll.SegmentIndex);
    }

    var lines = new List<Geometry>();
    LineStringExtracter.GetLines(geom, lines);
    lines[ll.ComponentIndex] = geom.Factory.CreateLineString(newSeq);
    if (lines.Count == 1)
        return lines[0];
    return geom.Factory.BuildGeometry(lines);

}
    }

    internal static class ICoordinateSequenceEx
    {
        public static void SetCoordinate(this CoordinateSequence self, int index, Coordinate coord)
        {
            self.SetOrdinate(index, Ordinate.X, coord.X);
            self.SetOrdinate(index, Ordinate.Y, coord.Y);
            if (self.Dimension > 2) self.SetOrdinate(index, Ordinate.Z, coord.Z);
        }
    }
}
