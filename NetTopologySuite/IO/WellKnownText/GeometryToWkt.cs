// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
//
// This file is part of GeoAPI.Net.
// GeoAPI.Net is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// GeoAPI.Net is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeoAPI.Net; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

// SOURCECODE IS MODIFIED FROM ANOTHER WORK AND IS ORIGINALLY BASED ON GeoTools.NET:
/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;
using NPack;

namespace GeoAPI.IO.WellKnownText
{
    /// <summary>
    /// Outputs the textual representation of a <see cref="GeoAPI.Geometries.IGeometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>The Well-Known Text (WKT) representation of Geometry is designed to exchange geometry data in ASCII form.</para>
    /// Examples of WKT representations of geometry objects are:
    /// <list type="table">
    /// <listheader><term>Geometry </term><description>WKT Representation</description></listheader>
    /// <item><term>A Point</term>
    /// <description>POINT(15 20)<br/> Note that point coordinates are specified with no separating comma.</description></item>
    /// <item><term>A LineString with four points:</term>
    /// <description>LINESTRING(0 0, 10 10, 20 25, 50 60)</description></item>
    /// <item><term>A Polygon with one exterior ring and one interior ring:</term>
    /// <description>POLYGON((0 0,10 0,10 10,0 10,0 0),(5 5,7 5,7 7,5 7, 5 5))</description></item>
    /// <item><term>A MultiPoint with three Point values:</term>
    /// <description>MULTIPOINT(0 0, 20 20, 60 60)</description></item>
    /// <item><term>A MultiLineString with two LineString values:</term>
    /// <description>MULTILINESTRING((10 10, 20 20), (15 15, 30 15))</description></item>
    /// <item><term>A MultiPolygon with two Polygon values:</term>
    /// <description>MULTIPOLYGON(((0 0,10 0,10 10,0 10,0 0)),((5 5,7 5,7 7,5 7, 5 5)))</description></item>
    /// <item><term>A GeometryCollection consisting of two Point values and one LineString:</term>
    /// <description>GEOMETRYCOLLECTION(POINT(10 10), POINT(30 30), LINESTRING(15 15, 20 20))</description></item>
    /// </list>
    /// </remarks>
    internal static class GeometryToWkt
    {
        #region Methods

        /// <summary>
        /// Converts a Geometry to its Well-Known Text representation.
        /// </summary>
        /// <param name="geometry">A Geometry to write.</param>
        /// <returns>A &lt;Geometry Tagged Text&gt; String (see the OpenGIS Simple
        ///  Features Specification)</returns>
        public static String Write<TCoordinate>(IGeometry<TCoordinate> geometry)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            StringWriter sw = new StringWriter();
            Write(geometry, sw);
            return sw.ToString();
        }

        /// <summary>
        /// Converts a Geometry to its Well-Known Text representation.
        /// </summary>
        /// <param name="geometry">A geometry to process.</param>
        /// <param name="writer">Stream to write out the geometry's text representation.</param>
        /// <remarks>
        /// Geometry is written to the output stream as &lt;Gemoetry Tagged Text&gt; String (see the OpenGIS
        /// Simple Features Specification).
        /// </remarks>
        public static void Write<TCoordinate>(IGeometry<TCoordinate> geometry, 
                                              TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            appendGeometryTaggedText(geometry, writer);
        }

        /// <summary>
        /// Converts a Geometry to &lt;Geometry Tagged Text &gt; format, then appends it to the writer.
        /// </summary>
        /// <param name="geometry">The Geometry to process.</param>
        /// <param name="writer">The output stream to append to.</param>
        private static void appendGeometryTaggedText<TCoordinate>(IGeometry<TCoordinate> geometry, 
                                                                  TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (geometry == null) throw new ArgumentNullException("geometry");

            if (geometry is IPoint<TCoordinate>)
            {
                IPoint<TCoordinate> point = geometry as IPoint<TCoordinate>;
                appendPointTaggedText(point, writer);
            }
            else if (geometry is ILineString<TCoordinate>)
            {
                appendLineStringTaggedText(geometry as ILineString<TCoordinate>, writer);
            }
            else if (geometry is IPolygon<TCoordinate>)
            {
                appendPolygonTaggedText(geometry as IPolygon<TCoordinate>, writer);
            }
            else if (geometry is IMultiPoint<TCoordinate>)
            {
                appendMultiPointTaggedText(geometry as IMultiPoint<TCoordinate>, writer);
            }
            else if (geometry is IMultiLineString<TCoordinate>)
            {
                appendMultiLineStringTaggedText(geometry as IMultiLineString<TCoordinate>, writer);
            }
            else if (geometry is IMultiPolygon<TCoordinate>)
            {
                appendMultiPolygonTaggedText(geometry as IMultiPolygon<TCoordinate>, writer);
            }
            else if (geometry is IGeometryCollection<TCoordinate>)
            {
                appendGeometryCollectionTaggedText(geometry as IGeometryCollection<TCoordinate>, writer);
            }
            else
            {
                throw new NotSupportedException("Unsupported Geometry implementation:" + geometry.GetType().Name);
            }
        }

        /// <summary>
        /// Converts a Coordinate to &lt;Point Tagged Text&gt; format,
        /// then appends it to the writer.
        /// </summary>
        /// <param name="coordinate">the <code>Coordinate</code> to process</param>
        /// <param name="writer">the output writer to append to</param>
        private static void appendPointTaggedText<TCoordinate>(IPoint<TCoordinate> coordinate, 
                                                               TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("POINT ");
            appendPointText(coordinate, writer);
        }

        /// <summary>
        /// Converts a LineString to LineString tagged text format, 
        /// </summary>
        /// <param name="lineString">The LineString to process.</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendLineStringTaggedText<TCoordinate>(ILineString<TCoordinate> lineString,
                                                                    TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("LINESTRING ");
            appendLineStringText(lineString, writer);
        }

        /// <summary>
        ///  Converts a Polygon to &lt;Polygon Tagged Text&gt; format,
        ///  then appends it to the writer.
        /// </summary>
        /// <param name="polygon">Th Polygon to process.</param>
        /// <param name="writer">The stream writer to append to.</param>
        private static void appendPolygonTaggedText<TCoordinate>(IPolygon<TCoordinate> polygon,
                                                                 TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("POLYGON ");
            appendPolygonText(polygon, writer);
        }

        /// <summary>
        /// Converts a MultiPoint to &lt;MultiPoint Tagged Text&gt;
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multipoint">The MultiPoint to process.</param>
        /// <param name="writer">The output writer to append to.</param>
        private static void appendMultiPointTaggedText<TCoordinate>(IMultiPoint<TCoordinate> multipoint,
                                                                    TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("MULTIPOINT ");
            appendMultiPointText(multipoint, writer);
        }

        /// <summary>
        /// Converts a MultiLineString to &lt;MultiLineString Tagged
        /// Text&gt; format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The MultiLineString to process</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendMultiLineStringTaggedText<TCoordinate>(IMultiLineString<TCoordinate> multiLineString,
                                                                         TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("MULTILINESTRING ");
            appendMultiLineStringText(multiLineString, writer);
        }

        /// <summary>
        /// Converts a MultiPolygon to &lt;MultiPolygon Tagged
        /// Text&gt; format, then appends it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon to process</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendMultiPolygonTaggedText<TCoordinate>(IMultiPolygon<TCoordinate> multiPolygon,
                                                                      TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("MULTIPOLYGON ");
            appendMultiPolygonText(multiPolygon, writer);
        }

        /// <summary>
        /// Converts a GeometryCollection to &lt;GeometryCollection Tagged
        /// Text&gt; format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The GeometryCollection to process</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendGeometryCollectionTaggedText<TCoordinate>(IGeometryCollection<TCoordinate> geometryCollection,
                                                                            TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            writer.Write("GEOMETRYCOLLECTION ");
            appendGeometryCollectionText(geometryCollection, writer);
        }


        /// <summary>
        /// Converts a Coordinate to Point Text format then appends it to the writer.
        /// </summary>
        /// <param name="point">The point to process.</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendPointText<TCoordinate>(IPoint<TCoordinate> point,
                                                         TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (point == null || point.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");
                appendCoordinate(point.Coordinate, writer);
                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a coordinate to &lt;Point&gt; format, then appends
        /// it to the writer. 
        /// </summary>
        /// <param name="coordinate">The Coordinate to process.</param>
        /// <param name="writer">The output writer to append to.</param>
        private static void appendCoordinate<TCoordinate>(TCoordinate coordinate, 
                                                          TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Int32 componentCount = coordinate.ComponentCount;

            for (UInt32 i = 0; i < componentCount; i++)
            {
                writer.Write(writeNumber((Double)coordinate[(Int32)i]));
                writer.Write((i < componentCount - 1 ? " " : ""));
            }
        }

        /// <summary>
        /// Converts a Double to a String, not in scientific notation.
        /// </summary>
        /// <param name="d">The Double to convert.</param>
        /// <returns>The <see cref="Double"/> value as a string, not in scientific notation.</returns>
        private static String writeNumber(Double d)
        {
            return d.ToString(WktTokenizer.NumberFormat_enUS);
        }

        /// <summary>
        /// Converts a LineString to &lt;LineString Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="lineString">The LineString to process.</param>
        /// <param name="writer">The output stream to append to.</param>
        private static void appendLineStringText<TCoordinate>(ILineString<TCoordinate> lineString,
                                                              TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (lineString == null || lineString.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");

                Boolean wroteFirst = false;

                foreach (TCoordinate coordinate in lineString.Coordinates)
                {
                    if (wroteFirst)
                    {
                        writer.Write(", ");
                    }
                    else
                    {
                        wroteFirst = true;
                    }

                    appendCoordinate(coordinate, writer);
                }

                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a Polygon to &lt;Polygon Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="polygon">The Polygon to process.</param>
        /// <param name="writer"></param>
        private static void appendPolygonText<TCoordinate>(IPolygon<TCoordinate> polygon, 
                                                           TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (polygon == null || polygon.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");
                appendLineStringText(polygon.ExteriorRing, writer);

                foreach (ILinearRing<TCoordinate> interiorRing in polygon.InteriorRings)
                {
                    writer.Write(", ");
                    appendLineStringText(interiorRing, writer);
                }

                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiPoint to &lt;MultiPoint Text&gt; format, then
        /// Appends it to the writer.
        /// </summary>
        /// <param name="multiPoint">The MultiPoint to process.</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendMultiPointText<TCoordinate>(IMultiPoint<TCoordinate> multiPoint, 
                                                              TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (multiPoint == null || multiPoint.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");

                for (Int32 i = 0; i < multiPoint.Count; i++)
                {
                    if (i > 0) writer.Write(", ");

                    appendPointText(multiPoint[i], writer);
                }

                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiLineString to &lt;MultiLineString Text&gt;
        /// format, then appends it to the writer.
        /// </summary>
        /// <param name="multiLineString">The MultiLineString to process.</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendMultiLineStringText<TCoordinate>(IMultiLineString<TCoordinate> multiLineString, 
                                                                   TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (multiLineString == null || multiLineString.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");

                for (Int32 i = 0; i < multiLineString.Count; i++)
                {
                    if (i > 0) writer.Write(", ");

                    appendLineStringText(multiLineString[i], writer);
                }

                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a MultiPolygon to &lt;MultiPolygon Text&gt; format, then Appends to it to the writer.
        /// </summary>
        /// <param name="multiPolygon">The MultiPolygon to process.</param>
        /// <param name="writer">The output stream to append to.</param>
        private static void appendMultiPolygonText<TCoordinate>(
                                    IMultiPolygon<TCoordinate> multiPolygon,
                                    TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (multiPolygon == null || multiPolygon.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");

                for (Int32 i = 0; i < multiPolygon.Count; i++)
                {
                    if (i > 0) writer.Write(", ");

                    appendPolygonText(multiPolygon[i], writer);
                }

                writer.Write(")");
            }
        }

        /// <summary>
        /// Converts a GeometryCollection to &lt;GeometryCollection Text &gt; format, then appends it to the writer.
        /// </summary>
        /// <param name="geometryCollection">The GeometryCollection to process.</param>
        /// <param name="writer">The output stream writer to append to.</param>
        private static void appendGeometryCollectionText<TCoordinate>(
                                        IGeometryCollection<TCoordinate> geometryCollection,
                                        TextWriter writer)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            if (geometryCollection == null || geometryCollection.IsEmpty)
            {
                writer.Write("EMPTY");
            }
            else
            {
                writer.Write("(");

                for (Int32 i = 0; i < geometryCollection.Count; i++)
                {
                    if (i > 0) writer.Write(", ");

                    appendGeometryTaggedText(geometryCollection[i], writer);
                }

                writer.Write(")");
            }
        }

        #endregion
    }
}