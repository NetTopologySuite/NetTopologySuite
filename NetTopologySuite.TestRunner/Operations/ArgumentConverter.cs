using System;
using System.Globalization;

namespace Open.Topology.TestRunner.Operations
{
    public class ArgumentConverter
    {
        /*
        public ArgumentConverter()
	    {
	    }
         */

        public Object[] Convert(Type[] parameterTypes, Object[] args)
        {
            Object[] actualArgs = new Object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                actualArgs[i] = Convert(parameterTypes[i], args[i]);
            }
            return actualArgs;
        }

        public Object Convert(Type destClass, Object srcValue)
        {
            if (srcValue is String)
            {
                return ConvertFromString(destClass, (String) srcValue);
            }
            if (destClass.IsAssignableFrom(srcValue.GetType()))
            {
                return srcValue;
            }
            ThrowInvalidConversion(destClass, srcValue);
            return null;
        }

        private static Object ConvertFromString(Type destClass, String src)
        {
            if (destClass == typeof (Boolean) || destClass == typeof (bool))
            {
                if (src.Equals("true"))
                {
                    return true;
                }
                if (src.Equals("false"))
                {
                    return false;
                }
                ThrowInvalidConversion(destClass, src);
            }
            else if (destClass == typeof(Int32) ||
            destClass == typeof(int))
            {
                // try as an int
                int val;
                if (int.TryParse(src, out val))
                    return val;

                ThrowInvalidConversion(destClass, src);
                /*
                try
                {
                    return new Integer(src);
                }
                catch (FormatException e)
                {
                    // eat this exception - it will be reported below
                }
                 */
            }
        else
            if (destClass == typeof(Double) ||
            destClass == typeof(double))
            {
                // try as a double
                double dval;
                if (double.TryParse(src, NumberStyles.Any,  CultureInfo.InvariantCulture, out dval))
                    return dval;
                /*
                try
                {
                    return new Double(src);
                }
                catch (FormatException e)
                {
                    // eat this exception - it will be reported below
                }
                 */
            }
        else
            if (destClass == typeof(string))
            {
                return src;
            }
            ThrowInvalidConversion(destClass, src);
            return null;
        }

        private static void ThrowInvalidConversion(Type destClass, Object srcValue)
        {
            throw new ArgumentException("Cannot convert " + srcValue + " to " + destClass);
        }
    }
}