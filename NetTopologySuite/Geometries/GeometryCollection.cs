using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;
using GeoAPI.Diagnostics;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="GeometryCollection{TCoordinate}" />.
    /// </summary>
    [Serializable]
    public class GeometryCollection<TCoordinate> 
        : Geometry<TCoordinate>, 
          IGeometryCollection<TCoordinate>, 
          IHasGeometryComponents<TCoordinate>, 
          IComparable<IGeometryCollection<TCoordinate>>
                where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                                    IComputable<Double, TCoordinate>,
                                    IComparable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>
        //public static readonly IGeometryCollection<TCoordinate> Empty = DefaultFactory.CreateGeometryCollection(null);

        public static Boolean HasNonEmptyElements<TGeometry>(IEnumerable<TGeometry> geometries)
            where TGeometry : IGeometry
        {
            foreach (TGeometry geometry in geometries)
            {
                if (!geometry.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Internal representation of this <see cref="GeometryCollection{TCoordinate}" />.        
        /// </summary>
        private readonly List<IGeometry<TCoordinate>> _geometries = new List<IGeometry<TCoordinate>>();

        private Boolean _isFrozen;

        //public GeometryCollection() : this(DefaultFactory) { }

        public GeometryCollection(IGeometryFactory<TCoordinate> factory) 
            : base(factory) { }

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <see cref="GeometryCollection{TCoordinate}" />,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public GeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries) 
            : this(geometries, ExtractGeometryFactory(geometries)) { }

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <see cref="GeometryCollection{TCoordinate}" />,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        public GeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries, 
                                  IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (geometries == null)
            {
                geometries = new IGeometry<TCoordinate>[] { };
            }

            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                _geometries.Add(geometry);
            }
        }

        /// <summary>
        /// Collects all coordinates of all subgeometries into an Array.
        /// Note that while changes to the coordinate objects themselves
        /// may modify the Geometries in place, the returned Array as such 
        /// is only a temporary container which is not synchronized back.
        /// </summary>
        /// <returns>The collected coordinates.</returns>
        public override ICoordinateSequence<TCoordinate> Coordinates
        {
            get
            {
                // TODO: cache this to improve performance?
                ICoordinateSequence<TCoordinate> sequence
                    = Factory.CoordinateSequenceFactory.Create();

                foreach (IGeometry<TCoordinate> geometry in this)
                {
                    sequence.AddSequence(geometry.Coordinates);
                }

                return sequence;
            }
        }

        public override Boolean IsEmpty
        {
            get
            {
                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    if (!geometry.IsEmpty)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public override Dimensions Dimension
        {
            get
            {
                Dimensions dimension = Dimensions.False;

                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    dimension = (Dimensions)Math.Max((Int32)dimension, (Int32)geometry.Dimension);
                }

                return dimension;
            }
        }

        public override Dimensions BoundaryDimension
        {
            get
            {
                Dimensions dimension = Dimensions.False;

                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    dimension = (Dimensions)Math.Max((Int32)dimension, (Int32)geometry.BoundaryDimension);
                }

                return dimension;
            }
        }

        public override Int32 GeometryCount
        {
            get { return _geometries.Count; }
        }

        public IList<IGeometry<TCoordinate>> Geometries
        {
            get { return _geometries.AsReadOnly(); }
        }

        public override Int32 PointCount
        {
            get
            {
                Int32 count = 0;

                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    count += geometry.PointCount;
                }

                return count;
            }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.GeometryCollection; }
        }

        public override Boolean IsRectangle
        {
            get
            {
                return Count == 1
                           ? this[0].IsRectangle
                           : false;
            }
        }

        public override Boolean IsSimple
        {
            get
            {
                CheckNotGeometryCollection(this);
                Assert.ShouldNeverReachHere();
                return false;
            }
        }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                CheckNotGeometryCollection(this);
                Assert.ShouldNeverReachHere();
                return null;
            }
        }

        /// <summary>  
        /// Returns the area of this <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>        
        public override Double Area
        {
            get
            {
                Double area = 0.0;

                foreach (ISurface<TCoordinate> surface in _geometries)
                {
                    if (surface != null)
                    {
                        area += surface.Area;
                    }
                }

                return area;
            }
        }

        /// <summary>  
        /// Returns the length of this <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>        
        public override Double Length
        {
            get
            {
                Double sum = 0.0;

                foreach (ICurve<TCoordinate> curve in _geometries)
                {
                    if (curve != null)
                    {
                        sum += curve.Length;
                    }
                }

                return sum;
            }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            IGeometryCollection<TCoordinate> otherCollection = other as IGeometryCollection<TCoordinate>;

            if (otherCollection == null)
            {
                return false;
            }

            if (Count != otherCollection.Count)
            {
                return false;
            }

            for (Int32 i = 0; i < _geometries.Count; i++)
            {
                if (!_geometries[i].Equals(otherCollection[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public override Boolean EqualsExact(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            if (!IsEquivalentClass(g))
            {
                return false;
            }

            GeometryCollection<TCoordinate> otherCollection 
                = g as GeometryCollection<TCoordinate>;

            Int32 count = Count;

            if (count != otherCollection.Count)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                if (!this[i].EqualsExact(otherCollection[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override IGeometry<TCoordinate> Clone()
        {
            List<IGeometry<TCoordinate>> geometries = new List<IGeometry<TCoordinate>>();

            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                geometries.Add(geometry.Clone());
            }

            return Factory.CreateGeometryCollection(geometries);
        }

        public override void Normalize()
        {
            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                geometry.Normalize();
            }

            _geometries.Sort();
        }

        protected override Extents<TCoordinate> ComputeExtentsInternal()
        {
            Extents<TCoordinate> extents = new Extents<TCoordinate>(Factory);

            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                extents.ExpandToInclude(geometry.Extents);
            }

            return extents;
        }

        protected internal override Int32 CompareToSameClass(IGeometry<TCoordinate> other)
        {
            if (other == null)
            {
                return 1;
            }

            IGeometryCollection<TCoordinate> collection = other as IGeometryCollection<TCoordinate>;
            return CompareTo(collection);
        }

        /// <summary>
        /// Return <see langword="true"/> if all features in collection are of the same type.
        /// </summary>
        public Boolean IsHomogeneous
        {
            get
            {
                if (IsEmpty)
                {
                    return true;
                }

                IGeometry baseGeom = Geometries[0];

                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    if (baseGeom.GeometryType != geometry.GeometryType)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Returns an element Geometry from a GeometryCollection.
        /// </summary>
        /// <param name="index">The index of the geometry element.</param>
        /// <returns>
        /// The geometry contained in this geometry at the given 
        /// <paramref name="index"/>.
        /// </returns>
        public virtual IGeometry<TCoordinate> this[Int32 index]
        {
            get
            {
                if (index < 0 || index > _geometries.Count)
                {
                    throw new ArgumentOutOfRangeException("index", index,
                        "Index must be 0 or greater and less than Count.");
                }

                return _geometries[index];
            }
            set { throw new NotSupportedException(GetType() + " is immutable."); }
        }

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>
        public Int32 Count
        {
            get { return _geometries.Count; }
        }

        #region IHasGeometryComponents<TCoordinate> Members

        public IEnumerable<IGeometry<TCoordinate>> Components
        {
            get
            {
                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    foreach (IGeometry<TCoordinate> component in enumerateComponents(geometry))
                    {
                        yield return component;
                    }
                }
            }
        }

        #endregion

        #region IList<IGeometry> Members

        /// <summary>
        /// Finds the index of a geometry is in the list.
        /// </summary>
        /// <param name="item">
        /// The <see cref="IGeometry"/> to find the index of.
        /// </param>
        /// <returns>
        /// The index of the geometry if it is in the list,
        /// <value>-1</value> otherwise.
        /// </returns>
        /// <remarks>
        /// The <paramref name="item"/> is treated as an 
        /// <see cref="IGeometry{TCoordinate}"/> for purposes of 
        /// membership testing. If it isn't, this method returns 
        /// <value>-1</value>.
        /// </remarks>
        Int32 IList<IGeometry>.IndexOf(IGeometry item)
        {
            return IndexOf(item as IGeometry<TCoordinate>);
        }

        void IList<IGeometry>.Insert(Int32 index, IGeometry item)
        {
            Insert(index, item as IGeometry<TCoordinate>);
        }

        #region ICollection<IGeometry> Members

        void ICollection<IGeometry>.Add(IGeometry item)
        {
            Add(item as IGeometry<TCoordinate>);
        }

        /// <summary>
        /// Determines if a geometry is present in the collection.
        /// </summary>
        /// <param name="item">
        /// The <see cref="IGeometry"/> to check.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the geometry is in the collection,
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// The <paramref name="item"/> is treated as an 
        /// <see cref="IGeometry{TCoordinate}"/> for purposes of 
        /// membership testing. If it isn't, this method returns 
        /// <see langword="false"/>.
        /// </remarks>
        Boolean ICollection<IGeometry>.Contains(IGeometry item)
        {
            return Contains(item as IGeometry<TCoordinate>);
        }

        /// <summary>
        /// Removes a geometry from the collection if is present in the collection.
        /// </summary>
        /// <param name="item">
        /// The <see cref="IGeometry"/> to remove.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the geometry was removed from the collection,
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// The <paramref name="item"/> is treated as an 
        /// <see cref="IGeometry{TCoordinate}"/> for purposes of 
        /// membership testing. If it isn't, this method returns 
        /// <see langword="false"/>.
        /// </remarks>
        Boolean ICollection<IGeometry>.Remove(IGeometry item)
        {
            return Remove(item as IGeometry<TCoordinate>);
        }

        #endregion

        #region IEnumerable<IGeometry> Members

        /// <summary>
        /// Returns a <see cref="GeometryCollectionEnumerator{TCoordinate}"/>:
        /// this IEnumerator returns the parent geometry as first element.
        /// </summary>
        public IEnumerator<IGeometry<TCoordinate>> GetEnumerator()
        {
            if (_geometries != null)
            {
                foreach (IGeometry<TCoordinate> geometry in _geometries)
                {
                    yield return geometry;
                }
            }
        }

        #endregion

        #region IList<IGeometry<TCoordinate>> Members

        public Int32 IndexOf(IGeometry<TCoordinate> item)
        {
            return _geometries.IndexOf(item);
        }

        public void Insert(Int32 index, IGeometry<TCoordinate> item)
        {
            if (item == null) throw new ArgumentNullException("item");
            CheckFrozen();
            CheckItemType(item);
            _geometries.Insert(index, item);
        }

        public void RemoveAt(Int32 index)
        {
            CheckFrozen();
            _geometries.RemoveAt(index);
        }

        #endregion

        #region ICollection<IGeometry<TCoordinate>> Members

        public virtual void Add(IGeometry<TCoordinate> item)
        {
            if (item == null) throw new ArgumentNullException("item");
            CheckFrozen();
            CheckItemType(item);
            _geometries.Add(item);
        }

        public void Clear()
        {
            CheckFrozen();
            _geometries.Clear();
        }

        void ICollection<IGeometry>.CopyTo(IGeometry[] array, Int32 arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException("arrayIndex");
            
            if (arrayIndex >= array.Length)
            {
                throw new ArgumentException("arrayIndex is greater than array length.");
            }

            if (_geometries.Count > array.Length - arrayIndex)
            {
                throw new ArgumentException(
                    "There is not enough room betwen 'arrayIndex' and the end of 'array'"+
                    " to contain the items in this collection.");
            }

            for (Int32 i = 0; i < _geometries.Count; i++)
            {
                array[i + arrayIndex] = _geometries[i];
            }
        }

        public Boolean Remove(IGeometry<TCoordinate> item)
        {
            CheckFrozen();
            return _geometries.Remove(item);
        }

        #endregion

        public void CopyTo(IGeometry<TCoordinate>[] array, Int32 arrayIndex)
        {
            _geometries.CopyTo(array, arrayIndex);
        }

        public Boolean IsReadOnly
        {
            get { return _isFrozen; }
        }

        IGeometry IList<IGeometry>.this[Int32 index]
        {
            get { return this[index]; }
            set { this[index] = value as IGeometry<TCoordinate>; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IEnumerable<IGeometry> Members

        IEnumerator<IGeometry> IEnumerable<IGeometry>.GetEnumerator()
        {
            foreach (IGeometry<TCoordinate> geometry in this)
            {
                yield return geometry;
            }
        }

        #endregion

        #region IComparable<IGeometryCollection<TCoordinate>> Members

        public Int32 CompareTo(IGeometryCollection<TCoordinate> other)
        {
            if (other == null)
            {
                return 1;
            }

            IEnumerator<IGeometry<TCoordinate>> i = GetEnumerator();
            IEnumerator<IGeometry<TCoordinate>> j = other.GetEnumerator();

            Boolean iHasNext = i.MoveNext();
            Boolean jHasNext = j.MoveNext();

            while (iHasNext && jHasNext)
            {
                int comparison = i.Current.CompareTo(j.Current);

                if (comparison != 0)
                {
                    return comparison;
                }
            }

            if (iHasNext)
            {
                return 1;
            }

            if (jHasNext)
            {
                return -1;
            }

            return 0;
        }

        #endregion

        protected void CheckFrozen()
        {
            if (_isFrozen)
            {
                throw new InvalidOperationException("The geometry is read-only");
            }
        }

        protected virtual void CheckItemType(IGeometry<TCoordinate> item) { }

        protected List<IGeometry<TCoordinate>> GeometriesInternal
        {
            get { return _geometries; }
        }

        private static IEnumerable<IGeometry<TCoordinate>> enumerateComponents(IGeometry<TCoordinate> geometry)
        {
            if (geometry is IHasGeometryComponents<TCoordinate>)
            {
                IHasGeometryComponents<TCoordinate> container =
                    geometry as IHasGeometryComponents<TCoordinate>;

                foreach (IGeometry<TCoordinate> component in container.Components)
                {
                    // avoid a recursion call (and another object on the heap), if possible
                    if (component is IHasGeometryComponents<TCoordinate>)
                    {
                        foreach (IGeometry<TCoordinate> childComponent in enumerateComponents(geometry))
                        {
                            yield return childComponent;
                        }
                    }
                    else
                    {
                        yield return component;
                    }
                }
            }
        }

        /*
         * [codekaizen 2008-01-14] removed when replaced visitor patterns with
         *                         enumeration / query patterns
         */

        //public override void Apply(ICoordinateFilter<TCoordinate> filter)
        //{
        //    foreach (IGeometry<TCoordinate> geometry in _geometries)
        //    {
        //        geometry.Apply(filter);
        //    }
        //}

        //public override void Apply(IGeometryFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);

        //    foreach (IGeometry<TCoordinate> geometry in _geometries)
        //    {
        //        geometry.Apply(filter);
        //    }
        //}

        //public void Apply(IGeometryComponentFilter<TCoordinate> filter)
        //{
        //    filter.Filter(this);

        //    foreach (IGeometry<TCoordinate> geometry in _geometries)
        //    {
        //        geometry.Apply(filter);
        //    }
        //}

    }
}