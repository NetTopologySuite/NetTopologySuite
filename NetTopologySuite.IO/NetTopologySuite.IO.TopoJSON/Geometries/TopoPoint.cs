using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoPoint : TopoObject
    {
        public TopoPoint(string type, double[][] coordinates)
            : base(type)
        {
            if (coordinates == null)
                throw new ArgumentNullException("coordinates");
            if (coordinates.Length == 0)
                throw new ArgumentException("coordinates empty");
            if (coordinates.Length > 1)
                throw new ArgumentException("coordinates too long");
            Coordinates = coordinates[0];
        }

        public double[] Coordinates { get; }
    }
}