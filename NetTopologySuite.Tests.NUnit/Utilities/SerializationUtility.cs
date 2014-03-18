using System;
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
                NetTopologySuite.IO.SerializerUtility.AddType(typeof(T));
                var serializer = NetTopologySuite.IO.SerializerUtility.CreateDataContractSerializer<T>();
                serializer.WriteObject(ms, obj);
#endif

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
#if !PCL
                var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)serializer.Deserialize(ms);
#else
                NetTopologySuite.IO.SerializerUtility.AddType(typeof(T));
                var serializer = NetTopologySuite.IO.SerializerUtility.CreateDataContractSerializer<T>();
                return (T)serializer.ReadObject(ms);
#endif
            }
        }
    }
}