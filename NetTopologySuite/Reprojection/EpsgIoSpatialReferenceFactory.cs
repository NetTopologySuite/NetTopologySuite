using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Reprojection
{
    /// <summary>
    /// A class to look up spatial reference systems from <a href="https://epsg.io" />.
    /// </summary>
    public class EpsgIoSpatialReferenceFactory : SpatialReferenceFactory
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="definitionKind"></param>
        /// <param name="factoryMethod"></param>
        public EpsgIoSpatialReferenceFactory(CoordinateSequenceFactory coordinateSequenceFactory,
            PrecisionModel precisionModel, string definitionKind)
            :base(coordinateSequenceFactory, precisionModel)
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

        protected override SpatialReference CreateSpatialReference(int srid)
        {
            var req = WebRequest.CreateHttp(new Uri($"https://epsg.io/{srid}.{DefinitionKind}"));
            var ms = new MemoryStream();
            using (var resp = req.GetResponse())
            {
                using (var respStream = resp.GetResponseStream())
                {
                    if (respStream != null)
                        respStream.CopyTo(ms);
                }
            }

            string definitionString = Encoding.UTF8.GetString(ms.ToArray());
            var factory = new GeometryFactory(PrecisionModel, srid, CoordinateSequenceFactory);
            return new SpatialReference(DefinitionKind, definitionString, factory);
        }

        /// <inheritdoc />
        protected override async Task<SpatialReference> CreateSpatialReferenceAsync(int srid)
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

            string definitionString = Encoding.UTF8.GetString(ms.ToArray());
            var factory = new GeometryFactory(PrecisionModel, srid, CoordinateSequenceFactory);
            return new SpatialReference(DefinitionKind, definitionString, factory);
        }
    }
}
