using System;
using System.Collections.Generic;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>GeometryCollection</c>.
    /// </summary>
    [Serializable]
    public class GeometryCollection : Geometry, IReadOnlyList<Geometry>
    {
        /// <summary>
        /// Represents an empty <c>GeometryCollection</c>.
        /// </summary>
        public static readonly GeometryCollection Empty = DefaultFactory.CreateGeometryCollection(null);

        /// <summary>
        /// Internal representation of this <c>GeometryCollection</c>.
        /// </summary>
        private Geometry[] _geometries;

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometries">
        /// The <c>Geometry</c>s for this <c>GeometryCollection</c>,
        /// or <c>null</c> or an empty array to create the empty
        /// point. Elements may be empty <c>Geometry</c>s,
        /// but not <c>null</c>s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/>
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public GeometryCollection(Geometry[] geometries) : this(geometries, DefaultFactory) { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometries">
        /// The <c>Geometry</c>s for this <c>GeometryCollection</c>,
        /// or <c>null</c> or an empty array to create the empty
        /// point. Elements may be empty <c>Geometry</c>s,
        /// but not <c>null</c>s.
        /// </param>
        /// <param name="factory"></param>
        public GeometryCollection(Geometry[] geometries, GeometryFactory factory) : base(factory)
        {
            if (geometries == null)
                geometries = new Geometry[] { };
            if (HasNullElements<Geometry>(geometries))
                throw new ArgumentException("geometries must not contain null elements");
            _geometries = geometries;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.GeometryCollection;

        /// <inheritdoc/>
        public override Coordinate Coordinate
        {
            get
            {
                for (int i = 0; i < _geometries.Length; i++)
                {
                    if (!_geometries[i].IsEmpty)
                    {
                        return _geometries[i].Coordinate;
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Collects all coordinates of all subgeometries into an Array.
        /// Note that while changes to the coordinate objects themselves
        /// may modify the Geometries in place, the returned Array as such
        /// is only a temporary container which is not synchronized back.
        /// </summary>
        /// <returns>The collected coordinates.</returns>
        public override Coordinate[] Coordinates
        {
            get
            {
                var coordinates = new Coordinate[NumPoints];
                int k = -1;
                for (int i = 0; i < _geometries.Length; i++)
                {
                    var childCoordinates = _geometries[i].Coordinates;
                    for (int j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }
                return coordinates;
            }
        }

        /// <inheritdoc cref="Geometry.GetOrdinates"/>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty)
                return new double[0];

            double[] result = new double[NumPoints];
            int offset = 0;
            for (int i = 0; i < NumGeometries; i++)
            {
                var geom = GetGeometryN(i);
                double[] ordinates = geom.GetOrdinates(ordinate);
                Array.Copy(ordinates, 0, result, offset, ordinates.Length);
                offset += ordinates.Length;
            }
            return result;
        }

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty
        {
            get
            {
                for (int i = 0; i < _geometries.Length; i++)
                    if (!_geometries[i].IsEmpty)
                        return false;
                return true;
            }
        }

        /// <inheritdoc cref="Geometry.Dimension"/>
        public override Dimension Dimension
        {
            get
            {
                var dimension = Dimension.False;
                for (int i = 0; i < _geometries.Length; i++)
                    dimension = (Dimension) Math.Max((int)dimension, (int)_geometries[i].Dimension);
                return dimension;
            }
        }

        /// <inheritdoc cref="Geometry.HasDimension(Dimension)"/>
        public override bool HasDimension(Dimension dim)
        {
            for (int i = 0; i < _geometries.Length; i++)
            {
                if (_geometries[i].HasDimension(dim))
                    return true;
            }
            return false;
        }

        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
        public override Dimension BoundaryDimension
        {
            get
            {
                var dimension = Dimension.False;
                for (int i = 0; i < _geometries.Length; i++)
                    dimension = (Dimension) Math.Max((int) dimension, (int) (_geometries[i].BoundaryDimension));
                return dimension;
            }
        }

        /// <inheritdoc cref="Geometry.NumGeometries"/>
        public override int NumGeometries => _geometries.Length;

        /// <inheritdoc cref="Geometry.GetGeometryN(int)"/>
        public override Geometry GetGeometryN(int n)
        {
            return _geometries[n];
        }

        /// <summary>
        ///
        /// </summary>
        public Geometry[] Geometries
        {
            get => _geometries;
            protected set => _geometries = value;
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints
        {
            get
            {
                int numPoints = 0;
                for (int i = 0; i < _geometries.Length; i++)
                    numPoints += _geometries[i].NumPoints;
                return numPoints;
            }
        }

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"GeometryCollection"</returns>
        public override string GeometryType => Geometry.TypeNameGeometryCollection;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.GeometryCollection;

        ///// <summary>
        /////
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        CheckNotGeometryCollection(this);
        //        Assert.ShouldNeverReachHere();
        //        return false;
        //    }
        //}

        /// <inheritdoc cref="Geometry.Boundary"/>
        public override Geometry Boundary
        {
            get
            {
                CheckNotGeometryCollection(this);
                Assert.ShouldNeverReachHere();
                return null;
            }
        }

        /// <summary>
        /// Returns the area of this <c>GeometryCollection</c>.
        /// </summary>
        public override double Area
        {
            get
            {
                double area = 0.0;
                for (int i = 0; i < _geometries.Length; i++)
                    area += _geometries[i].Area;
                return area;
            }
        }

        /// <summary>
        /// Returns the length of this <c>GeometryCollection</c>.
        /// </summary>
        public override double Length
        {
            get
            {
                double sum = 0.0;
                for (int i = 0; i < _geometries.Length; i++)
                    sum += (_geometries[i]).Length;
                return sum;
            }
        }

        //internal override int GetHashCodeInternal(int baseValue, Func<int, int> operation)
        //{
        //    if (!IsEmpty)
        //    {
        //        for (int i = 0; i < Count; i++)
        //        {
        //            var g = GetGeometryN(i);
        //            if (g is Point)
        //            {
        //                baseValue = ((Point)g).GetHashCodeInternal(baseValue, operation);
        //                continue;
        //            }
        //            if (g is LineString)
        //            {
        //                baseValue = ((LineString)g).GetHashCodeInternal(baseValue, operation);
        //                continue;
        //            }
        //            if (g is Polygon)
        //            {
        //                baseValue = ((Polygon)g).GetHashCodeInternal(baseValue, operation);
        //                continue;
        //            }

        //            baseValue = ((GeometryCollection) g).GetHashCodeInternal(baseValue, operation);

        //        }
        //    }
        //    return baseValue;
        //}

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

            var otherCollection = (GeometryCollection) other;
            if (_geometries.Length != otherCollection.Geometries.Length)
                return false;

            for (int i = 0; i < _geometries.Length; i++)
                if (!_geometries[i].EqualsExact(
                     otherCollection.Geometries[i], tolerance))
                        return false;
            return true;
        }

        /// <inheritdoc cref="Apply(ICoordinateFilter)"/>
        public override void Apply(ICoordinateFilter filter)
        {
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        /// <inheritdoc cref="Apply(ICoordinateSequenceFilter)"/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (_geometries.Length == 0)
                return;
            for (int i = 0; i < _geometries.Length; i++)
            {
                _geometries[i].Apply(filter);
                if (filter.Done)
                {
                    break;
                }
            }
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <inheritdoc />
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (_geometries.Length == 0)
            {
                return;
            }

            foreach (var geom in _geometries)
            {
                geom.Apply(filter);
                if (filter.Done)
                {
                    break;
                }
            }

            if (filter.GeometryChanged)
            {
                GeometryChanged();
            }
        }

        /// <inheritdoc cref="Apply(IGeometryFilter)"/>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        /// <inheritdoc cref="Apply(IGeometryComponentFilter)"/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override Geometry CopyInternal()
        {
            var geometries = new Geometry[_geometries.Length];
            for (int i = 0; i < _geometries.Length; i++)
                geometries[i] = _geometries[i].Copy();
            return new GeometryCollection(geometries, Factory);
        }

        /// <summary>
        ///
        /// </summary>
        public override void Normalize()
        {
            for (int i = 0; i < _geometries.Length; i++)
                _geometries[i].Normalize();
            Array.Sort(_geometries);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal()
        {
            var envelope = new Envelope();
            for (int i = 0; i < _geometries.Length; i++)
                envelope.ExpandToInclude(_geometries[i].EnvelopeInternal);
            return envelope;
        }

        ///<inheritdoc cref="Geometry.CompareToSameClass(object)"/>
        protected internal override int CompareToSameClass(object o)
        {
            var theseElements = new List<Geometry>(_geometries);
            var otherElements = new List<Geometry>(((GeometryCollection) o)._geometries);
            return Compare(theseElements, otherElements);
        }

        ///<inheritdoc cref="Geometry.CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            var gc = (GeometryCollection) o;

            int n1 = NumGeometries;
            int n2 = gc.NumGeometries;
            int i = 0;
            while (i < n1 && i < n2)
            {
                var thisGeom = GetGeometryN(i);
                //Assert.IsTrue(thisGeom is Geometry);
                var otherGeom = gc.GetGeometryN(i);
                int holeComp = thisGeom.CompareToSameClass(otherGeom, comp);
                if (holeComp != 0) return holeComp;
                i++;
            }
            if (i < n1) return 1;
            if (i < n2) return -1;
            return 0;
        }

        /// <summary>
        /// Return <c>true</c> if all features in collection are of the same type.
        /// </summary>
        public bool IsHomogeneous
        {
            get
            {
                var baseGeom = Geometries[0];
                for (int i = 1; i < Geometries.Length; i++)
                    if (baseGeom.GetType() != Geometries[i].GetType())
                        return false;
                return true;
            }
        }

        /// <inheritdoc />
        public IEnumerator<Geometry> GetEnumerator()
        {
            return ((IEnumerable<Geometry>)Geometries).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns the iTh element in the collection.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Geometry this[int i] => _geometries[i];

        /// <summary>
        /// Creates a <see cref="GeometryCollection"/> with
        /// every component reversed.
        /// The order of the components in the collection are not reversed.
        /// </summary>
        /// <returns>A <see cref="GeometryCollection"/></returns> in the reverse order
        [Obsolete("Call Geometry.Reverse()")]
#pragma warning disable 809
        public override Geometry Reverse()
        {
            return base.Reverse();
        }
#pragma warning restore 809

        /// <summary>
        /// The actual implementation of the <see cref="Geometry.Reverse"/> function for <c>GeometryCollection</c>s.
        /// </summary>
        /// <returns>A reversed geometry</returns>
        protected override Geometry ReverseInternal()
        {
            int numGeometries = _geometries.Length;
            var reversed = new Geometry[numGeometries];
            for (int i = 0; i < numGeometries; i++)
                reversed[i] = _geometries[i].Reverse();

            return new GeometryCollection(reversed, Factory);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="GeometryCollection" />.
        /// </summary>
        public int Count => _geometries.Length;

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
