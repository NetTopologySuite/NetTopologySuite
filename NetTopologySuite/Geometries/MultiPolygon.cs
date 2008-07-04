using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
    [Serializable]
    public class MultiPolygon : GeometryCollection, IMultiPolygon 
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
        public override Dimensions Dimension
        {
            get
            {
                return Dimensions.Surface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions BoundaryDimension
        {
            get
            {
                return Dimensions.Curve;
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

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)    
                    return Factory.CreateGeometryCollection(null);

                List<ILineString> allRings = new List<ILineString>();
                for (int i = 0; i < geometries.Length; i++)
                {
                    IPolygon polygon = (IPolygon) geometries[i];
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
    }
}
