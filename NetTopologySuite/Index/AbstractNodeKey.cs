using System;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Index
{
    /// <summary> 
    /// A key is a unique identifier for a node in regular region indexes.
    /// It contains a lower-left point and a level number. The level number
    /// is the log with the base of the scale of the decomposition.
    /// </summary>
    public abstract class AbstractNodeKey<TBounds, TLocation>
        where TBounds : IContainable<TBounds>, IIntersectable<TBounds>
        where TLocation : IAddable<TLocation>, IDivisible<Double, TLocation>
    {
        // the fields which make up the key
        private TLocation _location;
        private readonly Int32 _level;

        // auxiliary data which is derived from the key for use in computation
        private TBounds _bounds;

        protected AbstractNodeKey(TBounds bounds)
        {
            _location = default(TLocation);
            _level = ComputeLevel(bounds);
            _bounds = bounds;

            computeKey(_level, _bounds);

            // TODO: MD - would be nice to have a non-iterative form of this algorithm
            while (!_bounds.Contains(bounds))
            {
                _level += 1;
                computeKey(_level, bounds);
            }
        }

        public TLocation Value
        {
            get { return _location; }
        }

        public Int32 Level
        {
            get { return _level; }
        }

        public TBounds Bounds
        {
            get { return _bounds; }
        }

        public TLocation Center
        {
            get
            {
                TLocation min = GetBoundsMin();
                TLocation max = GetBoundsMax();

                return min.Add(max).Divide(2.0);
            }
        }

        protected abstract TLocation GetBoundsMax();

        protected abstract TLocation GetBoundsMin();

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        //protected abstract void ComputeKey(TBounds bounds);

        protected abstract Int32 ComputeLevel(TBounds bounds);

        protected abstract TBounds CreateBounds(TLocation min, Double nodeSize);

        protected abstract TLocation CreateLocation(TBounds bounds, Double nodeSize);

        private void computeKey(Int32 level, TBounds bounds)
        {
            Double nodeSize = DoubleBits.PowerOf2(level);
            _location = CreateLocation(bounds, nodeSize);
            _bounds = CreateBounds(_location, nodeSize);
        }
    }
}