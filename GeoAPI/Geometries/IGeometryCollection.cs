using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface IGeometryCollection : IGeometry
    {        
        int Count { get; }

        IGeometry[] Geometries { get; }

        IGeometry this[int i] { get; }

        bool IsHomogeneous { get; }                
    }
}
