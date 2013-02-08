using System;
using System.Collections.Generic;
using GeoAPI;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    // ReSharper disable ImpureMethodCallOnReadonlyValueField

    /// <summary>
    /// Delegate to convert from a <see cref="CoordinateBuffer"/> to a <see cref="ICoordinateSequence"/>
    /// </summary>
    /// <param name="buffer">The coordinate sequence</param>
    /// <returns>The converted coordinate sequence</returns>
#if UseCoordinateBufferPublicly
    public delegate ICoordinateSequence CoordinateBufferToSequenceConverterHandler(CoordinateBuffer buffer);
#else
    internal delegate ICoordinateSequence CoordinateBufferToSequenceConverterHandler(CoordinateBuffer buffer);
#endif

    /// <summary>
    /// Utility class for storing coordinates
    /// </summary>
    /// <remarks>
    /// This class may be useful for other IO classes as well
    /// </remarks>
#if UseCoordinateBufferPublicly
    public class CoordinateBuffer : IEquatable<CoordinateBuffer>, IEquatable<ICoordinateSequence>, ICoordinateBuffer
#else
    internal class CoordinateBuffer : IEquatable<CoordinateBuffer>, IEquatable<ICoordinateSequence>, ICoordinateBuffer
#endif
    {
        #region NoDataChecker
        /// <summary>
        /// Utility to check <see cref="double"/> values for a defined null/no-data-value
        /// </summary>
        private struct DoubleNoDataChecker
        {
            private enum IsNoDataCheck
            {
                NaN,
                PosInf,
                NegInf,
                Inf,
                Equal,
                LessThan,
            }

            private readonly double _noDataCheckValue;
            private readonly double _noDataValue;
            private readonly IsNoDataCheck _isNoDataCheck;
            
            /// <summary>
            /// Initializes this stucture with a <paramref name="noDataValue"/>
            /// </summary>
            /// <param name="noDataValue">The value that is to be treated as <c>null</c></param>
            /// <param name="lessThan">This optional parameter controls whether a value has to be less than <see cref="noDataValue"/> to be considered <c>null</c></param>
            public DoubleNoDataChecker(double noDataValue, bool lessThan = false)
            {
                _noDataValue = _noDataCheckValue = noDataValue;
                if (double.IsNaN(noDataValue))
                    _isNoDataCheck = IsNoDataCheck.NaN;
                else if (double.IsPositiveInfinity(noDataValue))
                    _isNoDataCheck = IsNoDataCheck.PosInf;
                else if (double.IsNegativeInfinity(noDataValue))
                    _isNoDataCheck = IsNoDataCheck.NegInf;
                else if (double.IsInfinity(noDataValue))
                    _isNoDataCheck = IsNoDataCheck.Inf;
                else
                {
                    if (lessThan)
                    {
                        _isNoDataCheck = IsNoDataCheck.LessThan;
                        _noDataValue = _noDataCheckValue * 1.01d;
                    }
                    else
                    {
                        _isNoDataCheck = IsNoDataCheck.Equal;
                    }
                }
            }

            /// <summary>
            /// Checks if <paramref name="value"/> doesn't satisfy null-check
            /// </summary>
            /// <param name="value">The value to check</param>
            /// <returns><c>true</c> if <paramref name="value"/> is not equal to <see cref="_noDataCheckValue"/></returns>
            public bool IsNotNoDataValue(double value)
            {
                switch (_isNoDataCheck)
                {
                    case IsNoDataCheck.NaN:
                        return !double.IsNaN(value);
                    case IsNoDataCheck.PosInf:
                        return !double.IsPositiveInfinity(value);
                    case IsNoDataCheck.NegInf:
                        return !double.IsNegativeInfinity(value);
                    case IsNoDataCheck.Inf:
                        return !double.IsInfinity(value);
                    case IsNoDataCheck.LessThan:
                        return value>=_noDataCheckValue;
                    default:
                        return _noDataCheckValue != value;
                }
            }

            /// <summary>
            /// Checks if <paramref name="value"/> does satisfy null-check
            /// </summary>
            /// <param name="value">The value to check</param>
            /// <returns><c>true</c> if <paramref name="value"/> is equal to <see cref="_noDataCheckValue"/></returns>
            public bool IsNoDataValue(double value)
            {
                switch (_isNoDataCheck)
                {
                    case IsNoDataCheck.NaN:
                        return double.IsNaN(value);
                    case IsNoDataCheck.PosInf:
                        return double.IsPositiveInfinity(value);
                    case IsNoDataCheck.NegInf:
                        return double.IsNegativeInfinity(value);
                    case IsNoDataCheck.Inf:
                        return double.IsInfinity(value);
                    case IsNoDataCheck.LessThan:
                        return value < _noDataCheckValue;
                    default:
                        return _noDataCheckValue == value;
                }
            }

            /// <summary>
            /// Gets the defined <c>null</c> value
            /// </summary>
            public double NoDataValue { get { return _noDataValue; } }

            public override string ToString()
            {
                switch (_isNoDataCheck)
                {
                    case IsNoDataCheck.Equal:
                    case IsNoDataCheck.LessThan:
                        return string.Format("IsNullCheck: {0} {1}", _isNoDataCheck, _noDataCheckValue);
                    default:
                        return string.Format("IsNullCheck: {0}", _isNoDataCheck);
                }
            }

            
        }
        #endregion

        private static readonly object FactoryLock = new object();
        private static volatile ICoordinateSequenceFactory _factory;

        private readonly Envelope _extents = new Envelope();
        private Interval _zInterval = Interval.Create();
        private Interval _mInterval = Interval.Create();

        private Ordinates _definedOrdinates = Ordinates.XY;
        private readonly DoubleNoDataChecker _doubleNoDataChecker;

        private readonly List<double[]> _coordinates;
        private readonly List<int> _markers = new List<int>();

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public CoordinateBuffer()
        {
            _coordinates = new List<double[]>();
            _doubleNoDataChecker = new DoubleNoDataChecker(Coordinate.NullOrdinate);
        }

        /// <summary>
        /// Creates an instance of this class with <paramref name="nullValue"/> defining the values that should be treated as null.
        /// </summary>
        /// <param name="nullValue">The value that should be treated as null.</param>
        /// <param name="lessThan">This optional parameter controls whether a value has to be less than <see cref="nullValue"/> to be considered <c>null</c></param>
        public CoordinateBuffer(double nullValue, bool lessThan = false)
        {
            _coordinates = new List<double[]>();
            _doubleNoDataChecker = new DoubleNoDataChecker(nullValue, lessThan);
        }

        /// <summary>
        /// Creates an instance of this class with an inital <paramref name="capacity"/>
        /// </summary>
        /// <param name="capacity">The inital capacity of the buffer.</param>
        public CoordinateBuffer(int capacity)
        {
            _coordinates = new List<double[]>(capacity);
            _doubleNoDataChecker = new DoubleNoDataChecker(double.NaN);
        }

        /// <summary>
        /// Creates an instance of this class with an inital <paramref name="capacity"/>
        /// </summary>
        /// <param name="capacity">The inital capacity of the buffer.</param>
        /// <param name="nullValue">The value that should be treated as null.</param>
        /// <param name="lessThan">This optional parameter controls whether a value has to be less than <see cref="nullValue"/> to be considered <c>null</c></param>
        public CoordinateBuffer(int capacity, double nullValue, bool lessThan = false)
        {
            _coordinates = new List<double[]>(capacity);
            _doubleNoDataChecker = new DoubleNoDataChecker(nullValue, lessThan);
        }

        /// <summary>
        /// Updates the <see cref="_definedOrdinates"/> flags
        /// </summary>
        /// <param name="z">The z-Ordinate</param>
        /// <param name="m">The m-Ordinate</param>
        private void CheckDefinedOrdinates(ref double z, ref double m)
        {
            if (_doubleNoDataChecker.IsNotNoDataValue(z))
                _definedOrdinates |= Ordinates.Z;
                
            else
                z = Coordinate.NullOrdinate;
            
            if (_doubleNoDataChecker.IsNotNoDataValue(m))
                _definedOrdinates |= Ordinates.M;
            else
                m = Coordinate.NullOrdinate;
        }

        /// <summary>
        /// Gets or sets the <see cref="ICoordinateSequenceFactory"/> used to create a coordinate sequence from the coordinate data in the buffer.
        /// </summary>
        public ICoordinateSequenceFactory Factory
        {
            get
            {
                if (_factory != null)
                    return _factory;

                lock (FactoryLock)
                {
                    if (_factory == null)
                        _factory = GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory;
                }

                return _factory;
            }

            set
            {
                lock (FactoryLock)
                {
                    _factory = value;
                }
            }
        }

        /// <summary>
        /// Gets the number of coordinates added to the buffer
        /// </summary>
        public int Count
        {
            get { return _coordinates.Count; }
        }

        /// <summary>
        /// Gets the defined ordinates in this buffer
        /// </summary>
        public Ordinates DefinedOrdinates { get { return _definedOrdinates; } }

        /// <summary>
        /// Gets the number of dimension a coordinate sequence must provide
        /// </summary>
        public int Dimension
        {
            get
            {
                var res = 2;
                if (HasM) res++;
                if (HasZ) res++;
                return res;
            }
        }

        /// <summary>
        /// Gets a value indicating if this buffer contains any z-ordinate values
        /// </summary>
        public bool HasZ { get { return (_definedOrdinates & Ordinates.Z) == Ordinates.Z; } }

        /// <summary>
        /// Gets a value indicating if this buffer contains any m-ordinate values
        /// </summary>
        public bool HasM { get { return (_definedOrdinates & Ordinates.M) == Ordinates.M; } }

        /// <summary>
        /// Gets the (current) capacity of the buffer
        /// </summary>
        public int Capacity
        {
            get { return _coordinates.Capacity; }
        }

        /// <summary>
        /// Adds a coordinate made up of the ordinates (x, y, z, m) to the buffer.
        /// </summary>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="z">The (optional) z-Ordinate</param>
        /// <param name="m">The (optional) m-Ordinate</param>
        /// <param name="allowRepeated">Allows repated coordinates to be added</param>
        /// <returns><value>true</value> if the coordinate was successfully added.</returns>
        public bool AddCoordinate(double x, double y, double? z = null, double? m = null, bool allowRepeated = true)
        {
            // Assign NoDataValue if not provided
            if (!z.HasValue) z = _doubleNoDataChecker.NoDataValue;
            if (!m.HasValue) m = _doubleNoDataChecker.NoDataValue;

            // Update defined flag and set Coordinate.NullValue where necessary
            var tmpZ = z.Value;
            var tmpM = m.Value;
            CheckDefinedOrdinates(ref tmpZ, ref tmpM);

            var toAdd = new[] {x, y, tmpZ, tmpM};
            if (!allowRepeated && _coordinates.Count > 0)
            {
                if (Equals(_coordinates[_coordinates.Count - 1], toAdd))
                    return false;
            }

            // Add new coordinate
            _coordinates.Add(toAdd);

            // Update envelope
            _extents.ExpandToInclude(x, y);

            // Update extents for z- and m-values
            _zInterval = _zInterval.ExpandedByValue(tmpZ);
            _mInterval = _mInterval.ExpandedByValue(tmpM);

            // Signal that coordinate was inserted
            return true;
        }

        /// <summary>
        /// Method to add a marker
        /// </summary>
        public void AddMarker()
        {
            _markers.Add(_coordinates.Count);
        }

        /// <summary>
        /// Inserts a coordinate made up of the ordinates (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>, <paramref name="m"/>) at index <paramref name="index"/> to the buffer.
        ///  </summary>
        /// <param name="index">The index at which to insert the ordinate.</param>
        /// <param name="x">The x-Ordinate</param>
        /// <param name="y">The y-Ordinate</param>
        /// <param name="z">The (optional) z-Ordinate</param>
        /// <param name="m">The (optional) m-Ordinate</param>
        /// <param name="allowRepeated">Allows repated coordinates to be added</param>
        /// <returns><value>true</value> if the coordinate was successfully inserted.</returns>
        public bool InsertCoordinate(int index, double x, double y, double? z = null, double? m = null, bool allowRepeated = true)
        {
            // Assign NoDataValue if not provided
            if (!z.HasValue) z = _doubleNoDataChecker.NoDataValue;
            if (!m.HasValue) m = _doubleNoDataChecker.NoDataValue;

            // Update defined flag and set Coordinate.NullValue where necessary
            var tmpZ = z.Value;
            var tmpM = m.Value;
            CheckDefinedOrdinates(ref tmpZ, ref tmpM);

            var toAdd = new[] {x, y, tmpZ, tmpM};
            if (!allowRepeated)
            {
                if (index > 0)
                {
                    //Check before
                    if (Equals(_coordinates[index - 1], toAdd))
                        return false;
                }
                if (index >= 0 && index < _coordinates.Count)
                {
                    //Check after
                    if (Equals(_coordinates[index], toAdd))
                        return false;
                }
            }
            _coordinates.Insert(index, toAdd);

            // Update envelope
            _extents.ExpandToInclude(x, y);

            // Update extents for z- and m-values
            _zInterval = _zInterval.ExpandedByValue(tmpZ);
            _mInterval = _mInterval.ExpandedByValue(tmpM);

            // Signal success
            return true;
        }

        /// <summary>
        /// Clears the contents of this buffer
        /// </summary>
        public void Clear()
        {
            _coordinates.Clear();
            _definedOrdinates = Ordinates.XY;
        }

        /// <summary>
        /// Convertes the contents of the buffer to an array of <see cref="Coordinate"/>s
        /// </summary>
        /// <returns>An array of <see cref="Coordinate"/>s</returns>
        public Coordinate[] ToCoordinateArray()
        {
            var res = new Coordinate[_coordinates.Count];
            var zIndex = HasM ? 3 : 2;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                res[i] = new Coordinate(_coordinates[i][0], _coordinates[i][1],  _coordinates[i][zIndex]);
            }
            return res;
        }


        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to a coordinate sequence using the provided <paramref name="converter"/>.
        /// </summary>
        /// <param name="converter">The converter to use</param>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence ToSequence(CoordinateBufferToSequenceConverterHandler converter)
        {
            // If we have a converter, use it
            if (converter != null)
                return converter(this);

            // so we don't. Bummer
            return ToSequence();
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to a coordinate sequence.
        /// </summary>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence ToSequence(ICoordinateSequenceFactory factory = null)
        {
            // Set the coordinate sequence factory, if not assigned
            if (factory == null)
                factory = _factory ?? (_factory = GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory);

            // determine ordinates to apply
            var useOrdinates = _definedOrdinates & factory.Ordinates;
            
            // create the sequence
            var sequence = factory.Create(_coordinates.Count, useOrdinates);
            var i = 0;
            foreach (var coordinate in _coordinates)
            {
                sequence.SetOrdinate(i, Ordinate.X, coordinate[0]);
                sequence.SetOrdinate(i, Ordinate.Y, coordinate[1]);
                if ((useOrdinates & Ordinates.Z) == Ordinates.Z)
                    sequence.SetOrdinate(i, Ordinate.Z, coordinate[2]);
                if ((useOrdinates & Ordinates.M) == Ordinates.M)
                    sequence.SetOrdinate(i, Ordinate.M, coordinate[3]);
                i++;
            }
            return sequence;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to a coordinate sequence.
        /// </summary>
        /// <returns>A coordinate sequence</returns>
        public ICoordinateSequence[] ToSequences(ICoordinateSequenceFactory factory = null)
        {
            // Set the coordinate sequence factory, if not assigned
            if (factory == null)
                factory = _factory ?? (_factory = GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory);

            // Copy the markers, append if neccessary
            var markers = new List<int>(_markers);
            if (markers.Count == 0 || markers[markers.Count-1] < _coordinates.Count) 
                markers.Add(_coordinates.Count);
            
            // determine ordinates to apply
            var useOrdinates = _definedOrdinates & factory.Ordinates;

            var res = new ICoordinateSequence[markers.Count];
            var offset = 0;

            //Iterate over all sections
            for (var s = 0; s < markers.Count; s++)
            {
                // compute the length of the current sequence
                var length = markers[s] - offset;
                
                // create a sequence of the apropriate size
                var sequence = res[s] = factory.Create(length, useOrdinates);
                var i = 0;

                // fill the sequence
                foreach (var coordinate in _coordinates.GetRange(offset, length))
                {
                    sequence.SetOrdinate(i, Ordinate.X, coordinate[0]);
                    sequence.SetOrdinate(i, Ordinate.Y, coordinate[1]);
                    if ((useOrdinates & Ordinates.Z) == Ordinates.Z)
                        sequence.SetOrdinate(i, Ordinate.Z, coordinate[2]);
                    if ((useOrdinates & Ordinates.M) == Ordinates.M)
                        sequence.SetOrdinate(i, Ordinate.M, coordinate[3]);
                    i++;
                }
                //Move the offset
                offset = offset + length;
            }
            return res;
        }

        /// <summary>
        /// Sets a z-value at the provided <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="z">The value</param>
        public void SetZ(int index, double z)
        {
            if (_doubleNoDataChecker.IsNoDataValue(z))
                z = Coordinate.NullOrdinate;
            else
            {
                _definedOrdinates |= Ordinates.Z;
                _zInterval = _zInterval.ExpandedByValue(z);
            }
            _coordinates[index][2] = z;
        }

        /// <summary>
        /// Sets a m-value at the provided <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="m">The value</param>
        public void SetM(int index, double m)
        {
            if (_doubleNoDataChecker.IsNoDataValue(m))
                m = Coordinate.NullOrdinate;
            else
            {
                _definedOrdinates |= Ordinates.M;
                _mInterval = _mInterval.ExpandedByValue(m);
            }
            _coordinates[index][3] = m;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate.X"/> and <see cref="Ordinate.Y"/> values.
        /// </summary>
        /// <returns>An array of <see cref="double"/>s</returns>
        public double[] ToXY()
        {
            var xy = new double[Count * 2];

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                xy[j++] = _coordinates[i][0];
                xy[j++] = _coordinates[i][1];
            }
            return xy;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate.X"/> and <see cref="Ordinate.Y"/> values.
        /// Additionally an array of <see cref="Ordinate.Z"/> values is supplied if this instance <see cref="HasZ"/> property is <c>true</c>
        /// </summary>
        /// <returns>An array of <see cref="double"/>s</returns>
        public double[] ToXYZ(out double[] z)
        {
            var xy = new double[Count * 2];
            var hasZ = HasZ;
            z = hasZ ? new double[Count] : null;

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                xy[j++] = _coordinates[i][0];
                xy[j++] = _coordinates[i][1];
                if (hasZ) z[i] = _coordinates[i][2];
            }
            return xy;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate.X"/> and <see cref="Ordinate.Y"/> values.
        /// Additionally an array of <see cref="Ordinate.M"/> values is supplied if this instance <see cref="HasM"/> property is <c>true</c>
        /// </summary>
        /// <returns>An array of <see cref="double"/>s</returns>
        public double[] ToXYM(out double[] m)
        {
            var xy = new double[Count * 2];
            var hasM = HasM;
            m = hasM ? new double[Count] : null;

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                xy[j++] = _coordinates[i][0];
                xy[j++] = _coordinates[i][1];
                if (hasM) m[i] = _coordinates[i][3];
            }
            return xy;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate.X"/> and <see cref="Ordinate.Y"/> values.
        /// Additionally an array of <see cref="Ordinate.M"/> and one of <see cref="Ordinate.M"/> values is supplied if this instance <see cref="HasZ"/> and or <see cref="HasM"/> property is <c>true</c>
        /// </summary>
        /// <returns>An array of <see cref="double"/>s</returns>
        public double[] ToXYZM(out double[] z, out double[] m)
        {
            var xy = new double[Count*2];
            var hasZ = HasZ;
            var hasM = HasM;
            z = hasZ ? new double[Count] : null;
            m = hasM ? new double[Count] : null;

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                xy[j++] = _coordinates[i][0];
                xy[j++] = _coordinates[i][1];
                if (hasZ) z[i] = _coordinates[i][2];
                if (hasM) m[i] = _coordinates[i][3];
            }

            return xy;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate"/> values.
        /// </summary>
        /// <returns>The number of dimensions and an array of <see cref="double"/>s</returns>
        public int ToPackedArray(out double[] ordinateValues)
        {
            var hasZ = HasZ;
            var hasM = HasM;
            var dimension = Dimension;
            ordinateValues = new double[Count * dimension];

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                ordinateValues[j++] = _coordinates[i][0];
                ordinateValues[j++] = _coordinates[i][1];
                if (hasZ) ordinateValues[j++] = _coordinates[i][2];
                if (hasM) ordinateValues[j++] = _coordinates[i][3];
            }

            return dimension;
        }

        /// <summary>
        /// Converts the contents of this <see cref="CoordinateBuffer"/> to an array of <see cref="Ordinate"/> values.
        /// </summary>
        /// <returns>The number of dimensions and an array of <see cref="double"/>s</returns>
        public int ToPackedArray(out float[] ordinateValues)
        {
            var hasZ = HasZ;
            var hasM = HasM;
            var dimension = Dimension;
            ordinateValues = new float[Count * dimension];

            var j = 0;
            for (var i = 0; i < _coordinates.Count; i++)
            {
                ordinateValues[j++] = (float)_coordinates[i][0];
                ordinateValues[j++] = (float)_coordinates[i][1];
                if (hasZ) ordinateValues[j++] = (float)_coordinates[i][2];
                if (hasM) ordinateValues[j++] = (float)_coordinates[i][3];
            }

            return dimension;
        }

        /// <summary>
        /// Checks of <paramref name="other"/> <see cref="CoordinateBuffer"/> is equal to this.
        /// </summary>
        /// <param name="other">The coordinate buffer to test.</param>
        /// <returns><c>true</c> if the coordinates in this buffer match those of other.</returns>
        public bool Equals(CoordinateBuffer other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other == null)
                return false;
            if (other.DefinedOrdinates != DefinedOrdinates)
                return false;
            if (other.Count != Count)
                return false;

            for (var i = 0; i < _coordinates.Count; i++)
            {
                if (_coordinates[i][0] != other._coordinates[i][0] ||
                    _coordinates[i][1] != other._coordinates[i][1] ||
                    !_coordinates[i][2].Equals(other._coordinates[i][2]) ||
                    !_coordinates[i][3].Equals(other._coordinates[i][3]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks a coordinate sequence for equality with this 
        /// </summary>
        /// <param name="other">The coordinate sequence to test</param>
        /// <returns><c>true</c> if the coordinates in the coordinate sequence are equal to those in this buffer.</returns>
        public bool Equals(ICoordinateSequence other)
        {
            if (other == null)
                return false;

            /*
            if (other.Ordinates != DefinedOrdinates)
                return false;
            */
            if (other.Count != Count)
                return false;

            for (var i = 0; i < _coordinates.Count; i++)
            {
                if (_coordinates[i][0] != other.GetOrdinate(i, Ordinate.X) ||
                    _coordinates[i][1] != other.GetOrdinate(i, Ordinate.Y))
                    return false;

                if (HasZ)
                {
                    if ((other.Ordinates & Ordinates.Z) == Ordinates.Z)
                        if (!_coordinates[i][2].Equals(other.GetOrdinate(i, Ordinate.Z))) return false;

                    if (HasM && (other.Ordinates & Ordinates.M) == Ordinates.M)
                        if (!_coordinates[i][3].Equals(other.GetOrdinate(i, Ordinate.M))) return false;
                }
                else
                {
                    if (HasM && (other.Ordinates & Ordinates.Z) == Ordinates.Z)
                        if (!_coordinates[i][3].Equals(other.GetOrdinate(i, Ordinate.Z))) return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return 685146 ^ _coordinates.Count ^ _extents.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("CoordinateBuffer: {0} coordinates, Extent {1}, Z-{2}, M-{3}",
                _coordinates.Count, _extents, _zInterval, _mInterval);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is CoordinateBuffer))
                return false;
            return Equals((CoordinateBuffer)obj);
        }

        /// <summary>
        /// Creates a coordinate sequence, that has all possibly repeated points removed
        /// </summary>
        /// <param name="checkZM">Controls if z- and m-values are to be considered in the equality check.</param>
        /// <returns>A coordinate buffer without repeated points</returns>
        public CoordinateBuffer RemoveRepeated(bool checkZM = false)
        {
            var res = new CoordinateBuffer(_coordinates.Count, Coordinate.NullOrdinate);
            foreach (var coordinate in _coordinates)
                res.AddCoordinate(coordinate[0], coordinate[1], coordinate[2], coordinate[3], checkZM);
            return res;
        }

        private static bool Equals(double[] c1, double[] c2, bool checkZM = false)
        {
            if (c1[0] != c2[0]) return false;
            if (c1[1] != c2[1]) return false;
            if (!checkZM)
                return true;

            if (!c1[2].Equals(c2[2])) return false;
            if (!c1[3].Equals(c2[3])) return false;
            return true;
        }

        // ReSharper restore ImpureMethodCallOnReadonlyValueField

    }
}