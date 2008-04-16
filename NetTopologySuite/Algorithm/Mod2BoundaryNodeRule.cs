using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// The Mod-2 Boundary Node Rule (as used in the OGC SFS). 
    /// It specifies that points are in the boundary of a lineal geometry 
    /// iff the point lies on the boundary of an odd number of components. 
    /// Under this rule <see cref="ILinearRing"/>s and closed
    /// <see cref="ILineString"/>s have empty boundaries. 
    /// </summary>
    public class Mod2BoundaryNodeRule : IBoundaryNodeRule
    {
        #region IBoundaryNodeRule Members

        public Boolean IsInBoundary(Int32 boundaryCount)
        {
            return boundaryCount % 2 == 1;
        }

        #endregion
    }
}
