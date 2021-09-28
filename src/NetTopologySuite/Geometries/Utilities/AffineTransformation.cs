using System;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Represents an affine transformation on the 2D Cartesian plane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It can be used to transform a <see cref="Coordinate"/> or <see cref="Geometry"/>.
    /// An affine transformation is a mapping of the 2D plane into itself
    /// via a series of transformations of the following basic types:
    /// <ul>
    /// <li>reflection (through a line)</li>
    /// <li>rotation (around the origin)</li>
    /// <li>scaling (relative to the origin)</li>
    /// <li>shearing (in both the X and Y directions)</li>
    /// <li>translation</li>
    /// </ul>
    /// </para>
    /// <para>
    /// In general, affine transformations preserve straightness and parallel lines,
    /// but do not preserve distance or shape.
    /// </para>
    /// <para>
    /// An affine transformation can be represented by a 3x3
    /// matrix in the following form:
    /// <blockquote><code>
    /// T = | m00 m01 m02 |<br/>
    ///     | m10 m11 m12 |<br/>
    ///     |  0   0   1  |
    /// </code></blockquote>
    /// A coordinate P = (x, y) can be transformed to a new coordinate P' = (x', y')
    /// by representing it as a 3x1 matrix and using matrix multiplication to compute:
    /// <blockquote><code>
    /// | x' |  = T x | x |<br/>
    /// | y' |        | y |<br/>
    /// | 1  |        | 1 |
    /// </code></blockquote>
    /// </para>
    /// <h3>Transformation Composition</h3>
    /// <para>
    /// Affine transformations can be composed using the <see cref="Compose"/> method.
    /// Composition is computed via multiplication of the
    /// transformation matrices, and is defined as:
    /// <blockquote><pre>
    /// A.compose(B) = T<sub>B</sub> x T<sub>A</sub>
    /// </pre></blockquote>
    /// </para>
    /// <para>
    /// This produces a transformation whose effect is that of A followed by B.
    /// The methods <see cref="Reflect"/>, <see cref="Rotate"/>,
    /// <see cref="Scale"/>, <see cref="Shear"/>, and <see cref="Translate"/>
    /// have the effect of composing a transformation of that type with
    /// the transformation they are invoked on.
    /// The composition of transformations is in general <i>not</i> commutative.
    /// </para>
    /// <h3>Transformation Inversion</h3>
    /// <para>
    /// Affine transformations may be invertible or non-invertible.
    /// If a transformation is invertible, then there exists
    /// an inverse transformation which when composed produces
    /// the identity transformation.
    /// The <see cref="GetInverse"/> method
    /// computes the inverse of a transformation, if one exists.
    /// </para>
    /// <para>
    /// @author Martin Davis
    /// </para>
    /// </remarks>
    public class AffineTransformation : ICloneable, ICoordinateSequenceFilter, IEquatable<AffineTransformation>
    {
        /// <summary>
        /// Creates a transformation for a reflection about the
        /// line (x0,y0) - (x1,y1).
        /// </summary>
        /// <param name="x0"> the x-ordinate of a point on the reflection line</param>
        /// <param name="y0"> the y-ordinate of a point on the reflection line</param>
        /// <param name="x1"> the x-ordinate of a another point on the reflection line</param>
        /// <param name="y1"> the y-ordinate of a another point on the reflection line</param>
        /// <returns> a transformation for the reflection</returns>
        public static AffineTransformation ReflectionInstance(double x0, double y0, double x1, double y1)
        {
            var trans = new AffineTransformation();
            trans.SetToReflection(x0, y0, x1, y1);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a reflection about the
        /// line (0,0) - (x,y).
        /// </summary>
        /// <param name="x"> the x-ordinate of a point on the reflection line</param>
        /// <param name="y"> the y-ordinate of a point on the reflection line</param>
        /// <returns> a transformation for the reflection</returns>
        public static AffineTransformation ReflectionInstance(double x, double y)
        {
            var trans = new AffineTransformation();
            trans.SetToReflection(x, y);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a rotation
        /// about the origin
        /// by an angle <i>theta</i>.
        /// </summary>
        /// <remarks>
        /// Positive angles correspond to a rotation
        /// in the counter-clockwise direction.
        /// </remarks>
        /// <param name="theta"> the rotation angle, in radians</param>
        /// <returns> a transformation for the rotation</returns>
        public static AffineTransformation RotationInstance(double theta)
        {
            return RotationInstance(Math.Sin(theta), Math.Cos(theta));
        }

        /// <summary>
        /// Creates a transformation for a rotation
        /// by an angle <i>theta</i>,
        /// specified by the sine and cosine of the angle.
        /// </summary>
        /// <remarks>
        /// This allows providing exact values for sin(theta) and cos(theta)
        /// for the common case of rotations of multiples of quarter-circles.
        /// </remarks>
        /// <param name="sinTheta"> the sine of the rotation angle</param>
        /// <param name="cosTheta"> the cosine of the rotation angle</param>
        /// <returns> a transformation for the rotation</returns>
        public static AffineTransformation RotationInstance(double sinTheta, double cosTheta)
        {
            var trans = new AffineTransformation();
            trans.SetToRotation(sinTheta, cosTheta);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a rotation
        /// about the point (x,y) by an angle <i>theta</i>.
        /// </summary>
        /// <remarks>
        /// Positive angles correspond to a rotation
        /// in the counter-clockwise direction.
        /// </remarks>
        /// <param name="theta"> the rotation angle, in radians</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> a transformation for the rotation</returns>
        public static AffineTransformation RotationInstance(double theta, double x, double y)
        {
            return RotationInstance(Math.Sin(theta), Math.Cos(theta), x, y);
        }

        /// <summary>
        /// Creates a transformation for a rotation
        /// about the point (x,y) by an angle <i>theta</i>,
        /// specified by the sine and cosine of the angle.
        /// </summary>
        /// <remarks>
        /// This allows providing exact values for sin(theta) and cos(theta)
        /// for the common case of rotations of multiples of quarter-circles.
        /// </remarks>
        /// <param name="sinTheta"> the sine of the rotation angle</param>
        /// <param name="cosTheta"> the cosine of the rotation angle</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> a transformation for the rotation</returns>
        public static AffineTransformation RotationInstance(double sinTheta, double cosTheta, double x, double y)
        {
            var trans = new AffineTransformation();
            trans.SetToRotation(sinTheta, cosTheta, x, y);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a scaling relative to the origin.
        /// </summary>
        /// <param name="xScale"> the value to scale by in the x direction</param>
        /// <param name="yScale"> the value to scale by in the y direction</param>
        /// <returns> a transformation for the scaling</returns>
        public static AffineTransformation ScaleInstance(double xScale, double yScale)
        {
            var trans = new AffineTransformation();
            trans.SetToScale(xScale, yScale);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a scaling relative to the point (x,y).
        /// </summary>
        /// <param name="xScale">The value to scale by in the x direction</param>
        /// <param name="yScale">The value to scale by in the y direction</param>
        /// <param name="x">The x-ordinate of the point to scale around</param>
        /// <param name="y">The y-ordinate of the point to scale around</param>
        /// <returns>A transformation for the scaling</returns>
        public static AffineTransformation ScaleInstance(double xScale, double yScale, double x, double y)
        {
            var trans = new AffineTransformation();
            trans.Translate(-x, -y);
            trans.Scale(xScale, yScale);
            trans.Translate(x, y);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a shear.
        /// </summary>
        /// <param name="xShear"> the value to shear by in the x direction</param>
        /// <param name="yShear"> the value to shear by in the y direction</param>
        /// <returns> a transformation for the shear</returns>
        public static AffineTransformation ShearInstance(double xShear, double yShear)
        {
            var trans = new AffineTransformation();
            trans.SetToShear(xShear, yShear);
            return trans;
        }

        /// <summary>
        /// Creates a transformation for a translation.
        /// </summary>
        /// <param name="x"> the value to translate by in the x direction</param>
        /// <param name="y"> the value to translate by in the y direction</param>
        /// <returns> a transformation for the translation</returns>
        public static AffineTransformation TranslationInstance(double x, double y)
        {
            var trans = new AffineTransformation();
            trans.SetToTranslation(x, y);
            return trans;
        }

        // affine matrix entries
        // (bottom row is always [ 0 0 1 ])
        private double _m00;
        private double _m01;
        private double _m02;
        private double _m10;
        private double _m11;
        private double _m12;

        /// <summary>
        /// Constructs a new identity transformation
        /// </summary>
        public AffineTransformation()
        {
            SetToIdentity();
        }

        /// <summary>
        /// Constructs a new transformation whose
        /// matrix has the specified values.
        /// </summary>
        /// <param name="matrix"> an array containing the 6 values { m00, m01, m02, m10, m11, m12 }</param>
        /// <exception cref="NullReferenceException"> if matrix is null</exception>
        /// <exception cref="IndexOutOfRangeException"> if matrix is too small</exception>
        public AffineTransformation(double[] matrix)
        {
            _m00 = matrix[0];
            _m01 = matrix[1];
            _m02 = matrix[2];
            _m10 = matrix[3];
            _m11 = matrix[4];
            _m12 = matrix[5];
        }

        /// <summary>
        /// Constructs a new transformation whose
        /// matrix has the specified values.
        /// </summary>
        /// <param name="m00"> the entry for the [0, 0] element in the transformation matrix</param>
        /// <param name="m01"> the entry for the [0, 1] element in the transformation matrix</param>
        /// <param name="m02"> the entry for the [0, 2] element in the transformation matrix</param>
        /// <param name="m10"> the entry for the [1, 0] element in the transformation matrix</param>
        /// <param name="m11"> the entry for the [1, 1] element in the transformation matrix</param>
        /// <param name="m12"> the entry for the [1, 2] element in the transformation matrix</param>
        public AffineTransformation(double m00,
            double m01,
            double m02,
            double m10,
            double m11,
            double m12)
        {
            SetTransformation(m00, m01, m02, m10, m11, m12);
        }

        /// <summary>
        /// Constructs a transformation which is
        /// a copy of the given one.
        /// </summary>
        /// <param name="trans"> the transformation to copy</param>
        public AffineTransformation(AffineTransformation trans)
        {
            SetTransformation(trans);
        }

        /// <summary>
        /// Constructs a transformation
        /// which maps the given source
        /// points into the given destination points.
        /// </summary>
        /// <param name="src0"> source point 0</param>
        /// <param name="src1"> source point 1</param>
        /// <param name="src2"> source point 2</param>
        /// <param name="dest0"> the mapped point for source point 0</param>
        /// <param name="dest1"> the mapped point for source point 1</param>
        /// <param name="dest2"> the mapped point for source point 2</param>
        public AffineTransformation(Coordinate src0,
            Coordinate src1,
            Coordinate src2,
            Coordinate dest0,
            Coordinate dest1,
            Coordinate dest2)
            : this(new AffineTransformationBuilder(src0, src1, src2, dest0, dest1, dest2).GetTransformation())
        {
        }

        /// <summary>
        /// Sets this transformation to be the identity transformation.
        /// </summary>
        /// <remarks>
        /// The identity transformation has the matrix:
        /// <blockquote><code>
        /// | 1 0 0 |<br/>
        /// | 0 1 0 |<br/>
        /// | 0 0 1 |
        /// </code></blockquote>
        /// </remarks>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToIdentity()
        {
            _m00 = 1.0; _m01 = 0.0; _m02 = 0.0;
            _m10 = 0.0; _m11 = 1.0; _m12 = 0.0;
            return this;
        }

        /// <summary>
        /// Sets this transformation's matrix to have the given values.
        /// </summary>
        /// <param name="m00"> the entry for the [0, 0] element in the transformation matrix</param>
        /// <param name="m01"> the entry for the [0, 1] element in the transformation matrix</param>
        /// <param name="m02"> the entry for the [0, 2] element in the transformation matrix</param>
        /// <param name="m10"> the entry for the [1, 0] element in the transformation matrix</param>
        /// <param name="m11"> the entry for the [1, 1] element in the transformation matrix</param>
        /// <param name="m12"> the entry for the [1, 2] element in the transformation matrix</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetTransformation(double m00,
            double m01,
            double m02,
            double m10,
            double m11,
            double m12)
        {
            _m00 = m00;
            _m01 = m01;
            _m02 = m02;
            _m10 = m10;
            _m11 = m11;
            _m12 = m12;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a copy of the given one
        /// </summary>
        /// <param name="trans"> a transformation to copy</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetTransformation(AffineTransformation trans)
        {
            _m00 = trans._m00; _m01 = trans._m01; _m02 = trans._m02;
            _m10 = trans._m10; _m11 = trans._m11; _m12 = trans._m12;
            return this;
        }

        /// <summary>
        /// Gets an array containing the entries
        /// of the transformation matrix.
        /// </summary>
        /// <remarks>
        /// Only the 6 non-trivial entries are returned,
        /// in the sequence:
        /// <pre>
        /// m00, m01, m02, m10, m11, m12
        /// </pre>
        /// </remarks>
        /// <returns> an array of length 6</returns>
        public double[] MatrixEntries => new[] { _m00, _m01, _m02, _m10, _m11, _m12 };

        /// <summary>
        /// Computes the determinant of the transformation matrix.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The determinant is computed as:
        /// <blockquote><code>
        /// | m00 m01 m02 |<br/>
        /// | m10 m11 m12 | = m00 * m11 - m01 * m10<br/>
        /// |  0   0   1  |
        /// </code></blockquote>
        /// </para>
        /// <para>
        /// If the determinant is zero,
        /// the transform is singular (not invertible),
        /// and operations which attempt to compute
        /// an inverse will throw a <see cref="NoninvertibleTransformationException"/>.
        /// </para>
        /// </remarks>
        /// <returns> the determinant of the transformation</returns>
        /// <see cref="GetInverse()" />
        ///
        /// <returns>The determinant of the transformation</returns>
        /// <see cref="GetInverse"/>
        public double Determinant => _m00 * _m11 - _m01 * _m10;

        /// <summary>
        /// Computes the inverse of this transformation, if one
        /// exists.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The inverse is the transformation which when
        /// composed with this one produces the identity
        /// transformation.
        /// A transformation has an inverse if and only if it
        /// is not singular (i.e. its
        /// determinant is non-zero).
        /// Geometrically, an transformation is non-invertible
        /// if it maps the plane to a line or a point.
        /// If no inverse exists this method
        /// will throw a <see cref="NoninvertibleTransformationException"/>.
        /// </para>
        /// <para>
        /// The matrix of the inverse is equal to the
        /// inverse of the matrix for the transformation.
        /// It is computed as follows:
        /// <blockquote><code>
        ///                 1
        /// inverse(A)  =  ---   x  adjoint(A)
        ///                det
        /// 
        /// 
        ///             =   1       |  m11  -m01   m01*m12-m02*m11  |
        ///                ---   x  | -m10   m00  -m00*m12+m10*m02  |
        ///                det      |  0     0     m00*m11-m10*m01  |
        /// 
        /// 
        /// 
        ///             = |  m11/det  -m01/det   m01*m12-m02*m11/det |
        ///               | -m10/det   m00/det  -m00*m12+m10*m02/det |
        ///               |   0           0          1               |
        /// </code></blockquote>
        /// </para>
        /// </remarks>
        /// <returns>A new inverse transformation</returns>
        /// <see cref="Determinant"/>
        /// <exception cref="NoninvertibleTransformationException"></exception>
        public AffineTransformation GetInverse()
        {
            double det = Determinant;
            if (det == 0)
                throw new NoninvertibleTransformationException("Transformation is non-invertible");

            double im00 = _m11 / det;
            double im10 = -_m10 / det;
            double im01 = -_m01 / det;
            double im11 = _m00 / det;
            double im02 = (_m01 * _m12 - _m02 * _m11) / det;
            double im12 = (-_m00 * _m12 + _m10 * _m02) / det;

            return new AffineTransformation(im00, im01, im02, im10, im11, im12);
        }

        /// <summary>
        /// Explicitly computes the math for a reflection.  May not work.
        /// </summary>
        /// <param name="x0">The x-ordinate of one point on the reflection line</param>
        /// <param name="y0">The y-ordinate of one point on the reflection line</param>
        /// <param name="x1">The x-ordinate of another point on the reflection line</param>
        /// <param name="y1">The y-ordinate of another point on the reflection line</param>
        /// <returns>This transformation with an updated matrix</returns>
        public AffineTransformation SetToReflectionBasic(double x0, double y0, double x1, double y1)
        {
            if (x0 == x1 && y0 == y1)
            {
                throw new ArgumentException("Reflection line points must be distinct");
            }
            double dx = x1 - x0;
            double dy = y1 - y0;
            double d = Math.Sqrt(dx * dx + dy * dy);
            double sin = dy / d;
            double cos = dx / d;
            double cs2 = 2 * sin * cos;
            double c2s2 = cos * cos - sin * sin;
            _m00 = c2s2; _m01 = cs2; _m02 = 0.0;
            _m10 = cs2; _m11 = -c2s2; _m12 = 0.0;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a reflection about the line defined by a line <tt>(x0,y0) - (x1,y1)</tt>.
        /// </summary>
        /// <param name="x0">The x-ordinate of one point on the reflection line</param>
        /// <param name="y0">The y-ordinate of one point on the reflection line</param>
        /// <param name="x1">The x-ordinate of another point on the reflection line</param>
        /// <param name="y1">The y-ordinate of another point on the reflection line</param>
        /// <returns>This transformation with an updated matrix</returns>
        public AffineTransformation SetToReflection(double x0, double y0, double x1, double y1)
        {
            if (x0 == x1 && y0 == y1)
            {
                throw new ArgumentException("Reflection line points must be distinct");
            }
            // translate line vector to origin
            SetToTranslation(-x0, -y0);

            // rotate vector to positive x axis direction
            double dx = x1 - x0;
            double dy = y1 - y0;
            double d = Math.Sqrt(dx * dx + dy * dy);
            double sin = dy / d;
            double cos = dx / d;
            Rotate(-sin, cos);
            // reflect about the x axis
            Scale(1, -1);
            // rotate back
            Rotate(sin, cos);
            // translate back
            Translate(x0, y0);
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a reflection
        /// about the line defined by vector (x,y).
        /// </summary>
        /// <remarks>
        /// The transformation for a reflection
        /// is computed by:
        /// <blockquote><code>
        /// d = sqrt(x<sup>2</sup> + y<sup>2</sup>)
        /// sin = x / d;
        /// cos = x / d;
        /// T<sub>ref</sub> = T<sub>rot(sin, cos)</sub> x T<sub>scale(1, -1)</sub> x T<sub>rot(-sin, cos)</sub>
        /// </code></blockquote>
        /// </remarks>
        /// <param name="x"> the x-component of the reflection line vector</param>
        /// <param name="y"> the y-component of the reflection line vector</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToReflection(double x, double y)
        {
            if (x == 0.0 && y == 0.0)
            {
                throw new ArgumentException("Reflection vector must be non-zero");
            }

            /*
             * Handle special case - x = y.
             * This case is specified explicitly to avoid round off error.
             */
            if (x == y)
            {
                _m00 = 0.0;
                _m01 = 1.0;
                _m02 = 0.0;
                _m10 = 1.0;
                _m11 = 0.0;
                _m12 = 0.0;
                return this;
            }

            // rotate vector to positive x axis direction
            double d = Math.Sqrt(x * x + y * y);
            double sin = y / d;
            double cos = x / d;
            Rotate(-sin, cos);
            // reflect about the x-axis
            Scale(1, -1);
            // rotate back
            Rotate(sin, cos);
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a rotation around the orign.
        /// </summary>
        /// <remarks>
        /// A positive rotation angle corresponds
        /// to a counter-clockwise rotation.
        /// The transformation matrix for a rotation
        /// by an angle <c>theta</c>
        /// has the value:
        /// <blockquote><pre>
        /// |  cos(theta)  -sin(theta)   0 |
        /// |  sin(theta)   cos(theta)   0 |
        /// |           0            0   1 |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="theta"> the rotation angle, in radians</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToRotation(double theta)
        {
            SetToRotation(Math.Sin(theta), Math.Cos(theta));
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a rotation around the origin
        /// by specifying the sin and cos of the rotation angle directly.
        /// </summary>
        /// <remarks>
        /// The transformation matrix for the rotation
        /// has the value:
        /// <blockquote><pre>
        /// |  cosTheta  -sinTheta   0 |
        /// |  sinTheta   cosTheta   0 |
        /// |         0          0   1 |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="sinTheta"> the sine of the rotation angle</param>
        /// <param name="cosTheta"> the cosine of the rotation angle</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToRotation(double sinTheta, double cosTheta)
        {
            _m00 = cosTheta; _m01 = -sinTheta; _m02 = 0.0;
            _m10 = sinTheta; _m11 = cosTheta; _m12 = 0.0;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a rotation
        /// around a given point (x,y).
        /// </summary>
        /// <remarks>
        /// A positive rotation angle corresponds
        /// to a counter-clockwise rotation.
        /// The transformation matrix for a rotation
        /// by an angle <paramref name="theta" />
        /// has the value:
        /// <blockquote><pre>
        /// |  cosTheta  -sinTheta   x-x*cos+y*sin |
        /// |  sinTheta   cosTheta   y-x*sin-y*cos |
        /// |           0            0   1 |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="theta"> the rotation angle, in radians</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToRotation(double theta, double x, double y)
        {
            SetToRotation(Math.Sin(theta), Math.Cos(theta), x, y);
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a rotation
        /// around a given point (x,y)
        /// by specifying the sin and cos of the rotation angle directly.
        /// </summary>
        /// <remarks>
        /// The transformation matrix for the rotation
        /// has the value:
        /// <blockquote><pre>
        /// |  cosTheta  -sinTheta   x-x*cos+y*sin |
        /// |  sinTheta   cosTheta   y-x*sin-y*cos |
        /// |         0          0         1       |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="sinTheta"> the sine of the rotation angle</param>
        /// <param name="cosTheta"> the cosine of the rotation angle</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToRotation(double sinTheta, double cosTheta, double x, double y)
        {
            _m00 = cosTheta; _m01 = -sinTheta; _m02 = x - x * cosTheta + y * sinTheta;
            _m10 = sinTheta; _m11 = cosTheta; _m12 = y - x * sinTheta - y * cosTheta;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a scaling.
        /// </summary>
        /// <remarks>
        /// The transformation matrix for a scale
        /// has the value:
        /// <blockquote><pre>
        /// |  xScale      0  dx |
        /// |  0      yScale  dy |
        /// |  0           0   1 |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="xScale"> the amount to scale x-ordinates by</param>
        /// <param name="yScale"> the amount to scale y-ordinates by</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToScale(double xScale, double yScale)
        {
            _m00 = xScale; _m01 = 0.0; _m02 = 0.0;
            _m10 = 0.0; _m11 = yScale; _m12 = 0.0;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a shear.
        /// </summary>
        /// <remarks>
        /// The transformation matrix for a shear
        /// has the value:
        /// <blockquote><pre>
        /// |  1      xShear  0 |
        /// |  yShear      1  0 |
        /// |  0           0  1 |
        /// </pre></blockquote>
        /// Note that a shear of (1, 1) is <i>not</i>
        /// equal to shear(1, 0) composed with shear(0, 1).
        /// Instead, shear(1, 1) corresponds to a mapping onto the
        /// line x = y.
        /// </remarks>
        /// <param name="xShear"> the x component to shear by</param>
        /// <param name="yShear"> the y component to shear by</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToShear(double xShear, double yShear)
        {
            _m00 = 1.0; _m01 = xShear; _m02 = 0.0;
            _m10 = yShear; _m11 = 1.0; _m12 = 0.0;
            return this;
        }

        /// <summary>
        /// Sets this transformation to be a translation.
        /// </summary>
        /// <remarks>
        /// For a translation by the vector (x, y)
        /// the transformation matrix has the value:
        /// <blockquote><pre>
        /// |  1  0  dx |
        /// |  1  0  dy |
        /// |  0  0   1 |
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="dx"> the x component to translate by</param>
        /// <param name="dy"> the y component to translate by</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation SetToTranslation(double dx, double dy)
        {
            _m00 = 1.0; _m01 = 0.0; _m02 = dx;
            _m10 = 0.0; _m11 = 1.0; _m12 = dy;
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a reflection transformation composed
        /// with the current value.
        /// </summary>
        /// <param name="x0"> the x-ordinate of a point on the line to reflect around</param>
        /// <param name="y0"> the y-ordinate of a point on the line to reflect around</param>
        /// <param name="x1"> the x-ordinate of a point on the line to reflect around</param>
        /// <param name="y1"> the y-ordinate of a point on the line to reflect around</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Reflect(double x0, double y0, double x1, double y1)
        {
            Compose(ReflectionInstance(x0, y0, x1, y1));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a reflection transformation composed
        /// with the current value.
        /// </summary>
        /// <param name="x"> the x-ordinate of the line to reflect around</param>
        /// <param name="y"> the y-ordinate of the line to reflect around</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Reflect(double x, double y)
        {
            Compose(ReflectionInstance(x, y));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a rotation transformation composed
        /// with the current value.
        /// </summary>
        /// <remarks>
        /// Positive angles correspond to a rotation
        /// in the counter-clockwise direction.
        /// </remarks>
        /// <param name="theta"> the angle to rotate by in radians</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Rotate(double theta)
        {
            Compose(RotationInstance(theta));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a rotation around the origin composed
        /// with the current value,
        /// with the sin and cos of the rotation angle specified directly.
        /// </summary>
        /// <param name="sinTheta"> the sine of the angle to rotate by</param>
        /// <param name="cosTheta"> the cosine of the angle to rotate by</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Rotate(double sinTheta, double cosTheta)
        {
            Compose(RotationInstance(sinTheta, cosTheta));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a rotation around a given point composed
        /// with the current value.
        /// </summary>
        /// <remarks>
        /// Positive angles correspond to a rotation
        /// in the counter-clockwise direction.
        /// </remarks>
        /// <param name="theta"> the angle to rotate by, in radians</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Rotate(double theta, double x, double y)
        {
            Compose(RotationInstance(theta, x, y));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a rotation around a given point composed
        /// with the current value,
        /// with the sin and cos of the rotation angle specified directly.
        /// </summary>
        /// <param name="sinTheta"> the sine of the angle to rotate by</param>
        /// <param name="cosTheta"> the cosine of the angle to rotate by</param>
        /// <param name="x"> the x-ordinate of the rotation point</param>
        /// <param name="y"> the y-ordinate of the rotation point</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Rotate(double sinTheta, double cosTheta, double x, double y)
        {
            Compose(RotationInstance(sinTheta, cosTheta, x, y));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a scale transformation composed
        /// with the current value.
        /// </summary>
        /// <param name="xScale"> the value to scale by in the x direction</param>
        /// <param name="yScale"> the value to scale by in the y direction</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Scale(double xScale, double yScale)
        {
            Compose(ScaleInstance(xScale, yScale));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a shear transformation composed
        /// with the current value.
        /// </summary>
        /// <param name="xShear"> the value to shear by in the x direction</param>
        /// <param name="yShear"> the value to shear by in the y direction</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Shear(double xShear, double yShear)
        {
            Compose(ShearInstance(xShear, yShear));
            return this;
        }

        /// <summary>
        /// Updates the value of this transformation
        /// to that of a translation transformation composed
        /// with the current value.
        /// </summary>
        /// <param name="x"> the value to translate by in the x direction</param>
        /// <param name="y"> the value to translate by in the y direction</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Translate(double x, double y)
        {
            Compose(TranslationInstance(x, y));
            return this;
        }

        /// <summary>
        /// Updates this transformation to be
        /// the composition of this transformation with the given <see cref="AffineTransformation" />.
        /// </summary>
        /// <remarks>
        /// This produces a transformation whose effect
        /// is equal to applying this transformation
        /// followed by the argument transformation.
        /// Mathematically,
        /// <blockquote><pre>
        /// A.compose(B) = T<sub>B</sub> x T<sub>A</sub>
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="trans"> an affine transformation</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation Compose(AffineTransformation trans)
        {
            double mp00 = trans._m00 * _m00 + trans._m01 * _m10;
            double mp01 = trans._m00 * _m01 + trans._m01 * _m11;
            double mp02 = trans._m00 * _m02 + trans._m01 * _m12 + trans._m02;
            double mp10 = trans._m10 * _m00 + trans._m11 * _m10;
            double mp11 = trans._m10 * _m01 + trans._m11 * _m11;
            double mp12 = trans._m10 * _m02 + trans._m11 * _m12 + trans._m12;
            _m00 = mp00;
            _m01 = mp01;
            _m02 = mp02;
            _m10 = mp10;
            _m11 = mp11;
            _m12 = mp12;
            return this;
        }

        /// <summary>
        /// Updates this transformation to be the composition
        /// of a given <see cref="AffineTransformation" /> with this transformation.
        /// </summary>
        /// <remarks>
        /// This produces a transformation whose effect
        /// is equal to applying the argument transformation
        /// followed by this transformation.
        /// Mathematically,
        /// <blockquote><pre>
        /// A.composeBefore(B) = T<sub>A</sub> x T<sub>B</sub>
        /// </pre></blockquote>
        /// </remarks>
        /// <param name="trans"> an affine transformation</param>
        /// <returns> this transformation, with an updated matrix</returns>
        public AffineTransformation ComposeBefore(AffineTransformation trans)
        {
            double mp00 = _m00 * trans._m00 + _m01 * trans._m10;
            double mp01 = _m00 * trans._m01 + _m01 * trans._m11;
            double mp02 = _m00 * trans._m02 + _m01 * trans._m12 + _m02;
            double mp10 = _m10 * trans._m00 + _m11 * trans._m10;
            double mp11 = _m10 * trans._m01 + _m11 * trans._m11;
            double mp12 = _m10 * trans._m02 + _m11 * trans._m12 + _m12;
            _m00 = mp00;
            _m01 = mp01;
            _m02 = mp02;
            _m10 = mp10;
            _m11 = mp11;
            _m12 = mp12;
            return this;
        }

        /// <summary>
        /// Applies this transformation to the <paramref name="src" /> coordinate
        /// and places the results in the <paramref name="dest" /> coordinate
        /// (which may be the same as the source).
        /// </summary>
        /// <param name="src"> the coordinate to transform</param>
        /// <param name="dest"> the coordinate to accept the results</param>
        /// <returns> the <c>dest</c> coordinate</returns>
        ///
        public Coordinate Transform(Coordinate src, Coordinate dest)
        {
            double xp = _m00 * src.X + _m01 * src.Y + _m02;
            double yp = _m10 * src.X + _m11 * src.Y + _m12;
            dest.X = xp;
            dest.Y = yp;
            return dest;
        }

        /// <summary>
        /// Creates a new <see cref="Geometry"/> which is the result of this transformation applied to the input Geometry.
        /// </summary>
        /// <param name="g">A <c>Geometry</c></param>
        /// <returns>The transformed Geometry</returns>
        public Geometry Transform(Geometry g)
        {
            var g2 = g.Copy();
            g2.Apply(this);
            return g2;
        }

        /// <summary>
        /// Applies this transformation to the i'th coordinate
        /// in the given CoordinateSequence.
        /// </summary>
        /// <param name="seq"> a <c>CoordinateSequence</c></param>
        /// <param name="i"> the index of the coordinate to transform</param>
        public void Transform(CoordinateSequence seq, int i)
        {
            double xp = _m00 * seq.GetOrdinate(i, 0) + _m01 * seq.GetOrdinate(i, 1) + _m02;
            double yp = _m10 * seq.GetOrdinate(i, 0) + _m11 * seq.GetOrdinate(i, 1) + _m12;
            seq.SetOrdinate(i, 0, xp);
            seq.SetOrdinate(i, 1, yp);
        }

        /// <summary>
        /// Transforms the i'th coordinate in the input sequence
        /// </summary>
        /// <param name="seq">A <c>CoordinateSequence</c></param>
        /// <param name="i">The index of the coordinate to transform</param>
        public void Filter(CoordinateSequence seq, int i)
        {
            Transform(seq, i);
        }

        public bool GeometryChanged => true;

        /// <summary>
        /// Reports that this filter should continue to be executed until
        /// all coordinates have been transformed.
        /// </summary>
        /// <returns> false</returns>
        public bool Done => false;

        /// <summary>Tests if this transformation is the identity transformation.</summary>
        public bool IsIdentity => (_m00 == 1 && _m01 == 0 && _m02 == 0
                                      && _m10 == 0 && _m11 == 1 && _m12 == 0);

        /// <summary>
        /// Tests if an object is an <c>AffineTransformation</c> and has the same matrix as this transformation.
        /// </summary>
        /// <param name="obj">An object to test</param>
        /// <returns>true if the given object is equal to this object</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as AffineTransformation);
            /*
            if (!(obj is AffineTransformation))
                return false;

            AffineTransformation trans = (AffineTransformation)obj;
            return m00 == trans.m00
            && m01 == trans.m01
            && m02 == trans.m02
            && m10 == trans.m10
            && m11 == trans.m11
            && m12 == trans.m12;
             */
        }

        /// <inheritdoc cref="object.GetHashCode()"/>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            const int prime = 31;
            long temp = BitConverter.DoubleToInt64Bits(_m00);
            int result = prime + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_m01);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_m02);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_m10);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_m11);
            result = prime * result + (int)(temp ^ (temp >> 32));
            temp = BitConverter.DoubleToInt64Bits(_m12);
            result = prime * result + (int)(temp ^ (temp >> 32));
            return result;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }

        public bool Equals(AffineTransformation trans)
        {
            if (trans == null)
                return false;

            return _m00 == trans._m00
            && _m01 == trans._m01
            && _m02 == trans._m02
            && _m10 == trans._m10
            && _m11 == trans._m11
            && _m12 == trans._m12;
        }

        /// <summary>
        /// Gets a text representation of this transformation.
        /// The string is of the form:
        /// <code>
        /// AffineTransformation[[m00, m01, m02], [m10, m11, m12]]
        /// </code>
        /// </summary>
        /// <returns>A string representing this transformation</returns>
        public override string ToString()
        {
            return "AffineTransformation[[" + _m00 + ", " + _m01 + ", " + _m02
            + "], ["
            + _m10 + ", " + _m11 + ", " + _m12 + "]]";
        }

        /// <summary>
        /// Clones this transformation
        /// </summary>
        /// <returns>A copy of this transformation</returns>
        public object Clone()
        {
            try
            {
                return MemberwiseClone();
            }
            catch (Exception)
            {
                return new AffineTransformation(this);
                //Assert.ShouldNeverReachHere();
            }
        }
    }
}
