namespace GeoAPI.Geometries.Prepared
{

    ///<summary>
    /// An interface for classes which prepare <see cref="IGeometry"/>s 
    /// in order to optimize the performance of repeated calls to specific geometric operations.
    ///</summary>
    /// <remarks>
    /// <para>
    /// A given implementation may provide optimized implementations 
    /// for only some of the specified methods, and delegate the remaining
    /// methods to the original <see cref="IGeometry"/> operations.
    /// </para>
    /// <para>
    /// An implementation may also only optimize certain situations, and delegate others.
    /// See the implementing classes for documentation about which methods and situations 
    /// they optimize.</para>
    /// <para>
    /// Subclasses are intended to be thread-safe, to allow <c>IPreparedGeometry</c>
    /// to be used in a multi-threaded context 
    /// (which allows extracting maximum benefit from the prepared state).
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public interface IPreparedGeometry
    {
        ///<summary>
        /// Gets the original <see cref="IGeometry"/> which has been prepared.
        ///</summary>
        IGeometry Geometry { get; }

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> contains a given geometry.
        ///</summary>
        /// <param name="geom">The Geometry to test</param>
        /// <returns>true if this Geometry contains the given Geometry</returns>
        /// <see cref="IGeometry.Contains(IGeometry)"/>
        bool Contains(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> contains a given geometry.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The <c>ContainsProperly</c> predicate has the following equivalent definitions:
        /// <list>
        /// <item>Every point of the other geometry is a point of this geometry's interior.</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches <c>>[T**FF*FF*]</c></item>
        /// </list>
        /// The advantage to using this predicate is that it can be computed
        /// efficiently, with no need to compute topology at individual points.
        /// </para>
        /// <para>
        /// An example use case for this predicate is computing the intersections
        /// of a set of geometries with a large polygonal geometry.  
        /// Since <tt>intersection</tt> is a fairly slow operation, it can be more efficient
        /// to use <tt>containsProperly</tt> to filter out test geometries which lie
        /// wholly inside the area.  In these cases the intersection 
        /// known a priori to be simply the original test geometry. 
        /// </para>
        /// </remarks>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry properly contains the given geometry</returns>
        //// <see cref="IGeometry.ContainsProperly(IGeometry)"/>
        bool ContainsProperly(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is covered by a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is covered by the given geometry</returns>
        /// <see cref="IGeometry.CoveredBy(IGeometry)"/>
        bool CoveredBy(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> covers a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry covers the given geometry</returns>
        /// <see cref="IGeometry.Covers(IGeometry)"/>
        bool Covers(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> crosses a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry crosses the given geometry</returns>
        /// <see cref="IGeometry.Crosses(IGeometry)"/>
        bool Crosses(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is disjoint from given geometry.
        ///</summary>
        /// <remarks>
       	/// This method supports <see cref="IGeometryCollection"/>s as input
       	/// </remarks>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is disjoint from the given geometry</returns>
        /// <see cref="IGeometry.Disjoint(IGeometry)"/>
        bool Disjoint(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> intersects a given geometry.
        ///</summary>
        /// <remarks>
        /// This method supports <see cref="IGeometryCollection"/>s as input
        /// </remarks>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry intersects the given geometry</returns>
        /// <see cref="IGeometry.Intersects(IGeometry)"/>
        bool Intersects(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> overlaps a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry overlaps the given geometry</returns>
        /// <see cref="IGeometry.Overlaps(IGeometry)"/>
        bool Overlaps(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> touches a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry touches the given geometry</returns>
        /// <see cref="IGeometry.Touches(IGeometry)"/>
        bool Touches(IGeometry geom);

        ///<summary>
        /// Tests whether the base <see cref="IGeometry"/> is within a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is within the given geometry</returns>
        /// <see cref="IGeometry.Within(IGeometry)"/>
        bool Within(IGeometry geom);

    }
}