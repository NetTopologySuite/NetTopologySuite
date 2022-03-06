using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Samples.Technique
{
    /// <summary>
    /// Utility extension class to change the coordinate dimension of a geometry
    /// </summary>
    /// <remarks>
    /// Some NTS operations accidentally change the coordinate dimension of the resulting
    /// geometry. Use this utility class to enforce original coordinate dimension.
    /// </remarks>
    public static class ChangeGeometryDimension
    {
        /// <summary>
        /// Enforces a coordinate sequence to <b>only</b> contain x- and y-ordinate values
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <param name="geometry">The geometry <paramref name="sequence"/> belongs to.</param>
        /// <returns>A sequence containing x- and y-ordinate values</returns>
        private static CoordinateSequence ChangeToXY(CoordinateSequence sequence, Geometry geometry)
        {
            if (sequence.Dimension == 2)
                return sequence;

            var res = geometry.Factory.CoordinateSequenceFactory.Create(sequence.Count, 2, 0);
            CoordinateSequences.Copy(sequence, 0, res, 0, sequence.Count);

            return res;
        }

        /// <summary>
        /// Enforces a coordinate sequence to <b>only</b> contain x-, y- and z-ordinate values
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <param name="geometry">The geometry <paramref name="sequence"/> belongs to.</param>
        /// <returns>A sequence containing x-, y- and z-ordinate values</returns>
        private static CoordinateSequence ChangeToXYZ(CoordinateSequence sequence, Geometry geometry)
        {
            if (sequence.Dimension == 3 && sequence.Measures == 0)
                return sequence;

            var res = geometry.Factory.CoordinateSequenceFactory.Create(sequence.Count, 3, 0);
            CoordinateSequences.Copy(sequence, 0, res, 0, sequence.Count);

            return res;
        }

        /// <summary>
        /// Enforces a coordinate sequence to <b>only</b> contain x-, y- and m-ordinate values
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <param name="geometry">The geometry <paramref name="sequence"/> belongs to.</param>
        /// <returns>A sequence containing x-, y- and m-ordinate values</returns>
        private static CoordinateSequence ChangeToXYM(CoordinateSequence sequence, Geometry geometry)
        {
            if (sequence.Dimension == 3 && sequence.Measures == 1)
                return sequence;

            var res = geometry.Factory.CoordinateSequenceFactory.Create(sequence.Count, 3, 1);
            CoordinateSequences.Copy(sequence, 0, res, 0, sequence.Count);

            return res;
        }

        /// <summary>
        /// Enforces a coordinate sequence to <b>only</b> contain x-, y-, z- and m-ordinate values
        /// </summary>
        /// <param name="sequence">The sequence</param>
        /// <param name="geometry">The geometry <paramref name="sequence"/> belongs to.</param>
        /// <returns>A sequence containing x-, y-, z- and m-ordinate values</returns>
        private static CoordinateSequence ChangeToXYZM(CoordinateSequence sequence, Geometry geometry)
        {
            if (sequence.Dimension == 4 && sequence.Measures == 1)
                return sequence;

            var res = geometry.Factory.CoordinateSequenceFactory.Create(sequence.Count, 4, 1);
            CoordinateSequences.Copy(sequence, 0, res, 0, sequence.Count);

            return res;
        }

        /// <summary>
        /// Changes the coordinate dimension of the provided <paramref name="geometry"/> to the given <paramref name="ordinates"/> set.
        /// </summary>
        /// <remarks>
        /// The allowed values for <paramref name="ordinates"/> are:
        /// <list type="bullet">
        /// <item><description><see cref="Ordinates.XY"/></description></item>
        /// <item><description><see cref="Ordinates.XYZ"/></description></item>
        /// <item><description><see cref="Ordinates.XYM"/><description></item>
        /// <item><description><see cref="Ordinates.XYZM"/></description></item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T">The type of the geometry</typeparam>
        /// <param name="geometry">The geometry to change or assert a specific coordinate dimension for</param>
        /// <param name="ordinates">The ordinate set wanted.</param>
        /// <returns>A geometry with the specified coordinate dimension</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the provided <paramref name="ordinates"/> set is not supported</exception>
        public static T ChangeDimension<T>(this T geometry, Ordinates ordinates) where T: Geometry
        {
            if (geometry == null)
                return null;

            Func<CoordinateSequence, Geometry, CoordinateSequence> fn = null;
            switch (ordinates)
            {
                case Ordinates.XY:
                    fn = ChangeToXY;
                    break;
                case Ordinates.XYZ:
                    fn = ChangeToXYZ;
                    break;
                case Ordinates.XYM:
                    fn = ChangeToXYM;
                    break;
                case Ordinates.XYZM:
                    fn = ChangeToXYZM;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ordinates));
            }

            var ge = new GeometryEditor(geometry.Factory) { CopyUserData = true };
            var op = new GeometryEditor.CoordinateSequenceOperation(fn);
            return (T)ge.Edit(geometry, op);
        }
    }

    /// <summary>
    /// Test cases
    /// </summary>
    public class ChangeDimensionUtilityTest
    {
        [Test]
        public void Test()
        {
            var rdr = new IO.WKTReader();
            var pt = (Point)rdr.Read("POINT ZM (1 2 3 4)");
            Assert.That(pt.CoordinateSequence.Dimension, Is.EqualTo(4));
            Assert.That(pt.CoordinateSequence.Measures, Is.EqualTo(1));

            var ptXY = pt.ChangeDimension(Ordinates.XY);
            Assert.That(ptXY.CoordinateSequence.Dimension, Is.EqualTo(2));
            Assert.That(ptXY.CoordinateSequence.Measures, Is.EqualTo(0));
            Assert.That(ptXY.X, Is.EqualTo(1d));
            Assert.That(ptXY.Y, Is.EqualTo(2d));

            var ptXYZ = pt.ChangeDimension(Ordinates.XYZ);
            Assert.That(ptXYZ.CoordinateSequence.Dimension, Is.EqualTo(3));
            Assert.That(ptXYZ.CoordinateSequence.Measures, Is.EqualTo(0));
            Assert.That(ptXYZ.X, Is.EqualTo(1d));
            Assert.That(ptXYZ.Y, Is.EqualTo(2d));
            Assert.That(ptXYZ.Z, Is.EqualTo(3d));

            var ptXYM = pt.ChangeDimension(Ordinates.XYM);
            Assert.That(ptXYM.CoordinateSequence.Dimension, Is.EqualTo(3));
            Assert.That(ptXYM.CoordinateSequence.Measures, Is.EqualTo(1));
            Assert.That(ptXYM.X, Is.EqualTo(1d));
            Assert.That(ptXYM.Y, Is.EqualTo(2d));
            Assert.That(ptXYM.M, Is.EqualTo(4d));

            var ptXYZM = pt.ChangeDimension(Ordinates.XYZM);
            Assert.That(ptXYZM.CoordinateSequence.Dimension, Is.EqualTo(4));
            Assert.That(ptXYZM.CoordinateSequence.Measures, Is.EqualTo(1));
            Assert.That(ptXYZM.X, Is.EqualTo(1d));
            Assert.That(ptXYZM.Y, Is.EqualTo(2d));
            Assert.That(ptXYZM.Z, Is.EqualTo(3d));
            Assert.That(ptXYZM.M, Is.EqualTo(4d));
            Assert.That(ptXYZM, Is.EqualTo(pt));
        }
    }

}
