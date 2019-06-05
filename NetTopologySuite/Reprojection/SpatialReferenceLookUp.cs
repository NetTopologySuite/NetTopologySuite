using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// Abstract base class for lookup of spatial reference definition strings
    /// </summary>
    public abstract class SpatialReferenceLookUp
    {
        /// <summary>
        /// A string defining what kind of definition is being retrieved.
        /// </summary>
        /// <remarks>The default value is <c>wkt</c>.</remarks>
        public string DefinitionKind { get; protected set; } = "wkt";

        /// <summary>
        /// Function to get the spatial reference definition string for the given <paramref name="srid"/> value.
        /// </summary>
        /// <param name="srid">The spatial reference id.</param>
        /// <returns>A</returns>
        public abstract Task<string> GetDefinition(int srid);
    }

    /// <summary>
    /// A class to look up spatial reference systems from <a href="https://epsg.io" />.
    /// </summary>
    public class EpsgIoSpatialReferenceLookUp : SpatialReferenceLookUp
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="definitionKind"></param>
        public EpsgIoSpatialReferenceLookUp(string definitionKind)
        {
            switch (definitionKind)
            {
                case "wkt":
                case "prettywkt":
                case "esriwkt":
                //case "gml":
                //case "xml":
                case "proj4":
                    break;

                default:
                    throw new ArgumentOutOfRangeException(definitionKind);
            }

            DefinitionKind = definitionKind;
        }

        /// <inheritdoc />
        public override async Task<string> GetDefinition(int srid)
        {
            var req = WebRequest.CreateHttp(new Uri($"https://epsg.io/{srid}.{DefinitionKind}"));
            var ms = new MemoryStream();
            using (var resp = await req.GetResponseAsync())
            {
                using (var respStream = resp.GetResponseStream())
                {
                    if (respStream != null)
                        await respStream.CopyToAsync(ms);
                }
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
