using System;
using System.IO;
using System.Reflection;

namespace NetTopologySuite.Tests.NUnit.TestData
{
    internal class EmbeddedResourceManager
    {
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
                    throw new ApplicationException("Unable to create directory - " + directoryPath, ex);
                }
            }

            var resourceNameComponents = resourceName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

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
                    throw new ApplicationException("Unable to delete existing file - " + filePath, ex);
                }
            }

            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            using (resourceStream)
            {
                try
                {
                    var buffer = new byte[resourceStream.Length];
                    resourceStream.Read(buffer, 0, Convert.ToInt32(resourceStream.Length));
                    File.WriteAllBytes(filePath, buffer);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unable to save resouce contents to file - " + filePath, ex);
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
                    throw new ApplicationException("Unable to delete file - " + filePath, ex);
                }
            }
        }
    }
}