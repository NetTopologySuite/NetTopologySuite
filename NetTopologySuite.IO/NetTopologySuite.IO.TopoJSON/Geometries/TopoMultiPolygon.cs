using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoMultiPolygon : TopoObject
    {
        private readonly int[][][] _arcs;

        public TopoMultiPolygon(string type, int[][][] arcs)
            : base(type)
        {
            if (arcs == null)
                throw new ArgumentNullException("arcs");
            _arcs = arcs;
        }

        public int[][][] Arcs
        {
            get { return _arcs; }
        }
    }
}