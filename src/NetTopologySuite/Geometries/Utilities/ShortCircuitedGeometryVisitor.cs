namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A visitor to <see cref="Geometry"/> elements which  components, which
    /// allows short-circuiting when a defined condition holds.
    /// </summary>
    public abstract class ShortCircuitedGeometryVisitor
    {
        private bool _isDone;

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        public void ApplyTo(Geometry geom)
        {
            for (int i = 0; i < geom.NumGeometries && ! _isDone; i++)
            {
                var element = geom.GetGeometryN(i);
                if (!(element is GeometryCollection))
                {
                    Visit(element);
                    if (IsDone())
                    {
                        _isDone = true;
                        return;
                    }
                }
                else ApplyTo(element);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        protected abstract void Visit(Geometry element);

        /// <summary>
        /// Reports whether visiting components can be terminated.
        /// Once this method returns <see langword="true"/>, it must
        /// continue to return <see langword="true"/> on every subsequent call.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if visiting can be terminated.
        /// </returns>
        protected abstract bool IsDone();
    }
}
