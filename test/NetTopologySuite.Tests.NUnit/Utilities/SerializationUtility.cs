using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {
        public static byte[] Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(ms);
                Console.WriteLine(reader.ReadToEnd());

                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
                var serializer = new BinaryFormatter();
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}