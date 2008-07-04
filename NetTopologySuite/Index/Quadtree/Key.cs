using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Index.Quadtree
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
        public static int ComputeQuadLevel(IEnvelope env)
        {
            double dx = env.Width;
            double dy = env.Height;
            double dMax = dx > dy ? dx : dy;
            int level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        // the fields which make up the key
        private ICoordinate pt = new Coordinate();
        private int level = 0;

        // auxiliary data which is derived from the key for use in computation
        private IEnvelope env = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemEnv"></param>
        public Key(IEnvelope itemEnv)
        {
            ComputeKey(itemEnv);
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Point
        {
            get
            {
                return pt;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Level
        {
            get
            {
                return level;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IEnvelope Envelope
        {
            get
            {
                return env;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Centre
        {
            get
            {
                return new Coordinate((env.MinX + env.MaxX) / 2, (env.MinY + env.MaxY) / 2);
            }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        /// <param name="itemEnv"></param>
        public void ComputeKey(IEnvelope itemEnv)
        {
            level = ComputeQuadLevel(itemEnv);
            env = new Envelope();
            ComputeKey(level, itemEnv);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!env.Contains(itemEnv))
            {
                level += 1;
                ComputeKey(level, itemEnv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="itemEnv"></param>
        private void ComputeKey(int level, IEnvelope itemEnv)
        {
            double quadSize = DoubleBits.PowerOf2(level);            
            pt.X = Math.Floor(itemEnv.MinX / quadSize) * quadSize;
            pt.Y = Math.Floor(itemEnv.MinY / quadSize) * quadSize;
            env.Init(pt.X, pt.X + quadSize, pt.Y, pt.Y + quadSize);
        }
    }
}
