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
            return nClassName.Substring(lastDotPos + 1, nClassName.Length);
        }

    }
}