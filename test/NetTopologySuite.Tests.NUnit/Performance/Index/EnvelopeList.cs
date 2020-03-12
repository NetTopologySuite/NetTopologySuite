using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Performance.Index
{
    public class EnvelopeList
    {
        private readonly List<Envelope> _envList = new List<Envelope>();

        public void Add(Envelope env)
        {
            _envList.Add(env);
        }

        public IList<Envelope> Query(Envelope searchEnv)
        {
            var result = new List<Envelope>();
            foreach (var env in _envList)
            {
                if (env.Intersects(searchEnv))
                    result.Add(env);
            }
            return result;
        }


    }
}
