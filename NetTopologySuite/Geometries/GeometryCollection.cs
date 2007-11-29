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
    public class GeometryCollection<TCoordinate> : Geometry<TCoordinate>, IGeometryCollection<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>
        public static readonly IGeometryCollection<TCoordinate> Empty = DefaultFactory.CreateGeometryCollection(null);

        /// <summary>
        /// Internal representation of this <see cref="GeometryCollection{TCoordinate}" />.        
        /// </summary>
        private readonly List<IGeometry<TCoordinate>> _geometries = new List<IGeometry<TCoordinate>>();

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <see cref="GeometryCollection{TCoordinate}" />,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public GeometryCollection(IEnumerable<IGeometry<TCoordinate>> geometries) : this(geometries, DefaultFactory) { }

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
        public override IEnumerable<TCoordinate> Coordinates
        {
            get
            {
                ICoordinate[] coordinates = new ICoordinate[NumPoints];
                Int32 k = -1;

                for (Int32 i = 0; i < _geometries.Count; i++)
                {
                    ICoordinate[] childCoordinates = _geometries[i].Coordinates;

                    for (Int32 j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }

                return coordinates;
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

        //public override IGeometry<TCoordinate> this[Int32 index]
        //{
        //    get { return _geometries[index]; }
        //    set { throw new NotSupportedException("GeometryCollection is immutable."); }
        //}

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

        public override void Apply(ICoordinateFilter<TCoordinate> filter)
        {
            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                geometry.Apply(filter);
            }
        }

        public override void Apply(IGeometryFilter<TCoordinate> filter)
        {
            filter.Filter(this);

            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                geometry.Apply(filter);
            }
        }

        public override void Apply(IGeometryComponentFilter<TCoordinate> filter)
        {
            filter.Filter(this);

            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                geometry.Apply(filter);
            }
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
            Extents<TCoordinate> extents = new Extents<TCoordinate>();

            foreach (IGeometry<TCoordinate> geometry in _geometries)
            {
                extents.ExpandToInclude(geometry.Extents);
            }

            return extents;
        }

        protected internal override int CompareToSameClass(IGeometry<TCoordinate> other)
        {
            ArrayList theseElements = new ArrayList(geometries);
            ArrayList otherElements = new ArrayList(((GeometryCollection)o).geometries);
            return Compare(theseElements, otherElements);
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
        /// Returns a <c>GeometryCollectionEnumerator</c>:
        /// this IEnumerator returns the parent geometry as first element.
        /// In most cases is more useful the code
        /// <c>geometryCollectionInstance.Geometries.GetEnumerator()</c>: 
        /// this returns an IEnumerator over geometries composing GeometryCollection.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return new GeometryCollectionEnumerator<TCoordinate>(this);
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
                if (index == 0)
                {
                    return this;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("index", index,
                        "Index must be 0.");
                }
            }
            set { throw new NotSupportedException("GeometryCollection is immutable."); }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="GeometryCollection{TCoordinate}" />.
        /// </summary>
        public Int32 Count
        {
            get { return _geometries.Count; }
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}