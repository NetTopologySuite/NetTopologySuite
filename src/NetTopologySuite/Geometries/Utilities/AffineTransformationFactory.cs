using System;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries.Utilities
{
    /// <summary>
    /// Supports creating <see cref="AffineTransformation"/>s defined by various kinds of inputs and transformation mapping rules.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class AffineTransformationFactory
    {
        /// <summary>
        /// Creates a transformation from a set of three control vectors. A control
        /// vector consists of a source point and a destination point, which is the
        /// image of the source point under the desired transformation. Three control
        /// vectors allows defining a fully general affine transformation.
        /// </summary>
        /// <param name="src0"></param>
        /// <param name="src1"></param>
        /// <param name="src2"></param>
        /// <param name="dest0"></param>
        /// <param name="dest1"></param>
        /// <param name="dest2"></param>
        /// <returns>The computed transformation</returns>
        public static AffineTransformation CreateFromControlVectors(Coordinate src0,
                Coordinate src1, Coordinate src2, Coordinate dest0, Coordinate dest1,
                Coordinate dest2)
        {
            var builder = new AffineTransformationBuilder(src0,
                    src1, src2, dest0, dest1, dest2);
            return builder.GetTransformation();
        }

        /// <summary>
        /// Creates an AffineTransformation defined by a pair of control vectors. A
        /// control vector consists of a source point and a destination point, which is
        /// the image of the source point under the desired transformation. The
        /// computed transformation is a combination of one or more of a uniform scale,
        /// a rotation, and a translation (i.e. there is no shear component and no
        /// reflection)
        /// </summary>
        /// <param name="src0"></param>
        /// <param name="src1"></param>
        /// <param name="dest0"></param>
        /// <param name="dest1"></param>
        /// <returns>The computed transformation</returns>
        /// <returns><c>null</c> if the control vectors do not determine a well-defined transformation</returns>
        public static AffineTransformation CreateFromControlVectors(Coordinate src0,
                Coordinate src1, Coordinate dest0, Coordinate dest1)
        {
            var rotPt = new Coordinate(dest1.X - dest0.X, dest1.Y - dest0.Y);

            double ang = AngleUtility.AngleBetweenOriented(src1, src0, rotPt);

            double srcDist = src1.Distance(src0);
            double destDist = dest1.Distance(dest0);

            if (srcDist == 0.0)
                return null;

            double scale = destDist / srcDist;

            var trans = AffineTransformation.TranslationInstance(
                    -src0.X, -src0.Y);
            trans.Rotate(ang);
            trans.Scale(scale, scale);
            trans.Translate(dest0.X, dest0.Y);
            return trans;
        }

        /// <summary>
        /// Creates an AffineTransformation defined by a single control vector. A
        /// control vector consists of a source point and a destination point, which is
        /// the image of the source point under the desired transformation. This
        /// produces a translation.
        /// </summary>
        /// <param name="src0">The start point of the control vector</param>
        /// <param name="dest0">The end point of the control vector</param>
        /// <returns>The computed transformation</returns>
        public static AffineTransformation CreateFromControlVectors(Coordinate src0,
                Coordinate dest0)
        {
            double dx = dest0.X - src0.X;
            double dy = dest0.Y - src0.Y;
            return AffineTransformation.TranslationInstance(dx, dy);
        }

        /// <summary>
        /// Creates an AffineTransformation defined by a set of control vectors.
        /// Between one and three vectors must be supplied.
        /// </summary>
        /// <param name="src">The source points of the vectors</param>
        /// <param name="dest">The destination points of the vectors</param>
        /// <returns>The computed transformation</returns>
        /// <exception cref="ArgumentException">if the control vector arrays are too short, long or of different lengths</exception>
        public static AffineTransformation CreateFromControlVectors(Coordinate[] src,
                Coordinate[] dest)
        {
            if (src.Length != dest.Length)
                throw new ArgumentException(
                        "Src and Dest arrays are not the same length");
            if (src.Length <= 0)
                throw new ArgumentException("Too few control points");
            if (src.Length > 3)
                throw new ArgumentException("Too many control points");

            if (src.Length == 1)
                return CreateFromControlVectors(src[0], dest[0]);
            if (src.Length == 2)
                return CreateFromControlVectors(src[0], src[1], dest[0], dest[1]);

            return CreateFromControlVectors(src[0], src[1], src[2], dest[0], dest[1],
                    dest[2]);
        }

        /// <summary>
        /// Creates an AffineTransformation defined by a mapping between two baselines.
        /// The computed transformation consists of:
        /// <list type="bullet">
        /// <item><description>a translation from the start point of the source baseline to the start point of the destination baseline,</description></item>
        /// <item><description>a rotation through the angle between the baselines about the destination start point,</description></item>
        /// <item><description>and a scaling equal to the ratio of the baseline lengths.</description></item>
        /// </list>
        /// If the source baseline has zero length, an identity transformation is returned.
        /// </summary>
        /// <param name="src0">The start point of the source baseline</param>
        /// <param name="src1">The end point of the source baseline</param>
        /// <param name="dest0">The start point of the destination baseline</param>
        /// <param name="dest1">The end point of the destination baseline</param>
        /// <returns></returns>
        public static AffineTransformation CreateFromBaseLines(
                Coordinate src0, Coordinate src1,
                Coordinate dest0, Coordinate dest1)
        {
            var rotPt = new Coordinate(src0.X + dest1.X - dest0.X, src0.Y + dest1.Y - dest0.Y);

            double ang = AngleUtility.AngleBetweenOriented(src1, src0, rotPt);

            double srcDist = src1.Distance(src0);
            double destDist = dest1.Distance(dest0);

            // return identity if transformation would be degenerate
            if (srcDist == 0.0)
                return new AffineTransformation();

            double scale = destDist / srcDist;

            var trans = AffineTransformation.TranslationInstance(
                    -src0.X, -src0.Y);
            trans.Rotate(ang);
            trans.Scale(scale, scale);
            trans.Translate(dest0.X, dest0.Y);
            return trans;
        }

    }
}
