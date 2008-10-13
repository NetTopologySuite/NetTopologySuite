using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Indexing;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using GisSharpBlog.NetTopologySuite.Operation.Distance;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Predicate;
using GisSharpBlog.NetTopologySuite.Operation.Relate;
using GisSharpBlog.NetTopologySuite.Operation.Valid;
using NPack.Interfaces;
using NPack;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>  
    /// Basic implementation of <see cref="IGeometry{TCoordinate}"/>, the fundamental
    /// unit of spatial reasoning in NTS.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Clone"/> returns a deep copy of the object.
    /// </para>
    /// <para>
    /// Binary Predicates: 
    /// Because it is not clear at this time what semantics for spatial
    /// analysis methods involving <see cref="GeometryCollection{TCoordinate}" />s 
    /// would be useful, <see cref="GeometryCollection{TCoordinate}" />s are not 
    /// supported as arguments to binary predicates 
    /// (other than <see cref="ConvexHull"/>) or the 
    /// <see cref="Relate(IGeometry{TCoordinate})"/> family of methods.
    /// </para>
    /// <para>
    /// Set-Theoretic Methods: 
    /// The spatial analysis methods will
    /// return the most specific class possible to represent the result. If the
    /// result is homogeneous, a <see cref="Point{TCoordinate}"/>, 
    /// <see cref="LineString{TCoordinate}"/>, or <see cref="Polygon{TCoordinate}" /> 
    /// will be returned if the result contains a single element; 
    /// otherwise, a <see cref="MultiPoint{TCoordinate}"/>, 
    /// <see cref="MultiLineString{TCoordinate}"/>, or 
    /// <see cref="MultiPolygon{TCoordinate}"/> 
    /// will be returned. If the result is heterogeneous a 
    /// <see cref="GeometryCollection{TCoordinate}" /> will be returned.
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
    /// the NTS definition of <see cref="IsSimple"/>.
    /// Noded applies only to overlays involving <see cref="LineString{TCoordinate}"/>s. It
    /// means that all intersection points on <see cref="LineString{TCoordinate}"/>s will be
    /// present as endpoints of <see cref="LineString{TCoordinate}"/>s in the result.
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
    /// analysis methods, it will throw a <see cref="TopologyException"/>. 
    /// If possible the exception will report the location of the collapse. 
    /// </para>
    /// <para>
    /// NOTE: <see cref="Object.Equals(object)" /> and <see cref="Object.GetHashCode" /> 
    /// are overridden, so that when two topologically equal Geometries are added 
    /// to collections and hash table implementations, they will collide. 
    /// This behavior is <strong>not</strong> desired in many cases, and is
    /// the opposite of JTS and previous versions of NTS. To get the desired behavior, 
    /// use <see cref="GeometryReferenceEqualityComparer{TCoordinate}.Default"/> as 
    /// the key equality comparer. The reasoning for this change is twofold. First, 
    /// <see cref="ISpatialRelation{TCoordinate}"/> includes 
    /// <see cref="IEquatable{IGeometry}"/> to derive the Equals method. 
    /// The sematics for this interface imply that the type-specific equality is
    /// value-based, since if it was reference equality, the interface would not 
    /// be implemented. Given the semantics of spatial relations, where two 
    /// <see cref="IGeometry"/> are equal if their coordinate-by-coordinate values are equal,
    /// implementing this interface makes sense. Second, this version of NTS is moving
    /// geometry objects toward immutability. This will allow greater ability to do
    /// distributed spatial processing and use functional-programming constructs as NTS evolves.
    /// If a geometry instance is immutable, value-type equality is more meaningful and more 
    /// desired. The use of collections and hash table implementations which rely on reference
    /// equality become more of an implementation detail which can be effectively hidden
    /// in instances where it is needed by a 
    /// <see cref="GeometryReferenceEqualityComparer{TCoordinate}"/>. 
    /// </para>
    /// <para>
    /// DEVELOPER NOTE: should we implement a ReferenceGeometryCollection and ReferenceGeometryDictionary
    /// to alleviate the increase burden on NTS users?
    /// </para>
    /// </remarks>
    [Serializable]
    public abstract class Geometry<TCoordinate> : IGeometry<TCoordinate>
         where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                             IComparable<TCoordinate>, IConvertible,
                             IComputable<Double, TCoordinate>
    {
        private static readonly RuntimeTypeHandle[] _sortedClasses
            = new RuntimeTypeHandle[]
            {
                typeof (IPoint<TCoordinate>).TypeHandle,
                typeof (IMultiPoint<TCoordinate>).TypeHandle,
                typeof (ILineString<TCoordinate>).TypeHandle,
                typeof (ILinearRing<TCoordinate>).TypeHandle,
                typeof (IMultiLineString<TCoordinate>).TypeHandle,
                typeof (IPolygon<TCoordinate>).TypeHandle,
                typeof (IMultiPolygon<TCoordinate>).TypeHandle,
                typeof (IGeometryCollection<TCoordinate>).TypeHandle,
            };

        private IGeometryFactory<TCoordinate> _factory;
        private Object _userData;
        private Extents<TCoordinate> _extents;
        private String _srid;
        //private Dimensions _dimension;
        private IGeometry<TCoordinate> _boundary;
        private Dimensions _boundaryDimension;
        private ICoordinateSystem<TCoordinate> _spatialReference;

        protected Geometry(IGeometryFactory<TCoordinate> factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }

            _factory = factory;
            _srid = factory.Srid;
            _spatialReference = factory.SpatialReference;
        }

        public override Boolean Equals(Object obj)
        {
            IGeometry<TCoordinate> other = obj as IGeometry<TCoordinate>;
            return Equals(other);
        }

        /// <summary>
        /// Returns the Well-Known Text representation of this 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <remarks>
        /// For a definition of the Well-Known Text format, see the OpenGIS Simple
        /// Features Specification.
        /// </remarks>
        /// <returns>
        /// The Well-Known Text representation of this 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </returns>
        public override String ToString()
        {
            return AsText();
        }

        public override Int32 GetHashCode()
        {
            Int32 result = 17;

            foreach (TCoordinate coord in Coordinates)
            {
                result = 37 * result ^ coord.GetHashCode();
            }

            return result;
        }

        /// <summary>
        /// Returns the number of Geometries in a GeometryCollection,
        /// or 1, if the geometry is not a collection.
        /// </summary>
        public virtual Int32 GeometryCount
        {
            get { return 1; }
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
        /// Computes an interior point of this <see cref="Geometry{TCoordinate}"/>.
        /// An interior point is guaranteed to lie in the interior of the Geometry,
        /// if it possible to calculate such a point exactly. Otherwise,
        /// the point may lie on the boundary of the point.
        /// </summary>
        /// <returns>
        /// An <see cref="IPoint{TCoordinate}"/> which is in the 
        /// interior of this Geometry.
        /// </returns>
        public IPoint<TCoordinate> InteriorPoint
        {
            get
            {
                TCoordinate interiorPt;
                Dimensions dim = Dimension;

                switch (dim)
                {
                    case Dimensions.Point:
                        {
                            InteriorPointPoint<TCoordinate> intPt
                                = new InteriorPointPoint<TCoordinate>(this);
                            interiorPt = intPt.InteriorPoint;
                        }
                        break;
                    case Dimensions.Curve:
                        {
                            InteriorPointLine<TCoordinate> intPt
                                = new InteriorPointLine<TCoordinate>(this);
                            interiorPt = intPt.InteriorPoint;
                        }
                        break;
                    default:
                        {
                            InteriorPointArea<TCoordinate> intPt
                                = new InteriorPointArea<TCoordinate>(this);
                            interiorPt = intPt.InteriorPoint;
                        }
                        break;
                }

                return createPointFromInternalCoord(interiorPt, this);
            }
        }

        public IPoint<TCoordinate> PointOnSurface
        {
            get { return InteriorPoint; }
        }

        #region IComparable<IGeometry<TCoordinate>> Members
        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, 
        /// equal to, or less than another <see cref="Geometry{TCoordinate}"/>. 
        /// </summary>
        /// <param name="other">
        /// A <see cref="Geometry{TCoordinate}"/> with which to 
        /// compare this <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <paramref name="other"/>, 
        /// as defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        ///     <see cref="Point{TCoordinate}"/> (lowest),
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="LinearRing{TCoordinate}"/>,
        ///     <see cref="MultiLineString{TCoordinate}"/>,
        ///     <see cref="Polygon{TCoordinate}"/>,
        ///     <see cref="MultiPolygon{TCoordinate}"/>,
        ///     <see cref="GeometryCollection{TCoordinate}"/> (highest).
        /// If the two <see cref="Geometry{TCoordinate}"/>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        public Int32 CompareTo(Object other)
        {
            if (other == null) throw new ArgumentNullException("other");

            IGeometry g = other as IGeometry;
            return CompareTo(g);
        }

        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, 
        /// equal to, or less than another <see cref="Geometry{TCoordinate}"/>. 
        /// </summary>
        /// <param name="other">
        /// A <see cref="Geometry{TCoordinate}"/> with which to 
        /// compare this <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <paramref name="other"/>, 
        /// as defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        ///     <see cref="Point{TCoordinate}"/> (lowest),
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="LinearRing{TCoordinate}"/>,
        ///     <see cref="MultiLineString{TCoordinate}"/>,
        ///     <see cref="Polygon{TCoordinate}"/>,
        ///     <see cref="MultiPolygon{TCoordinate}"/>,
        ///     <see cref="GeometryCollection{TCoordinate}"/> (highest).
        /// If the two <see cref="Geometry{TCoordinate}"/>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        public Int32 CompareTo(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            IGeometry<TCoordinate> g = other as IGeometry<TCoordinate>;
            return CompareTo(g);
        }

        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, 
        /// equal to, or less than another <see cref="Geometry{TCoordinate}"/>. 
        /// </summary>
        /// <param name="other">
        /// A <see cref="Geometry{TCoordinate}"/> with which to 
        /// compare this <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <paramref name="other"/>, 
        /// as defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        /// <remarks>
        /// If their classes are different, they are compared using the following
        /// ordering:
        ///     <see cref="Point{TCoordinate}"/> (lowest),
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="MultiPoint{TCoordinate}"/>,
        ///     <see cref="LinearRing{TCoordinate}"/>,
        ///     <see cref="MultiLineString{TCoordinate}"/>,
        ///     <see cref="Polygon{TCoordinate}"/>,
        ///     <see cref="MultiPolygon{TCoordinate}"/>,
        ///     <see cref="GeometryCollection{TCoordinate}"/> (highest).
        /// If the two <see cref="Geometry{TCoordinate}"/>s have the same class, their first
        /// elements are compared. If those are the same, the second elements are
        /// compared, etc.
        /// </remarks>
        public Int32 CompareTo(IGeometry<TCoordinate> other)
        {
            if (other == null) throw new ArgumentNullException("other");

            Int32 classSortIndex = getClassSortIndex(this);
            Int32 otherClassSortIndex = getClassSortIndex(other);

            if (classSortIndex != otherClassSortIndex)
            {
                return classSortIndex - otherClassSortIndex;
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

            return CompareToSameClass(other);
        }

        #endregion

        #region IEquatable<IGeometry<TCoordinate>> Members

        public Boolean Equals(IGeometry<TCoordinate> g)
        {
            if (ReferenceEquals(g, null))
            {
                return false;
            }

            if (IsEmpty && g.IsEmpty || ReferenceEquals(this, g))
            {
                return true;
            }

            // Short-circuit test
            if (!ExtentsInternal.Intersects(g.Extents))
            {
                return false;
            }

            // We use an alternative method for compare GeometryCollections 
            // (but not subclasses!), 
            if (isGeometryCollection(this) || isGeometryCollection(g))
            {
                return compareGeometryCollections(this, g);
            }

            // Use RelateOp comparation method
            return Relate(g).IsEquals(Dimension, g.Dimension);
        }

        #endregion

        #region IVertexStream<TCoordinate,DoubleComponent> Members

        public IEnumerable<TCoordinate> GetVertexes(ITransformMatrix<DoubleComponent> transform)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TCoordinate> GetVertexes()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGeometry<TCoordinate> Members

        /// <summary> 
        /// Computes the centroid of this <see cref="Geometry{TCoordinate}"/>.
        /// The centroid is equal to the centroid of the set of component Geometries of highest
        /// dimension (since the lower-dimension geometries contribute zero "weight" to the centroid).
        /// </summary>
        /// <returns>A Point which is the centroid of this Geometry.</returns>
        public IPoint<TCoordinate> Centroid
        {
            get
            {
                if (IsEmpty)
                {
                    return null;
                }

                TCoordinate centPt;
                Dimensions dim = Dimension;

                switch (dim)
                {
                    case Dimensions.Point:
                        {
                            CentroidPoint<TCoordinate> cent
                                = new CentroidPoint<TCoordinate>(Factory.CoordinateFactory);
                            cent.Add(this);
                            centPt = cent.Centroid;
                        }
                        break;
                    case Dimensions.Curve:
                        {
                            CentroidLine<TCoordinate> cent
                                = new CentroidLine<TCoordinate>(Factory.CoordinateFactory);
                            cent.Add(this);
                            centPt = cent.Centroid;
                        }
                        break;
                    default:
                        {
                            CentroidArea<TCoordinate> cent
                                = new CentroidArea<TCoordinate>(Factory.CoordinateFactory);
                            cent.Add(this);
                            centPt = cent.Centroid;
                        }
                        break;
                }

                return createPointFromInternalCoord(centPt, this);
            }
        }

        public virtual IGeometry<TCoordinate> Clone()
        {
            Geometry<TCoordinate> clone = (Geometry<TCoordinate>)MemberwiseClone();

            if (clone.ExtentsInternal != null)
            {
                clone.ExtentsInternal = (Extents<TCoordinate>)ExtentsInternal.Clone();
            }

            if (clone._boundary != null)
            {
                clone._boundary = _boundary.Clone();
            }

            ICloneable clonableUserData = _userData as ICloneable;

            if (clonableUserData != null)
            {
                clone._userData = clonableUserData.Clone();
            }

            return clone;
        }

        public abstract ICoordinateSequence<TCoordinate> Coordinates { get; }

        /// <summary>  
        /// Returns this <see cref="Geometry{TCoordinate}"/>s bounding box. If this <see cref="Geometry{TCoordinate}"/>
        /// is the empty point, returns an empty <c>Point</c>. If the <see cref="Geometry{TCoordinate}"/>
        /// is a point, returns a non-empty <c>Point</c>. Otherwise, returns a
        /// <see cref="Polygon{TCoordinate}" /> whose points are (minx, miny), (maxx, miny), (maxx,
        /// maxy), (minx, maxy), (minx, miny).
        /// </summary>
        /// <returns>    
        /// An empty <c>Point</c> (for empty <see cref="Geometry{TCoordinate}"/>s), a
        /// <c>Point</c> (for <c>Point</c>s) or a <see cref="Polygon{TCoordinate}" />
        /// (in all other cases).
        /// </returns>
        public IGeometry<TCoordinate> Envelope
        {
            get { return Factory.ToGeometry(ExtentsInternal); }
        }

        /// <summary>
        /// Gets the bounding box of the <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        public IExtents<TCoordinate> Extents
        {
            get { return ExtentsInternal; }
        }

        /// <summary> 
        /// Gets the factory which contains the context in which this point was created.
        /// </summary>
        /// <returns>The factory for this point.</returns>
        public IGeometryFactory<TCoordinate> Factory
        {
            get { return _factory; }
        }

        /// <summary>  
        /// Returns the <see cref="PrecisionModel{TCoordinate}"/> 
        /// used by the <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <returns>    
        /// The specification of the grid of allowable points, for this
        /// <see cref="Geometry{TCoordinate}"/> and all other <see cref="Geometry{TCoordinate}"/>s.
        /// </returns>
        public IPrecisionModel<TCoordinate> PrecisionModel
        {
            get { return Factory.PrecisionModel; }
        }

        public IGeometry<TCoordinate> Project(ICoordinateSystem<TCoordinate> toCoordinateSystem)
        {
            if (toCoordinateSystem == null) throw new ArgumentNullException("toCoordinateSystem");
            throw new NotImplementedException();
        }

        public ICoordinateSystem<TCoordinate> SpatialReference
        {
            get { return _spatialReference; }
            set { _spatialReference = value; }
        }

        #endregion

        #region IGeometry Members

        public Byte[] AsBinary()
        {
            return _factory.WkbWriter.Write(this);
        }

        public String AsText()
        {
            return _factory.WktWriter.Write(this);
        }

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
            get { return _boundaryDimension; }
            set { _boundaryDimension = value; }
        }

        IPoint IGeometry.Centroid
        {
            get { return Centroid; }
        }

        IGeometry IGeometry.Clone()
        {
            return Clone();
        }

        ICoordinateSequence IGeometry.Coordinates
        {
            get { return Coordinates; }
        }

        /// <summary> 
        /// Gets the dimension of this <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <returns>  
        /// The dimension of the class implementing this interface, whether
        /// or not this object is the empty point.
        /// </returns>
        public abstract Dimensions Dimension { get; }

        IGeometry IGeometry.Envelope
        {
            get { return Envelope; }
        }

        /// <summary>
        /// Returns <see langword="true"/> if the DE-9IM intersection matrix for the two
        /// <see cref="Geometry{TCoordinate}"/>s is T*F**FFF*.
        /// </summary>
        /// <param name="g">
        /// The <see cref="Geometry{TCoordinate}"/> with which to compare this 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the two <see cref="Geometry{TCoordinate}"/>s are equal.
        /// </returns>
        public Boolean Equals(IGeometry g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // TODO: this could be redone to relate the IGeometry 
            // instance to this instance, using ICoordinate
            return Equals(convertIGeometry(g));
        }

        IExtents IGeometry.Extents
        {
            get { return Extents; }
        }

        IGeometryFactory IGeometry.Factory
        {
            get { return Factory; }
        }

        public String GeometryTypeName
        {
            get { return GeometryType.ToString(); }
        }

        /// <summary> 
        /// Returns whether or not the set of points in this <see cref="Geometry{TCoordinate}"/> is empty.
        /// </summary>
        /// <returns><see langword="true"/> if this <see cref="Geometry{TCoordinate}"/> equals the empty point.</returns>
        public abstract Boolean IsEmpty { get; }

        public virtual Boolean IsRectangle
        {
            get
            {
                // Polygon overrides to check for actual rectangle
                if (Dimension == Dimensions.Surface)
                {
                    throw new InvalidOperationException("This method must be " +
                                                        "overridden in 2D geometries.");
                }

                return false;
            }
        }

        /// <summary> 
        /// Returns <see langword="false"/> if the <see cref="Geometry{TCoordinate}"/> not simple.
        /// Subclasses provide their own definition of "simple". If
        /// this <see cref="Geometry{TCoordinate}"/> is empty, returns <see langword="true"/>. 
        /// </summary>
        /// <remarks>
        /// In general, the SFS specifications of simplicity seem to follow the
        /// following rule:
        ///  A Geometry is simple if the only self-intersections are at boundary points.
        /// For all empty <see cref="Geometry{TCoordinate}"/>s, <c>IsSimple==true</c>.
        /// </remarks>
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
                IsValidOp<TCoordinate> isValidOp = new IsValidOp<TCoordinate>(this);
                return isValidOp.IsValid;
            }
        }

        /// <summary>  
        /// Returns the count of this <see cref="Geometry{TCoordinate}"/>s vertices.
        /// </summary>
        /// <remarks>
        /// The <see cref="Geometry{TCoordinate}"/>s contained by composite 
        /// <see cref="Geometry{TCoordinate}"/>s must be 
        /// <see cref="Geometry{TCoordinate}"/> instances; that is, 
        /// they must implement <see cref="IGeometry{TCoordinate}.PointCount"/>.
        /// </remarks>
        /// <returns>The number of vertices in this <see cref="Geometry{TCoordinate}"/>.</returns>
        public abstract Int32 PointCount { get; }

        /// <summary>  
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>The name of this <see cref="Geometry{TCoordinate}"/>s most specific interface.</returns>
        public abstract OgcGeometryType GeometryType { get; }

        /// <summary>
        /// Converts this <see cref="Geometry{TCoordinate}"/> to normal form (or 
        /// canonical form).
        /// </summary>
        /// <remarks>
        /// Normal form is a unique representation for 
        /// <see cref="Geometry{TCoordinate}"/>s. It can be used to test whether two <see cref="Geometry{TCoordinate}"/>s are equal
        /// in a way that is independent of the ordering of the coordinates within
        /// them. Normal form equality is a stronger condition than topological
        /// equality, but weaker than pointwise equality. The definitions for normal
        /// form use the standard lexicographical ordering for coordinates. "Sorted in
        /// order of coordinates" means the obvious extension of this ordering to
        /// sequences of coordinates.
        /// </remarks>
        public abstract void Normalize();

        IPrecisionModel IGeometry.PrecisionModel
        {
            get { return PrecisionModel; }
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
        /// accessor operations for this field, but no others. 
        /// <para>
        /// The SRID is represented as a nullable <see cref="Int32"/>.
        /// </para>
        /// </remarks>     
        public String Srid
        {
            get { return _srid; }
            set
            {
                if (_srid == value)
                {
                    return;
                }

                _srid = value;
                _factory = new GeometryFactory<TCoordinate>(value,
                                                            _factory.CoordinateSequenceFactory);

                IGeometryCollection collection = this as IGeometryCollection;

                if (collection != null)
                {
                    foreach (Geometry<TCoordinate> geometry in collection)
                    {
                        geometry._srid = value;
                        geometry._factory = _factory;
                    }
                }

            }
        }

        /// <summary> 
        /// Gets or sets the user data object for this point, if any.
        /// </summary>
        /// <remarks>
        /// A simple scheme for applications to add their own custom data to a Geometry.
        /// Note that user data objects are not present in geometries created by
        /// construction methods.
        /// </remarks>
        public Object UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }

        #endregion

        #region ICloneable Members

        Object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        #region ISpatialOperator<TCoordinate> Members

        /// <summary>  
        /// Gets the boundary, or the empty point if this <see cref="Geometry{TCoordinate}"/>
        /// is empty. For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <see cref="Geometry{TCoordinate}"/>.</returns>
        public virtual IGeometry<TCoordinate> Boundary
        {
            get { return _boundary; }
        }

        /// <summary>  
        /// Returns the minimum distance between this <see cref="Geometry{TCoordinate}"/>
        /// and the <see cref="Geometry{TCoordinate}"/> g.
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> from which to compute the distance.</param>
        public Double Distance(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            return DistanceOp<TCoordinate>.FindDistance(this, g);
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
        public IGeometry<TCoordinate> Buffer(Double distance)
        {
            return BufferOp<TCoordinate>.Buffer(this, distance);
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
        public IGeometry<TCoordinate> Buffer(Double distance, Int32 quadrantSegments)
        {
            return BufferOp<TCoordinate>.Buffer(this, distance, quadrantSegments);
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
        public IGeometry<TCoordinate> Buffer(Double distance, BufferStyle endCapStyle)
        {
            return BufferOp<TCoordinate>.Buffer(this, distance, endCapStyle);
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
        /// <param name="quadrantSegments">
        /// The number of segments to use to approximate a quadrant of a circle.
        /// </param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry{TCoordinate}"/>
        /// are less than or equal to <c>distance</c>.
        /// </returns>
        public IGeometry<TCoordinate> Buffer(Double distance, 
                                             Int32 quadrantSegments, 
                                             BufferStyle endCapStyle)
        {
            return BufferOp<TCoordinate>.Buffer(this, distance, quadrantSegments, endCapStyle);
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing the points shared by this
        /// <see cref="Geometry{TCoordinate}"/> and <c>other</c>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the intersection.</param>
        /// <returns>The points common to the two <see cref="Geometry{TCoordinate}"/>s.</returns>
        public IGeometry<TCoordinate> Intersection(IGeometry<TCoordinate> other)
        {
            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp<TCoordinate>.Overlay(this, other, SpatialFunctions.Intersection);
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing all the points in this <see cref="Geometry{TCoordinate}"/>
        /// and <c>other</c>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the union.</param>
        /// <returns>A set combining the points of this <see cref="Geometry{TCoordinate}"/> and the points of <c>other</c>.</returns>
        public IGeometry<TCoordinate> Union(IGeometry<TCoordinate> other)
        {
            if (other == null) throw new ArgumentNullException("other");

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp<TCoordinate>.Overlay(this, other, SpatialFunctions.Union);
        }

        /// <summary>
        /// Returns a <see cref="Geometry{TCoordinate}"/> representing the points making up this
        /// <see cref="Geometry{TCoordinate}"/> that do not make up <c>other</c>. This method
        /// returns the closure of the resultant <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the difference.</param>
        /// <returns>The point set difference of this <see cref="Geometry{TCoordinate}"/> with <c>other</c>.</returns>
        public IGeometry<TCoordinate> Difference(IGeometry<TCoordinate> other)
        {
            if (other == null) throw new ArgumentNullException("other");

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp<TCoordinate>.Overlay(this, other, SpatialFunctions.Difference);
        }

        /// <summary>
        /// Returns a set combining the points in this <see cref="Geometry{TCoordinate}"/> not in
        /// <c>other</c>, and the points in <c>other</c> not in this
        /// <see cref="Geometry{TCoordinate}"/>. This method returns the closure of the resultant
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="other">The <see cref="Geometry{TCoordinate}"/> with which to compute the symmetric difference.</param>
        /// <returns>The point set symmetric difference of this <see cref="Geometry{TCoordinate}"/> with <c>other</c>.</returns>
        public IGeometry<TCoordinate> SymmetricDifference(IGeometry<TCoordinate> other)
        {
            if (other == null) throw new ArgumentNullException("other");

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(other);

            return OverlayOp<TCoordinate>.Overlay(this, other, SpatialFunctions.SymDifference);
        }

        /// <summary>
        /// Returns the smallest convex <see cref="Polygon{TCoordinate}" /> that contains all the
        /// points in the <see cref="Geometry{TCoordinate}"/>. This obviously applies only to <see cref="Geometry{TCoordinate}"/>
        /// s which contain 3 or more points.
        /// </summary>
        /// <returns>the minimum-area convex polygon containing this <see cref="Geometry{TCoordinate}"/>'s points.</returns>
        public virtual IGeometry<TCoordinate> ConvexHull()
        {
            return (new ConvexHull<TCoordinate>(this)).GetConvexHull();
        }

        #endregion

        #region ISpatialRelation<TCoordinate> Members

        public Boolean Contains(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            if (!ExtentsInternal.Contains(g.Extents))
            {
                return false;
            }

            // optimizations for rectangle arguments
            if (IsRectangle)
            {
                return RectangleContains<TCoordinate>.Contains(this as IPolygon<TCoordinate>, g);
            }

            // general case
            return Relate(g).IsContains();
        }

        public Boolean Contains(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean CoveredBy(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            return g.Covers(this);
        }

        public Boolean CoveredBy(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            return g.Covers(this, tolerance);
        }

        public Boolean Covers(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            if (!ExtentsInternal.Contains(g.Extents))
            {
                return false;
            }

            // optimization for rectangle arguments
            if (IsRectangle)
            {
                return ExtentsInternal.Contains(g.Extents);
            }

            return Relate(g).IsCovers();
        }

        public Boolean Covers(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean Crosses(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            return ExtentsInternal.Intersects(g.Extents) && 
                   Relate(g).IsCrosses(Dimension, g.Dimension);
        }

        public Boolean Crosses(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean Disjoint(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            if (!ExtentsInternal.Intersects(g.Extents))
            {
                return true;
            }

            return Relate(g).IsDisjoint();
        }

        public Boolean Disjoint(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");
            throw new NotImplementedException();
        }

        public abstract Boolean Equals(IGeometry<TCoordinate> g, Tolerance tolerance);

        public Boolean EqualsExact(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");
            return EqualsExact(g, Tolerance.Zero);
        }

        public abstract Boolean EqualsExact(IGeometry<TCoordinate> g, Tolerance tolerance);

        public Boolean Intersects(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            if (!ExtentsInternal.Intersects(g.Extents))
            {
                return false;
            }

            // optimizations for rectangle arguments
            if (IsRectangle)
            {
                return RectangleIntersects<TCoordinate>.Intersects(this as IPolygon<TCoordinate>, g);
            }

            if (g.IsRectangle)
            {
                return RectangleIntersects<TCoordinate>.Intersects(g as IPolygon<TCoordinate>, this);
            }

            return Relate(g).IsIntersects();
        }

        public Boolean Intersects(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");
            throw new NotImplementedException();
        }

        public Boolean IsWithinDistance(IGeometry<TCoordinate> g, Double distance)
        {
            if (g == null) throw new ArgumentNullException("g");
            return IsWithinDistance(g, distance, Tolerance.Global);
        }

        public Boolean IsWithinDistance(IGeometry<TCoordinate> g, Double distance, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            Double envDist = ExtentsInternal.Distance(g.Extents);

            if (envDist > distance)
            {
                return false;
            }

            return DistanceOp<TCoordinate>.IsWithinDistance(this, g, distance, tolerance);
        }

        public Boolean Overlaps(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            return ExtentsInternal.Intersects(g.Extents) &&
                   Relate(g).IsOverlaps(Dimension, g.Dimension);
        }

        public Boolean Overlaps(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry<TCoordinate> g, IntersectionMatrix intersectionPattern)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean Relate(IGeometry<TCoordinate> g,
                              IntersectionMatrix intersectionPattern,
                              Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            return Relate(g).Equals(intersectionPattern);
        }

        public Boolean Relate(IGeometry<TCoordinate> g, String intersectionPattern)
        {
            if (g == null) throw new ArgumentNullException("g");

            return Relate(g).Matches(intersectionPattern);
        }

        public Boolean Relate(IGeometry<TCoordinate> g,
                              String intersectionPattern,
                              Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public IntersectionMatrix Relate(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            CheckNotGeometryCollection(this);
            CheckNotGeometryCollection(g);

            return RelateOp<TCoordinate>.Relate(this, g);
        }

        public Boolean Touches(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            // short-circuit test
            return ExtentsInternal.Intersects(g.Extents) &&
                   Relate(g).IsTouches(Dimension, g.Dimension);
        }

        public Boolean Touches(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        public Boolean Within(IGeometry<TCoordinate> g)
        {
            if (g == null) throw new ArgumentNullException("g");

            return g.Contains(this);
        }

        public Boolean Within(IGeometry<TCoordinate> g, Tolerance tolerance)
        {
            if (g == null) throw new ArgumentNullException("g");

            throw new NotImplementedException();
        }

        #endregion

        #region ISpatialOperator Members

        IGeometry ISpatialOperator.Boundary
        {
            get { return Boundary; }
        }

        IGeometry ISpatialOperator.Buffer(Double distance)
        {
            return Buffer(distance);
        }

        IGeometry ISpatialOperator.Buffer(Double distance, Int32 quadrantSegments)
        {
            return Buffer(distance, quadrantSegments);
        }

        IGeometry ISpatialOperator.Buffer(Double distance, BufferStyle endCapStyle)
        {
            return Buffer(distance, endCapStyle);
        }

        IGeometry ISpatialOperator.Buffer(Double distance,
                                          Int32 quadrantSegments,
                                          BufferStyle endCapStyle)
        {
            return Buffer(distance, quadrantSegments, endCapStyle);
        }

        IGeometry ISpatialOperator.ConvexHull()
        {
            return ConvexHull();
        }

        IGeometry ISpatialOperator.Difference(IGeometry other)
        {
            return Difference(convertIGeometry(other));
        }

        Double ISpatialOperator.Distance(IGeometry other)
        {
            return Distance(convertIGeometry(other));
        }

        IGeometry ISpatialOperator.Intersection(IGeometry other)
        {
            return Intersection(convertIGeometry(other));
        }

        IGeometry ISpatialOperator.SymmetricDifference(IGeometry other)
        {
            return SymmetricDifference(convertIGeometry(other));
        }

        IGeometry ISpatialOperator.Union(IGeometry other)
        {
            return Union(convertIGeometry(other));
        }

        #endregion

        #region ISpatialRelation Members

        Boolean ISpatialRelation.Contains(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Contains(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Contains(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Contains(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.CoveredBy(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return other.Covers(this);
        }

        Boolean ISpatialRelation.CoveredBy(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return other.Covers(this, tolerance);
        }

        Boolean ISpatialRelation.Covers(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Covers(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Covers(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Covers(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Crosses(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Crosses(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Crosses(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Crosses(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Disjoint(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Disjoint(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Disjoint(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Disjoint(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Equals(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Equals(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.EqualsExact(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return EqualsExact(convertIGeometry(other));
        }

        Boolean ISpatialRelation.EqualsExact(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return EqualsExact(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Intersects(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Intersects(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Intersects(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Intersects(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.IsWithinDistance(IGeometry other, Double distance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return IsWithinDistance(convertIGeometry(other), distance);
        }

        Boolean ISpatialRelation.IsWithinDistance(IGeometry other, Double distance, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return IsWithinDistance(convertIGeometry(other), distance, tolerance);
        }

        Boolean ISpatialRelation.Overlaps(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Overlaps(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Overlaps(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Overlaps(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Relate(IGeometry other, IntersectionMatrix intersectionPattern)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Relate(convertIGeometry(other), intersectionPattern);
        }

        Boolean ISpatialRelation.Relate(IGeometry other,
                                        IntersectionMatrix intersectionPattern,
                                        Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Relate(convertIGeometry(other), intersectionPattern, tolerance);
        }

        Boolean ISpatialRelation.Relate(IGeometry other, String intersectionPattern)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Relate(convertIGeometry(other), intersectionPattern);
        }

        Boolean ISpatialRelation.Relate(IGeometry other,
                                        String intersectionPattern,
                                        Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Relate(convertIGeometry(other), intersectionPattern, tolerance);
        }

        IntersectionMatrix ISpatialRelation.Relate(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Relate(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Touches(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Touches(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Touches(IGeometry other, Tolerance tolerance)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Touches(convertIGeometry(other), tolerance);
        }

        Boolean ISpatialRelation.Within(IGeometry other)
        {
            if (other == null) throw new ArgumentNullException("other");

            return Within(convertIGeometry(other));
        }

        Boolean ISpatialRelation.Within(IGeometry other, Tolerance tolerance)
        {
            return Within(convertIGeometry(other), tolerance);
        }

        #endregion

        #region IBoundable<IExtents<TCoordinate>> Members

        IExtents<TCoordinate> IBoundable<IExtents<TCoordinate>>.Bounds
        {
            get { return Extents; }
        }

        public Boolean Intersects(IExtents<TCoordinate> bounds)
        {
            return Extents.Intersects(bounds);
        }

        #endregion

        #region Explicit IGeometry Members
        ICoordinateSystem IGeometry.SpatialReference
        {
            get { return SpatialReference; }
        }

        #endregion

        #region Protected helper members
        protected static Boolean Equal(TCoordinate a, TCoordinate b, Tolerance tolerance)
        {
            return tolerance == Tolerance.Zero
                        ? a.Equals(b)
                        : tolerance.Equal(0, a.Distance(b));
        }

        /// <summary> 
        /// Returns the minimum and maximum x and y values in this 
        /// <see cref="Geometry{TCoordinate}"/>, or a null 
        /// <see cref="Extents{TCoordinate}"/> if this 
        /// <see cref="Geometry{TCoordinate}"/> is empty.
        /// </summary>
        /// <returns>    
        /// This <see cref="Geometry{TCoordinate}"/>s bounding box; 
        /// if the <see cref="Geometry{TCoordinate}"/>
        /// is empty, <c>Envelope.IsNull</c> will return <see langword="true"/>.
        /// </returns>
        protected Extents<TCoordinate> ExtentsInternal
        {
            get
            {
                if (_extents == null)
                {
                    _extents = ComputeExtentsInternal();
                }

                return _extents;
            }
            set
            {
                _extents = value;
            }
        }

        /// <summary>
        /// Returns whether the two <see cref="Geometry{TCoordinate}"/>s are equal, 
        /// from the point of view of the <see cref="EqualsExact"/> method. 
        /// Called by <c>EqualsExact</c>. In general, two 
        /// <see cref="Geometry{TCoordinate}"/> classes are considered to be
        /// "equivalent" only if they are the same class. 
        /// An exception is <c>LineString</c>, which is considered to be 
        /// equivalent to its subclasses.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Geometry{TCoordinate}"/> with which to compare 
        /// this <see cref="Geometry{TCoordinate}"/> for equality.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the classes of the two 
        /// <see cref="Geometry{TCoordinate}"/>s are considered 
        /// to be equal by the <see cref="EqualsExact"/> method.
        /// </returns>
        protected virtual Boolean IsEquivalentClass(IGeometry other)
        {
            return GetType() == other.GetType();
        }

        /// <summary>
        /// Throws an exception if <paramref name="g"/> 's type is 
        /// <see cref="IGeometryCollection{TCoordinate}" /> 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> to check.</param>
        /// <exception cref="ArgumentException">
        /// if <paramref name="g"/> is a <see cref="GeometryCollection{TCoordinate}" />, 
        /// but not one of its subclasses.
        /// </exception>
        protected static void CheckNotGeometryCollection(IGeometry g)
        {
            if (isGeometryCollection(g))
            {
                throw new ArgumentException("This method does not support " +
                                            "GeometryCollection arguments");
            }
        }

        /// <summary>
        /// Returns the minimum and maximum x and y values in this 
        /// <see cref="Geometry{TCoordinate}"/>, or a null <see cref="Extents{TCoordinate}"/> 
        /// if this <see cref="Geometry{TCoordinate}"/> is empty.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="Extents"/> or <see cref="ExtentsInternal"/>, this method 
        /// calculates the <see cref="Extents{TCoordinate}"/>
        /// each time it is called; <see cref="Extents"/> caches the result
        /// of this method.        
        /// </remarks>
        /// <returns>
        /// This <see cref="Geometry{TCoordinate}"/>s bounding box; 
        /// if the <see cref="Geometry{TCoordinate}"/>
        /// is empty, <see cref="Extents{TCoordinate}.IsEmpty"/> will return <see langword="true"/>.
        /// </returns>
        protected abstract Extents<TCoordinate> ComputeExtentsInternal();

        protected static IGeometryFactory<TCoordinate> ExtractGeometryFactory(IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                if (geometry != null && geometry.Factory != null)
                {
                    return geometry.Factory;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns whether this <see cref="Geometry{TCoordinate}"/> is greater than, equal to,
        /// or less than another <see cref="IGeometry{TCoordinate}"/> having the same class.
        /// </summary>
        /// <param name="other">
        /// A <see cref="Geometry{TCoordinate}"/> having the same class as this 
        /// <see cref="Geometry{TCoordinate}"/>.
        /// </param>
        /// <returns>
        /// A positive number, 0, or a negative number, depending on whether
        /// this object is greater than, equal to, or less than <c>o</c>, as
        /// defined in "Normal Form For Geometry" in the NTS Technical
        /// Specifications.
        /// </returns>
        protected internal abstract Int32 CompareToSameClass(IGeometry<TCoordinate> other);

        #endregion

        #region Private helper members
        private static IGeometry<TCoordinate> convertIGeometry(IGeometry other)
        {
            return GenericInterfaceConverter<TCoordinate>.Convert(other);
        }

        /// <summary>
        /// Returns <see langword="true"/> if <c>g</c>'s class is <see cref="GeometryCollection{TCoordinate}" />. 
        /// (its subclasses do not trigger an exception).
        /// </summary>
        /// <param name="g">The <see cref="Geometry{TCoordinate}"/> to check.</param>
        /// <exception cref="ArgumentException">
        /// If <c>g</c> is a <see cref="GeometryCollection{TCoordinate}" />, but not one of its subclasses.
        /// </exception>        
        private static Boolean isGeometryCollection(IGeometry g)
        {
            return g is IGeometryCollection &&
                !(g is IMultiPoint) &&
                !(g is IMultiCurve) &&
                !(g is IMultiSurface);
        }

        private static IPoint<TCoordinate> createPointFromInternalCoord(TCoordinate coord, IGeometry<TCoordinate> exemplar)
        {
            exemplar.PrecisionModel.MakePrecise(coord);
            return exemplar.Factory.CreatePoint(coord);
        }

        private static Int32 getClassSortIndex(IGeometry<TCoordinate> geometry)
        {
            for (Int32 i = 0; i < _sortedClasses.Length; i++)
            {
                Type interfaceType = Type.GetTypeFromHandle(_sortedClasses[i]);

                if (interfaceType.IsInstanceOfType(geometry))
                {
                    return i;
                }
            }

            Assert.ShouldNeverReachHere("Geometry type not supported: " + geometry.GetType());
            return -1;
        }

        private static Boolean compareGeometryCollections(IGeometry<TCoordinate> obj1, IGeometry<TCoordinate> obj2)
        {
            IGeometryCollection coll1 = obj1 as IGeometryCollection;
            IGeometryCollection coll2 = obj2 as IGeometryCollection;

            if (coll1 == null || coll2 == null)
            {
                return false;
            }

            // Short-circuit test
            if (coll1.Count != coll2.Count)
            {
                return false;
            }

            // Deep test
            for (Int32 i = 0; i < coll1.Count; i++)
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

        #endregion

        #region Deprecated

        /*
         * [codekaizen 2008-04-15]  I'm considering these harmful at the moment, given that
         *                          .Net provides ReferenceEquals.
         */
        //public static Boolean operator ==(Geometry<TCoordinate> left, IGeometry<TCoordinate> right)
        //{
        //    return Equals(left, right);
        //}

        //public static Boolean operator !=(Geometry<TCoordinate> left, IGeometry<TCoordinate> right)
        //{
        //    return !(left == right);
        //}

        /*
         * [codekaizen 2008-01-14]  replaced the following external notification methods
         *                          with an event on ICoordinateSequence.
         */
        ///// <summary>
        ///// Notifies this Geometry that its Coordinates have been changed by an external
        ///// party (using a CoordinateFilter, for example). The Geometry will flush
        ///// and/or update any information it has cached (such as its Envelope).
        ///// </summary>
        //public void GeometryChanged()
        //{
        //    Apply(new GeometryChangedFilter());
        //}

        ///// <summary> 
        ///// Notifies this Geometry that its Coordinates have been changed by an external
        ///// party. When GeometryChanged is called, this method will be called for
        ///// this Geometry and its component Geometries.
        ///// </summary>
        //public void GeometryChangedAction()
        //{
        //    ExtentsInternal = null;
        //}

        /*
         * [codekaizen 2008-01-14]  Removed this method due to the removal of 
         *                          IO implementation. Use specific external IO 
         *                          libraries to encode geometry instances.
         */

        ///// <summary>
        ///// Returns the feature representation as GML 2.1.1 XML document.
        ///// This XML document is based on <c>Geometry.xsd</c> schema.
        ///// NO features or XLink are implemented here!
        ///// </summary>        
        //public XmlReader ToGMLFeature()
        //{
        //    GMLWriter writer = new GMLWriter();
        //    return writer.Write(this);
        //}

        /*
         * [codekaizen 2008-01-14]  replaced the following visitor pattern methods
         *                          with enumeration / query pattern methods which 
         *                          accept a Func<T, TResult> method.
         */

        ///// <summary>
        ///// Performs an operation with or on this <see cref="Geometry{TCoordinate}"/>'s
        ///// coordinates. If you are using this method to modify the point, be sure
        ///// to call GeometryChanged() afterwards. Note that you cannot use this
        ///// method to
        ///// modify this Geometry if its underlying CoordinateSequence's Get method
        ///// returns a copy of the Coordinate, rather than the actual Coordinate stored
        ///// (if it even stores Coordinates at all).
        ///// </summary>
        ///// <param name="filter">The filter to apply to this <see cref="Geometry{TCoordinate}"/>'s coordinates</param>
        //public abstract void Apply(ICoordinateFilter<TCoordinate> filter);

        ///// <summary>
        ///// Performs an operation with or on this <see cref="Geometry{TCoordinate}"/> and its
        ///// subelement <see cref="Geometry{TCoordinate}"/>s (if any).
        ///// Only GeometryCollections and subclasses
        ///// have subelement Geometry's.
        ///// </summary>
        ///// <param name="filter">
        ///// The filter to apply to this <see cref="Geometry{TCoordinate}"/> (and
        ///// its children, if it is a <see cref="GeometryCollection{TCoordinate}" />).
        ///// </param>
        //public abstract void Apply(IGeometryFilter<TCoordinate> filter);

        ///// <summary>
        ///// Performs an operation with or on this Geometry and its
        ///// component Geometry's. Only GeometryCollections and
        ///// Polygons have component Geometriess; for Polygons they are the LinearRings
        ///// of the shell and holes.
        ///// </summary>
        ///// <param name="filter">The filter to apply to this <see cref="Geometry{TCoordinate}"/>.</param>
        //public abstract void Apply(IGeometryComponentFilter<TCoordinate> filter);

        ///// <summary>
        ///// Returns the first non-zero result of <c>CompareTo</c> encountered as
        ///// the two <c>Collection</c>s are iterated over. If, by the time one of
        ///// the iterations is complete, no non-zero result has been encountered,
        ///// returns 0 if the other iteration is also complete. If <c>b</c>
        ///// completes before <c>a</c>, a positive number is returned; if a
        ///// before b, a negative number.
        ///// </summary>
        ///// <param name="a">A <c>Collection</c> of <c>IComparable</c>s.</param>
        ///// <param name="b">A <c>Collection</c> of <c>IComparable</c>s.</param>
        ///// <returns>The first non-zero <c>compareTo</c> result, if any; otherwise, zero.</returns>
        //protected Int32 Compare(ArrayList a, ArrayList b)
        //{
        //    IEnumerator i = a.GetEnumerator();
        //    IEnumerator j = b.GetEnumerator();

        //    while (i.MoveNext() && j.MoveNext())
        //    {
        //        IComparable aElement = (IComparable)i.Current;
        //        IComparable bElement = (IComparable)j.Current;
        //        Int32 comparison = aElement.CompareTo(bElement);

        //        if (comparison != 0)
        //        {
        //            return comparison;
        //        }
        //    }

        //    if (i.MoveNext())
        //    {
        //        return 1;
        //    }

        //    if (j.MoveNext())
        //    {
        //        return -1;
        //    }

        //    return 0;
        //}

        /*
         * [codekaizen 2008-01-14] removed when replaced visitor patterns with
         *                         enumeration / query patterns
         */
        //private class GeometryChangedFilter : IGeometryComponentFilter<TCoordinate>
        //{
        //    public void Filter(IGeometry<TCoordinate> geom)
        //    {
        //        geom.GeometryChangedAction();
        //    }
        //}

        // ============= BEGIN ADDED BY MPAUL42: monoGIS team

        ///// <summary>
        ///// A predefined <see cref="GeometryFactory{TCoordinate}" /> 
        ///// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModelType.Fixed" />.
        ///// </summary>
        ///// <seealso cref="GeometryFactory{TCoordinate}.CreateFixedPrecision"/>
        //public static readonly IGeometryFactory<TCoordinate> DefaultFactory = GeometryFactory<TCoordinate>.Default;

        // ============= END ADDED BY MPAUL42: monoGIS team
        #endregion
    }
}