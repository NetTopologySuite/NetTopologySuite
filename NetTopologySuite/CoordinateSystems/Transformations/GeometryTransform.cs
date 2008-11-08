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
        /// <param name="factory"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
		private static IPoint ToNTS(IGeometryFactory factory, double x, double y)
        {
            return factory.CreatePoint(new Coordinate(x, y));
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
	    /// 
	    /// </summary>
	    /// <param name="ls"></param>
	    /// <param name="transform"></param>
	    /// <returns></returns>
	    private static ICoordinate[] ExtractCoordinates(IGeometry ls, IMathTransform transform)
	    {
	        List<double[]> points = new List<double[]>(ls.NumPoints);
	        foreach (ICoordinate c in ls.Coordinates)
	            points.Add(ToArray(c.X, c.Y));
	        points = transform.TransformList(points);
	        List<ICoordinate> coords = new List<ICoordinate>(points.Count);
	        foreach (double[] p in points)
	            coords.Add(new Coordinate(p[0], p[1]));
	        return coords.ToArray();
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
		        return TransformPoint(factory, g as IPoint, transform);
		    if (g is ILineString)
                return TransformLineString(factory, g as ILineString, transform);
		    if (g is IPolygon)
                return TransformPolygon(factory, g as IPolygon, transform);
		    if (g is IMultiPoint)
                return TransformMultiPoint(factory, g as IMultiPoint, transform);
		    if (g is IMultiLineString)
                return TransformMultiLineString(factory, g as IMultiLineString, transform);
		    if (g is IMultiPolygon)
                return TransformMultiPolygon(factory, g as IMultiPolygon, transform);
		    throw new ArgumentException(String.Format(
                "Could not transform geometry type '{0}'", g.GetType()));
		}

	    /// <summary>
		/// Transforms a <see cref="Point" /> object.
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
                double[] point = transform.Transform(ToArray(p.X, p.Y));
                return ToNTS(factory, point[0], point[1]);
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LineString" /> object.
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
				ICoordinate[] coords = ExtractCoordinates(l, transform);
                return factory.CreateLineString(coords); 
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LinearRing" /> object.
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
                ICoordinate[] coords = ExtractCoordinates(r, transform);
                return factory.CreateLinearRing(coords);
            }
			catch { return null; }
		}

	    /// <summary>
		/// Transforms a <see cref="Polygon" /> object.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="p"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
        public static IPolygon TransformPolygon(IGeometryFactory factory,
            IPolygon p, IMathTransform transform)
		{
			List<ILinearRing> holes = new List<ILinearRing>(p.InteriorRings.Length); 
            for (int i = 0; i < p.InteriorRings.Length; i++)
            {
                ILinearRing hole = TransformLinearRing(factory, 
                    (ILinearRing) p.InteriorRings[i], transform);
                holes.Add(hole);
            }
	        ILinearRing shell = TransformLinearRing(factory, 
                (ILinearRing) p.ExteriorRing, transform);
	        return factory.CreatePolygon(shell, holes.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPoint" /> object.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="points"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
        public static IMultiPoint TransformMultiPoint(IGeometryFactory factory, 
            IMultiPoint points, IMathTransform transform)
		{
            List<double[]> pointList = new List<double[]>(points.Geometries.Length);
			foreach (IPoint p in points.Geometries)
                pointList.Add(ToArray(p.X, p.Y));
			pointList = transform.TransformList(pointList);
			IPoint[] array = new IPoint[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
                array[i] = ToNTS(factory, pointList[i][0], pointList[i][1]);
		    return factory.CreateMultiPoint(array);
		}

		/// <summary>
		/// Transforms a <see cref="MultiLineString" /> object.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="lines"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
        public static IMultiLineString TransformMultiLineString(IGeometryFactory factory,
            IMultiLineString lines, IMathTransform transform)
		{
			List<ILineString> strings = new List<ILineString>(lines.Geometries.Length);
			foreach (ILineString ls in lines.Geometries)
			{
			    ILineString item = TransformLineString(factory, ls, transform);
			    strings.Add(item);
			}
		    return factory.CreateMultiLineString(strings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPolygon" /> object.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="polys"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
        public static IMultiPolygon TransformMultiPolygon(IGeometryFactory factory,
            IMultiPolygon polys, IMathTransform transform)
		{
			List<IPolygon> polygons = new List<IPolygon>(polys.Geometries.Length);
			foreach (IPolygon p in polys.Geometries)
			{
			    IPolygon item = TransformPolygon(factory, p, transform);
			    polygons.Add(item);
			}
		    return factory.CreateMultiPolygon(polygons.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="GeometryCollection" /> object.
		/// </summary>
		/// <param name="factory"></param>
		/// <param name="geoms"></param>
		/// <param name="transform"></param>
		/// <returns></returns>
        public static IGeometryCollection TransformGeometryCollection(IGeometryFactory factory, 
            GeometryCollection geoms, IMathTransform transform)
		{
			List<IGeometry> coll = new List<IGeometry>(geoms.Geometries.Length);
			foreach (IGeometry g in geoms.Geometries)
			{
			    IGeometry item = TransformGeometry(factory, g, transform);
			    coll.Add(item);
			}
		    return factory.CreateGeometryCollection(coll.ToArray());
		}
	}
}
