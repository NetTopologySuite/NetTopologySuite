using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// An interface for classes which test whether a <c>Coordinate</c> lies inside a ring.
    /// </summary>
    public interface IPointInRing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        Boolean IsInside(ICoordinate pt);
    }
}