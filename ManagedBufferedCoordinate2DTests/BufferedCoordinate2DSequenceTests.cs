using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;
using NUnit.Framework;

namespace ManagedBufferedCoordinate2DTests
{
    [TestFixture]
    public class BufferedCoordinate2DSequenceTests
    {
        [Test]
        public void CreatingCoordinateSequenceSucceeds()
        {
            BufferedCoordinate2DSequenceFactory factory 
                = new BufferedCoordinate2DSequenceFactory();
            ICoordinateSequence<BufferedCoordinate2D> seq 
                = factory.Create(CoordinateDimensions.Two);
        }

        [Test]
        public void ChangingSequenceElementDoesntAffectOtherSequencesWithTheSameCoordinate()
        {
            BufferedCoordinate2DSequenceFactory factory
                = new BufferedCoordinate2DSequenceFactory();

            ICoordinateSequence<BufferedCoordinate2D> seq1
                = factory.Create(CoordinateDimensions.Two);
            ICoordinateSequence<BufferedCoordinate2D> seq2
                = factory.Create(CoordinateDimensions.Two);

            ICoordinateFactory<BufferedCoordinate2D> coordFactory = factory.CoordinateFactory;

            Random rnd = new MersenneTwister();

            for (Int32 i = 0; i < 100; i++)
            {
                BufferedCoordinate2D coord = coordFactory.Create(rnd.NextDouble(), 
                                                                 rnd.NextDouble());
                seq1.Add(coord);
                seq2.Add(coord);
                Assert.IsTrue(seq1[i].Equals(seq2[i]));
            }

            BufferedCoordinate2D c = seq1[10];
            Double x = c.X;
            Double y = c.Y;

            seq1[10] = coordFactory.Create(1234, 1234);

            Assert.AreEqual(x, seq2[10][Ordinates.X]);
            Assert.AreEqual(y, seq2[10][Ordinates.Y]);
        }
    }
}
