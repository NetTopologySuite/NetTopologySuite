using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Removes common most-significant mantissa bits
    /// from one or more <see cref="Geometry"/>s.
    /// <para/>
    /// The CommonBitsRemover "scavenges" precision
    /// which is "wasted" by a large displacement of the geometry
    /// from the origin.
    /// For example, if a small geometry is displaced from the origin
    /// by a large distance,
    /// the displacement increases the significant figures in the coordinates,
    /// but does not affect the <i>relative</i> topology of the geometry.
    /// Thus the geometry can be translated back to the origin
    /// without affecting its topology.
    /// In order to compute the translation without affecting
    /// the full precision of the coordinate values,
    /// the translation is performed at the bit level by
    /// removing the common leading mantissa bits.
    /// <para/>
    /// If the geometry envelope already contains the origin,
    /// the translation procedure cannot be applied.
    /// In this case, the common bits value is computed as zero.
    /// <para/>
    /// If the geometry crosses the Y axis but not the X axis
    /// (and <i>mutatis mutandum</i>),
    /// the common bits for Y are zero,
    /// but the common bits for X are non-zero.
    /// </summary>
    public class CommonBitsRemover
    {
        private Coordinate _commonCoord;
        private readonly CommonCoordinateFilter _ccFilter = new CommonCoordinateFilter();

        /*
        /// <summary>
        ///
        /// </summary>
        public CommonBitsRemover() { }
        */

        /// <summary>
        /// Add a point to the set of geometries whose common bits are
        /// being computed.  After this method has executed the
        /// common coordinate reflects the common bits of all added
        /// geometries.
        /// </summary>
        /// <param name="geom">A Geometry to test for common bits.</param>
        public void Add(Geometry geom)
        {
            geom.Apply(_ccFilter);
            _commonCoord = _ccFilter.CommonCoordinate;
        }

        /// <summary>
        /// The common bits of the Coordinates in the supplied Geometries.
        /// </summary>
        public Coordinate CommonCoordinate => _commonCoord;

        /// <summary>
        /// Removes the common coordinate bits from a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry from which to remove the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public Geometry RemoveCommonBits(Geometry geom)
        {
            if (_commonCoord.X == 0.0 && _commonCoord.Y == 0.0)
                return geom;
            var invCoord = _commonCoord.Copy();
            invCoord.X = -invCoord.X;
            invCoord.Y = -invCoord.Y;
            var trans = new Translater(invCoord);
            geom.Apply(trans);
            geom.GeometryChanged();
            return geom;
        }

        /// <summary>
        /// Adds the common coordinate bits back into a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry to which to add the common coordinate bits.</param>
        public void AddCommonBits(Geometry geom)
        {
            var trans = new Translater(_commonCoord);
            geom.Apply(trans);
            geom.GeometryChanged();
        }

        /// <summary>
        ///
        /// </summary>
        public class CommonCoordinateFilter : ICoordinateFilter
        {
            private readonly CommonBits _commonBitsX = new CommonBits();
            private readonly CommonBits _commonBitsY = new CommonBits();

            /// <summary>
            ///
            /// </summary>
            /// <param name="coord"></param>
            public void Filter(Coordinate coord)
            {
                _commonBitsX.Add(coord.X);
                _commonBitsY.Add(coord.Y);
            }

            /// <summary>
            ///
            /// </summary>
            public Coordinate CommonCoordinate => new Coordinate(_commonBitsX.Common, _commonBitsY.Common);
        }

        /// <summary>
        ///
        /// </summary>
        private class Translater : ICoordinateSequenceFilter
        {
            private readonly Coordinate _trans;

            /// <summary>
            ///
            /// </summary>
            /// <param name="trans"></param>
            public Translater(Coordinate trans)
            {
                _trans = trans;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="seq">The coordinate sequence</param>
            public void Filter(CoordinateSequence seq, int i)
            {
                double xp = seq.GetOrdinate(i, 0) + _trans.X;
                double yp = seq.GetOrdinate(i, 1) + _trans.Y;
                seq.SetOrdinate(i, 0, xp);
                seq.SetOrdinate(i, 1, yp);
            }

            public bool Done => false;

            public bool GeometryChanged => true;
        }
    }
}
