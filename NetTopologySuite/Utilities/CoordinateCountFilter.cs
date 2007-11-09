using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <c>CoordinateFilter</c> that counts the total number of coordinates
    /// in a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    public class CoordinateCountFilter<TCoordinate> : ICoordinateFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        private Int32 _count = 0;

        /// <summary>
        /// Returns the result of the filtering.
        /// </summary>
        public Int32 Count
        {
            get { return _count; }
        }

        public void Filter(TCoordinate coord)
        {
            _count++;
        }
    }
}