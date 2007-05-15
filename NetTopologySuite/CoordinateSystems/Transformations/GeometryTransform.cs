// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using SharpMap.CoordinateSystems.Transformations;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.CoordinateSystems.Transformations
{
	/// <summary>
	/// Helper class for transforming <see cref="Geometry"/>
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
        private static double[] ToLightStruct(double x, double y)
        {
            return new double[] { x, y, };
        }        

		/// <summary>
		/// Transforms a <see cref="Envelope"/>.
		/// </summary>
		/// <param name="box">BoundingBox to transform</param>
		/// <param name="transform">Math Transform</param>
		/// <returns>Transformed object</returns>
		public static IEnvelope TransformBox(IEnvelope box, IMathTransform transform)
		{
			if (box == null)
				return null;
            double[][] corners = new double[4][];
            corners[0] = transform.Transform(ToLightStruct(box.MinX, box.MinY)); //LL
            corners[1] = transform.Transform(ToLightStruct(box.MaxX, box.MaxY)); //UR
            corners[2] = transform.Transform(ToLightStruct(box.MinX, box.MaxY)); //UL
            corners[3] = transform.Transform(ToLightStruct(box.MaxX, box.MinY)); //LR

			IEnvelope result = new Envelope();
            foreach (double[] p in corners)
				result.ExpandToInclude(p[0], p[1]);
			return result;
		}

		/// <summary>
		/// Transforms a <see cref="Geometry"/>.
		/// </summary>
		/// <param name="g">Geometry to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Geometry</returns>
		public static IGeometry TransformGeometry(IGeometry g, IMathTransform transform)
		{
			if (g == null)
				return null;
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
		/// Transforms a <see cref="Point"/>.
		/// </summary>
		/// <param name="p">Point to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Point</returns>
		public static IPoint TransformPoint(IPoint p, IMathTransform transform)
		{
			try 
            { 
                double[] point = transform.Transform(ToLightStruct(p.X, p.Y));
                return ToNTS(point[0], point[1]);
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LineString"/>.
		/// </summary>
		/// <param name="l">LineString to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed LineString</returns>
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
		/// Transforms a <see cref="LinearRing"/>.
		/// </summary>
		/// <param name="r">LinearRing to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed LinearRing</returns>
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
            List<double[]> points =
                new List<double[]>(ls.NumPoints);
            foreach (ICoordinate c in ls.Coordinates)
                points.Add(ToLightStruct(c.X, c.Y));
            points = transform.TransformList(points);
            List<ICoordinate> coords = new List<ICoordinate>(points.Count);
            foreach (double[] p in points)
                coords.Add(new Coordinate(p[0], p[1]));
            return coords;
        }

		/// <summary>
		/// Transforms a <see cref="Polygon"/>.
		/// </summary>
		/// <param name="p">Polygon to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Polygon</returns>
		public static IPolygon TransformPolygon(IPolygon p, IMathTransform transform)
		{
			List<ILinearRing> rings = new List<ILinearRing>(p.InteriorRings.Length); 
            for (int i = 0; i < p.InteriorRings.Length; i++)
				rings.Add(TransformLinearRing((ILinearRing)p.InteriorRings[i], transform));
			return new Polygon(TransformLinearRing((ILinearRing)p.ExteriorRing, transform), rings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPoint"/>.
		/// </summary>
		/// <param name="points">MultiPoint to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiPoint</returns>
		public static IMultiPoint TransformMultiPoint(IMultiPoint points, IMathTransform transform)
		{
            List<double[]> pointList = new List<double[]>(points.Geometries.Length);
			foreach (IPoint p in points.Geometries)
                pointList.Add(ToLightStruct(p.X, p.Y));
			pointList = transform.TransformList(pointList);
			IPoint[] array = new IPoint[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
                array[i] = ToNTS(pointList[i][0], pointList[i][1]);
			return new MultiPoint(array);
		}

		/// <summary>
		/// Transforms a <see cref="MultiLineString"/>.
		/// </summary>
		/// <param name="lines">MultiLineString to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiLineString</returns>
		public static IMultiLineString TransformMultiLineString(IMultiLineString lines, IMathTransform transform)
		{
			List<ILineString> strings = new List<ILineString>(lines.Geometries.Length);
			foreach (ILineString ls in lines.Geometries)
				strings.Add(TransformLineString(ls, transform));
            return new MultiLineString(strings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPolygon"/>.
		/// </summary>
		/// <param name="polys">MultiPolygon to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiPolygon</returns>
		public static IMultiPolygon TransformMultiPolygon(IMultiPolygon polys, IMathTransform transform)
		{
			List<IPolygon> polygons = new List<IPolygon>(polys.Geometries.Length);
			foreach (IPolygon p in polys.Geometries)
				polygons.Add(TransformPolygon(p, transform));
			return new MultiPolygon(polygons.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="GeometryCollection"/>.
		/// </summary>
		/// <param name="geoms">GeometryCollection to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed GeometryCollection</returns>
		public static IGeometryCollection TransformGeometryCollection(GeometryCollection geoms, IMathTransform transform)
		{
			List<IGeometry> coll = new List<IGeometry>(geoms.Geometries.Length);
			foreach (IGeometry g in geoms.Geometries)
				coll.Add(TransformGeometry(g, transform));
			return new GeometryCollection(coll.ToArray());
		}
	}
}
