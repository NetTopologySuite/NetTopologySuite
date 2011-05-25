using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Utilities
{
    /**
     * Represents a affine transformation on the 2D Cartesian plane. 
     * It can be used to transform a {@link Coordinate} or {@link Geometry}.
     * An affine transformation is a mapping of the 2D plane into itself
     * via a series of transformations of the following basic types:
     * <ul>
     * <li>reflection (through a line)
     * <li>rotation (around the origin)
     * <li>scaling (relative to the origin)
     * <li>shearing (in both the X and Y directions)
     * <li>translation 
     * </ul>
     * In general, affine transformations preserve straightness and parallel lines,
     * but do not preserve distance or shape.
     * <p>
     * An affine transformation can be represented by a 3x3 
     * matrix in the following form:
     * <blockquote><pre>
     * T = | m00 m01 m02 |
     *     | m10 m11 m12 |
     *     |  0   0   1  |
     * </pre></blockquote>
     * A coordinate P = (x, y) can be transformed to a new coordinate P' = (x', y')
     * by representing it as a 3x1 matrix and using matrix multiplication to compute:
     * <blockquote><pre>
     * | x' |  = T x | x |
     * | y' |        | y |
     * | 1  |        | 1 |
     * </pre></blockquote>
     * Affine transformations can be composed using the {@link #compose} method.
     * The composition of transformations is in general not commutative.
     * transformation matrices as follows:
     * <blockquote><pre>
     * A.compose(B) = T<sub>B</sub> x T<sub>A</sub>
     * </pre></blockquote>
     * This produces a transformation whose effect is that of A followed by B.
     * Composition is computed via multiplication of the 
     * The methods {@link #reflect}, {@link #rotate}, {@link #scale}, {@link #shear}, and {@link #translate} 
     * have the effect of composing a transformation of that type with
     * the transformation they are invoked on.  
     * <p>
     * Affine transformations may be invertible or non-invertible.  
     * If a transformation is invertible, then there exists 
     * an inverse transformation which when composed produces 
     * the identity transformation.  
     * The {@link #getInverse} method
     * computes the inverse of a transformation, if one exists.
     * 
     * @author Martin Davis
     *
     */
    public class AffineTransformation : ICloneable, ICoordinateSequenceFilter, IEquatable<AffineTransformation>
    {

        /**
         * Creates a transformation for a reflection about the 
         * line (x0,y0) - (x1,y1).
         * 
         * @param x0 the x-ordinate of a point on the reflection line
         * @param y0 the y-ordinate of a point on the reflection line
         * @param x1 the x-ordinate of a another point on the reflection line
         * @param y1 the y-ordinate of a another point on the reflection line
         * @return a transformation for the reflection
         */
        public static AffineTransformation ReflectionInstance(double x0, double y0, double x1, double y1)
        {
            AffineTransformation trans = new AffineTransformation();
            trans.SetToReflection(x0, y0, x1, y1);
            return trans;
        }

        /**
         * Creates a transformation for a reflection about the 
         * line (0,0) - (x,y).
         * 
         * @param x the x-ordinate of a point on the reflection line
         * @param y the y-ordinate of a point on the reflection line
         * @return a transformation for the reflection
         */
        public static AffineTransformation ReflectionInstance(double x, double y)
        {
            AffineTransformation trans = new AffineTransformation();
            trans.SetToReflection(x, y);
            return trans;
        }

        /**
         * Creates a transformation for a rotation
         * about the origin 
         * by an angle <i>theta</i>.
         * Positive angles correspond to a rotation 
         * in the counter-clockwise direction.
         * 
         * @param theta the rotation angle, in radians
         * @return a transformation for the rotation
         */
        public static AffineTransformation RotationInstance(double theta)
        {
            return RotationInstance(Math.Sin(theta), Math.Cos(theta));
        }

        /**
         * Creates a transformation for a rotation 
         * by an angle <i>theta</i>,
         * specified by the sine and cosine of the angle.
         * This allows providing exact values for sin(theta) and cos(theta)
         * for the common case of rotations of multiples of quarter-circles. 
         * 
         * @param sinTheta the sine of the rotation angle
         * @param cosTheta the cosine of the rotation angle
         * @return a transformation for the rotation
         */
        public static AffineTransformation RotationInstance(double sinTheta, double cosTheta)
        {
            AffineTransformation trans = new AffineTransformation();
            trans.SetToRotation(sinTheta, cosTheta);
            return trans;
        }

        public static AffineTransformation ScaleInstance(double xScale, double yScale)
        {
            AffineTransformation trans = new AffineTransformation();
            trans.SetToScale(xScale, yScale);
            return trans;
        }

        public static AffineTransformation ShearInstance(double xShear, double yShear)
        {
            AffineTransformation trans = new AffineTransformation();
            trans.SetToShear(xShear, yShear);
            return trans;
        }

        public static AffineTransformation TranslationInstance(double x, double y)
        {
            AffineTransformation trans = new AffineTransformation();
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

        /**
         * Constructs a new identity transformation
         *
         */
        public AffineTransformation()
        {
            SetToIdentity();
        }

        /**
         * Constructs a new transformation whose 
         * matrix has the specified values.
         * 
         * @param matrix an array containing the 6 values { m00, m01, m02, m10, m11, m12 }
         * @throws NullPointerException if matrix is null
         * @throws ArrayIndexOutOfBoundsException if matrix is too small 
         */
        public AffineTransformation(double[] matrix)
        {
            _m00 = matrix[0];
            _m01 = matrix[1];
            _m02 = matrix[2];
            _m10 = matrix[3];
            _m11 = matrix[4];
            _m12 = matrix[5];
        }

        /**
         * Constructs a new transformation whose 
         * matrix has the specified values.
         * 
         * @param m00 the entry for the [0, 0] element in the transformation matrix 
         * @param m01 the entry for the [0, 1] element in the transformation matrix
         * @param m02 the entry for the [0, 2] element in the transformation matrix
         * @param m10 the entry for the [1, 0] element in the transformation matrix
         * @param m11 the entry for the [1, 1] element in the transformation matrix
         * @param m12 the entry for the [1, 2] element in the transformation matrix
         */
        public AffineTransformation(double m00,
            double m01,
            double m02,
            double m10,
            double m11,
            double m12)
        {
            SetTransformation(m00, m01, m02, m10, m11, m12);
        }

        /**
         * Constructs a transformation which is
         * a copy of the given one.
         * 
         * @param trans the transformation to copy
         */
        public AffineTransformation(AffineTransformation trans)
        {
            SetTransformation(trans);
        }

        /**
         * Constructs a transformation
         * which maps the given source
         * points into the given destination points.
         * 
         * @param src0 source point 0
         * @param src1 source point 1
         * @param src2 source point 2
         * @param dest0 the mapped point for source point 0
         * @param dest1 the mapped point for source point 1
         * @param dest2 the mapped point for source point 2
         * 
         */
        public AffineTransformation(ICoordinate src0,
            ICoordinate src1,
            ICoordinate src2,
            ICoordinate dest0,
            ICoordinate dest1,
            ICoordinate dest2)
            : this(new AffineTransformationBuilder(src0, src1, src2, dest0, dest1, dest2).GetTransformation())
        {
        }

        /**
         * Sets this transformation to be the identity transformation.
         * The identity transformation has the matrix:
         * <blockquote><pre>
         * | 1 0 0 |
         * | 0 1 0 |
         * | 0 0 1 |
         * </pre></blockquote>
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToIdentity()
        {
            _m00 = 1.0; _m01 = 0.0; _m02 = 0.0;
            _m10 = 0.0; _m11 = 1.0; _m12 = 0.0;
            return this;
        }

        /**
         * Sets this transformation's matrix to have the given values.
         * 
         * @param m00 the entry for the [0, 0] element in the transformation matrix 
         * @param m01 the entry for the [0, 1] element in the transformation matrix
         * @param m02 the entry for the [0, 2] element in the transformation matrix
         * @param m10 the entry for the [1, 0] element in the transformation matrix
         * @param m11 the entry for the [1, 1] element in the transformation matrix
         * @param m12 the entry for the [1, 2] element in the transformation matrix
         * @return this transformation, with an updated matrix
         */
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

        /**
         * Sets this transformation to be a copy of the given one
         * 
         * @param trans a transformation to copy
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetTransformation(AffineTransformation trans)
        {
            _m00 = trans._m00; _m01 = trans._m01; _m02 = trans._m02;
            _m10 = trans._m10; _m11 = trans._m11; _m12 = trans._m12;
            return this;
        }

        /**
         * Gets an array containing the entries
         * of the transformation matrix.
         * Only the 6 non-trivial entries are returned,
         * in the sequence:
         * <pre>
         * m00, m01, m02, m10, m11, m12
         * </pre>
         * 
         * @return an array of length 6
         */
        public double[] MatrixEntries
        {
            get
            {
                return new[] {_m00, _m01, _m02, _m10, _m11, _m12};
            }
        }

        /**
        * Computes the determinant of the transformation matrix. 
        * The determinant is computed as:
        * <blockquote><pre>
        * | m00 m01 m02 |
        * | m10 m11 m12 | = m00 * m11 - m01 * m10
        * |  0   0   1  |
        * </pre></blockquote>
        * If the determinant is zero, 
        * the transform is singular (not invertible), 
        * and operations which attempt to compute
        * an inverse will throw a <tt>NoninvertibleTransformException</tt>. 

        * @return the determinant of the transformation
        * @see #getInverse()
        */
        ///<returns>The determinant of the transformation</returns>
        ///<see cref="GetInverse()"/>
        public double Determinant
        {
            get { return _m00*_m11 - _m01*_m10; }
        }

        /**
         * Computes the inverse of this transformation, if one
         * exists.
         * The inverse is the transformation which when 
         * composed with this one produces the identity 
         * transformation.
         * A transformation has an inverse if and only if it
         * is not singular (i.e. its
         * determinant is non-zero).
         * Geometrically, an transformation is non-invertible
         * if it maps the plane to a line or a point.
         * If no inverse exists this method
         * will throw a <tt>NoninvertibleTransformationException</tt>.
         * <p>
         * The matrix of the inverse is equal to the 
         * inverse of the matrix for the transformation.
         * It is computed as follows:
         * <blockquote><pre>  
         *                 1    
         * inverse(A)  =  ---   x  adjoint(A) 
         *                det 
         *
         *
         *             =   1       |  m11  -m01   m01*m12-m02*m11  |
         *                ---   x  | -m10   m00  -m00*m12+m10*m02  |
         *                det      |  0     0     m00*m11-m10*m01  |
         *
         *
         *
         *             = |  m11/det  -m01/det   m01*m12-m02*m11/det |
         *               | -m10/det   m00/det  -m00*m12+m10*m02/det |
         *               |   0           0          1               |
         *
         * </pre></blockquote>  
         *  
         * @return a new inverse transformation
         * @throws NoninvertibleTransformationException
         * @see #getDeterminant()
         */
        ///<returns>A new inverse transformation</returns>
        ///<see cref="Determinant"/>
        ///<exception cref="NoninvertibleTransformationException"></exception>
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

        ///<summary>
        /// Explicitly computes the math for a reflection.  May not work.
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
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

        /**
         * Sets this transformation to be a reflection 
         * about the line defined by vector (x,y).
         * The transformation for a reflection
         * is computed by:
         * <blockquote><pre>
         * d = sqrt(x<sup>2</sup> + y<sup>2</sup>)  
         * sin = x / d;
         * cos = x / d;
         * 
         * T<sub>ref</sub> = T<sub>rot(sin, cos)</sub> x T<sub>scale(1, -1)</sub> x T<sub>rot(-sin, cos)</sub  
         * </pre></blockquote> 
         * 
         * @param x the x-component of the reflection line vector
         * @param y the y-component of the reflection line vector
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToReflection(double x, double y)
        {
            if (x == 0.0 && y == 0.0)
            {
                throw new ArgumentException("Reflection vector must be non-zero");
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

        /**
         * Sets this transformation to be a rotation.
         * A positive rotation angle corresponds 
         * to a counter-clockwise rotation.
         * The transformation matrix for a rotation
         * by an angle <tt>theta</tt>
         * has the value:
         * <blockquote><pre>  
         * |  cos(theta)  -sin(theta)   0 |
         * |  sin(theta)   cos(theta)   0 |
         * |           0            0   1 |
         * </pre></blockquote> 
         * 
         * @param theta the rotation angle, in radians
         * @return this transformation, with an updated matrix
         */
        private AffineTransformation SetToRotation(double theta)
        {
            SetToRotation(Math.Sin(theta), Math.Cos(theta));
            return this;
        }

        /**
         * Sets this transformation to be a rotation 
         * by specifying the sin and cos of the rotation angle directly.
         * The transformation matrix for the rotation
         * has the value:
         * <blockquote><pre>  
         * |  cosTheta  -sinTheta   0 |
         * |  sinTheta   cosTheta   0 |
         * |         0          0   1 |
         * </pre></blockquote> 
         * 
         * @param sinTheta the sine of the rotation angle
         * @param cosTheta the cosine of the rotation angle
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToRotation(double sinTheta, double cosTheta)
        {
            _m00 = cosTheta; _m01 = -sinTheta; _m02 = 0.0;
            _m10 = sinTheta; _m11 = cosTheta; _m12 = 0.0;
            return this;
        }

        /**
         * Sets this transformation to be a scaling.
         * The transformation matrix for a scale
         * has the value:
         * <blockquote><pre>  
         * |  xScale      0  dx |
         * |  1      yScale  dy |
         * |  0           0   1 |
         * </pre></blockquote> 
         * 
         * @param xScale the amount to scale x-ordinates by
         * @param yScale the amount to scale y-ordinates by
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToScale(double xScale, double yScale)
        {
            _m00 = xScale; _m01 = 0.0; _m02 = 0.0;
            _m10 = 0.0; _m11 = yScale; _m12 = 0.0;
            return this;
        }

        /**
         * Sets this transformation to be a shear.
         * The transformation matrix for a shear 
         * has the value:
         * <blockquote><pre>  
         * |  1      xShear  0 |
         * |  yShear      1  0 |
         * |  0           0  1 |
         * </pre></blockquote> 
         * Note that a shear of (1, 1) is <i>not</i> 
         * equal to shear(1, 0) composed with shear(0, 1).
         * Instead, shear(1, 1) corresponds to a mapping onto the 
         * line x = y.
         * 
         * @param xShear the x component to shear by
         * @param yShear the y component to shear by
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToShear(double xShear, double yShear)
        {
            _m00 = 1.0; _m01 = xShear; _m02 = 0.0;
            _m10 = yShear; _m11 = 1.0; _m12 = 0.0;
            return this;
        }

        /**
         * Sets this transformation to be a translation.
         * For a translation by the vector (x, y)
         * the transformation matrix has the value:
         * <blockquote><pre>  
         * |  1  0  dx |
         * |  1  0  dy |
         * |  0  0   1 |
         * </pre></blockquote> 
         * @param dx the x component to translate by
         * @param dy the y component to translate by
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation SetToTranslation(double dx, double dy)
        {
            _m00 = 1.0; _m01 = 0.0; _m02 = dx;
            _m10 = 0.0; _m11 = 1.0; _m12 = dy;
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a reflection transformation composed 
         * with the current value.
         * 
         * @param x0 the x-ordinate of a point on the line to reflect around
         * @param y0 the y-ordinate of a point on the line to reflect around
         * @param x1 the x-ordinate of a point on the line to reflect around
         * @param y1 the y-ordinate of a point on the line to reflect around
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Reflect(double x0, double y0, double x1, double y1)
        {
            Compose(ReflectionInstance(x0, y0, x1, y1));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a reflection transformation composed 
         * with the current value.
         * 
         * @param x the x-ordinate of the line to reflect around
         * @param y the y-ordinate of the line to reflect around
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Reflect(double x, double y)
        {
            Compose(ReflectionInstance(x, y));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a rotation transformation composed 
         * with the current value.
         * 
         * @param theta the angle to rotate by
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Rotate(double theta)
        {
            Compose(RotationInstance(theta));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a rotation transformation composed 
         * with the current value.
         * 
         * @param sinTheta the sine of the angle to rotate by
         * @param cosTheta the cosine of the angle to rotate by
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Rotate(double sinTheta, double cosTheta)
        {
            Compose(RotationInstance(sinTheta, cosTheta));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a scale transformation composed 
         * with the current value.
         * 
         * @param xScale the value to scale by in the x direction
         * @param yScale the value to scale by in the y direction
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Scale(double xScale, double yScale)
        {
            Compose(ScaleInstance(xScale, yScale));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a shear transformation composed 
         * with the current value.
         * 
         * @param xShear the value to shear by in the x direction
         * @param yShear the value to shear by in the y direction
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Shear(double xShear, double yShear)
        {
            Compose(ShearInstance(xShear, yShear));
            return this;
        }

        /**
         * Updates the value of this transformation
         * to that of a translation transformation composed 
         * with the current value.
         * 
         * @param x the value to translate by in the x direction
         * @param y the value to translate by in the y direction
         * @return this transformation, with an updated matrix
         */
        public AffineTransformation Translate(double x, double y)
        {
            Compose(TranslationInstance(x, y));
            return this;
        }


        /**
         * Composes the given {@link AffineTransformation} 
         * with this transformation.
         * This produces a transformation whose effect 
         * is equal to applying this transformation 
         * followed by the argument transformation.
         * Mathematically,
         * <blockquote><pre>
         * A.compose(B) = T<sub>B</sub> x T<sub>A</sub>
         * </pre></blockquote>
         * 
         * @param trans an affine transformation
         * @return this transformation, with an updated matrix
         */
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

        /**
         * Composes this transformation 
         * with the given {@link AffineTransformation}.
         * This produces a transformation whose effect 
         * is equal to applying the argument transformation 
         * followed by this transformation.
         * Mathematically,
         * <blockquote><pre>
         * A.composeBefore(B) = T<sub>A</sub> x T<sub>B</sub>
         * </pre></blockquote>
         * 
         * @param trans an affine transformation
         * @return this transformation, with an updated matrix
         */
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

        /**
         * Applies this transformation to the <tt>src</tt> coordinate
         * and places the results in the <tt>dest</tt> coordinate
         * (which may be the same as the source).
         * 
         * @param src the coordinate to transform
         * @param dest the coordinate to accept the results 
         * @return the <tt>dest</tt> coordinate
         */
        public ICoordinate Transform(ICoordinate src, ICoordinate dest)
        {
            double xp = _m00 * src.X + _m01 * src.Y + _m02;
            double yp = _m10 * src.X + _m11 * src.Y + _m12;
            dest.X = xp;
            dest.Y = yp;
            return dest;
        }

        /**
         * Applies this transformation to the i'th coordinate
         * in the given CoordinateSequence.
         * 
         *@param seq  a <code>CoordinateSequence</code>
         *@param i the index of the coordinate to transform
         */
        public void Transform(ICoordinateSequence seq, int i)
        {
            double xp = _m00 * seq.GetOrdinate(i, Ordinates.X) + _m01 * seq.GetOrdinate(i, Ordinates.Y) + _m02;
            double yp = _m10 * seq.GetOrdinate(i, Ordinates.X) + _m11 * seq.GetOrdinate(i, Ordinates.Y) + _m12;
            seq.SetOrdinate(i, Ordinates.X, xp);
            seq.SetOrdinate(i, Ordinates.Y, yp);
        }

        /**
         * 
         * 
         *@param seq  
         *@param i 
         */
        ///<summary>
        /// Transforms the i'th coordinate in the input sequence
        ///</summary>
        /// <param name="seq">A <c>CoordinateSequence</c></param>
        /// <param name="i">The index of the coordinate to transform</param>
        public void Filter(ICoordinateSequence seq, int i)
        {
            Transform(seq, i);
        }

        public Boolean GeometryChanged
        {
            get { return true; }
        }

        /**
         * Reports that this filter should continue to be executed until 
         * all coordinates have been transformed.
         * 
         * @return false
         */
        public Boolean Done
        {
            get { return false; }
        }

        ///<summary>Tests if this transformation is the identity transformation.</summary>
        public Boolean IsIdentity
        {
            get
            {
                return (_m00 == 1 && _m01 == 0 && _m02 == 0
                        && _m10 == 0 && _m11 == 1 && _m12 == 0);
            }
        }

        ///<summary>
        /// Tests if an object is an <c>AffineTransformation</c> and has the same matrix as this transformation.
        ///</summary>
        /// <param name="obj">An object to test</param>
        /// <returns>true if the given object is equal to this object</returns>
        public override Boolean Equals(Object obj)
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

        ///<summary>
        /// Gets a text representation of this transformation.
        /// The string is of the form:
        /// <code>
        /// AffineTransformation[[m00, m01, m02], [m10, m11, m12]]
        /// </code>
        /// </summary>
        ///<returns>A string representing this transformation</returns>
        public override String ToString()
        {
            return "AffineTransformation[[" + _m00 + ", " + _m01 + ", " + _m02
            + "], ["
            + _m10 + ", " + _m11 + ", " + _m12 + "]]";
        }

        ///<summary>
        /// Clones this transformation
        ///</summary>
        /// <returns>A copy of this transformation</returns>
        public object Clone()
        {
            return new AffineTransformation(this);
        }
    }
}