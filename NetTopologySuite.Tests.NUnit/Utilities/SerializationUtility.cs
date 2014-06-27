using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {
        public static byte[] Serialize<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(ms);
                Console.WriteLine(reader.ReadToEnd());

                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}