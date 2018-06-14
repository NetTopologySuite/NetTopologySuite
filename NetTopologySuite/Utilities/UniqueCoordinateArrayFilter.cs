using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that builds a set of <c>Coordinate</c>s.
    /// The set of coordinates contains no duplicate points.
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

        private readonly List<Coordinate> _list = new List<Coordinate>();

        /// <summary>
        /// Returns the gathered <see cref="Coordinate"/>s.
        /// </summary>
        public Coordinate[] Coordinates => _list.ToArray();

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        public void Filter(Coordinate coord)
        {
            if (!_list.Contains(coord))
                _list.Add(coord);
        }
    }
}
