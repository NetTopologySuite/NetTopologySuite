using System;
using System.Collections;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
    [Serializable]
    public class MultiPolygon : GeometryCollection 
    {
        /// <summary>
        /// Represents an empty <c>MultiPolygon</c>.
        /// </summary>
        public static new readonly MultiPolygon Empty = new GeometryFactory().CreateMultiPolygon(null);

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
        public MultiPolygon(Polygon[] polygons) : this(polygons, DefaultFactory) { }  

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
        public MultiPolygon(Polygon[] polygons, GeometryFactory factory) : base(polygons, factory) { }  

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
        [Obsolete("Use reflection! GetType().Name")]
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
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)    return Factory.CreateGeometryCollection(null);
                ArrayList allRings = new ArrayList();
                for (int i = 0; i < geometries.Length; i++)
                {
                    Polygon polygon = (Polygon)geometries[i];
                    Geometry rings = polygon.Boundary;
                    for (int j = 0; j < rings.NumGeometries; j++)
                        allRings.Add(rings.GetGeometryN(j));
                }
                LineString[] allRingsArray = new LineString[allRings.Count];
                return Factory.CreateMultiLineString((LineString[])allRings.ToArray(typeof(LineString)));                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(Geometry other, double tolerance) 
        {
            if (!IsEquivalentClass(other)) 
                return false;
            return base.EqualsExact(other, tolerance);
        }
    }
}
