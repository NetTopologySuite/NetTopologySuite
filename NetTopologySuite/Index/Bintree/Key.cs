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
        private double pt = 0.0;
        private int level = 0;

        // auxiliary data which is derived from the key for use in computation
        private Interval interval;

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
        public  double Point
        {
            get
            {
                return pt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  int Level
        {
            get
            {
                return level;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public  Interval Interval
        {
            get
            {
                return interval;
            }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        /// <param name="itemInterval"></param>
        public  void ComputeKey(Interval itemInterval)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="itemInterval"></param>
        private void ComputeInterval(int level, Interval itemInterval)
        {
            double size = DoubleBits.PowerOf2(level);            
            pt = Math.Floor(itemInterval.Min / size) * size;
            interval.Init(pt, pt + size);
        }
    }
}
