namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Builds an <see cref="AffineTransformation"/> defined by a set of control vectors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A control vector consists of a source point and a destination point,
    /// which is the image of the source point under the desired transformation.
    /// </para>
    /// <para>
    /// A transformation is well-defined
    /// by a set of three control vectors
    /// if and only if the source points are not collinear.
    /// (In particular, the degenerate situation
    /// where two or more source points are identical will not produce a well-defined transformation).
    /// A well-defined transformation exists and is unique.
    /// If the control vectors are not well-defined, the system of equations
    /// defining the transformation matrix entries is not solvable,
    /// and no transformation can be determined.</para>
    /// <para>
    /// No such restriction applies to the destination points.
    /// However, if the destination points are collinear or non-unique,
    /// a non-invertible transformations will be generated.
    /// </para>
    /// <para>
    /// This technique of recovering a transformation
    /// from its effect on known points is used in the Bilinear Interpolated Triangulation
    /// algorithm for warping planar surfaces.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class AffineTransformationBuilder
    {
        private readonly Coordinate _src0;
        private readonly Coordinate _src1;
        private readonly Coordinate _src2;
        private readonly Coordinate _dest0;
        private readonly Coordinate _dest1;
        private readonly Coordinate _dest2;

        // the matrix entries for the transformation
        private double _m00, _m01, _m02, _m10, _m11, _m12;

        /// <summary>
        /// Constructs a new builder for the transformation defined by the given set of control point mappings.
        /// </summary>
        /// <param name="src0">A control point</param>
        /// <param name="src1">A control point</param>
        /// <param name="src2">A control point</param>
        /// <param name="dest0">The image of <paramref name="src0"/> under the required transformation</param>
        /// <param name="dest1">The image of <paramref name="src1"/> under the required transformation</param>
        /// <param name="dest2">The image of <paramref name="src2"/> under the required transformation</param>
        public AffineTransformationBuilder(Coordinate src0,
            Coordinate src1,
            Coordinate src2,
            Coordinate dest0,
            Coordinate dest1,
            Coordinate dest2)
        {
            _src0 = src0;
            _src1 = src1;
            _src2 = src2;
            _dest0 = dest0;
            _dest1 = dest1;
            _dest2 = dest2;
        }

        /// <summary>
        /// Computes the <see cref="AffineTransformation"/>
        /// determined by the control point mappings,
        /// or <c>null</c> if the control vectors do not determine a well-defined transformation.
        /// </summary>
        /// <returns>
        /// An affine transformation, or <see langword="null"/> if the control vectors do not
        /// determine a well-defined transformation.
        /// </returns>
        public AffineTransformation GetTransformation()
        {
            // compute full 3-point transformation
            bool isSolvable = Compute();
            if (isSolvable)
                return new AffineTransformation(_m00, _m01, _m02, _m10, _m11, _m12);
            return null;
        }

        /// <summary>
        /// Computes the transformation matrix by
        /// solving the two systems of linear equations
        /// defined by the control point mappings,
        /// if this is possible.
        /// </summary>
        /// <returns>True if the transformation matrix is solvable</returns>
        private bool Compute()
        {
            double[] bx = new[] { _dest0.X, _dest1.X, _dest2.X };
            double[] row0 = Solve(bx);
            if (row0 == null) return false;
            _m00 = row0[0];
            _m01 = row0[1];
            _m02 = row0[2];

            double[] by = new[] { _dest0.Y, _dest1.Y, _dest2.Y };
            double[] row1 = Solve(by);
            if (row1 == null) return false;
            _m10 = row1[0];
            _m11 = row1[1];
            _m12 = row1[2];
            return true;
        }

        /// <summary>
        /// Solves the transformation matrix system of linear equations
        /// for the given right-hand side vector.
        /// </summary>
        /// <param name="b">The vector for the right-hand side of the system</param>
        /// <returns>The solution vector, or <see langword="null"/> if no solution could be determined.</returns>
        private double[] Solve(double[] b)
        {
            double[][] a = new double[3][];
            a[0] = new[] { _src0.X, _src0.Y, 1 };
            a[1] = new[] { _src1.X, _src1.Y, 1 };
            a[2] = new[] { _src2.X, _src2.Y, 1 };
            return Matrix.Solve(a, b);
        }
    }
}
