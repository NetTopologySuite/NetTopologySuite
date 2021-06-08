namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of <see cref="Dimension.Surface"/>
    /// and have components that are <see cref="Polygon"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="IPuntal"/>
    /// <seealso cref="ILineal"/>
    public interface IPolygonal
    {
    }

    /// <summary>
    /// Interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of <see cref="Dimension.Surface"/>
    /// and only have <b>one</b> component.
    /// </summary>
    public interface ISurface : IPolygonal { }

    /// <summary>
    /// Interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of <see cref="Dimension.Surface"/>
    /// and only have <b>one</b> component.
    /// </summary>
    public interface ISurface<out T> : ISurface where T: Geometry
    {
        /// <summary>
        /// Gets a value indicating the exterior ring of the surface
        /// </summary>
        T ExteriorRing { get; }

        /// <summary>
        /// Gets a value indicating the number of interior rings inside the polygon
        /// </summary>
        int NumInteriorRings { get; }

        /// <summary>
        /// Gets the interior ring at <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index of the requested ring</param>
        /// <returns>An interior ring</returns>
        T GetInteriorRingN(int index);
    }
}
