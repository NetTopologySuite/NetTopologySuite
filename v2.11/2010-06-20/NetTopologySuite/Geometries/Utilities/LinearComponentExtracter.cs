using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 1-dimensional (<see cref="ILineString{TCoordinate}"/>) 
    /// components from a <see cref="IGeometry{TCoordinate}"/>.
    /// </summary>
    public class LinearComponentExtracter<TCoordinate> /*: IGeometryComponentFilter<TCoordinate>*/
        where TCoordinate : IEquatable<TCoordinate>, IComparable<TCoordinate>, ICoordinate<TCoordinate>,
            IComputable<double, TCoordinate>
    {
        /// <summary> 
        /// Extracts the linear components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <see cref="LinearComponentExtracter{TCoordinate}"/> 
        /// instance and pass it to multiple geometries.
        /// </summary>
        /// <param name="geometry">The point from which to extract linear components.</param>
        /// <returns>The list of linear components.</returns>
        public static IEnumerable<ILineString<TCoordinate>> GetLines(IGeometry<TCoordinate> geometry)
        {
            //IList<IGeometry<TCoordinate>> lines = new List<IGeometry<TCoordinate>>();
            return new List<ILineString<TCoordinate>>(
                GeometryComponentFilter<TCoordinate>.Filter<ILineString<TCoordinate>>(geometry));
        }
        
        /*
        private readonly List<ILineString<TCoordinate>> _lines
            = new List<ILineString<TCoordinate>>();

        /// <summary> 
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        /// </summary>
        public LinearComponentExtracter(IEnumerable<ILineString<TCoordinate>> lines)
        {
            _lines.AddRange(lines);
        }

        #region IGeometryComponentFilter<TCoordinate> Members

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                _lines.Add(geom as ILineString<TCoordinate>);
            }
        }

        #endregion

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
            if (geom is ILineString<TCoordinate>)
                yield return geom as ILineString<TCoordinate>;
            else
            {
                if (geom is IPolygon<TCoordinate>)
                {
                    IPolygon<TCoordinate> polygon = geom as IPolygon<TCoordinate>;
                    yield return polygon.ExteriorRing;
                    foreach (ILineString<TCoordinate> lineString in polygon.InteriorRings)
                    {
                        yield return lineString;
                    }
                }
                else if (geom is IGeometryCollection<TCoordinate>)
                {
                    foreach (IGeometry<TCoordinate> geometry in geom as IGeometryCollection<TCoordinate>)
                    {
                        foreach (ILineString<TCoordinate> lineString in GetLines(geometry))
                            yield return lineString;
                    }
                }
            }

            //jd: need to compare with JTS
        }
         */
    }
}