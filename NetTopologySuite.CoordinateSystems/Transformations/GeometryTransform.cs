using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

namespace NetTopologySuite.CoordinateSystems.Transformations
{
    /// <summary>
    /// Helper class for transforming <see cref="IGeometry" /> objects.
    /// </summary>
    public class GeometryTransform
    {
        private static double[] ToArray(double x, double y)
        {
            return new[] { x, y };
        }

        /// <summary>
        /// Transforms a <see cref="Envelope" /> object.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static Envelope TransformBox(Envelope box, IMathTransform transform)
        {
            if (box == null) return null;

            double[][] corners = new double[4][];
            corners[0] = transform.Transform(ToArray(box.MinX, box.MinY)); //LL
            corners[1] = transform.Transform(ToArray(box.MaxX, box.MaxY)); //UR
            corners[2] = transform.Transform(ToArray(box.MinX, box.MaxY)); //UL
            corners[3] = transform.Transform(ToArray(box.MaxX, box.MinY)); //LR

            var result = new Envelope();
            foreach (double[] p in corners)
                result.ExpandToInclude(p[0], p[1]);
            return result;
        }

        /// <summary>
        /// Transforms a <see cref="IGeometry" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="g"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IGeometry TransformGeometry(IGeometryFactory factory,
            IGeometry g, IMathTransform transform)
        {
            if (g == null)
                return null;
            if (g is IPoint)
                return TransformPoint(factory, (IPoint) g, transform);
            if (g is ILineString)
                return TransformLineString(factory, (ILineString) g, transform);
            if (g is IPolygon)
                return TransformPolygon(factory, (IPolygon) g, transform);
            if (g is IMultiPoint)
                return TransformMultiPoint(factory, (IMultiPoint) g, transform);
            if (g is IMultiLineString)
                return TransformMultiLineString(factory, (IMultiLineString) g, transform);
            if (g is IMultiPolygon)
                return TransformMultiPolygon(factory, (IMultiPolygon) g, transform);
            if (g is IGeometryCollection)
                return TransformGeometryCollection(factory, (IGeometryCollection) g, transform);
            throw new ArgumentException(string.Format(
                "Could not transform geometry type '{0}'", g.GetType()));
        }

        /// <summary>
        /// Transforms a <see cref="IPoint" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="p"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IPoint TransformPoint(IGeometryFactory factory,
            IPoint p, IMathTransform transform)
        {
            try
            {
                var transformed = transform.Transform(p.CoordinateSequence);
                return factory.CreatePoint(transformed);
            }
            catch { return null; }
        }

        /// <summary>
        /// Transforms a <see cref="ILineString" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="l"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static ILineString TransformLineString(IGeometryFactory factory,
            ILineString l, IMathTransform transform)
        {
            try
            {
                var coordSequence = transform.Transform(l.CoordinateSequence);
                return factory.CreateLineString(coordSequence);
            }
            catch { return null; }
        }

        /// <summary>
        /// Transforms a <see cref="ILinearRing" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="r"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static ILinearRing TransformLinearRing(IGeometryFactory factory,
            ILinearRing r, IMathTransform transform)
        {
            try
            {
                var coordSequence = transform.Transform(r.CoordinateSequence);
                return factory.CreateLinearRing(coordSequence);
            }
            catch { return null; }
        }

        /// <summary>
        /// Transforms a <see cref="IPolygon" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="p"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IPolygon TransformPolygon(IGeometryFactory factory,
            IPolygon p, IMathTransform transform)
        {
            var holes = new List<ILinearRing>(p.InteriorRings.Length);
            for (int i = 0; i < p.InteriorRings.Length; i++)
            {
                var hole = TransformLinearRing(factory,
                    (ILinearRing) p.InteriorRings[i], transform);
                holes.Add(hole);
            }
            var shell = TransformLinearRing(factory,
                (ILinearRing) p.ExteriorRing, transform);
            return factory.CreatePolygon(shell, holes.ToArray());
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPoint" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="points"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IMultiPoint TransformMultiPoint(IGeometryFactory factory,
            IMultiPoint points, IMathTransform transform)
        {
            //We assume the first point holds all the ordinates
            var firstPoint = (IPoint) points.GetGeometryN(0);
            var ordinateFlags = firstPoint.CoordinateSequence.Ordinates;
            var ordinates = OrdinatesUtility.ToOrdinateArray(ordinateFlags);
            var coordSequence = factory.CoordinateSequenceFactory.Create(points.NumPoints, ordinateFlags);

            for (int i = 0; i < points.NumGeometries; i++)
            {
                var currPoint = (IPoint) points.GetGeometryN(i);
                var seq = currPoint.CoordinateSequence;
                foreach (var ordinate in ordinates)
                {
                    double d = seq.GetOrdinate(0, ordinate);
                    coordSequence.SetOrdinate(i, ordinate, d);
                }
            }
            var transPoints = transform.Transform(coordSequence);
            return factory.CreateMultiPoint(transPoints);
        }

        /// <summary>
        /// Transforms a <see cref="IMultiLineString" /> object.
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="lines"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static IMultiLineString TransformMultiLineString(IGeometryFactory factory,
            IMultiLineString lines, IMathTransform transform)
        {
            var geometries = lines.Geometries;
            var strings = new List<ILineString>(geometries.Length);
            foreach (var ls in lines.Geometries)
            {
                var item = TransformLineString(factory, (ILineString)ls, transform);
                strings.Add(item);
            }
            return factory.CreateMultiLineString(strings.ToArray());
        }

        /// <summary>
        /// Transforms a <see cref="IMultiPolygon" /> object.
        /// </summary>
        /// <param name="factory">The factory to create the new <see cref="IMultiPolygon"/></param>
        /// <param name="polys">The input <see cref="IMultiPolygon"/></param>
        /// <param name="transform">The <see cref="IMathTransform"/></param>
        /// <returns>A transformed <see cref="IMultiPolygon"/></returns>
        public static IMultiPolygon TransformMultiPolygon(IGeometryFactory factory,
            IMultiPolygon polys, IMathTransform transform)
        {
            var geometries = polys.Geometries;
            var polygons = new List<IPolygon>(geometries.Length);
            foreach (var p in geometries)
            {
                var item = TransformPolygon(factory, (IPolygon)p, transform);
                polygons.Add(item);
            }
            return factory.CreateMultiPolygon(polygons.ToArray());
        }

        /// <summary>
        /// Transforms a <see cref="IGeometryCollection" /> object.
        /// </summary>
        /// <param name="factory">The factory to create the new <see cref="IGeometryCollection"/></param>
        /// <param name="geoms">The input <see cref="IGeometryCollection"/></param>
        /// <param name="transform">The <see cref="IMathTransform"/></param>
        /// <returns>A transformed <see cref="IGeometryCollection"/></returns>
        public static IGeometryCollection TransformGeometryCollection(IGeometryFactory factory,
            IGeometryCollection geoms, IMathTransform transform)
        {
            var geometries = geoms.Geometries;
            var coll = new List<IGeometry>(geometries.Length);
            foreach (var g in geometries)
            {
                var item = TransformGeometry(factory, g, transform);
                coll.Add(item);
            }
            return factory.CreateGeometryCollection(coll.ToArray());
        }
    }
}
