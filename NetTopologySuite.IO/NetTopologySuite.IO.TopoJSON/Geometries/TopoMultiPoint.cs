using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoMultiPoint : TopoObject
    {
        public TopoMultiPoint(string type, double[][] coordinates)
            : base(type)
        {
            if (coordinates == null)
                throw new ArgumentNullException("coordinates");
            Coordinates = coordinates;
        }

        public double[][] Coordinates { get; }
    }
}