using System;
using System.Diagnostics;

namespace Open.Topology.TestRunner.Utility
{
    public class ClassUtility
    {
        public static string GetClassname(Type javaClass)
        {
            string nClassName = javaClass.FullName;
            Debug.Assert(!string.IsNullOrEmpty(nClassName));

            int lastDotPos = nClassName.LastIndexOf(".");
            // Java's substring takes an end index where .NET's takes a length, and the port kept
            // the second argument: for any type in a namespace that asks for more characters than
            // are left and throws.
            return nClassName.Substring(lastDotPos + 1);
        }

    }
}