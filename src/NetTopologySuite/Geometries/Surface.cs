using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of <see cref="Dimension.Surface"/>
    /// and only have <b>one</b> component.
    /// </summary>
    public interface ISurface : IPolygonal { }

    /// <summary>
    /// Abstract base class for geometries that have a <c>Dimension</c> of <see cref="Geometries.Dimension.Surface"/>
    /// and only have <b>one</b> component.
    /// </summary>
    [Serializable]
    public abstract class Surface<T> : Geometry, ISurface where T:Curve
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="factory"></param>
        protected Surface(GeometryFactory factory)
            :base(factory)
        {}

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
        public sealed override Dimension Dimension => Dimension.Surface;

        /// <summary>
        /// Returns the dimension of this <c>Geometry</c>s inherent boundary.
        /// </summary>
        /// <returns>
        /// The dimension of the boundary of the class implementing this
        /// interface, whether or not this object is the empty point. Returns
        /// <c>Dimension.False</c> if the boundary is the empty point.
        /// </returns>
        public sealed override Dimension BoundaryDimension => Dimension.Curve;

        /// <summary>
        /// Gets a value indicating the exterior ring of the surface
        /// </summary>
        public abstract T ExteriorRing { get; }

        /// <summary>
        /// Gets a value indicating the number of interior rings inside the polygon
        /// </summary>
        public abstract int NumInteriorRings { get; }

        /// <summary>
        /// Gets the interior ring at <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the requested ring</param>
        /// <returns>An interior ring</returns>
        public abstract T GetInteriorRingN(int index);
    }
}
