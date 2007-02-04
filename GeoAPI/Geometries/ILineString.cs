using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface ILineString : ICurve
    {
        IPoint GetPointN(int n);
    }
}
