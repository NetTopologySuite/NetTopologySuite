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
            if (HasNullElements(geometries))             
                throw new ArgumentException("geometries must not contain null elements");            
            this.geometries = geometries;
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICoordinate Coordinate 
        {
            get
            {
                if (IsEmpty) 
                    return null;
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
                int k = -1;
                for (int i = 0; i < geometries.Length; i++)
                {
                    ICoordinate[] childCoordinates = geometries[i].Coordinates;
                    for (int j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }
                return coordinates;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                for (int i = 0; i < geometries.Length; i++)
                    if (!geometries[i].IsEmpty) 
                        return false;
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions Dimension 
        {
            get
            {
                Dimensions dimension = Dimensions.False;
                for (int i = 0; i < geometries.Length; i++)
                    dimension = (Dimensions) Math.Max((int)dimension, (int)geometries[i].Dimension);
                return dimension;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions BoundaryDimension
        {
            get
            {
                Dimensions dimension = Dimensions.False;
                for (int i = 0; i < geometries.Length; i++)
                    dimension = (Dimensions) Math.Max((int) dimension, (int) (geometries[i].BoundaryDimension));
                return dimension;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int NumGeometries
        {
            get
            {
                return geometries.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public override IGeometry GetGeometryN(int n) 
        {
            return geometries[n];
        }

        /// <summary>
        /// 
        /// </summary>
        public IGeometry[] Geometries
        {
            get
            {
                return geometries;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int NumPoints 
        {
            get
            {
                int numPoints = 0;
                for (int i = 0; i < geometries.Length; i++)
                    numPoints += geometries[i].NumPoints;
                return numPoints;
            }
        }

        /// <summary>
        /// 
        /// </summary>        
        public override string GeometryType
        {
            get
            {                
                return "GeometryCollection";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                CheckNotGeometryCollection(this);
                Assert.ShouldNeverReachHere();
                return false;
            }
        }

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
                for (int i = 0; i < geometries.Length; i++)
                    area += geometries[i].Area;
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
                for (int i = 0; i < geometries.Length; i++)
                    sum += (geometries[i]).Length;
                return sum;
            }
        }

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

            IGeometryCollection otherCollection = (IGeometryCollection) other;
            if (geometries.Length != otherCollection.Geometries.Length)
                return false;

            for (int i = 0; i < geometries.Length; i++) 
                if (!geometries[i].EqualsExact(
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
            for (int i = 0; i < geometries.Length; i++)
                 geometries[i].Apply(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryFilter filter)
        {
            filter.Filter(this);
            for (int i = 0; i < geometries.Length; i++)
                 geometries[i].Apply(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryComponentFilter filter) 
        {
            filter.Filter(this);
            for (int i = 0; i < geometries.Length; i++)
                 geometries[i].Apply(filter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone() 
        {
            GeometryCollection gc = (GeometryCollection) base.Clone();
            gc.geometries = new IGeometry[geometries.Length];
            for (int i = 0; i < geometries.Length; i++) 
                gc.geometries[i] = (IGeometry) geometries[i].Clone();
            return gc; 
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Normalize() 
        {
            for (int i = 0; i < geometries.Length; i++) 
                geometries[i].Normalize();
            Array.Sort(geometries);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnvelope ComputeEnvelopeInternal()
        {
            IEnvelope envelope = new Envelope();
            for (int i = 0; i < geometries.Length; i++) 
                envelope.ExpandToInclude(geometries[i].EnvelopeInternal);
            return envelope;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o) 
        {
            ArrayList theseElements = new ArrayList(geometries);
            ArrayList otherElements = new ArrayList(((GeometryCollection) o).geometries);
            return Compare(theseElements, otherElements);
        }

        /// <summary>
        /// Return <c>true</c> if all features in collection are of the same type.
        /// </summary>
        public bool IsHomogeneous
        {
            get
            {
                IGeometry baseGeom = Geometries[0];
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
        public IEnumerator GetEnumerator()
        {
            return new GeometryCollectionEnumerator(this);
        }

        /// <summary>
        /// Returns the iTh element in the collection.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IGeometry this[int i]
        {
            get
            {
                return this.geometries[i];
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Returns the number of geometries contained by this <see cref="GeometryCollection" />.
        /// </summary>
        public int Count
        {
            get
            {
                return geometries.Length;
            }
        }
        
        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
