using System;

namespace NetTopologySuite.IO.TopoJSON.Geometries
{
    internal class TopoGeometryCollection : TopoObject
    {
        private readonly TopoObject[] _geometries;

        public TopoGeometryCollection(string type, TopoObject[] geometries)
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