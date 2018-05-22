using System;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue179Fixture
    {
        [Test]
        public void clone_of_null_envelope_should_return_null_envelope()
        {
            var envelope = new Envelope();
            Assert.That(envelope.IsNull, Is.True);

            var clone = envelope.Copy();
            Assert.That(clone, Is.Not.Null);
            Assert.That(clone.IsNull, Is.True);
            Assert.That(clone, Is.EqualTo(clone));      
            Assert.That(Object.ReferenceEquals(envelope, clone), Is.False);
        }
    }
}