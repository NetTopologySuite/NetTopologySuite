using System;

namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence factory class that creates DotSpatial's Shape/ShapeRange like coordinate sequences.
    /// </summary>
    [Serializable]
    public class DotSpatialAffineCoordinateSequenceFactory : CoordinateSequenceFactory
    {
        private DotSpatialAffineCoordinateSequenceFactory() : this(Ordinates.XYZM) { }

        public DotSpatialAffineCoordinateSequenceFactory(Ordinates ordinates)
            : base(ordinates & Ordinates.XYZM)
        {
        }

        /// <summary>
        /// Returns the singleton instance of DotSpatialAffineCoordinateSequenceFactory.
        /// </summary>
        /// <returns></returns>
        public static DotSpatialAffineCoordinateSequenceFactory Instance { get; } = new DotSpatialAffineCoordinateSequenceFactory();

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public override CoordinateSequence Create(Coordinate[] coordinates)
        {
            return new DotSpatialAffineCoordinateSequence(coordinates, Ordinates);
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" />  which is a copy
        /// of the given <see cref="CoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns>A coordinate sequence</returns>
        public override CoordinateSequence Create(CoordinateSequence coordSeq)
        {
            return new DotSpatialAffineCoordinateSequence(coordSeq, Ordinates);
        }

        /// <inheritdoc />
        public override CoordinateSequence Create(int size, int dimension, int measures)
        {
            return new DotSpatialAffineCoordinateSequence(size, dimension, measures);
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="CoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size">The number of coordinates.</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="Geometries.Ordinates.XY"/> is fix,
        /// <see cref="Geometries.Ordinates.Z"/> and <see cref="Geometries.Ordinates.M"/> can be set.
        /// </param>
        /// <returns>A coordinate sequence.</returns>
        public override CoordinateSequence Create(int size, Ordinates ordinates)
        {
            return new DotSpatialAffineCoordinateSequence(size, Ordinates & ordinates);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <returns>A coordinate sequence</returns>
        public CoordinateSequence Create(double[] xy)
        {
            return new DotSpatialAffineCoordinateSequence(xy, null, null);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates,
        /// the <paramref name="zm"/> array for either z-ordinates or measure values. This is indicated by <paramref name="isMeasure"/>.
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <param name="zm">An array of z- or measure values</param>
        /// <param name="isMeasure">A value indicating if <paramref name="zm"/> contains z-ordinates or measure values.</param>
        /// <returns>A coordinate sequence</returns>
        public CoordinateSequence Create(double[] xy, double[] zm, bool isMeasure = false)
        {
            return new DotSpatialAffineCoordinateSequence(xy, isMeasure ? null : zm, isMeasure ? zm : null);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates,
        /// the <paramref name="z"/> array for z ordinates and <paramref name="m"/> for measure values.
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <param name="z">An array of z- or measure values</param>
        /// <param name="m">An array of measure values.</param>
        /// <returns>A coordinate sequence</returns>

        public CoordinateSequence Create(double[] xy, double[] z, double[] m)
        {
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }
    }
}
