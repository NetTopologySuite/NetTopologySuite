using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Utility functions for manipulating <see cref="CoordinateSequence" />s.
    /// </summary>
    public static class CoordinateSequences
    {
        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        /// <param name="seq">The coordinate sequence to reverse.</param>
        public static void Reverse(CoordinateSequence seq)
        {
            if (seq.Count <= 1) return;

            int last = seq.Count - 1;
            int mid = last / 2;
            for (int i = 0; i <= mid; i++)
                Swap(seq, i, last - i);
        }

        /// <summary>
        /// Swaps two coordinates in a sequence.
        /// </summary>
        /// <param name="seq">seq the sequence to modify</param>
        /// <param name="i">the index of a coordinate to swap</param>
        /// <param name="j">the index of a coordinate to swap</param>
        public static void Swap(CoordinateSequence seq, int i, int j)
        {
            if (i == j)
                return;

            for (int dim = 0; dim < seq.Dimension; dim++)
            {
                double tmp = seq.GetOrdinate(i, dim);
                seq.SetOrdinate(i, dim, seq.GetOrdinate(j, dim));
                seq.SetOrdinate(j, dim, tmp);
            }
        }

        /// <summary>
        /// Copies a section of a <see cref="CoordinateSequence"/> to another <see cref="CoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        /// </summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        public static void Copy(CoordinateSequence src, int srcPos, CoordinateSequence dest, int destPos, int length)
        {
            // Attempt shortcuts
            if (src is PackedDoubleCoordinateSequence srcPD && dest is PackedDoubleCoordinateSequence destPD)
            {
                if (TryRawCopy(srcPD, srcPos, destPD, destPos, length))
                    return;
            }
            else if (src is PackedFloatCoordinateSequence srcPF && dest is PackedFloatCoordinateSequence destPF)
            {
                if (TryRawCopy(srcPF, srcPos, destPF, destPos, length))
                    return;
            }
            else if (src is DotSpatialAffineCoordinateSequence srcDS && dest is DotSpatialAffineCoordinateSequence destDS)
            {
                if (TryRawCopy(srcDS, srcPos, destDS, destPos, length))
                    return;
            }

            GetCommonOrdinateIndices(src, dest, out int[] srcIndices, out int[] destIndices);

            // Copy one by one
            for (int i = 0; i < length; i++)
                CopyCoord(src, srcPos + i, srcIndices, dest, destPos + i, destIndices);
        }

        /// <summary>
        /// Get the common ordinate indices of two <c>CoordinateSequence</c>s.
        /// </summary>
        /// <param name="seq0">A <c>CoordinateSequence</c></param>
        /// <param name="seq1">A <c>CoordinateSequence</c></param>
        /// <param name="seq0Indices">The array of common ordinate indices as in <paramref name="seq0"/></param>
        /// <param name="seq1Indices">The array of common ordinate indices as in <paramref name="seq1"/></param>
        private static void GetCommonOrdinateIndices(CoordinateSequence seq0, CoordinateSequence seq1,
            out int[] seq0Indices, out int[] seq1Indices)
        {
            var srcIndexList = new List<int>(16);
            var destIndexList = new List<int>(16);
            var commonOrdinates = seq0.Ordinates & seq1.Ordinates;
            var ordinatesChecked = Ordinates.None;

            // Check all spatial and non-spatial ordinate values
            for (int i = 0; i < 16; i++)
            {
                // if indices of all common ordinates were gathered then exit
                if (commonOrdinates == ordinatesChecked)
                    break;

                // investigate spatial ordinate
                var ordinate = (Ordinates)(1 << i);

                // is ordinate common to both sequences?
                if ((commonOrdinates & ordinate) == ordinate)
                {
                    seq0.TryGetOrdinateIndex((Ordinate)i, out int index);
                    srcIndexList.Add(index);
                    seq1.TryGetOrdinateIndex((Ordinate)i, out index);
                    destIndexList.Add(index);
                    ordinatesChecked |= ordinate;
                }

                // investigate non-spatial ordinate
                ordinate = (Ordinates)(1 << (i+16));

                // is ordinate common to both sequences?
                if ((commonOrdinates & ordinate) == ordinate)
                {
                    seq0.TryGetOrdinateIndex((Ordinate)i, out int index);
                    srcIndexList.Add(index);
                    seq1.TryGetOrdinateIndex((Ordinate)i, out index);
                    destIndexList.Add(index);
                    ordinatesChecked |= ordinate;
                }
            }

            seq0Indices = srcIndexList.ToArray();
            seq1Indices = destIndexList.ToArray();
        }

        /// <summary>
        /// Copies a section of a <see cref="PackedDoubleCoordinateSequence"/> to another <see cref="PackedDoubleCoordinateSequence"/>.
        /// The sequences must have same dimensions.
        /// </summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        private static bool TryRawCopy(PackedDoubleCoordinateSequence src, int srcPos, PackedDoubleCoordinateSequence dest, int destPos, int length)
        {
            if (src.Ordinates != dest.Ordinates)
                return false;

            if (srcPos + length > src.Count || destPos + length > dest.Count)
                return false;

            double[] srcRaw = src.GetRawCoordinates();
            double[] destRaw = dest.GetRawCoordinates();
            int srcOffset = srcPos * src.Dimension;
            int destOffset = destPos * dest.Dimension;

            Array.Copy(srcRaw, srcOffset, destRaw, destOffset, length * src.Dimension);
            dest.ReleaseCoordinateArray();

            return true;
        }

        /// <summary>
        /// Copies a section of a <see cref="PackedFloatCoordinateSequence"/> to another <see cref="PackedFloatCoordinateSequence"/>.
        /// The sequences must have same dimensions.
        /// </summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        public static bool TryRawCopy(PackedFloatCoordinateSequence src, int srcPos, PackedFloatCoordinateSequence dest, int destPos, int length)
        {
            if (src.Ordinates != dest.Ordinates)
                return false;

            if (srcPos + length > src.Count || destPos + length > dest.Count)
                return false;

            float[] srcRaw = src.GetRawCoordinates();
            float[] destRaw = dest.GetRawCoordinates();
            int srcOffset = srcPos * src.Dimension;
            int destOffset = destPos * dest.Dimension;

            Array.Copy(srcRaw, srcOffset, destRaw, destOffset, length * src.Dimension);
            dest.ReleaseCoordinateArray();

            return true;
        }


        /// <summary>
        /// Copies a section of a <see cref="PackedDoubleCoordinateSequence"/> to another <see cref="PackedDoubleCoordinateSequence"/>.
        /// The sequences must have same dimensions.
        /// </summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        private static bool TryRawCopy(DotSpatialAffineCoordinateSequence src, int srcPos, DotSpatialAffineCoordinateSequence dest, int destPos, int length)
        {
            if (srcPos + length > src.Count || destPos + length > dest.Count)
                return false;

            // Copy XY
            double[] srcRaw = src.XY;
            double[] destRaw = dest.XY;
            int srcOffset = srcPos * 2;
            int destOffset = destPos * 2;
            Array.Copy(srcRaw, srcOffset, destRaw, destOffset, 2 * length);

            // Copy Z
            srcRaw = src.Z;
            destRaw = dest.Z;
            if (srcRaw != null && destRaw != null)
                Array.Copy(srcRaw, srcPos, destRaw, destPos, length);

            // Copy M
            srcRaw = src.M;
            destRaw = dest.M;
            if (srcRaw != null && destRaw != null)
                Array.Copy(srcRaw, srcPos, destRaw, destPos, length);

            dest.ReleaseCoordinateArray();

            return true;
        }
        /// <summary>
        /// Copies a coordinate of a <see cref="CoordinateSequence"/> to another <see cref="CoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        /// </summary>
        /// <param name="src">The sequence to copy coordinate from</param>
        /// <param name="srcPos">The index of the coordinate to copy</param>
        /// <param name="dest">The sequence to which the coordinate should be copied to</param>
        /// <param name="destPos">The index of the coordinate in <see paramref="dest"/></param>
        public static void CopyCoord(CoordinateSequence src, int srcPos, CoordinateSequence dest, int destPos)
        {
            GetCommonOrdinateIndices(src, dest, out int[] srcIndices, out int[] destIndices);
            CopyCoord(src, srcPos, srcIndices, dest, destPos, destIndices);
        }

        private static void CopyCoord(CoordinateSequence src, int srcPos, int[] srcIndices,
                                      CoordinateSequence dest, int destPos, int[] destIndices)
        {
            if (srcIndices.Length != destIndices.Length)
                throw new ArgumentException(nameof(destIndices));

            for (int dim = 0; dim < srcIndices.Length; dim++)
            {
                double value = src.GetOrdinate(srcPos, srcIndices[dim]);
                dest.SetOrdinate(destPos, destIndices[dim], value);
            }
        }
        /// <summary>
        /// Tests whether a <see cref="CoordinateSequence"/> forms a valid <see cref="LinearRing"/>,
        /// by checking the sequence length and closure
        /// (whether the first and last points are identical in 2D).
        /// Self-intersection is not checked.
        /// </summary>
        /// <param name="seq">The sequence to test</param>
        /// <returns>True if the sequence is a ring</returns>
        /// <seealso cref="LinearRing"/>
        public static bool IsRing(CoordinateSequence seq)
        {
            int n = seq.Count;
            if (n == 0) return true;
            // too few points
            if (n <= 3)
                return false;
            // test if closed
            return seq.GetOrdinate(0, 0) == seq.GetOrdinate(n - 1, 0)
                && seq.GetOrdinate(0, 1) == seq.GetOrdinate(n - 1, 1);
        }

        /// <summary>
        /// Ensures that a CoordinateSequence forms a valid ring,
        /// returning a new closed sequence of the correct length if required.
        /// If the input sequence is already a valid ring, it is returned
        /// without modification.
        /// If the input sequence is too short or is not closed,
        /// it is extended with one or more copies of the start point.
        /// </summary>
        /// <param name="fact">The CoordinateSequenceFactory to use to create the new sequence</param>
        /// <param name="seq">The sequence to test</param>
        /// <returns>The original sequence, if it was a valid ring, or a new sequence which is valid.</returns>
        public static CoordinateSequence EnsureValidRing(CoordinateSequenceFactory fact, CoordinateSequence seq)
        {
            int n = seq.Count;
            // empty sequence is valid
            if (n == 0) return seq;
            // too short - make a new one
            if (n <= 3)
                return CreateClosedRing(fact, seq, 4);

            bool isClosed = seq.GetOrdinate(0, 0) == seq.GetOrdinate(n - 1, 0) &&
                           seq.GetOrdinate(0, 1) == seq.GetOrdinate(n - 1, 1);
            if (isClosed) return seq;
            // make a new closed ring
            return CreateClosedRing(fact, seq, n + 1);
        }

        private static CoordinateSequence CreateClosedRing(CoordinateSequenceFactory fact, CoordinateSequence seq, int size)
        {
            var newseq = fact.Create(size, seq.Dimension, seq.Measures);
            int n = seq.Count;
            Copy(seq, 0, newseq, 0, n);
            // fill remaining coordinates with start point
            for (int i = n; i < size; i++)
                Copy(seq, 0, newseq, i, 1);
            return newseq;
        }

        /// <summary>
        /// Extends a given <see cref="CoordinateSequence"/>.
        /// <para/>
        /// Because coordinate sequences are fix in size, extending is done by
        /// creating a new coordinate sequence of the requested size.
        /// <para/>
        /// The new, trailing coordinate entries (if any) are filled with the last
        /// coordinate of the input sequence
        /// </summary>
        /// <param name="fact">The factory to use when creating the new sequence.</param>
        /// <param name="seq">The sequence to extend.</param>
        /// <param name="size">The required size of the extended sequence</param>
        /// <returns>The extended sequence</returns>
        public static CoordinateSequence Extend(CoordinateSequenceFactory fact, CoordinateSequence seq, int size)
        {
            var newSeq = fact.Create(size, seq.Ordinates);
            int n = seq.Count;
            Copy(seq, 0, newSeq, 0, n);
            // fill remaining coordinates with end point, if it exists
            if (n > 0)
            {
                for (int i = n; i < size; i++)
                    Copy(seq, n - 1, newSeq, i, 1);
            }
            return newSeq;
        }

        /// <summary>
        /// Tests whether two <see cref="CoordinateSequence"/>s are equal.
        /// To be equal, the sequences must be the same length.
        /// They do not need to be of the same dimension,
        /// but the ordinate values for the smallest dimension of the two
        /// must be equal.
        /// Two <c>NaN</c> ordinates values are considered to be equal.
        /// </summary>
        /// <param name="cs1">a CoordinateSequence</param>
        /// <param name="cs2">a CoordinateSequence</param>
        /// <returns><c>true</c> if the sequences are equal in the common dimensions</returns>
        public static bool IsEqual(CoordinateSequence cs1, CoordinateSequence cs2)
        {
            int cs1Size = cs1.Count;
            int cs2Size = cs2.Count;
            if (cs1Size != cs2Size)
                return false;
            int dim = Math.Min(cs1.Dimension, cs2.Dimension);
            for (int i = 0; i < cs1Size; i++)
            {
                for (int d = 0; d < dim; d++)
                {
                    double v1 = cs1.GetOrdinate(i, d);
                    double v2 = cs2.GetOrdinate(i, d);
                    if (v1 == v2)
                        continue;
                    // special check for NaNs
                    else if (double.IsNaN(v1) && double.IsNaN(v2))
                        continue;
                    else
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a string representation of a <see cref="CoordinateSequence"/>.
        /// The format is:
        /// <para>
        ///  ( ord0,ord1.. ord0,ord1,...  ... )
        /// </para>
        /// </summary>
        /// <param name="cs">the sequence to output</param>
        /// <returns>the string representation of the sequence</returns>
        public static string ToString(CoordinateSequence cs)
        {
            int size = cs.Count;
            if (size == 0)
                return "()";
            int dim = cs.Dimension;
            var sb = new StringBuilder();
            sb.Append('(');
            for (int i = 0; i < size; i++)
            {
                if (i > 0) sb.Append(" ");
                for (int d = 0; d < dim; d++)
                {
                    if (d > 0) sb.Append(",");
                    double ordinate = cs.GetOrdinate(i, d);
                    sb.Append(OrdinateFormat.Default.Format(ordinate));
                }
            }
            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>
        /// Returns the minimum coordinate, using the usual lexicographic comparison.
        /// </summary>
        /// <param name="seq">The coordinate sequence to search</param>
        /// <returns>The minimum coordinate in the sequence, found using <see cref="Coordinate.CompareTo(Coordinate)"/></returns>
        public static Coordinate MinCoordinate(CoordinateSequence seq)
        {
            Coordinate minCoord = null;
            for (int i = 0; i < seq.Count; i++)
            {
                var testCoord = seq.GetCoordinate(i);
                if (minCoord == null || minCoord.CompareTo(testCoord) > 0)
                {
                    minCoord = testCoord;
                }
            }
            return minCoord;
        }

        /// <summary>
        /// Returns the index of the minimum coordinate of the whole
        /// coordinate sequence, using the usual lexicographic comparison.
        /// </summary>
        /// <param name="seq">The coordinate sequence to search</param>
        /// <returns>The index of the minimum coordinate in the sequence, found using <see cref="Coordinate.CompareTo(Coordinate)"/></returns>
        public static int MinCoordinateIndex(CoordinateSequence seq)
        {
            return MinCoordinateIndex(seq, 0, seq.Count - 1);
        }

        /// <summary>
        /// Returns the index of the minimum coordinate of a part of
        /// the coordinate sequence (defined by <paramref name="from"/>
        /// and <paramref name="to"/>), using the usual lexicographic
        /// comparison.
        /// </summary>
        /// <param name="seq">The coordinate sequence to search</param>
        /// <param name="from">The lower search index</param>
        /// <param name="to">The upper search index</param>
        /// <returns>The index of the minimum coordinate in the sequence, found using <see cref="Coordinate.CompareTo(Coordinate)"/></returns>
        public static int MinCoordinateIndex(CoordinateSequence seq, int from, int to)
        {
            int minCoordIndex = -1;
            Coordinate minCoord = null;
            for (int i = from; i <= to; i++)
            {
                var testCoord = seq.GetCoordinate(i);
                if (minCoord == null || minCoord.CompareTo(testCoord) > 0)
                {
                    minCoord = testCoord;
                    minCoordIndex = i;
                }
            }
            return minCoordIndex;
        }

        /// <summary>
        /// Shifts the positions of the coordinates until <c>firstCoordinate</c> is first.
        /// </summary>
        /// <param name="seq">The coordinate sequence to rearrange</param>
        /// <param name="firstCoordinate">The coordinate to make first"></param>
        public static void Scroll(CoordinateSequence seq, Coordinate firstCoordinate)
        {
            int i = IndexOf(firstCoordinate, seq);
            if (i <= 0) return;
            Scroll(seq, i);
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate at  <c>firstCoordinateIndex</c>
        /// is first.
        /// </summary>
        /// <param name="seq">The coordinate sequence to rearrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        public static void Scroll(CoordinateSequence seq, int indexOfFirstCoordinate)
        {
            Scroll(seq, indexOfFirstCoordinate, IsRing(seq));
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate at  <c>firstCoordinateIndex</c>
        /// is first.
        /// </summary>
        /// <param name="seq">The coordinate sequence to rearrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        /// <param name="ensureRing">Makes sure that <paramref name="seq"/> will be a closed ring upon exit</param>
        public static void Scroll(CoordinateSequence seq, int indexOfFirstCoordinate, bool ensureRing)
        {
            int i = indexOfFirstCoordinate;
            if (i <= 0) return;

            // make a copy of the sequence
            var copy = seq.Copy();

            // test if ring, determine last index
            int last = ensureRing ? seq.Count - 1 : seq.Count;

            // fill in values
            for (int j = 0; j < last; j++)
            {
                for (int k = 0; k < seq.Dimension; k++)
                    seq.SetOrdinate(j, k, copy.GetOrdinate((indexOfFirstCoordinate + j) % last, k));
            }

            // Fix the ring (first == last)
            if (ensureRing)
            {
                for (int k = 0; k < seq.Dimension; k++)
                    seq.SetOrdinate(last, k, seq.GetOrdinate(0, k));
            }
        }

        /// <summary>
        /// Returns the index of <c>coordinate</c> in a <see cref="CoordinateSequence"/>
        /// The first position is 0; the second, 1; etc.
        /// </summary>
        /// <param name="coordinate">The <c>Coordinate</c> to search for</param>
        /// <param name="seq">The coordinate sequence to search</param>
        /// <returns>
        /// The position of <c>coordinate</c>, or -1 if it is not found
        /// </returns>
        public static int IndexOf(Coordinate coordinate, CoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                if (coordinate.X == seq.GetOrdinate(i, 0) &&
                    coordinate.Y == seq.GetOrdinate(i, 1))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
