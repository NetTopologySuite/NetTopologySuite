using System;
using GeoAPI.Geometries;
namespace NetTopologySuite.Geometries.Implementation
{
    /// <summary>
    /// A coordinate sequence factory class that creates DotSpatial's Shape/ShapeRange like coordinate sequences.
    /// </summary>
#if HAS_SYSTEM_SERIALIZABLEATTRIBUTE
    [Serializable]
#endif
    public class DotSpatialAffineCoordinateSequenceFactory : ICoordinateSequenceFactory
    {
        private static DotSpatialAffineCoordinateSequenceFactory _instance;
        private static readonly object InstanceLock = new object();
        private readonly Ordinates _ordinates;

        private DotSpatialAffineCoordinateSequenceFactory()
            :this(Ordinates.XYZM)
        {
        }

        public DotSpatialAffineCoordinateSequenceFactory(Ordinates ordinates)
        {
            _ordinates = Ordinates.XY | ordinates;
        }

        /// <summary>
        /// Returns the singleton instance of DotSpatialAffineCoordinateSequenceFactory.
        /// </summary>
        /// <returns></returns>
        public static DotSpatialAffineCoordinateSequenceFactory Instance
        {
            get
            {
                lock(InstanceLock)
                {
                    return _instance ?? (_instance = new DotSpatialAffineCoordinateSequenceFactory());
                }
            }
            set => _instance = value;
        }

        /// <summary>
        ///  Returns a CoordinateArraySequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">the coordinates, which may not be null nor contain null elements.</param>
        /// <returns></returns>
        public ICoordinateSequence Create(Coordinate[] coordinates)
        {
            return new DotSpatialAffineCoordinateSequence(coordinates, Ordinates);
        }

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" />  which is a copy
        /// of the given <see cref="ICoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence Create(ICoordinateSequence coordSeq)
        {
            return new DotSpatialAffineCoordinateSequence(coordSeq, Ordinates);
        }

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and dimension.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension">the dimension of the coordinates in the sequence
        /// (if user-specifiable, otherwise ignored)</param>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence Create(int size, int dimension)
        {

            return new DotSpatialAffineCoordinateSequence(size, Ordinates & OrdinatesUtility.DimensionToOrdinates(dimension));
        }

        /// <summary>
        /// Creates a <see cref="ICoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="ICoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size">The number of coordinates.</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="GeoAPI.Geometries.Ordinates.XY"/> is fix,
        /// <see cref="GeoAPI.Geometries.Ordinates.Z"/> and <see cref="GeoAPI.Geometries.Ordinates.M"/> can be set.
        /// </param>
        /// <returns>A coordinate sequence.</returns>
        public ICoordinateSequence Create(int size, Ordinates ordinates)
        {
            return new DotSpatialAffineCoordinateSequence(size, Ordinates & ordinates);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence Create(double[] xy)
        {
            return new DotSpatialAffineCoordinateSequence(xy, null);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates,
        /// the <paramref name="zm"/> array for either z-ordinates or measure values. This is indicated by <paramref name="isMeasure"/>.
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <param name="zm">An array of z- or measure values</param>
        /// <param name="isMeasure">A value indicating if <paramref name="zm"/> contains z-ordinates or measure values.</param>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence Create(double[] xy, double[] zm, bool isMeasure = false)
        {
            if (isMeasure)
                return new DotSpatialAffineCoordinateSequence(xy, null, zm);
            return new DotSpatialAffineCoordinateSequence(xy, zm);
        }

        /// <summary>
        /// Creates an instance of this class using the provided <paramref name="xy"/> array for x- and y ordinates,
        /// the <paramref name="z"/> array for z ordinates and <paramref name="m"/> for measure values.
        /// </summary>
        /// <param name="xy">The x- and y-ordinates</param>
        /// <param name="z">An array of z- or measure values</param>
        /// <param name="m">An array of measure values.</param>
        /// <returns>A coordinate sequence</returns>

        public ICoordinateSequence Create(double[] xy, double[] z, double[] m)
        {
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

        /// <summary>
        /// Gets the Ordinate flags that sequences created by this factory can cope with.
        /// </summary>
        public Ordinates Ordinates => _ordinates;
    }
}
