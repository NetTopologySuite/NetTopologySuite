using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>GeometryCollection</c>.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class GeometryCollection : Geometry, IGeometryCollection
    {
        /// <summary>
        /// Represents an empty <c>GeometryCollection</c>.
        /// </summary>
        public static readonly IGeometryCollection Empty = DefaultFactory.CreateGeometryCollection(null);

        /// <summary>
        /// Internal representation of this <c>GeometryCollection</c>.
        /// </summary>
        private IGeometry[] _geometries;

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
        public GeometryCollection(IGeometry[] geometries) : this(geometries, DefaultFactory) { }

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
        public GeometryCollection(IGeometry[] geometries, IGeometryFactory factory) : base(factory)
        {
            if (geometries == null)
                geometries = new IGeometry[] { };
            if (HasNullElements(CollectionUtil.Cast<IGeometry, object>(geometries)))
                throw new ArgumentException("geometries must not contain null elements");
            _geometries = geometries;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        protected override SortIndexValue SortIndex => SortIndexValue.GeometryCollection;

        /// <summary>
        ///
        /// </summary>
        public override Coordinate Coordinate
        {
            get
            {
                if (IsEmpty)
                    return null;
                return _geometries[0].Coordinate;
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

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        public override int NumGeometries => _geometries.Length;

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public override IGeometry GetGeometryN(int n)
        {
            return _geometries[n];
        }

        /// <summary>
        ///
        /// </summary>
        public IGeometry[] Geometries
        {
            get => _geometries;
            protected set => _geometries = value;
        }

        /// <summary>
        ///
        /// </summary>
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
        public override string GeometryType => "GeometryCollection";

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

        /// <summary>
        ///
        /// </summary>
        public override IGeometry Boundary
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
        public override bool EqualsExact(IGeometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;

            var otherCollection = (IGeometryCollection) other;
            if (_geometries.Length != otherCollection.Geometries.Length)
                return false;

            for (int i = 0; i < _geometries.Length; i++)
                if (!_geometries[i].EqualsExact(
                     otherCollection.Geometries[i], tolerance))
                        return false;
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(ICoordinateFilter filter)
        {
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (_geometries.Length == 0)
                return;
            for (int i = 0; i < _geometries.Length; i++)
            {
                ((Geometry)_geometries[i]).Apply(filter);
                if (filter.Done)
                {
                    break;
                }
            }
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < _geometries.Length; i++)
                 _geometries[i].Apply(filter);
        }

        /// <summary>
        /// Creates and returns a full copy of this <see cref="IGeometryCollection"/> object.
        /// (including all coordinates contained by it).
        /// </summary>
        /// <returns>A copy of this instance</returns>
        [Obsolete("Use Copy()")]
        public override object Clone()
        {
            return Copy();
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>>
        protected override IGeometry CopyInternal()
        {
            var geometries = new IGeometry[_geometries.Length];
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

        /// <summary>
        ///
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o)
        {
            var theseElements = new List<IGeometry>(_geometries);
            var otherElements = new List<IGeometry>(((GeometryCollection) o)._geometries);
            return Compare(theseElements, otherElements);
        }

        protected internal override int CompareToSameClass(object o, IComparer<ICoordinateSequence> comp)
        {
            var gc = (IGeometryCollection) o;

            int n1 = NumGeometries;
            int n2 = gc.NumGeometries;
            int i = 0;
            while (i < n1 && i < n2)
            {
                var thisGeom = GetGeometryN(i);
                Assert.IsTrue(thisGeom is Geometry);
                var otherGeom = gc.GetGeometryN(i);
                int holeComp = ((Geometry) thisGeom).CompareToSameClass(otherGeom, comp);
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

        /// <summary>
        /// Returns a <c>GeometryCollectionEnumerator</c>:
        /// this IEnumerator returns the parent geometry as first element.
        /// In most cases is more useful the code
        /// <c>geometryCollectionInstance.Geometries.GetEnumerator()</c>:
        /// this returns an IEnumerator over geometries composing GeometryCollection.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IGeometry> GetEnumerator()
        {
            return new GeometryCollectionEnumerator(this);
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
        public IGeometry this[int i] => _geometries[i];

        ///<summary>
        /// Creates a <see cref="IGeometryCollection"/> with
        /// every component reversed.
        /// The order of the components in the collection are not reversed.
        ///</summary>
        /// <returns>A <see cref="IGeometryCollection"/></returns> in the reverse order
        public override IGeometry Reverse()
        {
            int n = _geometries.Length;
            var revGeoms = new IGeometry[n];
            for (int i = 0; i < _geometries.Length; i++)
            {
                revGeoms[i] = _geometries[i].Reverse();
            }
            return Factory.CreateGeometryCollection(revGeoms);
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="GeometryCollection" />.
        /// </summary>
        public int Count => _geometries.Length;

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
