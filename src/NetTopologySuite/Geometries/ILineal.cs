namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of <see cref="Dimension.Curve"/> 
    /// and have components which are <see cref="LineString"/>s.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="IPuntal"/>
    /// <seealso cref="IPolygonal"/>
    public interface ILineal
    {
        
    }

    /// <summary>
    /// Marker interface to identify all <c>Geometry</c> subclasses that have a <c>Dimension</c> of
    /// <see cref="Dimension.Curve"/> and consist of <b>only</b> <c>1</c> component.
    /// </summary>
    public interface ICurve : ILineal
    {
        /// <summary>
        /// Gets a value indicating that this <c>Curve</c> forms a ring
        /// </summary>
        bool IsRing { get; }

        /// <summary>
        /// Creates a flattened version of the (possibly) curved geometry.
        /// </summary>
        /// <returns>A flattened geometry</returns>
        LineString Flatten();
    }
}
