// Copyright 2012 - Felix Obermaier (www.ivv-aachen.de)
//
// This file is part of DotSpatial.Topology.GeoAPIConverter.
// DotSpatial.Topology.GeoAPIConverter is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// DotSpatial.Topology.GeoAPIConverter is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

// The conversion code for GeoAPI to DotSpatial.Data.Shape was taken and enhanced
// from DotSpatial.Data.Shape.

using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoAPI.Geometries
{
    using DSCoordinate = DotSpatial.Topology.Coordinate;
    using DSExtent = DotSpatial.Data.Extent;
    using DSFeatureType = DotSpatial.Topology.FeatureType;
    using DSGeometry = DotSpatial.Topology.IGeometry;
    using DSGeometryCollection = DotSpatial.Topology.IGeometryCollection;
    using DSGeometryFactory = DotSpatial.Topology.IGeometryFactory;
    using DSLinearRing = DotSpatial.Topology.ILinearRing;
    using DSLineString = DotSpatial.Topology.ILineString;
    using DSMultiLineString = DotSpatial.Topology.IMultiLineString;
    using DSMultiPoint = DotSpatial.Topology.IMultiPoint;
    using DSMultiPolygon = DotSpatial.Topology.IMultiPolygon;
    using DSPartRange = DotSpatial.Data.PartRange;
    using DSPoint = DotSpatial.Topology.IPoint;
    using DSPolygon = DotSpatial.Topology.IPolygon;
    using DSShape = DotSpatial.Data.Shape;
    using DSShapeRange = DotSpatial.Data.ShapeRange;

    public static class GeometryConverter
    {
        public static readonly DSGeometryFactory Default = DotSpatial.Topology.GeometryFactory.Default;

        /// <summary>
        /// Converts a <see cref="IGeometry"/> to a <see cref="DSGeometry"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IGeometry"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSGeometry"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSGeometry ToDotSpatial(this IGeometry geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromGeometry(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IPoint"/> to a <see cref="DSPoint"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IPoint"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSPoint"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSPoint ToDotSpatial(this IPoint geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromPoint(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="ILineString"/> to a <see cref="DSLineString"/>
        /// </summary>
        /// <param name="geometry">The <see cref="ILineString"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSLineString"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSLineString ToDotSpatial(this ILineString geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromLineString(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="ILinearRing"/> to a <see cref="DSLinearRing"/>
        /// </summary>
        /// <param name="geometry">The <see cref="ILinearRing"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSLinearRing"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSLinearRing ToDotSpatial(this ILinearRing geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return (DSLinearRing)FromLineString(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IPolygon"/> to a <see cref="DSPolygon"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IPolygon"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSPolygon"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSPolygon ToDotSpatial(this IPolygon geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromPolygon(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiPoint"/> to a <see cref="DSMultiPoint"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IMultiPoint"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSMultiPoint"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSMultiPoint ToDotSpatial(this IMultiPoint geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromMultiPoint(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiLineString"/> to a <see cref="DSMultiLineString"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IMultiLineString"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSMultiLineString"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSMultiLineString ToDotSpatial(this IMultiLineString geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromMultiLineString(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiPolygon"/> to a <see cref="DSMultiPolygon"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IMultiPolygon"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSMultiPolygon"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSMultiPolygon ToDotSpatial(this IMultiPolygon geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromMultiPolygon(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IGeometryCollection"/> to a <see cref="DSGeometryCollection"/>
        /// </summary>
        /// <param name="geometry">The <see cref="IGeometryCollection"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="DSGeometryCollection"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometry"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static DSGeometryCollection ToDotSpatial(this IGeometryCollection geometry, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return FromGeometryCollection(geometry, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="System.Collections.Generic.IList{IGeometry}"/> to a <see cref="System.Collections.Generic.IList{DSGeometry}"/>
        /// </summary>
        /// <param name="geometries">The <see cref="System.Collections.Generic.IList{IGeometry}"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="System.Collections.Generic.IList{DSGeometry}"/></param>
        /// <param name="setUserData">Sets the <see cref="DSGeometry.UserData"/> to <paramref name="geometries"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static IList<DSGeometry> ToDotSpatial(this IList<IGeometry> geometries, DSGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? Default;
            return geometries.Select(geometry => FromGeometry(geometry, useFactory, setUserData)).ToList();
        }

        private static DSGeometry FromGeometry(IGeometry geometry, DSGeometryFactory factory, bool setUserData)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            if (factory == null)
                throw new ArgumentNullException("factory");

            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    return FromPoint((IPoint)geometry, factory, setUserData);
                case OgcGeometryType.LineString:
                    return FromLineString((ILineString)geometry, factory, setUserData);
                case OgcGeometryType.Polygon:
                    return FromPolygon((IPolygon)geometry, factory, setUserData);
                case OgcGeometryType.MultiPoint:
                    return FromMultiPoint((IMultiPoint)geometry, factory, setUserData);
                case OgcGeometryType.MultiLineString:
                    return FromMultiLineString((IMultiLineString)geometry, factory, setUserData);
                case OgcGeometryType.MultiPolygon:
                    return FromMultiPolygon((IMultiPolygon)geometry, factory, setUserData);
                case OgcGeometryType.GeometryCollection:
                    return FromGeometryCollection((IGeometryCollection)geometry, factory, setUserData);
                default:
                    throw new ArgumentException();
            }
        }

        private static DSMultiPoint FromMultiPoint(IMultiPoint geometry, DSGeometryFactory factory, bool setUserData)
        {
            var result = factory.CreateMultiPoint(FromCoordinates(geometry.Coordinates));
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSCoordinate FromCoordinate(Coordinate coordinate)
        {
            return new DSCoordinate(new[] { coordinate.X, coordinate.Y, coordinate.Z });
        }

        private static DSCoordinate[] FromCoordinates(Coordinate[] coordinates)
        {
            var ret = new DSCoordinate[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
                ret[i] = FromCoordinate(coordinates[i]);
            return ret;
        }

        /*
        private static DSCoordinateSequence FromCoordinateSequence(ICoordinateSequence coordinateSequence, DSCoordinateSequenceFactory factory)
        {
            var coordinates = ConvertCoordinates(coordinateSequence.ToCoordinateArray());
            return factory.Create(coordinates);
        }
         */

        private static DSPoint FromPoint(IPoint geometry, DSGeometryFactory factory, bool setUserData)
        {
            var coord = FromCoordinate(geometry.Coordinate);
            var result = factory.CreatePoint(coord);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSLineString FromLineString(ILineString geometry, DSGeometryFactory factory, bool setUserData)
        {
            var coordinates = FromCoordinates(geometry.Coordinates);
            var result = (geometry is ILinearRing)
                             ? factory.CreateLinearRing(coordinates)
                             : factory.CreateLineString(coordinates);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSPolygon FromPolygon(IPolygon geometry, DSGeometryFactory factory, bool setUserData)
        {
            var shell = (DSLinearRing)FromLineString(geometry.Shell, factory, setUserData);
            DSLinearRing[] holes = null;
            if (geometry.Holes != null && geometry.Holes.Length > 0)
            {
                holes = new DSLinearRing[geometry.Holes.Length];
                for (var i = 0; i < holes.Length; i++)
                    holes[i] = (DSLinearRing)FromLineString(geometry.Holes[i], factory, setUserData);
            }
            var result = factory.CreatePolygon(shell, holes);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSMultiLineString FromMultiLineString(IMultiLineString geometry, DSGeometryFactory factory, bool setUserData)
        {
            var dsLineStrings = new DotSpatial.Topology.IBasicLineString[geometry.NumGeometries];

            for (var i = 0; i < dsLineStrings.Length; i++)
                dsLineStrings[i] = FromLineString((ILineString)geometry.GetGeometryN(i), factory, setUserData);

            var result = factory.CreateMultiLineString(dsLineStrings);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSMultiPolygon FromMultiPolygon(IMultiPolygon geometry, DSGeometryFactory factory, bool setUserData)
        {
            var dsPolygons = new DotSpatial.Topology.IPolygon[geometry.NumGeometries];

            for (var i = 0; i < dsPolygons.Length; i++)
                dsPolygons[i] = FromPolygon((IPolygon)geometry.GetGeometryN(i), factory, setUserData);

            var result = factory.CreateMultiPolygon(dsPolygons);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static DSGeometryCollection FromGeometryCollection(IGeometryCollection geometry, DSGeometryFactory factory, bool setUserData)
        {
            var dsGeometries = new DotSpatial.Topology.IGeometry[geometry.NumGeometries];

            for (var i = 0; i < dsGeometries.Length; i++)
                dsGeometries[i] = FromGeometry(geometry.GetGeometryN(i), factory, setUserData);

            var result = factory.CreateGeometryCollection(dsGeometries);
            if (setUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        /// <summary>
        /// Creates a shape based on the specified geometry.  This shape will be standing alone,
        /// all by itself.
        /// </summary>
        /// <param name="geometry">The geometry to create a shape from.</param>
        /// <param name="copyAttributes">The <see cref="IGeometry.UserData"/> will be copied to the <see cref="DSShape.Attributes"/>.</param>
        /// <returns>A <see cref="DSShape"/> representing the <paramref name="geometry"/></returns>
        public static DSShape ToDotSpatialShape(this IGeometry geometry, bool copyAttributes = false)
        {
            if (geometry is IPolygonal)
                geometry = OrientationEnsured((IPolygonal)geometry);

            var coords = geometry.Coordinates;
            var vertices = new double[geometry.NumPoints * 2];
            var z = new double[geometry.NumPoints];
            var hasZ = false;
            double[] m = null; //new double[geometry.NumPoints];
            var hasM = false;

            var shape = new DSShape
                            {
                                MinZ = double.MaxValue,
                                MaxZ = double.MinValue,
                                MinM = double.MaxValue,
                                MaxM = double.MinValue
                            };

            for (var i = 0; i < coords.Length; i++)
            {
                var c = coords[i];
                vertices[i * 2] = c.X;
                vertices[i * 2 + 1] = c.Y;

                if (!Double.IsNaN(c.Z))
                {
                    z[i] = c.Z;
                    shape.MinZ = shape.MinZ < c.Z ? shape.MinZ : c.Z;
                    shape.MaxZ = shape.MaxZ < c.Z ? shape.MaxZ : c.Z;
                    hasZ = true;
                }
                /*
                if (!Double.IsNaN(c.M))
                {
                    m[i] = c.M;
                    shape.MinM = shape.MinM < c.M ? shape.MinM : c.M;
                    shape.MaxM = shape.MaxM < c.M ? shape.MaxM : c.M;
                    hasZ = true;
                }
                 */
            }

            if (!hasZ)
            {
                z = null;
                shape.MinZ = shape.MaxZ = double.NaN;
            }
            if (!hasM)
            {
                m = null;
                shape.MinM = shape.MaxM = double.NaN;
            }

            shape.Range = ShapeRangeFromGeometry(geometry, vertices, 0);
            shape.Vertices = vertices;
            shape.Z = z;
            shape.M = m;

            if (geometry.UserData != null && copyAttributes)
            {
                var ud = geometry.UserData as object[];
                shape.Attributes = ud ?? new[] { geometry.UserData };
            }
            return shape;
        }

        public static DSExtent ToDotSpatial(this Envelope self)
        {
            return new DSExtent(self.MinX, self.MinY, self.MaxX, self.MaxY);
        }

        private static DSFeatureType ToDotSpatial(this OgcGeometryType self)
        {
            switch (self)
            {
                case OgcGeometryType.Point:
                    return DSFeatureType.Point;
                case OgcGeometryType.MultiPoint:
                    return DSFeatureType.MultiPoint;
                case OgcGeometryType.Polygon:
                case OgcGeometryType.MultiPolygon:
                    return DSFeatureType.Polygon;
                case OgcGeometryType.LineString:
                case OgcGeometryType.MultiLineString:
                    return DSFeatureType.Line;
                default:
                    return DSFeatureType.Unspecified;
            }
        }

        /// <summary>
        /// Create a ShapeRange from a Geometry to use in constructing a Shape
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="vertices"></param>
        /// <param name="offset">offset into vertices array where this feature starts</param>
        /// <returns></returns>
        private static DSShapeRange ShapeRangeFromGeometry(IGeometry geometry, double[] vertices, int offset)
        {
            var featureType = geometry.OgcGeometryType.ToDotSpatial();
            var shx = new DSShapeRange(featureType) { Extent = geometry.EnvelopeInternal.ToDotSpatial() };
            var vIndex = offset / 2;
            shx.Parts = new List<DSPartRange>();
            var shapeStart = vIndex;

            for (var part = 0; part < geometry.NumGeometries; part++)
            {
                var prtx = new DSPartRange(vertices, shapeStart, vIndex - shapeStart, featureType);
                var bp = geometry.GetGeometryN(part) as IPolygon;
                if (bp != null)
                {
                    // Account for the Shell
                    prtx.NumVertices = bp.Shell.NumPoints;

                    vIndex += bp.Shell.NumPoints;

                    // The part range should be adjusted to no longer include the holes
                    foreach (var hole in bp.Holes)
                    {
                        var holex = new DSPartRange(vertices, shapeStart, vIndex - shapeStart, featureType)
                        {
                            NumVertices = hole.NumPoints
                        };
                        shx.Parts.Add(holex);
                        vIndex += hole.NumPoints;
                    }
                }
                else
                {
                    int numPoints = geometry.GetGeometryN(part).NumPoints;

                    // This is not a polygon, so just add the number of points.
                    vIndex += numPoints;
                    prtx.NumVertices = numPoints;
                }

                shx.Parts.Add(prtx);
            }
            return shx;
        }

        private static IGeometry OrientationEnsured(IPolygonal geometry)
        {
            var polygon = geometry as IPolygon;
            if (polygon != null)
                return OrientationEnsured(polygon);

            var mp = (IMultiPolygon)geometry;
            var polygons = new IPolygon[mp.NumGeometries];
            for (var i = 0; i < mp.NumGeometries; i++)
                polygons[i] = (IPolygon)OrientationEnsured((IPolygon)mp.GetGeometryN(i));
            return mp.Factory.CreateMultiPolygon(polygons);
        }

        private static IGeometry OrientationEnsured(IPolygon geometry)
        {
            var shell = OrientationEnsured(geometry.Shell, false);
            ILinearRing[] holes = null;
            if (geometry.NumInteriorRings > 0)
            {
                holes = new ILinearRing[geometry.NumInteriorRings];
                for (var i = 0; i < geometry.NumInteriorRings; i++)
                    holes[i] = OrientationEnsured((ILinearRing)geometry.GetInteriorRingN(i), true);
            }
            return geometry.Factory.CreatePolygon(shell, holes);
        }

        private static ILinearRing OrientationEnsured(ILinearRing ring, bool ccw)
        {
            Console.WriteLine(NetTopologySuite.Algorithm.CGAlgorithms.IsCCW(ring.Coordinates) == ccw);

            if (NetTopologySuite.Algorithm.CGAlgorithms.IsCCW(ring.Coordinates) == ccw)
                return ring;

            var coordinates = new Coordinate[ring.NumPoints];
            Array.Copy(ring.Coordinates, coordinates, coordinates.Length);
            Array.Reverse(coordinates);
            return ring.Factory.CreateLinearRing(coordinates);
        }
    }
}