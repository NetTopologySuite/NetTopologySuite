using System;
//using NetTopologySuite.DataStructures;
using NetTopologySuite.Index.Quadtree;

namespace NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A Key is a unique identifier for a node in a tree.
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </summary>
    public class Key
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public static int ComputeLevel(Interval interval)
        {
            double dx = interval.Width;
            int level = DoubleBits.GetExponent(dx) + 1;
            return level;
        }

        // the fields which make up the key
        private double _pt;
        private int _level;

        // auxiliary data which is derived from the key for use in computation
        private Interval _interval;

        /// <summary>
        ///
        /// </summary>
        /// <param name="interval"></param>
        public Key(Interval interval)
        {
            ComputeKey(interval);
        }

        /// <summary>
        ///
        /// </summary>
        public  double Point => _pt;

        /// <summary>
        ///
        /// </summary>
        public  int Level => _level;

        /// <summary>
        ///
        /// </summary>
        public  Interval Interval => _interval;

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        /// <param name="itemInterval"></param>
        public void ComputeKey(Interval itemInterval)
        {
            _level = ComputeLevel(itemInterval);
            _interval = new Interval();
            //_interval = Interval.Create();
            ComputeInterval(_level, itemInterval);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!_interval.Contains(itemInterval))
            {
                _level += 1;
                ComputeInterval(_level, itemInterval);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="level"></param>
        /// <param name="itemInterval"></param>
        private void ComputeInterval(int level, Interval itemInterval)
        {
            double size = DoubleBits.PowerOf2(level);
            _pt = Math.Floor(itemInterval.Min / size) * size;
            _interval.Init(_pt, _pt + size);
            //_interval = Interval.Create(_pt, _pt + size);
        }
    }
}
