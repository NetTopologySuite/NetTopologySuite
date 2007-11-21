using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// An <see cref="ICoordinateFilter{TCoordinate}"/> that builds a set of 
    /// <typeparamref name="TCoordinate"/>s. The set of coordinates contains no
    /// duplicate points.
    /// </summary>
    public class UniqueCoordinateArrayFilter<TCoordinate> : ICoordinateFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly SortedSet<TCoordinate> _coordinateSet = new SortedSet<TCoordinate>();
        private List<TCoordinate> list = new List<TCoordinate>();

        /// <summary>
        /// Gets an enumeration of the gathered <typeparamref name="TCoordinate"/>s.
        /// </summary>
        public IEnumerable<TCoordinate> Coordinates
        {
            get
            {
                foreach (TCoordinate coordinate in list)
                {
                    yield return coordinate;
                }
            }
        }

        public void Filter(TCoordinate coord)
        {
            if (!_coordinateSet.Contains(coord))
            {
                list.Add(coord);
                _coordinateSet.Add(coord);
            }
        }
    }
}