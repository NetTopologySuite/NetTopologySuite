using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface ICurve : IGeometry
    {
        IPoint StartPoint { get; }
        
        IPoint EndPoint { get; }

        bool IsClosed { get; }
        
        bool IsRing { get; }        
    }
}
