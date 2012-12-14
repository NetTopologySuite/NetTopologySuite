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
        /// <param name="seq"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
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
        /// The sequences must have the same dimension.
        ///</summary>
        /// <param name="src">The sequence to copy coordinates from</param>
        /// <param name="srcPos">The starting index of the coordinates to copy</param>
        /// <param name="dest">The sequence to which the coordinates should be copied to</param>
        /// <param name="destPos">The starting index of the coordinates in <see paramref="dest"/></param>
        /// <param name="length">The number of coordinates to copy</param>
        public static void Copy(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos, int length)
        {
            for (var i = 0; i < length; i++)
                CopyCoord(src, srcPos + i, dest, destPos + i);
        }

        ///<summary>
        /// Copies a coordinate of a <see cref="ICoordinateSequence"/> to another <see cref="ICoordinateSequence"/>.
        /// The sequences must have the same dimension.
        ///</summary>
        /// <param name="src">The sequence to copy coordinate from</param>
        /// <param name="srcPos">The index of the coordinate to copy</param>
        /// <param name="dest">The sequence to which the coordinate should be copied to</param>
        /// <param name="destPos">The index of the coordinate in <see paramref="dest"/></param>
        public static void CopyCoord(ICoordinateSequence src, int srcPos, ICoordinateSequence dest, int destPos)
        {
            for (Ordinate dim = 0; (int)dim < src.Dimension; dim++)
                dest.SetOrdinate(destPos, dim, src.GetOrdinate(srcPos, dim));
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
            var n = seq.Count;
            // empty sequence is valid
            if (n == 0) return seq;
            // too short - make a new one
            if (n <= 3)
                return CreateClosedRing(fact, seq, 4);

            var isClosed = seq.GetOrdinate(0, Ordinate.X) == seq.GetOrdinate(n - 1, Ordinate.X) &&
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
            var n = seq.Count;
            Copy(seq, 0, newseq, 0, n);
            // fill remaining coordinates with end point, if it exists
            if (n > 0)
            {
                for (var i = n; i < size; i++)
                    Copy(seq, n - 1, newseq, i, 1);
            }
            return newseq;
        }
  

    }
}
