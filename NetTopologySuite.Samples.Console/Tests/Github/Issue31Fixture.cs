using GeoAPI.Geometries;
using NetTopologySuite.Index.Strtree;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Issue31Fixture
    {
        [Test]
        public void TestRemoveSTRtree()
        {
            var tree = new STRtree<string>();
            tree.Insert(new Envelope(0, 10, 0, 10), "1");
            tree.Insert(new Envelope(5, 15, 5, 15), "2");
            tree.Insert(new Envelope(10, 20, 10, 20), "3");
            tree.Insert(new Envelope(15, 25, 15, 25), "4");
            Assert.DoesNotThrow(() => tree.Remove(new Envelope(10, 20, 10, 20), "4"));
            Assert.AreEqual(3, tree.Count);
        }

        //Not necessary since AbstractSTRtree<>.Remove is not exposed with SIRtree
        /*
        public void TestRemoveSIRtree()
        {
            var tree = new SIRtree<string>();
            tree.Insert(0, 10, "1");
            tree.Insert(5, 15, "2");
            tree.Insert(10, 20, "3");
            tree.Insert(15, 25, "4");
            Assert.DoesNotThrow(() => tree.Remove(new Index.Strtree.Interval(10, 20), "4"));
            Assert.AreEqual(3, tree.Count);
        }
        */
    }
}