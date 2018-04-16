using System;

namespace NetTopologySuite.IO.Geometries
{
    internal class TopoCollection : TopoObject
    {
        public TopoCollection(string type, TopoObject[] geometries)
            : base(type)
        {
            if (geometries == null)
                throw new ArgumentNullException("geometries");
            Geometries = geometries;
        }

        public TopoObject[] Geometries { get; }
    }
}