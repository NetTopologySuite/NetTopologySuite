using System;
using System.Text;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Utility functions for manipulating <see cref="ICoordinateSequence" />s.
    /// </summary>
    public static class CoordinateSequences
    {
        /// <summary>
        /// Reverses the coordinates in a sequence in-place.
        /// </summary>
        /// <param name="seq"></param>
        public static void Reverse(ICoordinateSequence seq)
        {
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
        public static void Swap(ICoordinateSequence seq, int i, int j)
        {
            if (i == j)
                return;

            for (int dim = 0; dim < seq.Dimension; dim++)
            {
                double tmp = seq.GetOrdinate(i, (Ordinate)dim);
                seq.SetOrdinate(i, (Ordinate)dim, seq.GetOrdinate(j, (Ordinate)dim));
                seq.SetOrdinate(j, (Ordinate)dim, tmp);
            }
        }

        ///<summary>
        /// Copies a section of a <see cref="ICoordinateSequence"/> to another <see cref="ICoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        ///</summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        public static void Copy(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos, int length)
        {
            for (int i = 0; i < length; i++)
                CopyCoord(src, srcPos + i, dest, destPos + i);
        }

        ///<summary>
        /// Copies a coordinate of a <see cref="ICoordinateSequence"/> to another <see cref="ICoordinateSequence"/>.
        /// The sequences may have different dimensions;
        /// in this case only the common dimensions are copied.
        ///</summary>
        /// <param name="src">The sequence to copy coordinate from</param>
        /// <param name="srcPos">The index of the coordinate to copy</param>
        /// <param name="dest">The sequence to which the coordinate should be copied to</param>
        /// <param name="destPos">The index of the coordinate in <see paramref="dest"/></param>
        public static void CopyCoord(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos)
        {
            int minDim = Math.Min(src.Dimension, dest.Dimension);
            for (int dim = 0; dim < minDim; dim++)
            {
                var ordinate = (Ordinate)dim;
                double value = src.GetOrdinate(srcPos, ordinate);
                dest.SetOrdinate(destPos, ordinate, value);
            }
        }

        /// <summary>
        /// Tests whether a <see cref="ICoordinateSequence"/> forms a valid <see cref="ILinearRing"/>,
        /// by checking the sequence length and closure
        /// (whether the first and last points are identical in 2D).
        /// Self-intersection is not checked.
        /// </summary>
        /// <param name="seq">The sequence to test</param>
        /// <returns>True if the sequence is a ring</returns>
        /// <seealso cref="ILinearRing"/>
        public static bool IsRing(ICoordinateSequence seq)
        {
            int n = seq.Count;
            if (n == 0) return true;
            // too few points
            if (n <= 3)
                return false;
            // test if closed
            return seq.GetOrdinate(0, Ordinate.X) == seq.GetOrdinate(n - 1, Ordinate.X)
                && seq.GetOrdinate(0, Ordinate.Y) == seq.GetOrdinate(n - 1, Ordinate.Y);
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
        public static ICoordinateSequence EnsureValidRing(ICoordinateSequenceFactory fact, ICoordinateSequence seq)
        {
            int n = seq.Count;
            // empty sequence is valid
            if (n == 0) return seq;
            // too short - make a new one
            if (n <= 3)
                return CreateClosedRing(fact, seq, 4);

            bool isClosed = seq.GetOrdinate(0, Ordinate.X) == seq.GetOrdinate(n - 1, Ordinate.X) &&
                           seq.GetOrdinate(0, Ordinate.Y) == seq.GetOrdinate(n - 1, Ordinate.Y);
            if (isClosed) return seq;
            // make a new closed ring
            return CreateClosedRing(fact, seq, n + 1);
        }

        private static ICoordinateSequence CreateClosedRing(ICoordinateSequenceFactory fact, ICoordinateSequence seq, int size)
        {
            var newseq = fact.Create(size, seq.Dimension);
            int n = seq.Count;
            Copy(seq, 0, newseq, 0, n);
            // fill remaining coordinates with start point
            for (int i = n; i < size; i++)
                Copy(seq, 0, newseq, i, 1);
            return newseq;
        }

        public static ICoordinateSequence Extend(ICoordinateSequenceFactory fact, ICoordinateSequence seq, int size)
        {
            var newseq = fact.Create(size, seq.Ordinates);
            int n = seq.Count;
            Copy(seq, 0, newseq, 0, n);
            // fill remaining coordinates with end point, if it exists
            if (n > 0)
            {
                for (int i = n; i < size; i++)
                    Copy(seq, n - 1, newseq, i, 1);
            }
            return newseq;
        }

        /// <summary>
        /// Tests whether two <see cref="ICoordinateSequence"/>s are equal.
        /// To be equal, the sequences must be the same length.
        /// They do not need to be of the same dimension,
        /// but the ordinate values for the smallest dimension of the two
        /// must be equal.
        /// Two <c>NaN</c> ordinates values are considered to be equal.
        /// </summary>
        /// <param name="cs1">a CoordinateSequence</param>
        /// <param name="cs2">a CoordinateSequence</param>
        /// <returns><c>true</c> if the sequences are equal in the common dimensions</returns>
        public static bool IsEqual(ICoordinateSequence cs1, ICoordinateSequence cs2)
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
                    var ordinate = (Ordinate)d;
                    double v1 = cs1.GetOrdinate(i, ordinate);
                    double v2 = cs2.GetOrdinate(i, ordinate);
                    if (cs1.GetOrdinate(i, ordinate) == cs2.GetOrdinate(i, ordinate))
                        continue;
                    // special check for NaNs
                    if (double.IsNaN(v1) && double.IsNaN(v2))
                        continue;
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Creates a string representation of a <see cref="ICoordinateSequence"/>.
        /// The format is:
        /// <para>
        ///  ( ord0,ord1.. ord0,ord1,...  ... )
        /// </para>
        /// </summary>
        /// <param name="cs">the sequence to output</param>
        /// <returns>the string representation of the sequence</returns>
        public static string ToString(ICoordinateSequence cs)
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
                    double ordinate = cs.GetOrdinate(i, (Ordinate)d);
                    sb.Append(string.Format("{0:0.#}", ordinate));
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
        public static Coordinate MinCoordinate(ICoordinateSequence seq)
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
        /// <param name="from">The lower search index</param>
        /// <param name="to">The upper search index</param>
        /// <returns>The index of the minimum coordinate in the sequence, found using <see cref="Coordinate.CompareTo(Coordinate)"/></returns>
        public static int MinCoordinateIndex(ICoordinateSequence seq)
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
        public static int MinCoordinateIndex(ICoordinateSequence seq, int from, int to)
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
        public static void Scroll(ICoordinateSequence seq, Coordinate firstCoordinate)
        {
            int i = IndexOf(firstCoordinate, seq);
            if (i <= 0) return;
            Scroll(seq, i);
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate at  <code>firstCoordinateIndex</code>
        /// is first.
        /// </summary>
        /// <param name="seq">The coordinate sequence to rearrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        public static void Scroll(ICoordinateSequence seq, int indexOfFirstCoordinate)
        {
            Scroll(seq, indexOfFirstCoordinate, CoordinateSequences.IsRing(seq));
        }

        /// <summary>
        /// Shifts the positions of the coordinates until the coordinate at  <code>firstCoordinateIndex</code>
        /// is first.
        /// </summary>
        /// <param name="seq">The coordinate sequence to rearrange</param>
        /// <param name="indexOfFirstCoordinate">The index of the coordinate to make first</param>
        /// <param name="ensureRing">Makes sure that <paramref name="seq"/> will be a closed ring upon exit</param>
        public static void Scroll(ICoordinateSequence seq, int indexOfFirstCoordinate, bool ensureRing)
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
                    seq.SetOrdinate(j, (Ordinate)k, copy.GetOrdinate((indexOfFirstCoordinate + j) % last, (Ordinate)k));
            }

            // Fix the ring (first == last)
            if (ensureRing)
            {
                for (int k = 0; k < seq.Dimension; k++)
                    seq.SetOrdinate(last, (Ordinate)k, seq.GetOrdinate(0, (Ordinate)k));
            }
        }

        /// <summary>
        /// Returns the index of <c>coordinate</c> in a <see cref="ICoordinateSequence"/>
        /// The first position is 0; the second, 1; etc.
        /// </summary>
        /// <param name="coordinate">The <c>Coordinate</c> to search for</param>
        /// <param name="seq">The coordinate sequence to search</param>
        /// <returns>
        /// The position of <c>coordinate</c>, or -1 if it is not found
        /// </returns>
        public static int IndexOf(Coordinate coordinate, ICoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                if (coordinate.X == seq.GetOrdinate(i, Ordinate.X) &&
                    coordinate.Y == seq.GetOrdinate(i, Ordinate.Y))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
