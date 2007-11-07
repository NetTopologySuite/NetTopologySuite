using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
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
                Double tmp = seq.GetOrdinate(i, (Ordinates)dim);
                seq.SetOrdinate(i, (Ordinates)dim, seq.GetOrdinate(j, (Ordinates)dim));
                seq.SetOrdinate(j, (Ordinates)dim, tmp);
            }
        }
    }
}