#nullable disable
using System.IO;
using System.Reflection;

namespace NetTopologySuite.Tests.NUnit.TestData
{
    internal class EmbeddedResourceManager
    {
        public static Stream GetResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
