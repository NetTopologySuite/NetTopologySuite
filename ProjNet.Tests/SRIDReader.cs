using System.Collections.Generic;
using System.IO;
using GeoAPI.CoordinateSystems;
using ProjNet.Converters.WellKnownText;

namespace ProjNet.UnitTests
{
    internal class SRIDReader
    {
        private const string Filename = @"..\..\SRID.csv";

        public struct WktString {
            /// <summary>
            /// Well-known ID
            /// </summary>
            public int WktId;
            /// <summary>
            /// Well-known Text
            /// </summary>
            public string Wkt;
        }

        /// <summary>
        /// Enumerates all SRID's in the SRID.csv file.
        /// </summary>
        /// <returns>Enumerator</returns>
        public static IEnumerable<WktString> GetSrids()
        {
            using (var sr = File.OpenText(Filename))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;

                    var split = line.IndexOf(';');
                    if (split <= -1) continue;

                    var wkt = new WktString
                                  { 
                                      WktId = int.Parse(line.Substring(0, split)), 
                                      Wkt = line.Substring(split + 1)
                                  };
                    yield return wkt;
                }
                sr.Close();
            }
        }
        /// <summary>
        /// Gets a coordinate system from the SRID.csv file
        /// </summary>
        /// <param name="id">EPSG ID</param>
        /// <returns>Coordinate system, or null if SRID was not found.</returns>
        public static ICoordinateSystem GetCSbyID(int id)
        {
            foreach (var wkt in GetSrids())
            {
                if (wkt.WktId == id)
                {
                    return CoordinateSystemWktReader.Parse(wkt.Wkt) as ICoordinateSystem;
                }
            }
            return null;
        }
    }
}
