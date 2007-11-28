using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
{
    /// <summary> 
    /// A Key is a unique identifier for a node in a quadtree.
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </summary>
    public class Key<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static Int32 ComputeQuadLevel(IExtents<TCoordinate> extents)
        {
            Double dx = extents.GetSize(Ordinates.X);
            Double dy = extents.GetSize(Ordinates.Y);
            Double dMax = dx > dy ? dx : dy;
            Int32 level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        // the fields which make up the key
        private TCoordinate _coordinate = new TCoordinate();
        private Int32 level = 0;

        // auxiliary data which is derived from the key for use in computation
        private IExtents<TCoordinate> _extents = null;

        public Key(IExtents<TCoordinate> itemExtents)
        {
            ComputeKey(itemExtents);
        }

        public TCoordinate Point
        {
            get { return _coordinate; }
        }

        public Int32 Level
        {
            get { return level; }
        }

        public IExtents<TCoordinate> Extents
        {
            get { return _extents; }
        }

        public TCoordinate Center
        {
            get
            {
                return new TCoordinate(
                    (_extents.Min[Ordinates.X] + _extents.Max[Ordinates.X]) / 2,
                    (_extents.Min[Ordinates.Y] + _extents.Max[Ordinates.Y]) / 2);
            }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        public void ComputeKey(IExtents<TCoordinate> itemExtents)
        {
            level = ComputeQuadLevel(itemExtents);
            _extents = new Extents<TCoordinate>();

            computeKey(level, itemExtents);

            // MD - would be nice to have a non-iterative form of this algorithm
            while (!_extents.Contains(itemExtents))
            {
                level += 1;
                computeKey(level, itemExtents);
            }
        }

        private void computeKey(Int32 level, IExtents<TCoordinate> itemExtents)
        {
            Double quadSize = DoubleBits.PowerOf2(level);
            Double x = Math.Floor(itemExtents.Min[Ordinates.X] / quadSize) * quadSize;
            Double y = Math.Floor(itemExtents.Min[Ordinates.Y] / quadSize) * quadSize;
            _coordinate = new TCoordinate(x, y);
            TCoordinate quadPoint = new TCoordinate(x + quadSize, y + quadSize);
            _extents.SetToEmpty();
            _extents.ExpandToInclude(_coordinate, quadPoint);
        }
    }
}