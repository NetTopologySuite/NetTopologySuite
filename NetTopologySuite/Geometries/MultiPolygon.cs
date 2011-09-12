using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
#region geoapi vs nts
#if useFullGeoAPI
using ICoordinate = GeoAPI.Geometries.ICoordinate;
using IGeometry = GeoAPI.Geometries.IGeometry;
using IPoint = GeoAPI.Geometries.IPoint;
using ILineString = GeoAPI.Geometries.ILineString;
using ILinearRing = GeoAPI.Geometries.ILinearRing;
using IPolygon = GeoAPI.Geometries.IPolygon;
using IGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
using IMultiPoint = GeoAPI.Geometries.IMultiPoint;
using IMultiLineString = GeoAPI.Geometries.IMultiLineString;
using IMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
#endif
#endregion

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    public class MultiPolygon : GeometryCollection
#if useFullGeoAPI
        , IMultiPolygon 
#endif
    {
        /// <summary>
        /// Represents an empty <c>MultiPolygon</c>.
        /// </summary>
        public static new readonly IMultiPolygon Empty = new GeometryFactory().CreateMultiPolygon(null);

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <c>Polygon</c>s for this <c>MultiPolygon</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Polygon</c>s, but not <c>null</c>
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPolygon(IPolygon[] polygons) : this(polygons, DefaultFactory) { }  

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <c>Polygon</c>s for this <c>MultiPolygon</c>
        /// , or <c>null</c> or an empty array to create the empty point.
        /// Elements may be empty <c>Polygon</c>s, but not <c>null</c>
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <param name="factory"></param>
        public MultiPolygon(IPolygon[] polygons, IGeometryFactory factory) : base(polygons, factory) { }  

        /// <summary>
        /// 
        /// </summary>
        public override Dimension Dimension
        {
            get
            {
                return Dimension.Surface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimension BoundaryDimension
        {
            get
            {
                return Dimension.Curve;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GeometryType
        {
            get
            {
                return "MultiPolygon";
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)    
                    return Factory.CreateMultiPolygon(null);

                List<ILineString> allRings = new List<ILineString>();
                for (int i = 0; i < Geometries.Length; i++)
                {
                    IPolygon polygon = (IPolygon) Geometries[i];
                    IGeometry rings = polygon.Boundary;
                    for (int j = 0; j < rings.NumGeometries; j++)
                        allRings.Add((ILineString) rings.GetGeometryN(j));
                }                
                return Factory.CreateMultiLineString(allRings.ToArray());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(IGeometry other, double tolerance) 
        {
            if (!IsEquivalentClass(other)) 
                return false;
            return base.EqualsExact(other, tolerance);
        }

        ///<summary>Creates a {@link MultiPolygon} with every component reversed.
        ///</summary>
        /// <remarks>The order of the components in the collection are not reversed.</remarks>
        /// <returns>An <see cref="IMultiPolygon"/> in the reverse order</returns>
        public override IGeometry Reverse()
        {
            int n = Geometries.Length;
            IPolygon[] revGeoms = new IPolygon[n];
            for (int i = 0; i < Geometries.Length; i++)
            {
                revGeoms[i] = (Polygon)Geometries[i].Reverse();
            }
            return Factory.CreateMultiPolygon(revGeoms);
        }

    }
}
