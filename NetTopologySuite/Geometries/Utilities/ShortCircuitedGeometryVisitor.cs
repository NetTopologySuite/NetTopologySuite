#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
#endif
namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A visitor to Geometry elements which can
    /// be short-circuited by a given condition.
    /// </summary>
    public abstract class ShortCircuitedGeometryVisitor
    {
        private bool _isDone;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void ApplyTo(IGeometry geom) 
        {
            for (int i = 0; i < geom.NumGeometries && ! _isDone; i++) 
            {
                IGeometry element = geom.GetGeometryN(i);
                if (!(element is IGeometryCollection)) 
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
        protected abstract void Visit(IGeometry element);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsDone();
    }
}
