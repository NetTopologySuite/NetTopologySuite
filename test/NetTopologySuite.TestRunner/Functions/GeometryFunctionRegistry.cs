using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    /// <summary>
    /// A registry to manage a collection of <see cref="IGeometryFunction"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryFunctionRegistry
    {
        private readonly List<IGeometryFunction> _functions = new List<IGeometryFunction>();

        private readonly SortedDictionary<string, IGeometryFunction> _sortedFunctions =
            new SortedDictionary<string, IGeometryFunction>();

        private readonly DoubleKeyMap<string, string, IGeometryFunction> _categorizedFunctions =
            new DoubleKeyMap<string, string, IGeometryFunction>();

        private readonly DoubleKeyMap<string, string, IGeometryFunction> _categorizedGeometryFunctions =
            new DoubleKeyMap<string, string, IGeometryFunction>();

        public GeometryFunctionRegistry() { }

        public GeometryFunctionRegistry(Type clz)
        {
            Add(clz);
        }

        public List<IGeometryFunction> Functions => _functions;

        public IList<IGeometryFunction> GetGeometryFunctions()
        {
            var funList = new List<IGeometryFunction>();
            foreach (var fun in _sortedFunctions.Values)
                if (HasGeometryResult(fun))
                    funList.Add(fun);
            return funList;
        }

        public static bool HasGeometryResult(IGeometryFunction func)
        {
            return typeof(Geometry).IsAssignableFrom(func.ReturnType);
        }

        public IList<IGeometryFunction> GetScalarFunctions()
        {
            var scalarFun = new List<IGeometryFunction>();
            foreach (var fun in _sortedFunctions.Values)
                if (!HasGeometryResult(fun))
                    scalarFun.Add(fun);
            return scalarFun;
        }

        /// <summary>
        /// Adds functions for all the static methods in the given class.
        /// </summary>
        /// <param name="geomFuncClass"></param>
        public void Add(Type geomFuncClass)
        {
            var funcs = CreateFunctions(geomFuncClass);
            // sort list of functions so they appear nicely in the UI list
            funcs.Sort();
            Add(funcs);
        }

        public void Add(IEnumerable<IGeometryFunction> funcs)
        {
            foreach (var f in funcs) Add(f);
        }

        /// <summary>
        /// Create <see cref="IGeometryFunction"/>s for all the static
        /// methods in the given class
        /// </summary>
        /// <param name="functionClass"></param>
        /// <returns>A list of the functions created</returns>
        public List<IGeometryFunction> CreateFunctions(Type functionClass)
        {
            var funcs = new List<IGeometryFunction>();
            var method = functionClass.GetMethods();
            for (int i = 0; i < method.Length; i++)
            {
                if (method[i].IsStatic && method[i].IsPublic)
                {
                    funcs.Add(StaticMethodGeometryFunction.CreateFunction(method[i]));
                }
            }
            return funcs;
        }

        /// <summary>
        /// Adds a function if it does not currently
        /// exist in the registry, or replaces the existing one
        /// with the same signature.
        /// </summary>
        /// <param name="func">A function</param>
        public void Add(IGeometryFunction func)
        {
            _functions.Add(func);
            _sortedFunctions.Add(func.Name, func);
            _categorizedFunctions.Put(func.Category, func.Name, func);
            if (HasGeometryResult(func))
                _categorizedGeometryFunctions.Put(func.Category, func.Name, func);
        }

        public DoubleKeyMap<string, string, IGeometryFunction> CategorizedGeometryFunctions => _categorizedGeometryFunctions;

        public ICollection<string> Categories => _categorizedFunctions.KeySet();

        public ICollection<IGeometryFunction> GetFunctions(string category)
        {
            return _categorizedFunctions.Values(category);
        }

        /// <summary>
        /// Finds the first function which matches the given signature.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="paramTypes"></param>
        /// <returns>A matching function<br/>or <c>null</c> if no matching function was found</returns>
        public IGeometryFunction Find(string name, Type[] paramTypes)
        {
            return null;
        }

        /// <summary>
        /// Finds the first function which matches the given signature.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="argCount"></param>
        /// <returns>A matching function<br/>or <c>null</c> if no matching function was found</returns>
        public IGeometryFunction Find(string name, int argCount)
        {
            foreach (var func in _functions)
            {
                string funcName = func.Name;
                if (funcName.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    && func.ParameterTypes.Length == argCount)
                    return func;
            }
            return null;
        }

        /// <summary>
        /// Finds the first function which matches the given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A matching function<br/>or <c>null</c> if no matching function was found</returns>
        public IGeometryFunction Find(string name)
        {
            foreach (var func in _functions)
            {
                string funcName = func.Name;
                if (funcName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return func;
            }
            return null;
        }

        /// <summary>
        /// Finds the first function which matches the given category and name.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="name"></param>
        /// <returns>A matching function or <c>null</c></returns>
        public IGeometryFunction Find(string category, string name)
        {
            foreach (var func in _functions)
            {
                if (string.Equals(category, func.Category, StringComparison.InvariantCultureIgnoreCase) &&
                    string.Equals(name, func.Name, StringComparison.InvariantCultureIgnoreCase))
                    return func;
            }
            return null;
        }
    }
}
