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
        private static Point ToNTS(double x, double y)
        {
            return new Point(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point ToLightStruct(double x, double y)
        {
            return new GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point(x, y);
        }        

		/// <summary>
		/// Transforms a <see cref="Envelope"/>.
		/// </summary>
		/// <param name="box">BoundingBox to transform</param>
		/// <param name="transform">Math Transform</param>
		/// <returns>Transformed object</returns>
        public static Envelope TransformBox(Envelope box, IMathTransform transform)
		{
			if (box == null)
				return null;
            GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point[] corners = 
                new GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point[4];
            corners[0] = transform.Transform(ToLightStruct(box.MinX, box.MinY)); //LL
            corners[1] = transform.Transform(ToLightStruct(box.MaxX, box.MaxY)); //UR
            corners[2] = transform.Transform(ToLightStruct(box.MinX, box.MaxY)); //UL
            corners[3] = transform.Transform(ToLightStruct(box.MaxX, box.MinY)); //LR

			Envelope result = new Envelope();
            foreach (GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point p in corners)
				result.ExpandToInclude(p.X, p.Y);
			return result;
		}

		/// <summary>
		/// Transforms a <see cref="Geometry"/>.
		/// </summary>
		/// <param name="g">Geometry to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Geometry</returns>
		public static Geometry TransformGeometry(Geometry g, IMathTransform transform)
		{
			if (g == null)
				return null;
			else if (g is Point)
				return TransformPoint(g as Point, transform);
			else if (g is LineString)
				return TransformLineString(g as LineString, transform);
			else if (g is Polygon)
				return TransformPolygon(g as Polygon, transform);
			else if (g is MultiPoint)
				return TransformMultiPoint(g as MultiPoint, transform);
			else if (g is MultiLineString)
				return TransformMultiLineString(g as MultiLineString, transform);
			else if (g is MultiPolygon)
				return TransformMultiPolygon(g as MultiPolygon, transform);
			else throw new ArgumentException("Could not transform geometry type '" + g.GetType().ToString() +"'");
		}

		/// <summary>
		/// Transforms a <see cref="Point"/>.
		/// </summary>
		/// <param name="p">Point to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Point</returns>
		public static Point TransformPoint(Point p, IMathTransform transform)
		{
			try 
            { 
                GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point point = 
                    transform.Transform(ToLightStruct(p.X, p.Y));
                return ToNTS(point.X, point.Y);
            }
			catch { return null; }
		}

		/// <summary>
		/// Transforms a <see cref="LineString"/>.
		/// </summary>
		/// <param name="l">LineString to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed LineString</returns>
		public static LineString TransformLineString(LineString l, IMathTransform transform)
		{
			try 
            {
                List<Coordinate> coords = ExtractCoordinates(l, transform);
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
		public static LinearRing TransformLinearRing(LinearRing r, IMathTransform transform)
		{
			try 
            {
                List<Coordinate> coords = ExtractCoordinates(r, transform);
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
        private static List<Coordinate> ExtractCoordinates(LineString ls, IMathTransform transform)
        {
            List<GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point> points =
                new List<GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point>(ls.Count);
            foreach (Coordinate c in ls.Coordinates)
                points.Add(ToLightStruct(c.X, c.Y));
            points = transform.TransformList(points);
            List<Coordinate> coords = new List<Coordinate>(points.Count);
            foreach (GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point p in points)
                coords.Add(new Coordinate(p.X, p.Y));
            return coords;
        }

		/// <summary>
		/// Transforms a <see cref="Polygon"/>.
		/// </summary>
		/// <param name="p">Polygon to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed Polygon</returns>
		public static Polygon TransformPolygon(Polygon p, IMathTransform transform)
		{
            List<LinearRing> rings  = new List<LinearRing>(p.InteriorRings.Length); 
            for (int i = 0; i < p.InteriorRings.Length; i++)
                rings.Add(TransformLinearRing((LinearRing) p.InteriorRings[i], transform));            
            return new Polygon(TransformLinearRing((LinearRing)p.ExteriorRing, transform), rings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPoint"/>.
		/// </summary>
		/// <param name="points">MultiPoint to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiPoint</returns>
		public static MultiPoint TransformMultiPoint(MultiPoint points, IMathTransform transform)
		{
            List<GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point> pointList =
                new List<GisSharpBlog.NetTopologySuite.Geometries.LightStructs.Point>(points.Geometries.Length);
            foreach (Point p in points.Geometries)
                pointList.Add(ToLightStruct(p.X, p.Y));
			pointList = transform.TransformList(pointList);
            Point[] array = new Point[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
                array[i] = ToNTS(pointList[i].X, pointList[i].Y);
			return new MultiPoint(array);
		}

		/// <summary>
		/// Transforms a <see cref="MultiLineString"/>.
		/// </summary>
		/// <param name="lines">MultiLineString to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiLineString</returns>
		public static MultiLineString TransformMultiLineString(MultiLineString lines, IMathTransform transform)
		{			
            List<LineString> strings = new List<LineString>(lines.Geometries.Length); 
			foreach (LineString ls in lines.Geometries)
				strings.Add(TransformLineString(ls, transform));
            return new MultiLineString(strings.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="MultiPolygon"/>.
		/// </summary>
		/// <param name="polys">MultiPolygon to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed MultiPolygon</returns>
		public static MultiPolygon TransformMultiPolygon(MultiPolygon polys, IMathTransform transform)
		{
            List<Polygon> polygons = new List<Polygon>(polys.Geometries.Length); 
			foreach (Polygon p in polys.Geometries)
				polygons.Add(TransformPolygon(p, transform));
			return new MultiPolygon(polygons.ToArray());
		}

		/// <summary>
		/// Transforms a <see cref="GeometryCollection"/>.
		/// </summary>
		/// <param name="geoms">GeometryCollection to transform</param>
		/// <param name="transform">MathTransform</param>
		/// <returns>Transformed GeometryCollection</returns>
		public static GeometryCollection TransformGeometryCollection(GeometryCollection geoms, IMathTransform transform)
		{
            List<Geometry> coll = new List<Geometry>(geoms.Geometries.Length); 
			foreach (Geometry g in geoms.Geometries)
				coll.Add(TransformGeometry(g, transform));
			return new GeometryCollection(coll.ToArray());
		}
	}
}
