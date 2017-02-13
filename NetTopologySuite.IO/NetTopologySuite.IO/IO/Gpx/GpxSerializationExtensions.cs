#if NET35

using System.IO;
using System.Linq;
using System.Xml.Serialization;

#if JSON
using NetTopologySuite.Features;
using Newtonsoft.Json;
#endif

namespace NetTopologySuite.IO
{
    internal static class GpxSerializarionExtensions
    {

#if JSON
        public static byte[] ToBytes(this FeatureCollection featureCollection)
        {
            using (var outputStream = new MemoryStream())
            {
                var writer = new StreamWriter(outputStream);
                var jsonWriter = new JsonTextWriter(writer);
                var serializer = new GeoJsonSerializer();
                serializer.Serialize(jsonWriter, featureCollection);
                jsonWriter.Flush();
                return outputStream.ToArray();
            }
        }

        public static FeatureCollection ToFeatureCollection(this byte[] featureCollectionContent)
        {
            using (var stream = new MemoryStream(featureCollectionContent))
            {
                var serializer = new GeoJsonSerializer();
                using (var streamReader = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<FeatureCollection>(jsonTextReader);
                }
            }
        }
#endif

        public static gpxType ToGpx(this byte[] gpxContent)
        {
            using (var stream = new MemoryStream(gpxContent))
            {
                var xmlSerializer = new XmlSerializer(typeof(gpxType));
                return xmlSerializer.Deserialize(stream) as gpxType;
            }
        }

        public static byte[] ToBytes(this gpxType gpx)
        {
            using (var outputStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(gpxType));
                xmlSerializer.Serialize(outputStream, gpx);
                return outputStream.ToArray();
            }
        }

        public static gpxType UpdateBounds(this gpxType gpx)
        {
            if (gpx.metadata?.bounds != null &&
                gpx.metadata.bounds.minlat != 0 &&
                gpx.metadata.bounds.maxlat != 0 &&
                gpx.metadata.bounds.minlon != 0 &&
                gpx.metadata.bounds.maxlon != 0)
            {
                return gpx;
            }
            var points = (gpx.rte ?? new rteType[0]).Where(r => r.rtept != null).SelectMany(r => r.rtept).ToArray();
            points = points.Concat(gpx.wpt ?? new wptType[0]).ToArray();
            points = points.Concat((gpx.trk ?? new trkType[0]).Where(r => r.trkseg != null).SelectMany(t => t.trkseg).SelectMany(s => s.trkpt)).ToArray();
            if (!points.Any())
            {
                return gpx;
            }
            if (gpx.metadata == null)
            {
                gpx.metadata = new metadataType { bounds = new boundsType() };
            }

            gpx.metadata.bounds = new boundsType
            {
                minlat = points.Min(p => p.lat),
                maxlat = points.Max(p => p.lat),
                minlon = points.Min(p => p.lon),
                maxlon = points.Max(p => p.lon)
            };
            return gpx;
        }
    }
}
#endif
