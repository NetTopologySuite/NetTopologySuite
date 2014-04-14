using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoLineString : TopoObject
    {
        private readonly int[] _arcs;

        public TopoLineString(string type, int[][][] arcs)
            : base(type)
        {
            if (arcs == null)
                throw new ArgumentNullException("arcs");
            if (arcs.Length == 0)
                throw new ArgumentException("arcs empty");
            if (arcs.Length > 1)
                throw new ArgumentException("arcs too long");
            int[][] arc = arcs[0];
            if (arc.Length == 0)
                throw new ArgumentException("arc empty");
            if (arc.Length > 1)
                throw new ArgumentException("arc too long");
            _arcs = arc[0];
        }

        public int[] Arcs
        {
            get { return _arcs; }
        }
    }
}