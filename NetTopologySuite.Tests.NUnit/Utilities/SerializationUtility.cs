using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {
        public static byte[] Serialize(Object obj)
        {
            using (var bos = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(bos, obj);
                return bos.ToArray();
            }
        }

        public static Object Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }
    }
}