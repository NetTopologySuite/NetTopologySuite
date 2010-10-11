using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.DataStructures;
using NetTopologySuite.Index.Bintree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class BinTreeTests
    {
        [Test]
        public void InsertingItemsResultsInCorrectCount()
        {
           
            BinTree<String> binTree = new BinTree<String>();
            binTree.Insert(new Interval(5, 10), "A");
            Assert.AreEqual(1, binTree.TotalItemCount);
            binTree.Insert(new Interval(15, 20), "B");
            Assert.AreEqual(2, binTree.TotalItemCount);
        }
    }
}
