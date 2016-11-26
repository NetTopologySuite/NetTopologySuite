using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Prepared
{
    ///<summary>
    /// A prepared version for <see cref="IPuntal"/> geometries.
    /// <para>Instances of this class are thread-safe.</para>
    ///</summary>
    /// <author>Martin Davis</author>
    public class PreparedPoint : BasicPreparedGeometry
    {
        public PreparedPoint(IPuntal point)
            : base((IGeometry)point)
        {
        }

        ///<summary>
        /// Tests whether this point intersects a <see cref="IGeometry"/>.
        ///</summary>
        /// <remarks>
        /// The optimization here is that computing topology for the test geometry
        /// is avoided. This can be significant for large geometries.
        /// </remarks>
        public override bool Intersects(IGeometry g)
        {
            if (!EnvelopesIntersect(g))
                return false;

            /*
             * This avoids computing topology for the test geometry
             */
            return IsAnyTargetComponentInTest(g);
        }
    }
}
