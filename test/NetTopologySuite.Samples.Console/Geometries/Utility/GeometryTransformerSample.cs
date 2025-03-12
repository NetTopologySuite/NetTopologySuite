using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Tests.NUnit;
using NUnit.Framework;
using System.Collections.Generic;

namespace NetTopologySuite.Samples.Geometries.Utility
{
    internal class GeometryTransformerSample : GeometryTestCase
    {
        [Test, Description("GitHub issue #764")]
        public void TestIntersectionPointZOrdinate()
        {
            var geom1 = Read("POLYGON Z((673561.48 2956214.6223 0, 673551.48 2956303 2, 673561.48 2956303 1.5, 673561.48 2956214.6223 0))");
            var geom2 = Read("POLYGON Z((673545.48 2956297 2.3, 673577.48 2956297 0.7, 673577.48 2956309 0.7, 673545.48 2956309 2.3, 673545.48 2956297 2.3))");

            var intersection = geom1.Intersection(geom2);
            TestContext.WriteLine(intersection);

            var t = new IntPtZToNaNTransformer(geom1, geom2);
            intersection = t.Transform(intersection);
            TestContext.WriteLine(intersection);

            var expected = Read("POLYGON Z((673552.15890429367 2956297 NaN, 673551.48 2956303 2, 673561.48 2956303 1.5, 673561.48 2956297 NaN, 673552.15890429367 2956297 NaN))");

            Assert.That(CoordinateSequences.IsEqual(((Polygon)intersection).ExteriorRing.CoordinateSequence,
                ((Polygon)expected).ExteriorRing.CoordinateSequence), Is.True);
        }

        internal class IntPtZToNaNTransformer : GeometryTransformer
        {
            private readonly HashSet<Coordinate> _origCoordinates;

            public IntPtZToNaNTransformer(params Geometry[] originals)
            {
                _origCoordinates = new HashSet<Coordinate>();
                foreach (var original in originals)
                    foreach (var originalCoord in original.Coordinates)
                        _origCoordinates.Add(originalCoord);
            }

            protected override CoordinateSequence Copy(CoordinateSequence seq)
            {
                var res = seq.Copy();
                for (int i = 0; i < res.Count; i++)
                {
                    if (_origCoordinates.Contains(res.GetCoordinate(i)))
                        continue;
                    res.SetZ(i, Coordinate.NullOrdinate);
                }
                return res;
            }
        }

    }
}
