#nullable disable
using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.Lab.Clean
{
    public class SmallHoleRemover
    {
        private class IsSmall : HoleRemover.Predicate
        {
            private readonly double _area;

            public IsSmall(double area)
            {
                _area = area;
            }

            public bool Value(Geometry geom)
            {
                double holeArea = Math.Abs(Area.OfRingSigned(geom.Coordinates));
                return holeArea <= _area;
            }
        }

        /// <summary>
        /// Removes small holes from the polygons in a geometry.
        /// </summary>
        /// <param name="geom">The geometry to clean.</param>
        /// <param name="areaTolerance">The geometry with invalid holes removed.</param>
        public static Geometry Clean(Geometry geom, double areaTolerance)
        {
            var remover = new HoleRemover(geom, new IsSmall(areaTolerance));
            return remover.GetResult();
        }
    }
}
