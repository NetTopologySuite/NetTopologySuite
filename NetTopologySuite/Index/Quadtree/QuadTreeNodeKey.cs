using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    public class QuadTreeNodeKey<TCoordinate> : AbstractNodeKey<IExtents<TCoordinate>, TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
            IComputable<TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        public QuadTreeNodeKey(IExtents<TCoordinate> bounds) 
            : base(bounds) { }

        protected override TCoordinate GetBoundsMax()
        {
            throw new NotImplementedException();
        }

        protected override TCoordinate GetBoundsMin()
        {
            throw new NotImplementedException();
        }

        protected override Int32 ComputeLevel(IExtents<TCoordinate> bounds)
        {
            throw new NotImplementedException();
        }

        protected override IExtents<TCoordinate> CreateBounds(TCoordinate min, Double nodeSize)
        {
            throw new NotImplementedException();
        }

        protected override TCoordinate CreateLocation(IExtents<TCoordinate> bounds, Double nodeSize)
        {
            throw new NotImplementedException();
        }
    }
}
