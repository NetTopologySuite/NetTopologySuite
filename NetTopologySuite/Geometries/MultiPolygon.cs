using System;
using System.Collections.Generic;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>MultiPolygon</c>.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
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

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override IGeometry CopyInternal()
        {
            var polygons = new IPolygon[NumGeometries];
            for (int i = 0; i < polygons.Length; i++)
                polygons[i] = (IPolygon)GetGeometryN(i).Copy();

            return new MultiPolygon(polygons, Factory);
        }
        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
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
        public override string GeometryType => "MultiPolygon";

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
        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreateMultiLineString();

                var allRings = new List<ILineString>();
                for (int i = 0; i < Geometries.Length; i++)
                {
                    var polygon = (IPolygon) Geometries[i];
                    var rings = polygon.Boundary;
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
            var revGeoms = new IPolygon[n];
            for (int i = 0; i < Geometries.Length; i++)
            {
                revGeoms[i] = (Polygon)Geometries[i].Reverse();
            }
            return Factory.CreateMultiPolygon(revGeoms);
        }

    }
}
