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
                double tmp = seq.GetOrdinate(i, (Ordinates)dim);
                seq.SetOrdinate(i, (Ordinates)dim, seq.GetOrdinate(j, (Ordinates)dim));
                seq.SetOrdinate(j, (Ordinates)dim, tmp);
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
            for (Ordinates dim = 0; (int)dim < src.Dimension; dim++)
                dest.SetOrdinate(destPos, dim, src.GetOrdinate(srcPos, dim));
        }

    }
}
