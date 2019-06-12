using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NetTopologySuite.Geometries;
using NetTopologySuite.Shape.Random;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    /// <summary>
    /// Test cases for geometry aggregation methods that depend only on the points of the underlying
    /// geometries rather than their actual topology.
    /// </summary>
    public sealed class PointwiseGeometryAggregationTestCases : IEnumerable
    {
        public IEnumerator GetEnumerator() => GetTestCases().Select(tup => new TestCaseData(tup.geoms?.ToList()).SetName($"{{c}}.{{m}}: {tup.testCaseName}")).GetEnumerator();

        private static IEnumerable<(string testCaseName, Geometry[] geoms)> GetTestCases()
        {
            var factory = NtsGeometryServices.Instance.CreateGeometryFactory();

            yield return ("Null", null);

            yield return ("Empty", Array.Empty<Geometry>());

            yield return ("Contains only nulls", new Geometry[4]);

            yield return ("Contains empties and nulls", new Geometry[]
            {
                factory.CreatePoint(),
                null,
                factory.CreatePolygon(),
            });

            yield return ("One point", new Geometry[]
            {
                factory.CreatePoint(new Coordinate(1, 1)),
                null,
                null,
            });

            yield return ("Four points in one polygon", new Geometry[]
            {
                null,
                factory.ToGeometry(new Envelope(1, 2, 3, 4)),
                null,
                null,
                null,
                null,
                null,
            });

            yield return ("Eight points in two polygons", new Geometry[]
            {
                factory.ToGeometry(new Envelope(1, 2, 3, 4)),
                null,
                factory.ToGeometry(new Envelope(4, 3, 2, 1)),
            });

            yield return ("Nine random points in three polygons", new Geometry[]
            {
                new RandomPointsBuilder(factory) { NumPoints = 3 }.GetGeometry(),
                new RandomPointsBuilder(factory) { NumPoints = 3 }.GetGeometry(),
                new RandomPointsBuilder(factory) { NumPoints = 3 }.GetGeometry(),
            });
        }
    }
}
