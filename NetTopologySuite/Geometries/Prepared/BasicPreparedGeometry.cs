using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    ///A base class for <see cref="IPreparedGeometry{TCoordinate}"/> subclasses.
    ///Contains default implementations for methods, which simply delegate
    ///to the equivalent <see cref="IGeometry{TCoordinate}"/> methods.
    ///This class may be used as a "no-op" class for Geometry types
    ///which do not have a corresponding <see cref="IPreparedGeometry{TCoordinate}"/> implementation.
    ///
    ///@author Martin Davis
    ///</summary>
    public class BasicPreparedGeometry<TCoordinate> : IPreparedGeometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _baseGeom;
        private readonly ICoordinateSequence<TCoordinate> _representativePts;  // List<Coordinate>

        /// <summary>
        /// constructs an instance of this class
        /// </summary>
        /// <param name="geom"> geometry to be prepared</param>
        public BasicPreparedGeometry(IGeometry<TCoordinate> geom)
        {
            _baseGeom = geom;
            _representativePts = geom.Coordinates;
        }

        ///<summary>
        ///Gets the list of representative points for this geometry.
        ///One vertex is included for every component of the geometry
        ///(i.e. including one for every ring of polygonal geometries) 
        ///</summary>
        public ICoordinateSequence<TCoordinate> RepresentativePoints
        {
            get { return _representativePts; }
        }

        #region IPreparedGeometry<TCoordinate> Members

        public IGeometry<TCoordinate> Geometry
        {
            get { return _baseGeom; }
        }

        ///<summary>
        ///Determines whether the envelope of 
        ///this geometry covers the Geometry g.
        ///</summary>
        ///<param name="g"> a Geometry</param>
        ///<returns><value>True</value> if g is contained in this envelopes</returns>
        protected Boolean EnvelopeCovers(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Extents.Contains(g.Extents);
                //_baseGeom.Extents.Envelope.Covers(g.Envelope);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Contains(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Contains(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean ContainsProperly(IGeometry<TCoordinate> g)
        {
            // since raw relate is used, provide some optimizations

            // short-circuit test
            if (!_baseGeom.Envelope.Contains(g.Envelope))
                return false;

            // otherwise, compute using relate mask
            return _baseGeom.Relate(g, "T**FF*FF*");
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean CoveredBy(IGeometry<TCoordinate> g)
        {
            return _baseGeom.CoveredBy(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Covers(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Covers(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Crosses(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Crosses(g);
        }

        ///<summary>
        ///Standard implementation for all geometries.
        ///Supports {@link GeometryCollection}s as input.
        ///</summary>
        public virtual Boolean Disjoint(IGeometry<TCoordinate> g)
        {
            return !Intersects(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Intersects(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Intersects(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Overlaps(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Overlaps(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Touches(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Touches(g);
        }

        ///<summary>
        ///Default implementation.
        ///</summary>
        public virtual Boolean Within(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Within(g);
        }

        #endregion

        ///<summary>
        ///Tests whether any representative of the target geometry 
        ///intersects the test geometry.
        ///This is useful in A/A, A/L, A/P, L/P, and P/P cases.
        ///</summary>
        ///<param name="testGeom"> The test geometry</param>
        ///<returns><value>True</value> if any component intersects the areal test geometry</returns>
        public Boolean IsAnyTargetComponentInTest(IGeometry<TCoordinate> testGeom)
        {
            PointLocator<TCoordinate> locator = new PointLocator<TCoordinate>();
            foreach (TCoordinate p in _representativePts)
            {
                if (locator.Intersects(p, testGeom))
                    return true;
            }
            return false;
        }

        ///<summary>
        ///Determines whether a Geometry g interacts with 
        ///this geometry by testing the geometry envelopes.
        ///</summary>
        ///<param name="g"> a Geometry</param>
        ///<returns><value>True</value> if the envelopes intersect</returns>
        protected Boolean EnvelopesIntersect(IGeometry<TCoordinate> g)
        {
            return _baseGeom.Extents.Intersects(g.Extents);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>WKT representation of base geometry</returns>
        public String ToString()
        {
            return _baseGeom.ToString();
        }
    }
}