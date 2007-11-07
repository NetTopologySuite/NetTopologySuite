using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A visitor to Geometry elements which can
    /// be short-circuited by a given condition.
    /// </summary>
    public abstract class ShortCircuitedGeometryVisitor
    {
        private Boolean isDone = false;

        public ShortCircuitedGeometryVisitor() {}

        public void ApplyTo(IGeometry geom)
        {
            for (Int32 i = 0; i < geom.NumGeometries && ! isDone; i++)
            {
                IGeometry element = geom.GetGeometryN(i);

                if (!(element is IGeometryCollection))
                {
                    Visit(element);

                    if (IsDone())
                    {
                        isDone = true;
                        return;
                    }
                }
                else
                {
                    ApplyTo(element);
                }
            }
        }

        protected abstract void Visit(IGeometry element);

        protected abstract Boolean IsDone();
    }
}