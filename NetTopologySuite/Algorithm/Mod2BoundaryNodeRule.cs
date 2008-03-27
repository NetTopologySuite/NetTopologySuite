using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// The Mod-2 Boundary Node Rule (as used in the OGC SFS).
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
