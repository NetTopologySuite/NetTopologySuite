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
        protected int dimension;

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public int Dimension
        {
            get { return dimension; }            
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public abstract int Count { get; }

        /// <summary>
        /// Returns (possibly a copy of) the ith Coordinate in this collection.
        /// Whether or not the Coordinate returned is the actual underlying
        /// Coordinate or merely a copy depends on the implementation.
        /// Note that in the future the semantics of this method may change
        /// to guarantee that the Coordinate returned is always a copy. Callers are
        /// advised not to assume that they can modify a CoordinateSequence by
        /// modifying the Coordinate returned by this method.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public ICoordinate GetCoordinate(int i) 
        {
            ICoordinate[] arr = GetCachedCoords();
            if(arr != null)
                 return arr[i];
            else return GetCoordinateInternal(i);
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
        public ICoordinate GetCoordinateCopy(int i) 
        {
            return GetCoordinateInternal(i);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.  
        /// Only the first two dimensions are copied.        
        /// </summary>
        /// <param name="i">The index of the coordinate to copy.</param>
        /// <param name="c">A Coordinate to receive the value.</param>
        public void GetCoordinate(int i, ICoordinate c) 
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
        /// <returns></returns>
        public ICoordinate[] ToCoordinateArray() 
        {
            ICoordinate[] arr = GetCachedCoords();
            // testing - never cache
            if (arr != null)
                return arr;

            arr = new ICoordinate[Count];
            for (int i = 0; i < arr.Length; i++) 
                arr[i] = GetCoordinateInternal(i);
            
            coordRef = new WeakReference(arr);
            return arr;
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ICoordinate[] GetCachedCoords() 
        {
            if (coordRef != null) 
            {
                ICoordinate[] arr = (ICoordinate[]) coordRef.Target;
                if (arr != null) 
                    return arr;
                else 
                {            
                    coordRef = null;
                    return null;
                }
            } 
            else return null;            
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public double GetX(int index) 
        {
            return GetOrdinate(index, Ordinates.X);
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public double GetY(int index) 
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
        /// <returns></returns>
        public abstract double GetOrdinate(int index, Ordinates ordinate);


        /// <summary>
        /// Sets the first ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetX(int index, double value) 
        {
            coordRef = null;
            SetOrdinate(index, Ordinates.X, value);
        }

        /// <summary>
        /// Sets the second ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void setY(int index, double value) 
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
        public abstract void SetOrdinate(int index, Ordinates ordinate, double value);

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected abstract ICoordinate GetCoordinateInternal(int index);

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
        public abstract IEnvelope ExpandEnvelope(IEnvelope env);
    }

    /// <summary>
    /// Packed coordinate sequence implementation based on doubles.
    /// </summary>
    public class PackedDoubleCoordinateSequence : PackedCoordinateSequence 
    {
        /// <summary>
        /// The packed coordinate array
        /// </summary>
        private double[] coords;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coords"></param>
        /// <param name="dimensions"></param>
        public PackedDoubleCoordinateSequence(double[] coords, int dimensions) 
        {
            if (dimensions < 2) 
                throw new ArgumentException("Must have at least 2 dimensions");
            
            if (coords.Length % dimensions != 0) 
                throw new ArgumentException("Packed array does not contain " + 
                    "an integral number of coordinates");
      
            this.dimension = dimensions;
            this.coords = coords;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimensions"></param>
        public PackedDoubleCoordinateSequence(float[] coordinates, int dimensions) 
        {
            this.coords = new double[coordinates.Length];
            this.dimension = dimensions;
            for (int i = 0; i < coordinates.Length; i++) 
                this.coords[i] = coordinates[i];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedDoubleCoordinateSequence(ICoordinate[] coordinates, int dimension) 
        {
            if (coordinates == null)
                coordinates = new ICoordinate[0];
            this.dimension = dimension;

            coords = new double[coordinates.Length * this.dimension];
            for (int i = 0; i < coordinates.Length; i++) 
            {
                coords[i * this.dimension] = coordinates[i].X;
                if (this.dimension >= 2)
                    coords[i * this.dimension + 1] = coordinates[i].Y;
                if (this.dimension >= 3)
                    coords[i * this.dimension + 2] = coordinates[i].Z;
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        public PackedDoubleCoordinateSequence(ICoordinate[] coordinates) : this(coordinates, 3) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedDoubleCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        public PackedDoubleCoordinateSequence(int size, int dimension)
        {
            this.dimension = dimension;
            coords = new double[size * this.dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override ICoordinate GetCoordinateInternal(int index) 
        {
            double x = coords[index * dimension];
            double y = coords[index * dimension + 1];
            double z = dimension == 2 ? 0.0 : coords[index * dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public override int Count 
        {
            get { return coords.Length / dimension; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public override Object Clone() 
        {
            double[] clone = new double[coords.Length];
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
        /// <returns></returns>
        public override double GetOrdinate(int index, Ordinates ordinate) 
        {
            return coords[index * dimension + (int) ordinate];
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
        public override void SetOrdinate(int index, Ordinates ordinate, double value) 
        {
            coordRef = null;
            coords[index * dimension + (int) ordinate] = value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override IEnvelope ExpandEnvelope(IEnvelope env)
        {
            for (int i = 0; i < coords.Length; i += dimension)
                env.ExpandToInclude(coords[i], coords[i + 1]);        
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
        /// <param name="coords"></param>
        /// <param name="dimensions"></param>
        public PackedFloatCoordinateSequence(float[] coords, int dimensions) 
        {
            if (dimensions < 2) 
                throw new ArgumentException("Must have at least 2 dimensions");      
            
            if (coords.Length % dimensions != 0) 
                throw new ArgumentException("Packed array does not contain " + 
                    "an integral number of coordinates");
      
            this.dimension = dimensions;
            this.coords = coords;
        }
    
        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimensions"></param>
        public PackedFloatCoordinateSequence(double[] coordinates, int dimensions) 
        {
            this.coords = new float[coordinates.Length];
            this.dimension = dimensions;
            for (int i = 0; i < coordinates.Length; i++) 
                this.coords[i] = (float) coordinates[i];      
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="dimension"></param>
        public PackedFloatCoordinateSequence(ICoordinate[] coordinates, int dimension) 
        {
            if (coordinates == null)
                coordinates = new ICoordinate[0];
            this.dimension = dimension;

            coords = new float[coordinates.Length * this.dimension];
            for (int i = 0; i < coordinates.Length; i++) 
            {
                coords[i * this.dimension] = (float) coordinates[i].X;
                if (this.dimension >= 2)
                    coords[i * this.dimension + 1] = (float) coordinates[i].Y;
                if (this.dimension >= 3)
                coords[i * this.dimension + 2] = (float) coordinates[i].Z;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackedFloatCoordinateSequence"/> class.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="dimension"></param>
        public PackedFloatCoordinateSequence(int size, int dimension) 
        {
            this.dimension = dimension;
            coords = new float[size * this.dimension];
        }

        /// <summary>
        /// Returns a Coordinate representation of the specified coordinate, by always
        /// building a new Coordinate object.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override ICoordinate GetCoordinateInternal(int index) 
        {
            double x = coords[index * dimension];
            double y = coords[index * dimension + 1];
            double z = dimension == 2 ? 0.0 : coords[index * dimension + 2];
            return new Coordinate(x, y, z);
        }

        /// <summary>
        /// Returns the number of coordinates in this sequence.
        /// </summary>
        /// <value></value>
        public override int Count
        {
            get { return coords.Length / dimension; }
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
        /// <returns></returns>
        public override double GetOrdinate(int index, Ordinates ordinate) 
        {
            return coords[index * dimension + (int) ordinate];
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
        public override void SetOrdinate(int index, Ordinates ordinate, double value) 
        {
            coordRef = null;
            coords[index * dimension + (int) ordinate] = (float) value;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="env">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public override IEnvelope ExpandEnvelope(IEnvelope env)
        {
        for (int i = 0; i < coords.Length; i += dimension )
            env.ExpandToInclude(coords[i], coords[i + 1]);      
        return env;
        }
    }
}
