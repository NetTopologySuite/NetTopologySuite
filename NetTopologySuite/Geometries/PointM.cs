using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Geometries
{
    public class PointM<TCoordinate> : Point<TCoordinate>, IPoint2DM, IPoint3DM
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly Double _measure = Double.NaN;

        /// <summary>
        /// Constructs a <see cref="Point{TCoordinate}"/> with the given coordinate.
        /// </summary>
        /// <param name="coordinate">
        /// Contains the single coordinate on which to base this <see cref="Point{TCoordinate}"/>,
        /// or <see langword="null" /> to create the empty point.
        /// </param>
        public PointM(TCoordinate coordinate, Double measure, IGeometryFactory<TCoordinate> factory)
            : base(coordinate, factory)
        {
            _measure = measure;
        }

        #region IPoint2DM Members

        public Double M
        {
            get { return _measure; }
        }

        #endregion
    }
}