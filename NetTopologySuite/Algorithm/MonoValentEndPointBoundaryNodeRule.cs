using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    ///<summary>
    /// A <see cref="IBoundaryNodeRule"/> which determines that only
    /// endpoints with valency of exactly 1 are on the boundary.
    /// This corresponds to the boundary of a <see cref="IMultiLineString"/>
    /// being all the "unattached" endpoints.
    ///</summary>
    public class MonoValentEndPointBoundaryNodeRule : IBoundaryNodeRule
    {
        public Boolean IsInBoundary(int boundaryCount)
        {
            return boundaryCount == 1;
        }
    }
}