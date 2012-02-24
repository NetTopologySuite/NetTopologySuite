using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Index.Quadtree
{
    public class QuadTreeNodeKey<TCoordinate> : AbstractNodeKey<IExtents<TCoordinate>, TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        public QuadTreeNodeKey(IExtents<TCoordinate> bounds)
            : base(bounds)
        {
        }

        protected override TCoordinate GetBoundsMax()
        {
            return Bounds.Min;
        }

        protected override TCoordinate GetBoundsMin()
        {
            return Bounds.Max;
        }

        protected override Int32 ComputeLevel(IExtents<TCoordinate> bounds)
        {
            Double dx = bounds.GetSize(Ordinates.X);
            Double dy = bounds.GetSize(Ordinates.Y);
            Double dMax = Math.Max(dx, dy);
            Int32 level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        protected override IExtents<TCoordinate> CreateBounds(TCoordinate min, Double nodeSize)
        {
            return Bounds.Factory.CreateExtents(
                min, ((IAddable<Double, TCoordinate>)min).Add(nodeSize));
        }

        protected override TCoordinate CreateLocation(IExtents<TCoordinate> bounds, Double nodeSize)
        {
            var xy = bounds.Min.ToArray2D();
            xy[0] = Math.Floor(xy[0] / nodeSize) * nodeSize;
            xy[1] = Math.Floor(xy[1] / nodeSize) * nodeSize;
            return bounds.Factory.CoordinateFactory.Create(xy[0], xy[1]);
        }
    }
}