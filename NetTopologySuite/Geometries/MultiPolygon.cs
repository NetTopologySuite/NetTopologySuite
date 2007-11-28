using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
    [Serializable]
    public class MultiPolygon<TCoordinate> : GeometryCollection<TCoordinate>, IMultiPolygon<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <c>MultiPolygon</c>.
        /// </summary>
        public new static readonly IMultiPolygon<TCoordinate> Empty = new GeometryFactory<TCoordinate>().CreateMultiPolygon(null);

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <see cref="Polygon{TCoordinate}" />s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Polygon{TCoordinate}" />s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPolygon(params IPolygon<TCoordinate>[] polygons) : this(polygons, DefaultFactory) {}

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <see cref="Polygon{TCoordinate}" />s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Polygon{TCoordinate}" />s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        public MultiPolygon(IEnumerable<IPolygon<TCoordinate>> polygons, IGeometryFactory<TCoordinate> factory) : base(polygons, factory) {}

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