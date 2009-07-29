using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Prepared
{
    public interface IPreparedGeometry<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Gets the original <see cref="IGeometry{TCoordinate}"/> which has been prepared
        /// </summary>
        IGeometry<TCoordinate> Geometry { get; }

        /// <summary>
        /// Tests whether the base <see cref="IGeometry{TCoordinate}"/> contains a given geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns><value>True</value> if this geometry conatains given geometry</returns>
        /// <seealso cref="ISpatialRelation.Contains(GeoAPI.Geometries.IGeometry)"/>
        Boolean Contains(IGeometry<TCoordinate> geometry);

        /// <summary>
        /// Tests whether the base <see cref="IGeometry{TCoordinate}"/> contains a given geometry
        /// The <code>ContainsProperty</code> predicate has the following equivalent definition:
        /// <list type="Bullet">
        /// <item>Every Point of the other geometry is a point of this geometry's interior</item>
        /// <item>The DE-9IM Intersection Matrix for the two geometries matches <code>[T**FF*FF*]</code></item>
        /// </list>
        /// The advantage to using this predicate is that it can be computed efficiently, with no
        /// need to compute topology at individual points.
        /// An example use case for this predicate is computing the intersections
        /// of a set of geometries with a large polygonal geometry.  
        /// Since <code>intersection"</code> is a fairly slow operation, it can be more efficient
        /// to use <code>ContainsProperly</code> to filter out test geometries which lie
        /// wholly inside the area.  In these cases the intersection 
        /// known a priori to be simply the original test geometry. 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns><value>True</value> if this geometry property conatains given geometry</returns>
        /// <seealso cref="ISpatialRelation.Contains(GeoAPI.Geometries.IGeometry)"/>
        Boolean ContainsProperly(IGeometry<TCoordinate> geometry);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> is covered by a given geometry.
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry is covered by the given Geometry</returns>
        ///<seealso cref="ISpatialRelation{TCoordinate}.CoveredBy(GeoAPI.Geometries.IGeometry{TCoordinate})"
        ///</summary>
        Boolean CoveredBy(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> covers a given geometry.
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry covers the given Geometry</returns>
        ///<seealso cref="ISpatialRelation{TCoordinate}.Covers(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Covers(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> crosses a given geometry.
        ///<param name="geom"> the Geometry to test</param>
        ///<returns><value>True</value> if this Geometry crosses the given Geometry</returns>
        ///<seealso cref="ISpatialRelation{TCoordinate}.Crosses(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Crosses(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> is disjoint from a given geometry.
        ///This method supports {@link GeometryCollection}s as input
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry is disjoint from the given Geometry
        ///<seealso cref="ISpatialRelation{TCoordinate}.Disjoint(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Disjoint(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> intersects a given geometry.
        ///This method supports {@link GeometryCollection}s as input
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry intersects the given Geometry
        ///<seealso cref="ISpatialRelation{TCoordinate}.Intersects(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Intersects(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> overlaps a given geometry.
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry overlaps the given Geometry
        ///<seealso cref="ISpatialRelation{TCoordinate}.Overlaps(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Overlaps(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> touches a given geometry.
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry touches the given Geometry</returns>
        ///<seealso cref="ISpatialRelation{TCoordinate}.Touches(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Touches(IGeometry<TCoordinate> geom);

        ///<summary>
        ///Tests whether the base <see cref="IGeometry{TCoordinate}"/> is within a given geometry.
        ///<param name="geom">The Geometry to test</param>
        ///<returns><value>True</value> if this Geometry is within the given Geometry</returns> 
        ///<seealso cref="ISpatialRelation{TCoordinate}.Within(GeoAPI.Geometries.IGeometry{TCoordinate})"/>
        ///</summary>
        Boolean Within(IGeometry<TCoordinate> geom);
    
    }
}
