using System;
using System.Collections;
using System.Xml;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.IO.GML2;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap;
using GisSharpBlog.NetTopologySuite.Operation.Predicate;
using GisSharpBlog.NetTopologySuite.Operation.Relate;
using GisSharpBlog.NetTopologySuite.Operation.Valid;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Geometries
{   
    /// <summary>  
    /// Basic implementation of <c>Geometry</c>.
    /// <c>Clone</c> returns a deep copy of the object.
    /// <para>
    /// Binary Predicates: 
    /// Because it is not clear at this time what semantics for spatial
    /// analysis methods involving <c>GeometryCollection</c>s would be useful,
    /// <c>GeometryCollection</c>s are not supported as arguments to binary
    /// predicates (other than <c>ConvexHull</c>) or the <c>Relate</c> method.
    /// </para>
    /// <para>
    /// Set-Theoretic Methods: 
    /// The spatial analysis methods will
    /// return the most specific class possible to represent the result. If the
    /// result is homogeneous, a <c>Point</c>, <c>LineString</c>, or
    /// <c>Polygon</c> will be returned if the result contains a single
    /// element; otherwise, a <c>MultiPoint</c>, <c>MultiLineString</c>,
    /// or <c>MultiPolygon</c> will be returned. If the result is
    /// heterogeneous a <c>GeometryCollection</c> will be returned.
    /// </para>
    /// <para>
    /// Representation of Computed Geometries:  
    /// The SFS states that the result
    /// of a set-theoretic method is the "point-set" result of the usual
    /// set-theoretic definition of the operation (SFS 3.2.21.1). However, there are
    /// sometimes many ways of representing a point set as a <c>Geometry</c>.
    /// The SFS does not specify an unambiguous representation of a given point set
    /// returned from a spatial analysis method. One goal of NTS is to make this
    /// specification precise and unambiguous. NTS will use a canonical form for
    /// <c>Geometry</c>s returned from spatial analysis methods. The canonical
    /// form is a <c>Geometry</c> which is simple and noded:
    /// Simple means that the Geometry returned will be simple according to
    /// the NTS definition of <c>IsSimple</c>.
    /// Noded applies only to overlays involving <c>LineString</c>s. It
    /// means that all intersection points on <c>LineString</c>s will be
    /// present as endpoints of <c>LineString</c>s in the result.
    /// This definition implies that non-simple geometries which are arguments to
    /// spatial analysis methods must be subjected to a line-dissolve process to
    /// ensure that the results are simple.
    /// </para>
    /// <para>
    /// Constructed Points And The Precision Model: 
    /// The results computed by the set-theoretic methods may
    /// contain constructed points which are not present in the input <c>Geometry</c>s. 
    /// These new points arise from intersections between line segments in the
    /// edges of the input <c>Geometry</c>s. In the general case it is not
    /// possible to represent constructed points exactly. This is due to the fact
    /// that the coordinates of an intersection point may contain twice as many bits
    /// of precision as the coordinates of the input line segments. In order to
    /// represent these constructed points explicitly, NTS must truncate them to fit
    /// the <c>PrecisionModel</c>. 
    /// Unfortunately, truncating coordinates moves them slightly. Line segments
    /// which would not be coincident in the exact result may become coincident in
    /// the truncated representation. This in turn leads to "topology collapses" --
    /// situations where a computed element has a lower dimension than it would in
    /// the exact result. 
    /// When NTS detects topology collapses during the computation of spatial
    /// analysis methods, it will throw an exception. If possible the exception will
    /// report the location of the collapse. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode" /> are not overridden, so that when two
    /// topologically equal Geometries are added to Collections and Dictionaries, they
    /// remain distinct. This behaviour is desired in many cases.
    /// </remarks>
    [Serializable]    
    public abstract class Geometry: IGeometry
    {        
        /// <summary>
        /// 
        /// </summary>
        private static readonly Type[] SortedClasses = new Type[] 
        {
            typeof(Point),
            typeof(MultiPoint),
            typeof(LineString),
            typeof(LinearRing),
            typeof(MultiLineString),
            typeof(Polygon),
            typeof(MultiPolygon),
            typeof(GeometryCollection),    
        };                    

        private IGeometryFactory factory = null;

        /// <summary> 
        /// Gets the factory which contains the context in which this point was created.
        /// </summary>
        /// <returns>The factory for this point.</returns>
        public IGeometryFactory Factory
        {
            get 
            { 
                return factory; 
            }
        }

        private object userData = null;
        
        /// <summary> 
        /// Gets/Sets the user data object for this point, if any.
        /// A simple scheme for applications to add their own custom data to a Geometry.
        /// An example use might be to add an object representing a Coordinate Reference System.
        /// Note that user data objects are not present in geometries created by
        /// construction methods.
        /// </summary>
        public object UserData
        {
            get
            {                
                return userData;
            }
            set
            {
                userData = value;
            }
        }
           
        /// <summary>
        /// The bounding box of this <c>Geometry</c>.
        /// </summary>
        protected IEnvelope envelope;
       
        // The ID of the Spatial Reference System used by this <c>Geometry</c>
        private int srid;

        /// <summary>  
        /// Gets/Sets the ID of the Spatial Reference System used by the <c>Geometry</c>. 
        /// NTS supports Spatial Reference System information in the simple way
        /// defined in the SFS. A Spatial Reference System ID (SRID) is present in
        /// each <c>Geometry</c> object. <c>Geometry</c> provides basic
        /// accessor operations for this field, but no others. The SRID is represented
        /// as an integer.
        /// </summary>        
        public int SRID
        {
            get 
            { 
                return srid; 
            }
            set 
            {
                srid = value;
				IGeometryCollection collection = this as IGeometryCollection;
				if (collection != null)
				{
					foreach (IGeometry geometry in collection.Geometries)
					{
						geometry.SRID = value;
					}
				}
				factory = new GeometryFactory(factory.PrecisionModel, value, factory.CoordinateSequenceFactory);
			}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        public Geometry(IGeometryFactory factory)
        {
            this.factory = factory;
            srid = factory.SRID;
        }

        /// <summary>  
        /// Returns the name of this object's interface.
		/// </summary>
		/// <returns>The name of this <c>Geometry</c>s most specific interface.</returns>
        public abstract string GeometryType { get; }

        /// <summary>  
        /// Returns true if the array contains any non-empty <c>Geometry</c>s.
		/// </summary>
		/// <param name="geometries"> an array of <c>Geometry</c>s; no elements may be <c>null</c></param>
		/// <returns>            
        /// <c>true</c> if any of the <c>Geometry</c>s
		/// <c>IsEmpty</c> methods return <c>false</c>.
		/// </returns>
        protected static bool HasNonEmptyElements(IGeometry[] geometries)
        {
            foreach (IGeometry g in geometries)
                if(!g.IsEmpty)
                    return true;                                        
            return false;
        }

        /// <summary>  
        /// Returns true if the array contains any <c>null</c> elements.
        /// </summary>
        /// <param name="array"> an array to validate.</param>
        /// <returns><c>true</c> if any of <c>array</c>s elements are <c>null</c>.</returns>
        public static bool HasNullElements(object[] array)
        {
            foreach (object o in array)
                if (o == null)
                    return true;
            return false;            
        }

        /// <summary>  
        /// Returns the <c>PrecisionModel</c> used by the <c>Geometry</c>.
        /// </summary>
        /// <returns>    
        /// the specification of the grid of allowable points, for this
        /// <c>Geometry</c> and all other <c>Geometry</c>s.
        /// </returns>
        public IPrecisionModel PrecisionModel
        {
            get
            {
                return Factory.PrecisionModel;
            }
        }

        /// <summary>  
        /// Returns a vertex of this <c>Geometry</c>.
        /// </summary>
        /// <returns>    
        /// a Coordinate which is a vertex of this <c>Geometry</c>.
        /// Returns <c>null</c> if this Geometry is empty.
        /// </returns>
        public abstract ICoordinate Coordinate { get; }

        /// <summary>  
        /// Returns this <c>Geometry</c> s vertices. If you modify the coordinates
        /// in this array, be sure to call GeometryChanged afterwards.
        /// The <c>Geometry</c>s contained by composite <c>Geometry</c>s
        /// must be Geometry's; that is, they must implement <c>Coordinates</c>.
        /// </summary>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        public abstract ICoordinate[] Coordinates { get; }

        /// <summary>  
        /// Returns the count of this <c>Geometry</c>s vertices. The <c>Geometry</c>
        /// s contained by composite <c>Geometry</c>s must be
        /// Geometry's; that is, they must implement <c>NumPoints</c>.
        /// </summary>
        /// <returns>The number of vertices in this <c>Geometry</c>.</returns>
        public abstract int NumPoints { get; }

        /// <summary>
        /// Returns the number of Geometryes in a GeometryCollection,
        /// or 1, if the geometry is not a collection.
        /// </summary>
        public virtual int NumGeometries
        {
            get
            {
                return 1;
            }
        }
        
        /// <summary>
        /// Returns an element Geometry from a GeometryCollection,
        /// or <code>this</code>, if the geometry is not a collection.
        /// </summary>
        /// <param name="n">The index of the geometry element.</param>
        /// <returns>The n'th geometry contained in this geometry.</returns>
        public virtual IGeometry GetGeometryN(int n)
        {
            return this;
        }

        /// <summary> 
        /// Returns false if the <c>Geometry</c> not simple.
        /// Subclasses provide their own definition of "simple". If
        /// this <c>Geometry</c> is empty, returns <c>true</c>. 
        /// In general, the SFS specifications of simplicity seem to follow the
        /// following rule:
        ///  A Geometry is simple if the only self-intersections are at boundary points.
        /// For all empty <c>Geometry</c>s, <c>IsSimple==true</c>.
        /// </summary>
        /// <returns>    
        /// <c>true</c> if this <c>Geometry</c> has any points of
        /// self-tangency, self-intersection or other anomalous points.
        /// </returns>
        public abstract bool IsSimple { get; }

        /// <summary>  
        /// Tests the validity of this <c>Geometry</c>.
        /// Subclasses provide their own definition of "valid".
        /// </summary>
        /// <returns><c>true</c> if this <c>Geometry</c> is valid.</returns>
        public virtual bool IsValid
        {
            get
            {
                IsValidOp isValidOp = new IsValidOp(this);
                return isValidOp.IsValid;
            }
        }

        /// <summary> 
        /// Returns whether or not the set of points in this <c>Geometry</c> is empty.
        /// </summary>
        /// <returns><c>true</c> if this <c>Geometry</c> equals the empty point.</returns>
        public abstract bool IsEmpty { get; }

        /// <summary>  
        /// Returns the minimum distance between this <c>Geometry</c>
        /// and the <c>Geometry</c> g.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> from which to compute the distance.</param>
        public double Distance(IGeometry g)
        {
            return DistanceOp.Distance(this, g);
        }

        /// <summary> 
        /// Tests whether the distance from this <c>Geometry</c>
        /// to another is less than or equal to a specified value.
        /// </summary>
        /// <param name="geom">the Geometry to check the distance to.</param>
        /// <param name="distance">the distance value to compare.</param>
        /// <returns><c>true</c> if the geometries are less than <c>distance</c> apart.</returns>
        public bool IsWithinDistance(IGeometry geom, double distance)
        {
            double envDist = EnvelopeInternal.Distance(geom.EnvelopeInternal);            
            if (envDist > distance)
                return false;
            return DistanceOp.IsWithinDistance(this, geom, distance);            
        }

        /// <summary>  
        /// Returns the area of this <c>Geometry</c>.
        /// Areal Geometries have a non-zero area.
        /// They override this function to compute the area.
        /// Others return 0.0
        /// </summary>
        /// <returns>The area of the Geometry.</returns>
        public virtual double Area
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary> 
        /// Returns the length of this <c>Geometry</c>.
        /// Linear geometries return their length.
        /// Areal geometries return their perimeter.
        /// They override this function to compute the length.
        /// Others return 0.0
        /// </summary>
        /// <returns>The length of the Geometry.</returns>
        public virtual double Length
        {
            get
            {
                return 0.0;
            }
        }

        /// <summary> 
        /// Computes the centroid of this <c>Geometry</c>.
        /// The centroid is equal to the centroid of the set of component Geometries of highest
        /// dimension (since the lower-dimension geometries contribute zero "weight" to the centroid).
        /// </summary>
        /// <returns>A Point which is the centroid of this Geometry.</returns>
        public IPoint Centroid
        {
            get
            {
                if (IsEmpty) 
                    return null;

                ICoordinate centPt = null;
                Dimensions dim = Dimension;
                if (dim == Dimensions.Point)
                {
                    CentroidPoint cent = new CentroidPoint();
                    cent.Add(this);
                    centPt = cent.Centroid;
                }
                else if (dim == Dimensions.Curve)
                {
                    CentroidLine cent = new CentroidLine();
                    cent.Add(this);
                    centPt = cent.Centroid;
                }
                else
                {
                    CentroidArea cent = new CentroidArea();
                    cent.Add(this);
                    centPt = cent.Centroid;
                }
                return CreatePointFromInternalCoord(centPt, this);
            }
        }

        /// <summary>
        /// Computes an interior point of this <c>Geometry</c>.
        /// An interior point is guaranteed to lie in the interior of the Geometry,
        /// if it possible to calculate such a point exactly. Otherwise,
        /// the point may lie on the boundary of the point.
        /// </summary>
        /// <returns>A <c>Point</c> which is in the interior of this Geometry.</returns>
        public IPoint InteriorPoint
        {
            get
            {
                ICoordinate interiorPt = null;
                Dimensions dim = Dimension;
                if (dim == Dimensions.Point)
                {
                    InteriorPointPoint intPt = new InteriorPointPoint(this);
                    interiorPt = intPt.InteriorPoint;
                }
                else if (dim == Dimensions.Curve)
                {
                    InteriorPointLine intPt = new InteriorPointLine(this);
                    interiorPt = intPt.InteriorPoint;
                }
                else
                {
                    InteriorPointArea intPt = new InteriorPointArea(this);
                    interiorPt = intPt.InteriorPoint;
                }
                return CreatePointFromInternalCoord(interiorPt, this);
            }
        }

        /// <summary>
        /// <see cref="InteriorPoint" />
        /// </summary>
        public IPoint PointOnSurface
        {
            get
            {
                return InteriorPoint;
            }
        }

        private Dimensions dimension;

        /// <summary> 
        /// Returns the dimension of this <c>Geometry</c>.
        /// </summary>
        /// <returns>  
        /// The dimension of the class implementing this interface, whether
        /// or not this object is the empty point.
        /// </returns>
        public virtual Dimensions Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }


        private IGeometry boundary;

        /// <summary>  
        /// Returns the boundary, or the empty point if this <c>Geometry</c>
        /// is empty. For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        public virtual IGeometry Boundary
        {
            get { return boundary; }
            set { boundary = value; }
        }

        private Dimensions boundaryDimension;

        /// <summary> 
        /// Returns the dimension of this <c>Geometry</c>s inherent boundary.
        /// </summary>
        /// <returns>    
        /// The dimension of the boundary of the class implementing this
        /// interface, whether or not this object is the empty point. Returns
        /// <c>Dimension.False</c> if the boundary is the empty point.
        /// </returns>
        public virtual Dimensions BoundaryDimension
        {
            get { return boundaryDimension; }
            set { boundaryDimension = value; }
        }

        /// <summary>  
        /// Returns this <c>Geometry</c>s bounding box. If this <c>Geometry</c>
        /// is the empty point, returns an empty <c>Point</c>. If the <c>Geometry</c>
        /// is a point, returns a non-empty <c>Point</c>. Otherwise, returns a
        /// <c>Polygon</c> whose points are (minx, miny), (maxx, miny), (maxx,
        /// maxy), (minx, maxy), (minx, miny).
        /// </summary>
        /// <returns>    
        /// An empty <c>Point</c> (for empty <c>Geometry</c>s), a
        /// <c>Point</c> (for <c>Point</c>s) or a <c>Polygon</c>
        /// (in all other cases).
        /// </returns>
        public IGeometry Envelope
        {
            get
            {
                return Factory.ToGeometry(EnvelopeInternal);
            }
        }

        /// <summary> 
        /// Returns the minimum and maximum x and y values in this <c>Geometry</c>
        /// , or a null <c>Envelope</c> if this <c>Geometry</c> is empty.
        /// </summary>
        /// <returns>    
        /// This <c>Geometry</c>s bounding box; if the <c>Geometry</c>
        /// is empty, <c>Envelope.IsNull</c> will return <c>true</c>.
        /// </returns>
        public IEnvelope EnvelopeInternal
        {
            get
            {
                if (envelope == null)
                    envelope = ComputeEnvelopeInternal();                
                return envelope;
            }
        }

        private class GeometryChangedFilter : IGeometryComponentFilter
        {
            public void Filter(IGeometry geom)
            {
                geom.GeometryChangedAction();
            }
        };

        /// <summary>
        /// Notifies this Geometry that its Coordinates have been changed by an external
        /// party (using a CoordinateFilter, for example). The Geometry will flush
        /// and/or update any information it has cached (such as its Envelope).
        /// </summary>
        public void GeometryChanged()
        {
            Apply(new GeometryChangedFilter());
        }

        /// <summary> 
        /// Notifies this Geometry that its Coordinates have been changed by an external
        /// party. When GeometryChanged is called, this method will be called for
        /// this Geometry and its component Geometries.
        /// </summary>
        public void GeometryChangedAction()
        {
            envelope = null;
        }

        /// <summary>  
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is FF*FF****.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are disjoint.</returns>
        public bool Disjoint(IGeometry g)
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return true;
            return Relate(g).IsDisjoint();
        }

        /// <summary>  
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is FT*******, F**T***** or F***T****.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if the two <c>Geometry</c>s touch;
        /// Returns false if both <c>Geometry</c>s are points.
        /// </returns>
        public bool Touches(IGeometry g) 
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsTouches(Dimension, g.Dimension);
        }

        /// <summary>  
        /// Returns <c>true</c> if <c>disjoint</c> returns false.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s intersect.</returns>
        public bool Intersects(IGeometry g) 
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            // optimizations for rectangle arguments
            if (IsRectangle)
                return RectangleIntersects.Intersects((IPolygon) this, g);
            if (g.IsRectangle)
                return RectangleIntersects.Intersects((IPolygon) g, this);
            return Relate(g).IsIntersects();
        }

        /// <summary>  
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is
        ///  T*T****** (for a point and a curve, a point and an area or a line
        /// and an area) 0******** (for two curves).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if the two <c>Geometry</c>s cross.
        /// For this function to return <c>true</c>, the <c>Geometry</c>
        /// s must be a point and a curve; a point and a surface; two curves; or a
        /// curve and a surface.
        /// </returns>
        public bool Crosses(IGeometry g) 
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsCrosses(Dimension, g.Dimension);
        }

        /// <summary>
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is T*F**F***.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if this <c>Geometry</c> is within <c>other</c>.</returns>
        public bool Within(IGeometry g)
        {
            return g.Contains(this); ;
        }

        /// <summary>
        /// Returns <c>true</c> if <c>other.within(this)</c> returns <c>true</c>.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if this <c>Geometry</c> contains <c>other</c>.</returns>
        public bool Contains(IGeometry g) 
        {
            // short-circuit test
            if (!EnvelopeInternal.Contains(g.EnvelopeInternal))
                return false;
            // optimizations for rectangle arguments
            if (IsRectangle)
                return RectangleContains.Contains((IPolygon) this, g);
            // general case
            return Relate(g).IsContains();
        }

        /// <summary>
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is
        ///  T*T***T** (for two points or two surfaces)
        ///  1*T***T** (for two curves).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if the two <c>Geometry</c>s overlap.
        /// For this function to return <c>true</c>, the <c>Geometry</c>
        /// s must be two points, two curves or two surfaces.
        /// </returns>
        public bool Overlaps(IGeometry g) 
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsOverlaps(Dimension, g.Dimension);
        }

        /// <summary>
        /// Returns <c>true</c> if this geometry covers the specified geometry.
        /// <para>
        /// The <c>Covers</c> predicate has the following equivalent definitions:
        ///     - Every point of the other geometry is a point of this geometry.
        ///     - The DE-9IM Intersection Matrix for the two geometries is <c>T*****FF*</c> or <c>*T****FF*</c> or <c>***T**FF*</c> or <c>****T*FF*</c>.
        ///     - <c>g.CoveredBy(this)</c> (<c>Covers</c> is the inverse of <c>CoveredBy</c>).
        /// </para>
        /// Note the difference between <c>Covers</c> and <c>Contains</c>: <c>Covers</c> is a more inclusive relation.
        /// In particular, unlike <c>Contains</c> it does not distinguish between
        /// points in the boundary and in the interior of geometries.        
        /// </summary>
        /// <remarks>
        /// For most situations, <c>Covers</c> should be used in preference to <c>Contains</c>.
        /// As an added benefit, <c>Covers</c> is more amenable to optimization, and hence should be more performant.
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if this <c>Geometry</c> covers <paramref name="g" /></returns>
        /// <seealso cref="Geometry.Contains" />
        /// <seealso cref="Geometry.CoveredBy" />
        public bool Covers(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Contains(g.EnvelopeInternal))
                return false;
            
            // optimization for rectangle arguments
            if (IsRectangle)
                return EnvelopeInternal.Contains(g.EnvelopeInternal);
            
            return Relate(g).IsCovers();
        }

        /// <summary>
        /// Returns <c>true</c> if this geometry is covered by the specified geometry.
        /// <para>
        /// The <c>CoveredBy</c> predicate has the following equivalent definitions:
        ///     - Every point of this geometry is a point of the other geometry.
        ///     - The DE-9IM Intersection Matrix for the two geometries is <c>T*F**F***</c> or <c>*TF**F***</c> or <c>**FT*F***</c> or <c>**F*TF***</c>.
        ///     - <c>g.Covers(this)</c> (<c>CoveredBy</c> is the inverse of <c>Covers</c>).
        /// </para>
        /// Note the difference between <c>CoveredBy</c> and <c>Within</c>: <c>CoveredBy</c> is a more inclusive relation.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>.
        /// <returns><c>true</c> if this <c>Geometry</c> is covered by <paramref name="g" />.</returns>
        /// <seealso cref="Geometry.Within" />
        /// <seealso cref="Geometry.Covers" />
        public bool CoveredBy(IGeometry g)
        {
            return g.Covers(this);
        }

        /// <summary>  
        /// Returns <c>true</c> if the elements in the DE-9IM intersection
        /// matrix for the two <c>Geometry</c>s match the elements in <c>intersectionPattern</c>
        /// , which may be:
        ///  0
        ///  1
        ///  2
        ///  T ( = 0, 1 or 2)
        ///  F ( = -1)
        ///  * ( = -1, 0, 1 or 2)
        /// For more information on the DE-9IM, see the OpenGIS Simple Features
        /// Specification.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <param name="intersectionPattern">The pattern against which to check the intersection matrix for the two <c>Geometry</c>s.</param>
        /// <returns><c>true</c> if the DE-9IM intersection matrix for the two <c>Geometry</c>s match <c>intersectionPattern</c>.</returns>
        public bool Relate(IGeometry g, string intersectionPattern)
        {
            return Relate(g).Matches(intersectionPattern);
        }

        /// <summary>
        /// Returns the DE-9IM intersection matrix for the two <c>Geometry</c>s.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns>
        /// A matrix describing the intersections of the interiors,
        /// boundaries and exteriors of the two <c>Geometry</c>s.
        /// </returns>
        public IntersectionMatrix Relate(IGeometry g) 
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(g);

            return RelateOp.Relate(this, g);
        }                    
                
        /// <summary>
        /// Returns <c>true</c> if the DE-9IM intersection matrix for the two
        /// <c>Geometry</c>s is T*F**FFF*.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are equal.</returns>
        public bool Equals(IGeometry g)
        {
            // NOTE: Not in JTS!!!
			if (IsEmpty && g.IsEmpty)
				return true;

            // Short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;

            // NOTE: Not in JTS!!!
            // We use an alternative method for compare GeometryCollections (but not subclasses!), 
            if (isGeometryCollection(this) || isGeometryCollection(g))
                return CompareGeometryCollections(this, g);
            
            // Use RelateOp comparation method
            return Relate(g).IsEquals(Dimension, g.Dimension);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private static bool CompareGeometryCollections(IGeometry obj1, IGeometry obj2)
        {
            IGeometryCollection coll1 = obj1 as IGeometryCollection;
            IGeometryCollection coll2 = obj2 as IGeometryCollection;
            if (coll1 == null || coll2 == null)
                return false;

            // Short-circuit test
            if (coll1.NumGeometries != coll2.NumGeometries)
                return false;

            // Deep test
            for (int i = 0; i < coll1.NumGeometries; i++)
            {
                IGeometry geom1 = coll1[i];
                IGeometry geom2 = coll2[i];
                if (!geom1.Equals(geom2))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (GetType().Namespace != obj.GetType().Namespace)
                return false;            
            return Equals((IGeometry) obj);         
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Geometry obj1, IGeometry obj2)
        {            
            return Equals(obj1, obj2); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Geometry obj1, IGeometry obj2)
        {
            return !(obj1 == obj2);
        }    

        /// <summary>
        /// 
        /// </summary>
        public override int GetHashCode()
        {
            int result = 17;            
            foreach (Coordinate coord in Coordinates)
                result = 37 * result + coord.X.GetHashCode();                        
            return result;
        } 

        /// <summary>
        /// Returns the Well-known Text representation of this <c>Geometry</c>.
        /// For a definition of the Well-known Text format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>
        /// The Well-known Text representation of this <c>Geometry</c>.
        /// </returns>
        public override string ToString() 
        {
            return ToText();
        }

        /// <summary>
        /// Returns the Well-known Text representation of this <c>Geometry</c>.
        /// For a definition of the Well-known Text format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>
        /// The Well-known Text representation of this <c>Geometry</c>.
        /// </returns>
        public string ToText() 
        {         
            WKTWriter writer = new WKTWriter();
            return writer.Write(this);
        }

        /// <summary>
        /// <see cref="ToText" />
        /// </summary>
        /// <returns></returns>
        public string AsText()
        {
            return ToText();
        }

        /// <summary>
        /// Returns the Well-known Binary representation of this <c>Geometry</c>.
        /// For a definition of the Well-known Binary format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>The Well-known Binary representation of this <c>Geometry</c>.</returns>
        public byte[] ToBinary()
        {
            WKBWriter writer = new WKBWriter();
            return writer.Write(this);
        }

        /// <summary>
        /// <see cref="ToBinary" />
        /// </summary>
        /// <returns></returns>
        public byte[] AsBinary()
        {
            return ToBinary();
        }

        /// <summary>
        /// Returns the feature representation as GML 2.1.1 XML document.
        /// This XML document is based on <c>Geometry.xsd</c> schema.
        /// NO features or XLink are implemented here!
        /// </summary>        
        public XmlReader ToGMLFeature()
        {            
            GMLWriter writer = new GMLWriter();
            return writer.Write(this);
        }        

        /// <summary>
        /// Returns a buffer region around this <c>Geometry</c> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the Geometry with a disc of radius <c>distance</c>.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <returns>
        /// All points whose distance from this <c>Geometry</c>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(double distance)
        {
            return BufferOp.Buffer(this, distance);
        }

        /// <summary>
        /// Returns a buffer region around this <c>Geometry</c> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the Geometry with a disc of radius <c>distance</c>.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// All points whose distance from this <c>Geometry</c>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(double distance, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, endCapStyle);
        }

        /// <summary>
        /// Returns a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <returns>
        /// All points whose distance from this <c>Geometry</c>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(double distance, int quadrantSegments) 
        {
            return BufferOp.Buffer(this, distance, quadrantSegments);
        }

        /// <summary>
        /// Returns a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// All points whose distance from this <c>Geometry</c>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(double distance, int quadrantSegments, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments, endCapStyle);
        } 

        /// <summary>
        /// Returns the smallest convex <c>Polygon</c> that contains all the
        /// points in the <c>Geometry</c>. This obviously applies only to <c>Geometry</c>
        /// s which contain 3 or more points.
        /// </summary>
        /// <returns>the minimum-area convex polygon containing this <c>Geometry</c>'s points.</returns>
        public virtual IGeometry ConvexHull()
        {            
            return (new ConvexHull(this)).GetConvexHull();         
        }

        /// <summary>
        /// Returns a <c>Geometry</c> representing the points shared by this
        /// <c>Geometry</c> and <c>other</c>.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the intersection.</param>
        /// <returns>The points common to the two <c>Geometry</c>s.</returns>
        public IGeometry Intersection(IGeometry other) 
        {
            // Special case: if one input is empty ==> empty
            if (IsEmpty) 
                return Factory.CreateGeometryCollection(null);
            if (other.IsEmpty) 
                return Factory.CreateGeometryCollection(null);


            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);        
            return SnapIfNeededOverlayOp.Overlay(this, other, SpatialFunction.Intersection);
        }

        /// <summary>
        /// Returns a <c>Geometry</c> representing all the points in this <c>Geometry</c>
        /// and <c>other</c>.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the union.</param>
        /// <returns>A set combining the points of this <c>Geometry</c> and the points of <c>other</c>.</returns>
        public IGeometry Union(IGeometry other) 
        {
            // Special case: if either input is empty ==> other input
            if (IsEmpty) 
                return (IGeometry) other.Clone();
            if (other.IsEmpty) 
                return (IGeometry) Clone();

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);
            return SnapIfNeededOverlayOp.Overlay(this, other, SpatialFunction.Union);
        }

        /// <summary>
        /// Returns a <c>Geometry</c> representing the points making up this
        /// <c>Geometry</c> that do not make up <c>other</c>. This method
        /// returns the closure of the resultant <c>Geometry</c>.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the difference.</param>
        /// <returns>The point set difference of this <c>Geometry</c> with <c>other</c>.</returns>
        public IGeometry Difference(IGeometry other)
        {
            // Special case: if A.isEmpty ==> empty; if B.isEmpty ==> A
            if (IsEmpty) 
                return Factory.CreateGeometryCollection(null);
            if (other.IsEmpty) 
                return (IGeometry) Clone();

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);
            return SnapIfNeededOverlayOp.Overlay(this, other, SpatialFunction.Difference);
         }

        /// <summary>
        /// Returns a set combining the points in this <c>Geometry</c> not in
        /// <c>other</c>, and the points in <c>other</c> not in this
        /// <c>Geometry</c>. This method returns the closure of the resultant
        /// <c>Geometry</c>.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the symmetric difference.</param>
        /// <returns>The point set symmetric difference of this <c>Geometry</c> with <c>other</c>.</returns>
        public IGeometry SymmetricDifference(IGeometry other) 
        {
            // Special case: if either input is empty ==> other input
            if (IsEmpty) 
                return (IGeometry) other.Clone();
            if (other.IsEmpty) 
                return (IGeometry) Clone();

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);
            return SnapIfNeededOverlayOp.Overlay(this, other, SpatialFunction.SymDifference);
        }

        /// <summary>
        /// Returns true if the two <c>Geometry</c>s are exactly equal,
        /// up to a specified tolerance.
        /// Two Geometries are exactly within a tolerance equal iff:
        /// they have the same class,
        /// they have the same values of Coordinates,
        /// within the given tolerance distance, in their internal
        /// Coordinate lists, in exactly the same order.
        /// If this and the other <c>Geometry</c>s are
        /// composites and any children are not <c>Geometry</c>s, returns
        /// false.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <param name="tolerance">Distance at or below which two Coordinates will be considered equal.</param>
        /// <returns>
        /// <c>true</c> if this and the other <c>Geometry</c>
        /// are of the same class and have equal internal data.
        /// </returns>
        public abstract bool EqualsExact(IGeometry other, double tolerance);

        /// <summary>
        /// Returns true if the two <c>Geometry</c>s are exactly equal.
        /// Two Geometries are exactly equal iff:
        /// they have the same class,
        /// they have the same values of Coordinates in their internal
        /// Coordinate lists, in exactly the same order.
        /// If this and the other <c>Geometry</c>s are
        /// composites and any children are not <c>Geometry</c>s, returns
        /// false.
        /// This provides a stricter test of equality than <c>equals</c>.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if this and the other <c>Geometry</c>
        /// are of the same class and have equal internal data.
        /// </returns>
        public bool EqualsExact(IGeometry other) 
        { 
            return EqualsExact(other, 0); 
        }

        /// <summary>
        /// Performs an operation with or on this <c>Geometry</c>'s
        /// coordinates. If you are using this method to modify the point, be sure
        /// to call GeometryChanged() afterwards. Note that you cannot use this
        /// method to
        /// modify this Geometry if its underlying CoordinateSequence's Get method
        /// returns a copy of the Coordinate, rather than the actual Coordinate stored
        /// (if it even stores Coordinates at all).
        /// </summary>
        /// <param name="filter">The filter to apply to this <c>Geometry</c>'s coordinates</param>
        public abstract void Apply(ICoordinateFilter filter);

        /// <summary>
        /// Performs an operation with or on this <c>Geometry</c> and its
        /// subelement <c>Geometry</c>s (if any).
        /// Only GeometryCollections and subclasses
        /// have subelement Geometry's.
        /// </summary>
        /// <param name="filter">
        /// The filter to apply to this <c>Geometry</c> (and
        /// its children, if it is a <c>GeometryCollection</c>).
        /// </param>
        public abstract void Apply(IGeometryFilter filter);

        /// <summary>
        /// Performs an operation with or on this Geometry and its
        /// component Geometry's. Only GeometryCollections and
        /// Polygons have component Geometry's; for Polygons they are the LinearRings
        /// of the shell and holes.
        /// </summary>
        /// <param name="filter">The filter to apply to this <c>Geometry</c>.</param>
        public abstract void Apply(IGeometryComponentFilter filter);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual object Clone() 
        {            
            Geometry clone = (Geometry) MemberwiseClone();
            if (clone.envelope != null) 
                clone.envelope = new Envelope(clone.envelope);                 
            return clone;         
        }

        /// <summary>
        /// Converts this <c>Geometry</c> to normal form (or 
        /// canonical form ). Normal form is a unique representation for <c>Geometry</c>
        /// s. It can be used to test whether two <c>Geometry</c>s are equal
        /// in a way that is independent of the ordering of the coordinates within
        /// them. Normal form equality is a stronger condition than topological
        /// equality, but weaker than pointwise equality. The definitions for normal
        /// form use the standard lexicographical ordering for coordinates. "Sorted in
        /// order of coordinates" means the obvious extension of this ordering to
        /// sequences of coordinates.
        /// </summary>
        public abstract void Normalize();

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>. 
        /// If their classes are different, they are compared using the following
        /// ordering:
        ///     Point (lowest),
        ///     MultiPoint,
        ///     LineString,
        ///     LinearRing,
        ///     MultiLineString,
        ///     Polygon,
        ///     MultiPolygon,
        ///     GeometryCollection (highest).
        /// If the two <c>Geometry</c>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </summary>
        /// <param name="o">A <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public int CompareTo(object o) 
        {
            return CompareTo((IGeometry) o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public int CompareTo(IGeometry geom)
        {
            Geometry other = (Geometry) geom;
            if (ClassSortIndex != other.ClassSortIndex)
                return ClassSortIndex - other.ClassSortIndex;
            if (IsEmpty && other.IsEmpty)
                return 0;
            if (IsEmpty)
                return -1;
            if (other.IsEmpty)
                return 1;
            return CompareToSameClass(geom);
        }

        /// <summary>
        /// Returns whether the two <c>Geometry</c>s are equal, from the point
        /// of view of the <c>EqualsExact</c> method. Called by <c>EqualsExact</c>
        /// . In general, two <c>Geometry</c> classes are considered to be
        /// "equivalent" only if they are the same class. An exception is <c>LineString</c>
        /// , which is considered to be equivalent to its subclasses.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compare this <c>Geometry</c> for equality.</param>
        /// <returns>
        /// <c>true</c> if the classes of the two <c>Geometry</c>
        /// s are considered to be equal by the <c>equalsExact</c> method.
        /// </returns>
        protected bool IsEquivalentClass(IGeometry other) 
        {
            return GetType().FullName == other.GetType().FullName;
        }

        /// <summary>
        /// Throws an exception if <c>g</c>'s class is <c>GeometryCollection</c>. 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> to check.</param>
        /// <exception cref="ArgumentException">
        /// if <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>
        protected void CheckNotGeometryCollection(IGeometry g) 
        {
            if (isGeometryCollection(g)) 
                throw new ArgumentException("This method does not support GeometryCollection arguments");                            
        }

        /// <summary>
        /// Returns <c>true</c> if <c>g</c>'s class is <c>GeometryCollection</c>. 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> to check.</param>
        /// <exception cref="ArgumentException">
        /// If <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>        
        private bool isGeometryCollection(IGeometry g)
        {
            return g.GetType().Name == "GeometryCollection" && g.GetType().Namespace == GetType().Namespace;
        }

        /// <summary>
        /// Returns the minimum and maximum x and y values in this <c>Geometry</c>
        /// , or a null <c>Envelope</c> if this <c>Geometry</c> is empty.
        /// Unlike <c>EnvelopeInternal</c>, this method calculates the <c>Envelope</c>
        /// each time it is called; <c>EnvelopeInternal</c> caches the result
        /// of this method.        
        /// </summary>
        /// <returns>
        /// This <c>Geometry</c>s bounding box; if the <c>Geometry</c>
        /// is empty, <c>Envelope.IsNull</c> will return <c>true</c>.
        /// </returns>
        protected abstract IEnvelope ComputeEnvelopeInternal();

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c> having the same class.
        /// </summary>
        /// <param name="o">A <c>Geometry</c> having the same class as this <c>Geometry</c>.</param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        protected internal abstract int CompareToSameClass(object o);

        /// <summary>
        /// Returns the first non-zero result of <c>CompareTo</c> encountered as
        /// the two <c>Collection</c>s are iterated over. If, by the time one of
        /// the iterations is complete, no non-zero result has been encountered,
        /// returns 0 if the other iteration is also complete. If <c>b</c>
        /// completes before <c>a</c>, a positive number is returned; if a
        /// before b, a negative number.
        /// </summary>
        /// <param name="a">A <c>Collection</c> of <c>IComparable</c>s.</param>
        /// <param name="b">A <c>Collection</c> of <c>IComparable</c>s.</param>
        /// <returns>The first non-zero <c>compareTo</c> result, if any; otherwise, zero.</returns>
        protected int Compare(ArrayList a, ArrayList b) 
        {
            IEnumerator i = a.GetEnumerator();
            IEnumerator j = b.GetEnumerator();

            while (i.MoveNext() && j.MoveNext()) 
            {
                IComparable aElement = (IComparable) i.Current;
                IComparable bElement = (IComparable) j.Current;
                int comparison = aElement.CompareTo(bElement);            
                if (comparison != 0)                 
                    return comparison;                
            }

            if (i.MoveNext())             
                return 1;            

            if (j.MoveNext())             
                return -1;            

            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        protected bool Equal(ICoordinate a, ICoordinate b, double tolerance) 
        {
            if (tolerance == 0)             
                return a.Equals(b);             

            return a.Distance(b) <= tolerance;
        }

        /// <summary>
        /// 
        /// </summary>
        private int ClassSortIndex 
        {
            get
            {
                for (int i = 0; i < SortedClasses.Length; i++)                
                    if (GetType().Equals(SortedClasses[i]))                                        
                        return i;                                    
                Assert.ShouldNeverReachHere(String.Format("Class not supported: {0}", GetType().FullName));
                return -1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        private IPoint CreatePointFromInternalCoord(ICoordinate coord, IGeometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRectangle
        {
            get
            {
                // Polygon overrides to check for actual rectangle
                return false;
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        /// <seealso cref="GeometryFactory.Default" />
        /// <seealso cref="GeometryFactory.Fixed"/>
        public static readonly IGeometryFactory DefaultFactory = GeometryFactory.Default;
        
        /* END ADDED BY MPAUL42: monoGIS team */

    }
}
