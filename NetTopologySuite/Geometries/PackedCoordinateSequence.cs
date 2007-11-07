using System;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// A <c>CoordinateSequence</c> implementation based on a packed arrays.
    /// </summary>
    public abstract class PackedCoordinateSequence : ICoordinateSequence
    {
        /// <summary>
        /// A soft reference to the Coordinate[] representation of this sequence.
        /// Makes repeated coordinate array accesses more efficient.
        /// </summary>
        protected WeakReference coordRef;

        /// <summary>
        /// The dimensions of the coordinates hold in the packed array
        /// </summary>
        protected Int32 dimension;

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        public Int32 Dimension
        {
            get { return dimension; }
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        public abstract Int32 Count { get; }

        /// <summary>
        /// Returns (possibly a copy of) the ith Coordinate in this collection.
        /// Whether or not the Coordinate returned is the actual underlying
        /// Coordinate or merely a copy depends on the implementation.
        /// Note that in the future the semantics of this method may change
        /// to guarantee that the Coordinate returned is always a copy. Callers are
        /// advised not to assume that they can modify a CoordinateSequence by
        /// modifying the Coordinate returned by this method.
        /// </summary>
        public ICoordinate GetCoordinate(Int32 i)
        {
            ICoordinate[] arr = GetCachedCoords();

            if (arr != null)
            {
                return arr[i];
            }
            else
            {
                return GetCoordinateInternal(i);
            }
        }

        /// <summary>
        /// Returns a copy of the i'th coordinate in this sequence.
        /// This method optimizes the situation where the caller is
        /// going to make a copy anyway - if the implementation
        /// has already created a new Coordinate object, no further copy is needed.
        /// </summary>
        /// <param name="i">The index of the coordinate to retrieve.</param>
        /// <returns>
        /// A copy of the i'th coordinate in the sequence
        /// </returns>
        public ICoordinate GetCoordinateCopy(Int32 i)
        {
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.  
        /// Only the first two dimensions are copied.        
        /// </summary>
        /// <param name="i">The index of the coordinate to copy.</param>
        /// <param name="c">A Coordinate to receive the value.</param>
        public void GetCoordinate(Int32 i, ICoordinate c)
        {
            c.X = GetOrdinate(i, Ordinates.X);
            c.Y = GetOrdinate(i, Ordinates.Y);
        }

        /// <summary>
        /// Returns (possibly copies of) the Coordinates in this collection.
        /// Whether or not the Coordinates returned are the actual underlying
        /// Coordinates or merely copies depends on the implementation. 
        /// Note that if this implementation does not store its data as an array of Coordinates,
        /// this method will incur a performance penalty because the array needs to
        /// be built from scratch.
        /// </summary>
        public ICoordinate[] ToCoordinateArray()
        {
            ICoordinate[] arr = GetCachedCoords();

            // testing - never cache
            if (arr != null)
            {
                return arr;
            }

            arr = new ICoordinate[Count];

            for (Int32 i = 0; i < arr.Length; i++)
            {
                arr[i] = GetCoordinateInternal(i);
            }

            coordRef = new WeakReference(arr);
            return arr;
        }

        private ICoordinate[] GetCachedCoords()
        {
            if (coordRef != null)
            {
                ICoordinate[] arr = (ICoordinate[]) coordRef.Target;
                
                if (arr != null)
                {
                    return arr;
                }
                else
                {
                    coordRef = null;
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public Double GetX(Int32 index)
        {
            return GetOrdinate(index, Ordinates.X);
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public Double GetY(Int32 index)
        {
            return GetOrdinate(index, Ordinates.Y);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        public abstract Double GetOrdinate(Int32 index, Ordinates ordinate);


        /// <summary>
        /// Sets the first ordinate of a coordinate in this sequence.
        /// </summary>
        public void SetX(Int32 index, Double value)
        {
            coordRef = null;
            SetOrdinate(index, Ordinates.X, value);
        }

        /// <summary>
        /// Sets the second ordinate of a coordinate in this sequence.
        /// </summary>
        public void setY(Int32 index, Double value)
        {
            coordRef = null;
            SetOrdinate(index, Ordinates.Y, value);
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>        
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based, 
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        public abstract void SetOrdinate(Int32 index, Ordinates ordinate, Double value);

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        protected abstract ICoordinate GetCoordinateInternal(Int32 index);

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public abstract Object Clone();

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public abstract IExtents ExpandEnvelope(IExtents env);
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on doubles.
    /// </summary>
    public class PackedDoubleCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private Double[] coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        public PackedDoubleCoordinateSequence(Double[] coords, Int32 dimensions)
        {
            if (dimensions < 2)
            {
                throw new ArgumentException("Must have at least 2 dimensions");
            }

            if (coords.Length%dimensions != 0)
            {
                throw new ArgumentException("Packed array does not contain " +
                                            "an integral number of coordinates");
            }

            dimension = dimensions;
            this.coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        public PackedDoubleCoordinateSequence(float[] coordinates, Int32 dimensions)
        {
            coords = new Double[coordinates.Length];
            dimension = dimensions;

            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                coords[i] = coordinates[i];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        public PackedDoubleCoordinateSequence(ICoordinate[] coordinates, Int32 dimension)
        {
            if (coordinates == null)
            {
                coordinates = new ICoordinate[0];
            }

            this.dimension = dimension;

            coords = new Double[coordinates.Length*this.dimension];

            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                coords[i*this.dimension] = coordinates[i].X;

                if (this.dimension >= 2)
                {
                    coords[i*this.dimension + 1] = coordinates[i].Y;
                }

                if (this.dimension >= 3)
                {
                    coords[i*this.dimension + 2] = coordinates[i].Z;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        public PackedDoubleCoordinateSequence(ICoordinate[] coordinates) : this(coordinates, 3) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        public PackedDoubleCoordinateSequence(Int32 size, Int32 dimension)
        {
            this.dimension = dimension;
            coords = new Double[size*this.dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        protected override ICoordinate GetCoordinateInternal(Int32 index)
        {
            Double x = coords[index*dimension];
            Double y = coords[index*dimension + 1];
            Double z = dimension == 2 ? 0.0 : coords[index*dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        public override Int32 Count
        {
            get { return coords.Length/dimension; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override Object Clone()
        {
            Double[] clone = new Double[coords.Length];
            Array.Copy(coords, clone, coords.Length);
            return new PackedDoubleCoordinateSequence(clone, dimension);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        public override Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            return coords[index*dimension + (Int32) ordinate];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>
        public override void SetOrdinate(Int32 index, Ordinates ordinate, Double value)
        {
            coordRef = null;
            coords[index*dimension + (Int32) ordinate] = value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override IExtents ExpandEnvelope(IExtents env)
        {
            for (Int32 i = 0; i < coords.Length; i += dimension)
            {
                env.ExpandToInclude(coords[i], coords[i + 1]);
            }

            return env;
        }
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on floats.
    /// </summary>
    public class PackedFloatCoordinateSequence : PackedCoordinateSequence
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private float[] coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        public PackedFloatCoordinateSequence(float[] coords, Int32 dimensions)
        {
            if (dimensions < 2)
            {
                throw new ArgumentException("Must have at least 2 dimensions");
            }

            if (coords.Length%dimensions != 0)
            {
                throw new ArgumentException("Packed array does not contain " +
                                            "an integral number of coordinates");
            }

            dimension = dimensions;
            this.coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        public PackedFloatCoordinateSequence(Double[] coordinates, Int32 dimensions)
        {
            coords = new float[coordinates.Length];
            dimension = dimensions;

            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                coords[i] = (float) coordinates[i];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        public PackedFloatCoordinateSequence(ICoordinate[] coordinates, Int32 dimension)
        {
            if (coordinates == null)
            {
                coordinates = new ICoordinate[0];
            }

            this.dimension = dimension;

            coords = new float[coordinates.Length*this.dimension];

            for (Int32 i = 0; i < coordinates.Length; i++)
            {
                coords[i*this.dimension] = (float) coordinates[i].X;

                if (this.dimension >= 2)
                {
                    coords[i*this.dimension + 1] = (float) coordinates[i].Y;
                }

                if (this.dimension >= 3)
                {
                    coords[i*this.dimension + 2] = (float) coordinates[i].Z;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        public PackedFloatCoordinateSequence(Int32 size, Int32 dimension)
        {
            this.dimension = dimension;
            coords = new float[size*this.dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        protected override ICoordinate GetCoordinateInternal(Int32 index)
        {
            Double x = coords[index*dimension];
            Double y = coords[index*dimension + 1];
            Double z = dimension == 2 ? 0.0 : coords[index*dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        public override Int32 Count
        {
            get { return coords.Length/dimension; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override Object Clone()
        {
            float[] clone = new float[coords.Length];
            Array.Copy(coords, clone, coords.Length);
            return new PackedFloatCoordinateSequence(clone, dimension);
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        public override Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            return coords[index*dimension + (Int32) ordinate];
        }

        /// <summary>
        /// Sets the ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index.</param>
        /// <param name="ordinate">The ordinate index in the coordinate, 0 based,
        /// smaller than the number of dimensions.</param>
        /// <param name="value">The new ordinate value.</param>
        /// <remarks>
        /// Warning: for performance reasons the ordinate index is not checked:
        /// if it is over dimensions you may not get an exception but a meaningless value.
        /// </remarks>
        public override void SetOrdinate(Int32 index, Ordinates ordinate, Double value)
        {
            coordRef = null;
            coords[index*dimension + (Int32) ordinate] = (float) value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override IExtents ExpandEnvelope(IExtents env)
        {
            for (Int32 i = 0; i < coords.Length; i += dimension)
            {
                env.ExpandToInclude(coords[i], coords[i + 1]);
            }

            return env;
        }
    }
}