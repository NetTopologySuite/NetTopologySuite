using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    ///
    /// </summary>
    internal class ReverseOrder :IComparer
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(object x, object y)
        {
            // flips result
            return Comparer<object>.Default.Compare(x, y) * -1;
        }

    }
}
