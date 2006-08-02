using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary>
    /// Allow computing and removing common mantissa bits from one or more Geometries.
    /// </summary>
    public class CommonBitsRemover
    {
        private Coordinate commonCoord = null;
        private CommonCoordinateFilter ccFilter = new CommonCoordinateFilter();

        /// <summary>
        /// 
        /// </summary>
        public CommonBitsRemover() { }

        /// <summary>
        /// Add a point to the set of geometries whose common bits are
        /// being computed.  After this method has executed the
        /// common coordinate reflects the common bits of all added
        /// geometries.
        /// </summary>
        /// <param name="geom">A Geometry to test for common bits.</param>
        public virtual void Add(Geometry geom)
        {
            geom.Apply(ccFilter);
            commonCoord = ccFilter.CommonCoordinate;
        }

        /// <summary>
        /// The common bits of the Coordinates in the supplied Geometries.
        /// </summary>
        public virtual Coordinate CommonCoordinate
        {
            get
            {
                return commonCoord; 
            }
        }

        /// <summary>
        /// Removes the common coordinate bits from a Geometry.
        /// The coordinates of the Geometry are changed.
        /// </summary>
        /// <param name="geom">The Geometry from which to remove the common coordinate bits.</param>
        /// <returns>The shifted Geometry.</returns>
        public virtual Geometry RemoveCommonBits(Geometry geom)
        {
            if (commonCoord.X == 0.0 && commonCoord.Y == 0.0)
                return geom;
            Coordinate invCoord = new Coordinate(commonCoord);
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
        public virtual void AddCommonBits(Geometry geom)
        {
            Translater trans = new Translater(commonCoord);
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
            public virtual void Filter(Coordinate coord)
            {
                commonBitsX.Add(coord.X);
                commonBitsY.Add(coord.Y);
            }

            /// <summary>
            /// 
            /// </summary>
            public virtual Coordinate CommonCoordinate
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
            private Coordinate trans = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="trans"></param>
            public Translater(Coordinate trans)
            {
                this.trans = trans;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coord"></param>
            public virtual void Filter(Coordinate coord)
            {
                coord.X += trans.X;
                coord.Y += trans.Y;
            }
        }
    }
}
