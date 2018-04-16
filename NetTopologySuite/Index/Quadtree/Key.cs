using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary> 
    /// A Key is a unique identifier for a node in a quadtree.
    /// It contains a lower-left point and a level number. The level number
    /// is the power of two for the size of the node envelope.
    /// </summary>
    public class Key
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static int ComputeQuadLevel(Envelope env)
        {
            double dx = env.Width;
            double dy = env.Height;
            double dMax = dx > dy ? dx : dy;
            int level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        // the fields which make up the key

        // auxiliary data which is derived from the key for use in computation
        private Envelope _env;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemEnv"></param>
        public Key(Envelope itemEnv)
        {
            ComputeKey(itemEnv);
        }

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Point { get; } = new Coordinate();

        /// <summary>
        /// 
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Envelope Envelope => _env;

        /// <summary>
        /// 
        /// </summary>
        public Coordinate Centre => new Coordinate((_env.MinX + _env.MaxX) / 2, (_env.MinY + _env.MaxY) / 2);

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        /// <param name="itemEnv"></param>
        public void ComputeKey(Envelope itemEnv)
        {
            Level = ComputeQuadLevel(itemEnv);
            _env = new Envelope();
            ComputeKey(Level, itemEnv);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!_env.Contains(itemEnv))
            {
                Level += 1;
                ComputeKey(Level, itemEnv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="itemEnv"></param>
        private void ComputeKey(int level, Envelope itemEnv)
        {
            double quadSize = DoubleBits.PowerOf2(level);            
            Point.X = Math.Floor(itemEnv.MinX / quadSize) * quadSize;
            Point.Y = Math.Floor(itemEnv.MinY / quadSize) * quadSize;
            _env.Init(Point.X, Point.X + quadSize, Point.Y, Point.Y + quadSize);
        }
    }
}
