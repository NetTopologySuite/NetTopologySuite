using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NPack;
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
                min, ((IAddable<Double, TCoordinate>) min).Add(nodeSize));
        }

        protected override TCoordinate CreateLocation(IExtents<TCoordinate> bounds, Double nodeSize)
        {
            TCoordinate min = bounds.Min;
            DoubleComponent dminx, dminy;
            min.GetComponents(out dminx, out dminy);
            Double x = Math.Floor((Double)dminx/nodeSize)*nodeSize;
            Double y = Math.Floor((Double)dminy/nodeSize)*nodeSize;
            //Double x = Math.Floor(min[Ordinates.X] / nodeSize) * nodeSize;
            //Double y = Math.Floor(min[Ordinates.Y] / nodeSize) * nodeSize;
            return Bounds.Factory.CoordinateFactory.Create(x, y);
        }
    }
}