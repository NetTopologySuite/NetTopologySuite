using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Hull
{
    internal class RingHullIndex
    {

        //TODO: use a proper spatial index
        List<RingHull> hulls = new List<RingHull>();

        public void Add(RingHull ringHull)
        {
            hulls.Add(ringHull);
        }

        public List<RingHull> Query(Envelope queryEnv)
        {
            var result = new List<RingHull>();
            foreach (var hull in hulls)
            {
                var envHull = hull.Envelope;
                if (queryEnv.Intersects(envHull))
                {
                    result.Add(hull);
                }
            }
            return result;
        }

    }
}
