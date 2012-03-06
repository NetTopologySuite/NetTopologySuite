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

namespace DotSpatial.Topology
{
    using GeoAPICoordinate = GeoAPI.Geometries.Coordinate;
    using GeoAPIEnvelope = GeoAPI.Geometries.Envelope;
    using GeoAPIGeometry = GeoAPI.Geometries.IGeometry;
    using GeoAPIGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
    using GeoAPIGeometryFactory = GeoAPI.Geometries.IGeometryFactory;
    using GeoAPILinearRing = GeoAPI.Geometries.ILinearRing;
    using GeoAPILineString = GeoAPI.Geometries.ILineString;
    using GeoAPIMultiLineString = GeoAPI.Geometries.IMultiLineString;
    using GeoAPIMultiPoint = GeoAPI.Geometries.IMultiPoint;
    using GeoAPIMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
    using GeoAPIPoint = GeoAPI.Geometries.IPoint;
    using GeoAPIPolygon = GeoAPI.Geometries.IPolygon;
    using GeoAPIPrecisionModel = GeoAPI.Geometries.IPrecisionModel;

    public static class GeometryConverter
    {
        private static readonly Dictionary<PrecisionModelType, GeoAPIPrecisionModel> ConvertedPrecisionModels =
            new Dictionary<PrecisionModelType, GeoAPIPrecisionModel>();

        private static readonly Dictionary<IGeometryFactory, GeoAPIGeometryFactory> ConvertedGeometryFactories =
            new Dictionary<IGeometryFactory, GeoAPIGeometryFactory>();

        public static GeoAPIPrecisionModel ToGeoAPI(this PrecisionModelType model)
        {
            GeoAPIPrecisionModel geoAPIPrecisionModel;
            if (ConvertedPrecisionModels.TryGetValue(model, out geoAPIPrecisionModel))
                return geoAPIPrecisionModel;

            var geoService = GeoAPI.GeometryServiceProvider.Instance;
            switch (model)
            {
                case PrecisionModelType.Floating:
                    geoAPIPrecisionModel = geoService.CreatePrecisionModel(GeoAPI.Geometries.PrecisionModels.Floating);
                    break;
                case PrecisionModelType.FloatingSingle:
                    geoAPIPrecisionModel = geoService.CreatePrecisionModel(GeoAPI.Geometries.PrecisionModels.FloatingSingle);
                    break;
                case PrecisionModelType.Fixed:
                    geoAPIPrecisionModel = geoService.CreatePrecisionModel(GeoAPI.Geometries.PrecisionModels.Fixed);
                    break;
            }

            ConvertedPrecisionModels.Add(model, geoAPIPrecisionModel);
            return geoAPIPrecisionModel;
        }

        public static GeoAPIGeometryFactory ToGeoAPI(this IGeometryFactory factory)
        {
            GeoAPIGeometryFactory geoAPIFactory;
            if (ConvertedGeometryFactories.TryGetValue(factory, out geoAPIFactory))
                return geoAPIFactory;

            var geoAPIPrecisionModel = factory.PrecisionModel.ToGeoAPI();

            var geoService = GeoAPI.GeometryServiceProvider.Instance;
            geoAPIFactory = geoService.CreateGeometryFactory(geoAPIPrecisionModel, factory.Srid);

            ConvertedGeometryFactories.Add(factory, geoAPIFactory);
            return geoAPIFactory;
        }

        /// <summary>
        /// Converts a <see cref="IGeometry"/> to a <see cref="GeoAPIGeometry"/>
        /// </summary>
        /// <param name="self">The <see cref="IGeometry"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIGeometry"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIGeometry ToGeoAPI(this IGeometry self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromGeometry(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IPoint"/> to a <see cref="GeoAPIPoint"/>
        /// </summary>
        /// <param name="self">The <see cref="IPoint"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIPoint"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIPoint ToGeoAPI(this IPoint self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromPoint(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="ILineString"/> to a <see cref="GeoAPILineString"/>
        /// </summary>
        /// <param name="self">The <see cref="ILineString"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPILineString"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPILineString ToGeoAPI(this ILineString self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromLineString(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="ILinearRing"/> to a <see cref="GeoAPILinearRing"/>
        /// </summary>
        /// <param name="self">The <see cref="ILinearRing"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPILinearRing"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPILinearRing ToGeoAPI(this ILinearRing self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return (GeoAPILinearRing)FromLineString(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IPolygon"/> to a <see cref="GeoAPIPolygon"/>
        /// </summary>
        /// <param name="self">The <see cref="IPolygon"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIPolygon"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIPolygon ToGeoAPI(this IPolygon self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromPolygon(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiPoint"/> to a <see cref="GeoAPIMultiPoint"/>
        /// </summary>
        /// <param name="self">The <see cref="IMultiPoint"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIMultiPoint"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIMultiPoint ToGeoAPI(this IMultiPoint self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromMultiPoint(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiLineString"/> to a <see cref="GeoAPIMultiLineString"/>
        /// </summary>
        /// <param name="self">The <see cref="IMultiLineString"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIMultiLineString"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIMultiLineString ToGeoAPI(this IMultiLineString self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromMultiLineString(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IMultiLineString"/> to a <see cref="GeoAPIMultiLineString"/>
        /// </summary>
        /// <param name="self">The <see cref="IMultiLineString"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIMultiLineString"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIMultiPolygon ToGeoAPI(this IMultiPolygon self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromMultiPolygon(self, useFactory, setUserData);
        }

        /// <summary>
        /// Converts a <see cref="IGeometryCollection"/> to a <see cref="GeoAPIGeometryCollection"/>
        /// </summary>
        /// <param name="self">The <see cref="IGeometryCollection"/> to convert</param>
        /// <param name="factory">The factory to create the <see cref="GeoAPIGeometryCollection"/></param>
        /// <param name="setUserData">Sets the <see cref="GeoAPIGeometry.UserData"/> to <paramref name="self"/>.UserData</param>
        /// <returns>The converted geometry</returns>
        public static GeoAPIGeometryCollection ToGeoAPI(this IGeometryCollection self, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            var useFactory = factory ?? self.Factory.ToGeoAPI();
            return FromGeometryCollection(self, useFactory, setUserData);
        }

        public static IList<GeoAPIGeometry> ToGeoAPI(this IList<IGeometry> geometries, GeoAPIGeometryFactory factory = null, bool setUserData = false)
        {
            return geometries.Select(geometry => FromGeometry(geometry, factory, setUserData)).ToList();
        }

        private static GeoAPIGeometry FromGeometry(IGeometry geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var point = geometry as IPoint;
            if (point != null)
                return FromPoint(point, factory, copyUserData);

            var lineString = geometry as ILineString;
            if (lineString != null)
                return FromLineString(lineString, factory, copyUserData);

            var polygon = geometry as IPolygon;
            if (polygon != null)
                return FromPolygon(polygon, factory, copyUserData);

            var multiPoint = geometry as IMultiPoint;
            if (multiPoint != null)
                return FromMultiPoint(multiPoint, factory, copyUserData);

            var multiLineString = geometry as IMultiLineString;
            if (multiLineString != null)
                return FromMultiLineString(multiLineString, factory, copyUserData);

            var multiPolygon = geometry as IMultiPolygon;
            if (multiPolygon != null)
                return FromMultiPolygon(multiPolygon, factory, copyUserData);

            var geometryCollection = geometry as IGeometryCollection;
            if (geometryCollection != null)
                return FromGeometryCollection(geometryCollection, factory, copyUserData);

            throw new ArgumentException();
        }

        private static GeoAPICoordinate FromCoordinate(Coordinate coordinate)
        {
            return new GeoAPICoordinate(coordinate.X, coordinate.Y, coordinate.Z);
        }

        private static GeoAPICoordinate[] FromCoordinates(IList<Coordinate> coordinates)
        {
            var ret = new GeoAPICoordinate[coordinates.Count];

            for (var i = 0; i < coordinates.Count; i++)

                ret[i] = FromCoordinate(coordinates[i]);

            return ret;
        }

        /*
        private static DSCoordinateSequence FromCoordinateSequence(ICoordinateSequence coordinateSequence, DSCoordinateSequenceFactory factory)
        {
            var coordinates = FromCoordinates(coordinateSequence.ToCoordinateArray());
            return factory.Create(coordinates);
        }
         */

        private static GeoAPIPoint FromPoint(IPoint geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var coord = FromCoordinate(geometry.Coordinate);
            var point = factory.CreatePoint(coord);
            if (copyUserData)
                point.UserData = geometry.UserData;
            return point;
        }

        private static GeoAPILineString FromLineString(ILineString geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var coordinates = FromCoordinates(geometry.Coordinates);
            var result = (geometry is ILinearRing)
                       ? factory.CreateLinearRing(coordinates)
                       : factory.CreateLineString(coordinates);
            if (copyUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static GeoAPIPolygon FromPolygon(IPolygon geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var shell = (GeoAPILinearRing)FromLineString(geometry.Shell, factory, copyUserData);
            GeoAPILinearRing[] holes = null;
            if (geometry.Holes != null && geometry.Holes.Length > 0)
            {
                holes = new GeoAPILinearRing[geometry.Holes.Length];
                for (var i = 0; i < holes.Length; i++)
                    holes[i] = (GeoAPILinearRing)FromLineString(geometry.Holes[i], factory, copyUserData);
            }
            return factory.CreatePolygon(shell, holes);
        }

        private static GeoAPIMultiPoint FromMultiPoint(IMultiPoint geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var result = factory.CreateMultiPoint(FromCoordinates(geometry.Coordinates));
            if (copyUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static GeoAPIMultiLineString FromMultiLineString(IMultiLineString geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var dsLineStrings = new GeoAPILineString[geometry.NumGeometries];

            for (var i = 0; i < dsLineStrings.Length; i++)
                dsLineStrings[i] = FromLineString((ILineString)geometry.GetGeometryN(i), factory, copyUserData);

            var result = factory.CreateMultiLineString(dsLineStrings);
            if (copyUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static GeoAPIMultiPolygon FromMultiPolygon(IMultiPolygon geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var dsPolygons = new GeoAPI.Geometries.IPolygon[geometry.NumGeometries];

            for (var i = 0; i < dsPolygons.Length; i++)
                dsPolygons[i] = FromPolygon((IPolygon)geometry.GetGeometryN(i), factory, copyUserData);

            var result = factory.CreateMultiPolygon(dsPolygons);
            if (copyUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        private static GeoAPIGeometryCollection FromGeometryCollection(IGeometryCollection geometry, GeoAPIGeometryFactory factory, bool copyUserData)
        {
            var dsGeometries = new GeoAPI.Geometries.IGeometry[geometry.NumGeometries];

            for (var i = 0; i < dsGeometries.Length; i++)
                dsGeometries[i] = FromGeometry(geometry.GetGeometryN(i), factory, copyUserData);

            var result = factory.CreateGeometryCollection(dsGeometries);
            if (copyUserData)
                result.UserData = geometry.UserData;
            return result;
        }

        public static GeoAPIEnvelope ToGeoAPI(this Data.Extent extent)
        {
            return new GeoAPIEnvelope(extent.MinX, extent.MaxX, extent.MinY, extent.MaxY);
        }

        public static GeoAPIEnvelope ToGeoAPI(this Envelope extent)
        {
            return new GeoAPIEnvelope(extent.Minimum.X, extent.Maximum.X, extent.Minimum.Y, extent.Maximum.Y);
        }

        public static GeoAPIGeometry ToGeoAPI(this Data.Shape shape, GeoAPI.Geometries.IGeometryFactory factory = null, bool copyAttributes = false)
        {
            var useFactory = factory ?? GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory();

            if (shape.Range.FeatureType == FeatureType.Polygon)
            {
                return FromPolygonShape(shape, useFactory, copyAttributes);
            }
            if (shape.Range.FeatureType == FeatureType.Line)
            {
                return FromLineShape(shape, useFactory, copyAttributes);
            }
            if (shape.Range.FeatureType == FeatureType.MultiPoint)
            {
                return FromMultiPointShape(shape, useFactory, copyAttributes);
            }
            if (shape.Range.FeatureType == FeatureType.Point)
            {
                return FromPointShape(shape, useFactory, copyAttributes);
            }
            return null;
        }

        /// <summary>
        /// Get the point for this shape if this is a point shape.
        /// </summary>
        /// <param name="shape">The shape to convert</param>
        /// <param name="factory">The geometry factory to use.</param>
        /// <param name="copyAttributes">A value indicating whether or not to copy the <see cref="Data.Shape.Attributes"/> to <see cref="GeoAPIGeometry.UserData"/></param>
        /// <returns>The geometry representing the converted shape.</returns>
        private static GeoAPIGeometry FromPointShape(Data.Shape shape, GeoAPIGeometryFactory factory, bool copyAttributes)
        {
            var part = shape.Range.Parts[0];
            var i = part.StartIndex;

            var c = new GeoAPICoordinate(part.Vertices[0], part.Vertices[1]);
            //if (shape.HasM()) c.M = shape.M[i]
            if (shape.HasZ()) c.Z = shape.Z[i];
            var ret = factory.CreatePoint(c);
            if (copyAttributes)
                ret.UserData = shape.Attributes;
            return ret;
        }

        /// <summary>
        /// Creates a new MultiPoint geometry from a MultiPoint shape
        /// </summary>
        /// <param name="shape">The shape to convert</param>
        /// <param name="factory">The geometry factory to use.</param>
        /// <param name="copyAttributes">A value indicating whether or not to copy the <see cref="Data.Shape.Attributes"/> to <see cref="GeoAPIGeometry.UserData"/></param>
        /// <returns>The geometry representing the converted shape.</returns>
        /// <returns></returns>
        private static GeoAPIGeometry FromMultiPointShape(Data.Shape shape, GeoAPIGeometryFactory factory, bool copyAttributes)
        {
            var coords = new List<GeoAPICoordinate>();
            foreach (var part in shape.Range.Parts)
            {
                var i = part.StartIndex;
                foreach (var vertex in part)
                {
                    var c = new GeoAPICoordinate(vertex.X, vertex.Y);
                    coords.Add(c);
                    //if (shape.HasM()) c.M = shape.M[i];
                    if (shape.HasZ()) c.Z = shape.Z[i];
                    i++;
                }
            }
            var ret = factory.CreateMultiPoint(coords.ToArray());
            if (copyAttributes)
                ret.UserData = shape.Attributes;
            return ret;
        }

        /// <summary>
        /// Gets the line for the specified index
        /// </summary>
        /// <param name="shape">The shape to convert</param>
        /// <param name="factory">The geometry factory to use.</param>
        /// <param name="copyAttributes">A value indicating whether or not to copy the <see cref="Data.Shape.Attributes"/> to <see cref="GeoAPIGeometry.UserData"/></param>
        /// <returns>The geometry representing the converted shape.</returns>
        private static GeoAPIGeometry FromLineShape(Data.Shape shape, GeoAPIGeometryFactory factory, bool copyAttributes)
        {
            var lines = new List<GeoAPILineString>();
            foreach (var part in shape.Range.Parts)
            {
                var i = part.StartIndex;
                var coords = new List<GeoAPICoordinate>();
                foreach (var d in part)
                {
                    var c = new GeoAPICoordinate(d.X, d.Y);
                    coords.Add(c);
                    //if (shape.HasM()) c.M = M[i];
                    if (shape.HasZ()) c.Z = shape.Z[i];
                    i++;
                }
                lines.Add(factory.CreateLineString(coords.ToArray()));
            }
            if (lines.Count == 1)
            {
                if (copyAttributes)
                    lines[0].UserData = shape.Attributes;
                return lines[0];
            }

            var ret = factory.CreateMultiLineString(lines.ToArray());
            if (copyAttributes)
                ret.UserData = shape.Attributes;
            return ret;
        }

        /// <summary>
        /// Creates a Polygon or MultiPolygon from this Polygon shape.
        /// </summary>
        /// <param name="shape">The shape to convert</param>
        /// <param name="factory">The geometry factory to use.</param>
        /// <param name="copyAttributes">A value indicating whether or not to copy the <see cref="Data.Shape.Attributes"/> to <see cref="GeoAPIGeometry.UserData"/></param>
        /// <returns>The geometry representing the converted shape.</returns>
        private static GeoAPIGeometry FromPolygonShape(Data.Shape shape, GeoAPIGeometryFactory factory, bool copyAttributes)
        {
            var shells = new List<GeoAPILinearRing>();
            var holes = new List<GeoAPILinearRing>();
            foreach (var part in shape.Range.Parts)
            {
                var coords = new List<GeoAPICoordinate>();
                var i = part.StartIndex;
                foreach (var d in part)
                {
                    var c = new GeoAPICoordinate(d.X, d.Y);
                    //if (shape.HasM()) c.M = M[i];
                    if (shape.HasZ()) c.Z = shape.Z[i];
                    i++;
                    coords.Add(c);
                }
                var ring = factory.CreateLinearRing(coords.ToArray());
                if (shape.Range.Parts.Count == 1)
                {
                    shells.Add(ring);
                }
                else
                {
                    if (NetTopologySuite.Algorithm.CGAlgorithms.IsCCW(ring.Coordinates))
                    {
                        holes.Add(ring);
                    }
                    else
                    {
                        shells.Add(ring);
                    }
                }
            }
            //// Now we have a list of all shells and all holes
            var holesForShells = new List<GeoAPILinearRing>[shells.Count];
            for (var i = 0; i < shells.Count; i++)
            {
                holesForShells[i] = new List<GeoAPILinearRing>();
            }

            // Find holes
            foreach (var testRing in holes)
            {
                GeoAPILinearRing minShell = null;
                GeoAPIEnvelope minEnv = null;
                var testEnv = testRing.EnvelopeInternal;
                var testPt = testRing.Coordinates[0];
                for (int j = 0; j < shells.Count; j++)
                {
                    var tryRing = shells[j];
                    var tryEnv = tryRing.EnvelopeInternal;
                    if (minShell != null)
                        minEnv = minShell.EnvelopeInternal;
                    var isContained = tryEnv.Contains(testEnv)
                                      && (NetTopologySuite.Algorithm.CGAlgorithms.IsPointInRing(testPt, tryRing.Coordinates)
                                           || (PointInList(testPt, tryRing.Coordinates)));

                    // Check if this new containing ring is smaller than the current minimum ring
                    if (isContained)
                    {
                        if (minShell == null || minEnv.Contains(tryEnv))
                        {
                            minShell = tryRing;
                        }
                        holesForShells[j].Add(testRing);
                    }
                }
            }

            var polygons = new GeoAPIPolygon[shells.Count];
            for (var i = 0; i < shells.Count; i++)
            {
                polygons[i] = factory.CreatePolygon(shells[i], holesForShells[i].ToArray());
            }

            if (polygons.Length == 1)
            {
                if (copyAttributes)
                    polygons[0].UserData = shape.Attributes;
                return polygons[0];
            }
            // It's a multi part
            var ret = factory.CreateMultiPolygon(polygons);
            if (copyAttributes)
                ret.UserData = shape.Attributes;
            return ret;
        }

        private static bool HasM(this Data.Shape self)
        {
            return self.M != null && self.M.Length > 0;
        }

        private static bool HasZ(this Data.Shape self)
        {
            return self.Z != null && self.Z.Length > 0;
        }

        /// <summary>
        /// Test if a point is in a list of coordinates.
        /// </summary>
        /// <param name="testPoint">The point to test for.</param>
        /// <param name="pointList">The list of points to look through.</param>
        /// <returns>true if <paramref name="testPoint"/> is a point in the <paramref name="pointList"/> list.</returns>
        private static bool PointInList(GeoAPICoordinate testPoint, IEnumerable<GeoAPICoordinate> pointList)
        {
            return pointList.Any(p => p.Equals2D(testPoint));
        }
    }
}