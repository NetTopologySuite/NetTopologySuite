using System;
using System.Collections.Generic;
using System.Threading;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    public static class Coordinates<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<Double, TCoordinate>, IConvertible
    {
        private static Object _defaultCoordinateFactory;
        private static Object _defaultCoordinateSequenceFactory;

        //public static ICoordinateFactory<TCoordinate> DefaultCoordinateFactory
        //{
        //    get
        //    {
        //        Object factory = Thread.VolatileRead(ref _defaultCoordinateFactory);

        //        if (ReferenceEquals(factory, null))
        //        {
        //            throw new InvalidOperationException("Default coordinate factory is not set.");
        //        }

        //        return factory as ICoordinateFactory<TCoordinate>;
        //    }
        //    set
        //    {
        //        Thread.VolatileWrite(ref _defaultCoordinateFactory, value);
        //    }
        //}

        //public static ICoordinateSequenceFactory<TCoordinate> DefaultCoordinateSequenceFactory
        //{
        //    get
        //    {
        //        Object factory = Thread.VolatileRead(ref _defaultCoordinateSequenceFactory);

        //        if (ReferenceEquals(factory, null))
        //        {
        //            throw new InvalidOperationException("Default coordinate factory is not set.");
        //        }

        //        return factory as ICoordinateSequenceFactory<TCoordinate>;
        //    }
        //    set
        //    {
        //        Thread.VolatileWrite(ref _defaultCoordinateSequenceFactory, value);
        //    }
        //}

        //public static TCoordinate Empty
        //{
        //    get
        //    {
        //        return DefaultCoordinateFactory.Create();
        //    }
        //}

        public static Boolean IsEmpty(TCoordinate coordinate)
        {
            if (coordinate is ValueType)
            {
                return coordinate.IsEmpty;
            }
            else
            {
                return ReferenceEquals(coordinate, null) || coordinate.IsEmpty;
            }
        }

        //public static IEnumerable<TCoordinate> RemoveRepeatedPoints(IEnumerable<TCoordinate> points)
        //{
        //    TCoordinate lastCoordinate = default(TCoordinate);

        //    foreach (TCoordinate point in points)
        //    {
        //        if (!point.Equals(lastCoordinate))
        //        {
        //            yield return point;
        //        }

        //        lastCoordinate = point;
        //    }
        //}

        ///// <summary>
        ///// Determines which orientation of the 
        ///// <see cref="ICoordinateSequence{TCoordinate}" /> array is (overall) increasing.
        ///// In other words, determines which end of the array is "smaller"
        ///// (using the standard ordering on <typeparamref name="TCoordinate"/>).
        ///// Returns an integer indicating the increasing direction.
        ///// If the sequence is a palindrome, it is defined to be
        ///// oriented in a positive direction.
        ///// </summary>
        ///// <param name="pts">The <see cref="ICoordinateSequence{TCoordinate}" /> to test.</param>
        ///// <returns>
        ///// <c>1</c> if the array is smaller at the start or is a palindrome,
        ///// <c>-1</c> if smaller at the end.
        ///// </returns>
        //public static Int32 IncreasingDirection(ICoordinateSequence<TCoordinate> pts)
        //{
        //    for (Int32 i = 0; i < pts.Count / 2; i++)
        //    {
        //        Int32 j = pts.Count - 1 - i;

        //        // skip equal points on both ends
        //        Int32 comp = pts[i].CompareTo(pts[j]);

        //        if (comp != 0)
        //        {
        //            return comp;
        //        }
        //    }

        //    // array must be a palindrome - defined to be in positive direction
        //    return 1;
        //}

        private static Int32 compareOriented(ICoordinateSequence<TCoordinate> pts1, Boolean orientation1,
            ICoordinateSequence<TCoordinate> pts2, Boolean orientation2)
        {
            if (orientation1)
	        {
	            pts1.Reverse();
	        }

            if (orientation2)
	        {
	            pts2.Reverse();
	        }

            IEnumerator<TCoordinate> p1Enumerator = pts1.GetEnumerator();
            IEnumerator<TCoordinate> p2Enumerator = pts2.GetEnumerator();

            Boolean done1, done2;

            do
            {
                done1 = p1Enumerator.MoveNext();
                done2 = p2Enumerator.MoveNext();

                if (done1 && !done2)
                {
                    return -1;
                }

                if (!done1 && done2)
                {
                    return 1;
                }

                Int32 compare = p1Enumerator.Current.CompareTo(p2Enumerator.Current);

                if (compare != 0)
                {
                    return compare;
                }

            } while (!(done1 & done2));

            return 0;
        }

        ///// <summary>
        ///// Computes the canonical orientation for a coordinate array.
        ///// </summary>
        ///// <returns>
        ///// <see langword="true"/> if the points are oriented forwards, or
        ///// <c>false</c>if the points are oriented in reverse.
        ///// </returns>
        //private static Boolean orientation(ICoordinateSequence<TCoordinate> pts)
        //{
        //    return IncreasingDirection(pts) == 1;
        //}

        internal static IEnumerable<ILinearRing<TCoordinate>> CreateLinearRings(
            ICoordinateSequence<TCoordinate> sequence, IGeometryFactory<TCoordinate> factory)
        {
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