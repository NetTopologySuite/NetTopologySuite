using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    public class BufferByUnionFunctions
    {

        public static IGeometry ComponentBuffers(IGeometry g, double distance)	
        {		
            var bufs = new List<IGeometry>();
            foreach (var comp in new GeometryCollectionEnumerator((IGeometryCollection)g))
            {
                if (comp is IGeometryCollection) continue;
                bufs.Add(comp.Buffer(distance));
            }
            return FunctionsUtil.getFactoryOrDefault(g)
                .CreateGeometryCollection(GeometryFactory.ToGeometryArray(bufs));
        }

        public static IGeometry BufferByComponents(IGeometry g, double distance)
        {
            return ComponentBuffers(g, distance).Union();
        }

        /**
         * Buffer polygons by buffering the individual boundary segments and
         * either unioning or differencing them.
         * 
         * @param g
         * @param distance
         * @return
         */
        public static IGeometry BufferBySegments(IGeometry g, double distance)
        {
            var segs = LineHandlingFunctions.ExtractSegments(g);
            var posDist = Math.Abs(distance);
            var segBuf = BufferByComponents(segs, posDist);
            if (distance < 0.0)
                return g.Difference(segBuf);
            return g.Union(segBuf);
        }

        public static IGeometry BufferByChains(IGeometry g, double distance, int maxChainSize)
        {
            if (maxChainSize <= 0)
                throw new ArgumentOutOfRangeException("maxChainSize", "Maximum Chain Size must be specified as an input parameter");
            var segs = LineHandlingFunctions.ExtractChains(g, maxChainSize);
            double posDist = Math.Abs(distance);
            var segBuf = BufferByComponents(segs, posDist);
            if (distance < 0.0)
                return g.Difference(segBuf);
            return g.Union(segBuf);
        }
    }
}