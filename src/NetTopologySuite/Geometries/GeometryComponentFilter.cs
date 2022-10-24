namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Delegate function declaration to handle filter operation
    /// </summary>
    /// <param name="geom">The geometry to filter</param>
    public delegate void FilterMethod(Geometry geom);

    /// <summary>
    /// An <see cref="IGeometryComponentFilter"/> implementation that applies filtering with the provided <see cref="FilterMethod"/>
    /// </summary>
    public class GeometryComponentFilter : IGeometryComponentFilter
    {
        private readonly FilterMethod _do;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="filterMethod">The filter method to be used</param>
        public GeometryComponentFilter(FilterMethod filterMethod)
        {
            //Assert.IsTrue(filterMethod != null);
            _do = filterMethod;
        }

        public void Filter(Geometry geom)
        {
            _do(geom);
        }
    }
}
