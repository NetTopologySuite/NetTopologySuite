using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoCollection : TopoObject
    {
        private readonly TopoObject[] _geometries;

        public TopoCollection(string type, TopoObject[] geometries)
            : base(type)
        {
            if (geometries == null)
                throw new ArgumentNullException("geometries");
            _geometries = geometries;
        }

        public TopoObject[] Geometries
        {
            get { return _geometries; }
        }
    }
}