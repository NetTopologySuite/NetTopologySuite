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

            // Test for max ordinate indices to copy
            int minSpatial = Math.Min(src.Spatial, dest.Spatial);
            int minMeasures = Math.Min(src.Measures, dest.Measures);

            // Copy one by one
            for (int i = 0; i < length; i++)
                CopyCoord(src, srcPos + i, dest, destPos + i, minSpatial, minMeasures);
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
            int minSpatial = Math.Min(src.Spatial, dest.Spatial);
            int minMeasures = Math.Min(src.Measures, dest.Measures);
            CopyCoord(src, srcPos, dest, destPos, minSpatial, minMeasures);
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
        /// <param name="numSpatial">The number of spatial ordinates to copy</param>
        /// <param name="numMeasures">The number of measure ordinates to copy</param>
        private static void CopyCoord(CoordinateSequence src, int srcPos, CoordinateSequence dest, int destPos,
            int numSpatial, int numMeasures)
        {
            // Copy spatial ordinates
            for (int dim = 0; dim < numSpatial; dim++)
                dest.SetOrdinate(destPos, dim, src.GetOrdinate(srcPos, dim));

            // Copy measure ordinates
            for (int measure = 0; measure < numMeasures; measure++)
                dest.SetOrdinate(destPos, dest.Spatial + measure,
                    src.GetOrdinate(srcPos, src.Spatial + measure));
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
            var newSeq = fact.Create(size, seq.Dimension, seq.Measures);
            int n = seq.Count;
            Copy(seq, 0, newSeq, 0, n);
            // fill remaining coordinates with start point
            for (int i = n; i < size; i++)
                Copy(seq, 0, newSeq, i, 1);
            return newSeq;
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
            var newSeq = fact.Create(size, seq.Dimension, seq.Measures);
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
        /// <param name="seq1">a CoordinateSequence</param>
        /// <param name="seq2">a CoordinateSequence</param>
        /// <returns><c>true</c> if the sequences are equal in the common dimensions</returns>
        public static bool IsEqual(CoordinateSequence seq1, CoordinateSequence seq2)
        {
            int cs1Size = seq1.Count;
            int cs2Size = seq2.Count;
            if (cs1Size != cs2Size)
                return false;

            int minSpatial = Math.Min(seq1.Spatial, seq2.Spatial);
            int minMeasures = Math.Min(seq1.Measures, seq2.Measures);
            for (int i = 0; i < cs1Size; i++)
            {
                if (!IsEqualAt(seq1, i, seq2, i, minSpatial, minMeasures))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether two <c>Coordinate</c>s <see cref="CoordinateSequence"/>s are equal.
        /// They do not need to be of the same dimension,
        /// but the ordinate values for the common ordinates of the two
        /// must be equal.
        /// Two <c>NaN</c> ordinates values are considered to be equal.
        /// </summary>
        /// <param name="seq1">A CoordinateSequence</param>
        /// <param name="pos1">The index of the <c>Coordinate</c> in <paramref name="seq1"/>.</param>
        /// <param name="seq2">a CoordinateSequence</param>
        /// <param name="pos2">The index of the <c>Coordinate</c> in <paramref name="seq2"/>.</param>
        /// <returns><c>true</c> if the sequences are equal in the common dimensions</returns>
        public static bool IsEqualAt(CoordinateSequence seq1, int pos1, CoordinateSequence seq2, int pos2)
        {
            int minSpatial = Math.Min(seq1.Spatial, seq2.Spatial);
            int minMeasures = Math.Min(seq1.Measures, seq2.Measures);
            return IsEqualAt(seq1, pos1, seq2, pos2, minSpatial, minMeasures);
        }

        /// <summary>
        /// Tests whether two <c>Coordinate</c>s <see cref="CoordinateSequence"/>s are equal.
        /// They do not need to be of the same dimension,
        /// but the ordinate values for the common ordinates of the two
        /// must be equal.
        /// Two <c>NaN</c> ordinates values are considered to be equal.
        /// </summary>
        /// <param name="seq1">A CoordinateSequence</param>
        /// <param name="pos1">The index of the <c>Coordinate</c> in <paramref name="seq1"/>.</param>
        /// <param name="seq2">a CoordinateSequence</param>
        /// <param name="pos2">The index of the <c>Coordinate</c> in <paramref name="seq2"/>.</param>
        /// <param name="numSpatial">The number of spatial ordinates to compare</param>
        /// <param name="numMeasures">The number of measure ordinates to compare</param>
        /// <returns><c>true</c> if the sequences are equal in the common dimensions</returns>
        private static bool IsEqualAt(CoordinateSequence seq1, int pos1, CoordinateSequence seq2, int pos2,
            int numSpatial, int numMeasures)
        {
            for (int i = 0; i < numSpatial; i++)
            {
                double v1 = seq1.GetOrdinate(pos1, i);
                double v2 = seq2.GetOrdinate(pos2, i);
                if (!v1.Equals(v2)) return false;
            }

            for (int i = 0; i < numMeasures; i++)
            {
                double v1 = seq1.GetOrdinate(pos1, seq1.Spatial + i);
                double v2 = seq2.GetOrdinate(pos2, seq2.Spatial + i);
                if (!v1.Equals(v2)) return false;
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
