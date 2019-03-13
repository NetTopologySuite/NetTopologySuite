using System;

using GeoAPI.Geometries;

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

        public string Name => _fun.Name;

        public string[] ParameterNames => _fun.ParameterNames;

        public Type[] ParameterTypes => _fun.ParameterTypes;

        public Type ReturnType => _fun.ReturnType;

        public string Signature => _fun.Signature;

        public bool IsBinary => _fun.IsBinary;

        public object Invoke(IGeometry geom, object[] args)
        {
            var results = new IGeometry[geom.NumGeometries];
            for (int i = 0; i < results.Length; i++)
            {
                var elt = geom.GetGeometryN(i);
                var result = (IGeometry)_fun.Invoke(elt, args);
                ////FunctionsUtil.showIndicator(result);
                results[i] = result;
            }

            return geom.Factory.CreateGeometryCollection(results);
        }
    }
}
