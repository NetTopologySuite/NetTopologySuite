using System.Collections.Generic;

namespace GeoAPI.Geometries
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
    }
}