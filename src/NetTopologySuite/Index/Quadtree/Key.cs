using System;
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
        private readonly Coordinate _pt = new Coordinate();
        private int _level;

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
        public Coordinate Point => _pt;

        /// <summary>
        ///
        /// </summary>
        public int Level => _level;

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
            _level = ComputeQuadLevel(itemEnv);
            _env = new Envelope();
            ComputeKey(_level, itemEnv);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!_env.Contains(itemEnv))
            {
                _level += 1;
                ComputeKey(_level, itemEnv);
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
            _pt.X = Math.Floor(itemEnv.MinX / quadSize) * quadSize;
            _pt.Y = Math.Floor(itemEnv.MinY / quadSize) * quadSize;
            _env.Init(_pt.X, _pt.X + quadSize, _pt.Y, _pt.Y + quadSize);
        }
    }
}
