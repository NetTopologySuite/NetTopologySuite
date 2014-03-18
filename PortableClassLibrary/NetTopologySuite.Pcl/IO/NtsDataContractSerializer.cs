using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace NetTopologySuite.IO
{
#if PCL
    public static class SerializerUtility
    {
        private static readonly HashSet<Type> KnownTypes;
        private static readonly object Lock = new object();
        static SerializerUtility()
        {
            lock (Lock)
            {
                KnownTypes = new HashSet<Type>();
                KnownTypes.Add(typeof (GeoAPI.Geometries.Coordinate));
                KnownTypes.Add(typeof (GeoAPI.Geometries.Envelope));
                KnownTypes.Add(typeof (GeoAPI.DataStructures.Interval));

                KnownTypes.Add(typeof (Geometries.Implementation.CoordinateArraySequence));
                KnownTypes.Add(typeof (Geometries.Implementation.DotSpatialAffineCoordinateSequence));

                KnownTypes.Add(typeof (Geometries.Geometry));
                KnownTypes.Add(typeof (Geometries.Point));
                KnownTypes.Add(typeof (Geometries.MultiPoint));
                KnownTypes.Add(typeof (Geometries.LineString));
                KnownTypes.Add(typeof (Geometries.LinearRing));
                KnownTypes.Add(typeof (Geometries.MultiLineString));
                KnownTypes.Add(typeof (Geometries.Polygon));
                KnownTypes.Add(typeof (Geometries.MultiPolygon));
                KnownTypes.Add(typeof (Geometries.GeometryCollection));

                KnownTypes.Add(typeof (Index.Strtree.Interval));
                KnownTypes.Add(typeof (Index.Bintree.Interval));
            }
        }

        public static DataContractSerializer CreateDataContractSerializer<T>()
        {
            lock(Lock)
                return new DataContractSerializer(typeof (T), KnownTypes);
        }

        public static DataContractJsonSerializer CreateDataContractJsonSerializer<T>()
        {
            lock (Lock)
                return new DataContractJsonSerializer(typeof(T), KnownTypes);
        }

        public static void AddType(Type type)
        {
            lock (Lock)
            {
                if (KnownTypes.Contains(type))
                    return;

                KnownTypes.Add(type);
            }
        }
    }
#endif
}