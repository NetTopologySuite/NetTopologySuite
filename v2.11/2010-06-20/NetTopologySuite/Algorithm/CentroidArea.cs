using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of an area point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Based on the usual algorithm for calculating
    /// the centroid as a weighted sum of the centroids
    /// of a decomposition of the area into (possibly overlapping) triangles.
    /// The algorithm has been extended to handle holes and multi-polygons.
    /// See <see href="http://www.faqs.org/faqs/graphics/algorithms-faq"/>
    /// for further details of the basic approach.
    /// </remarks>
    public class CentroidArea<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private Double _areasum2; // Partial area sum
        private TCoordinate _basePoint; // the point all triangles are based at
        private TCoordinate _cg3; // partial centroid sum
        private TCoordinate _triangleCent3; // temporary variable to hold centroid of triangle

        public CentroidArea(ICoordinateFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        public TCoordinate Centroid
        {
            get
            {
                Double x = _cg3[Ordinates.X]/3/_areasum2;
                Double y = _cg3[Ordinates.Y]/3/_areasum2;
                return _factory.Create(x, y);
            }
        }

        /// <summary> 
        /// Adds the area defined by a Geometry to the centroid total.
        /// If the geometry has no area it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The geometry to add.</param>
        public void Add(IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygon<TCoordinate>)
            {
                IPolygon<TCoordinate> poly = geom as IPolygon<TCoordinate>;
                setBasePoint(Slice.GetFirst(poly.ExteriorRing.Coordinates));
                add(poly);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    Add(geometry);
                }
            }
        }

        /// <summary> 
        /// Adds the area defined by an array of
        /// coordinates.  The array must be a ring;
        /// i.e. end with the same coordinate as it starts with.
        /// </summary>
        /// <param name="ring">A set of <typeparamref name="TCoordinate"/>s.</param>
        public void Add(IEnumerable<TCoordinate> ring)
        {
            setBasePoint(Slice.GetFirst(ring));
            addShell(ring);
        }

        private void setBasePoint(TCoordinate basePoint)
        {
            if (Coordinates<TCoordinate>.IsEmpty(_basePoint))
            {
                _basePoint = basePoint;
            }
        }

        private void add(IPolygon<TCoordinate> poly)
        {
            addShell(poly.ExteriorRing.Coordinates);

            foreach (ILineString<TCoordinate> ls in poly.InteriorRings)
            {
                addHole(ls.Coordinates);
            }
        }

        private void addShell(IEnumerable<TCoordinate> points)
        {
            Boolean isPositiveArea = !CGAlgorithms<TCoordinate>.IsCCW(points);
            addRing(points, isPositiveArea);
        }

        private void addHole(IEnumerable<TCoordinate> points)
        {
            Boolean isPositiveArea = CGAlgorithms<TCoordinate>.IsCCW(points);
            addRing(points, isPositiveArea);
        }

        private void addRing(IEnumerable<TCoordinate> points, Boolean isPositiveArea)
        {
            Boolean isPreviousSet = false;
            TCoordinate previousPoint = default(TCoordinate);

            foreach (TCoordinate point in points)
            {
                if (!isPreviousSet)
                {
                    isPreviousSet = true;
                    previousPoint = point;
                    continue;
                }

                addTriangle(_basePoint, previousPoint, point, isPositiveArea);
                previousPoint = point;
            }
        }

        private void addTriangle(TCoordinate p0, TCoordinate p1, TCoordinate p2, Boolean isPositiveArea)
        {
            Double sign = (isPositiveArea) ? 1.0 : -1.0;
            centroid3(p0, p1, p2, ref _triangleCent3);
            Double a2 = area2(p0, p1, p2);

            Double x = 0;
            Double y = 0;

            if (!Coordinates<TCoordinate>.IsEmpty(_cg3))
            {
                x = _cg3[Ordinates.X];
                y = _cg3[Ordinates.Y];
            }

            x += sign*a2*_triangleCent3[Ordinates.X];
            y += sign*a2*_triangleCent3[Ordinates.Y];
            _cg3 = _factory.Create(x, y);
            _areasum2 += sign*a2;
        }

        /// <summary> 
        /// Returns three times the centroid of the triangle p1-p2-p3.
        /// The factor of 3 is left in to permit division to be avoided until later.
        /// </summary>
        private void centroid3(TCoordinate p1, TCoordinate p2, TCoordinate p3, ref TCoordinate c)
        {
            Double x = p1[Ordinates.X] + p2[Ordinates.X] + p3[Ordinates.X];
            Double y = p1[Ordinates.Y] + p2[Ordinates.Y] + p3[Ordinates.Y];
            c = _factory.Create(x, y);
            return;
        }

        /// <summary>
        /// Returns twice the signed area of the triangle p1-p2-p3,
        /// positive if a,b,c are oriented ccw, and negative if cw.
        /// </summary>
        private static Double area2(TCoordinate p1, TCoordinate p2, TCoordinate p3)
        {
            return (p2[Ordinates.X] - p1[Ordinates.X])*(p3[Ordinates.Y] - p1[Ordinates.Y])
                   - (p3[Ordinates.X] - p1[Ordinates.X])*(p2[Ordinates.Y] - p1[Ordinates.Y]);
        }
    }
}