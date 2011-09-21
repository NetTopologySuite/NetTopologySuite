namespace NetTopologySuite.Index.Strtree
{
    using System;

    /// <summary>
    /// A function method which computes the distance
    /// between two <see cref="ItemBoundable"/>s in an <see cref="STRtree"/>.
    /// Used for Nearest Neighbour searches.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IItemDistance
    {
        /**
         * 
         * 
         * @param item1
         * @param item2
         * @return the distance between the items
         * 
         * @throws IllegalArgumentException 
         */
        /// <summary>
        /// Computes the distance between two items.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="ArgumentException">If the metric is not applicable to the arguments</exception>
        /// <returns>The distance between <paramref name="item1"/> and <paramref name="item2"/>.</returns>
        double Distance(ItemBoundable item1, ItemBoundable item2);

    }
}