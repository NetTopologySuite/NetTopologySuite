using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
    [Serializable]
    public class MultiPolygon : GeometryCollection, IPolygonal
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

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            var polygons = new Polygon[NumGeometries];
            for (int i = 0; i < polygons.Length; i++)
                polygons[i] = (Polygon)GetGeometryN(i).Copy();

            return new MultiPolygon(polygons, Factory);
        }
        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.MultiPolygon;

        /// <summary>
        ///
        /// </summary>
        public override Dimension Dimension => Dimension.Surface;

        /// <summary>
        ///
        /// </summary>
        public override Dimension BoundaryDimension => Dimension.Curve;

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"MultiPolygon"</returns>
        public override string GeometryType => Geometry.TypeNameMultiPolygon;

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiPolygon;

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
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreateMultiLineString();

                var allRings = new List<LineString>();
                for (int i = 0; i < Geometries.Length; i++)
                {
                    var polygon = (Polygon) Geometries[i];
                    var rings = polygon.Boundary;
                    for (int j = 0; j < rings.NumGeometries; j++)
                        allRings.Add((LineString) rings.GetGeometryN(j));
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
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
        }

        /// <summary>
        /// Creates a <see cref="MultiPolygon"/> with every component reversed.
        /// </summary>
        /// <remarks>The order of the components in the collection are not reversed.</remarks>
        /// <returns>An <see cref="MultiPolygon"/> in the reverse order</returns>
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <inheritdoc cref="ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var polygons = new Polygon[Geometries.Length];
            for (int i = 0; i < polygons.Length; i++)
            {
                polygons[i] = (Polygon)Geometries[i].Reverse();
            }
            return new MultiPolygon(polygons, Factory);
        }
    }
}
