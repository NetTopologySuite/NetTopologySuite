using System;
using System.Diagnostics;
namespace Open.Topology.TestRunner.Utility
{
    public class ClassUtility
    {
        public static string GetClassname(Type javaClass)
        {
            var nClassName = javaClass.FullName;
            Debug.Assert(!string.IsNullOrEmpty(nClassName));
            var lastDotPos = nClassName.LastIndexOf(".");
            return nClassName.Substring(lastDotPos + 1, nClassName.Length);
        }
    }
}
