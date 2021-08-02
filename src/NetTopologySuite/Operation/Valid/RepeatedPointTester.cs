using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Implements the appropriate checks for repeated points
    /// (consecutive identical coordinates) as defined in the
    /// NTS spec.
    /// </summary>
    public class RepeatedPointTester
    {

        // save the repeated coord found (if any)
        private Coordinate _repeatedCoord;

        /// <summary>
        /// Gets a value indicating the location of the repeated point
        /// </summary>
        public Coordinate Coordinate => _repeatedCoord;

        /// <summary>
        /// Checks if a geometry has a repeated point
        /// </summary>
        /// <param name="g">The geometry to test</param>
        /// <returns><c>true</c> if the geometry has a repeated point, otherwise <c>false</c></returns>
        public bool HasRepeatedPoint(Geometry g)
        {
            if (g.IsEmpty)
                return false;

            switch (g)
            {
                case Point _:
                case MultiPoint _:
                    return false;
                // LineString also handles LinearRings
                case LineString ls:
                    return HasRepeatedPoint(ls.CoordinateSequence);
                case Polygon pg:
                    return HasRepeatedPoint(pg);
                case GeometryCollection gc:
                    return HasRepeatedPoint(gc);
                default:
                    throw new NotSupportedException(g.GetType().FullName);
            }
        }

        /// <summary>
        /// Checks if an array of <c>Coordinate</c>s has a repeated point
        /// </summary>
        /// <param name="coord">An array of coordinates</param>
        /// <returns><c>true</c> if <paramref name="coord"/> has a repeated point, otherwise <c>false</c></returns>
        public bool HasRepeatedPoint(Coordinate[] coord)
        {
            for (int i = 1; i < coord.Length; i++)
            {
                if (coord[i - 1].Equals(coord[i]))
                {
                    _repeatedCoord = coord[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if an array of <c>Coordinate</c>s has a repeated point
        /// </summary>
        /// <param name="sequence">A coordinate sequence</param>
        /// <returns><c>true</c> if <paramref name="sequence"/> has a repeated point, otherwise <c>false</c></returns>
        public bool HasRepeatedPoint(CoordinateSequence sequence)
        {
            if (sequence.Count < 2)
                return false;

            var last = sequence.GetCoordinate(0);
            for (int i = 1; i < sequence.Count; i++)
            {
                var curr = sequence.GetCoordinate(i);
                if (curr.Equals(last))
                {
                    _repeatedCoord = curr.Copy();
                    return true;
                }

                last = curr;
            }
            return false;
        }

        private bool HasRepeatedPoint(Polygon p)
        {
            if (HasRepeatedPoint(p.ExteriorRing.CoordinateSequence))
                return true;
            for (int i = 0; i < p.NumInteriorRings; i++)
                if (HasRepeatedPoint(p.GetInteriorRingN(i).CoordinateSequence))
                    return true;
            return false;
        }

        private bool HasRepeatedPoint(GeometryCollection gc)
        {
            for (int i = 0; i < gc.NumGeometries; i++)
            {
                var g = gc.GetGeometryN(i);
                if (HasRepeatedPoint(g))
                    return true;
            }
            return false;
        }
    }
}
