using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Static utility functions for dealing with <see cref="Ordinates"/> and dimension
    /// </summary>
    public static class OrdinatesUtility
    {
        /// <summary>
        /// Translates the <paramref name="ordinates"/>-flag to a number of dimensions.
        /// </summary>
        /// <param name="ordinates">The ordinates flag</param>
        /// <returns>The number of dimensions</returns>
        public static int OrdinatesToDimension(Ordinates ordinates)
        {
            var ret = 2;
            if ((ordinates & Ordinates.Z) != 0) ret++;
            if ((ordinates & Ordinates.M) != 0) ret++;

            return ret;
        }

        /// <summary>
        /// Translates a dimension value to an <see cref="Ordinates"/>-flag.
        /// </summary>
        /// <remarks>The flag for <see cref="Ordinate.Z"/> is set first.</remarks>
        /// <param name="dimension">The dimension.</param>
        /// <returns>The ordinates-flag</returns>
        public static Ordinates DimensionToOrdinates(int dimension)
        {
            if (dimension == 3)
                return Ordinates.XYZ;
            if (dimension == 4)
                return Ordinates.XYZM;
            return Ordinates.XY;
        }

        /// <summary>
        /// Converts an <see cref="Ordinates"/> encoded flag to an array of <see cref="Ordinate"/> indices.
        /// </summary>
        /// <param name="ordinates">The ordinate flags</param>
        /// <param name="maxEval">The maximum oridinate flag that is to be checked</param>
        /// <returns>The ordinate indices</returns>
        public static Ordinate[] ToOrdinateArray(Ordinates ordinates, int maxEval = 4)
        {
            if (maxEval > 32) maxEval = 32;
            var intOrdinates = (int) ordinates;
            var ordinateList = new List<Ordinate>(maxEval);
            for (var i = 0; i < maxEval; i++)
            {
                if ((intOrdinates & (1<<i)) != 0) ordinateList.Add((Ordinate)i);
            }
            return ordinateList.ToArray();
        }

        /// <summary>
        /// Converts an array of <see cref="Ordinate"/> values to an <see cref="Ordinates"/> flag.
        /// </summary>
        /// <param name="ordinates">An array of <see cref="Ordinate"/> values</param>
        /// <returns>An <see cref="Ordinates"/> flag.</returns>
        public static Ordinates ToOrdinatesFlag(params Ordinate[] ordinates)
        {
            var result = Ordinates.None;
            foreach (var ordinate in ordinates)
            {
                result |= (Ordinates) (1 << ((int) ordinate));
            }
            return result;
        }

        /// <summary>
        /// Converts an <see cref="Ordinate"/> value to the index of that ordinate within a
        /// particular <see cref="CoordinateSequence"/>, or <see langword="null"/> if the sequence
        /// does not contain values for that ordinate.
        /// <para>
        /// Ordinate values greater than <see cref="Ordinate.M"/> are considered ambiguous and will
        /// always map to <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <param name="ordinate">The <see cref="Ordinate"/> value to convert.</param>
        /// <param name="seq">The <see cref="CoordinateSequence"/> to look for.</param>
        /// <returns>
        /// The ordinate index to use to store / fetch the values of <paramref name="ordinate"/> in
        /// <paramref name="seq"/>, or <see langword="null"/> if the ordinate is not present.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="seq"/> is <see langword="null"/>.
        /// </exception>
        public static int? IndexOfOrdinateInSequence(Ordinate ordinate, CoordinateSequence seq)
        {
            if (seq is null)
            {
                throw new ArgumentNullException(nameof(seq));
            }

            switch (ordinate)
            {
                case Ordinate.X:
                    return 0;

                case Ordinate.Y:
                    return 1;

                case Ordinate.Z when seq.HasZ:
                    return 2;

                case Ordinate.M when seq.HasM:
                    return seq.HasZ ? 3 : 2;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="seq">The <see cref="CoordinateSequence"/> whose ordinate value to get.</param>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate value to get.</param>
        /// <returns>The ordinate value, or <see cref="Coordinate.NullOrdinate"/> if the sequence does not provide values for <paramref name="ordinate"/>"/></returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="seq"/> is <see langword="null"/>.
        /// </exception>
        public static double GetOrdinate(this CoordinateSequence seq, int index, Ordinate ordinate)
        {
            if (seq is null)
            {
                throw new ArgumentNullException(nameof(seq));
            }

            return IndexOfOrdinateInSequence(ordinate, seq) is int ordinateIndex
                ? seq.GetOrdinate(index, ordinateIndex)
                : Coordinate.NullOrdinate;
        }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="seq">The <see cref="CoordinateSequence"/> whose ordinate value to set.</param>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate value to set.</param>
        /// <param name="value">The new ordinate value.</param>
        public static void SetOrdinate(this CoordinateSequence seq, int index, Ordinate ordinate, double value)
        {
            if (seq is null)
            {
                throw new ArgumentNullException(nameof(seq));
            }

            if (IndexOfOrdinateInSequence(ordinate, seq) is int ordinateIndex)
            {
                seq.SetOrdinate(index, ordinateIndex, value);
            }
        }
    }
}
