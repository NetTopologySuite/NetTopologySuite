using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// PostGIS Geometry types
	/// </summary>
	internal enum PostGis2GeometryType
    {
        /// <summary>
        /// Point.
        /// </summary>
        Point = 1,

        /// <summary>
        /// LineString.
        /// </summary>
        LineString = 2,

        /// <summary>
        /// Polygon.
        /// </summary>
        Polygon = 3,

        /// <summary>
        /// MultiPoint.
        /// </summary>
        MultiPoint = 4,

        /// <summary>
        /// MultiLineString.
        /// </summary>
        MultiLineString = 5,

        /// <summary>
        /// MultiPolygon.
        /// </summary>
        MultiPolygon = 6,

        /// <summary>
        /// GeometryCollection.
        /// </summary>
        GeometryCollection = 7,

        /// <summary>
        /// CircularString
        /// </summary>
        CircularString = 8,

        /// <summary>
        /// CompoundCurve
        /// </summary>
        CompoundCurve = 9,

        /// <summary>
        /// CurvePolygon
        /// </summary>
        CurvePolygon = 10,

        /// <summary>
        /// MultiCurve
        /// </summary>
        MultiCurve = 11,

        /// <summary>
        /// MultiSurface
        /// </summary>
        MultiSurface = 12,

        /// <summary>
        /// PolyhedralSurface
        /// </summary>
        PolyhedralSurface = 13,

        /// <summary>
        /// Triangle
        /// </summary>
        Triangle = 14,

        /// <summary>
        /// TIN
        /// </summary>
        // ReSharper disable InconsistentNaming
        TIN = 15,

        /// <summary>
        /// NUM
        /// </summary>
        NUM = 16
        // ReSharper restore InconsistentNaming
    }

    internal static class PostGis2Utility
    {
        public static IEnumerable<ICoordinateSequence> GetSequences(this IGeometry self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            switch (self.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    yield return ((IPoint)self).CoordinateSequence;
                    break;
                case OgcGeometryType.LineString:
                    yield return ((IPoint)self).CoordinateSequence;
                    break;
                case OgcGeometryType.Polygon:
                    yield return ((IPoint)self).CoordinateSequence;
                    break;
                default:
                    for (var i = 0; i < self.NumGeometries; i++ )
                    {
                        foreach (var cs in self.GetGeometryN(i).GetSequences())
                        {
                            yield return cs;
                        }
                    }
                    break;
            }
        }
        public static ICoordinateSequence GetSequence(this IGeometry self)
        {
            if (self == null)
                throw new ArgumentNullException("self");

            switch (self.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    return ((IPoint)self).CoordinateSequence;
                case OgcGeometryType.LineString:
                    return ((IPoint)self).CoordinateSequence;
                case OgcGeometryType.Polygon:
                    return ((IPoint)self).CoordinateSequence;
                default:
                    return GetSequence(self.GetGeometryN(0));
            }
        }
        
        public static bool HasZ(this IGeometry geometry)
        {
            return (geometry.GetSequence().Ordinates | Ordinates.Z) == Ordinates.Z;
        }

        public static bool HasM(this IGeometry geometry)
        {
            return (geometry.GetSequence().Ordinates | Ordinates.M) == Ordinates.M;
        }

        public static double[] GetZRange(this IGeometry self)
        {
            var res = new[] {double.MaxValue, double.MinValue};
            return GetRange(self, Ordinate.Z, res);
        }

        private static double[] GetRange(this IGeometry self, Ordinate ordinate, double[] minmax)
        {
            foreach (var sequence in self.GetSequences())
            {
                for (var i = 0; i < sequence.Count; i++)
                {
                    var ordinateValue = sequence.GetOrdinate(i, ordinate);
                    minmax[0] = minmax[0] < ordinateValue ? minmax[0] : ordinateValue;
                    minmax[1] = minmax[1] > ordinateValue ? minmax[1] : ordinateValue;
                }
            }
            return minmax;
        }

        public static double[] GetMRange(this IGeometry self)
        {
            var res = new[] { double.MaxValue, double.MinValue };
            return GetRange(self, Ordinate.M, res);
        }

        public static OgcGeometryType ToOgc(this PostGis2GeometryType self)
        {
            switch (self)
            {
                case PostGis2GeometryType.Point:
                case PostGis2GeometryType.LineString:
                case PostGis2GeometryType.Polygon:
                case PostGis2GeometryType.MultiPoint:
                case PostGis2GeometryType.MultiLineString:
                case PostGis2GeometryType.MultiPolygon:
                case PostGis2GeometryType.GeometryCollection:
                case PostGis2GeometryType.CircularString:
                case PostGis2GeometryType.CompoundCurve:
                case PostGis2GeometryType.MultiCurve:
                case PostGis2GeometryType.MultiSurface:
                    return (OgcGeometryType) self;

                case PostGis2GeometryType.PolyhedralSurface:
                    return OgcGeometryType.PolyhedralSurface;
                case PostGis2GeometryType.TIN:
                    return OgcGeometryType.TIN;
                
                default:
                    throw new InvalidOperationException();
            }
        }

        public static PostGis2GeometryType ToPostGis2(this OgcGeometryType self)
        {
            switch (self)
            {
                case OgcGeometryType.Point:
                case OgcGeometryType.LineString:
                case OgcGeometryType.Polygon:
                case OgcGeometryType.MultiPoint:
                case OgcGeometryType.MultiLineString:
                case OgcGeometryType.MultiPolygon:
                case OgcGeometryType.GeometryCollection:
                case OgcGeometryType.CircularString:
                case OgcGeometryType.CompoundCurve:
                case OgcGeometryType.MultiCurve:
                case OgcGeometryType.MultiSurface:
                    return (PostGis2GeometryType)self;

                case OgcGeometryType.PolyhedralSurface:
                    return PostGis2GeometryType.PolyhedralSurface;
                case OgcGeometryType.TIN:
                    return PostGis2GeometryType.TIN;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
