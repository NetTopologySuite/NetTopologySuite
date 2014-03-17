using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class CoordinateSequencesTest
    {
        [TestAttribute]
        public void TestCopyToLargerDim()
        {
            PackedCoordinateSequenceFactory csFactory = new PackedCoordinateSequenceFactory();
            ICoordinateSequence cs2D = CreateTestSequence(csFactory, 10, 2);
            ICoordinateSequence cs3D = csFactory.Create(10, 3);
            CoordinateSequences.Copy(cs2D, 0, cs3D, 0, cs3D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }

        [TestAttribute]
        public void TestCopyToSmallerDim()
        {
            PackedCoordinateSequenceFactory csFactory = new PackedCoordinateSequenceFactory();
            ICoordinateSequence cs3D = CreateTestSequence(csFactory, 10, 3);
            ICoordinateSequence cs2D = csFactory.Create(10, 2);
            CoordinateSequences.Copy(cs3D, 0, cs2D, 0, cs2D.Count);
            Assert.IsTrue(CoordinateSequences.IsEqual(cs2D, cs3D));
        }

        private static ICoordinateSequence CreateTestSequence(ICoordinateSequenceFactory csFactory, int size, int dim)
        {
            ICoordinateSequence cs = csFactory.Create(size, dim);
            // initialize with a data signature where coords look like [1, 10, 100, ...]
            for (int i = 0; i < size; i++)
                for (int d = 0; d < dim; d++)
                    cs.SetOrdinate(i, (Ordinate) d, i*Math.Pow(10, d));
            return cs;
        }
    }
}
