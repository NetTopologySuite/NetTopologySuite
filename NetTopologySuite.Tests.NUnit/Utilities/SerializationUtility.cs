using System;
using System.IO;
using System.Runtime.Serialization;
#if !PCL
using System.Runtime.Serialization.Formatters.Binary;
#endif

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {
#if !PCL
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
#else
        public static Stream Serialize(object obj, Func<Stream> streamCreator = null)
        {
            var s = new DataContractSerializer(obj.GetType());
            var stream = streamCreator != null ? streamCreator() : new MemoryStream();
            s.WriteObject(stream, obj);
            return stream;
        }

        public static T Deserialize<T>(Stream stream)
        {
            var s = new DataContractSerializer(typeof(T));
            return (T)s.ReadObject(stream);
        }

#endif
    }
}