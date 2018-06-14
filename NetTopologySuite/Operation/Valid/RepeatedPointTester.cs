using System;
using GeoAPI.Geometries;

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
        private Coordinate repeatedCoord;

        /// <summary>
        ///
        /// </summary>
        public Coordinate Coordinate => repeatedCoord;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public bool HasRepeatedPoint(IGeometry g)
        {
            if (g.IsEmpty)  return false;
            if (g is IPoint) return false;
            else if (g is IMultiPoint) return false;
            // LineString also handles LinearRings
            else if (g is ILineString)
                return HasRepeatedPoint((g).Coordinates);
            else if (g is IPolygon)
                return HasRepeatedPoint((IPolygon) g);
            else if (g is IGeometryCollection)
                return HasRepeatedPoint((IGeometryCollection) g);
            else  throw new NotSupportedException(g.GetType().FullName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool HasRepeatedPoint(Coordinate[] coord)
        {
            for (int i = 1; i < coord.Length; i++)
            {
                if (coord[i - 1].Equals(coord[i]))
                {
                    repeatedCoord = coord[i];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool HasRepeatedPoint(IPolygon p)
        {
            if (HasRepeatedPoint(p.ExteriorRing.Coordinates))
                return true;
            for (int i = 0; i < p.NumInteriorRings; i++)
                if (HasRepeatedPoint(p.GetInteriorRingN(i).Coordinates))
                    return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gc"></param>
        /// <returns></returns>
        private bool HasRepeatedPoint(IGeometryCollection gc)
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
