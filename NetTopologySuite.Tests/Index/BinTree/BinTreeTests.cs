using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Index.Bintree;
using Xunit;

namespace NetTopologySuite.Tests.Index
{
    public class BinTreeTests
    {
        [Fact]
        public void InsertingItemsResultsInCorrectCount()
        {
           
            BinTree<String> binTree = new BinTree<String>();
            binTree.Insert(new Interval(5, 10), "A");
            Assert.Equal(1, binTree.TotalItemCount);
            binTree.Insert(new Interval(15, 20), "B");
            Assert.Equal(2, binTree.TotalItemCount);
        }
    }
}
