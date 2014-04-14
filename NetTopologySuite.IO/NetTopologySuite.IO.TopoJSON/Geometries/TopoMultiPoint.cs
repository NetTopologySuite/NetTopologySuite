using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoMultiPoint : TopoObject
    {
        private readonly double[][] _coordinates;

        public TopoMultiPoint(string type, double[][] coordinates)
            : base(type)
        {
            if (coordinates == null)
                throw new ArgumentNullException("coordinates");
            _coordinates = coordinates;
        }

        public double[][] Coordinates
        {
            get { return _coordinates; }
        }
    }
}