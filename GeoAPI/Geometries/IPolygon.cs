using System;
using System.Collections.Generic;
using System.Text;

namespace GeoAPI.Geometries
{
    public interface IPolygon : ISurface
    {
        ILineString ExteriorRing { get; }

        ILinearRing Shell { get; }

        int NumInteriorRings { get; }

        ILineString[] InteriorRings { get; }

        ILineString GetInteriorRingN(int n);

        ILinearRing[] Holes { get; }  
    }
}
