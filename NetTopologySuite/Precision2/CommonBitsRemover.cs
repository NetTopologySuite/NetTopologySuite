using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary>
    /// Allow computing and removing common significand bits from one or more 
    /// <see cref="IGeometry{TCoordinate}"/> instances.
    /// </summary>
    public class CommonBitsRemover<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private TCoordinate _commonCoordinate;
        private readonly CommonCoordinateFilter<TCoordinate> _filter = new CommonCoordinateFilter<TCoordinate>();

        /// <summary>
        /// Add a point to the set of geometries whose common bits are
        /// being computed.  After this method has executed the
        /// common coordinate reflects the common bits of all added
        /// geometries.
        /// </summary>
        /// <param name="geom">A Geometry to test for common bits.</param>
        public void Add(IGeometry<TCoordinate> geom)
        {
            geom.Apply(_filter);
            _commonCoordinate = _filter.CommonCoordinate;
        }

        /// <summary>
        /// The common bits of the Coordinates in the supplied Geometries.
        /// </summary>
        public TCoordinate CommonCoordinate
        {
            get { return _commonCoordinate; }
        }

        /// <summary>
        /// Removes the common coordinate bits from a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry from which to remove the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public IGeometry<TCoordinate> RemoveCommonBits(IGeometry<TCoordinate> geom)
        {
            if (_commonCoordinate[Ordinates.X] == 0.0 && _commonCoordinate[Ordinates.Y] == 0.0)
            {
                return geom;
            }

            TCoordinate invCoord = new TCoordinate(_commonCoordinate.Negative());
            Translater<TCoordinate> trans = new Translater<TCoordinate>(invCoord);
            geom.Apply(trans);
            //geom.GeometryChanged();
            return geom;
        }

        /// <summary>
        /// Adds the common coordinate bits back into a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry to which to add the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public void AddCommonBits(IGeometry<TCoordinate> geom)
        {
            Translater<TCoordinate> trans = new Translater<TCoordinate>(_commonCoordinate);
            geom.Apply(trans);
            //geom.GeometryChanged();
        }

        public class CommonCoordinateFilter<TCoordinate> : ICoordinateFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
        {
            private readonly CommonBits _commonBitsX = new CommonBits();
            private readonly CommonBits _commonBitsY = new CommonBits();
            private readonly CommonBits _commonBitsZ = new CommonBits();
            private readonly CommonBits _commonBitsM = new CommonBits();
            private Boolean _hasZ = false;
            private Boolean _hasM = false;


            public void Filter(TCoordinate coord)
            {
                _commonBitsX.Add(coord[Ordinates.X]);
                _commonBitsY.Add(coord[Ordinates.Y]);

                if (_hasZ || coord.ContainsOrdinate(Ordinates.Z))
                {
                    _hasZ = true;
                    _commonBitsZ.Add(coord[Ordinates.Z]);
                }

                if (_hasM || coord.ContainsOrdinate(Ordinates.M))
                {
                    _hasM = true;
                    _commonBitsM.Add(coord[Ordinates.M]);
                }
            }

            public TCoordinate CommonCoordinate
            {
                get { return new TCoordinate(_commonBitsX.Common, _commonBitsY.Common); }
            }
        }

        private class Translater<TCoordinate> : ICoordinateFilter<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
        {
            private readonly TCoordinate _trans;

            public Translater(TCoordinate trans)
            {
                _trans = trans;
            }

            public void Filter(TCoordinate coord)
            {
                coord[Ordinates.X] += _trans[Ordinates.X];
                coord[Ordinates.Y] += _trans[Ordinates.Y];
            }
        }
    }
}