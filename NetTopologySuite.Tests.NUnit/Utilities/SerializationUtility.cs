using System.IO;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public static class SerializationUtility
    {

        public static byte[] Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
#if !PCL
                var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                serializer.Serialize(ms, obj);
#else
                var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                serializer.WriteObject(ms, obj);
#endif
                return ms.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] buffer)
        {
            using (var ms = new MemoryStream(buffer))
            {
#if !PCL
                var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)serializer.Deserialize(ms);
#else
                var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(ms);
#endif
            }
        }
    }
}