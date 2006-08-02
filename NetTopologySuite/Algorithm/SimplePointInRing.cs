using System;
using System.Collections;
using System.Text;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Tests whether a <c>Coordinate</c> lies inside
    /// a ring, using a linear-time algorithm.
    /// </summary>
    public class SimplePointInRing : IPointInRing
    {
        /// <summary>
        /// 
        /// </summary>
        private Coordinate[] pts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        public SimplePointInRing(LinearRing ring)
        {
            pts = ring.Coordinates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public virtual bool IsInside(Coordinate pt)
        {
            return CGAlgorithms.IsPointInRing(pt, pts);
        }
    }
}
