using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="GeometryCollection{TCoordinate}" />.
    /// </summary>
    [Serializable]
    public class GeometryCollection<TCoordinate> : Geometry<TCoordinate>, IGeometryCollection<TCoordinate>, IHasGeometryComponents<TCoordinate>, IComparable<IGeometryCollection<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<Double, TCoordinate>, IConvertible
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

        //public GeometryCollection() : this(DefaultFactory) { }

        public GeometryCollection(IGeometryFactory<TCoordinate> factory) : base(factory) { }

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
        public GeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries) : this(geometries, ExtractGeometryFactory(geometries)) { }

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <see cref="GeometryCollection{TCoordinate}" />,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        public GeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            if (geometries == null)
            {
                geometries = new IGeometry<TCoordinate>[] { };
            }

            _geometries.AddRange(geometries);
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
            Extents<TCoordinate> extents = new Extents<TCoordinate>();

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

        public Int32 IndexOf(IGeometry item)
        {
            throw new NotImplementedException();
        }

        public void Insert(Int32 index, IGeometry item)
        {
            throw new NotImplementedException();
        }

        #region ICollection<IGeometry> Members

        public void Add(IGeometry item)
        {
            throw new NotImplementedException();
        }

        #region IEnumerable<IGeometry> Members

        /// <summary>
        /// Returns a <see cref="GeometryCollectionEnumerator{TCoordinate}"/>:
        /// this IEnumerator returns the parent geometry as first element.
        /// </summary>
        public IEnumerator<IGeometry<TCoordinate>> GetEnumerator()
        {
            return new GeometryCollectionEnumerator<TCoordinate>(this);
        }

        #endregion

        #region IList<IGeometry<TCoordinate>> Members

        public Int32 IndexOf(IGeometry<TCoordinate> item)
        {
            return _geometries.IndexOf(item);
        }

        void IList<IGeometry<TCoordinate>>.Insert(Int32 index, IGeometry<TCoordinate> item)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        void IList<IGeometry<TCoordinate>>.RemoveAt(Int32 index)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        void IList<IGeometry>.RemoveAt(Int32 index)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        #endregion

        #endregion

        #region ICollection<IGeometry<TCoordinate>> Members

        void ICollection<IGeometry<TCoordinate>>.Add(IGeometry<TCoordinate> item)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        void ICollection<IGeometry<TCoordinate>>.Clear()
        {
            throw new NotSupportedException("Collection is read only.");
        }

        void ICollection<IGeometry>.Clear()
        {
            throw new NotSupportedException("Collection is read only.");
        }

        bool ICollection<IGeometry>.Contains(IGeometry item)
        {
            return Contains(item as IGeometry<TCoordinate>);
        }

        void ICollection<IGeometry>.CopyTo(IGeometry[] array, Int32 arrayIndex)
        {
            // TODO: implement
            throw new NotImplementedException();
        }

        bool ICollection<IGeometry>.Remove(IGeometry item)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        bool ICollection<IGeometry>.IsReadOnly
        {
            get { return true; }
        }

        #endregion

        public void CopyTo(IGeometry<TCoordinate>[] array, Int32 arrayIndex)
        {
            _geometries.CopyTo(array, arrayIndex);
        }

        bool ICollection<IGeometry<TCoordinate>>.Remove(IGeometry<TCoordinate> item)
        {
            throw new NotSupportedException("Collection is read only.");
        }

        public bool IsReadOnly
        {
            get { return true; }
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
    }
}