using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class CoordinateArraysTest : BaseSamples
    {
        ICoordinate[] array = new ICoordinate[]  
        {  
            new Coordinate(10, 10), 
            new Coordinate(20, 20),
            new Coordinate(30, 30),
            new Coordinate(40, 40),
            new Coordinate(50, 50),
            new Coordinate(50, 60), 
        };

        /// <summary>
        /// 
        /// </summary>
        public CoordinateArraysTest() : base() { }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ExtractTest()
        {
            ICoordinate[] result = CoordinateArrays.Extract(array, 1, 5);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(result[0], array[1]);
            Assert.AreEqual(result[1], array[2]);
            Assert.AreEqual(result[2], array[3]);
            Assert.AreEqual(result[3], array[4]);
            Assert.AreEqual(result[4], array[5]);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [ExpectedException("System.ArgumentException")]
        public void ExtractTest2()
        {            
            ICoordinate[] result = CoordinateArrays.Extract(array, 1, 10);
            Assert.IsNull(result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void EqualsComparerTest()
        {
            ICoordinate[] reverse = CoordinateArrays.CopyDeep(array);
            CoordinateArrays.Reverse(reverse);
            Assert.IsFalse(CoordinateArrays.Equals(array, reverse));
            Assert.IsFalse(CoordinateArrays.Equals(array, reverse, new CoordinateArrays.ForwardComparator()));
            Assert.IsTrue (CoordinateArrays.Equals(array, reverse, new CoordinateArrays.BidirectionalComparator()));
        }
    }
}

