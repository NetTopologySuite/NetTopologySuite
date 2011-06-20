using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Tests whether a <c>Coordinate</c> lies inside
    /// a ring, using a linear-time algorithm.
    /// </summary>
    public class SimplePointInRing<TCoordinate> : IPointInRing<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IEnumerable<TCoordinate> _ring;

        public SimplePointInRing(ILinearRing<TCoordinate> ring)
        {
            _ring = ring.Coordinates;
        }

        #region IPointInRing<TCoordinate> Members

        public Boolean IsInside(TCoordinate coordinate)
        {
            return CGAlgorithms<TCoordinate>.IsPointInRing(coordinate, _ring);
        }

        #endregion
    }
}