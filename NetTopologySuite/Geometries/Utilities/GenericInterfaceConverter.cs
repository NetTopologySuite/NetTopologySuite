using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    internal static class GenericInterfaceConverter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static ICoordinateSequence<TCoordinate> Convert(ICoordinateSequence coordinates,
                                                               ICoordinateSequenceFactory<TCoordinate> coordSeqFactory)
        {
            ICoordinateSequence<TCoordinate> converted 
                = coordinates as ICoordinateSequence<TCoordinate>;

            if (converted != null)
            {
                return converted;
            }

            converted = coordSeqFactory.Create(coordinates);
            return converted;
        }

        public static IEnumerable<ILineString<TCoordinate>> Convert(IEnumerable<ILineString> lineStrings, 
                                                                    IGeometryFactory<TCoordinate> geoFactory)
        {
            foreach (ILineString lineString in lineStrings)
            {
                ILineString<TCoordinate> l = lineString as ILineString<TCoordinate>;

                if (l != null)
                {
                    yield return l;
                }
                else
                {
                    ICoordinateSequence<TCoordinate> coordSeq 
                        = Convert(lineString.Coordinates, geoFactory.CoordinateSequenceFactory);
                    yield return geoFactory.CreateLineString(coordSeq);
                }
            }
        }
    }
}
