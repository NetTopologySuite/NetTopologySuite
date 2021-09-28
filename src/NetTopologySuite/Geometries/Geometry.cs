using System;
using System.Collections.Generic;
using System.Xml;
using NetTopologySuite.Algorithm;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.Operation;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.Predicate;
using NetTopologySuite.Operation.Relate;
using NetTopologySuite.Operation.Valid;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A representation of a planar, linear vector geometry.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <h3>Binary Predicates:</h3>
    /// Because it is not clear at this time what semantics for spatial
    /// analysis methods involving <c>GeometryCollection</c>s would be useful,
    /// <c>GeometryCollection</c>s are not supported as arguments to binary
    /// predicates or the <c>Relate</c> method.
    /// </para>
    /// <para>
    /// <h3>Overlay Methods:</h3>
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
    /// specification precise and unambiguous. NTS uses a canonical form for
    /// <c>Geometry</c>s returned from overlay methods. The canonical
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
    ///
    /// <h3>Geometry Equality</h3>
    /// There are two ways of comparing geometries for equality:
    /// <b>structural equality</b> and <b>topological equality</b>.
    /// <h4>Structural Equality</h4>
    /// Structural Equality is provided by the
    /// <see cref="EqualsExact(Geometry)"/> method.
    /// This implements a comparison based on exact, structural pointwise
    /// equality.
    /// The <see cref="Equals(object)"/> is a synonym for this method,
    /// to provide structural equality semantics for
    /// use in collections.
    /// It is important to note that structural pointwise equality
    /// is easily affected by things like
    /// ring order and component order.  In many situations
    /// it will be desirable to normalize geometries before
    /// comparing them (using the <see cref="Normalized()"/>
    /// or <see cref="Normalize()"/> methods).
    /// <see cref="EqualsNormalized(Geometry)"/> is provided
    /// as a convenience method to compute equality over
    /// normalized geometries, but it is expensive to use.
    /// Finally, <see cref="EqualsExact(Geometry, double)"/>
    /// allows using a tolerance value for point comparison.
    ///
    /// <h4>Topological Equality</h4>
    /// Topological Equality is provided by the
    /// <see cref="EqualsTopologically(Geometry)"/> method.
    /// It implements the SFS definition of point-set equality
    /// defined in terms of the DE-9IM matrix.
    /// To support the SFS naming convention, the method
    /// <see cref="Equals(Geometry)"/> is also provided as a synonym.
    /// However, due to the potential for confusion with <see cref="Equals(object)"/>
    /// its use is discouraged.
    /// </para>
    /// <para>
    /// Since <see cref="Equals(object)"/> and <see cref="GetHashCode"/> are overridden,
    /// Geometries can be used effectively in .Net collections.
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class Geometry : IComparable, IComparable<Geometry>
    {
        /// <summary>
        /// An enumeration of sort values for geometries
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 the values in this enum have been renamed to 'TYPECODE...'
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected enum SortIndexValue
        {
            /// <summary>Sort hierarchy value of a <see cref="Point"/></summary>
            Point = 0,
            /// <summary>Sort hierarchy value of a <see cref="MultiPoint"/></summary>
            MultiPoint = 1,
            /// <summary>Sort hierarchy value of a <see cref="LineString"/></summary>
            LineString = 2,
            /// <summary>Sort hierarchy value of a <see cref="LinearRing"/></summary>
            LinearRing = 3,
            /// <summary>Sort hierarchy value of a <see cref="MultiLineString"/></summary>
            MultiLineString = 4,
            /// <summary>Sort hierarchy value of a <see cref="Polygon"/></summary>
            Polygon = 5,
            /// <summary>Sort hierarchy value of a <see cref="MultiPolygon"/></summary>
            MultiPolygon = 6,
            /// <summary>Sort hierarchy value of a <see cref="GeometryCollection"/></summary>
            GeometryCollection = 7
        }

        /// <summary>
        /// The name of point geometries
        /// </summary>
        public const string TypeNamePoint = "Point";
        /// <summary>
        /// The name of multi-point geometries
        /// </summary>
        public const string TypeNameMultiPoint = "MultiPoint";
        /// <summary>
        /// The name of linestring geometries
        /// </summary>
        public const string TypeNameLineString = "LineString";
        /// <summary>
        /// The name of linearring geometries
        /// </summary>
        public const string TypeNameLinearRing = "LinearRing";
        /// <summary>
        /// The name of multi-linestring geometries
        /// </summary>
        public const string TypeNameMultiLineString = "MultiLineString";
        /// <summary>
        /// The name of polygon geometries
        /// </summary>
        public const string TypeNamePolygon = "Polygon";
        /// <summary>
        /// The name of multi-polygon geometries
        /// </summary>
        public const string TypeNameMultiPolygon = "MultiPolygon";
        /// <summary>
        /// The name of geometry collection geometries.
        /// </summary>
        public const string TypeNameGeometryCollection = "GeometryCollection";

        //FObermaier: not *readonly* due to SRID property in geometryfactory
        private /*readonly*/ GeometryFactory _factory;

        /// <summary>
        /// Gets the factory which contains the context in which this point was created.
        /// </summary>
        /// <returns>The factory for this point.</returns>
        public GeometryFactory Factory => _factory;

        /*
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
            get => _userData;
            set => _userData = value;
        }

        /// <summary>
        /// The bounding box of this <c>Geometry</c>.
        /// </summary>
        private Envelope _envelope;

        /// <summary>
        /// Sets the ID of the Spatial Reference System used by the <c>Geometry</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>NOTE:</b> This method should only be used for exceptional circumstances or
        /// for backwards compatibility.  Normally the <c>SRID</c> should be set on the
        /// <see cref="GeometryFactory"/> used to <b>create</b> the geometry.
        /// <c>SRID</c>s set using this method will change the <see cref="Factory"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GeometryFactory"/>
        /*
        /// NTS supports Spatial Reference System information in the simple way
        /// defined in the SFS. A Spatial Reference System ID (SRID) is present in
        /// each <c>Geometry</c> object. <c>Geometry</c> provides basic
        /// accessor operations for this field, but no others. The SRID is represented
        /// as an integer.
         */
        public int SRID
        {
            get => _factory.SRID;
            set
            {
                if (value == _factory.SRID)
                    return;

                // Adjust the geometry factory
                _factory = _factory.WithSRID(value);

                var collection = this as GeometryCollection;
                if (collection == null) return;

                foreach (var geometry in collection.Geometries)
                {
                    geometry.SRID = value;
                }
            }
        }

        /// <summary>
        /// Creates a new <c>Geometry</c> via the specified GeometryFactory.
        /// </summary>
        /// <param name="factory">The factory</param>
        protected Geometry(GeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Returns the name of this Geometry's actual class.
        /// </summary>
        /// <returns>The name of this <c>Geometry</c>s actual class.</returns>
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
        protected static bool HasNonEmptyElements(Geometry[] geometries)
        {
            foreach (var g in geometries)
                if (!g.IsEmpty)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if the array contains any <c>null</c> elements.
        /// </summary>
        /// <param name="array"> an array to validate.</param>
        /// <returns><c>true</c> if any of <c>array</c>s elements are <c>null</c>.</returns>
        [Obsolete("Use HasNullElements<T>")]
        public static bool HasNullElements(object[] array)
        {
            foreach (object o in array)
                if (o == null)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if the array contains any <c>null</c> elements.
        /// </summary>
        /// <param name="array"> an array to validate.</param>
        /// <returns><c>true</c> if any of <c>array</c>s elements are <c>null</c>.</returns>
        public static bool HasNullElements<T>(IEnumerable<T> array) where T:class
        {
            foreach (var o in array)
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
        public PrecisionModel PrecisionModel => Factory.PrecisionModel;

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
        /// Use the <see cref="CoordinateSequence.SetOrdinate(int, int, double)"/> or
        /// <see cref="CoordinateSequence.SetOrdinate(int, Ordinate, double)"/> method
        /// (possibly on the components) to modify the underlying data.
        /// If the coordinates are modified,
        /// <see cref="Geometry.GeometryChanged"/> must be called afterwards.
        /// </para>
        /// </remarks>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        /// <seealso cref="Geometry.GeometryChanged"/>
        /// <seealso cref="CoordinateSequence.SetOrdinate(int, int, double)"/>
        /// <seealso cref="CoordinateSequence.SetOrdinate(int, Ordinate, double)"/>
        public abstract Coordinate[] Coordinates { get; }

        /// <summary>
        /// Gets an array of <see cref="double"/> ordinate values
        /// </summary>
        /// <param name="ordinate">The ordinate index</param>
        /// <returns>An array of ordinate values</returns>
        public abstract double[] GetOrdinates(Ordinate ordinate);

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
        public virtual int NumGeometries => 1;

        /// <summary>
        /// Returns an element Geometry from a GeometryCollection,
        /// or <c>this</c>, if the geometry is not a collection.
        /// </summary>
        /// <param name="n">The index of the geometry element.</param>
        /// <returns>The n'th geometry contained in this geometry.</returns>
        public virtual Geometry GetGeometryN(int n)
        {
            return this;
        }

        /// <summary>
        /// Tests whether this <see cref="Geometry"/> is simple.
        /// <para/>
        /// The SFS definition of simplicity
        /// follows the general rule that a Geometry is simple if it has no points of
        /// self-tangency, self-intersection or other anomalous points.
        /// <para/>
        /// Simplicity is defined for each <see cref="Geometry"/> subclass as follows:
        /// <list type="bullet">
        /// <item><description>Valid polygonal geometries are simple, since their rings
        /// must not self-intersect. <c>IsSimple</c>
        /// tests for this condition and reports <c>false</c> if it is not met.
        /// (This is a looser test than checking for validity).</description></item>
        /// <item><description>Linear rings have the same semantics.</description></item>
        /// <item><description>Linear geometries are simple if they do not self-intersect at points
        /// other than boundary points.</description></item>
        /// <item><description>Zero-dimensional geometries (points) are simple if they have no
        /// repeated points.</description></item>
        /// <item><description>Empty <c>Geometry</c>s are always simple.</description></item>
        /// </list>
        /// </summary>
        /// <returns><c>true</c> if this <c>Geometry</c> is simple</returns>
        /// <seealso cref="IsValid"/>
        public virtual bool IsSimple
        {
            get
            {
                var isSimpleOp = new Operation.Valid.IsSimpleOp(this);
                return isSimpleOp.IsSimple();
            }
        }

        /// <summary>
        /// Tests whether this <c>Geometry</c> is topologically
        /// valid, according to the OGC SFS specification.<para/>
        /// For validity rules see the documentation for the specific geometry subclass.
        /// </summary>
        /// <returns><c>true</c> if this <c>Geometry</c> is valid.</returns>
        public virtual bool IsValid => new IsValidOp(this).IsValid;

        /// <summary>
        /// Tests whether the set of points covered in this <c>Geometry</c> is empty.
        /// <para/>
        /// Note this test is for topological emptiness, not structural emptiness.<br/>
        /// A collection containing only empty elements is reported as empty.<br/>
        /// To check structural emptiness use <see cref="NumGeometries"/>.
        /// </summary>
        /// <returns><c>true</c> if this <c>Geometry</c> does not cover any points.</returns>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Returns the minimum distance between this <c>Geometry</c>
        /// and another <c>Geometry</c> g.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> from which to compute the distance.</param>
        /// <returns>The distance between the geometries</returns>
        /// <returns>0 if either input geometry is empty</returns>
        /// <exception cref="ArgumentException">if g is null</exception>
        public virtual double Distance(Geometry g)
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
        public virtual bool IsWithinDistance(Geometry geom, double distance)
        {
            return DistanceOp.IsWithinDistance(this, geom, distance);
        }

        /// <summary>
        /// Returns the area of this <c>Geometry</c>.
        /// Areal Geometries have a non-zero area.
        /// They override this function to compute the area.
        /// Others return 0.0
        /// </summary>
        /// <returns>The area of the Geometry.</returns>
        public virtual double Area => 0.0;

        /// <summary>
        /// Returns the length of this <c>Geometry</c>.
        /// Linear geometries return their length.
        /// Areal geometries return their perimeter.
        /// They override this function to compute the length.
        /// Others return 0.0
        /// </summary>
        /// <returns>The length of the Geometry.</returns>
        public virtual double Length => 0.0;

        /// <summary>
        /// Computes the centroid of this <c>Geometry</c>.
        /// The centroid
        /// is equal to the centroid of the set of component Geometries of highest
        /// dimension (since the lower-dimension geometries contribute zero
        /// "weight" to the centroid).
        /// <para/>
        /// The centroid of an empty geometry is <c>POINT EMPTY</c>.
        /// </summary>
        /// <returns>A Point which is the centroid of this Geometry.</returns>
        public virtual Point Centroid
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreatePoint();

                var centPt = Algorithm.Centroid.GetCentroid(this);
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
        /// <para/>
        /// The interior point of an empty geometry is <c>POINT EMPTY</c>.
        /// </remarks>
        /// <returns>A <c>Point</c> which is in the interior of this Geometry.</returns>
        public virtual Point InteriorPoint
        {
            get
            {
                if (IsEmpty)
                    return _factory.CreatePoint();

                var pt = Algorithm.InteriorPoint.GetInteriorCoord(this);
                return CreatePointFromInternalCoord(pt, this);
            }
        }

        /// <summary>
        /// <see cref="InteriorPoint" />
        /// </summary>
        public Point PointOnSurface => InteriorPoint;

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
        public abstract Dimension Dimension { get; }

        /// <summary>
        /// Returns the boundary, or an empty geometry of appropriate dimension
        /// if this <c>Geometry</c> is empty.
        /// For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        public abstract Geometry Boundary { get; }

        /// <summary>
        /// Returns the dimension of this <c>Geometry</c>s inherent boundary.
        /// </summary>
        /// <returns>
        /// The dimension of the boundary of the class implementing this
        /// interface, whether or not this object is the empty point. Returns
        /// <c>Dimension.False</c> if the boundary is the empty point.
        /// </returns>
        public abstract Dimension BoundaryDimension { get; }

        /// <summary>
        /// Gets a geometry representing the envelope (bounding box) of this <c>Geometry</c>.
        /// </summary>
        /// <remarks>If this <c>Geometry</c> is
        /// <list type="bullet">
        /// <item><description>empty, returns an empty <c>Point</c></description></item>
        /// <item><description>a point, returns a <c>Point</c></description></item>
        /// <item><description>a line parallel to an axis, a two-vertex <c>LineString</c>,</description></item>
        /// <item><description>otherwise, returns a
        /// <c>Polygon</c> whose vertices are (minx, miny), (maxx, miny),
        /// (maxx, maxy), (minx, maxy), (minx, miny).</description></item>
        /// </list>
        /// </remarks>
        /// <returns>
        /// A Geometry representing the envelope of this Geometry
        /// </returns>
        /// <seealso cref="GeometryFactory.ToGeometry(Geometries.Envelope)"/>
        public Geometry Envelope => Factory.ToGeometry(EnvelopeInternal);

        /// <summary>
        /// Gets an <see cref="Geometries.Envelope"/> containing
        /// the minimum and maximum x and y values in this <c>Geometry</c>.
        /// If the geometry is empty, an empty <c>Envelope</c>
        /// is returned.
        /// </summary>
        /// <remarks>
        /// The returned object is a copy of the one maintained internally,
        /// to avoid aliasing issues.
        /// For best performance, clients which access this
        /// envelope frequently should cache the return value.</remarks>
        /// <returns>the envelope of this <c>Geometry</c>.</returns>
        /// <returns>An empty Envelope if this Geometry is empty</returns>
        public Envelope EnvelopeInternal
        {
            get
            {
                if (_envelope == null)
                    _envelope = ComputeEnvelopeInternal();
                return new Envelope(_envelope);
            }
        }

        private class GeometryChangedFilter : IGeometryComponentFilter
        {
            public void Filter(Geometry geom)
            {
                geom.GeometryChangedAction();
            }
        };

        /// <summary>
        /// Notifies this geometry that its coordinates have been changed by an external
        /// party (for example, via a <see cref="ICoordinateFilter"/>).
        /// </summary>
        /// <remarks>
        /// When this method is called the geometry will flush
        /// and/or update any derived information it has cached (such as its <see cref="Geometries.Envelope"/> ).
        /// The operation is applied to all component Geometries.
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
        /// Tests whether this geometry is disjoint from the argument geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Disjoint</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The DE-9IM intersection matrix for the two geometries matches <c>FF*FF****</c>.</description></item>
        /// <item><description><c>!g.intersects(this) == true</c><br/>(<c>Disjoint</c> is the inverse of <c>Intersects</c>)</description></item>
        /// </list>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are disjoint.</returns>
        /// <see cref="Intersects"/>
        public bool Disjoint(Geometry g)
        {
            return !Intersects(g);
        }

        /// <summary>
        /// Tests whether this geometry touches the argument geometry
        /// </summary>
        /// <remarks>
        /// The <c>Touches</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The geometries have at least one point in common,
        /// but their interiors do not intersect</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the following patterns
        /// <list type="bullet">
        /// <item><description><c>FT*******</c>,</description></item>
        /// <item><description><c>F**T*****</c> or</description></item>
        /// <item><description><c>F***T****</c>.</description></item>
        /// </list></description></item>
        /// </list>
        /// If both geometries have dimension 0, the predicate returns <c>false</c>,
        /// since points have only interiors.
        /// This predicate is symmetric.
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if the two <c>Geometry</c>s touch;
        /// Returns false if both <c>Geometry</c>s are points.
        /// </returns>
        public virtual bool Touches(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsTouches(Dimension, g.Dimension);
        }

        /// <summary>
        /// Tests whether this geometry intersects the argument geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Intersects</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The two geometries have at least one point in common</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches<br/>
        /// <c>[T********]</c> or<br/>
        /// <c>[*T*******]</c> or<br/>
        /// <c>[***T*****]</c> or<br/>
        /// <c>[****T****]</c></description></item>
        /// <item><description><c>!g.disjoint(this)</c><br/>
        /// (<c>Intersects</c> is the inverse of <c>Disjoint</c>)</description></item>
        /// </list></remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s intersect.</returns>
        /// <see cref="Disjoint"/>
        public virtual bool Intersects(Geometry g)
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
                return RectangleIntersects.Intersects((Polygon)this, g);
            if (g.IsRectangle)
                return RectangleIntersects.Intersects((Polygon)g, this);

            if (IsGeometryCollection || g.IsGeometryCollection)
            {
                for (int i = 0; i < NumGeometries; i++)
                {
                    for (int j = 0; j < g.NumGeometries; j++)
                    {
                        if (GetGeometryN(i).Intersects(g.GetGeometryN(j)))
                            return true;
                    }
                }
                return false;
            }

            return Relate(g).IsIntersects();
        }

        /// <summary>
        /// Tests whether this geometry crosses the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Crosses</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item>The geometries have some but not all interior points in common.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches
        /// one of the following patterns:
        /// <list type="table">
        /// <listheader><term>Code</term><description>Description</description></listheader>
        /// <item><term><c>[T*T******]</c></term><description>for P/L, P/A, and L/A situations</description></item>
        /// <item><term><c>[T*****T**]</c></term><description>for L/P, A/P, and A/L situations)</description></item>
        /// <item><term><c>[0********]</c></term><description>for L/L situations</description></item>
        /// </list>
        /// </item>
        /// </list>
        /// For the A/A and P/P situations this predicate returns <c>false</c>.
        /// <para>
        /// The SFS defined this predicate only for P/L, P/A, L/L, and L/A situations.
        /// To make the relation symmetric,
        /// NTS extends the definition to apply to L/P, A/P and A/L situations as well.
        /// </para>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s cross.</returns>
        public virtual bool Crosses(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsCrosses(Dimension, g.Dimension);
        }

        /// <summary>
        /// Tests whether this geometry is within the specified geometry.
        /// </summary>
        /// <remarks>
        /// The <c>within</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>
        /// Every point of this geometry is a point of the other geometry,
        /// and the interiors of the two geometries have at least one point in common.
        /// </description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches <c>[T*F**F***]</c></description></item>
        /// <item><description><c>g.contains(this) == true</c><br/>(<c>Within</c> is the converse of <see cref="Contains"/>)</description></item>
        /// </list>
        /// <para>
        /// An implication of the definition is that "The boundary of a geometry is not within the Polygon".
        /// In other words, if a geometry A is a subset of the points in the boundary of a geometry B, <c>A.within(B) == false</c>
        /// (As a concrete example, take A to be a LineString which lies in the boundary of a Polygon B.)
        /// For a predicate with similar behaviour but avoiding
        /// this subtle limitation, see <see cref="CoveredBy"/>.
        /// </para>
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns><c>true</c> if this <c>Geometry</c> is within <c>other</c>.</returns>
        /// <see cref="Contains"/>
        /// <see cref="CoveredBy"/>
        public bool Within(Geometry g)
        {
            return g.Contains(this);
        }

        /// <summary>
        /// Tests whether this geometry contains the argument geometry.
        /// </summary>
        /// <remarks>
        /// The <c>Contains</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of the other geometry is a point of this geometry,
        /// and the interiors of the two geometries have at least one point in common.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches the pattern
        /// <c>[T*****FF*]</c></description></item>
        /// <item><description><c>g.within(this)</c><br/>
        /// (<c>Contains</c> is the converse of <see cref="Within"/>)</description></item>
        /// </list>
        /// <para>
        /// An implication of the definition is that "Geometries do not
        /// contain their boundary".  In other words, if a geometry A is a subset of
        /// the points in the boundary of a geometry B, <c>B.Contains(A) == false</c>.
        /// (As a concrete example, take A to be a LineString which lies in the boundary of a Polygon B.)
        /// For a predicate with similar behaviour but avoiding
        /// this subtle limitation, see <see cref="Covers"/>.
        /// </para>
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if this <c>Geometry</c> contains <c>g</c></returns>
        /// <see cref="Within"/>
        /// <see cref="Covers"/>
        public virtual bool Contains(Geometry g)
        {
            // optimization - lower dimension cannot contain areas
            if (g.Dimension == Dimension.Surface && Dimension < Dimension.Surface)
                return false;

            // optimization - P cannot contain a non-zero-length L
            // Note that a point can contain a zero-length lineal geometry,
            // since the line has no boundary due to Mod-2 Boundary Rule
            if (g.Dimension == Dimension.Curve && Dimension < Dimension.Curve && g.Length > 0.0)
                return false;

            // optimization - envelope test
            if (!EnvelopeInternal.Contains(g.EnvelopeInternal))
                return false;

            // optimizations for rectangle arguments
            if (IsRectangle)
                return RectangleContains.Contains((Polygon)this, g);

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
        /// <list type="bullet">
        /// <item><description>The geometries have at least one point each not shared by the other (or equivalently neither covers the other),
        /// they have the same dimension,
        /// and the intersection of the interiors of the two geometries has
        /// the same dimension as the geometries themselves.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        ///  <c>[T*T***T**]</c> (for two points or two surfaces)
        ///  or <c>[1*T***T**]</c> (for two curves)</description></item>
        /// </list>
        /// If the geometries are of different dimension this predicate returns <c>false</c>.
        /// </remarks>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if the two <c>Geometry</c>s overlap.
        /// For this function to return <c>true</c>, the <c>Geometry</c>
        /// s must be two points, two curves or two surfaces.
        /// </returns>
        public virtual bool Overlaps(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return Relate(g).IsOverlaps(Dimension, g.Dimension);
        }

        /// <summary>
        /// Tests whether this geometry covers the argument geometry
        /// </summary>
        /// <remarks>
        /// The <c>covers</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of the other geometry is a point of this geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches at least
        /// one of the following patterns:
        /// <list type="bullet">
        /// <item><description><c>[T*****FF*]</c> or<br/></description></item>
        /// <item><description><c>[*T****FF*]</c> or<br/></description></item>
        /// <item><description><c>[***T**FF*]</c> or<br/></description></item>
        /// <item><description><c>[****T*FF*]</c></description></item>
        /// </list>
        /// </description></item>
        /// <item><c>g.CoveredBy(this) == true</c><br/>
        /// (<c>covers</c> is the converse of <see cref="CoveredBy"/>)</item>
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
        public virtual bool Covers(Geometry g)
        {
            // optimization - lower dimension cannot cover areas
            if (g.Dimension == Dimension.Surface && Dimension < Dimension.Surface)
                return false;

            // optimization - P cannot cover a non-zero-length L
            // Note that a point can cover a zero-length lineal geometry
            if (g.Dimension == Dimension.Curve && Dimension < Dimension.Curve && g.Length > 0.0)
                return false;

            // optimization - envelope test
            if (!EnvelopeInternal.Covers(g.EnvelopeInternal))
                return false;

            // optimization for rectangle arguments
            if (IsRectangle)
                // since we have already tested that the test envelope is covered
                return true;

            return Relate(g).IsCovers();
        }

        /// <summary>Tests whether this geometry is covered by the specified geometry.</summary>
        /// <remarks>
        /// The <c>CoveredBy</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of this geometry is a point of the other geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// at least one of the following patterns:
        /// <list type="bullet">
        /// <item><description><c>[T*F**F***]</c></description></item>
        /// <item><description><c>[*TF**F***]</c></description></item>
        /// <item><description><c>[**FT*F***]</c></description></item>
        /// <item><description><c>[**F*TF***]</c></description></item></list></description></item>
        /// <item><description><c>g.Covers(this) == true</c><br/>
        /// (<c>CoveredBy</c> is the converse of <see cref="Covers"/>)
        /// </description></item>
        /// </list>
        /// If either geometry is empty, the value of this predicate is <c>false</c>.
        /// <para>
        /// This predicate is similar to <see cref="Within"/>,
        /// but is more inclusive (i.e. returns <c>true</c> for more cases).
        /// </para>
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if this <c>Geometry</c> is covered by <c>g</c></returns>
        /// <seealso cref="Within"/>
        /// <seealso cref="Covers"/>
        public bool CoveredBy(Geometry g)
        {
            return g.Covers(this);
        }

        /// <summary>
        /// Tests whether the elements in the DE-9IM
        /// <see cref="IntersectionMatrix"/> for the two <c>Geometry</c>s match the elements in <c>intersectionPattern</c>.
        /// </summary>
        /// <remarks>
        /// The pattern is a 9-character string, with symbols drawn from the following set:
        /// <list type="table">
        /// <item><term>0</term><description>(dimension 0)</description></item>
        /// <item><term>1</term><description>(dimension 1)</description></item>
        /// <item><term>2</term><description>(dimension 2)</description></item>
        /// <item><term>T</term><description>( matches 0, 1 or 2)</description></item>
        /// <item><term>F</term><description>( matches FALSE)</description></item>
        /// <item><term>*</term><description>( matches any value)</description></item>
        /// </list>
        /// For more information on the DE-9IM, see the <i>OpenGIS Simple Features
        /// Specification</i>.
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <param name="intersectionPattern">the pattern against which to check the
        /// intersection matrix for the two <c>Geometry</c>s</param>
        /// <returns><c>true</c> if the DE-9IM intersection
        /// matrix for the two <c>Geometry</c>s match <c>intersectionPattern</c></returns>
        /// <seealso cref="IntersectionMatrix"/>
        public virtual bool Relate(Geometry g, string intersectionPattern)
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
        public virtual IntersectionMatrix Relate(Geometry g)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(g);

            return RelateOp.Relate(this, g);
        }

        /// <summary>
        /// Tests whether this geometry is
        /// topologically equal to the argument geometry.
        /// <para/>
        /// This method is included for backward compatibility reasons.
        /// It has been superseded by the <seealso cref="EqualsTopologically"/> method,
        /// which has been named to clearly denote its functionality.
        /// <para/>
        /// This method should <b>NOT</b> be confused with the method
        /// <seealso cref="Equals(object)"/>, which implements
        /// an exact equality comparison.
        /// </summary>
        /// <param name="g">The <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are topologically equal.</returns>
        /// <seealso cref="EqualsTopologically"/>
        public bool Equals(Geometry g)
        {
            if (g == null)
                return false;
            return EqualsTopologically(g);
        }

        /// <summary>
        /// Tests whether this geometry is topologically equal to the argument geometry
        /// as defined by the SFS <c>Equals</c> predicate.
        /// </summary>
        /// <remarks>
        /// The SFS <c>equals</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>The two geometries have at least one point in common,
        /// and no point of either geometry lies in the exterior of the other geometry.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches
        /// the pattern <tt>T*F**FFF*</tt>
        /// <pre>
        /// T*F
        /// **F
        /// FF*
        /// </pre></description></item>
        /// </list>
        /// <b>Note</b> that this method computes <b>topologically equality</b>.
        /// For structural equality, see {@link #equalsExact(Geometry)}.
        /// </remarks>
        /// <param name="g">the <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <returns><c>true</c> if the two <c>Geometry</c>s are topologically equal</returns>
        public virtual bool EqualsTopologically(Geometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Equals(g.EnvelopeInternal))
                return false;

            return Relate(g).IsEquals(Dimension, g.Dimension);
        }

        /// <summary>
        /// Tests whether this geometry is structurally and numerically equal
        /// to a given <tt>Object</tt>.
        /// </summary>
        /// <remarks>
        /// If the argument <tt>Object</tt> is not a <c>Geometry</c>,
        /// the result is <c>false</c>.
        /// Otherwise, the result is computed using
        /// <seealso cref="EqualsExact(Geometry)"/>.
        /// <para/>
        /// This method is provided to fulfill the Java contract
        /// for value-based object equality.
        /// In conjunction with <seealso cref="GetHashCode"/>
        /// it provides semantics which are most useful
        /// for using
        /// <c>Geometry</c>s as keys and values in Java collections.
        /// <para/>
        /// Note that to produce the expected result the input geometries
        /// should be in normal form.  It is the caller's
        /// responsibility to perform this where required
        /// (using <seealso cref="Normalized"/>
        /// or <seealso cref="Normalize"/> as appropriate).
        /// </remarks>
        /// <param name="o">The object to compare</param>
        /// <returns><c>true</c> if this geometry is exactly equal to the argument</returns>
        /// <seealso cref="EqualsExact(Geometry)"/>
        /// <seealso cref="GetHashCode"/>
        /// <seealso cref="Normalized"/>
        /// <seealso cref="Normalize"/>
        public override bool Equals(object o)
        {
            var g = o as Geometry;
            return g != null && EqualsExact(g);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        private static bool CompareGeometryCollections(Geometry obj1, Geometry obj2)
        {
            var coll1 = obj1 as GeometryCollection;
            var coll2 = obj2 as GeometryCollection;
            if (coll1 == null || coll2 == null)
                return false;

            // Short-circuit test
            if (coll1.NumGeometries != coll2.NumGeometries)
                return false;

            // Deep test
            for (int i = 0; i < coll1.NumGeometries; i++)
            {
                var geom1 = coll1[i];
                var geom2 = coll2[i];
                if (!geom1.EqualsExact(geom2))
                    return false;
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator ==(Geometry obj1, Geometry obj2)
        {
            return Equals(obj1, obj2);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool operator !=(Geometry obj1, Geometry obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// Gets a hash code for the Geometry.
        /// </summary>
        /// <returns>
        /// An integer value suitable for use as a hashcode
        /// </returns>
        public override int GetHashCode()
        {
            return EnvelopeInternal.GetHashCode();
            //int result = 17;
            //return GetHashCodeInternal(result, x => 37 * x );
            ////
            ////foreach (var coord in Coordinates)
            ////    result = 37 * result + coord.X.GetHashCode();
            ////
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
            var writer = new WKTWriter(3);
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
            var writer = new WKBWriter();
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
            var writer = new GMLWriter();
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
        /// <para>The end cap style is <c>EndCapStyle.Round</c>.</para>
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
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, BufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public Geometry Buffer(double distance)
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
        /// <seealso cref="Buffer(double, BufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public Geometry Buffer(double distance, EndCapStyle endCapStyle)
        {
            var para = new BufferParameters { EndCapStyle = endCapStyle };
            return BufferOp.Buffer(this, distance, para);
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
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, BufferParameters)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public Geometry Buffer(double distance, int quadrantSegments)
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
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, BufferParameters)"/>
        /// <seealso cref="Buffer(double, int)"/>
        public Geometry Buffer(double distance, int quadrantSegments, EndCapStyle endCapStyle)
        {
            var para = new BufferParameters {EndCapStyle = endCapStyle, QuadrantSegments = quadrantSegments };
            return BufferOp.Buffer(this, distance, para);
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
        /// This is also the result for the buffers of degenerate (zero-area) polygons.
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
        /// <seealso cref="Buffer(double, EndCapStyle)"/>
        /// <seealso cref="Buffer(double, int)"/>
        /// <seealso cref="Buffer(double, int, EndCapStyle)"/>
        public Geometry Buffer(double distance, BufferParameters bufferParameters)
        {
            return BufferOp.Buffer(this, distance, bufferParameters);
        }

        /// <summary>
        /// Returns the smallest convex <c>Polygon</c> that contains all the
        /// points in the <c>Geometry</c>. This obviously applies only to <c>Geometry</c>
        /// s which contain 3 or more points.
        /// </summary>
        /// <returns>the minimum-area convex polygon containing this <c>Geometry</c>'s points.</returns>
        public virtual Geometry ConvexHull()
        {
            return (new ConvexHull(this)).GetConvexHull();
        }

        /// <summary>
        /// Computes a new geometry which has all component coordinate sequences
        /// in reverse order (opposite orientation) to this one.
        /// </summary>
        /// <returns>A reversed geometry</returns>
        /// <remarks>Don't override this function, implement <see cref="ReverseInternal"/>.</remarks>
        public virtual Geometry Reverse()
        {
            var res = ReverseInternal();
            res._envelope = _envelope?.Copy();
            //res.SRID = SRID;

            return res;
        }

        /// <summary>
        /// The actual implementation of the <see cref="Reverse"/> function
        /// </summary>
        /// <returns>A reversed geometry</returns>
        /// <remarks>In JTS this function is abstract, but that would break binary compatibility of current version.</remarks>
        protected virtual Geometry ReverseInternal() { throw new NotSupportedException("Calling Geometry.ReverseInternal directly is not supported"); }

        /// <summary>
        /// Computes a <c>Geometry</c> representing the point-set which is
        /// common to both this <c>Geometry</c> and the <c>other</c> Geometry.
        /// <para/>
        /// The intersection of two geometries of different dimension produces a result
        /// geometry of dimension less than or equal to the minimum dimension of the input
        /// geometries.
        /// The result geometry may be a heterogeneous <see cref="GeometryCollection"/>.
        /// If the result is empty, it is an atomic geometry
        /// with the dimension of the lowest input dimension.
        /// <para/>
        /// Intersection of <see cref="GeometryCollection"/>s is supported
        /// only for homogeneous collection types.
        /// <para/>
        /// Non-empty heterogeneous <see cref="GeometryCollection"/> arguments are not supported.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the intersection.</param>
        /// <returns>A geometry representing the point-set common to the two <c>Geometry</c>s.</returns>
        /// <exception cref="TopologyException">if a robustness error occurs.</exception>
        /// <exception cref="ArgumentException">if the argument is a non-empty heterogeneous <c>GeometryCollection</c></exception>
        /// <exception cref="ArgumentException">if the argument has a factory with a different <c>GeometryOverlay</c> object assigned</exception>
        public Geometry Intersection(Geometry other)
        {
            if (other != null && !Equals(other.Factory.GeometryOverlay, Factory.GeometryOverlay))
                throw new ArgumentException("Geometry has factory with different GeometryOverlay operation assigned", nameof(other));
            return Factory.GeometryOverlay.Intersection(this, other);
        }

        /// <summary>
        /// Computes a <c>Geometry</c> representing  the point-set
        /// which is contained in both this
        /// <c>Geometry</c> and the <c>other</c> Geometry.
        /// </summary>
        /// <remarks>
        /// The method may be used on arguments of different dimension, but it does not
        /// support <see cref="GeometryCollection"/> arguments.
        /// <para/>
        /// The union of two geometries of different dimension produces a result
        /// geometry of dimension equal to the maximum dimension of the input
        /// geometries.
        /// The result geometry may be a heterogeneous
        /// <see cref="GeometryCollection"/>.
        /// If the result is empty, it is an atomic geometry
        /// with the dimension of the highest input dimension.
        /// <para/>
        /// Unioning <see cref="LineString"/>s has the effect of
        /// <b>noding</b> and <b>dissolving</b> the input linework. In this context
        /// "noding" means that there will be a node or endpoint in the result for
        /// every endpoint or line segment crossing in the input. "Dissolving" means
        /// that any duplicate (i.e. coincident) line segments or portions of line
        /// segments will be reduced to a single line segment in the result.
        /// If <b>merged</b> linework is required, the <see cref="LineMerger"/>
        /// class can be used.
        /// <para/>
        /// Non-empty <see cref="GeometryCollection"/> arguments are not supported.</remarks>
        /// <param name="other">the <c>Geometry</c> with which to compute the union</param>
        /// <returns>A point-set combining the points of this <c>Geometry</c> and the
        /// points of <c>other</c></returns>
        /// <exception cref="TopologyException">Thrown if a robustness error occurs</exception>
        /// <exception cref="ArgumentException">Thrown if either input is a non-empty GeometryCollection</exception>
        /// <exception cref="ArgumentException">if the argument has a factory with a different <c>GeometryOverlay</c> object assigned</exception>
        /// <seealso cref="LineMerger"/>
        public Geometry Union(Geometry other)
        {
            if (other != null && !Equals(other.Factory.GeometryOverlay, Factory.GeometryOverlay))
                throw new ArgumentException("Geometry has factory with different GeometryOverlay operation assigned", nameof(other));
            return Factory.GeometryOverlay.Union(this, other);
        }

        /// <summary>
        /// Computes a <c>Geometry</c> representing the closure of the point-set
        /// of the points contained in this <c>Geometry</c> that are not contained in
        /// the <c>other</c> Geometry.
        /// <para/>
        /// If the result is empty, it is an atomic geometry
        /// with the dimension of the left-hand input.
        /// <para/>
        /// Non-empty <see cref="GeometryCollection"/> arguments are not supported.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the difference.</param>
        /// <returns>A Geometry representing the point-set difference of this <c>Geometry</c> with <c>other</c>.</returns>
        /// <exception cref="ArgumentException">if the argument has a factory with a different <c>GeometryOverlay</c> object assigned</exception>
        public Geometry Difference(Geometry other)
        {
            if (other != null && !Equals(other.Factory.GeometryOverlay, Factory.GeometryOverlay))
                throw new ArgumentException("Geometry has factory with different GeometryOverlay operation assigned", nameof(other));
            return Factory.GeometryOverlay.Difference(this, other);
        }

        /// <summary>
        /// Computes a <c>Geometry </c> representing the closure of the point-set
        /// which is the union of the points in this <c>Geometry</c> which are not
        /// contained in the <c>other</c> Geometry,
        /// with the points in the <c>other</c> Geometry not contained in this
        /// <c>Geometry</c>.
        /// If the result is empty, it is an atomic geometry
        /// with the dimension of the highest input dimension.
        /// <para/>
        /// Non-empty <see cref="GeometryCollection"/> arguments are not supported.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compute the symmetric difference.</param>
        /// <returns>a Geometry representing the point-set symmetric difference of this <c>Geometry</c> with <c>other</c>.</returns>
        /// <exception cref="ArgumentException">if the argument has a factory with a different <c>GeometryOverlay</c> object assigned</exception>
        public Geometry SymmetricDifference(Geometry other)
        {
            if (other != null && !Equals(other.Factory.GeometryOverlay, Factory.GeometryOverlay))
                throw new ArgumentException("Geometry has factory with different GeometryOverlay operation assigned", nameof(other));
            return Factory.GeometryOverlay.SymmetricDifference(this, other);
        }

        /// <summary>
        /// Computes the union of all the elements of this geometry.
        /// </summary>
        /// <remarks>
        /// This method supports <see cref="GeometryCollection"/>s (which the other overlay operations currently do not).</remarks>
        /// <remarks>
        /// The result obeys the following contract:
        /// <list type="bullet">
        /// <item><description>Unioning a set of <see cref="LineString"/>s has the effect of fully noding and dissolving the linework.</description></item>
        /// <item><description>Unioning a set of <see cref="Polygon"/>s always returns a <see cref="IPolygonal"/> geometry
        /// (unlike <see cref="Union(Geometry)"/>), which may return geometries of lower dimension if a topology
        /// collapse occurred).</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="TopologyException">Thrown if a robustness error occurs</exception>
        public Geometry Union()
        {
            return Factory.GeometryOverlay.Union(this);
        }

        /// <summary>
        /// Returns true if the two <c>Geometry</c>s are exactly equal,
        /// up to a specified tolerance.
        /// Two Geometries are exactly within a tolerance equal if:
        /// <list type="bullet">
        /// <item><description>they have the same class,</description></item>
        /// <item><description>they have the same values of Coordinates,
        /// within the given tolerance distance, in their internal
        /// Coordinate lists, in exactly the same order.</description></item>
        /// </list>
        /// This method does <i>not</i>
        /// test the values of the <c>GeometryFactory</c>, the <c>SRID</c>,
        /// or the <c>UserData</c> fields.
        /// <para/>
        /// To properly test equality between different geometries,
        /// it is usually necessary to <see cref="Normalize"/> them first.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compare this <c>Geometry</c>
        /// have identical structure and point values, up to the distance tolerance.</param>
        /// <param name="tolerance">Distance at or below which two Coordinates will be considered equal.</param>
        /// <returns>
        /// <c>true</c> if this and the other <c>Geometry</c>
        /// are of the same class and have equal internal data.
        /// </returns>
        /// <seealso cref="EqualsExact(Geometry)"/>
        /// <seealso cref="Normalize"/>
        /// <seealso cref="Normalized"/>
        public abstract bool EqualsExact(Geometry other, double tolerance);

        /// <summary>
        /// Returns true if the two <c>Geometry</c>s are exactly equal.
        /// Two Geometries are exactly equal if:
        /// <list type="bullet">
        /// <item><description>they have the same class,</description></item>
        /// <item><description>they have the same values of Coordinates in their internal
        /// Coordinate lists, in exactly the same order.</description></item>
        /// </list>
        /// This provides a stricter test of equality than
        /// <see cref="EqualsTopologically"/>, which is more useful
        /// in certain situations
        /// (such as using geometries as keys in collections).
        /// <para/>
        /// This method does <i>not</i>
        /// test the values of the <c>GeometryFactory</c>, the <c>SRID</c>,
        /// or the <c>UserData</c> fields.
        /// <para/>
        /// To properly test equality between different geometries,
        /// it is usually necessary to <see cref="Normalize"/> them first.
        /// </summary>
        /// <param name="other">The <c>Geometry</c> with which to compare this <c>Geometry</c>.</param>
        /// <returns>
        /// <c>true</c> if this and the other <c>Geometry</c> have identical structure and point values.
        /// </returns>
        public bool EqualsExact(Geometry other)
        {
            // this is the exact meaning of jts r929: http://sourceforge.net/p/jts-topo-suite/code/929
            // be aware of == operator overload!
            bool sameref = ReferenceEquals(this, other);
            return sameref || EqualsExact(other, 0);
        }

        /// <summary>
        /// Tests whether two geometries are exactly equal
        /// in their normalized forms.
        /// <remarks>
        /// This is a convenience method which creates normalized
        /// versions of both geometries before computing
        /// <seealso cref="EqualsExact(Geometry)"/>.<para/>
        /// This method is relatively expensive to compute.
        /// For maximum performance, the client
        /// should instead perform normalization  on the individual geometries
        /// at an appropriate point during processing.
        /// </remarks>
        /// </summary>
        /// <param name="g">A geometry</param>
        /// <returns>true if the input geometries are exactly equal in their normalized form</returns>
        /// <seealso cref="EqualsExact(Geometry)"/>
        public bool EqualsNormalized(Geometry g)
        {
            if (g == null) return false;
            return Normalized().EqualsExact(g.Normalized());
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

        /// <summary>
        /// Performs an operation on the coordinates in this <c>Geometry</c>'s <see cref="CoordinateSequence"/>s.
        /// </summary>
        /// <remarks>
        /// If the filter reports that a coordinate value has been changed,
        /// <see cref="GeometryChanged"/> will be called automatically.
        /// </remarks>
        /// <param name="filter">The filter to apply</param>
        public abstract void Apply(ICoordinateSequenceFilter filter);

        /// <summary>
        /// Performs an operation on this <c>Geometry</c>'s <see cref="CoordinateSequence"/>s.
        /// </summary>
        /// <remarks>
        /// If the filter reports that a coordinate value has been changed,
        /// <see cref="GeometryChanged"/> will be called automatically.
        /// </remarks>
        /// <param name="filter">The filter to apply</param>
        public virtual void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            Apply(new EntireCoordinateSequenceFilterAdapter(filter));
        }

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
        /// Creates a deep copy of this <see cref="Geometry"/> object.
        /// Coordinate sequences contained in it are copied.
        /// All instance fields are copied
        /// (i.e. the <c>SRID</c>, <c>EnvelopeInternal</c> and <c>UserData</c>).
        /// </summary>
        /// <remarks>
        /// <b>NOTE:</b> The UserData object reference (if present) is copied,
        /// but the value itself is not copied.
        /// If a deep copy is required this must be performed by the caller. 
        /// </remarks>
        /// <returns>A deep copy of this geometry</returns>
        public Geometry Copy()
        {
            var copy = CopyInternal();

            //Not necessary as SRID is Factory.SRID
            //copy.SRID = SRID;
            copy._envelope = _envelope?.Copy();
            copy.UserData = UserData;
            return copy;
        }

        /// <summary>
        /// An internal method to copy subclass-specific geometry data.
        /// </summary>
        /// <returns>A copy of the target geometry object.</returns>
        protected abstract Geometry CopyInternal();

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
        /// <para>
        /// The definitions for normal
        /// form use the standard lexicographical ordering for coordinates. "Sorted in
        /// order of coordinates" means the obvious extension of this ordering to
        /// sequences of coordinates.
        /// </para>
        /// <para>
        /// <b>NOTE</b> that this method mutates the value of this geometry in-place.
        /// If this is not safe and/or wanted, the geometry should be
        /// cloned prior to normalization.
        /// </para>
        /// </remarks>
        public abstract void Normalize();

        /// <summary>
        /// Creates a new Geometry which is a normalized copy of this Geometry.
        /// </summary>
        /// <returns>A normalized copy of this geometry.</returns>
        /// <seealso cref="Normalize"/>
        public Geometry Normalized()
        {
            var copy = Copy();
            copy.Normalize();
            return copy;
        }

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list type="bullet">
        /// <item><description>Point (lowest),</description></item>
        /// <item><description>MultiPoint,</description></item>
        /// <item><description>LineString,</description></item>
        /// <item><description>LinearRing,</description></item>
        /// <item><description>MultiLineString,</description></item>
        /// <item><description>Polygon,</description></item>
        /// <item><description>MultiPolygon,</description></item>
        /// <item><description>GeometryCollection (highest).</description></item>
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
            return CompareTo(o as Geometry);
        }

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list type="bullet">
        /// <item><description>Point (lowest),</description></item>
        /// <item><description>MultiPoint,</description></item>
        /// <item><description>LineString,</description></item>
        /// <item><description>LinearRing,</description></item>
        /// <item><description>MultiLineString,</description></item>
        /// <item><description>Polygon,</description></item>
        /// <item><description>MultiPolygon,</description></item>
        /// <item><description>GeometryCollection (highest).</description></item>
        /// /// </list>
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
        public int CompareTo(Geometry geom)
        {
            var other = geom as Geometry;
            if (other == null)
                return -1;

            // Since JTS v.1.17 getTypeCode instead of getSortIndex
            if (SortIndex != other.SortIndex)
                return (int)SortIndex - (int)other.SortIndex;
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
        /// or less than another <c>Geometry</c>, using the given <see paramref="IComparer{CoordinateSequence}"/>.</summary>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        /// <list type="bullet">
        /// <item><description>Point (lowest),</description></item>
        /// <item><description>MultiPoint,</description></item>
        /// <item><description>LineString,</description></item>
        /// <item><description>LinearRing,</description></item>
        /// <item><description>MultiLineString,</description></item>
        /// <item><description>Polygon,</description></item>
        /// <item><description>MultiPolygon,</description></item>
        /// <item><description>GeometryCollection (highest).</description></item>
        /// </list>
        /// If the two <c>Geometry</c>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        /// <param name="o">A <c>Geometry</c> with which to compare this <c>Geometry</c></param>
        /// <param name="comp">A <c>IComparer&lt;CoordinateSequence&gt;</c></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public int CompareTo(object o, IComparer<CoordinateSequence> comp)
        {
            var other = o as Geometry;
            if (other == null)
                return -1;

            // Since JTS v.1.17 getTypeCode instead of getSortIndex
            if (SortIndex != other.SortIndex)
                return (int)SortIndex - (int)other.SortIndex;
            if (IsEmpty && other.IsEmpty)
                return 0;
            if (IsEmpty)
                return -1;
            if (other.IsEmpty)
                return 1;

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
        protected virtual bool IsEquivalentClass(Geometry other)
        {
            return GetType().FullName == other.GetType().FullName;
        }

        /// <summary>
        /// Throws an exception if <c>g</c>'s type is a <c>GeometryCollection</c>.
        /// (Its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> to check.</param>
        /// <exception cref="ArgumentException">
        /// if <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>
        protected static void CheckNotGeometryCollection(Geometry g)
        {
            if (g.IsGeometryCollection)
                throw new ArgumentException("Operation does not support GeometryCollection arguments");
        }

        /*
        /// <summary>
        /// Returns <c>true</c> if <c>g</c>'s class is <c>GeometryCollection</c>.
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <c>Geometry</c> to check.</param>
        /// <exception cref="ArgumentException">
        /// If <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>
        private static bool IsNonHomogenousGeometryCollection(Geometry g)
        {
            return
                g is GeometryCollection &&
                g.GeometryType == "GeometryCollection";  //g.GetType().Name == "GeometryCollection" && g.GetType().Namespace == GetType().Namespace;
        }
         */

        /// <summary>
        /// Tests whether this is an instance of a general {@link GeometryCollection},
        /// rather than a homogeneous subclass.
        /// </summary>
        /// <returns><c>true</c> if this is a heterogeneous GeometryCollection</returns>
        protected bool IsGeometryCollection =>
            // Since JTS v.1.17 getTypeCode is tested
            OgcGeometryType == OgcGeometryType.GeometryCollection;

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

        /// <summary>
        /// Returns whether this <c>Geometry</c> is greater than, equal to,
        /// or less than another <c>Geometry</c> of the same class.
        /// using the given <see cref="IComparer{CoordinateSequence}"/>.
        /// </summary>
        /// <param name="o">A <c>Geometry</c> having the same class as this <c>Geometry</c></param>
        /// <param name="comp">The comparer</param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the JTS Technical
        /// Specifications
        /// </returns>
        protected internal abstract int CompareToSameClass(object o, IComparer<CoordinateSequence> comp);

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
        protected static int Compare(List<Geometry> a, List<Geometry> b)
        {
            var i = a.GetEnumerator();
            var j = b.GetEnumerator();

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

        [Obsolete("Will be removed in a future version")]
        protected static bool Equal(Coordinate a, Coordinate b, double tolerance)
        {
            if (tolerance == 0)
                return a.Equals(b);

            return a.Distance(b) <= tolerance;
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected abstract SortIndexValue SortIndex { get; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="exemplar"></param>
        /// <returns></returns>
        private static Point CreatePointFromInternalCoord(Coordinate coord, Geometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        /// <summary>
        /// Tests whether this is a rectangular <see cref="Polygon"/>.
        /// </summary>
        /// <returns><c>true</c> if the geometry is a rectangle.</returns>
        /// <remarks>Polygon overrides to check for actual rectangle.</remarks>
        public virtual bool IsRectangle => false;

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// A predefined <see cref="GeometryFactory" /> with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        /// <seealso cref="GeometryFactory.Default" />
        /// <seealso cref="GeometryFactory.Fixed"/>
        public static readonly GeometryFactory DefaultFactory = GeometryFactory.Default;

        /* END ADDED BY MPAUL42: monoGIS team */

        protected static double[] CreateArray(int size, double value)
        {
            double[] result = new double[size];
            for (int i = 0; i < size; i++)
                result[i] = value;
            return result;
        }

        protected static double[] CreateArray(CoordinateSequence sequence, Ordinate ordinate)
        {
            double[] result = new double[sequence.Count];
            if (sequence.TryGetOrdinateIndex(ordinate, out int ordinateIndex))
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = sequence.GetOrdinate(i, ordinateIndex);
            }
            else
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = Coordinate.NullOrdinate;
            }

            return result;
        }

        private sealed class EntireCoordinateSequenceFilterAdapter : ICoordinateSequenceFilter
        {
            private readonly IEntireCoordinateSequenceFilter _inner;

            public EntireCoordinateSequenceFilterAdapter(IEntireCoordinateSequenceFilter inner)
            {
                _inner = inner;
            }

            public bool Done => _inner.Done;

            public bool GeometryChanged => _inner.GeometryChanged;

            public void Filter(CoordinateSequence seq, int i)
            {
                if (i == 0)
                {
                    _inner.Filter(seq);
                }
            }

        }
    }
}
