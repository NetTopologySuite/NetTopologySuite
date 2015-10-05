using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Match;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue12Fixture
    {
        [Test]
        public void check_difference_results_with_fixed_precision()
        {
            GeometryFactory precisionModel = new GeometryFactory(new PrecisionModel(100));
            WKTReader reader = new WKTReader(precisionModel);
            IGeometry p1 = reader.Read(@"POLYGON ((504927.9 6228865.64, 504969.88 6228833.89, 504980.82 6228861.76, 504927.9 6228865.64))");
            IGeometry p2 = reader.Read(@"POLYGON ((504927.9 6228865.64, 504951.14 6228848.06, 504957.42 6228863.47, 504927.9 6228865.64))");
            IGeometry test = p1.Difference(p2);
            Assert.That(test, Is.Not.Null);
            Assert.That(test.IsEmpty, Is.False);

            const string expected = @"POLYGON ((504927.9 6228865.64, 504980.82 6228861.76, 504969.88 6228833.89, 504951.14 6228848.06, 504957.42 6228863.47, 504927.9 6228865.64))";
            var res = reader.Read(expected);
            res.Normalize();
            test.Normalize();
            Console.WriteLine(test.AsText());
            Console.WriteLine(res.AsText());
            var hd = new HausdorffSimilarityMeasure();
            var resD  = hd.Measure(test, res);
            Assert.That(1-resD, Is.LessThan(1e7));
        }
    }
}