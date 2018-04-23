using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoMultiPolygon : TopoObject
    {
        public TopoMultiPolygon(string type, int[][][] arcs)
            : base(type)
        {
            if (arcs == null)
                throw new ArgumentNullException("arcs");
            Arcs = arcs;
        }

        public int[][][] Arcs { get; }
    }
}