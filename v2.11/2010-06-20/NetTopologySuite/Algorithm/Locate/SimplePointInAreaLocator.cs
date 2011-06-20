using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm.Locate
{
    ///<summary>
    /// Simple Point in Area Locator
    ///</summary>
    public class SimplePointInAreaLocator<TCoordinate> : IPointOnGeometryLocator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IGeometry<TCoordinate> _geom;

        ///<summary>
        /// Constructs an instance of this class
        ///</summary>
        ///<param name="geom">an areal geometry</param>
        public SimplePointInAreaLocator(IGeometry<TCoordinate> geom)
        {
            _geom = geom;
        }

        #region IPointOnGeometryLocator<TCoordinate> Members

        public Locations Locate(TCoordinate p)
        {
            return Locate(p, _geom);
        }

        #endregion

        /// <summary>
        /// Determines the <see cref="Locations"/> of a point in an areal <see cref="IGeometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="p">the point to test</param>
        /// <param name="geom">the areal geometry to test</param>
        /// <returns>the Location of the point in the geometry</returns>
        public static Locations Locate(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom.IsEmpty)
                return Locations.Exterior;

            if (ContainsPoint(p, geom))
                return Locations.Interior;
            return Locations.Exterior;
        }

        private static Boolean ContainsPoint(TCoordinate p, IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygon<TCoordinate>)
            {
                return ContainsPointInPolygon(p, (IPolygon<TCoordinate>) geom);
            }
            if (geom is IGeometryCollection<TCoordinate>)
            {
                foreach (IGeometry<TCoordinate> tmpGeometry in ((IGeometryCollection<TCoordinate>) geom))
                {
                    if (ContainsPoint(p, tmpGeometry))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether a point lies in a Polygon.
        /// </summary>
        /// <param name="p">the point to test</param>
        /// <param name="poly">a polygon</param>
        /// <returns>true if the point lies inside the polygon</returns>
        public static Boolean ContainsPointInPolygon(TCoordinate p, IPolygon<TCoordinate> poly)
        {
            if (poly.IsEmpty)
                return false;

            if (!IsPointInRing(p, (ILinearRing<TCoordinate>) poly.ExteriorRing))
                return false;
            // now test if the point lies in or on the holes
            foreach (ILineString<TCoordinate> ring in poly.InteriorRings)
            {
                if (IsPointInRing(p, (ILinearRing<TCoordinate>) ring))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether a point lies in a LinearRing, using the ring envelope to short-circuit if possible.
        /// </summary>
        /// <param name="p">the point to test</param>
        /// <param name="ring">a linear ring</param>
        /// <returns>true if the point lies inside the ring</returns>
        private static Boolean IsPointInRing(TCoordinate p, ILinearRing<TCoordinate> ring)
        {
            // short-circuit if point is not in ring envelope
            if (!ring.Extents.Intersects(p))
                return false;
            return CGAlgorithms<TCoordinate>.IsPointInRing(p, ring.Coordinates);
        }
    }
}