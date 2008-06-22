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
    [Obsolete("The visitor pattern will be replaced by an enumeration / query pattern.")]
    public abstract class ShortCircuitedGeometryVisitor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private Boolean _isDone;

        public void ApplyTo(IGeometry<TCoordinate> geom)
        {
            // Short-circuit any more comparisons if the visitor has been set
            // to done.
            if (_isDone)
            {
                return;
            }

            IGeometryCollection<TCoordinate> collection = geom as IGeometryCollection<TCoordinate>;

            if (collection != null)
            {
                foreach (IGeometry<TCoordinate> component in collection)
                {
                    ApplyTo(component);
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