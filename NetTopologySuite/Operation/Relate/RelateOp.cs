using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Implements the SFS <c>relate()</c> operation on two <see cref="IGeometry"/>s.
    /// This class supports specifying a custom <see cref="IBoundaryNodeRule"/>
    /// to be used during the relate computation.
    /// </summary>
    /// <remarks>
    /// <b>Note:</b> custom Boundary Node Rules do not (currently)
    /// affect the results of other Geometry methods (such
    /// as <see cref="IGeometry.Boundary"/>.  The results of
    /// these methods may not be consistent with the relationship computed by
    /// a custom Boundary Node Rule.
    /// </remarks>
    public class RelateOp : GeometryGraphOperation
    {
        /// <summary>
        /// Computes the <see cref="IntersectionMatrix"/> for the spatial relationship
        ///  between two <see cref="IGeometry"/>s, using the default (OGC SFS) Boundary Node Rule
        /// </summary>
        /// <param name="a">A geometry to test</param>
        /// <param name="b">A geometry to test</param>
        /// <returns>The IntersectonMatrix for the spatial relationship between the geometries</returns>
        public static IntersectionMatrix Relate(IGeometry a, IGeometry b)
        {
            RelateOp relOp = new RelateOp(a, b);
            IntersectionMatrix im = relOp.IntersectionMatrix;
            return im;
        }

        /// <summary>
        /// Computes the <see cref="IntersectionMatrix"/> for the spatial relationship
        ///  between two <see cref="IGeometry"/>s, using the specified Boundary Node Rule
        /// </summary>
        /// <param name="a">A geometry to test</param>
        /// <param name="b">A geometry to test</param>
        /// <param name="boundaryNodeRule">The Boundary Node Rule to use</param>
        /// <returns>The IntersectonMatrix for the spatial relationship between the geometries</returns>
        public static IntersectionMatrix Relate(IGeometry a, IGeometry b, IBoundaryNodeRule boundaryNodeRule)
        {
            RelateOp relOp = new RelateOp(a, b, boundaryNodeRule);
            IntersectionMatrix im = relOp.IntersectionMatrix;
            return im;
        }


        private readonly RelateComputer _relate;

        /// <summary>
        /// Creates a new Relate operation, using the default (OGC SFS) Boundary Node Rule.
        /// </summary>
        /// <param name="g0">a Geometry to relate</param>
        /// <param name="g1">another Geometry to relate</param>
        public RelateOp(IGeometry g0, IGeometry g1) : base(g0, g1)
        {            
            _relate = new RelateComputer(arg);
        }

        /// <summary>
        /// Creates a new Relate operation, using the default (OGC SFS) Boundary Node Rule.
        /// </summary>
        /// <param name="g0">a Geometry to relate</param>
        /// <param name="g1">another Geometry to relate</param>
        /// <param name="boundaryNodeRule">The Boundary Node Rule to use</param>
        public RelateOp(IGeometry g0, IGeometry g1, IBoundaryNodeRule boundaryNodeRule)
            : base(g0, g1, boundaryNodeRule)
        {
            _relate = new RelateComputer(arg);
        }

        /// <summary>
        /// Gets the IntersectionMatrix for the spatial relationship
        /// between the input geometries.
        /// </summary>
        public IntersectionMatrix IntersectionMatrix
        {
            get
            {
                return _relate.ComputeIM();
            }
        }
    }
}