using System;
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

            public Boolean Intersects(IExtents<BufferedCoordinate> other)
            {
                return _extents.Intersects(other);
            }

            public IExtents<BufferedCoordinate> Bounds
            {
                get { return _extents; }
            }
        }

        [Fact]
        public void InsertingItemsResultsInCorrectCount()
        {
            StrTree<BufferedCoordinate, BoundedString> strTree 
                = new StrTree<BufferedCoordinate, BoundedString>(TestFactories.GeometryFactory);
            IExtents<BufferedCoordinate> bounds 
                = TestFactories.GeometryFactory.CreateExtents2D(5, 5, 10, 10) as IExtents<BufferedCoordinate>;
            strTree.Insert(new BoundedString("A", bounds));
            Assert.Equal(1, strTree.Count);
        }
    }
}
