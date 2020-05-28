using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Functions to reduce the precision of a geometry
    /// by rounding it to a given precision model.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PrecisionReducer
    {

        /// <summary>
        /// Reduces the precision of a geometry by rounding it to the
        /// supplied <see cref="PrecisionModel"/>.
        /// <para/> 
        /// The output is always a valid geometry.  This implies that input components
        /// may be merged if they are closer than the grid precision.
        /// if merging is not desired, then the individual geometry components
        /// should be processed separately.
        /// <para/>
        /// The output is fully noded.  
        /// This provides an effective way to node / snap-round a collection of <see cref="LineString"/>s.
        /// </summary>
        /// <param name="geom">The geometry to reduce</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The precision-reduced geometry</returns>
        public static Geometry ReducePrecision(Geometry geom, PrecisionModel pm)
        {
            var reduced = OverlayNG.Union(geom, pm);
            return reduced;
        }

        private PrecisionReducer()
        {
            // no instantiation for now
        }
    }
}
