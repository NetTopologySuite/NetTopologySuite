using System.Collections.Generic;

namespace GeoAPI.Geometries
{
    public interface IGeometryCollection : IGeometry, IEnumerable<IGeometry>
    {        
        int Count { get; }

        IGeometry[] Geometries { get; }

        IGeometry this[int i] { get; }

        bool IsHomogeneous { get; }                
    }
}
