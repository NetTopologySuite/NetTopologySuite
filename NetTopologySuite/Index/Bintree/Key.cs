using System;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A Key is a unique identifier for a node in a tree.
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </summary>
    public class Key
    {
        public static Int32 ComputeLevel(Interval interval)
        {
            Double dx = interval.Width;
            Int32 level = DoubleBits.GetExponent(dx) + 1;
            return level;
        }

        // the fields which make up the key
        private Double pt = 0.0;
        private Int32 level = 0;

        // auxiliary data which is derived from the key for use in computation
        private Interval interval;

        public Key(Interval interval)
        {
            ComputeKey(interval);
        }

        public Double Point
        {
            get { return pt; }
        }

        public Int32 Level
        {
            get { return level; }
        }

        public Interval Interval
        {
            get { return interval; }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        public void ComputeKey(Interval itemInterval)
        {
            level = ComputeLevel(itemInterval);
            interval = new Interval();
            ComputeInterval(level, itemInterval);

            // MD - would be nice to have a non-iterative form of this algorithm
            while (!interval.Contains(itemInterval))
            {
                level += 1;
                ComputeInterval(level, itemInterval);
            }
        }

        private void ComputeInterval(Int32 level, Interval itemInterval)
        {
            Double size = DoubleBits.PowerOf2(level);
            pt = Math.Floor(itemInterval.Min/size)*size;
            interval.Init(pt, pt + size);
        }
    }
}