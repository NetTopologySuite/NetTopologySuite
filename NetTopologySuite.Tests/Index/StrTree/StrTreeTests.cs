using System;
using System.Collections.Generic;
using System.Text;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.Index.StrTree
{
    public class StrTreeTests
    {
        public class BoundedString : IBoundable<IExtents<BufferedCoordinate>>
        {
            private String _value;
            private IExtents<BufferedCoordinate> _extents;

            public BoundedString(String value, IExtents<BufferedCoordinate> extents)
            {
                _value = value;
                _extents = extents;
            }

            public bool Intersects(IExtents<BufferedCoordinate> other)
            {
                throw new System.NotImplementedException();
            }

            public IExtents<BufferedCoordinate> Bounds
            {
                get { throw new System.NotImplementedException(); }
            }
        }

        [Fact]
        public void InsertingItemsResultsInCorrectCount()
        {
            StrTree<BufferedCoordinate, BoundedString> strTree = new StrTree<BufferedCoordinate, BoundedString>();
            strTree.Insert(new BoundedString("A"));
        }
    }
}
