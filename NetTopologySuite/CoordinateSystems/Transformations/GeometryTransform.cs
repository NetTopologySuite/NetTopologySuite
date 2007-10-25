using System;
using System.Collections.Generic;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
	/// <summary>
	/// Helper class for transforming <see cref="Geometry" /> objects.
	/// </summary>
	public class GeometryTransform
	{
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
		private static IPoint ToNTS(double x, double y)
        {
            return new Point(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double[] ToArray(double x, double y)
        {
            return new double[] { x, y, };
        }        

		/// <summary>
		/// Transforms a <see cref="IEnvelope" /> object.
		/// </summary>
        /// <param name="box"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IEnvelope TransformBox(IEnvelope box, IMathTransform transform)
		{
			if (box == null) return null;

            double[][] corners = new double[4][];
            corners[0] = transform.Transform(ToArray(box.MinX, box.MinY)); //LL
            corners[1] = transform.Transform(ToArray(box.MaxX, box.MaxY)); //UR
            corners[2] = transform.Transform(ToArray(box.MinX, box.MaxY)); //UL
            corners[3] = transform.Transform(ToArray(box.MaxX, box.MinY)); //LR

			IEnvelope result = new Envelope();
            foreach (double[] p in corners)
				result.ExpandToInclude(p[0], p[1]);
			return result;
		}

		/// <summary>
		/// Transforms a <see cref="Geometry" /> object.
		/// </summary>
		/// <param name="g"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IGeometry TransformGeometry(IGeometry g, IMathTransform transform)
		{
			if (g == null) return null;

			else if (g is IPoint)
				return TransformPoint(g as IPoint, transform);
			else if (g is ILineString)
				return TransformLineString(g as ILineString, transform);
			else if (g is IPolygon)
				return TransformPolygon(g as IPolygon, transform);
			else if (g is IMultiPoint)
				return TransformMultiPoint(g as IMultiPoint, transform);
			else if (g is IMultiLineString)
				return TransformMultiLineString(g as IMultiLineString, transform);
			else if (g is IMultiPolygon)
				return TransformMultiPolygon(g as IMultiPolygon, transform);
			else throw new ArgumentException("Could not transform geometry type '" + g.GetType().ToString() +"'");
		}

		/// <summary>
		/// Transforms a <see cref="Point" /> object.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IPoint TransformPoint(IPoint p, IMathTransform transform)
		{
			try 
            { 
                double[] point = transform.Transform(ToArray(p.X, p.Y));
                return ToNTS(point[0], point[1]);
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LineString" /> object.
		/// </summary>
		/// <param name="l"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static ILineString TransformLineString(ILineString l, IMathTransform transform)
		{
			try 
            {
				List<ICoordinate> coords = ExtractCoordinates(l, transform);
				return new LineString(coords.ToArray()); 
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LinearRing" /> object.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static ILinearRing TransformLinearRing(ILinearRing r, IMathTransform transform)
		{
			try 
            {
                List<ICoordinate> coords = ExtractCoordinates(r, transform);
                return new LinearRing(coords.ToArray()); 
            }
			catch { return null; }
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ls"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static List<ICoordinate> ExtractCoordinates(ILineString ls, IMathTransform transform)
        {
            List<double[]> points = new List<double[]>(ls.NumPoints);
            foreach (ICoordinate c in ls.Coordinates)
                points.Add(ToArray(c.X, c.Y));
            points = transform.TransformList(points);
            List<ICoordinate> coords = new List<ICoordinate>(points.Count);
            foreach (double[] p in points)
                coords.Add(new Coordinate(p[0], p[1]));
            return coords;
        }

		/// <summary>
		/// Transforms a <see cref="Polygon" /> object.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IPolygon TransformPolygon(IPolygon p, IMathTransform transform)
		{
			List<ILinearRing> rings = new List<ILinearRing>(p.InteriorRings.Length); 
            for (int i = 0; i < p.InteriorRings.Length; i++)
				rings.Add(TransformLinearRing((ILinearRing)p.InteriorRings[i], transform));
			return new Polygon(TransformLinearRing((ILinearRing)p.ExteriorRing, transform), rings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPoint" /> object.
		/// </summary>
		/// <param name="points"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IMultiPoint TransformMultiPoint(IMultiPoint points, IMathTransform transform)
		{
            List<double[]> pointList = new List<double[]>(points.Geometries.Length);
			foreach (IPoint p in points.Geometries)
                pointList.Add(ToArray(p.X, p.Y));
			pointList = transform.TransformList(pointList);
			IPoint[] array = new IPoint[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
                array[i] = ToNTS(pointList[i][0], pointList[i][1]);
			return new MultiPoint(array);
		}

		/// <summary>
		/// Transforms a <see cref="MultiLineString" /> object.
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IMultiLineString TransformMultiLineString(IMultiLineString lines, IMathTransform transform)
		{
			List<ILineString> strings = new List<ILineString>(lines.Geometries.Length);
			foreach (ILineString ls in lines.Geometries)
				strings.Add(TransformLineString(ls, transform));
            return new MultiLineString(strings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPolygon" /> object.
		/// </summary>
		/// <param name="polys"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IMultiPolygon TransformMultiPolygon(IMultiPolygon polys, IMathTransform transform)
		{
			List<IPolygon> polygons = new List<IPolygon>(polys.Geometries.Length);
			foreach (IPolygon p in polys.Geometries)
				polygons.Add(TransformPolygon(p, transform));
			return new MultiPolygon(polygons.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="GeometryCollection" /> object.
		/// </summary>
		/// <param name="geoms"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
		public static IGeometryCollection TransformGeometryCollection(GeometryCollection geoms, IMathTransform transform)
		{
			List<IGeometry> coll = new List<IGeometry>(geoms.Geometries.Length);
			foreach (IGeometry g in geoms.Geometries)
				coll.Add(TransformGeometry(g, transform));
			return new GeometryCollection(coll.ToArray());
		}
	}
}
