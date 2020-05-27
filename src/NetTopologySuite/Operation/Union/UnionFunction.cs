using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Union
{
    public class UnionFunction
    {
        private readonly Func<Geometry, Geometry, Geometry> _unionFunction;

        internal UnionFunction(Func<Geometry, Geometry, Geometry> func)
        {
            _unionFunction = func;
        }

        public Geometry Union(Geometry g0, Geometry g1)
        {
            return _unionFunction(g0, g1);
        }
    }
}
