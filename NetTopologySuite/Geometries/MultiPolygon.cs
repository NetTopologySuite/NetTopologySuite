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
        public new static readonly IMultiPolygon Empty = new GeometryFactory().CreateMultiPolygon(null);

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <c>Polygon</c>s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Polygon</c>s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="Geometry{TCoordinate}Factory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPolygon(IPolygon[] polygons) : this(polygons, DefaultFactory) {}

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <c>Polygon</c>s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Polygon</c>s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        public MultiPolygon(IPolygon[] polygons, IGeometryFactory factory) : base(polygons, factory) {}

        public override Dimensions Dimension
        {
            get { return Dimensions.Surface; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.Curve; }
        }

        public override string GeometryType
        {
            get { return "MultiPolygon"; }
        }

        public override Boolean IsSimple
        {
            get { return true; }
        }

        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection(null);
                }

                List<ILineString> allRings = new List<ILineString>();
                
                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    IPolygon polygon = (IPolygon) geometries[i];
                    IGeometry rings = polygon.Boundary;
                    for (Int32 j = 0; j < rings.NumGeometries; j++)
                    {
                        allRings.Add((ILineString) rings.GetGeometryN(j));
                    }
                }

                return Factory.CreateMultiLineString(allRings.ToArray());
            }
        }

        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.EqualsExact(other, tolerance);
        }
    }
}