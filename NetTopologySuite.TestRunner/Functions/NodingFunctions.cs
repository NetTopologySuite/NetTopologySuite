using System.Collections;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Precision;
using NetTopologySuite.Utilities;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class NodingFunctions
    {

        public static IGeometry nodeWithPointwisePrecision(IGeometry geom, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);

            var roundedGeom = SimpleGeometryPrecisionReducer.Reduce(geom, pm);

            var geomList = new List<IGeometry>();
            geomList.Add(roundedGeom);

            var noder = new GeometryNoder(pm);
            var lines = noder.Node(geomList);

            return FunctionsUtil.getFactoryOrDefault(geom).BuildGeometry(CollectionUtil.Cast<IGeometry>((ICollection)lines));
        }

    }
}