using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Implements the SFS <c>relate()</c>  generalized spatial predicate on two <see cref="Geometry"/>s.
    /// <br/>
    /// The class supports specifying a custom <see cref="IBoundaryNodeRule"/>
    /// to be used during the relate computation.
    /// </summary>
    /// <remarks>
    /// If named spatial predicates are used on the result <see cref="IntersectionMatrix"/>
    /// of the RelateOp, the result may or not be affected by the
    /// choice of <tt>BoundaryNodeRule</tt>, depending on the exact nature of the pattern.
    /// For instance, <see cref="Geometries.IntersectionMatrix.IsIntersects"/> is insensitive
    /// to the choice of <tt>BoundaryNodeRule</tt>,
    /// whereas <see cref="Geometries.IntersectionMatrix.IsTouches"/> is affected by the rule chosen.
    /// <para/>
    /// <b>Note:</b> custom Boundary Node Rules do not (currently)
    /// affect the results of other <see cref="Geometry"/> methods (such
    /// as <see cref="Geometry.Boundary"/>.  The results of
    /// these methods may not be consistent with the relationship computed by
    /// a custom Boundary Node Rule.
    /// </remarks>
    public class RelateOp : GeometryGraphOperation
    {
        /// <summary>
        /// Computes the <see cref="IntersectionMatrix"/> for the spatial relationship
        ///  between two <see cref="Geometry"/>s, using the default (OGC SFS) Boundary Node Rule
        /// </summary>
        /// <param name="a">A geometry to test</param>
        /// <param name="b">A geometry to test</param>
        /// <returns>The <c>IntersectionMatrix</c> for the spatial relationship between the geometries</returns>
        public static IntersectionMatrix Relate(Geometry a, Geometry b)
        {
            var relOp = new RelateOp(a, b);
            var im = relOp.IntersectionMatrix;
            return im;
        }

        /// <summary>
        /// Computes the <see cref="IntersectionMatrix"/> for the spatial relationship
        ///  between two <see cref="Geometry"/>s, using the specified Boundary Node Rule
        /// </summary>
        /// <param name="a">A geometry to test</param>
        /// <param name="b">A geometry to test</param>
        /// <param name="boundaryNodeRule">The Boundary Node Rule to use</param>
        /// <returns>The <c>IntersectionMatrix</c> for the spatial relationship between the geometries</returns>
        public static IntersectionMatrix Relate(Geometry a, Geometry b, IBoundaryNodeRule boundaryNodeRule)
        {
            var relOp = new RelateOp(a, b, boundaryNodeRule);
            var im = relOp.IntersectionMatrix;
            return im;
        }

        private readonly RelateComputer _relate;

        /// <summary>
        /// Creates a new Relate operation, using the default (OGC SFS) Boundary Node Rule.
        /// </summary>
        /// <param name="g0">a Geometry to relate</param>
        /// <param name="g1">another Geometry to relate</param>
        public RelateOp(Geometry g0, Geometry g1) : base(g0, g1)
        {
            _relate = new RelateComputer(arg);
        }

        /// <summary>
        /// Creates a new Relate operation, using the default (OGC SFS) Boundary Node Rule.
        /// </summary>
        /// <param name="g0">a Geometry to relate</param>
        /// <param name="g1">another Geometry to relate</param>
        /// <param name="boundaryNodeRule">The Boundary Node Rule to use</param>
        public RelateOp(Geometry g0, Geometry g1, IBoundaryNodeRule boundaryNodeRule)
            : base(g0, g1, boundaryNodeRule)
        {
            _relate = new RelateComputer(arg);
        }

        /// <summary>
        /// Gets the IntersectionMatrix for the spatial relationship
        /// between the input geometries.
        /// </summary>
        public IntersectionMatrix IntersectionMatrix => _relate.ComputeIM();
    }
}