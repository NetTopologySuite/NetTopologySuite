using System;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// The <c>ICoordinateSequence</c> implementation that <see cref="Geometry{TCoordinate}"/>s use by default.
    /// In this implementation, Coordinates returned by ToArray and Coordinate are live --
    /// modifications to them are actually changing the
    /// CoordinateSequence's underlying data.
    /// </summary>
    [Serializable]
    public class CoordinateArraySequence<TCoordinate> : ICoordinateSequence<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private TCoordinate[] _coordinates;

        /// <summary>
        /// Constructs a sequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordinates">The coordinate array that will be referenced.</param>
        public CoordinateArraySequence(TCoordinate[] coordinates)
        {
            _coordinates = coordinates;

            if (coordinates == null)
            {
                _coordinates = new TCoordinate[0];
            }
        }

        /// <summary>
        /// Constructs a sequence of a given size, populated with new Coordinates.
        /// </summary>
        /// <param name="size">The size of the sequence to create.</param>
        public CoordinateArraySequence(Int32 size)
        {
            _coordinates = new TCoordinate[size];

            for (Int32 i = 0; i < size; i++)
            {
                _coordinates[i] = default(TCoordinate);
            }
        }

        /// <summary>
        /// Constructs a sequence based on the given array (the array is not copied).
        /// </summary>
        /// <param name="coordSeq">The coordinate array that will be referenced.</param>      
        public CoordinateArraySequence(ICoordinateSequence coordSeq)
        {
            if (coordSeq != null)
            {
                _coordinates = new TCoordinate[coordSeq.Count];
            }
            else
            {
                _coordinates = new TCoordinate[0];
            }

            for (Int32 i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = coordSeq.GetCoordinateCopy(i);
            }
        }

        /// <summary>
        /// Returns the dimension (number of ordinates in each coordinate) for this sequence.
        /// </summary>
        /// <value></value>
        public Int32 Dimension
        {
            get { return 3; }
        }

        /// <summary>
        /// Get the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>The requested Coordinate instance.</returns>
        public ICoordinate GetCoordinate(Int32 i)
        {
            return _coordinates[i];
        }

        /// <summary>
        /// Get a copy of the Coordinate with index i.
        /// </summary>
        /// <param name="i">The index of the coordinate.</param>
        /// <returns>A copy of the requested Coordinate.</returns>
        public virtual ICoordinate GetCoordinateCopy(Int32 i)
        {
            return new Coordinate(_coordinates[i]);
        }

        /// <summary>
        /// Copies the i'th coordinate in the sequence to the supplied Coordinate.
        /// Only the first two dimensions are copied.
        /// </summary>
        /// <param name="index">The index of the coordinate to copy.</param>
        /// <param name="coord">A Coordinate to receive the value.</param>
        public void GetCoordinate(Int32 index, ICoordinate coord)
        {
            coord.X = _coordinates[index].X;
            coord.Y = _coordinates[index].Y;
        }

        /// <summary>
        /// Returns ordinate X (0) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the X ordinate in the index'th coordinate.
        /// </returns>
        public Double GetX(Int32 index)
        {
            return _coordinates[index].X;
        }

        /// <summary>
        /// Returns ordinate Y (1) of the specified coordinate.
        /// </summary>
        /// <returns>
        /// The value of the Y ordinate in the index'th coordinate.
        /// </returns>
        public Double GetY(Int32 index)
        {
            return _coordinates[index].Y;
        }

        /// <summary>
        /// Returns the ordinate of a coordinate in this sequence.
        /// Ordinate indices 0 and 1 are assumed to be X and Y.
        /// Ordinates indices greater than 1 have user-defined semantics
        /// (for instance, they may contain other dimensions or measure values).
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        public Double GetOrdinate(Int32 index, Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    return _coordinates[index].X;
                case Ordinates.Y:
                    return _coordinates[index].Y;
                case Ordinates.Z:
                    return _coordinates[index].Z;
                default:
                    return Double.NaN;
            }
        }

        /// <summary>
        /// Creates a deep copy of the object.
        /// </summary>
        /// <returns>The deep copy.</returns>
        public virtual object Clone()
        {
            ICoordinate[] cloneCoordinates = GetClonedCoordinates();
            return new CoordinateArraySequence<TCoordinate>(cloneCoordinates);
        }

        protected ICoordinate[] GetClonedCoordinates()
        {
            ICoordinate[] cloneCoordinates = new ICoordinate[Count];

            for (Int32 i = 0; i < _coordinates.Length; i++)
            {
                cloneCoordinates[i] = (ICoordinate) _coordinates[i].Clone();
            }

            return cloneCoordinates;
        }

        /// <summary>
        /// Returns the length of the coordinate sequence.
        /// </summary>
        public Int32 Count
        {
            get { return _coordinates.Length; }
        }

        /// <summary>
        /// Sets the value for a given ordinate of a coordinate in this sequence.
        /// </summary>
        /// <param name="index">The coordinate index in the sequence.</param>
        /// <param name="ordinate">The ordinate index in the coordinate (in range [0, dimension-1]).</param>
        /// <param name="value">The new ordinate value.</param>
        public void SetOrdinate(Int32 index, Ordinates ordinate, Double value)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                    _coordinates[index].X = value;
                    break;
                case Ordinates.Y:
                    _coordinates[index].Y = value;
                    break;
                case Ordinates.Z:
                    _coordinates[index].Z = value;
                    break;
                default:
                    throw new ArgumentException("invalid ordinate index: " + ordinate);
            }
        }

        /// <summary>
        ///This method exposes the internal Array of Coordinate Objects.       
        /// </summary>
        public TCoordinate[] ToCoordinateArray()
        {
            return _coordinates;
        }

        /// <summary>
        /// Expands the given Envelope to include the coordinates in the sequence.
        /// Allows implementing classes to optimize access to coordinate values.
        /// </summary>
        /// <param name="extents">The envelope to expand.</param>
        /// <returns>A reference to the expanded envelope.</returns>
        public IExtents ExpandEnvelope(IExtents extents)
        {
            foreach (TCoordinate coordinate in _coordinates)
            {
                extents.ExpandToInclude(coordinate);
            }

            return extents;
        }

        /// <summary>
        /// Returns the string representation of the coordinate array.
        /// </summary>
        public override string ToString()
        {
            if (_coordinates.Length > 0)
            {
                StringBuilder strBuf = new StringBuilder(17*_coordinates.Length);
                strBuf.Append('(');
                strBuf.Append(_coordinates[0]);

                foreach (TCoordinate coordinate in _coordinates)
                {
                    strBuf.Append(", ");
                    strBuf.Append(coordinate);
                }

                strBuf.Append(')');
                return strBuf.ToString();
            }
            else
            {
                return "()";
            }
        }

        #region ICoordinateSequence<TCoordinate> Members

        public TCoordinate[] ToArray()
        {
            throw new NotImplementedException();
        }

        public IExtents<TCoordinate> ExpandEnvelope(IExtents<TCoordinate> extents)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList<TCoordinate> Members

        public int IndexOf(TCoordinate item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, TCoordinate item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TCoordinate this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<TCoordinate> Members

        public void Add(TCoordinate item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TCoordinate item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TCoordinate[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(TCoordinate item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<TCoordinate> Members

        public System.Collections.Generic.IEnumerator<TCoordinate> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinateSequence Members


        ICoordinate[] ICoordinateSequence.ToArray()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IList<ICoordinate> Members

        public int IndexOf(ICoordinate item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ICoordinate item)
        {
            throw new NotImplementedException();
        }

        ICoordinate System.Collections.Generic.IList<ICoordinate>.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<ICoordinate> Members

        public void Add(ICoordinate item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(ICoordinate item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ICoordinate[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ICoordinate item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<ICoordinate> Members

        System.Collections.Generic.IEnumerator<ICoordinate> System.Collections.Generic.IEnumerable<ICoordinate>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}