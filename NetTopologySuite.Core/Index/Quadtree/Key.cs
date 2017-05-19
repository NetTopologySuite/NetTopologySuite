using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.Quadtree
{
    /// <summary>
    ///     A Key is a unique identifier for a node in a quadtree.
    ///     It contains a lower-left point and a level number. The level number
    ///     is the power of two for the size of the node envelope.
    /// </summary>
    public class Key
    {
        // the fields which make up the key

        // auxiliary data which is derived from the key for use in computation

        /// <summary>
        /// </summary>
        /// <param name="itemEnv"></param>
        public Key(Envelope itemEnv)
        {
            ComputeKey(itemEnv);
        }

        /// <summary>
        /// </summary>
        public Coordinate Point { get; } = new Coordinate();

        /// <summary>
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// </summary>
        public Envelope Envelope { get; private set; }

        /// <summary>
        /// </summary>
        public Coordinate Centre => new Coordinate((Envelope.MinX + Envelope.MaxX)/2, (Envelope.MinY + Envelope.MaxY)/2)
            ;

        /// <summary>
        /// </summary>
        /// <param name="env"></param>
        /// <returns></returns>
        public static int ComputeQuadLevel(Envelope env)
        {
            var dx = env.Width;
            var dy = env.Height;
            var dMax = dx > dy ? dx : dy;
            var level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        /// <summary>
        ///     Return a square envelope containing the argument envelope,
        ///     whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        /// <param name="itemEnv"></param>
        public void ComputeKey(Envelope itemEnv)
        {
            Level = ComputeQuadLevel(itemEnv);
            Envelope = new Envelope();
            ComputeKey(Level, itemEnv);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!Envelope.Contains(itemEnv))
            {
                Level += 1;
                ComputeKey(Level, itemEnv);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="level"></param>
        /// <param name="itemEnv"></param>
        private void ComputeKey(int level, Envelope itemEnv)
        {
            var quadSize = DoubleBits.PowerOf2(level);
            Point.X = Math.Floor(itemEnv.MinX/quadSize)*quadSize;
            Point.Y = Math.Floor(itemEnv.MinY/quadSize)*quadSize;
            Envelope.Init(Point.X, Point.X + quadSize, Point.Y, Point.Y + quadSize);
        }
    }
}