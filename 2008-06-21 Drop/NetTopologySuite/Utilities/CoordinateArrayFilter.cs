using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Utilities
{
    /// <summary>
    /// A <see cref="ICoordinateFilter{TCoordinate}"/> that creates an array 
    /// containing every coordinate in a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    public class CoordinateArrayFilter<TCoordinate> : ICoordinateFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly TCoordinate[] _points = null;
        private Int32 _count = 0;

        /// <summary>
        /// Constructs a <c>CoordinateArrayFilter</c>.
        /// </summary>
        /// <param name="size">The number of points that the <c>CoordinateArrayFilter</c> will collect.</param>
        public CoordinateArrayFilter(Int32 size) 
        {
            _points = new TCoordinate[size];
        }

        /// <summary>
        /// Returns the <typeparam name="TCoordinate">s collected by this 
        /// <see cref="CoordinateArrayFilter{TCoordinate}"/>.
        /// </summary>
        public TCoordinate[] Coordinates
        {
            get
            {
                return _points;
            }
        }

        public void Filter(TCoordinate coord) 
        {
            _points[_count++] = coord;
        }
    }
}
