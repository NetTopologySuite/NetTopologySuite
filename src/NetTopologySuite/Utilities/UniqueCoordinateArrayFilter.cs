using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <see cref="ICoordinateFilter"/>
    /// that extracts a unique array of<c>Coordinate</c> s.
    /// The array of coordinates contains no duplicate points.
    /// <para/>
    /// It preserves the order of the input points.
    /// </summary>
    public class UniqueCoordinateArrayFilter : ICoordinateFilter
    {
        /// <summary>
        /// Convenience method which allows running the filter over an array of <see cref="Coordinate"/>s.
        /// </summary>
        /// <param name="coords">an array of coordinates</param>
        /// <returns>an array of the unique coordinates</returns>
        public static Coordinate[] FilterCoordinates(Coordinate[] coords)
        {
            var filter = new UniqueCoordinateArrayFilter();
            for (int i = 0; i < coords.Length; i++)
                filter.Filter(coords[i]);
            return filter.Coordinates;
        }

        private readonly ISet<Coordinate> _coordSet = new HashSet<Coordinate>();
        // Use an auxiliary list as well in order to preserve coordinate order
        private readonly List<Coordinate> _list = new List<Coordinate>();

        /// <summary>
        /// Returns the gathered <see cref="Coordinate"/>s.
        /// </summary>
        /// <returns>The <c>Coordinate</c>s collected by this <c>ICoordinateArrayFilter</c></returns>
        public Coordinate[] Coordinates => _list.ToArray();

        /// <inheritdoc cref="ICoordinateFilter.Filter"/>
        public void Filter(Coordinate coord)
        {
            if (_coordSet.Add(coord))
                _list.Add(coord);
        }
    }
}
