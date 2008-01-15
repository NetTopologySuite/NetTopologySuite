using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// A visitor to Geometry elements which can
    /// be short-circuited by a given condition.
    /// </summary>
    public abstract class ShortCircuitedGeometryVisitor<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private Boolean _isDone = false;

        public void ApplyTo(IGeometry<TCoordinate> geom)
        {
            // Short-circuit any more comparisons if the visitor has been set
            // to done.
            if (_isDone)
            {
                return;
            }

            if(geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> collection = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in collection)
                {
                    ApplyTo(geometry);
                }
            }
            else
            {
                Visit(geom);

                if (IsDone())
                {
                    _isDone = true;
                }
            }
        }

        protected abstract void Visit(IGeometry<TCoordinate> element);

        protected abstract Boolean IsDone();
    }
}