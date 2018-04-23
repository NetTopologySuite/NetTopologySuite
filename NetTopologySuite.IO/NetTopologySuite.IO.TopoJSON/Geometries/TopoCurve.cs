using System;

namespace NetTopologySuite.IO.Geometries
{
    internal abstract class TopoCurve : TopoObject
    {
        public TopoCurve(string type, int[][][] arcs)
            : base(type)
        {
            if (arcs == null)
                throw new ArgumentNullException("arcs");
            if (arcs.Length == 0)
                throw new ArgumentException("arcs empty");
            if (arcs.Length > 1)
                throw new ArgumentException("arcs too long");
            Arcs = arcs[0];
        }

        public int[][] Arcs { get; }
    }
}