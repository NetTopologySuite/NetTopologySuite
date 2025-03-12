using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {
        public static byte[] Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                // don't use BinaryFormatter in production. read this instead:
                // https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
                var serializer = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(ms);
                TestContext.WriteLine(reader.ReadToEnd());

                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                // don't use BinaryFormatter in production. read this instead:
                // https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide
#pragma warning disable SYSLIB0011 // Type or member is obsolete
                AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
                var serializer = new BinaryFormatter();
#pragma warning restore SYSLIB0011 // Type or member is obsolete
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}
