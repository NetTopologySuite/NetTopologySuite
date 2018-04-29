using System.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ReverseTest : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public ReverseTest() : base() { }        

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void LineStringReverseTest()
        {
            ILineString lineString = Factory.CreateLineString(new Coordinate[] 
            { 
                new Coordinate(10, 10), 
                new Coordinate(20, 20), 
                new Coordinate(20, 30), 
            });            
            var reverse = (ILineString)lineString.Reverse();

            Debug.WriteLine(lineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.IsTrue(lineString.Equals(reverse));
            Assert.IsFalse(lineString.EqualsExact(reverse));
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void MultiLineStringReverseTest()
        {
            ILineString lineString1 = Factory.CreateLineString(new Coordinate[] 
            { 
                new Coordinate(10, 10), 
                new Coordinate(20, 20), 
                new Coordinate(20, 30), 
            });
            ILineString lineString2 = Factory.CreateLineString(new Coordinate[] 
            { 
                new Coordinate(12, 12), 
                new Coordinate(24, 24), 
                new Coordinate(24, 36), 
            });
            IMultiLineString multiLineString = Factory.CreateMultiLineString(new[] { lineString1, lineString2, });
            IMultiLineString reverse = multiLineString.Reverse();

            Debug.WriteLine(multiLineString.ToString());
            Debug.WriteLine(reverse.ToString());

            Assert.IsTrue(multiLineString.Equals(reverse));
            Assert.IsFalse(multiLineString.EqualsExact(reverse));

            IGeometry result2 = reverse[1];
            Assert.IsTrue(lineString1.Equals(result2));
            Assert.IsFalse(lineString1.EqualsExact(result2));

            IGeometry result1 = reverse[0];
            Assert.IsTrue(lineString2.Equals(result1));
            Assert.IsFalse(lineString2.EqualsExact(result1));
        }
    }
}
