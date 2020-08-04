using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Strtree
{
    public class EnvelopeDistanceTest
    {

        [Test]
        public void TestDisjoint()
        {
            CheckEnvelopeDistance(new Envelope(0, 10, 0, 10), new Envelope(20, 30, 20, 40), 50);
        }

        [Test]
        public void TestOverlapping()
        {
            CheckEnvelopeDistance(new Envelope(0, 30, 0, 30), new Envelope(20, 30, 20, 40), 50);
        }

        [Test]
        public void TestCrossing()
        {
            CheckEnvelopeDistance(new Envelope(0, 40, 10, 20), new Envelope(20, 30, 0, 30), 50);
        }

        [Test]
        public void TestCrossing2()
        {
            CheckEnvelopeDistance(new Envelope(0, 10, 4, 6), new Envelope(4, 6, 0, 10), 14.142135623730951);
        }

        private void CheckEnvelopeDistance(Envelope env1, Envelope env2, double expected)
        {
            double result = EnvelopeDistance.MaximumDistance(env1, env2);
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
