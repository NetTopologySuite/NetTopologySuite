using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Operation;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Models a collection of <c>Point</c>s.
    /// </summary>
    [Serializable]
    public class MultiPoint<TCoordinate> : GeometryCollection<TCoordinate>, IMultiPoint<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <c>MultiPoint</c>.
        /// </summary>
        public new static readonly IMultiPoint Empty = new GeometryFactory<TCoordinate>().CreateMultiPoint();

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <see langword="null" />s.
        /// </param>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points, IGeometryFactory<TCoordinate> factory) 
            : base(points, factory) { }

        /// <summary>
        /// Constructs a <c>MultiPoint</c>.
        /// </summary>
        /// <param name="points">
        /// The <c>Point</c>s for this <c>MultiPoint</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <c>Point</c>s, but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public MultiPoint(IEnumerable<IPoint<TCoordinate>> points) : this(points, DefaultFactory) {}

        public override Dimensions Dimension
        {
            get { return Dimensions.Point; }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.False; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.MultiPoint; }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get { return Factory.CreateGeometryCollection(null); }
        }

        public override Boolean IsSimple
        {
            get { return (new IsSimpleOp<TCoordinate>()).IsSimple(this); }
        }

        public override Boolean IsValid
        {
            get { return true; }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.Equals(other, tolerance);
        }

        /// <summary>
        /// Returns the <c>Coordinate</c> at the given position.
        /// </summary>
        /// <param name="n">The index of the <c>Coordinate</c> to retrieve, beginning at 0.
        /// </param>
        /// <returns>The <c>n</c>th <c>Coordinate</c>.</returns>
        protected TCoordinate GetCoordinate(Int32 n)
        {
            return this[n].Coordinate;
        }

        #region IMultiPoint<TCoordinate> Members

        public IPoint<TCoordinate> this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion

        #region IList<IGeometry> Members

        public int IndexOf(IGeometry item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IGeometry item)
        {
            throw new NotImplementedException();
        }

        #region ICollection<IGeometry> Members

        public void Add(IGeometry item)
        {
            throw new NotImplementedException();
        }

        #region IEnumerable<IGeometry> Members

        IEnumerator<IGeometry> IEnumerable<IGeometry>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGeometryCollection<TCoordinate> Members

        IEnumerator<IGeometry<TCoordinate>> IGeometryCollection<TCoordinate>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGeometry<TCoordinate> Members

        public IList<TCoordinate> Coordinates
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IList<IGeometry<TCoordinate>> Members

        public int IndexOf(IGeometry<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IGeometry<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        void IList<IGeometry>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region ICollection<IGeometry<TCoordinate>> Members

        public void Add(IGeometry<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        void ICollection<IGeometry>.Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(IGeometry item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IGeometry[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IGeometry item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<IGeometry>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public void CopyTo(IGeometry<TCoordinate>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IGeometry<TCoordinate> item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable<IGeometry<TCoordinate>> Members

        public IEnumerator<IGeometry<TCoordinate>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMultiPoint Members

        IPoint IMultiPoint.this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}