using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An object that knows how to build a particular implementation of
    /// <c>CoordinateSequence</c> from an array of Coordinates.
    /// </summary>
    /// <seealso cref="CoordinateSequence" />
    [Serializable]
    public abstract class CoordinateSequenceFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSequenceFactory"/> class.`
        /// </summary>
        protected CoordinateSequenceFactory()
            : this(Ordinates.AllOrdinates) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateSequenceFactory"/> class.
        /// </summary>
        /// <param name="ordinates">
        /// The maximum set of <see cref="Geometries.Ordinates"/> flags that this instance will be
        /// able to create sequences for.
        /// </param>
        protected CoordinateSequenceFactory(Ordinates ordinates) => Ordinates = Ordinates.XY | ordinates;

        /// <summary>
        /// Gets the Ordinate flags that sequences created by this factory can maximal cope with.
        /// </summary>
        public Ordinates Ordinates { get; }

        /// <summary>
        /// Returns a <see cref="CoordinateSequence" /> based on the given array;
        /// whether or not the array is copied is implementation-dependent.
        /// </summary>
        /// <param name="coordinates">A coordinates array, which may not be null nor contain null elements</param>
        /// <returns>A coordinate sequence.</returns>
        public virtual CoordinateSequence Create(Coordinate[] coordinates)
        {
            (int count, int dimension, int measures) = GetCommonSequenceParameters(coordinates);
            var result = Create(count, dimension, measures);
            if (coordinates != null)
            {
                int spatial = dimension - measures;
                for (int i = 0; i < coordinates.Length; i++)
                {
                    var coord = coordinates[i];
                    int coordDimension = Coordinates.Dimension(coord);
                    int coordMeasures = Coordinates.Measures(coord);
                    int coordSpatial = coordDimension - coordMeasures;
                    for (int dim = 0; dim < coordSpatial; dim++)
                    {
                        result.SetOrdinate(i, dim, coord[dim]);
                    }

                    for (int dim = coordSpatial; dim < spatial; dim++)
                    {
                        result.SetOrdinate(i, dim, Coordinate.NullOrdinate);
                    }

                    for (int measure = 0; measure < coordMeasures; measure++)
                    {
                        result.SetOrdinate(i, spatial + measure, coord[coordSpatial + measure]);
                    }

                    for (int measure = coordMeasures; measure < measures; measure++)
                    {
                        result.SetOrdinate(i, spatial + measure, Coordinate.NullOrdinate);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" />  which is a copy
        /// of the given <see cref="CoordinateSequence" />.
        /// This method must handle null arguments by creating an empty sequence.
        /// </summary>
        /// <param name="coordSeq"></param>
        /// <returns>A coordinate sequence</returns>
        public virtual CoordinateSequence Create(CoordinateSequence coordSeq)
        {
            var result = Create(coordSeq?.Count ?? 0, coordSeq?.Dimension ?? 2, coordSeq?.Measures ?? 0);
            if (coordSeq != null)
            {
                for (int i = 0; i < coordSeq.Count; i++)
                {
                    for (int dim = 0; dim < result.Dimension; dim++)
                    {
                        result.SetOrdinate(i, dim, coordSeq.GetOrdinate(i, dim));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and dimension.
        /// For this to be useful, the <see cref="CoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <remarks>
        /// If the requested dimension is larger than the CoordinateSequence implementation
        /// can provide, then a sequence of maximum possible dimension should be created.
        /// An error should not be thrown.
        /// </remarks>
        /// <param name="size"></param>
        /// <param name="dimension">the dimension of the coordinates in the sequence
        /// (if user-specifiable, otherwise ignored)</param>
        /// <returns>A coordinate sequence</returns>
        [Obsolete("Use an overload that accepts measures.  This overload will be removed in a future release.")]
        public CoordinateSequence Create(int size, int dimension) => Create(size, dimension, 0);

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and dimension
        /// with measure support. For this to be useful, the <see cref="CoordinateSequence" />
        /// implementation must be mutable.
        /// </summary>
        /// <remarks>
        /// If the requested dimension or measures are larger than the CoordinateSequence implementation
        /// can provide, then a sequence of maximum possible dimension should be created.
        /// An error should not be thrown.
        /// </remarks>
        /// <param name="size">The number of coordinates in the sequence</param>
        /// <param name="dimension">The dimension of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        /// <param name="measures">The number of measures of the coordinates in the sequence (if user-specifiable,
        /// otherwise ignored)</param>
        public abstract CoordinateSequence Create(int size, int dimension, int measures);

        /// <summary>
        /// Creates a <see cref="CoordinateSequence" /> of the specified size and ordinates.
        /// For this to be useful, the <see cref="CoordinateSequence" /> implementation must be mutable.
        /// </summary>
        /// <param name="size">The number of coordinates.</param>
        /// <param name="ordinates">
        /// The ordinates each coordinate has. <see cref="Geometries.Ordinates.XY"/> is fix, <see cref="Geometries.Ordinates.Z"/> and <see cref="Geometries.Ordinates.M"/> can be set.
        /// </param>
        /// <returns>A coordinate sequence.</returns>
        public virtual CoordinateSequence Create(int size, Ordinates ordinates) => Create(size, OrdinatesUtility.OrdinatesToDimension(ordinates & Ordinates), OrdinatesUtility.OrdinatesToMeasures(ordinates & Ordinates));

        /// <summary>
        /// Gets the three parameters needed to create any <see cref="CoordinateSequence"/> instance
        /// (<see cref="CoordinateSequence.Count"/>, <see cref="CoordinateSequence.Dimension"/>, and
        /// <see cref="CoordinateSequence.Measures"/>) such that the sequence can store all the data
        /// from a given array of <see cref="Coordinate"/> instances.
        /// </summary>
        /// <param name="coordinates">
        /// The array of <see cref="Coordinate"/> instances that the sequence will be created from.
        /// </param>
        /// <returns>
        /// The values of the three parameters to use for creating the sequence.
        /// </returns>
        public static (int Count, int Dimension, int Measures) GetCommonSequenceParameters(Coordinate[] coordinates)
        {
            if (coordinates == null)
            {
                return (0, 2, 0);
            }

            int count = coordinates.Length;
            int spatial = 2;
            int measures = 0;

            // figure out the minimum number of spatial dimensions and measures needed to hold all
            // the data from the array.  this is NOT CoordinateArrays.Dimension(coordinates) and
            // CoordinateArrays.Measures(coordinates): an array of only CoordinateZ and CoordinateM
            // instances mixed together will give us dimension = 3 and measures = 1, which would
            // mean that the resulting sequence would not contain Z.
            foreach (var coord in coordinates)
            {
                int coordDimension = Coordinates.Dimension(coord);
                int coordMeasures = Coordinates.Measures(coord);
                int coordSpatial = coordDimension - coordMeasures;
                if (coordSpatial > spatial)
                {
                    spatial = coordSpatial;
                }

                if (coordMeasures > measures)
                {
                    measures = coordMeasures;
                }
            }

            return (count, spatial + measures, measures);
        }
    }
}
