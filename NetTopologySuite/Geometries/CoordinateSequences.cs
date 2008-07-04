using GeoAPI.Geometries;

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
    }
}
