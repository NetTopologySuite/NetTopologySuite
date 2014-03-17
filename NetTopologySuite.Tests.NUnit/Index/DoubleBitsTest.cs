using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    [TestFixtureAttribute]
    public class DoubleBitsTest
    {
        [TestAttribute]
        public void TestExponent()
        {
            Assert.IsTrue(DoubleBits.GetExponent(-1) == 0);
            Assert.IsTrue(DoubleBits.GetExponent(8.0) == 3);
            Assert.IsTrue(DoubleBits.GetExponent(128.0) == 7);
        }
    }
}