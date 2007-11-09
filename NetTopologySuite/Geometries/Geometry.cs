using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Predicate;
using GisSharpBlog.NetTopologySuite.Operation.Relate;
using GisSharpBlog.NetTopologySuite.Operation.Valid;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <see cref="Geometry{TCoordinate}"/>.
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
    /// sometimes many ways of representing a point set as a <see cref="Geometry{TCoordinate}"/>.
    /// The SFS does not specify an unambiguous representation of a given point set
    /// returned from a spatial analysis method. One goal of NTS is to make this
    /// specification precise and unambiguous. NTS will use a canonical form for
    /// <see cref="Geometry{TCoordinate}"/>s returned from spatial analysis methods. The canonical
    /// form is a <see cref="Geometry{TCoordinate}"/> which is simple and noded:
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
    /// contain constructed points which are not present in the input <see cref="Geometry{TCoordinate}"/>s. 
    /// These new points arise from intersections between line segments in the
    /// edges of the input <see cref="Geometry{TCoordinate}"/>s. In the general case it is not
    /// possible to represent constructed points exactly. This is due to the fact
    /// that the coordinates of an intersection point may contain twice as many bits
    /// of precision as the coordinates of the input line segments. In order to
    /// represent these constructed points explicitly, NTS must truncate them to fit
    /// the <see cref="PrecisionModel{TCoordinate}"/>. 
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
    /// <see cref="object.Equals(object)" /> and <see cref="object.GetHashCode" /> 
    /// are not overridden, so that when two topologically equal Geometries are added 
    /// to Collections and Dictionaries, they remain distinct. 
    /// This behavior is desired in many cases.
    /// </remarks>
    [Serializable]
    public abstract class Geometry<TCoordinate> : IGeometry<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                             IComputable<TCoordinate>, IConvertible
    {
        //private static readonly Type[] SortedClasses = new Type[]
        //    {
        //        typeof (Point),
        //        typeof (MultiPoint),
        //        typeof (LineString),
        //        typeof (LinearRing),
        //        typeof (MultiLineString),
        //        typeof (Polygon),
        //        typeof (MultiPolygon),
        //        typeof (GeometryCollection),
        //    };

        private IGeometryFactory<TCoordinate> factory = null;

        /// <summary> 
        /// Gets the factory which contains the context in which this point was created.
        /// </summary>
        /// <returns>The factory for this point.</returns>
        public IGeometryFactory<TCoordinate> Factory
        {
            get { return factory; }
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
            get { return userData; }
            set { userData = value; }
        }

        private IExtents<TCoordinate> _extents;
        private Int32? _srid;

        /// <summary>
        /// Gets the bounding box of the <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        protected IExtents<TCoordinate> Extents
        {
            get { return _extents; }
        }

        /// <summary>  
        /// Gets or sets the ID of the Spatial Reference System used by the 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </summary>   
        /// <remarks>
        /// NTS supports Spatial Reference System information in the simple way
        /// defined in the SFS. A Spatial Reference System ID (SRID) is present in
        /// each <see cref="Geometry{TCoordinate}"/> object. 
        /// <see cref="Geometry{TCoordinate}"/> provides basic
        /// accessor operations for this field, but no others. The SRID is represented
        /// as a nullable <see cref="Int32"/>.
        /// </remarks>     
        public Int32? Srid
        {
            get { return _srid; }
            set
            {
                _srid = value;

                IGeometryCollection collection = this as IGeometryCollection;

                if (collection != null)
                {
                    foreach (IGeometry geometry in collection.Geometries)
                    {
                        geometry.Srid = value;
                    }
                }

                factory = new GeometryFactory(factory.PrecisionModel, value, factory.CoordinateSequenceFactory);
            }
        }

        public Geometry(IGeometryFactory factory)
        {
            this.factory = factory;
            _srid = factory.SRID;
        }

        /// <summary>  
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>The name of this <see cref="Geometry{TCoordinate}"/>s most specific interface.</returns>
        public abstract string GeometryType { get; }

        /// <summary>  
        /// Returns true if the array contains any non-empty <see cref="Geometry{TCoordinate}"/>s.
        /// </summary>
        /// <param name="geometries"> an array of <see cref="Geometry{TCoordinate}"/>s; no elements may be <see langword="null" /></param>
        /// <returns>            
        /// <see langword="true"/> if any of the <see cref="Geometry{TCoordinate}"/>s
        /// <c>IsEmpty</c> methods return <c>false</c>.
        /// </returns>
        protected static Boolean HasNonEmptyElements(IGeometry[] geometries)
        {
            foreach (IGeometry g in geometries)
            {
                if (!g.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>  
        /// Returns true if the array contains any <see langword="null" /> elements.
        /// </summary>
        /// <param name="array"> an array to validate.</param>
        /// <returns><see langword="true"/> if any of <c>array</c>s elements are <see langword="null" />.</returns>
        public static Boolean HasNullElements(object[] array)
        {
            foreach (object o in array)
            {
                if (o == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>  
        /// Returns the <see cref="PrecisionModel{TCoordinate}"/> used by the <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <returns>    
        /// the specification of the grid of allowable points, for this
        /// <see cref="Geometry{TCoordinate}"/> and all other <see cref="Geometry{TCoordinate}"/>s.
        /// </returns>
        public IPrecisionModel PrecisionModel
        {
            get { return Factory.PrecisionModel; }
        }

        /// <summary>  
        /// Returns a vertex of this <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <returns>    
        /// a Coordinate which is a vertex of this <see cref="Geometry{TCoordinate}"/>.
        /// Returns <see langword="null" /> if this Geometry is empty.
        /// </returns>
        public abstract ICoordinate Coordinate { get; }

        /// <summary>  
        /// Returns this <see cref="Geometry{TCoordinate}"/> s vertices. If you modify the coordinates
        /// in this array, be sure to call GeometryChanged afterwards.
        /// The <see cref="Geometry{TCoordinate}"/>s contained by composite <see cref="Geometry{TCoordinate}"/>s
        /// must be Geometry's; that is, they must implement <c>Coordinates</c>.
        /// </summary>
        /// <returns>The vertices of this <see cref="Geometry{TCoordinate}"/>.</returns>
        public abstract ICoordinate[] Coordinates { get; }

        /// <summary>  
        /// Returns the count of this <see cref="Geometry{TCoordinate}"/>s vertices. The <see cref="Geometry{TCoordinate}"/>
        /// s contained by composite <see cref="Geometry{TCoordinate}"/>s must be
        /// Geometry's; that is, they must implement <c>NumPoints</c>.
        /// </summary>
        /// <returns>The number of vertices in this <see cref="Geometry{TCoordinate}"/>.</returns>
        public abstract Int32 NumPoints { get; }

        /// <summary>
        /// Returns the number of Geometryes in a GeometryCollection,
        /// or 1, if the geometry is not a collection.
        /// </summary>
        public virtual Int32 NumGeometries
        {
            get { return 1; }
        }

        /// <summary>
        /// Returns an element Geometry from a GeometryCollection,
        /// or <code>this</code>, if the geometry is not a collection.
        /// </summary>
        /// <param name="n">The index of the geometry element.</param>
        /// <returns>The n'th geometry contained in this geometry.</returns>
        public virtual IGeometry GetGeometryN(Int32 n)
        {
            return this;
        }

        /// <summary> 
        /// Returns false if the <see cref="Geometry{TCoordinate}"/> not simple.
        /// Subclasses provide their own definition of "simple". If
        /// this <see cref="Geometry{TCoordinate}"/> is empty, returns <see langword="true"/>. 
        /// In general, the SFS specifications of simplicity seem to follow the
        /// following rule:
        ///  A Geometry is simple if the only self-intersections are at boundary points.
        /// For all empty <see cref="Geometry{TCoordinate}"/>s, <c>IsSimple==true</c>.
        /// </summary>
        /// <returns>    
        /// <see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> has any points of
        /// self-tangency, self-intersection or other anomalous points.
        /// </returns>
        public abstract Boolean IsSimple { get; }

        /// <summary>  
        /// Tests the validity of this <see cref="Geometry{TCoordinate}"/>.
        /// Subclasses provide their own definition of "valid".
        /// </summary>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> is valid.</returns>
        public virtual Boolean IsValid
        {
            get
            {
                IsValidOp isValidOp = new IsValidOp(this);
                return isValidOp.IsValid;
            }
        }

        /// <summary> 
        /// Returns whether or not the set of points in this <see cref="Geometry{TCoordinate}"/> is empty.
        /// </summary>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> equals the empty point.</returns>
        public abstract Boolean IsEmpty { get; }

        /// <summary>  
        /// Returns the minimum distance between this <see cref="Geometry{TCoordinate}"/>
        /// and the <see cref="Geometry{TCoordinate}"/> g.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> from which to compute the distance.</param>
        public Double Distance(IGeometry g)
        {
            return DistanceOp.Distance(this, g);
        }

        /// <summary> 
        /// Tests whether the distance from this <see cref="Geometry{TCoordinate}"/>
        /// to another is less than or equal to a specified value.
        /// </summary>
        /// <param name="geom">the Geometry to check the distance to.</param>
        /// <param name="distance">the distance value to compare.</param>
        /// <returns><see langword="true"/> if the geometries are less than <c>distance</c> apart.</returns>
        public Boolean IsWithinDistance(IGeometry geom, Double distance)
        {
            Double envDist = EnvelopeInternal.Distance(geom.EnvelopeInternal);

            if (envDist > distance)
            {
                return false;
            }

            return DistanceOp.IsWithinDistance(this, geom, distance);
        }

        /// <summary>  
        /// Returns the area of this <see cref="Geometry{TCoordinate}"/>.
        /// Areal Geometries have a non-zero area.
        /// They override this function to compute the area.
        /// Others return 0.0
        /// </summary>
        /// <returns>The area of the Geometry.</returns>
        public virtual Double Area
        {
            get { return 0.0; }
        }

        /// <summary> 
        /// Returns the length of this <see cref="Geometry{TCoordinate}"/>.
        /// Linear geometries return their length.
        /// Areal geometries return their perimeter.
        /// They override this function to compute the length.
        /// Others return 0.0
        /// </summary>
        /// <returns>The length of the Geometry.</returns>
        public virtual Double Length
        {
            get { return 0.0; }
        }

        /// <summary> 
        /// Computes the centroid of this <see cref="Geometry{TCoordinate}"/>.
        /// The centroid is equal to the centroid of the set of component Geometries of highest
        /// dimension (since the lower-dimension geometries contribute zero "weight" to the centroid).
        /// </summary>
        /// <returns>A Point which is the centroid of this Geometry.</returns>
        public IPoint Centroid
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

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
        /// Computes an interior point of this <see cref="Geometry{TCoordinate}"/>.
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

        public IPoint PointOnSurface
        {
            get { return InteriorPoint; }
        }

        private Dimensions dimension;

        /// <summary> 
        /// Returns the dimension of this <see cref="Geometry{TCoordinate}"/>.
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
        /// Returns the boundary, or the empty point if this <see cref="Geometry{TCoordinate}"/>
        /// is empty. For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <see cref="Geometry{TCoordinate}"/>.</returns>
        public virtual IGeometry Boundary
        {
            get { return boundary; }
            set { boundary = value; }
        }

        private Dimensions boundaryDimension;

        /// <summary> 
        /// Returns the dimension of this <see cref="Geometry{TCoordinate}"/>s inherent boundary.
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
        /// Returns this <see cref="Geometry{TCoordinate}"/>s bounding box. If this <see cref="Geometry{TCoordinate}"/>
        /// is the empty point, returns an empty <c>Point</c>. If the <see cref="Geometry{TCoordinate}"/>
        /// is a point, returns a non-empty <c>Point</c>. Otherwise, returns a
        /// <c>Polygon</c> whose points are (minx, miny), (maxx, miny), (maxx,
        /// maxy), (minx, maxy), (minx, miny).
        /// </summary>
        /// <returns>    
        /// An empty <c>Point</c> (for empty <see cref="Geometry{TCoordinate}"/>s), a
        /// <c>Point</c> (for <c>Point</c>s) or a <c>Polygon</c>
        /// (in all other cases).
        /// </returns>
        public IGeometry Envelope
        {
            get { return Factory.ToGeometry((Extents)EnvelopeInternal); }
        }

        /// <summary> 
        /// Returns the minimum and maximum x and y values in this <see cref="Geometry{TCoordinate}"/>
        /// , or a null <see cref="Extents{TCoordinate}"/> if this <see cref="Geometry{TCoordinate}"/> is empty.
        /// </summary>
        /// <returns>    
        /// This <see cref="Geometry{TCoordinate}"/>s bounding box; if the <see cref="Geometry{TCoordinate}"/>
        /// is empty, <c>Envelope.IsNull</c> will return <see langword="true"/>.
        /// </returns>
        public IExtents EnvelopeInternal
        {
            get
            {
                if (envelope == null)
                {
                    envelope = ComputeEnvelopeInternal();
                }

                return envelope;
            }
        }

        // DESIGN_NOTE: Delegate
        private class GeometryChangedFilter : IGeometryComponentFilter
        {
            public void Filter(IGeometry geom)
            {
                geom.GeometryChangedAction();
            }
        } ;

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
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is FF*FF****.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns><see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s are disjoint.</returns>
        public Boolean Disjoint(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return true;
            }

            return Relate(g).IsDisjoint();
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is FT*******, F**T***** or F***T****.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s touch;
        /// Returns false if both <see cref="Geometry{TCoordinate}"/>s are points.
        /// </returns>
        public Boolean Touches(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return false;
            }

            return Relate(g).IsTouches(Dimension, g.Dimension);
        }

        /// <summary>  
        /// Returns <see langword="true"/> if <c>disjoint</c> returns false.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns><see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s intersect.</returns>
        public Boolean Intersects(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return false;
            }

            // optimizations for rectangle arguments
            if (IsRectangle)
            {
                return RectangleIntersects.Intersects((IPolygon)this, g);
            }

            if (g.IsRectangle)
            {
                return RectangleIntersects.Intersects((IPolygon)g, this);
            }

            return Relate(g).IsIntersects();
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is
        ///  T*T****** (for a point and a curve, a point and an area or a line
        /// and an area) 0******** (for two curves).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s cross.
        /// For this function to return <see langword="true"/>, the <see cref="Geometry{TCoordinate}"/>
        /// s must be a point and a curve; a point and a surface; two curves; or a
        /// curve and a surface.
        /// </returns>
        public Boolean Crosses(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return false;
            }

            return Relate(g).IsCrosses(Dimension, g.Dimension);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is T*F**F***.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> is within <c>other</c>.</returns>
        public Boolean Within(IGeometry g)
        {
            return g.Contains(this);
        }

        /// <summary>
        /// Returns <see langword="true"/> if <c>other.within(this)</c> returns <see langword="true"/>.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> contains <c>other</c>.</returns>
        public Boolean Contains(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Contains(g.EnvelopeInternal))
            {
                return false;
            }

            // optimizations for rectangle arguments
            if (IsRectangle)
            {
                return RectangleContains.Contains((IPolygon)this, g);
            }

            // general case
            return Relate(g).IsContains();
        }

        /// <summary>
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is
        ///  T*T***T** (for two points or two surfaces)
        ///  1*T***T** (for two curves).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s overlap.
        /// For this function to return <see langword="true"/>, the <see cref="Geometry{TCoordinate}"/>
        /// s must be two points, two curves or two surfaces.
        /// </returns>
        public Boolean Overlaps(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return false;
            }

            return Relate(g).IsOverlaps(Dimension, g.Dimension);
        }

        /// <summary>
        /// Returns <see langword="true"/> if this geometry covers the specified geometry.
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
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/></param>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> covers <paramref name="g" /></returns>
        /// <seealso cref="Geometry.Contains" />
        /// <seealso cref="Geometry.CoveredBy" />
        public Boolean Covers(IGeometry g)
        {
            // short-circuit test
            if (!EnvelopeInternal.Contains(g.EnvelopeInternal))
            {
                return false;
            }

            // optimization for rectangle arguments
            if (IsRectangle)
            {
                return EnvelopeInternal.Contains(g.EnvelopeInternal);
            }

            return Relate(g).IsCovers();
        }

        /// <summary>
        /// Returns <see langword="true"/> if this geometry is covered by the specified geometry.
        /// <para>
        /// The <c>CoveredBy</c> predicate has the following equivalent definitions:
        ///     - Every point of this geometry is a point of the other geometry.
        ///     - The DE-9IM Intersection Matrix for the two geometries is <c>T*F**F***</c> or <c>*TF**F***</c> or <c>**FT*F***</c> or <c>**F*TF***</c>.
        ///     - <c>g.Covers(this)</c> (<c>CoveredBy</c> is the inverse of <c>Covers</c>).
        /// </para>
        /// Note the difference between <c>CoveredBy</c> and <c>Within</c>: <c>CoveredBy</c> is a more inclusive relation.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/></param>.
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> is covered by <paramref name="g" />.</returns>
        /// <seealso cref="Geometry.Within" />
        /// <seealso cref="Geometry.Covers" />
        public Boolean CoveredBy(IGeometry g)
        {
            return g.Covers(this);
        }

        /// <summary>  
        /// Returns <see langword="true"/> if the elements in the DE-9IM intersection
        /// matrix for the two <see cref="Geometry{TCoordinate}"/>s match the elements in <c>intersectionPattern</c>
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
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <param name="intersectionPattern">The pattern against which to check the intersection matrix for the two <see cref="Geometry{TCoordinate}"/>s.</param>
        /// <returns><see langword="true"/> if the DE-9IM intersection matrix for the two <see cref="Geometry{TCoordinate}"/>s match <c>intersectionPattern</c>.</returns>
        public Boolean Relate(IGeometry g, string intersectionPattern)
        {
            return Relate(g).Matches(intersectionPattern);
        }

        /// <summary>
        /// Returns the DE-9IM intersection matrix for the two <see cref="Geometry{TCoordinate}"/>s.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/></param>
        /// <returns>
        /// A matrix describing the intersections of the interiors,
        /// boundaries and exteriors of the two <see cref="Geometry{TCoordinate}"/>s.
        /// </returns>
        public IntersectionMatrix Relate(IGeometry g)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection((IGeometry)g);

            return RelateOp.Relate(this, g);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is T*F**FFF*.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns><see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s are equal.</returns>
        public Boolean Equals(IGeometry g)
        {
            if (IsEmpty && g.IsEmpty)
            {
                return true;
            }

            // Short-circuit test
            if (!EnvelopeInternal.Intersects(g.EnvelopeInternal))
            {
                return false;
            }

            // We use an alternative method for compare GeometryCollections (but not subclasses!), 
            if (isGeometryCollection(this) || isGeometryCollection(g))
            {
                return CompareGeometryCollections(this, g);
            }

            // Use RelateOp comparation method
            return Relate(g).IsEquals(Dimension, g.Dimension);
        }

        private static Boolean CompareGeometryCollections(IGeometry obj1, IGeometry obj2)
        {
            IGeometryCollection coll1 = obj1 as IGeometryCollection;
            IGeometryCollection coll2 = obj2 as IGeometryCollection;

            if (coll1 == null || coll2 == null)
            {
                return false;
            }

            // Short-circuit test
            if (coll1.NumGeometries != coll2.NumGeometries)
            {
                return false;
            }

            // Deep test
            for (Int32 i = 0; i < coll1.NumGeometries; i++)
            {
                IGeometry geom1 = coll1[i];
                IGeometry geom2 = coll2[i];

                if (!geom1.Equals(geom2))
                {
                    return false;
                }
            }

            return true;
        }

        public override Boolean Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (GetType().Namespace != obj.GetType().Namespace)
            {
                return false;
            }

            return Equals((IGeometry)obj);
        }

        public static Boolean operator ==(Geometry obj1, IGeometry obj2)
        {
            return Equals(obj1, obj2);
        }

        public static Boolean operator !=(Geometry obj1, IGeometry obj2)
        {
            return !(obj1 == obj2);
        }

        public override Int32 GetHashCode()
        {
            Int32 result = 17;

            foreach (Coordinate coord in Coordinates)
            {
                result = 37 * result + coord.X.GetHashCode();
            }

            return result;
        }

        /// <summary>
        /// Returns the Well-known Text representation of this <see cref="Geometry{TCoordinate}"/>.
        /// For a definition of the Well-known Text format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>
        /// The Well-known Text representation of this <see cref="Geometry{TCoordinate}"/>.
        /// </returns>
        public override string ToString()
        {
            return ToText();
        }

        /// <summary>
        /// Returns the Well-known Text representation of this <see cref="Geometry{TCoordinate}"/>.
        /// For a definition of the Well-known Text format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>
        /// The Well-known Text representation of this <see cref="Geometry{TCoordinate}"/>.
        /// </returns>
        public string ToText()
        {
            WKTWriter writer = new WKTWriter();
            return writer.Write(this);
        }

        public string AsText()
        {
            return ToText();
        }

        /// <summary>
        /// Returns the Well-known Binary representation of this <see cref="Geometry{TCoordinate}"/>.
        /// For a definition of the Well-known Binary format, see the OpenGIS Simple
        /// Features Specification.
        /// </summary>
        /// <returns>The Well-known Binary representation of this <see cref="Geometry{TCoordinate}"/>.</returns>
        public Byte[] ToBinary()
        {
            WKBWriter writer = new WKBWriter();
            return writer.Write(this);
        }

        public Byte[] AsBinary()
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
        /// Returns a buffer region around this <see cref="Geometry{TCoordinate}"/> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the Geometry with a disc of radius <c>distance</c>.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel{TCoordinate}"/> of the <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry{TCoordinate}"/>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(Double distance)
        {
            return BufferOp.Buffer(this, distance);
        }

        /// <summary>
        /// Returns a buffer region around this <see cref="Geometry{TCoordinate}"/> having the given width.
        /// The buffer of a Geometry is the Minkowski sum or difference of the Geometry with a disc of radius <c>distance</c>.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel{TCoordinate}"/> of the <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry{TCoordinate}"/>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(Double distance, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, endCapStyle);
        }

        /// <summary>
        /// Returns a buffer region around this <see cref="Geometry{TCoordinate}"/> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel{TCoordinate}"/> of the <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry{TCoordinate}"/>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(Double distance, Int32 quadrantSegments)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments);
        }

        /// <summary>
        /// Returns a buffer region around this <see cref="Geometry{TCoordinate}"/> having the given
        /// width and with a specified number of segments used to approximate curves.
        /// The buffer of a Geometry is the Minkowski sum of the Geometry with
        /// a disc of radius <c>distance</c>.  Curves in the buffer polygon are
        /// approximated with line segments.  This method allows specifying the
        /// accuracy of that approximation.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel{TCoordinate}"/> of the <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <param name="quadrantSegments">The number of segments to use to approximate a quadrant of a circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry{TCoordinate}"/>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry Buffer(Double distance, Int32 quadrantSegments, BufferStyle endCapStyle)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments, endCapStyle);
        }

        /// <summary>
        /// Returns the smallest convex <c>Polygon</c> that contains all the
        /// points in the <see cref="Geometry{TCoordinate}"/>. This obviously applies only to <see cref="Geometry{TCoordinate}"/>
        /// s which contain 3 or more points.
        /// </summary>
        /// <returns>the minimum-area convex polygon containing this <see cref="Geometry{TCoordinate}"/>'s points.</returns>
        public virtual IGeometry ConvexHull()
        {
            return (new ConvexHull(this)).GetConvexHull();
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing the points shared by this
        /// <see cref="Geometry{TCoordinate}"/> and <c>other</c>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the intersection.</param>
        /// <returns>The points common to the two <see cref="Geometry{TCoordinate}"/>s.</returns>
        public IGeometry Intersection(IGeometry other)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp.Overlay(this, other, SpatialFunctions.Intersection);
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing all the points in this <see cref="Geometry{TCoordinate}"/>
        /// and <c>other</c>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the union.</param>
        /// <returns>A set combining the points of this <see cref="Geometry{TCoordinate}"/> and the points of <c>other</c>.</returns>
        public IGeometry Union(IGeometry other)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp.Overlay(this, other, SpatialFunctions.Union);
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing the points making up this
        /// <see cref="Geometry{TCoordinate}"/> that do not make up <c>other</c>. This method
        /// returns the closure of the resultant <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the difference.</param>
        /// <returns>The point set difference of this <see cref="Geometry{TCoordinate}"/> with <c>other</c>.</returns>
        public IGeometry Difference(IGeometry other)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp.Overlay(this, other, SpatialFunctions.Difference);
        }

        /// <summary>
        /// Returns a set combining the points in this <see cref="Geometry{TCoordinate}"/> not in
        /// <c>other</c>, and the points in <c>other</c> not in this
        /// <see cref="Geometry{TCoordinate}"/>. This method returns the closure of the resultant
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the symmetric difference.</param>
        /// <returns>The point set symmetric difference of this <see cref="Geometry{TCoordinate}"/> with <c>other</c>.</returns>
        public IGeometry SymmetricDifference(IGeometry other)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp.Overlay(this, other, SpatialFunctions.SymDifference);
        }

        /// <summary>
        /// Returns true if the two <see cref="Geometry{TCoordinate}"/>s are exactly equal,
        /// up to a specified tolerance.
        /// Two Geometries are exactly within a tolerance equal iff:
        /// they have the same class,
        /// they have the same values of Coordinates,
        /// within the given tolerance distance, in their internal
        /// Coordinate lists, in exactly the same order.
        /// If this and the other <see cref="Geometry{TCoordinate}"/>s are
        /// composites and any children are not <see cref="Geometry{TCoordinate}"/>s, returns
        /// false.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/></param>
        /// <param name="tolerance">Distance at or below which two Coordinates will be considered equal.</param>
        /// <returns>
        /// <see langword="true"/> if this and the other <see cref="Geometry{TCoordinate}"/>
        /// are of the same class and have equal internal data.
        /// </returns>
        public abstract Boolean EqualsExact(IGeometry other, Double tolerance);

        /// <summary>
        /// Returns true if the two <see cref="Geometry{TCoordinate}"/>s are exactly equal.
        /// Two Geometries are exactly equal iff:
        /// they have the same class,
        /// they have the same values of Coordinates in their internal
        /// Coordinate lists, in exactly the same order.
        /// If this and the other <see cref="Geometry{TCoordinate}"/>s are
        /// composites and any children are not <see cref="Geometry{TCoordinate}"/>s, returns
        /// false.
        /// This provides a stricter test of equality than <c>equals</c>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if this and the other <see cref="Geometry{TCoordinate}"/>
        /// are of the same class and have equal internal data.
        /// </returns>
        public Boolean EqualsExact(IGeometry other)
        {
            return EqualsExact(other, 0);
        }

        /// <summary>
        /// Performs an operation with or on this <see cref="Geometry{TCoordinate}"/>'s
        /// coordinates. If you are using this method to modify the point, be sure
        /// to call GeometryChanged() afterwards. Note that you cannot use this
        /// method to
        /// modify this Geometry if its underlying CoordinateSequence's Get method
        /// returns a copy of the Coordinate, rather than the actual Coordinate stored
        /// (if it even stores Coordinates at all).
        /// </summary>
        /// <param name="filter">The filter to apply to this <see cref="Geometry{TCoordinate}"/>'s coordinates</param>
        public abstract void Apply(ICoordinateFilter filter);

        /// <summary>
        /// Performs an operation with or on this <see cref="Geometry{TCoordinate}"/> and its
        /// subelement <see cref="Geometry{TCoordinate}"/>s (if any).
        /// Only GeometryCollections and subclasses
        /// have subelement Geometry's.
        /// </summary>
        /// <param name="filter">
        /// The filter to apply to this <see cref="Geometry{TCoordinate}"/> (and
        /// its children, if it is a <c>GeometryCollection</c>).
        /// </param>
        public abstract void Apply(IGeometryFilter filter);

        /// <summary>
        /// Performs an operation with or on this Geometry and its
        /// component Geometry's. Only GeometryCollections and
        /// Polygons have component Geometry's; for Polygons they are the LinearRings
        /// of the shell and holes.
        /// </summary>
        /// <param name="filter">The filter to apply to this <see cref="Geometry{TCoordinate}"/>.</param>
        public abstract void Apply(IGeometryComponentFilter filter);

        public virtual object Clone()
        {
            Geometry clone = (Geometry)base.MemberwiseClone();

            if (clone.envelope != null)
            {
                clone.envelope = new Extents(clone.envelope);
            }

            return clone;
        }

        /// <summary>
        /// Converts this <see cref="Geometry{TCoordinate}"/> to normal form (or 
        /// canonical form ). Normal form is a unique representation for <see cref="Geometry{TCoordinate}"/>
        /// s. It can be used to test whether two <see cref="Geometry{TCoordinate}"/>s are equal
        /// in a way that is independent of the ordering of the coordinates within
        /// them. Normal form equality is a stronger condition than topological
        /// equality, but weaker than pointwise equality. The definitions for normal
        /// form use the standard lexicographical ordering for coordinates. "Sorted in
        /// order of coordinates" means the obvious extension of this ordering to
        /// sequences of coordinates.
        /// </summary>
        public abstract void Normalize();

        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, equal to,
        /// or less than another <see cref="Geometry{TCoordinate}"/>. 
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
        /// If the two <see cref="Geometry{TCoordinate}"/>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </summary>
        /// <param name="o">A <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/></param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        public Int32 CompareTo(object o)
        {
            return CompareTo((IGeometry)o);
        }

        public Int32 CompareTo(IGeometry geom)
        {
            Geometry other = (Geometry)geom;

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

            return CompareToSameClass(geom);
        }

        /// <summary>
        /// Returns whether the two <see cref="Geometry{TCoordinate}"/>s are equal, from the point
        /// of view of the <c>EqualsExact</c> method. Called by <c>EqualsExact</c>
        /// . In general, two <see cref="Geometry{TCoordinate}"/> classes are considered to be
        /// "equivalent" only if they are the same class. An exception is <c>LineString</c>
        /// , which is considered to be equivalent to its subclasses.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compare this <see cref="Geometry{TCoordinate}"/> for equality.</param>
        /// <returns>
        /// <see langword="true"/> if the classes of the two <see cref="Geometry{TCoordinate}"/>
        /// s are considered to be equal by the <c>equalsExact</c> method.
        /// </returns>
        protected Boolean IsEquivalentClass(IGeometry other)
        {
            return GetType().FullName == other.GetType().FullName;
        }

        /// <summary>
        /// Throws an exception if <c>g</c>'s class is <c>GeometryCollection</c>. 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> to check.</param>
        /// <exception cref="ArgumentException">
        /// if <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>
        protected void CheckNotGeometryCollection(IGeometry g)
        {
            if (isGeometryCollection(g))
            {
                throw new ArgumentException("This method does not support GeometryCollection arguments");
            }
        }

        /// <summary>
        /// Returns <see langword="true"/> if <c>g</c>'s class is <c>GeometryCollection</c>. 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> to check.</param>
        /// <exception cref="ArgumentException">
        /// If <c>g</c> is a <c>GeometryCollection</c>, but not one of its subclasses.
        /// </exception>        
        private static Boolean isGeometryCollection(IGeometry g)
        {
            return g is IGeometryCollection;
        }

        /// <summary>
        /// Returns the minimum and maximum x and y values in this <see cref="Geometry{TCoordinate}"/>
        /// , or a null <see cref="Extents{TCoordinate}"/> if this <see cref="Geometry{TCoordinate}"/> is empty.
        /// Unlike <c>EnvelopeInternal</c>, this method calculates the <see cref="Extents{TCoordinate}"/>
        /// each time it is called; <c>EnvelopeInternal</c> caches the result
        /// of this method.        
        /// </summary>
        /// <returns>
        /// This <see cref="Geometry{TCoordinate}"/>s bounding box; if the <see cref="Geometry{TCoordinate}"/>
        /// is empty, <c>Envelope.IsNull</c> will return <see langword="true"/>.
        /// </returns>
        protected abstract IExtents ComputeEnvelopeInternal();

        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, equal to,
        /// or less than another <see cref="Geometry{TCoordinate}"/> having the same class.
        /// </summary>
        /// <param name="o">A <see cref="Geometry{TCoordinate}"/> having the same class as this <see cref="Geometry{TCoordinate}"/>.</param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        protected internal abstract Int32 CompareToSameClass(object o);

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
        protected Int32 Compare(ArrayList a, ArrayList b)
        {
            IEnumerator i = a.GetEnumerator();
            IEnumerator j = b.GetEnumerator();

            while (i.MoveNext() && j.MoveNext())
            {
                IComparable aElement = (IComparable)i.Current;
                IComparable bElement = (IComparable)j.Current;
                Int32 comparison = aElement.CompareTo(bElement);

                if (comparison != 0)
                {
                    return comparison;
                }
            }

            if (i.MoveNext())
            {
                return 1;
            }

            if (j.MoveNext())
            {
                return -1;
            }

            return 0;
        }

        protected Boolean Equal(ICoordinate a, ICoordinate b, Double tolerance)
        {
            if (tolerance == 0)
            {
                return a.Equals(b);
            }

            return a.Distance(b) <= tolerance;
        }

        private Int32 ClassSortIndex
        {
            get
            {
                for (Int32 i = 0; i < SortedClasses.Length; i++)
                {
                    if (GetType().Equals(SortedClasses[i]))
                    {
                        return i;
                    }
                }

                Assert.ShouldNeverReachHere("Class not supported: " + GetType().FullName);
                return -1;
            }
        }

        private IPoint CreatePointFromInternalCoord(ICoordinate coord, IGeometry exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        public virtual Boolean IsRectangle
        {
            get
            {
                // Polygon overrides to check for actual rectangle
                return false;
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// A predefined <see cref="Geometry{TCoordinate}Factory{TCoordinate}" /> with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Fixed" />.
        /// </summary>
        /// <seealso cref="GeometryFactory.Default" />
        /// <seealso cref="GeometryFactory.Fixed"/>
        public static readonly IGeometryFactory DefaultFactory = GeometryFactory.Default;

        /* END ADDED BY MPAUL42: monoGIS team */
    }
}