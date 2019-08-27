using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public static class BufferByUnionFunctions
    {
        public static Geometry ComponentBuffers(Geometry g, double distance)
        {
            var bufs = new List<Geometry>();
            foreach (var comp in new GeometryCollectionEnumerator((GeometryCollection)g))
            {
                if (comp is GeometryCollection) continue;
                bufs.Add(comp.Buffer(distance));
            }
            return FunctionsUtil.GetFactoryOrDefault(g)
                .CreateGeometryCollection(GeometryFactory.ToGeometryArray(bufs));
        }

        public static Geometry BufferByComponents(Geometry g, double distance)
        {
            return ComponentBuffers(g, distance).Union();
        }

        /// <summary>
        /// Buffer polygons by buffering the individual boundary segments and
        /// either unioning or differencing them.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="distance"></param>
        /// <returns>The buffer geometry</returns>
        public static Geometry BufferBySegments(Geometry g, double distance)
        {
            var segs = LineHandlingFunctions.ExtractSegments(g);
            double posDist = Math.Abs(distance);
            var segBuf = BufferByComponents(segs, posDist);
            if (distance < 0.0)
                return g.Difference(segBuf);
            return g.Union(segBuf);
        }

        public static Geometry BufferByChains(Geometry g, double distance, int maxChainSize)
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