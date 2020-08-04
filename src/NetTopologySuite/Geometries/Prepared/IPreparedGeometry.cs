namespace NetTopologySuite.Geometries.Prepared
{

    ///<summary>
    /// An interface for classes which prepare <see cref="Geometry"/>s 
    /// in order to optimize the performance of repeated calls to specific geometric operations.
    ///</summary>
    /// <remarks>
    /// <para>
    /// A given implementation may provide optimized implementations 
    /// for only some of the specified methods, and delegate the remaining
    /// methods to the original <see cref="Geometry"/> operations.
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
        /// Gets the original <see cref="Geometry"/> which has been prepared.
        ///</summary>
        Geometry Geometry { get; }

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> contains a given geometry.
        ///</summary>
        /// <param name="geom">The Geometry to test</param>
        /// <returns>true if this Geometry contains the given Geometry</returns>
        /// <see cref="Geometry.Contains(Geometry)"/>
        bool Contains(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> contains a given geometry.
        ///</summary>
        /// <remarks>
        /// <para>
        /// The <c>ContainsProperly</c> predicate has the following equivalent definitions:
        /// <list type="bullet">
        /// <item><description>Every point of the other geometry is a point of this geometry's interior.</description></item>
        /// <item><description>The DE-9IM Intersection Matrix for the two geometries matches <c>>[T**FF*FF*]</c></description></item>
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
        //// <see cref="Geometry.ContainsProperly(Geometry)"/>
        bool ContainsProperly(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> is covered by a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is covered by the given geometry</returns>
        /// <see cref="Geometry.CoveredBy(Geometry)"/>
        bool CoveredBy(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> covers a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry covers the given geometry</returns>
        /// <see cref="Geometry.Covers(Geometry)"/>
        bool Covers(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> crosses a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry crosses the given geometry</returns>
        /// <see cref="Geometry.Crosses(Geometry)"/>
        bool Crosses(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> is disjoint from given geometry.
        ///</summary>
        /// <remarks>
       	/// This method supports <see cref="GeometryCollection"/>s as input
       	/// </remarks>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is disjoint from the given geometry</returns>
        /// <see cref="Geometry.Disjoint(Geometry)"/>
        bool Disjoint(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> intersects a given geometry.
        ///</summary>
        /// <remarks>
        /// This method supports <see cref="GeometryCollection"/>s as input
        /// </remarks>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry intersects the given geometry</returns>
        /// <see cref="Geometry.Intersects(Geometry)"/>
        bool Intersects(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> overlaps a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry overlaps the given geometry</returns>
        /// <see cref="Geometry.Overlaps(Geometry)"/>
        bool Overlaps(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> touches a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry touches the given geometry</returns>
        /// <see cref="Geometry.Touches(Geometry)"/>
        bool Touches(Geometry geom);

        ///<summary>
        /// Tests whether the base <see cref="Geometry"/> is within a given geometry.
        ///</summary>
        /// <param name="geom">The geometry to test</param>
        /// <returns>true if this geometry is within the given geometry</returns>
        /// <see cref="Geometry.Within(Geometry)"/>
        bool Within(Geometry geom);

    }
}
