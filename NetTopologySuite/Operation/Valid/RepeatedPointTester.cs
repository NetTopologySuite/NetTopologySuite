using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary> 
    /// Implements the appropriate checks for repeated points
    /// (consecutive identical coordinates) as defined in the
    /// NTS spec.
    /// </summary>
    public class RepeatedPointTester<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        // save the repeated coord found (if any)
        private TCoordinate _repeatedCoord;

        public TCoordinate Coordinate
        {
            get { return _repeatedCoord; }
        }

        public Boolean HasRepeatedPoint(IGeometry<TCoordinate> g)
        {
            if (g.IsEmpty)
            {
                return false;
            }

            if (g is IPoint<TCoordinate>)
            {
                return false;
            }
            else if (g is IMultiPoint<TCoordinate>)
            {
                return false;
            }
            else if (g is ILineString<TCoordinate>)
            {
                // LineString also handles LinearRings
                return HasRepeatedPoint(g.Coordinates);
            }
            else if (g is IPolygon<TCoordinate>)
            {
                return HasRepeatedPoint(g as IPolygon<TCoordinate>);
            }
            else if (g is IGeometryCollection<TCoordinate>)
            {
                return HasRepeatedPoint(g as IGeometryCollection<TCoordinate>);
            }
            else
            {
                throw new NotSupportedException(g.GetType().FullName);
            }
        }

        public Boolean HasRepeatedPoint(IEnumerable<TCoordinate> coord)
        {
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(coord))
            {
                if(pair.First.Equals(pair.Second))
                {
                    _repeatedCoord = pair.First;
                    return true;
                }
            }

            return false;
        }

        private Boolean HasRepeatedPoint(IPolygon<TCoordinate> p)
        {
            if (HasRepeatedPoint(p.ExteriorRing.Coordinates))
            {
                return true;
            }

            foreach (ILineString<TCoordinate> ring in p.InteriorRings)
            {
                if (HasRepeatedPoint(ring.Coordinates))
                {
                    return true;
                }  
            }

            return false;
        }

        private Boolean HasRepeatedPoint(IGeometryCollection<TCoordinate> gc)
        {
            foreach (IGeometry<TCoordinate> geometry in gc)
            {
                if (HasRepeatedPoint(geometry))
                {
                    return true;
                }
            }

            return false;
        }
    }
}