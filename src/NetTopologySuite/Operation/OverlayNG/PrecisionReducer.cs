using System;

using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Precision;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Functions to reduce the precision of a geometry
    /// by rounding it to a given precision model.
    /// <para/>
    /// This class handles only polygonal and linear inputs.
    /// For full functionality <see cref="GeometryPrecisionReducer"/>.
    /// </summary>
    /// <seealso cref="GeometryPrecisionReducer"/>
    /// <author>Martin Davis</author>
    public static class PrecisionReducer
    {

        /// <summary>
        /// Reduces the precision of a geometry by rounding and snapping it to the
        /// supplied <see cref="PrecisionModel"/>.<br/>
        /// The input geometry must be polygonal or linear.
        /// <para/> 
        /// The output is always a valid geometry.  This implies that input components
        /// may be merged if they are closer than the grid precision.
        /// if merging is not desired, then the individual geometry components
        /// should be processed separately.
        /// <para/>
        /// The output is fully noded (i.e. coincident lines are merged and noded).  
        /// This provides an effective way to node / snap-round a collection of <see cref="LineString"/>s.
        /// </summary>
        /// <param name="geom">The geometry to reduce</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The precision-reduced geometry</returns>
        public static Geometry ReducePrecision(Geometry geom, PrecisionModel pm)
        {
            if (geom == null)
            {
                throw new ArgumentNullException(nameof(geom));
            }

            var ov = new OverlayNG(geom, null, pm, SpatialFunction.Union);
            /*
             * Ensure reducing a area only produces polygonal result.
             * (I.e. collapse lines are not output)
             */
            if (geom.Dimension == Dimension.Surface)
                ov.AreaResultOnly = true;
            try
            {
                var reduced = ov.GetResult();
                return reduced;
            }
            catch (TopologyException ex)
            {
                throw new ArgumentException("Reduction failed, possible invalid input", ex);
            }
        }
    }
}
