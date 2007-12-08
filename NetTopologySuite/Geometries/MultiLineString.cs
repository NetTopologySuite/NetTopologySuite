using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="IMultiLineString{TCoordinate}"/>.
    /// </summary>    
    public class MultiLineString<TCoordinate> : GeometryCollection<TCoordinate>, IMultiLineString<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<TCoordinate>, IConvertible

    {
        /// <summary>
        /// Represents an empty <see cref="MultiLineString{TCoordinate}"/>.
        /// </summary>
        public new static readonly IMultiLineString Empty = new GeometryFactory<TCoordinate>().CreateMultiLineString(null);

        /// <summary>
        /// Constructs a <c>MultiLineString</c>.
        /// </summary>
        /// <param name="lineStrings">
        /// The <c>LineString</c>s for this <c>MultiLineString</c>,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <c>LineString</c>s,
        /// but not <see langword="null" />s.
        /// </param>
        public MultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings, IGeometryFactory<TCoordinate> factory)
            : base(EnumerableConverter.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(lineStrings), factory) { }

        /// <summary>
        /// Constructs a <c>MultiLineString</c>.
        /// </summary>
        /// <param name="lineStrings">
        /// The <c>LineString</c>s for this <c>MultiLineString</c>,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <c>LineString</c>s,
        /// but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> 
        /// is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings) 
            : this(lineStrings, DefaultFactory) { }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection(null);
                }

                GeometryGraph<TCoordinate> g = new GeometryGraph<TCoordinate>(0, this);
                IEnumerable<TCoordinate> pts = g.GetBoundaryPoints();
                return Factory.CreateMultiPoint(pts);
            }
        }

        public override Dimensions BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimensions.False;
                }

                return Dimensions.Point;
            }
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Curve; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.MultiLineString; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is closed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is closed; otherwise, <see langword="false"/>.
        /// </value>
        public Boolean IsClosed
        {
            get
            {
                if (IsEmpty)
                {
                    return false;
                }

                foreach (ICurve curve in GeometriesInternal)
                {
                    Debug.Assert(curve != null);

                    if(!curve.IsClosed)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp<TCoordinate>()).IsSimple(this); }
        }

        public new ILineString<TCoordinate> this[Int32 index]
        {
            get { return base[index] as ILineString<TCoordinate>; }
            set { base[index] = value; }
        }

        /// <summary>
        /// Creates a <see cref="MultiLineString{TCoordinate}" /> in the reverse order to this object.
        /// Both the order of the component LineStrings
        /// and the order of their coordinate sequences are reversed.
        /// </summary>
        /// <returns>a <see cref="MultiLineString{TCoordinate}" /> in the reverse order.</returns>
        public IMultiLineString<TCoordinate> Reverse()
        {
            IEnumerable<ILineString<TCoordinate>> reversed = Slice.Reverse(this as IEnumerable<ILineString<TCoordinate>>);
            return Factory.CreateMultiLineString(reversed);
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.Equals(other, tolerance);
        }

        public new IEnumerator<ILineString<TCoordinate>> GetEnumerator()
        {
            foreach (ILineString<TCoordinate> line in GeometriesInternal)
            {
                yield return line;
            }
        }

        IEnumerator<ILineString> IEnumerable<ILineString>.GetEnumerator()
        {
            foreach (ILineString<TCoordinate> lineString in this)
            {
                yield return lineString;
            }
        }

        IMultiLineString IMultiLineString.Reverse()
        {
            return Reverse();
        }

        ILineString IMultiLineString.this[Int32 index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value as ILineString<TCoordinate>;
            }
        }
    }
}