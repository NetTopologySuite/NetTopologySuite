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
    public class GeometryTransform<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
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
        public static IExtents<TCoordinate> TransformBox(IExtents<TCoordinate> box, IMathTransform<TCoordinate> transform)
        {
            if (box == null)
            {
                return null;
            }

            IExtents<TCoordinate> result = new Extents<TCoordinate>(
                transform.Transform(box.Min),
                transform.Transform(box.Max));

            return result;
        }

        /// <summary>
        /// Transforms a <see cref="Geometry{TCoordinate}" /> object.
        /// </summary>
        public static IGeometry<TCoordinate> TransformGeometry(IGeometry<TCoordinate> g, IMathTransform<TCoordinate> transform)
        {
            if (g == null)
            {
                return null;
            }
            else if (g is IPoint<TCoordinate>)
            {
                return TransformPoint(g as IPoint<TCoordinate>, transform);
            }
            else if (g is ILineString<TCoordinate>)
            {
                return TransformLineString(g as ILineString<TCoordinate>, transform);
            }
            else if (g is IPolygon<TCoordinate>)
            {
                return TransformPolygon(g as IPolygon<TCoordinate>, transform);
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                return TransformMultiPoint(g as IMultiPoint<TCoordinate>, transform);
            }
            else if (g is IMultiLineString<TCoordinate>)
            {
                return TransformMultiLineString(g as IMultiLineString<TCoordinate>, transform);
            }
            else if (g is IMultiPolygon<TCoordinate>)
            {
                return TransformMultiPolygon(g as IMultiPolygon<TCoordinate>, transform);
            }
            else
            {
                throw new ArgumentException("Could not transform geometry type '" + g.GetType().ToString() + "'");
            }
        }

        /// <summary>
        /// Transforms a <see cref="Point" /> object.
        /// </summary>
        public static IPoint<TCoordinate> TransformPoint(IPoint<TCoordinate> p, IMathTransform<TCoordinate> transform)
        {
            try
            {
                TCoordinate point = transform.Transform(p.Coordinate);
                return new Point<TCoordinate>(point);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="LineString" /> object.
        /// </summary>
        public static ILineString<TCoordinate> TransformLineString(ILineString<TCoordinate> l, IMathTransform<TCoordinate> transform)
        {
            try
            {
                List<ICoordinate> coords = ExtractCoordinates(l, transform);
                return new LineString<TCoordinate>(coords.ToArray());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Transforms a <see cref="LinearRing" /> object.
        /// </summary>
        public static ILinearRing<TCoordinate> TransformLinearRing(ILinearRing<TCoordinate> r, IMathTransform<TCoordinate> transform)
        {
            try
            {
                IEnumerable<TCoordinate> coords = ExtractCoordinates(r, transform);
                return new LinearRing<TCoordinate>(coords);
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<TCoordinate> ExtractCoordinates(IGeometry<TCoordinate> g, IMathTransform<TCoordinate> transform)
        {
            return transform.Transform(g.Coordinates);
        }

        /// <summary>
        /// Transforms a <see cref="Polygon" /> object.
        /// </summary>
        public static IPolygon<TCoordinate> TransformPolygon(IPolygon<TCoordinate> p, IMathTransform<TCoordinate> transform)
        {
            List<ILinearRing<TCoordinate>> rings = new List<ILinearRing<TCoordinate>>(p.InteriorRings.Count);

            foreach (ILinearRing<TCoordinate> hole in p.InteriorRings)
            {
                rings.Add(hole);
            }

            ILinearRing<TCoordinate> shell = TransformLinearRing(p.ExteriorRing as ILinearRing<TCoordinate>, transform);
            return new Polygon<TCoordinate>(shell, rings);
        }

        /// <summary>
        /// Transforms a <see cref="MultiPoint" /> object.
        /// </summary>
        public static IMultiPoint<TCoordinate> TransformMultiPoint(IMultiPoint<TCoordinate> points, IMathTransform<TCoordinate> transform)
        {
            List<TCoordinate> pointList = new List<TCoordinate>(points.Count);

            foreach (IPoint<TCoordinate> p in points)
            {
                pointList.Add(p.Coordinate);
            }

            return new MultiPoint(transform.Transform(pointList));
        }

        /// <summary>
        /// Transforms a <see cref="MultiLineString" /> object.
        /// </summary>
        public static IMultiLineString TransformMultiLineString(IMultiLineString lines, IMathTransform transform)
        {
            List<ILineString> strings = new List<ILineString>(lines.Geometries.Length);
            
            foreach (ILineString ls in lines.Geometries)
            {
                strings.Add(TransformLineString(ls, transform));
            }

            return new MultiLineString(strings.ToArray());
        }

        /// <summary>
        /// Transforms a <see cref="MultiPolygon" /> object.
        /// </summary>
        public static IMultiPolygon TransformMultiPolygon(IMultiPolygon polys, IMathTransform transform)
        {
            List<IPolygon> polygons = new List<IPolygon>(polys.Geometries.Length);
            
            foreach (IPolygon p in polys.Geometries)
            {
                polygons.Add(TransformPolygon(p, transform));
            }

            return new MultiPolygon(polygons.ToArray());
        }

        /// <summary>
        /// Transforms a <see cref="GeometryCollection{TCoordinate}" /> object.
        /// </summary>
        public static IGeometryCollection TransformGeometryCollection(GeometryCollection geoms, IMathTransform transform)
        {
            List<IGeometry> coll = new List<IGeometry>(geoms.Geometries.Length);

            foreach (IGeometry g in geoms.Geometries)
            {
                coll.Add(TransformGeometry(g, transform));
            }

            return new GeometryCollection(coll.ToArray());
        }
    }
}