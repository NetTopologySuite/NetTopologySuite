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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GeoAPI.IO.WellKnownText
{
    /// <summary>
    /// Converts a Well-Known Text representation to a <see cref="IGeometry"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Well-Known Text (WKT) representation of Geometry is designed to 
    /// exchange geometry data in ASCII form.
    /// </para>
    /// Examples of WKT representations of geometry objects are:
    /// <list type="table">
    /// <listheader><term>Geometry </term>
    /// <description>WKT Representation</description>
    /// </listheader>
    /// <item><term>A Point</term>
    /// <description>POINT(15 20)<br/> Note that point coordinates are specified 
    /// with no separating comma.</description></item>
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
    internal static class GeometryFromWkt
    {
        /// <summary>
        /// Converts a Well-known text representation to a 
        /// <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="wellKnownText">
        /// A <see cref="IGeometry"/> tagged text String 
        /// (see the OpenGIS Simple Features Specification).
        /// </param>
        /// <returns>
        /// Returns a <see cref="IGeometry"/> specified by wellKnownText.  
        /// Throws an exception if there is a parsing problem.
        /// </returns>
        public static IGeometry<TCoordinate> Parse<TCoordinate>(String wellKnownText,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // throws a parsing exception is there is a problem.
            StringReader reader = new StringReader(wellKnownText);
            return Parse(reader, factory);
        }

        public static IGeometry<TCoordinate> Parse<TCoordinate>(Stream wellKnownText,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            // throws a parsing exception is there is a problem.
            StreamReader reader = new StreamReader(wellKnownText);
            return Parse(reader, factory);
        }

        /// <summary>
        /// Converts a Well-Known Text representation to a 
        /// <see cref="IGeometry"/>.
        /// </summary>
        /// <param name="reader">
        /// A reader which will return a Geometry Tagged Text
        /// String (see the OpenGIS Simple Features Specification).</param>
        /// <returns>
        /// Returns a <see cref="IGeometry"/> read from StreamReader. 
        /// An exception will be thrown if there is a parsing problem.
        /// </returns>
        public static IGeometry<TCoordinate> Parse<TCoordinate>(TextReader reader,
                                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            WktTokenizer tokenizer = new WktTokenizer(reader);

            return readGeometryTaggedText(tokenizer, factory);
        }

        /// <summary>
        /// Returns the next array of Coordinates in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text format. 
        /// The next element returned by the stream should be "(" 
        /// (the beginning of "(x1 y1, x2 y2, ..., xn yn)" or "EMPTY".
        /// </param>
        /// <returns>
        /// The next array of Coordinates in the stream, or an empty array of 
        /// "EMPTY" is the next element returned by the stream.
        /// </returns>
        private static IEnumerable<TCoordinate> getCoordinates<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            List<TCoordinate> coordinates = new List<TCoordinate>();

            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return coordinates;
            }

            do
            {
                TCoordinate coordinate = getCoordinate(tokenizer, false, factory);
                coordinates.Add(coordinate);
                nextToken = getNextCloserOrComma(tokenizer);
            } while (nextToken == ",");

            return coordinates;
        }

        /// <summary>
        /// Returns the next number in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in well-known text format. 
        /// The next token must be a number.
        /// </param>
        /// <returns>Returns the next number in the stream.</returns>
        /// <exception cref="ParseException">Thrown if the next token is not a number.</exception>
        private static Double? getNextTokenIfNumber(WktTokenizer tokenizer)
        {
            TokenType type = tokenizer.NextTokenType;

            if (type == TokenType.Number)
            {
                tokenizer.Read();
                return tokenizer.CurrentTokenAsNumber;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the next "EMPTY" or "(" in the stream as uppercase text.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next token must be "EMPTY" or "(".
        /// </param>
        /// <returns>
        /// The next "EMPTY" or "(" in the stream as uppercase text.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if the next token is not "EMPTY" or "(".
        /// </exception>
        private static String getNextEmptyOrOpener(WktTokenizer tokenizer)
        {
            tokenizer.Read();
            String nextWord = tokenizer.CurrentToken;

            if (nextWord == "EMPTY" || nextWord == "(")
            {
                return nextWord;
            }

            throw new ParseException("Expected 'EMPTY' or '(' but encountered '"
                                     + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ")" or "," in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next token must be ")" or ",".
        /// </param>
        /// <returns>Returns the next ")" or "," in the stream.</returns>
        /// <exception cref="ParseException">
        /// Thrown if the next token is not ")" or ",".
        /// </exception>
        private static String getNextCloserOrComma(WktTokenizer tokenizer)
        {
            tokenizer.Read();
            String nextWord = tokenizer.CurrentToken;

            if (nextWord == "," || nextWord == ")")
            {
                return nextWord;
            }

            throw new ParseException("Expected ')' or ',' but encountered '"
                                     + nextWord + "'");
        }

        /// <summary>
        /// Returns the next ")" in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next token must be ")".
        /// </param>
        /// <returns>
        /// Returns the next ")" in the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if the next token is not ")".
        /// </exception>
        private static void removeNextCloser(WktTokenizer tokenizer)
        {
            String nextWord = getNextWord(tokenizer);

            if (nextWord != ")")
            {
                throw new ParseException("Expected ')' but encountered '"
                                         + nextWord + "'");
            }
        }

        /// <summary>
        /// Returns the next word in the stream as uppercase text.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next token must be a word.
        /// </param>
        /// <returns>Returns the next word in the stream as uppercase text.</returns>
        /// <exception cref="ParseException">
        /// Thrown if the next token is not a word.
        /// </exception>
        private static String getNextWord(WktTokenizer tokenizer)
        {
            TokenType type = tokenizer.Read();
            String token = tokenizer.CurrentToken;

            if (type == TokenType.Number)
            {
                throw new Exception("Expected a number but got " + token);
            }
            else if (type == TokenType.Word)
            {
                return token.ToUpper();
            }
            else if (token == "(")
            {
                return "(";
            }
            else if (token == ")")
            {
                return ")";
            }
            else if (token == ",")
            {
                return ",";
            }

            throw new ParseException("Not a valid symbol in WKT format.");
        }

        /// <summary>
        /// Creates a Geometry using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">Tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a &lt;Geometry Tagged Text&gt;.</param>
        /// <returns>Returns a Geometry specified by the next token in the stream.</returns>
        /// <remarks>
        /// Exception is thrown if the coordinates used to create a Polygon
        /// shell and holes do not form closed linestrings, or if an unexpected
        /// token is encountered.
        /// </remarks>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if an unsupported geometry is encountered.
        /// </exception>
        private static IGeometry<TCoordinate> readGeometryTaggedText<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            tokenizer.Read();
            String type = tokenizer.CurrentToken.ToUpper();
            IGeometry<TCoordinate> geometry;

            switch (type)
            {
                case "POINT":
                    geometry = readPointText(tokenizer, factory);
                    break;
                case "LINESTRING":
                    geometry = readLineStringText(tokenizer, factory);
                    break;
                case "MULTIPOINT":
                    geometry = readMultiPointText(tokenizer, factory);
                    break;
                case "MULTILINESTRING":
                    geometry = readMultiLineStringText(tokenizer, factory);
                    break;
                case "POLYGON":
                    geometry = readPolygonText(tokenizer, factory);
                    break;
                case "MULTIPOLYGON":
                    geometry = readMultiPolygonText(tokenizer, factory);
                    break;
                case "GEOMETRYCOLLECTION":
                    geometry = readGeometryCollectionText(tokenizer, factory);
                    break;
                case "LINEARRING":
                    geometry = readLinearRing(tokenizer, factory);
                    break;
                default:
                    throw new NotSupportedException(String.Format(WktTokenizer.NumberFormat_enUS,
                                                                  "Geometrytype '{0}' is not supported.", type));
            }

            return geometry;
        }



        private static TCoordinate getCoordinate<TCoordinate>(WktTokenizer tokenizer,
                                                              Boolean is3D,
                                                              IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            Double x, y, z = 0;
            Double? m;
            Boolean inBraces = false;

            if (tokenizer.NextToken == "(")
            {
                tokenizer.ReadToken("(");
                inBraces = true;
            }
            Double? value = getNextTokenIfNumber(tokenizer);

            Debug.Assert(value != null);
            x = (Double)value;
            value = getNextTokenIfNumber(tokenizer);
            Debug.Assert(value != null);
            y = (Double)value;

            value = getNextTokenIfNumber(tokenizer);

            if (is3D)
            {
                if (value != null)
                {
                    z = (Double)value;
                }

                value = getNextTokenIfNumber(tokenizer);
            }

            m = value;
            if (inBraces) tokenizer.ReadToken(")");

            if (is3D)
            {
                if (m.HasValue)
                {
                    return factory.CoordinateFactory.Create3D(x, y, z, m.Value);
                }
                else
                {
                    return factory.CoordinateFactory.Create3D(x, y, z);
                }
            }
            else
            {
                if (m.HasValue)
                {
                    return factory.CoordinateFactory.Create(x, y, m.Value);
                }
                else
                {
                    return factory.CoordinateFactory.Create(x, y);
                }
            }
            
        }

        /// <summary>
        /// Creates a Point using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a &lt;Point Text&gt;.
        /// </param>
        /// <returns>
        /// Returns a Point specified by the next token in
        /// the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered.
        /// </exception>
        private static IMultiPoint<TCoordinate> readMultiPointText<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IMultiPoint<TCoordinate> multipoint = factory.CreateMultiPoint();
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return multipoint;
            }

            do
            {
                TCoordinate coordinate = getCoordinate(tokenizer, false, factory);
                multipoint.Add(factory.CreatePoint(coordinate));
                nextToken = getNextCloserOrComma(tokenizer);
            } while (nextToken == ",");

            return multipoint;
        }

        /// <summary>
        /// Creates a Point using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a &lt;Point Text&gt;.
        /// </param>
        /// <returns>
        /// Returns a Point specified by the next token in
        /// the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered.
        /// </exception>
        private static IPoint<TCoordinate> readPointText<TCoordinate>(
                                                WktTokenizer tokenizer,
                                                IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return factory.CreatePoint();
            }

            TCoordinate coordinate = getCoordinate(tokenizer, false, factory);

            removeNextCloser(tokenizer);

            return factory.CreatePoint(coordinate);
        }

        /// <summary>
        /// Creates a <see cref="IMultiPolygon"/> using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a MultiPolygon.</param>
        /// <returns>
        /// A <see cref="IMultiPolygon"/> specified by the next token in the 
        /// stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered or 
        /// if if the coordinates used to create the <see cref="IPolygon"/>
        /// shells and holes do not form closed linestrings.
        /// </exception>
        private static IMultiPolygon<TCoordinate> readMultiPolygonText<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IMultiPolygon<TCoordinate> polygons = factory.CreateMultiPolygon();
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return polygons;
            }

            do
            {
                IPolygon<TCoordinate> polygon = readPolygonText(tokenizer, factory);
                polygons.Add(polygon);
                nextToken = getNextCloserOrComma(tokenizer);
            } while (nextToken == ",");

            return polygons;
        }

        /// <summary>
        /// Creates a Polygon using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a &lt;Polygon Text&gt;.</param>
        /// <returns>
        /// Returns a Polygon specified by the next token
        /// in the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered or 
        /// if if the coordinates used to create the <see cref="IPolygon"/>
        /// shells and holes do not form closed linestrings.
        /// </exception>
        private static IPolygon<TCoordinate> readPolygonText<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return factory.CreatePolygon();
            }

            ILinearRing<TCoordinate> shell = factory.CreateLinearRing(getCoordinates(tokenizer, factory));
            List<ILinearRing<TCoordinate>> interiorRings = null;

            nextToken = getNextCloserOrComma(tokenizer);

            while (nextToken == ",")
            {
                if (interiorRings == null)
                {
                    interiorRings = new List<ILinearRing<TCoordinate>>();
                }

                //Add holes
                interiorRings.Add(factory.CreateLinearRing(getCoordinates(tokenizer, factory)));
                nextToken = getNextCloserOrComma(tokenizer);
            }

            IPolygon<TCoordinate> polygon;

            if (interiorRings == null)
            {
                polygon = factory.CreatePolygon(shell);
            }
            else
            {
                polygon = factory.CreatePolygon(shell, interiorRings);
            }

            return polygon;
        }

        /// <summary>
        /// Creates a <see cref="IMultiLineString"/> using the next token in the stream. 
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text format. 
        /// The next tokens must form a &lt;MultiLineString Text&gt;.
        /// </param>
        /// <returns>
        /// A <see cref="IMultiLineString"/> specified by the next token in the stream.
        /// </returns>
        private static IMultiLineString<TCoordinate> readMultiLineStringText<TCoordinate>(
                                                        WktTokenizer tokenizer,
                                                        IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IMultiLineString<TCoordinate> lines = factory.CreateMultiLineString();
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken == "EMPTY")
            {
                return lines;
            }

            do
            {
                lines.Add(readLineStringText(tokenizer, factory));
                nextToken = getNextCloserOrComma(tokenizer);
            } while (nextToken == ",");

            return lines;
        }

        /// <summary>
        /// Creates a LineString using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text format.  
        /// The next tokens must form a &lt;LineString Text&gt;.
        /// </param>
        /// <returns>
        /// Returns a LineString specified by the next token in the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered.
        /// </exception>
        private static ILineString<TCoordinate> readLineStringText<TCoordinate>(
                                                    WktTokenizer tokenizer,
                                                    IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            return factory.CreateLineString(getCoordinates(tokenizer, factory));
        }

        private static ILinearRing<TCoordinate> readLinearRing<TCoordinate>(WktTokenizer tokenizer, IGeometryFactory<TCoordinate> factory) where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>, IComputable<double, TCoordinate>
        {
            return factory.CreateLinearRing(getCoordinates(tokenizer, factory));
        }

        /// <summary>
        /// Creates a <see cref="IGeometryCollection"/> using the next token in the stream.
        /// </summary>
        /// <param name="tokenizer">
        /// Tokenizer over a stream of text in Well-Known Text
        /// format. The next tokens must form a GeometryCollection Text.
        /// </param>
        /// <returns>
        /// A <see cref="IGeometryCollection"/> specified by the next token in the stream.
        /// </returns>
        /// <exception cref="ParseException">
        /// Thrown if an unexpected token is encountered.
        /// </exception>
        private static IGeometryCollection<TCoordinate> readGeometryCollectionText<TCoordinate>(
                                                            WktTokenizer tokenizer,
                                                            IGeometryFactory<TCoordinate> factory)
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
        {
            IGeometryCollection<TCoordinate> geometries = factory.CreateGeometryCollection();
            String nextToken = getNextEmptyOrOpener(tokenizer);

            if (nextToken.Equals("EMPTY"))
            {
                return geometries;
            }

            do
            {
                geometries.Add(readGeometryTaggedText(tokenizer, factory));
                nextToken = getNextCloserOrComma(tokenizer);
            } while (nextToken.Equals(","));

            return geometries;
        }
    }
}