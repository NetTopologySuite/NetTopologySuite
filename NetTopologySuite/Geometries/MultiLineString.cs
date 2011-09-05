using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Operation;
using NPack.Interfaces;

#if DOTNET35
using sl = System.Linq;
#else
using sl = GeoAPI.DataStructures;
#endif

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="IMultiLineString{TCoordinate}"/>.
    /// </summary>    
    public class MultiLineString<TCoordinate>
        : GeometryCollection<TCoordinate>, IMultiLineString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComputable<Double, TCoordinate>,
            IComparable<TCoordinate>, IConvertible

    {
        ///// <summary>
        ///// Represents an empty <see cref="MultiLineString{TCoordinate}"/>.
        ///// </summary>
        //public new static readonly IMultiLineString Empty = new GeometryFactory<TCoordinate>().CreateMultiLineString(null);

        /// <summary>
        /// Constructs a <see cref="MultiLineString{TCoordinate}"/>.
        /// </summary>
        /// <param name="lineStrings">
        /// The <see cref="LineString{TCoordinate}"/>s for this 
        /// <see cref="MultiLineString{TCoordinate}"/>,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="LineString{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        public MultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings,
                               IGeometryFactory<TCoordinate> factory)
            : base(lineStrings == null 
/* C# syntax ->  */
                       ? null 
/* can be clumsy */
                       : Caster.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(lineStrings),
                   factory)
        {
        }

        /// <summary>
        /// Constructs an empty <see cref="MultiLineString{TCoordinate}"/>.
        /// </summary>
        public MultiLineString(IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
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

                    if (!curve.IsClosed)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        ///// <summary>
        ///// Constructs a <c>MultiLineString</c>.
        ///// </summary>
        ///// <param name="lineStrings">
        ///// The <c>LineString</c>s for this <c>MultiLineString</c>,
        ///// or <see langword="null" /> or an empty array to create the empty
        ///// point. Elements may be empty <c>LineString</c>s,
        ///// but not <see langword="null" />s.
        ///// </param>
        ///// <remarks>
        ///// For create this <see cref="Geometry{TCoordinate}"/> 
        ///// is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        ///// with <see cref="IPrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        ///// </remarks>
        //public MultiLineString(IEnumerable<ILineString<TCoordinate>> lineStrings)
        //    : this(lineStrings, 
        //           ExtractGeometryFactory(Caster.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(lineStrings))) { }

        #region IMultiLineString<TCoordinate> Members

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                return new BoundaryOp<TCoordinate>(this).GetBoundary();
                //if (IsEmpty)
                //{
                //    return Factory.CreateGeometryCollection(null);
                //}

                //GeometryGraph<TCoordinate> g = new GeometryGraph<TCoordinate>(0, this);
                //IEnumerable<TCoordinate> pts = g.GetBoundaryPoints();
                //return Factory.CreateMultiPoint(pts);
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

        public override IGeometry<TCoordinate> Clone()
        {
            List<ILineString<TCoordinate>> lineStrings = new List<ILineString<TCoordinate>>();

            foreach (ILineString<TCoordinate> lineString in this)
            {
                lineStrings.Add(lineString.Clone() as ILineString<TCoordinate>);
            }

            return Factory.CreateMultiLineString(lineStrings);
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Curve; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.MultiLineString; }
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
            IEnumerable<ILineString<TCoordinate>> reversed
                = Reverse(sl.Enumerable.Reverse(this as IEnumerable<ILineString<TCoordinate>>));
            return Factory.CreateMultiLineString(reversed);
        }

        private static IEnumerable<ILineString<TCoordinate>> Reverse(IEnumerable<ILineString<TCoordinate>> input)
        {
            foreach (var lineString in input)
                yield return lineString.Reverse();
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
            get { return this[index]; }
            set { this[index] = value as ILineString<TCoordinate>; }
        }

        #endregion

        protected override void CheckItemType(IGeometry<TCoordinate> item)
        {
            if (!(item is ILineString<TCoordinate>))
            {
                throw new InvalidOperationException(String.Format(
                                                        "Cannot add geometry of type {0} " +
                                                        "to a MultiLineString",
                                                        item.GetType()));
            }
        }
    }
}