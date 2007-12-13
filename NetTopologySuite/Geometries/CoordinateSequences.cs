using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Utility functions for manipulating 
    /// <see cref="ICoordinateSequence" />s.
    /// </summary>
    public static class CoordinateSequences
    {
        public static ICoordinateSequence<TCoordinate> CreateEmpty<TCoordinate>()
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        public static void Reverse<TCoordinate>(ICoordinateSequence<TCoordinate> seq)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            Int32 last = seq.Count - 1;
            Int32 mid = last / 2;

            for (Int32 i = 0; i <= mid; i++)
            {
                Swap(seq, i, last - i);
            }
        }

        /// <summary>
        /// Swaps two coordinates in a sequence.
        /// </summary>
        public static void Swap(ICoordinateSequence seq, Int32 i, Int32 j)
        {
            if (i == j)
            {
                return;
            }

            for (Int32 dim = 0; dim < seq.Dimension; dim++)
            {
                Double tmp = seq[i, (Ordinates)dim];
                seq[i, (Ordinates)dim] = seq[j, (Ordinates)dim];
                seq[j, (Ordinates)dim] = tmp;
            }
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(Int32 initialCapacity)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(params TCoordinate[] coordinates)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }

        public static ICoordinateSequence<TCoordinate> Create<TCoordinate>(IEnumerable<TCoordinate> coordinates, Boolean allowRepeatedPoints)
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                                IComputable<TCoordinate>, IConvertible
        {
            throw new NotImplementedException();
        }
    }
}