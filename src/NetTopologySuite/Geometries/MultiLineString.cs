using NetTopologySuite.Operation;

namespace NetTopologySuite.Geometries
{
    using System;

    /// <summary>
    /// Models a collection of <see cref="LineString"/>s.
    /// <para/>
    /// Any collection of <c>LineString</c>s is a valid <c>MultiLineString</c>.
    /// </summary>
    [Serializable]
    public class MultiLineString : GeometryCollection, ILineal
    {
        /// <summary>
        /// Represents an empty <c>MultiLineString</c>.
        /// </summary>
        public new static readonly MultiLineString Empty = new GeometryFactory().CreateMultiLineString(null);

        /// <summary>
        /// Constructs a <c>MultiLineString</c>.
        /// </summary>
        /// <param name="lineStrings">
        /// The <c>LineString</c>s for this <c>MultiLineString</c>,
        /// or <c>null</c> or an empty array to create the empty
        /// point. Elements may be empty <c>LineString</c>s,
        /// but not <c>null</c>s.
        /// </param>
        /// <param name="factory"></param>
        public MultiLineString(LineString[] lineStrings, GeometryFactory factory)
            : base(lineStrings, factory) { }

        /// <summary>
        /// Constructs a <c>MultiLineString</c>.
        /// </summary>
        /// <param name="lineStrings">
        /// The <c>LineString</c>s for this <c>MultiLineString</c>,
        /// or <c>null</c> or an empty array to create the empty
        /// point. Elements may be empty <c>LineString</c>s,
        /// but not <c>null</c>s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiLineString(LineString[] lineStrings) : this(lineStrings, DefaultFactory) { }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.MultiLineString;

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public override Dimension Dimension => Dimension.Curve;

        /// <inheritdoc cref="Geometry.HasDimension(Dimension)"/>
        /// <returns><c>true</c> if <paramref name="dim"/> == <c><see cref="Dimension.Curve"/></c></returns>
        public override bool HasDimension(Dimension dim)
        {
            return dim == Dimension.Curve;
        }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                    return Dimension.False;
                return Dimension.Point;
            }
        }

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"MultiLineString"</returns>
        public override string GeometryType => Geometry.TypeNameMultiLineString;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiLineString;

        /// <summary>
        /// Gets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed
        {
            get
            {
                if (IsEmpty)
                    return false;
                for (int i = 0; i < Geometries.Length; i++)
                    if (!((LineString) Geometries[i]).IsClosed)
                        return false;
                return true;
            }
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <value></value>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return (new IsSimpleOp()).IsSimple((MultiLineString) this);
        //    }
        //}

        public override Geometry Boundary => (new BoundaryOp(this)).GetBoundary();
        //{
        //    get
        //    {
        //        if(IsEmpty)
        //            return Factory.CreateGeometryCollection(null);
        //        GeometryGraph g = new GeometryGraph(0, this);
        //        Coordinate[] pts = g.GetBoundaryPoints();
        //        return Factory.CreateMultiPoint(pts);
        //    }
        //}

        /// <summary>
        /// Creates a <see cref="MultiLineString" /> in the reverse order to this object.
        /// Both the order of the component LineStrings
        /// and the order of their coordinate sequences are reversed.
        /// </summary>
        /// <returns>a <see cref="MultiLineString" /> in the reverse order.</returns>
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
            var lineStrings = new LineString[Geometries.Length];
            for (int i = 0; i < lineStrings.Length; i++)
            {
                lineStrings[i] = (LineString)Geometries[i].Reverse();
            }
            return new MultiLineString(lineStrings, Factory);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            var lineStrings = new LineString[NumGeometries];
            for (int i = 0; i < lineStrings.Length; i++)
                lineStrings[i] = (LineString)GetGeometryN(i).Copy();

            return new MultiLineString(lineStrings, Factory);
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
