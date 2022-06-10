using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Simplify
{
    internal class RingHullIndex
    {

        //TODO: use a proper spatial index
        private readonly List<RingHull> _hulls = new List<RingHull>();

        public void Add(RingHull ringHull)
        {
            _hulls.Add(ringHull);
        }

        public List<RingHull> Query(Envelope queryEnv)
        {
            var result = new List<RingHull>();
            foreach (var hull in _hulls)
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
