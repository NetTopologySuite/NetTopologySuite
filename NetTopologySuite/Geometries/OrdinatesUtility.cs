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
            // https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
            unchecked
            {
                uint v = (uint)ordinates;
                v = v - ((v >> 1) & 0x55555555);
                v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
                return (int)(((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24);
            }
        }

        /// <summary>
        /// Translates the <paramref name="ordinates"/>-flag to a number of measures.
        /// </summary>
        /// <param name="ordinates">The ordinates flag</param>
        /// <returns>The number of measures</returns>
        public static int OrdinatesToMeasures(Ordinates ordinates)
        {
            return OrdinatesToDimension(ordinates & Ordinates.AllMeasureOrdinates);
        }
    }
}
