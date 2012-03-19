using System;
using System.Collections.Generic;
using System.Reflection;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Utility;

namespace Open.Topology.TestRunner.Functions
{
    /**
     * A registry to manage a collection of {@link GeometryFunction}s.
     * 
     * @author Martin Davis
     *
     */

    public class GeometryFunctionRegistry
    {
        private readonly List<IGeometryFunction> _functions = new List<IGeometryFunction>();

        private readonly SortedDictionary<string, IGeometryFunction> _sortedFunctions =
            new SortedDictionary<string, IGeometryFunction>();

        private readonly DoubleKeyMap<string, string, IGeometryFunction> _categorizedFunctions =
            new DoubleKeyMap<string, string, IGeometryFunction>();

        private readonly DoubleKeyMap<string, string, IGeometryFunction> _categorizedGeometryFunctions =
            new DoubleKeyMap<string, string, IGeometryFunction>();

        public GeometryFunctionRegistry()
        {
        }

        public GeometryFunctionRegistry(Type clz)
        {
            Add(clz);
        }

        public List<IGeometryFunction> Functions
        {
            get { return _functions; }
        }

        public IList<IGeometryFunction> GetGeometryFunctions()
        {
            var funList = new List<IGeometryFunction>();
            foreach (var fun in _sortedFunctions.Values)
            {
                if (HasGeometryResult(fun))
                    funList.Add(fun);
            }
            return funList;
        }

        public static bool HasGeometryResult(IGeometryFunction func)
        {
            return typeof (IGeometry).IsAssignableFrom(func.ReturnType);
        }

        public IList<IGeometryFunction> GetScalarFunctions()
        {
            var scalarFun = new List<IGeometryFunction>();
            foreach (IGeometryFunction fun in _sortedFunctions.Values)
            {
                if (!HasGeometryResult(fun))
                    scalarFun.Add(fun);
            }
            return scalarFun;
        }

        /**
         * Adds functions for all the static methods in the given class.
         * 
         * @param geomFuncClass
         */

        public void Add(Type geomFuncClass)
        {
            var funcs = CreateFunctions(geomFuncClass);
            // sort list of functions so they appear nicely in the UI list
            funcs.Sort();
            Add(funcs);
        }

        /**
         * Adds functions for all the static methods in the given class.
         * 
         * @param geomFuncClassname the name of the class to load and extract functions from
         */
        //public void Add(String geomFuncClassname)
        //{
        //    Type geomFuncClass = LoadClass(geomFuncClassname);
        //    Add(geomFuncClass);
        //}


        public void Add(IEnumerable<IGeometryFunction> funcs)
        {
            foreach (var f in funcs) Add(f);
        }

        /**
         * Create {@link GeometryFunction}s for all the static 
         * methods in the given class
         * 
         * @param functionClass
         * @return a list of the functions created
         */

        public List<IGeometryFunction> CreateFunctions(Type functionClass)
        {
            var funcs = new List<IGeometryFunction>();
            MethodInfo[] method = functionClass.GetMethods();
            for (int i = 0; i < method.Length; i++)
            {
                if (method[i].IsStatic && method[i].IsPublic)
                {
                    funcs.Add(StaticMethodGeometryFunction.CreateFunction(method[i]));
                }
            }
            return funcs;
        }


        /**
         * Adds a function if it does not currently
       * exist in the registry, or replaces the existing one
         * with the same signature.
         * 
         * @param func a function
         */

        public void Add(IGeometryFunction func)
        {
            _functions.Add(func);
            _sortedFunctions.Add(func.Name, func);
            _categorizedFunctions.Put(func.Category, func.Name, func);
            if (HasGeometryResult(func))
                _categorizedGeometryFunctions.Put(func.Category, func.Name, func);
        }

        public DoubleKeyMap<string, string, IGeometryFunction> CategorizedGeometryFunctions
        {
            get { return _categorizedGeometryFunctions; }
        }

        public ICollection<string> Categories
        {
            get { return _categorizedFunctions.KeySet(); }
        }

        public ICollection<IGeometryFunction> GetFunctions(String category)
        {
            return _categorizedFunctions.Values(category);
        }

        /*
            int index = functions.indexOf(func);
            if (index == -1) {
                sortedFunctions.put(func.getName(), func);
            }
            else {
                functions.set(index, func);
            }	
        }
        */


        /**
         * Finds the first function which matches the given signature.
         * 
         * @param name
         * @param paramTypes
         * @return a matching function
         * @return null if no matching function was found
         */

        public IGeometryFunction Find(String name, Type[] paramTypes)
        {
            return null;
        }

        /**
         * Finds the first function which matches the given name and argument count.
         * 
         * @param name
         * @return a matching function
         * @return null if no matching function was found
         */

        public IGeometryFunction Find(String name, int argCount)
        {
            foreach (var func in _functions)
            {
                String funcName = func.Name;
                if (funcName.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                    && func.ParameterTypes.Length == argCount)
                    return func;
            }
            return null;
        }

        /**
         * Finds the first function which matches the given name.
         * 
         * @param name
         * @return a matching function
         * @return null if no matching function was found
         */

        public IGeometryFunction Find(String name)
        {
            foreach (var func in _functions)
            {
                var funcName = func.Name;
                if (funcName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return func;
            }
            return null;
        }
    }
}