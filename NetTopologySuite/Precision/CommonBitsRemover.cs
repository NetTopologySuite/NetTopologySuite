using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary>
    /// Allow computing and removing common mantissa bits from one or more Geometries.
    /// </summary>
    public class CommonBitsRemover
    {
        private ICoordinate _commonCoord;
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
        public void Add(IGeometry geom)
        {
            geom.Apply(_ccFilter);
            _commonCoord = _ccFilter.CommonCoordinate;
        }

        /// <summary>
        /// The common bits of the Coordinates in the supplied Geometries.
        /// </summary>
        public ICoordinate CommonCoordinate
        {
            get { return _commonCoord; }
        }

        /// <summary>
        /// Removes the common coordinate bits from a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry from which to remove the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public IGeometry RemoveCommonBits(IGeometry geom)
        {
            if (_commonCoord.X == 0.0 && _commonCoord.Y == 0.0)
                return geom;
            ICoordinate invCoord = new Coordinate(_commonCoord);
            invCoord.X = -invCoord.X;
            invCoord.Y = -invCoord.Y;
            Translater trans = new Translater(invCoord);            
            geom.Apply(trans);
            geom.GeometryChanged();
            return geom;
        }

        /// <summary>
        /// Adds the common coordinate bits back into a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry to which to add the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public void AddCommonBits(IGeometry geom)
        {
            Translater trans = new Translater(_commonCoord);
            geom.Apply(trans);
            geom.GeometryChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        public class CommonCoordinateFilter : ICoordinateFilter
        {
            private CommonBits commonBitsX = new CommonBits();
            private CommonBits commonBitsY = new CommonBits();

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coord"></param>
            public void Filter(ICoordinate coord)
            {
                commonBitsX.Add(coord.X);
                commonBitsY.Add(coord.Y);
            }

            /// <summary>
            /// 
            /// </summary>
            public ICoordinate CommonCoordinate
            {
                get
                {
                    return new Coordinate(commonBitsX.Common, commonBitsY.Common);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        class Translater : ICoordinateFilter
        {
            private readonly ICoordinate _trans;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="trans"></param>
            public Translater(ICoordinate trans)
            {
                _trans = trans;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coord"></param>
            public void Filter(ICoordinate coord)
            {
                coord.X += _trans.X;
                coord.Y += _trans.Y;
            }
        }
    }
}
