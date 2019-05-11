using System;
using System.Collections.Generic;
using System.Linq;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Shape.Random;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Utility
{
    public class EnvelopeCombinerTests
    {
        private static readonly GeometryFactory Factory = GeometryFactory.Default;

        public static List<Geometry>[] EnvelopeCombinerTestCases =
        {
            null,

            new List<Geometry>(),

            new List<Geometry>
            {
                null,
                null,
                null,
                null,
            },

            new List<Geometry>
            {
                Factory.CreatePoint(),
                null,
                Factory.CreatePolygon(),
            },

            new List<Geometry>
            {
                Factory.CreatePoint(new Coordinate(1, 1)),
                null,
                null,
            },

            new List<Geometry>
            {
                null,
                Factory.ToGeometry(new Envelope(1, 2, 3, 4)),
                null,
                null,
                null,
                null,
                null,
            },

            new List<Geometry>
            {
                Factory.ToGeometry(new Envelope(1, 2, 3, 4)),
                null,
                Factory.ToGeometry(new Envelope(4, 3, 2, 1)),
            },

            new List<Geometry>
            {
                new RandomPointsBuilder(Factory) { NumPoints = 3 }.GetGeometry(),
                new RandomPointsBuilder(Factory) { NumPoints = 3 }.GetGeometry(),
                new RandomPointsBuilder(Factory) { NumPoints = 3 }.GetGeometry(),
            },
        };

        [TestCaseSource(nameof(EnvelopeCombinerTestCases))]
        public void TestEnvelopeCombine(ICollection<Geometry> geoms)
        {
            var actual = EnvelopeCombiner.Combine(geoms);

            // JTS doesn't usually bother doing anything special about nulls,
            // so our ports of their stuff will suffer the same.
            geoms = geoms?.Where(g => g != null).ToArray() ?? Array.Empty<Geometry>();

            var combinedGeometry = GeometryCombiner.Combine(geoms);

            // JTS also doesn't fear giving us nulls back from its algorithms.
            var expected = combinedGeometry?.EnvelopeInternal ?? new Envelope();

            Assert.AreEqual(expected, actual);
        }
    }
}
