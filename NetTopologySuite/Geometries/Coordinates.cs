using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public static class Coordinates<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
    {
        public static Boolean IsEmpty(TCoordinate coordinate)
        {
            return coordinate is ValueType
                       ? coordinate.IsEmpty
                       : ReferenceEquals(coordinate, null) || coordinate.IsEmpty;
        }

        internal static IEnumerable<ILinearRing<TCoordinate>> CreateLinearRings(
                                                            ICoordinateSequence<TCoordinate> sequence, 
                                                            IGeometryFactory<TCoordinate> factory)
        {
            if (sequence == null)
                yield break;

            TCoordinate firstCoord = default(TCoordinate);
            Int32 firstCoordIndex = -1;

            for (Int32 i = 0; i < sequence.Count; i++)
            {
                if (firstCoordIndex == -1 || IsEmpty(firstCoord))
                {
                    firstCoord = sequence[i];
                    firstCoordIndex = i;
                }
                else if (firstCoord.Equals(sequence[i])) // we have a ring
                {
                    ICoordinateSequence<TCoordinate> ring 
                        = sequence.Slice(firstCoordIndex, i);

                    yield return factory.CreateLinearRing(ring);

                    firstCoord = default(TCoordinate);
                }
            }
        }
    }
}