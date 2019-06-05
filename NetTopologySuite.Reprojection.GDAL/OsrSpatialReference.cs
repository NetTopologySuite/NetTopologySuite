using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using OSGeo.OSR;

namespace NetTopologySuite.Reprojection
{
    public class OsrSpatialReference : SpatialReference, IDisposable
    {
        internal OsrSpatialReference(string wkt, GeometryFactory factory, OSGeo.OSR.SpatialReference instance)
            :base(wkt,factory)
        {
            Instance = instance;
        }

        internal OSGeo.OSR.SpatialReference Instance { get; }

        void IDisposable.Dispose()
        {
            Instance?.Dispose();
        }
    }
}
