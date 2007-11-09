using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <c>GeometryCollection</c>.
    /// </summary>
    [Serializable]
    public class GeometryCollection : Geometry, IGeometryCollection
    {
        /// <summary>
        /// Represents an empty <c>GeometryCollection</c>.
        /// </summary>
        public static readonly IGeometryCollection Empty = DefaultFactory.CreateGeometryCollection(null);

        /// <summary>
        /// Internal representation of this <c>GeometryCollection</c>.        
        /// </summary>
        protected IGeometry[] geometries = null;

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <c>GeometryCollection</c>,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="Geometry{TCoordinate}Factory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public GeometryCollection(IGeometry[] geometries) : this(geometries, DefaultFactory) {}

        /// <param name="geometries">
        /// The <see cref="Geometry{TCoordinate}"/>s for this <c>GeometryCollection</c>,
        /// or <see langword="null" /> or an empty array to create the empty
        /// point. Elements may be empty <see cref="Geometry{TCoordinate}"/>s,
        /// but not <see langword="null" />s.
        /// </param>
        public GeometryCollection(IGeometry[] geometries, IGeometryFactory factory) : base(factory)
        {
            if (geometries == null)
            {
                geometries = new IGeometry[] {};
            }

            if (HasNullElements(geometries))
            {
                throw new ArgumentException("geometries must not contain null elements");
            }

            this.geometries = geometries;
        }

        public override ICoordinate Coordinate
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                return geometries[0].Coordinate;
            }
        }

        /// <summary>
        /// Collects all coordinates of all subgeometries into an Array.
        /// Note that while changes to the coordinate objects themselves
        /// may modify the Geometries in place, the returned Array as such 
        /// is only a temporary container which is not synchronized back.
        /// </summary>
        /// <returns>The collected coordinates.</returns>
        public override ICoordinate[] Coordinates
        {
            get
            {
                ICoordinate[] coordinates = new ICoordinate[NumPoints];
                Int32 k = -1;
                
                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    ICoordinate[] childCoordinates = geometries[i].Coordinates;
                   
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
                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    if (!geometries[i].IsEmpty)
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

                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    dimension = (Dimensions) Math.Max((Int32) dimension, (Int32) geometries[i].Dimension);
                }

                return dimension;
            }
        }

        public override Dimensions BoundaryDimension
        {
            get
            {
                Dimensions dimension = Dimensions.False;
                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    dimension = (Dimensions) Math.Max((Int32) dimension, (Int32) (geometries[i].BoundaryDimension));
                }
                return dimension;
            }
        }

        public override Int32 NumGeometries
        {
            get { return geometries.Length; }
        }

        public override IGeometry GetGeometryN(Int32 n)
        {
            return geometries[n];
        }

        public IGeometry[] Geometries
        {
            get { return geometries; }
        }

        public override Int32 NumPoints
        {
            get
            {
                Int32 numPoints = 0;

                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    numPoints += geometries[i].NumPoints;
                }

                return numPoints;
            }
        }
       
        public override string GeometryType
        {
            get { return "GeometryCollection"; }
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
        public override Double Area
        {
            get
            {
                Double area = 0.0;

                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    area += geometries[i].Area;
                }

                return area;
            }
        }

        /// <summary>  
        /// Returns the length of this <c>GeometryCollection</c>.
        /// </summary>        
        public override Double Length
        {
            get
            {
                Double sum = 0.0;

                for (Int32 i = 0; i < geometries.Length; i++)
                {
                    sum += (geometries[i]).Length;
                }

                return sum;
            }
        }

        public override Boolean EqualsExact(IGeometry other, Double tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            IGeometryCollection otherCollection = (IGeometryCollection) other;
          
            if (geometries.Length != otherCollection.Geometries.Length)
            {
                return false;
            }

            for (Int32 i = 0; i < geometries.Length; i++)
            {
                if (!geometries[i].EqualsExact(
                         otherCollection.Geometries[i], tolerance))
                {
                    return false;
                }
            }

            return true;
        }

        public override void Apply(ICoordinateFilter filter)
        {
            for (Int32 i = 0; i < geometries.Length; i++)
            {
                geometries[i].Apply(filter);
            }
        }

        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);

            for (Int32 i = 0; i < geometries.Length; i++)
            {
                geometries[i].Apply(filter);
            }
        }

        public override void Apply(IGeometryComponentFilter filter)
        {
            filter.Filter(this);

            for (Int32 i = 0; i < geometries.Length; i++)
            {
                geometries[i].Apply(filter);
            }
        }

        public override object Clone()
        {
            GeometryCollection gc = (GeometryCollection) base.Clone();
            gc.geometries = new IGeometry[geometries.Length];

            for (Int32 i = 0; i < geometries.Length; i++)
            {
                gc.geometries[i] = (IGeometry) geometries[i].Clone();
            }

            return gc;
        }

        public override void Normalize()
        {
            for (Int32 i = 0; i < geometries.Length; i++)
            {
                geometries[i].Normalize();
            }

            Array.Sort(geometries);
        }

        protected override IExtents ComputeEnvelopeInternal()
        {
            IExtents envelope = new Extents();

            for (Int32 i = 0; i < geometries.Length; i++)
            {
                envelope.ExpandToInclude(geometries[i].EnvelopeInternal);
            }

            return envelope;
        }

        protected internal override Int32 CompareToSameClass(object o)
        {
            ArrayList theseElements = new ArrayList(geometries);
            ArrayList otherElements = new ArrayList(((GeometryCollection) o).geometries);
            return Compare(theseElements, otherElements);
        }

        /// <summary>
        /// Return <see langword="true"/> if all features in collection are of the same type.
        /// </summary>
        public Boolean IsHomogeneous
        {
            get
            {
                IGeometry baseGeom = Geometries[0];

                for (Int32 i = 1; i < Geometries.Length; i++)
                {
                    if (baseGeom.GetType() != Geometries[i].GetType())
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
            return new GeometryCollectionEnumerator(this);
        }

        /// <summary>
        /// Returns the iTh element in the collection.
        /// </summary>
        public IGeometry this[Int32 i]
        {
            get { return geometries[i]; }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="Geometry{TCoordinate}Collection" />.
        /// </summary>
        public Int32 Count
        {
            get { return geometries.Length; }
        }

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}