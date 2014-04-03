using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.TestData
{
    internal class EmbeddedResourceManager
    {
        public static Stream GetResourceStream(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.GetManifestResourceStream(resourceName);
        }
        
        public static string SaveEmbeddedResourceToTempFile(string resourceName)
        {
            string directoryPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Temp");

            if (!Directory.Exists(directoryPath))
            {
                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (Exception ex)
                {
                    throw new IgnoreException("Unable to create directory - " + directoryPath, ex);
                }
            }

            var resourceNameComponents = resourceName.Split(new [] { "." }, StringSplitOptions.RemoveEmptyEntries);

            string fileName = resourceNameComponents[resourceNameComponents.Length - 2] + "." + resourceNameComponents[resourceNameComponents.Length - 1];

            string filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new IgnoreException("Unable to delete existing file - " + filePath, ex);
                }
            }

            using (var resourceStream = GetResourceStream(resourceName))
            {
                try
                {
                    var buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, Convert.ToInt32(resourceStream.Length));
                    File.WriteAllBytes(filePath, buffer);
                }
                catch (Exception ex)
                {
                    throw new IgnoreException("Unable to save resouce contents to file - " + filePath, ex);
                }
            }

            return filePath;
        }

        public static void CleanUpTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    throw new IgnoreException("Unable to delete file - " + filePath, ex);
                }
            }
        }
    }
}