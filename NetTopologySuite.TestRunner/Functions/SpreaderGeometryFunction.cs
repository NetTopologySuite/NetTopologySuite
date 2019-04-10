using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class SpreaderGeometryFunction : IGeometryFunction
    {
        private readonly IGeometryFunction _fun;

        public SpreaderGeometryFunction(IGeometryFunction fun)
        {
            _fun = fun;
        }

        public string Category => _fun.Category;

        public string Name => $"{_fun.Name}-Each";

        public string[] ParameterNames => _fun.ParameterNames;

        public Type[] ParameterTypes => _fun.ParameterTypes;

        public Type ReturnType => _fun.ReturnType;

        public string Signature => _fun.Signature;

        public bool IsBinary => _fun.IsBinary;

        public object Invoke(Geometry geom, object[] args)
        {
#if false
            var results = new Geometry[geom.NumGeometries];
            for (int i = 0; i < results.Length; i++)
            {
                var elt = geom.GetGeometryN(i);
                var result = (Geometry)_fun.Invoke(elt, args);

                // can't include null results
                if (result == null)
                {
                    continue;
                }

                ////FunctionsUtil.showIndicator(result);
                results[i] = result;
            }

            return geom.Factory.CreateGeometryCollection(results);
#else
            return GeometryMapper.Map(geom, g => (Geometry)_fun.Invoke(g, args));
#endif
        }
    }
}
