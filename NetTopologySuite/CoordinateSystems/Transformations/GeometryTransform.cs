using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
    /// <summary>
    /// Helper class for transforming <see cref="Geometry{TCoordinate}" /> objects.
    /// </summary>
    public static class GeometryTransform<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<Double, TCoordinate>, IConvertible
    {
        //private static IPoint<TCoordinate> ToNTS(Double x, Double y)
        //{
        //    return new Point<TCoordinate>(x, y);
        //}

        //private static Double[] ToArray(Double x, Double y)
        //{
        //    return new Double[] {x, y,};
        //}

        /// <summary>
        /// Transforms a <see cref="IExtents" /> object.
        /// </summary>
        public static IExtents<TCoordinate> TransformBox(
            IExtents<TCoordinate> box, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            if (box == null)
            {
                return null;
            }

            IExtents<TCoordinate> result = new Extents<TCoordinate>(
                box.Factory,
                transform.Transform(box.Min),
                transform.Transform(box.Max));

            return result;
        }

        /// <summary>
        /// Transforms a <see cref="Geometry{TCoordinate}" /> object.
        /// </summary>
        public static IGeometry<TCoordinate> TransformGeometry(
            IGeometry<TCoordinate> g, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            if (g == null)
            {
                return null;
            }

            if (transform == null) throw new ArgumentNullException("transform");
            if (geoFactory == null) throw new ArgumentNullException("geoFactory");

            if (g is IPoint<TCoordinate>)
            {
                return TransformPoint(g as IPoint<TCoordinate>, transform, geoFactory);
            }
            
            if (g is ILineString<TCoordinate>)
            {
                return TransformLineString(g as ILineString<TCoordinate>, transform, geoFactory);
            }
            
            if (g is IPolygon<TCoordinate>)
            {
                return TransformPolygon(g as IPolygon<TCoordinate>, transform, geoFactory);
            }
            
            if (g is IMultiPoint<TCoordinate>)
            {
                return TransformMultiPoint(g as IMultiPoint<TCoordinate>, transform, geoFactory);
            }
            
            if (g is IMultiLineString<TCoordinate>)
            {
                return TransformMultiLineString(g as IMultiLineString<TCoordinate>, transform, geoFactory);
            }
            
            if (g is IMultiPolygon<TCoordinate>)
            {
                return TransformMultiPolygon(g as IMultiPolygon<TCoordinate>, transform, geoFactory);
            }
            
            throw new ArgumentException("Could not transform geometry type '" 
                + g.GetType() + "'");
        }

        /// <summary>
        /// Transforms a <see cref="IPoint{TCoordinate}" /> object.
        /// </summary>
        public static IPoint<TCoordinate> TransformPoint(
            IPoint<TCoordinate> p, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            try
            {
                TCoordinate point = transform.Transform(p.Coordinate);
                return geoFactory.CreatePoint(point);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="ILineString{TCoordinate}" /> object.
        /// </summary>
        public static ILineString<TCoordinate> TransformLineString(
            ILineString<TCoordinate> l, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            try
            {
                IEnumerable<TCoordinate> coords = extractCoordinates(l, transform);
                return geoFactory.CreateLineString(coords);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="ILinearRing{TCoordinate}" /> object.
        /// </summary>
        public static ILinearRing<TCoordinate> TransformLinearRing(
            ILinearRing<TCoordinate> r, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            try
            {
                IEnumerable<TCoordinate> coords = extractCoordinates(r, transform);
                return geoFactory.CreateLinearRing(coords);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="IPolygon{TCoordinate}" /> object.
        /// </summary>
        public static IPolygon<TCoordinate> TransformPolygon(
            IPolygon<TCoordinate> p, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            List<ILinearRing<TCoordinate>> rings
                = new List<ILinearRing<TCoordinate>>(p.InteriorRings.Count);

            foreach (ILinearRing<TCoordinate> hole in p.InteriorRings)
            {
                rings.Add(hole);
            }

            ILinearRing<TCoordinate> shell = TransformLinearRing(
                p.ExteriorRing as ILinearRing<TCoordinate>, transform, geoFactory);
            return geoFactory.CreatePolygon(shell, rings);
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPoint{TCoordinate}" /> object.
        /// </summary>
        public static IMultiPoint<TCoordinate> TransformMultiPoint(
            IMultiPoint<TCoordinate> points, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            List<TCoordinate> pointList = new List<TCoordinate>(points.Count);

            IEnumerable<IPoint<TCoordinate>> pointsEnum = points;

            foreach (IPoint<TCoordinate> p in pointsEnum)
            {
                pointList.Add(p.Coordinate);
            }

            IEnumerable<TCoordinate> coordinates = transform.Transform(pointList);
            return geoFactory.CreateMultiPoint(coordinates); 
        }

        /// <summary>
        /// Transforms a <see cref="IMultiLineString{TCoordinate}" /> object.
        /// </summary>
        public static IMultiLineString<TCoordinate> TransformMultiLineString(
            IMultiLineString<TCoordinate> lines, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            List<ILineString<TCoordinate>> strings = new List<ILineString<TCoordinate>>(lines.Count);

            foreach (ILineString<TCoordinate> ls in (IEnumerable<ILineString<TCoordinate>>)lines)
            {
                strings.Add(TransformLineString(ls, transform, geoFactory));
            }

            return geoFactory.CreateMultiLineString(strings);
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPolygon{TCoordinate}" /> object.
        /// </summary>
        public static IMultiPolygon<TCoordinate> TransformMultiPolygon(
            IMultiPolygon<TCoordinate> polys, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            List<IPolygon<TCoordinate>> polygons = new List<IPolygon<TCoordinate>>(polys.Count);

            foreach (IPolygon<TCoordinate> p in polys)
            {
                polygons.Add(TransformPolygon(p, transform, geoFactory));
            }

            return geoFactory.CreateMultiPolygon(polygons);
        }

        /// <summary>
        /// Transforms a <see cref="IGeometryCollection{TCoordinate}" /> object.
        /// </summary>
        public static IGeometryCollection<TCoordinate> TransformGeometryCollection(
            IGeometryCollection<TCoordinate> geoms, IMathTransform<TCoordinate> transform,
            IGeometryFactory<TCoordinate> geoFactory)
        {
            List<IGeometry<TCoordinate>> coll = new List<IGeometry<TCoordinate>>(geoms.Count);

            foreach (IGeometry<TCoordinate> g in geoms)
            {
                coll.Add(TransformGeometry(g, transform, geoFactory));
            }

            return geoFactory.CreateGeometryCollection(coll);
        }

        private static IEnumerable<TCoordinate> extractCoordinates(
            IGeometry<TCoordinate> g, IMathTransform<TCoordinate> transform)
        {
            return transform.Transform(g.Coordinates);
        }
    }
}