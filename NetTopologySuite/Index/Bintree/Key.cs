using System;
using GeoAPI.DataStructures;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A <see cref="Key"/> is a unique identifier for a node in a tree.
    /// </summary>
    /// <remarks>
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </remarks>
    public struct Key
    {
        public static Int32 ComputeLevel(Interval interval)
        {
            Double dx = interval.Width;
            Int32 level = DoubleBits.GetExponent(dx) + 1;
            return level;
        }

        // the fields which make up the key
        private Double _value;
        private Int32 _level;

        // auxiliary data which is derived from the key for use in computation
        private Interval _interval;

        public Key(Interval interval)
        {
            _interval = new Interval();
            _level = 0;
            _value = 0;

            computeKey(interval);
        }

        public Double Point
        {
            get { return _value; }
        }

        public Int32 Level
        {
            get { return _level; }
        }

        public Interval Interval
        {
            get { return _interval; }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        private void computeKey(Interval itemInterval)
        {
            _level = ComputeLevel(itemInterval);
            _interval = new Interval();
            computeInterval(_level, itemInterval);

            // MD - would be nice to have a non-iterative form of this algorithm
            while (!_interval.Contains(itemInterval))
            {
                _level += 1;
                computeInterval(_level, itemInterval);
            }
        }

        private void computeInterval(Int32 level, Interval itemInterval)
        {
            Double size = DoubleBits.PowerOf2(level);
            _value = Math.Floor(itemInterval.Min / size) * size;
            _interval = new Interval(_value, _value + size);
        }
    }
}