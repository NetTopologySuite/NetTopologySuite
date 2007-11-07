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
        public static Int32 ComputeQuadLevel(IExtents env)
        {
            Double dx = env.Width;
            Double dy = env.Height;
            Double dMax = dx > dy ? dx : dy;
            Int32 level = DoubleBits.GetExponent(dMax) + 1;
            return level;
        }

        // the fields which make up the key
        private ICoordinate pt = new Coordinate();
        private Int32 level = 0;

        // auxiliary data which is derived from the key for use in computation
        private IExtents env = null;

        public Key(IExtents itemEnv)
        {
            ComputeKey(itemEnv);
        }

        public ICoordinate Point
        {
            get { return pt; }
        }

        public Int32 Level
        {
            get { return level; }
        }

        public IExtents Envelope
        {
            get { return env; }
        }

        public ICoordinate Centre
        {
            get { return new Coordinate((env.MinX + env.MaxX)/2, (env.MinY + env.MaxY)/2); }
        }

        /// <summary>
        /// Return a square envelope containing the argument envelope,
        /// whose extent is a power of two and which is based at a power of 2.
        /// </summary>
        public void ComputeKey(IExtents itemEnv)
        {
            level = ComputeQuadLevel(itemEnv);
            env = new Extents();
            ComputeKey(level, itemEnv);
            // MD - would be nice to have a non-iterative form of this algorithm
            while (!env.Contains(itemEnv))
            {
                level += 1;
                ComputeKey(level, itemEnv);
            }
        }

        private void ComputeKey(Int32 level, IExtents itemEnv)
        {
            Double quadSize = DoubleBits.PowerOf2(level);
            pt.X = Math.Floor(itemEnv.MinX/quadSize)*quadSize;
            pt.Y = Math.Floor(itemEnv.MinY/quadSize)*quadSize;
            env.Init(pt.X, pt.X + quadSize, pt.Y, pt.Y + quadSize);
        }
    }
}