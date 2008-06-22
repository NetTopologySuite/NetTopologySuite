using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 1-dimensional (<see cref="LineString{TCoordinate}"/>) 
    /// components from a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    public class LinearComponentExtracter<TCoordinate> : IGeometryComponentFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<ILineString<TCoordinate>> _lines
            = new List<ILineString<TCoordinate>>();

        /// <summary> 
        /// Extracts the linear components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <see cref="LinearComponentExtracter{TCoordinate}"/> 
        /// instance and pass it to multiple geometries.
        /// </summary>
        /// <param name="geom">The point from which to extract linear components.</param>
        /// <returns>The list of linear components.</returns>
        public static IEnumerable<ILineString<TCoordinate>> GetLines(IGeometry<TCoordinate> geom)
        {
            List<ILineString<TCoordinate>> lines = new List<ILineString<TCoordinate>>();
            geom.Apply(new LinearComponentExtracter<TCoordinate>(lines));
            return lines;
        }

        /// <summary> 
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        public LinearComponentExtracter(IEnumerable<ILineString<TCoordinate>> lines)
        {
            _lines.AddRange(lines);
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                _lines.Add(geom as ILineString<TCoordinate>);
            }
        }
    }
}