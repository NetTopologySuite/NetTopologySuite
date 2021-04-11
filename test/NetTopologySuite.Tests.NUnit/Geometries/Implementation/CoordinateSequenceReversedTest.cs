using NetTopologySuite.Geometries;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public class CoordinateSequenceReversedTest
    {
        [Test]
        public void TestCoordinateArraySequence()
        {
            var csf = new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(
                new[] {new Coordinate(0, 0), new Coordinate(10, 10), new Coordinate(10, 0), new Coordinate(0, 0),});
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestDotSpatialAffineCoordinateSequence()
        {
            var csf = new NetTopologySuite.Geometries.Implementation.DotSpatialAffineCoordinateSequence(
                new[] { 0d, 0d, 10d, 10d, 10d, 0d, 0d, 0d, }, new []{ 1d, 2, 3, 4 }, new [] { 4, 3, 2, 1d} );
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestPackedDoubleCoordinateSequence()
        {
            var csf = new NetTopologySuite.Geometries.Implementation.PackedDoubleCoordinateSequence(
                new[] { 0d, 0d, 1d, 4d, 10d, 10d, 2d, 3d, 10d, 0d, 3d, 2d, 0d, 0d, 4d, 1d}, 4, 1);
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestPackedFloatCoordinateSequence()
        {
            var csf = new NetTopologySuite.Geometries.Implementation.PackedFloatCoordinateSequence(
                new[] { 0f, 0f, 1f, 4f, 10f, 10f, 2f, 3f, 10f, 0f, 3f, 2f, 0d, 0d, 4d, 1d }, 4, 1);
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceX_Y()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXY(
                new[] { 0d, 1d, 10d, 2d, 10d, 3d, 0d, 4d },
                new[] { 0d, 4d, 10d, 3d, 0d, 2d, 0d, 1d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceXY()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXY(
                new[] { 0d, 0d, 1d, 4d, 10d, 10d, 2d, 3d, 10d, 0d, 3d, 2d, 0d, 0d, 4d, 1d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceX_Y_Z()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZ(
                new[] { 0d, 1d, 10d, 2d, 10d, 3d, 0d, 4d },
                new[] { 0d, 4d, 10d, 3d, 0d, 2d, 0d, 1d },
                new[] { 8d, 7d, 6d, 5d, 4d, 3d, 2d, 1d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceXY_Z()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZ(
                new[] { 0d, 0d, 1d, 4d, 10d, 10d, 2d, 3d, 10d, 0d, 3d, 2d, 0d, 0d, 4d, 1d },
                new[] { 8d, 7d, 6d, 5d, 4d, 3d, 2d, 1d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceXYZ()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZ(
                new[] { 0d, 0d, 8d, 1d, 4d, 7d, 10d, 10d, 6d, 2d, 3d, 5d, 10d, 0d, 4d, 3d, 2d, 3d, 0d, 0d, 2d, 4d, 1d, 1d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceX_Y_Z_M()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZM(
                new[] { 0d, 1d, 10d, 2d, 10d, 3d, 0d, 4d },
                new[] { 0d, 4d, 10d, 3d, 0d, 2d, 0d, 1d },
                new[] { 8d, 7d, 6d, 5d, 4d, 3d, 2d, 1d },
                new[] { -1d, -2d, -3d, -4d, -5d, -6d, -7d, -8d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceXY_Z_M()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZM(
                new[] { 0d, 0d, 1d, 4d, 10d, 10d, 2d, 3d, 10d, 0d, 3d, 2d, 0d, 0d, 4d, 1d },
                new[] { 8d, 7d, 6d, 5d, 4d, 3d, 2d, 1d },
                new[] { -1d, -2d, -3d, -4d, -5d, -6d, -7d, -8d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceXYZM()
        {
            var csf = NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory.CreateXYZM(
                new[] { 0d, 0d, 8d, -1d, 1d, 4d, 7d, -2d, 10d, 10d, 6d, -3d, 2d, 3d, 5d, -4d, 10d, 0d, 4d, -5d, 3d, 2d, 3d, -6d, 0d, 0d, 2d, -7d, 4d, 1d, 1d, -8d });
            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        [Test]
        public void TestRawCoordinateSequenceHighlyAtypical()
        {
            // 6 spatial dimensions plus 4 measures = 10 total dimensions.
            // sprinkle their data throughout the various arrays that we create.
            // note that Measure3 is not represented, so it goes by itself.
            Ordinates[] ordinateGroups =
            {
                Ordinates.X | Ordinates.M,
                Ordinates.Y | Ordinates.Z,
                Ordinates.Spatial4 | Ordinates.Spatial5 | Ordinates.Measure2,
                Ordinates.Spatial6 | Ordinates.Measure4,
            };
            var factory = new NetTopologySuite.Geometries.Implementation.RawCoordinateSequenceFactory(ordinateGroups);
            var csf = factory.Create(102, 10, 4);
            for (int i = 0; i < csf.Count; i++)
            {
                for (int j = 0; j < csf.Dimension; j++)
                {
                    csf.SetOrdinate(i, j, (i * 391) - (j * 23));
                }
            }

            var csr = csf.Reversed();
            DoTest(csf, csr);
        }

        private static void DoTest(CoordinateSequence forward, CoordinateSequence reversed)
        {
            const double eps = 1e-12;

            Assert.AreEqual(forward.Count, reversed.Count, "Coordinate sequences don't have same size");
            Assert.AreEqual(forward.Ordinates, reversed.Ordinates, "Coordinate sequences don't serve same ordinate values");

            var ordinates = ToOrdinateArray(forward.Ordinates);
            int j = forward.Count;
            for (int i = 0; i < forward.Count; i++)
            {
                j--;
                foreach(var ordinate in ordinates)
                    Assert.AreEqual(forward.GetOrdinate(i, ordinate), reversed.GetOrdinate(j, ordinate), eps, string.Format("{0} values are not within tolerance", ordinate));
                var cf = forward.GetCoordinate(i);
                var cr = reversed.GetCoordinate(j);

                Assert.IsFalse(ReferenceEquals(cf, cr), "Coordinate sequences deliver same coordinate instances");
                Assert.IsTrue(cf.Equals(cr), "Coordinate sequences do not provide equal coordinates");
            }
        }

        private static Ordinate[] ToOrdinateArray(Ordinates ordinates)
        {
            var result = new Ordinate[OrdinatesUtility.OrdinatesToDimension(ordinates)];

            int nextResultIndex = 0;
            for (int i = 0; i < 32; i++)
            {
                if (ordinates.HasFlag((Ordinates)(1 << i)))
                {
                    result[nextResultIndex++] = (Ordinate)i;
                }
            }

            return result;
        }
    }
}
