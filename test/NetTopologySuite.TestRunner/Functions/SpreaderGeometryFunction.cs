using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class SpreaderGeometryFunction : IGeometryFunction
    {
        private readonly IGeometryFunction _fun;
        private readonly bool _isEachA;
        private readonly bool _isEachB;

        public SpreaderGeometryFunction(IGeometryFunction fun, bool isEachA, bool isEachB)
        {
            _fun = fun;
            _isEachA = isEachA;
            _isEachB = isEachB;
        }

        public string Category => _fun.Category;

        public string Name
        {
            get
            {
                string name = _fun.Name;
                if (_isEachA) name += "*A";
                if (_isEachB) name += "*B";

                return name;
            }
        }

        public string[] ParameterNames => _fun.ParameterNames;

        public Type[] ParameterTypes => _fun.ParameterTypes;

        public Type ReturnType => _fun.ReturnType;

        public string Signature => _fun.Signature;

        public bool IsBinary => _fun.IsBinary;

        public object OLDInvoke(Geometry geom, object[] args)
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
            var result = GeometryMapper.Map(geom, g => (Geometry)_fun.Invoke(g, args));
            if (result.IsEmpty) return null;
            return result;
#endif
        }

        public object Invoke(Geometry geom, object[] args)
        {
            var result = new List<Geometry>();
            if (_isEachA)
            {
                InvokeEachA(geom, args, result);
            }
            else
            {
                InvokeB(geom, args, result);
            }
            return CreateResult(result, geom.Factory);
        }

        private static object CreateResult(List<Geometry> result, GeometryFactory geometryFactory)
        {
            if (result.Count == 1)
            {
                return result[0];
            }
            var resultGeoms = result.ToArray();
            return geometryFactory.CreateGeometryCollection(resultGeoms);
        }

        private void InvokeEachA(Geometry geom, object[] args, List<Geometry> result)
        {
            int nElt = geom.NumGeometries;
            for (int i = 0; i < nElt; i++)
            {
                var geomN = geom.GetGeometryN(i);
                InvokeB(geomN, args, result);
            }
        }

        private void InvokeB(Geometry geom, object[] args, List<Geometry> result)
        {
            if (HasBGeom(args) && _isEachB)
            {
                InvokeEachB(geom, args, result);
                return;
            }
            InvokeFun(geom, args, result);
        }

        private static bool HasBGeom(object[] args)
        {
            if (args.Length <= 0) return false;
            return args[0] is Geometry;
        }

        private void InvokeEachB(Geometry geom, object[] args, List<Geometry> result)
        {
            var geomB = (Geometry)args[0];
            object[] argsCopy = (object[])args.Clone();
            int nElt = geomB.NumGeometries;
            for (int i = 0; i < nElt; i++)
            {
                var geomBN = geomB.GetGeometryN(i);
                argsCopy[0] = geomBN;
                InvokeFun(geom, argsCopy, result);
            }
        }

        private void InvokeFun(Geometry geom, object[] args, List<Geometry> result)
        {
            var resultGeom = (Geometry)_fun.Invoke(geom, args);
            if (resultGeom == null) return;
            if (resultGeom.IsEmpty) return;
            //FunctionsUtil.showIndicator(resultGeom);
            result.Add(resultGeom);
        }


    }
}
