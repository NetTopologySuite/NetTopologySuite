using System;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Coordinates;
using Xunit;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif

namespace NetTopologySuite.Tests.Index.StrTree
{
    public class StrTreeTests
    {
        public class BoundedString : IBoundable<IExtents<coord>>
        {
            private String _value;
            private IExtents<coord> _extents;

            public BoundedString(String value, IExtents<coord> extents)
            {
                _value = value;
                _extents = extents;
            }

            public String Value
            {
                get { return _value; }
            }

            public Boolean Intersects(IExtents<coord> other)
            {
                return _extents.Intersects(other);
            }

            public IExtents<coord> Bounds
            {
                get { return _extents; }
            }
        }

        [Fact]
        public void InsertingItemsResultsInCorrectCount()
        {
            StrTree<coord, BoundedString> strTree 
                = new StrTree<coord, BoundedString>(TestFactories.GeometryFactory);
            IExtents<coord> bounds 
                = TestFactories.GeometryFactory.CreateExtents2D(5, 5, 10, 10) as IExtents<coord>;
            strTree.Insert(new BoundedString("A", bounds));
            Assert.Equal(1, strTree.Count);
        }
    }
}
