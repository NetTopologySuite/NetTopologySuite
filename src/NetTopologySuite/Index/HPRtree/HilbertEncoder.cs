using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Shape.Fractal;

namespace NetTopologySuite.Index.HPRtree
{
    public class HilbertEncoder
    {
        private readonly int _level;
        private readonly double _minx;
        private readonly double _miny;
        private readonly double _strideX;
        private readonly double _strideY;

        public HilbertEncoder(int level, Envelope extent)
        {
            _level = level;
            int hside = (int)Math.Pow(2, level) - 1;

            _minx = extent.MinX;
            _strideX = extent.Width / hside;

            _miny = extent.MinY;
            _strideY = extent.Height / hside;
        }

        public int Encode(Envelope env)
        {
            double midx = env.Width / 2 + env.MinX;
            int x = (int)((midx - _minx) / _strideX);

            double midy = env.Height / 2 + env.MinY;
            int y = (int)((midy - _miny) / _strideY);

            return HilbertCode.Encode(_level, x, y);
        }

    }
}
