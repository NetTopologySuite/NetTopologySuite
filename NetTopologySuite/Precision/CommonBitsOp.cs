using NetTopologySuite.Geometries;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Provides versions of Geometry spatial functions which use
    /// common bit removal to reduce the likelihood of robustness problems.
    /// In the current implementation no rounding is performed on the
    /// reshifted result point, which means that it is possible
    /// that the returned Geometry is invalid.
    /// Client classes should check the validity of the returned result themselves.
    /// </summary>
    public class CommonBitsOp
    {
        private readonly bool _returnToOriginalPrecision = true;
        private CommonBitsRemover _cbr;

        /// <summary>
        /// Creates a new instance of class, which reshifts result <c>Geometry</c>s.
        /// </summary>
        public CommonBitsOp() : this(true) { }

        /// <summary>
        /// Creates a new instance of class, specifying whether
        /// the result <c>Geometry</c>s should be reshifted.
        /// </summary>
        /// <param name="returnToOriginalPrecision"></param>
        public CommonBitsOp(bool returnToOriginalPrecision)
        {
            _returnToOriginalPrecision = returnToOriginalPrecision;
        }

        /// <summary>
        /// Computes the set-theoretic intersection of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic intersection of the input Geometries.</returns>
        public Geometry Intersection(Geometry geom0, Geometry geom1)
        {
            var geom = RemoveCommonBits(geom0, geom1);
            return ComputeResultPrecision(geom[0].Intersection(geom[1]));
        }

        /// <summary>
        /// Computes the set-theoretic union of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic union of the input Geometries.</returns>
        public Geometry Union(Geometry geom0, Geometry geom1)
        {
            var geom = RemoveCommonBits(geom0, geom1);
            return ComputeResultPrecision(geom[0].Union(geom[1]));
        }

        /// <summary>
        /// Computes the set-theoretic difference of two <c>Geometry</c>s, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry, to be subtracted from the first.</param>
        /// <returns>The Geometry representing the set-theoretic difference of the input Geometries.</returns>
        public Geometry Difference(Geometry geom0, Geometry geom1)
        {
            var geom = RemoveCommonBits(geom0, geom1);
            return ComputeResultPrecision(geom[0].Difference(geom[1]));
        }

        /// <summary
        /// > Computes the set-theoretic symmetric difference of two geometries,
        /// using enhanced precision.
        /// </summary>
        /// <param name="geom0">The first Geometry.</param>
        /// <param name="geom1">The second Geometry.</param>
        /// <returns>The Geometry representing the set-theoretic symmetric difference of the input Geometries.</returns>
        public Geometry SymDifference(Geometry geom0, Geometry geom1)
        {
            var geom = RemoveCommonBits(geom0, geom1);
            return ComputeResultPrecision(geom[0].SymmetricDifference(geom[1]));
        }

        /// <summary>
        /// Computes the buffer a point, using enhanced precision.
        /// </summary>
        /// <param name="geom0">The Geometry to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <returns>The Geometry representing the buffer of the input Geometry.</returns>
        public Geometry Buffer(Geometry geom0, double distance)
        {
            var geom = RemoveCommonBits(geom0);
            return ComputeResultPrecision(geom.Buffer(distance));
        }

        /// <summary>
        /// If required, returning the result to the original precision if required.
        /// In this current implementation, no rounding is performed on the
        /// reshifted result point, which means that it is possible
        /// that the returned Geometry is invalid.
        /// </summary>
        /// <param name="result">The result Geometry to modify.</param>
        /// <returns>The result Geometry with the required precision.</returns>
        private Geometry ComputeResultPrecision(Geometry result)
        {
            if (_returnToOriginalPrecision)
                _cbr.AddCommonBits(result);
            return result;
        }

        /// <summary>
        /// Computes a copy of the input <c>Geometry</c> with the calculated common bits
        /// removed from each coordinate.
        /// </summary>
        /// <param name="geom0">The Geometry to remove common bits from.</param>
        /// <returns>A copy of the input Geometry with common bits removed.</returns>
        private Geometry RemoveCommonBits(Geometry geom0)
        {
            _cbr = new CommonBitsRemover();
            _cbr.Add(geom0);
            var geom = _cbr.RemoveCommonBits((Geometry) geom0.Copy());
            return geom;
        }

        /// <summary>
        /// Computes a copy of each input <c>Geometry</c>s with the calculated common bits
        /// removed from each coordinate.
        /// </summary>
        /// <param name="geom0">A Geometry to remove common bits from.</param>
        /// <param name="geom1">A Geometry to remove common bits from.</param>
        /// <returns>
        /// An array containing copies
        /// of the input Geometry's with common bits removed.
        /// </returns>
        private Geometry[] RemoveCommonBits(Geometry geom0, Geometry geom1)
        {
            _cbr = new CommonBitsRemover();
            _cbr.Add(geom0);
            _cbr.Add(geom1);
            var geom = new Geometry[2];
            geom[0] = _cbr.RemoveCommonBits((Geometry) geom0.Copy());
            geom[1] = _cbr.RemoveCommonBits((Geometry) geom1.Copy());
            return geom;
        }
    }
}
