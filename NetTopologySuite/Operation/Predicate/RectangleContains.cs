using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Predicate
{
    /// <summary>
    /// Optimized implementation of spatial predicate "contains"
    /// for cases where the first <see cref="Geometry{TCoordinate}"/> is a rectangle.    
    /// As a further optimization,
    /// this class can be used directly to test many geometries against a single rectangle.
    /// </summary>
    public class RectangleContains<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        public static Boolean Contains(IPolygon<TCoordinate> rectangle, IGeometry<TCoordinate> b)
        {
            RectangleContains<TCoordinate> rc = new RectangleContains<TCoordinate>(rectangle);
            return rc.Contains(b);
        }

        private IPolygon<TCoordinate> _rectangle;
        private readonly IExtents<TCoordinate> _rectExtents;

        /// <summary>
        /// Create a new contains computer for two geometries.
        /// </summary>
        /// <param name="rectangle">A rectangular geometry.</param>
        public RectangleContains(IPolygon<TCoordinate> rectangle)
        {
            _rectangle = rectangle;
            _rectExtents = rectangle.Extents;
        }

        public Boolean Contains(IGeometry<TCoordinate> geom)
        {
            if (!_rectExtents.Contains(geom.Extents))
            {
                return false;
            }

            // check that geom is not contained entirely in the rectangle boundary
            if (isContainedInBoundary(geom))
            {
                return false;
            }

            return true;
        }

        private Boolean isContainedInBoundary(IGeometry<TCoordinate> geom)
        {
            // polygons can never be wholely contained in the boundary
            if (geom is IPolygon<TCoordinate>)
            {
                return false;
            }
            if (geom is IPoint<TCoordinate>)
            {
                return isPointContainedInBoundary(geom as IPoint<TCoordinate>);
            }
            if (geom is ILineString<TCoordinate>)
            {
                return isLineStringContainedInBoundary(geom as ILineString<TCoordinate>);
            }
            if(geom is IGeometryCollection<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> geometry in (geom as IGeometryCollection<TCoordinate>))
                {
                    if (!isContainedInBoundary(geometry))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private Boolean isPointContainedInBoundary(IPoint<TCoordinate> point)
        {
            return isPointContainedInBoundary(point.Coordinate);
        }

        private Boolean isPointContainedInBoundary(TCoordinate pt)
        {
            // we already know that the point is contained in the rectangle envelope
            if (!(pt[Ordinates.X] == _rectExtents.GetMin(Ordinates.X) 
                || pt[Ordinates.X] == _rectExtents.GetMax(Ordinates.X)))
            {
                return false;
            }
            
            if (!(pt[Ordinates.Y] == _rectExtents.GetMin(Ordinates.Y)
                || pt[Ordinates.Y] == _rectExtents.GetMax(Ordinates.Y)))
            {
                return false;
            }

            return true;
        }

        private Boolean isLineStringContainedInBoundary(ILineString<TCoordinate> line)
        {
            ICoordinateSequence<TCoordinate> seq = line.Coordinates;

            for (Int32 i = 0; i < seq.Count - 1; i++)
            {
                TCoordinate p0 = seq[i];
                TCoordinate p1= seq[i + 1];

                if (!isLineSegmentContainedInBoundary(p0, p1))
                {
                    return false;
                }
            }

            return true;
        }

        private Boolean isLineSegmentContainedInBoundary(TCoordinate p0, TCoordinate p1)
        {
            if (p0.Equals(p1))
            {
                return isPointContainedInBoundary(p0);
            }

            // we already know that the segment is contained in the rectangle envelope
            if (p0[Ordinates.X] == p1[Ordinates.X])
            {
                if (p0[Ordinates.X] == _rectExtents.GetMin(Ordinates.X) ||
                    p0[Ordinates.X] == _rectExtents.GetMax(Ordinates.X))
                {
                    return true;
                }
            }
            else if (p0[Ordinates.Y] == p1[Ordinates.Y])
            {
                if (p0[Ordinates.Y] == _rectExtents.GetMin(Ordinates.Y) ||
                    p0[Ordinates.Y] == _rectExtents.GetMax(Ordinates.Y))
                {
                    return true;
                }
            }

            /*
             * Either both x and y values are different
             * or one of x and y are the same, but the other ordinate is not the same as a boundary ordinate
             * In either case, the segment is not wholely in the boundary
             */
            return false;
        }
    }
}