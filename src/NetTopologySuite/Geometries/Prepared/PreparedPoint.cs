namespace NetTopologySuite.Geometries.Prepared
{
    /// <summary>
    /// A prepared version for <see cref="IPuntal"/> geometries.
    /// <para>Instances of this class are thread-safe.</para>
    /// </summary>
    /// <author>Martin Davis</author>
    public class PreparedPoint : BasicPreparedGeometry
    {
        public PreparedPoint(IPuntal point)
            : base((Geometry)point)
        {
        }

        /// <summary>
        /// Tests whether this point intersects a <see cref="Geometry"/>.
        /// </summary>
        /// <remarks>
        /// The optimization here is that computing topology for the test geometry
        /// is avoided. This can be significant for large geometries.
        /// </remarks>
        public override bool Intersects(Geometry g)
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
