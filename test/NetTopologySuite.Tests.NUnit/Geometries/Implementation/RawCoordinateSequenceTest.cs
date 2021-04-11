using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    public abstract class RawCoordinateSequenceTest : CoordinateSequenceTestBase
    {
        protected abstract Ordinates[] OrdinateGroups { get; }

        protected sealed override CoordinateSequenceFactory CsFactory => new RawCoordinateSequenceFactory(OrdinateGroups);

        [Test]
        public void Test499()
        {
            var cs = CsFactory.Create(new[] { new Coordinate(0, 10) });
            Assert.That(cs.ToCoordinateArray, Throws.Nothing);
        }
    }

    public sealed class RawCoordinateSequenceTestSoA : RawCoordinateSequenceTest
    {
        protected override Ordinates[] OrdinateGroups => Array.Empty<Ordinates>();
    }

    public sealed class RawCoordinateSequenceTestAoS : RawCoordinateSequenceTest
    {
        protected override Ordinates[] OrdinateGroups => new[] { Ordinates.AllOrdinates };
    }

    public sealed class RawCoordinateSequenceTestMixed1 : RawCoordinateSequenceTest
    {
        protected override Ordinates[] OrdinateGroups => new[] { Ordinates.XY, Ordinates.Z | Ordinates.M };
    }

    public sealed class RawCoordinateSequenceTestMixed2 : RawCoordinateSequenceTest
    {
        protected override Ordinates[] OrdinateGroups => new[] { Ordinates.X | Ordinates.Z, Ordinates.Y | Ordinates.M };
    }
}
