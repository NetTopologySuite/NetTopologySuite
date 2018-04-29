using System;
using System.Globalization;
using System.Reflection;
using GeoAPI.Geometries;
using Open.Topology.TestRunner.Result;

namespace Open.Topology.TestRunner.Operations
{
    /**
     * Invokes a named operation on a set of arguments,
     * the first of which is a {@link Geometry}.
     * This class provides operations which are the methods 
     * defined on the Geometry class.
     * Other {@link GeometryOperation} classes can delegate to
     * instances of this class to run standard Geometry methods.
     *
     * @author Martin Davis
     * @version 1.7
     */

    public class GeometryMethodOperation : IGeometryOperation
    {
        public static bool IsBooleanFunction(String name)
        {
            return GetGeometryReturnType(name) == typeof (bool);
        }

        public static bool IsIntegerFunction(String name)
        {
            return GetGeometryReturnType(name) == typeof (int);
        }

        public static bool IsDoubleFunction(String name)
        {
            return GetGeometryReturnType(name) == typeof (double);
        }

        public static bool IsGeometryFunction(String name)
        {
            return typeof (IGeometry).IsAssignableFrom(GetGeometryReturnType(name));
        }


        public static Type GetGeometryReturnType(String functionName)
        {

            //MethodInfo[] methods = typeof(IGeometry).GetMethods();
            for (int i = 0; i < GeometryMethods.Length; i++)
            {
                if (GeometryMethods[i].Name.Equals(functionName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Type returnClass = GeometryMethods[i].ReturnType;
                    /**
                     * Filter out only acceptable classes. (For instance, don't accept the
                     * relate()=>IntersectionMatrix method)
                     */
                    if (returnClass == typeof (bool)
                        || typeof (IGeometry).IsAssignableFrom(returnClass)
                        || returnClass == typeof (double) || returnClass == typeof (int))
                    {
                        return returnClass;
                    }
                }
            }
            return null;
        }

        private static readonly MethodInfo[] GeometryMethods = typeof (IGeometry).GetMethods();

        public Type GetReturnType(XmlTestType op)
        {
            return GetReturnType(op.ToString());
        }

        public Type GetReturnType(String opName)
        {
            return GetGeometryReturnType(opName);
        }

        public IResult Invoke(XmlTestType opName, IGeometry geometry, Object[] args)
        {
            return Invoke(opName.ToString(), geometry, args);
        }

        public IResult Invoke(String opName, IGeometry geometry, Object[] args)
        {
            Object[] actualArgs = new Object[args.Length];
            MethodInfo geomMethod = GetGeometryMethod(opName, args, actualArgs);
            if (geomMethod == null)
                throw new NTSTestReflectionException(opName, args);
            return InvokeMethod(geomMethod, geometry, actualArgs);
        }

        private MethodInfo GetGeometryMethod(String opName, Object[] args, Object[] actualArgs)
        {
            // could index methods by name for efficiency...
            for (int i = 0; i < GeometryMethods.Length; i++)
            {
                if (!GeometryMethods[i].Name.Equals(opName, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }
                if (ConvertArgs(GeometryMethods[i].GetParameters(), args, actualArgs))
                {
                    return GeometryMethods[i];
                }
            }
            return null;
        }

        private static int NonNullItemCount(Object[] obj)
        {
            int count = 0;
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i] != null)
                    count++;
            }
            return count;
        }

        private readonly Object[] _convArg = new Object[1];

        private bool ConvertArgs(ParameterInfo[] parameterTypes, Object[] args, Object[] actualArgs)
        {
            if (parameterTypes.Length != NonNullItemCount(args))
                return false;

            for (int i = 0; i < args.Length; i++)
            {
                bool isCompatible = ConvertArg(parameterTypes[i].ParameterType, args[i], _convArg);
                if (!isCompatible)
                    return false;
                actualArgs[i] = _convArg[0];
            }
            return true;
        }

        private bool ConvertArg(Type destClass, Object srcValue, Object[] convArg)
        {
            convArg[0] = null;
            if (srcValue is string)
            {
                return convertArgFromString(destClass, (String) srcValue, convArg);
            }
            if (destClass.IsAssignableFrom(srcValue.GetType()))
            {
                convArg[0] = srcValue;
                return true;
            }
            return false;
        }

        private bool convertArgFromString(Type destClass, String srcStr, Object[] convArg)
        {
            convArg[0] = null;
            if (destClass == typeof (Boolean) || destClass == typeof (bool))
            {
                if (srcStr.Equals("true"))
                {
                    convArg[0] = true;
                    return true;
                }
                if (srcStr.Equals("false"))
                {
                    convArg[0] = false;
                    return true;
                }
                return false;
            }

            if (destClass == typeof (Int32) || destClass == typeof (int))
            {
                // try as an int
                try
                {
                    convArg[0] = int.Parse(srcStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                    return true;
                }
                catch (FormatException)
                {
                    // eat this exception
                }
                return false;
            }

            if (destClass == typeof (Double) || destClass == typeof (double))
            {
                // try as an int
                try
                {
                    convArg[0] = Double.Parse(srcStr, NumberStyles.Any, CultureInfo.InvariantCulture);
                    return true;
                }
                catch (FormatException)
                {
                    // eat this exception
                }
                return false;
            }
            if (destClass == typeof (String) || destClass == typeof (string))
            {
                convArg[0] = srcStr;
                return true;
            }
            return false;
        }


        private IResult InvokeMethod(MethodInfo method, IGeometry geometry, Object[] args)
        {
            try
            {
                if (method.ReturnType == typeof (bool))
                {
                    return new BooleanResult((Boolean) method.Invoke(geometry, args));
                }
                if (typeof (IGeometry).IsAssignableFrom(method.ReturnType))
                {
                    return new GeometryResult((IGeometry) method.Invoke(geometry, args));
                }
                if (method.ReturnType == typeof (double))
                {
                    return new DoubleResult((Double) method.Invoke(geometry, args));
                }
                if (method.ReturnType == typeof (int))
                {
                    return new IntegerResult((int) method.Invoke(geometry, args));
                }
            }
            catch (TargetInvocationException e)
            {
                var t = e.InnerException;
                throw t;
            }
            throw new NTSTestReflectionException("Unsupported result type: " + method.ReturnType);
        }

    }
}