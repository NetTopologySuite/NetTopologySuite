namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Interface describing objects that can perform an intersects predicate with <typeparamref name="T"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of the component that can intersect</typeparam>
    public interface IIntersectable<in T>
    {
        /// <summary>
        /// Predicate function to test if <paramref name="other"/> intersects with this object.
        /// </summary>
        /// <param name="other">The object to test</param>
        /// <returns><c>true</c> if this objects intersects with <paramref name="other"/></returns>
        bool Intersects(T other);
    }
}
