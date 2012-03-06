using System;
//using System.Collections;
using System.Collections.Generic;
using System.Xml;
using GeoAPI.Geometries;
using GeoAPI.IO;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.Operation;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Operation.Predicate;
using NetTopologySuite.Operation.Relate;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Utilities;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Geometries
{   
    /// <summary>  
    /// Basic implementation of <c>Geometry</c>.
    /// </summary>
    /// <remarks>
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
    /// <para>
    /// <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode" /> are not overridden, so that when two
    /// topologically equal Geometries are added to Collections and Dictionaries, they
    /// remain distinct. This behaviour is desired in many cases.
    /// </para>
    /// </remarks>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    public abstract class Geometry : IGeometry
    {        
        /// <summary>
        /// 
        /// </summary>
        private static Type[] _sortedClasses;/* = new[] 
        {
            typeof(Point),
            typeof(MultiPoint),
            typeof(LineString),
            typeof(LinearRing),
            typeof(MultiLineString),
            typeof(Polygon),
            typeof(MultiPolygon),
            typeof(GeometryCollection),    
        };                    */

        //FObermaier: not *readonly* due to SRID property in geometryfactory
        private /*readonly*/ IGeometryFactory _factory;

        /// <summary> 
        /// Gets the factory which contains the context in which this point was created.
        /// </summary>
        /// <returns>The factory for this point.</returns>
        public IGeometryFactory Factory
        {
            get 
            { 
                return _factory;
            }
        }

        /**
         * An object reference which can be used to carry ancillary data defined
         * by the client.
         */
        private object _userData;
        
        /// <summary> 
        /// Gets/Sets the user data object for this point, if any.
        /// </summary>
        /// <remarks>
        /// A simple scheme for applications to add their own custom data to a Geometry.
        /// An example use might be to add an object representing a Coordinate Reference System.
        /// Note that user data objects are not present in geometries created by
        /// construction methods.
        /// </remarks>
        public object UserData
        {
            get
            {                
                return _userData;
            }
            set
            {
                _userData = value;
            }
        }
           
        /// <summary>
        /// The bounding box of this <c>Geometry</c>.
        /// </summary>
        private Envelope _envelope;
       
        // The ID of the Spatial Reference System used by this <c>Geometry</c>
        private int _srid;

        /// <summary>  
        /// Sets the ID of the Spatial Reference System used by the <c>Geometry</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>NOTE:</b> This method should only be used for exceptional circumstances or 
        /// for backwards compatibility.  Normally the SRID should be set on the 
        /// <see cref="IGeometryFactory"/> used to create the geometry.
        /// SRIDs set using this method will <i>not</i> be propagated to 
        /// geometries returned by constructive methods.</para>
        /// </remarks>
        /// <seealso cref="IGeometryFactory"/>  
        /*
        /// NTS supports Spatial Reference System information in the simple way
        /// defined in the SFS. A Spatial Reference System ID (SRID) is present in
        /// each <c>Geometry</c> object. <c>Geometry</c> provides basic
        /// accessor operations for this field, but no others. The SRID is represented
        /// as an integer.
         */
        public int SRID
        {
            get 
            { 
                return _srid; 
            }
            set 
            {
                
                _srid = value;

                //Adjust the geometry factory
                _factory = GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory(
                    _factory.PrecisionModel, value, _factory.CoordinateSequenceFactory);

				var collection = this as IGeometryCollection;
                if (collection == null) return;

                foreach (var geometry in collection.Geometries)
                {
                    geometry.SRID = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected Geometry(IGeometryFactory factory)
        {
            _factory = factory;
            _srid = factory.SRID;
        }

        /// <summary>  
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>The name of this <c>Geometry</c>s most specific interface.</returns>
        public abstract string GeometryType { get; }

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public abstract OgcGeometryType OgcGeometryType { get; }

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
        /// Returns a vertex of this <c>Geometry</c>
        /// (usually, but not necessarily, the first one).
        /// </summary>
        /// <remarks>
        /// The returned coordinate should not be assumed to be an actual Coordinate object used in the internal representation. 
        /// </remarks>
        /// <returns>a Coordinate which is a vertex of this <c>Geometry</c>.</returns>
        /// <returns><c>null</c> if this Geometry is empty.
        /// </returns>
        public abstract Coordinate Coordinate { get; }

        /// <summary>
        /// Returns an array containing the values of all the vertices for 
        /// this geometry.
        /// </summary>
        /// <remarks>
        /// If the geometry is a composite, the array will contain all the vertices
        /// for the components, in the order in which the components occur in the geometry.
        /// <para>
        /// In general, the array cannot be assumed to be the actual internal 
        /// storage for the vertices.  Thus modifying the array
        /// may not modify the geometry itself. 
        /// Use the <see cref="ICoordinateSequence.SetOrdinate"/> method
        /// (possibly on the components) to modify the underlying data.
        /// If the coordinates are modified, 
        /// <see cref="IGeometry.GeometryChanged"/> must be called afterwards.
        /// </para> 
        /// </remarks>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        /// <seealso cref="IGeometry.GeometryChanged"/>
        /// <seealso cref="ICoordinateSequence.SetOrdinate"/>
        public abstract Coordinate[] Coordinates { get; }

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
        //public abstract bool IsSimple { get; }
        public bool IsSimple
        {
            get
            {
                CheckNotGeometryCollection(this);
                IsSimpleOp isSimpleOp = new IsSimpleOp(this);
                return isSimpleOp.IsSimple();
            }
        }

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
        /// <returns>The distance between the geometries</returns>
        /// <returns>0 if either input geometry is empty</returns>
        /// <exception cref="ArgumentException">if g is null</exception>
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

                Coordinate centPt = null;
                Dimension dim = Dimension;
                if (dim == Dimension.Point)
                {
                    CentroidPoint cent = new CentroidPoint();
                    cent.Add(this);
                    centPt = cent.Centroid;
                }
                else if (dim == Dimension.Curve)
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
        /// </summary>
        /// <remarks>
        /// An interior point is guaranteed to lie in the interior of the Geometry,
        /// if it possible to calculate such a point exactly. Otherwise,
        /// the point may lie on the boundary of the point.
        /// </remarks>
        /// <returns>A <c>Point</c> which is in the interior of this Geometry.</returns>
        public IPoint InteriorPoint
        {
            get
            {
                Coordinate interiorPt = null;
                Dimension dim = Dimension;
                if (dim == Dimension.Point)
                {
                    InteriorPointPoint intPt = new InteriorPointPoint(this);
                    interiorPt = intPt.InteriorPoint;
                }
                else if (dim == Dimension.Curve)
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

        private Dimension _dimension;

        /// <summary> 
        /// Returns the dimension of this geometry.
        /// </summary>
        /// <remarks>
        /// The dimension of a geometry is is the topological 
        /// dimension of its embedding in the 2-D Euclidean plane.
        /// In the NTS spatial model, dimension values are in the set {0,1,2}.
        /// <para>
        /// Note that this is a different concept to the dimension of 
        /// the vertex <see cref="Coordinate"/>s.
        /// The geometry dimension can never be greater than the coordinate dimension.
        /// For example, a 0-dimensional geometry (e.g. a Point) 
        /// may have a coordinate dimension of 3 (X,Y,Z). 
        /// </para>
        /// </remarks>
        /// <returns>  
        /// The topological dimensions of this geometry
        /// </returns>
        public virtual Dimension Dimension
        {
            get { return _dimension; }
            set { _dimension = value; }
        }


        /*private IGeometry boundary;*/

        /// <summary>  
        /// Returns the boundary, or an empty geometry of appropriate dimension 
        /// if this <c>Geometry</c> is empty. 
        /// For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        /// NOTE: make abstract, remove setter and change geoapi
        public virtual IGeometry Boundary { get; set; }

        /*private Dimensions boundaryDimension;*/

        /// <summary> 
        /// Returns the dimension of this <c>Geometry</c>s inherent boundary.
        /// </summary>
        /// <returns>    
        /// The dimension of the boundary of the class implementing this
        /// interface, whether or not this object is the empty point. Returns
        /// <c>Dimension.False</c> if the boundary is the empty point.
        /// </returns>
        /// NOTE: make abstract, remove setter and change geoapi
        public virtual Dimension BoundaryDimension { get; set; }

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
        /// Returns the minimum and maximum x and y values in this <c>Geometry</c>, or a null <c>Envelope</c> if this <c>Geometry</c> is empty.
        /// </summary>
        /// <returns>    
        /// This <c>Geometry</c>s bounding box; if the <c>Geometry</c>
        /// is empty, <c>Envelope.IsNull</c> will return <c>true</c>.
        /// </returns>
        public Envelope EnvelopeInternal
        {
            get
            {
                if (_envelope == null)
                    _envelope = ComputeEnvelopeInternal();
                return _envelope;
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
        /// party (for example, via a <see cref="ICoordinateFilter"/>).
        /// </summary>
        /// <remarks>
        /// When this method is called the geometry will flush
        /// and/or update any derived information it has cached (such as its <see cref="Envelope"/> ).
        /// </remarks>
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
            _envelope = null;
        }

        /// <summary>  
        /// Tests whether this geometry is disjoint from the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Disjoint</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>The DE-9IM intersection matrix for the two geometries matches <c>FF*FF****</c>.</item>
        /// <item><c>!g.intersects(this)</c><br/>(<c>Disjoint</c> is the inverse of <c>Intersects</c>)</item>
        /// </list>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are disjoint.</returns>
        /// <see cref="Intersects"/>
        public bool Disjoint(IGeometry g)
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return true;
            return Relate(g).IsDisjoint();
        }

        /// <summary>  
        /// Tests whether this geometry touches the specified geometry
        /// </summary>
        /// <remarks>
        /// The <c>Touches</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>The geometries have at least one point in common, but their interiors do not intersect</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches  <c>FT*******</c>, <c>F**T*****</c> or <c>F***T****</c>.</item>
        /// </list>
        /// If both geometries have dimension 0, this predicate returns <c>false</c>
        /// </remarks>
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
        /// Tests whether this geometry intersects the specified geometry.
        ///</summary>
        /// <remarks>
        /// The <c>Intersects</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>The two geometries have at least one point in common</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches<br/>
        /// <c>[T********]</c> or<br/>
        /// <c>[*T*******]</c> or<br/>
        /// <c>[***T*****]</c> or<br/>
        /// <c>[****T****]</c></item>
        /// <item> <c>!g.disjoint(this)</c><br/>
        /// (<c>Intersects</c> is the inverse of <c>Disjoint</c>)</item>
        /// </list></remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s intersect.</returns>
        /// <see cref="Disjoint"/>
        public bool Intersects(IGeometry g) 
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
           /*
            * TODO: (MD) Add optimizations:
            *
            * - for P-A case:
            * If P is in env(A), test for point-in-poly
            *
            * - for A-A case:
            * If env(A1).overlaps(env(A2))
            * test for overlaps via point-in-poly first (both ways)
            * Possibly optimize selection of point to test by finding point of A1
            * closest to centre of env(A2).
            * (Is there a test where we shouldn't bother - e.g. if env A
            * is much smaller than env B, maybe there's no point in testing
            * pt(B) in env(A)?
            */

            // optimizations for rectangle arguments
            if (IsRectangle)
                return RectangleIntersects.Intersects((IPolygon) this, g);
            if (g.IsRectangle)
                return RectangleIntersects.Intersects((IPolygon) g, this);
            return Relate(g).IsIntersects();
        }

        ///<summary>
        /// Tests whether this geometry crosses the specified geometry.
        ///</summary>
        /// <remarks>
        /// The <c>Crosses</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>The geometries have some but not all interior points in common.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches
        /// <list type="Table">
        /// <listheader><item>Code</item><description>Description</description></listheader>
        /// <item><c>[T*T******]</c></item><description>for P/L, P/A, and L/A situations</description>
        /// <item><c>[T*****T**]</c></item><description>for L/P, A/P, and A/L situations)</description>
        /// <item><c>[0********]</c></item><description>for L/L situations</description>
        /// </list>
        /// </item>
        /// </list>
        /// For any other combination of dimensions this predicate returns <code>false</code>.
        /// <para>
        /// The SFS defined this predicate only for P/L, P/A, L/L, and L/A situations.
        /// NTS extends the definition to apply to L/P, A/P and A/L situations as well,
        /// in order to make the relation symmetric.
        /// </para>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s cross.</returns>
        public bool Crosses(IGeometry g) 
        {
            // short-circuit test
            if (! EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsCrosses(Dimension, g.Dimension);
        }

        /// <summary>
        /// Tests whether this geometry is within the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <code>within</code> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>
        /// Every point of this geometry is a point of the other geometry,
        /// and the interiors of the two geometries have at least one point in common.
        /// </item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches <c>[T*F**F***]</c></item>
        /// <item><c>g.contains(this)</c><br/>(<c>Within</c> is the converse of <c>Contains</c>)</item>
        /// </list>
        /// <para>
        /// An implication of the definition is that "The boundary of a geometry is not within the Polygon".
        /// In other words, if a geometry A is a subset of the points in the boundary of a geometry B, <c>A.within(B) = false</c>
        /// </para>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if this <c>Geometry</c> is within <c>other</c>.</returns>
        /// <see cref="Contains"/>
        public bool Within(IGeometry g)
        {
            return g.Contains(this);
        }

        ///<summary>
        /// Tests whether this geometry contains the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Contains</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>Every point of the other geometry is a point of this geometry,
        /// and the interiors of the two geometries have at least one point in common.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches
        /// <c>[T*****FF*]</c></item>
        /// <item><c>g.within(this)</c><br/>
        /// (<c>Contains</c> is the converse of <c>within</c>)</item>
        /// </list>
        /// <para>
        /// An implication of the definition is that "Geometries do not
        /// contain their boundary".  In other words, if a geometry A is a subset of
        /// the points in the boundary of a geometry B, <c>B.contains(A) = false</c>
        /// </para>
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if this <c>Geometry</c> contains <c>g</c></returns>
        /// <see cref="Within"/>
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

        /*
   * <li>The geometries have at least one point each not shared by the other
   * (or equivalently neither covers the other),
         */
        /// <summary>
        /// Tests whether this geometry overlaps the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Overlaps</c> predicate has the following equivalent definitions:
        /// <list type="Bullet">
        /// <item>The geometries have at least one point each not shared by the other (or equivalently neither covers the other),
        /// they have the same dimension,
        /// and the intersection of the interiors of the two geometries has
        /// the same dimension as the geometries themselves.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches
        ///  <c>[T*T***T**]</c> (for two points or two surfaces)
        ///  or <c>[1*T***T**]</c> (for two curves)</item>
        /// </list>
        /// If the geometries are of different dimension this predicate returns <c>false</c>.
        /// </remarks>
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
        /// Tests whether this geometry covers the specified geometry
        /// </summary>
        /// <remarks>
        /// The <c>covers</c> predicate has the following equivalent definitions:
        /// <list>
        /// <item>Every point of the other geometry is a point of this geometry.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches<br/>
        /// <c>[T*****FF*]</c> or<br/>
        /// <c>[*T****FF*]</c> or<br/>
        /// <c>[***T**FF*]</c> or<br/>
        /// <c>[****T*FF*]</c>
        /// </item>
        /// <item><c>g.coveredBy(this)</c>
        /// (<c>covers</c> is the converse of <c>coveredBy</c>)</item>
        /// </list>
        /// If either geometry is empty, the value of this predicate is <c>false</c>.
        /// <para>
        /// This predicate is similar to <see cref="Contains"/>,
        /// but is more inclusive (i.e. returns <c>true</c> for more cases).
        /// In particular, unlike <c>Contains</c> it does not distinguish between
        /// points in the boundary and in the interior of geometries.
        /// For most situations, <c>Covers</c> should be used in preference to <c>Contains</c>.
        /// As an added benefit, <c>Covers</c> is more amenable to optimization,
        /// and hence should be more performant.
        /// </para>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if this <c>Geometry</c> covers <paramref name="g" /></returns>
        /// <seealso cref="Contains" />
        /// <seealso cref="CoveredBy" />
        public bool Covers(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Covers(g.EnvelopeInternal))
                return false;
            
            // optimization for rectangle arguments
            if (IsRectangle)
                // since we have already tested that the test envelope is covered
                return true;
            
            return Relate(g).IsCovers();
        }

        ///<summary>Tests whether this geometry is covered by the specified geometry.</summary>
        /// <remarks>
        /// The <c>CoveredBy</c> predicate has the following equivalent definitions:
        /// <list>
        /// <item>Every point of this geometry is a point of the other geometry.
        /// </item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches
        /// <c>[T*F**F***]</c> or<br/>
        /// <c>[*TF**F***]</c> or<br/>
        /// <c>[**FT*F***]</c> or<br/>
        /// <c>[**F*TF***]</c></item>
        /// <item><c>g.covers(this)</c><br/>
        /// (<c>CoveredBy</c> is the converse of <c>Covers</c>)
        /// </item>
        /// </list>
        /// If either geometry is empty, the value of this predicate is <c>false</c>.
        /// <para>
        /// This predicate is similar to <see cref="Within"/>, 
        /// but is more inclusive (i.e. returns <c>true</c> for more cases).
        /// </para>
        ///</remarks>
        ///<param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        ///<returns><c>true</c> if this <c>Geometry</c> is covered by <c>g</c></returns>
        ///<seealso cref="Within"/>
        ///<seealso cref="Covers"/>
        public bool CoveredBy(IGeometry g)
        {
            return g.Covers(this);
        }

        ///<summary>
        /// Tests whether the elements in the DE-9IM
        /// <see cref="IntersectionMatrix"/> for the two <c>Geometry</c>s match the elements in <c>intersectionPattern</c>.
        /// </summary>
        /// <remarks>
        /// The pattern is a 9-character string, with symbols drawn from the following set:
        /// <list>
        ///<item>0 (dimension 0)</item>
        ///<item>1 (dimension 1)</item>
        ///<item>2 (dimension 2)</item>
        ///<item>T ( matches 0, 1 or 2)</item>
        ///<item>F ( matches FALSE)</item>
        ///<item>* ( matches any value)</item>
        /// </list> For more information on the DE-9IM, see the <i>OpenGIS Simple Features 
        /// Specification</i>.
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <param name="intersectionPattern">the pattern against which to check the 
        /// intersection matrix for the two <c>Geometry</c>s</param>
        /// <returns><c>true</c> if the DE-9IM intersection 
        /// matrix for the two <c>Geometry</c>s match <c>intersectionPattern</c></returns>
        /// <seealso cref="IntersectionMatrix"/>
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
        /// Tests whether this geometry is equal to the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>equals</c> predicate has the following equivalent definitions:
        /// <list>
        /// <item>The two geometries have at least one point in common, and no point of either geometry lies in the exterior of the other geometry.
        /// 
        /// </item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries is <c>[T*F**FFF*]</c>
        /// </item>
        /// </list>
        /// <b>Note</b> that this method computes topologically equality, not structural or
        /// vertex-wise equality.
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are equal.</returns>
        public bool Equals(IGeometry g)
        {
            if (Equals(g, null))
                return false;

            if (ReferenceEquals(g, this))
                return true;

            // NOTE: Not in JTS!!!
			if (IsEmpty && g.IsEmpty)
				return true;

            // Short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;

            // NOTE: Not in JTS!!!
            if (IsGeometryCollection(this) || IsGeometryCollection(g))
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
            //These checks would mean double effort 
            //(except for namespace, but I don't see the use for it)
            /* 
            if (obj == null)
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            if (GetType().Namespace != obj.GetType().Namespace)
                return false;           
            if (!(obj is IGeometry))
                return false;
            return Equals((IGeometry) obj);         
             */
            return Equals(obj as IGeometry);
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
            return GetHashCodeInternal(result, x => 37 * x );
            /*
            foreach (var coord in Coordinates)
                result = 37 * result + coord.X.GetHashCode();                        
             */
        }

        internal abstract int GetHashCodeInternal(int baseValue, Func<int, int> operation);

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
        /// Computes a buffer area around this geometry having the given width. The
        /// buffer of a Geometry is the Minkowski sum or difference of the geometry
        /// with a disc of radius <c>Abs(distance)</c>.
        /// </summary>
        /// <remarks><para>Mathematically-exact buffer area boundaries can contain circular arcs. 
        /// To represent these arcs using linear geometry they must be approximated with line segments.
        /// The buffer geometry is constructed using 8 segments per quadrant to approximate 
        /// the circular arcs.</para>
        /// <para>The end cap style is <c>BufferStyle.CapRound</c>.</para>
        /// <para>
        /// The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.
        /// This is also the result for the buffers of degenerate (zero-area) polygons.
        /// </para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer (may be positive, negative or 0), interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public IGeometry Buffer(double distance)
        {
            return BufferOp.Buffer(this, distance);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the geometry
        /// with a disc of radius <c>Abs(distance)</c>.
        /// </summary>
        /// <remarks>
        /// <para>The end cap style specifies the buffer geometry that will be
        /// created at the ends of linestrings.  The styles provided are:
        /// <ul>
        /// <li><see cref="BufferStyle.CapRound" /> - (default) a semi-circle</li>
        /// <li><see cref="BufferStyle.CapButt" /> - a straight line perpendicular to the end segment</li>
        /// <li><see cref="BufferStyle.CapSquare" /> - a half-square</li>
        /// </ul></para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.</para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        [Obsolete]
        public IGeometry Buffer(double distance, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, BufferParameters.DefaultQuadrantSegments, endCapStyle);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the geometry
        /// with a disc of radius <c>Abs(distance)</c>.
        /// </summary>
        /// <remarks>
        /// <para>The end cap style specifies the buffer geometry that will be
        /// created at the ends of linestrings.  The styles provided are:
        /// <ul>
        /// <li><see cref="EndCapStyle.Round" /> - (default) a semi-circle</li>
        /// <li><see cref="EndCapStyle.Flat" /> - a straight line perpendicular to the end segment</li>
        /// <li><see cref="EndCapStyle.Square" /> - a half-square</li>
        /// </ul></para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.</para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public IGeometry Buffer(double distance, EndCapStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, BufferParameters.DefaultQuadrantSegments, (BufferStyle)endCapStyle);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified accuracy of approximation for circular arcs.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <remarks><para>Mathematically-exact buffer area boundaries can contain circular arcs. 
        /// To represent these arcs using linear geometry they must be approximated with line segments.
        /// The <c>quadrantSegments</c> argument allows controlling the accuracy of
        /// the approximation by specifying the number of line segments used to
        /// represent a quadrant of a circle</para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.
        /// This is also the result for the buffers of degenerate (zero-area) polygons.
        /// </para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer (may be positive, negative or 0), interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public IGeometry Buffer(double distance, int quadrantSegments) 
        {
            return BufferOp.Buffer(this, distance, quadrantSegments);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <remarks><para>Mathematically-exact buffer area boundaries can contain circular arcs. 
        /// To represent these arcs using linear geometry they must be approximated with line segments.
        /// The <c>quadrantSegments</c> argument allows controlling the accuracy of
        /// the approximation by specifying the number of line segments used to
        /// represent a quadrant of a circle</para>
        /// <para>The end cap style specifies the buffer geometry that will be
        /// created at the ends of linestrings.  The styles provided are:
        /// <ul>
        /// <li><see cref="BufferStyle.CapRound" /> - (default) a semi-circle</li>
        /// <li><see cref="BufferStyle.CapButt" /> - a straight line perpendicular to the end segment</li>
        /// <li><see cref="BufferStyle.CapSquare" /> - a half-square</li>
        /// </ul></para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.
        /// This is also the result for the buffers of degenerate (zero-area) polygons.
        /// </para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        [Obsolete]
        public IGeometry Buffer(double distance, int quadrantSegments, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments, endCapStyle);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <remarks><para>Mathematically-exact buffer area boundaries can contain circular arcs. 
        /// To represent these arcs using linear geometry they must be approximated with line segments.
        /// The <c>quadrantSegments</c> argument allows controlling the accuracy of
        /// the approximation by specifying the number of line segments used to
        /// represent a quadrant of a circle</para>
        /// <para>The end cap style specifies the buffer geometry that will be
        /// created at the ends of linestrings.  The styles provided are:
        /// <ul>
        /// <li><see cref="EndCapStyle.Round" /> - (default) a semi-circle</li>
        /// <li><see cref="EndCapStyle.Flat" /> - a straight line perpendicular to the end segment</li>
        /// <li><see cref="EndCapStyle.Square" /> - a half-square</li>
        /// </ul></para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.
        /// This is also the result for the buffers of degenerate (zero-area) polygons.
        /// </para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, IBufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        public IGeometry Buffer(double distance, int quadrantSegments, EndCapStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments, (BufferStyle)endCapStyle);
        }

        /// <summary>
        /// Computes a buffer region around this <c>Geometry</c> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <remarks><para>Mathematically-exact buffer area boundaries can contain circular arcs. 
        /// To represent these arcs using linear geometry they must be approximated with line segments.
        /// The <c>bufferParameters</c> argument has a property <c>QuadrantSegments</c> controlling the accuracy of
        /// the approximation by specifying the number of line segments used to
        /// represent a quadrant of a circle</para>
        /// <para>The <c>EndCapStyle</c> property of the <c>bufferParameters</c> argument specifies the buffer geometry that will be
        /// created at the ends of linestrings.  The styles provided are:
        /// <ul>
        /// <li><see cref="EndCapStyle.Round" /> - (default) a semi-circle</li>
        /// <li><see cref="EndCapStyle.Flat" /> - a straight line perpendicular to the end segment</li>
        /// <li><see cref="EndCapStyle.Square" /> - a half-square</li>
        /// </ul></para>
        /// <para>The buffer operation always returns a polygonal result. The negative or
        /// zero-distance buffer of lines and points is always an empty <see cref="IPolygonal"/>.
        ///	This is also the result for the buffers of degenerate (zero-area) polygons.
        /// </para>
        /// </remarks>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <c>PrecisionModel</c> of the <c>Geometry</c>.
        /// </param>
        /// <param name="bufferParameters">This argument type has a number of properties that control the construction of the
        /// buffer, including <c>QuadrantSegments</c>, <c>EndCapStyle</c>, <c>JoinStyle</c>, and <c>MitreLimit</c></param>
        /// <returns>
        /// a polygonal geometry representing the buffer region (which may be empty)
        /// </returns>
        /// <exception cref="TopologyException">If a robustness error occurs</exception>
        /// <seealso cref="Buffer(double)"/>
        /// <seealso cref="Buffer(double, BufferStyle)"/>
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, BufferStyle)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public IGeometry Buffer(double distance, IBufferParameters bufferParameters)
        {
            return BufferOp.Buffer(this, distance, bufferParameters);
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

        ///<summary>
        /// Computes a new geometry which has all component coordinate sequences
        /// in reverse order (opposite orientation) to this one.
        ///</summary>
        /// <returns>A reversed geometry</returns>
        public abstract IGeometry Reverse();

        /// <summary>
        /// Returns a <c>Geometry</c> representing the points shared by this
        /// <c>Geometry</c> and <c>other</c>.
        /// </summary>
        /// <remarks>
        /// <see cref="IGeometryCollection"/>s support intersection with 
        /// homogeneous collection types, with the semantics that
        /// the result is a <see cref="IGeometryCollection"/> of the
        /// intersection of each element of the target with the argument. 
        /// </remarks>
        /// <param name="other">The <c>Geometry</c> with which to compute the intersection.</param>
        /// <returns>The points common to the two <c>Geometry</c>s.</returns>
        /// <exception cref="TopologyException">if a robustness error occurs.</exception>
        /// <exception cref="ArgumentException">if the argument is a non-empty geometry collectiont</exception>
        public IGeometry Intersection(IGeometry other) 
        {
            // Special case: if one input is empty ==> empty
            if (IsEmpty) 
                return Factory.CreateGeometryCollection(null);
            if (other.IsEmpty) 
                return Factory.CreateGeometryCollection(null);

            // compute for GCs
            if (IsGeometryCollection(this))
            {
                IGeometry g2 = other;
                return GeometryCollectionMapper.Map(
                    (IGeometryCollection) this, g => g.Intersection(g2));
            }
        //    if (isGeometryCollection(other))
        //      return other.intersection(this);
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
            // mod to handle empty cases better - return type of input
            //if (IsEmpty) || other.isEmpty()) return (IGeometry) clone();

            // Special case: if A.isEmpty ==> empty; if B.isEmpty ==> A
            if (IsEmpty) 
                return Factory.CreateGeometryCollection(null);
            if (other == null || other.IsEmpty) 
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

        ///<summary> 
        /// Computes the union of all the elements of this geometry. Heterogeneous
        /// <see cref="IGeometryCollection"/>s are fully supported.
        ///</summary>
        /// <remarks>
        /// The result obeys the following contract:
        /// <list type="Bullet">
        /// <item>Unioning a set of <see cref="ILineString"/>s has the effect of fully noding and dissolving the linework.</item>
        /// <item>Unioning a set of <see cref="IPolygon"/>s will always return a <see cref="IPolygonal"/> geometry (unlike <see cref="Union(IGeometry)"/>), which may return geometrys of lower dimension if a topology collapse occurred.</item>
        /// </list>
        /// </remarks>
        public IGeometry Union()
        {
            return UnaryUnionOp.Union(this);
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
        /// Performs an operation with or on this <c>Geometry</c>'s coordinates. 
        /// </summary>
        /// <remarks>
        /// If this method modifies any coordinate values,
        /// <see cref="GeometryChanged"/> must be called to update the geometry state. 
        /// Note that you cannot use this method to
        /// modify this Geometry if its underlying CoordinateSequence's #get method
        /// returns a copy of the Coordinate, rather than the actual Coordinate stored
        /// (if it even stores Coordinate objects at all).
        /// </remarks>
        /// <param name="filter">The filter to apply to this <c>Geometry</c>'s coordinates</param>
        public abstract void Apply(ICoordinateFilter filter);

        ///<summary>
        /// Performs an operation on the coordinates in this <c>Geometry</c>'s <see cref="ICoordinateSequence"/>s.
        /// </summary>
        /// <remarks>
        /// If the filter reports that a coordinate value has been changed, 
        /// <see cref="GeometryChanged"/> will be called automatically.
        ///</remarks>
        /// <param name="filter">The filter to apply</param>
        public abstract void Apply(ICoordinateSequenceFilter filter);

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
            if (clone._envelope != null) 
                clone._envelope = new Envelope(clone._envelope);                 
            return clone;         
        }

        /// <summary>
        /// Converts this <c>Geometry</c> to normal form (or canonical form ).
        /// </summary>
        /// <remarks>
        /// <para>
        /// Normal form is a unique representation for <c>Geometry</c>s. 
        /// It can be used to test whether two <c>Geometry</c>s are equal
        /// in a way that is independent of the ordering of the coordinates within
        /// them. Normal form equality is a stronger condition than topological
        /// equality, but weaker than pointwise equality.</para>
        /// <para>The definitions for normal
        /// form use the standard lexicographical ordering for coordinates. "Sorted in
        /// order of coordinates" means the obvious extension of this ordering to
        /// sequences of coordinates.</para>
        /// </remarks>
        public abstract void Normalize();

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list>
        /// <item>Point (lowest),</item>
        /// <item>MultiPoint,</item>
        /// <item>LineString,</item>
        /// <item>LinearRing,</item>
        /// <item>MultiLineString,</item>
        /// <item>Polygon,</item>
        /// <item>MultiPolygon,</item>
        /// <item>GeometryCollection (highest).</item>
        /// </list>
        /// If the two <c>Geometry</c>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        /// <param name="o">A <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public int CompareTo(object o) 
        {
            return CompareTo(o as IGeometry);
        }

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list>
        /// <item>Point (lowest),</item>
        /// <item>MultiPoint,</item>
        /// <item>LineString,</item>
        /// <item>LinearRing,</item>
        /// <item>MultiLineString,</item>
        /// <item>Polygon,</item>
        /// <item>MultiPolygon,</item>
        /// <item>GeometryCollection (highest).</item>
        /// </list>
        /// If the two <c>Geometry</c>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        /// <param name="geom">A <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public int CompareTo(IGeometry geom)
        {
            Geometry other =  geom as Geometry;
            if (other == null)
                return -1;

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
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>, using the given <see paramref="IComparer{ICoordinateSequence}"/>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list>
        /// <item>Point (lowest),</item>
        /// <item>MultiPoint,</item>
        /// <item>LineString,</item>
        /// <item>LinearRing,</item>
        /// <item>MultiLineString,</item>
        /// <item>Polygon,</item>
        /// <item>MultiPolygon,</item>
        /// <item>GeometryCollection (highest).</item>
        /// </list>
        /// If the two <c>Geometry</c>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        /// <param name="o">A <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <param name="comp">A <c>IComparer&lt;ICoordinateSequence&gt;</c></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public int CompareTo(Object o, IComparer<ICoordinateSequence> comp)
        {
            Geometry other = (Geometry)o;
            if (ClassSortIndex != other.ClassSortIndex)
            {
                return ClassSortIndex - other.ClassSortIndex;
            }
            if (IsEmpty && other.IsEmpty)
            {
                return 0;
            }
            if (IsEmpty)
            {
                return -1;
            }
            if (other.IsEmpty)
            {
                return 1;
            }
            return CompareToSameClass(o, comp);
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
            if (IsNonHomogenousGeometryCollection(g)) 
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
        private static bool IsNonHomogenousGeometryCollection(IGeometry g)
        {
            return 
                g is IGeometryCollection &&
                g.GeometryType == "GeometryCollection"; ; /*g.GetType().Name == "GeometryCollection" && g.GetType().Namespace == GetType().Namespace;*/
        }

        
        protected static bool IsGeometryCollection(IGeometry g)
        {
            return IsNonHomogenousGeometryCollection(g);
        }
        

        /// <summary>
        /// Returns the minimum and maximum x and y values in this <c>Geometry</c>,
        /// or a null <c>Envelope</c> if this <c>Geometry</c> is empty.
        /// Unlike <c>EnvelopeInternal</c>, this method calculates the <c>Envelope</c>
        /// each time it is called; <c>EnvelopeInternal</c> caches the result
        /// of this method.        
        /// </summary>
        /// <returns>
        /// This <c>Geometry</c>s bounding box; if the <c>Geometry</c>
        /// is empty, <c>Envelope.IsNull</c> will return <c>true</c>.
        /// </returns>
        protected abstract Envelope ComputeEnvelopeInternal();

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

        ///<summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c> of the same class.
        /// using the given <see cref="IComparer{ICoordinateSequence}"/>.
        ///</summary>
        /// <param name="o">A <c>Geometry</c> having the same class as this <c>Geometry</c></param>
        /// <param name="comp">The comparer</param>
        /// <returns>A positive number, 0, or a negative number, depending on whether
        ///      this object is greater than, equal to, or less than <code>o</code>, as
        ///      defined in "Normal Form For Geometry" in the JTS Technical
        ///      Specifications
        /// </returns>
        protected internal abstract int CompareToSameClass(Object o, IComparer<ICoordinateSequence> comp);

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
        protected static int Compare(List<IGeometry> a, List<IGeometry> b) 
        {
            IEnumerator<IGeometry> i = a.GetEnumerator();
            IEnumerator<IGeometry> j = b.GetEnumerator();

            while (i.MoveNext() && j.MoveNext()) 
            {
                IComparable aElement = i.Current;
                IComparable bElement = j.Current;
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
        protected static bool Equal(Coordinate a, Coordinate b, double tolerance) 
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
                if (_sortedClasses == null)
                    InitSortedClasses();
                for (int i = 0; i < _sortedClasses.Length; i++)                
                    if (GetType().Equals(_sortedClasses[i]))                                        
                        return i;                                    
                Assert.ShouldNeverReachHere(String.Format("Class not supported: {0}", GetType().FullName));
                return -1;
            }
        }

        private static void InitSortedClasses()
        {
            _sortedClasses = new []
                                 {
                                     typeof (Point),
                                     typeof (MultiPoint),
                                     typeof (LineString),
                                     typeof (LinearRing),
                                     typeof (MultiLineString),
                                     typeof (Polygon),
                                     typeof (MultiPolygon),
                                     typeof (GeometryCollection),
                                 };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        private static IPoint CreatePointFromInternalCoord(Coordinate coord, IGeometry exemplar)
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
