using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    ///<summary>
    /// A <see cref="IBoundaryNodeRule"/> which determines that only
    /// endpoints with valency greater than 1 are on the boundary.
    /// This corresponds to the boundary of a <see cref="IMultiLineString"/>
    /// being all the "attached" endpoints, but not
    /// the "unattached" ones.
    ///</summary>
    ///<author>Martin Davis</author>
    ///<version>1.7</version>
    public class MultiValentEndPointBoundaryNodeRule : IBoundaryNodeRule
    {
        public Boolean IsInBoundary(int boundaryCount)
        {
            return boundaryCount > 1;
        }
    }
}