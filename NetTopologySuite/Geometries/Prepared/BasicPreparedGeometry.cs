using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A base class for <see cref="IPreparedGeometry"/> subclasses.
    ///</summary>
    /// <remarks>
    /// <para>Contains default implementations for methods, which simply delegate to the equivalent <see cref="IGeometry"/> methods.</para>
    /// <para>This class may be used as a "no-op" class for Geometry types which do not have a corresponding <see cref="IPreparedGeometry"/> implementation.</para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class BasicPreparedGeometry : IPreparedGeometry
    {
        private readonly IGeometry _baseGeom;
        private readonly List<Coordinate> _representativePts;  // List<Coordinate>

        public BasicPreparedGeometry(IGeometry geom)
        {
            _baseGeom = geom;
            _representativePts = ComponentCoordinateExtracter.GetCoordinates(geom);
        }

        public IGeometry Geometry => _baseGeom;

        ///<summary>
        /// Gets the list of representative points for this geometry.
        /// One vertex is included for every component of the geometry
        /// (i.e. including one for every ring of polygonal geometries).
        /// <para/>
        /// Do not modify the returned list!
        /// </summary>
        public IList<Coordinate> RepresentativePoints => new ReadOnlyCollection<Coordinate>(_representativePts);

        ///<summary>
        /// Tests whether any representative of the target geometry intersects the test geometry.
        /// This is useful in A/A, A/L, A/P, L/P, and P/P cases.
        ///</summary>
        /// <param name="testGeom">The test geometry</param>
        /// <returns>true if any component intersects the areal test geometry</returns>
        public bool IsAnyTargetComponentInTest(IGeometry testGeom)
        {
            var locator = new PointLocator();
            foreach (var representativePoint in RepresentativePoints)
            {
                if (locator.Intersects(representativePoint, testGeom))
                    return true;
            }
            return false;
        }

        ///<summary>
        /// Determines whether a Geometry g interacts with this geometry by testing the geometry envelopes.
        ///</summary>
        /// <param name="g">A geometry</param>
        /// <returns>true if the envelopes intersect</returns>
        protected bool EnvelopesIntersect(IGeometry g)
        {
            if (!_baseGeom.EnvelopeInternal.Intersects(g.EnvelopeInternal))
                return false;
            return true;
        }

        ///<summary>
        /// Determines whether the envelope of this geometry covers the Geometry g.
        ///</summary>
        /// <param name="g">A geometry</param>
        /// <returns>true if g is contained in this envelope</returns>
        protected bool EnvelopeCovers(IGeometry g)
        {
            if (!_baseGeom.EnvelopeInternal.Covers(g.EnvelopeInternal))
                return false;
            return true;
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> contains a given geometry.
        ///</summary>
        /// <param name="g">The Geometry to test</param>
        /// <returns>true if this Geometry contains the given Geometry</returns>
        /// <see cref="IGeometry.Contains(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public virtual bool Contains(IGeometry g)
        {
            return _baseGeom.Contains(g);
        }

        /// <summary>
        /// Tests whether the base <see cref="IGeometry"/> properly contains a given geometry.
        /// <para/>
        /// The <c>ContainsProperly</c> predicate has the following equivalent definitions:
        /// <list>
        /// <item>Every point of the other geometry is a point of this geometry's interior.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches <c>>[T**FF*FF*]</c></item>
        /// </list>
        /// In other words, if the test geometry has any interaction with the boundary of the target
        /// geometry the result of <c>ContainsProperly</c> is <c>false</c>.
        /// This is different semantics to the {@link Geometry#contains} predicate,
        /// in which test geometries can intersect the target's boundary and still be contained.
        /// <para/>
        /// The advantage of using this predicate is that it can be computed
        /// efficiently, since it avoids the need to compute the full topological relationship
        /// of the input boundaries in cases where they intersect.
        /// <para/>
        /// An example use case is computing the intersections
        /// of a set of geometries with a large polygonal geometry.
        /// Since <i>intersection</i> is a fairly slow operation, it can be more efficient
        /// to use <see cref="ContainsProperly" /> to filter out test geometries which lie
        /// wholly inside the area.  In these cases the intersection is
        /// known <c>a priori</c> to be simply the original test geometry.
        /// </summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry properly contains the given geometry</returns>
        /// <see cref="IPreparedGeometry.ContainsProperly(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        /// <seealso cref="Contains"/>
        public virtual bool ContainsProperly(IGeometry g)
        {
            // since raw relate is used, provide some optimizations

            // short-circuit test
            if (!_baseGeom.EnvelopeInternal.Contains(g.EnvelopeInternal))
                return false;

            // otherwise, compute using relate mask
            return _baseGeom.Relate(g, "T**FF*FF*");
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is covered by a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry is covered by the given geometry</returns>
        /// <see cref="IGeometry.CoveredBy(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool CoveredBy(IGeometry g)
        {
            return _baseGeom.CoveredBy(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> covers a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry covers the given geometry</returns>
        /// <see cref="IGeometry.Covers(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public virtual bool Covers(IGeometry g)
        {
            return _baseGeom.Covers(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> crosses a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry crosses the given geometry</returns>
        /// <see cref="IGeometry.Crosses(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool Crosses(IGeometry g)
        {
            return _baseGeom.Crosses(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is disjoint from given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry is disjoint from the given geometry</returns>
        /// <see cref="IGeometry.Disjoint(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool Disjoint(IGeometry g)
        {
            return !Intersects(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> intersects a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry intersects the given geometry</returns>
        /// <see cref="IGeometry.Intersects(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public virtual bool Intersects(IGeometry g)
        {
            return _baseGeom.Intersects(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> overlaps a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry overlaps the given geometry</returns>
        /// <see cref="IGeometry.Overlaps(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool Overlaps(IGeometry g)
        {
            return _baseGeom.Overlaps(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> touches a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry touches the given geometry</returns>
        /// <see cref="IGeometry.Touches(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool Touches(IGeometry g)
        {
            return _baseGeom.Touches(g);
        }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is within a given geometry.
        ///</summary>
        /// <param name="g">The geometry to test</param>
        /// <returns>true if this geometry is within the given geometry</returns>
        /// <see cref="IGeometry.Within(IGeometry)"/>
        /// <remarks>Default implementation.</remarks>
        public bool Within(IGeometry g)
        {
            return _baseGeom.Within(g);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return _baseGeom.ToString();
        }
    }
}
