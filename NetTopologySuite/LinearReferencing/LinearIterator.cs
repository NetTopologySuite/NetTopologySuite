using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// An iterator over the components and coordinates of a linear geometry
    /// (<see cref="ILineString{TCoordinate}" />s 
    /// and <see cref="IMultiLineString{TCoordinate}" />s.
    /// </summary>
    public class LinearIterator<TCoordinate> : IEnumerable<LinearIterator<TCoordinate>.LinearElement>
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                IComputable<TCoordinate>, IConvertible
    {
        private static Int32 SegmentEndVertexIndex(LinearLocation loc)
        {
            if (loc.SegmentFraction > 0.0)
            {
                return loc.SegmentIndex + 1;
            }
            return loc.SegmentIndex;
        }

        private readonly IGeometry<TCoordinate> _linear;
        private Int32 _lineCount;

        /*
         * Invariant: currentLine <> null if the iterator is pointing at a valid coordinate
         */
        private ILineString<TCoordinate> _currentLine;

        private Int32 _componentIndex = 0;
        private Int32 _vertexIndex = 0;

        // Used for avoid the first call to Next() in MoveNext()
        private Boolean _atStart;

        // Returned by Ienumerator.Current
        private LinearElement _current = null;

        // Cached start values - for Reset() call
        private readonly Int32 _startComponentIndex = 0;
        private readonly Int32 _startVertexIndex = 0;

        /// <summary>
        /// Creates an iterator initialized to the start of a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linear">The linear geometry to iterate over.</param>
        public LinearIterator(IGeometry<TCoordinate> linear) : this(linear, 0, 0) { }

        /// <summary>
        /// Creates an iterator starting at a <see cref="LinearLocation" /> on a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linear">The linear geometry to iterate over.</param>
        /// <param name="start">The location to start at.</param>
        public LinearIterator(IGeometry<TCoordinate> linear, LinearLocation start) :
            this(linear, start.ComponentIndex, SegmentEndVertexIndex(start)) {}

        /// <summary>
        /// Creates an iterator starting at
        /// a component and vertex in a linear <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <param name="linear">The linear geometry to iterate over.</param>
        /// <param name="componentIndex">The component to start at.</param>
        /// <param name="vertexIndex">The vertex to start at.</param>
        public LinearIterator(IGeometry<TCoordinate> linear, Int32 componentIndex, Int32 vertexIndex)
        {
            _startComponentIndex = componentIndex;
            _startVertexIndex = vertexIndex;

            this._linear = linear;
            Reset();

            _current = new LinearElement(this);
        }

        private void LoadCurrentLine()
        {
            if (_componentIndex >= _lineCount)
            {
                _currentLine = null;
                return;
            }

            _currentLine = (ILineString) _linear.GetGeometryN(_componentIndex);
        }

        /// <summary>
        /// Evaluate if the iterator could step over.
        /// Does not perform the step at all.
        /// </summary>
        /// <returns></returns>
        /// <returns><see langword="true"/> if there are more vertices to scan.</returns>
        protected Boolean HasNext()
        {
            if (_componentIndex >= _lineCount)
            {
                return false;
            }

            if ((_componentIndex == _lineCount - 1) && (_vertexIndex >= _currentLine.PointCount))
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Jump to the next element of the iteration.
        /// </summary>
        protected void Next()
        {
            if (!HasNext())
            {
                return;
            }

            _vertexIndex++;

            if (_vertexIndex >= _currentLine.PointCount)
            {
                _componentIndex++;
                LoadCurrentLine();
                _vertexIndex = 0;
            }
        }

        /// <summary>
        /// Checks whether the iterator cursor is pointing to the
        /// endpoint of a linestring.
        /// </summary>
        private Boolean IsEndOfLine
        {
            get
            {
                if (_componentIndex >= _lineCount)
                {
                    return false;
                }

                if (_vertexIndex < _currentLine.PointCount - 1)
                {
                    return false;
                }
                
                return true;
            }
        }

        /// <summary>
        /// The component index of the vertex the iterator is currently at.
        /// </summary>
        private Int32 ComponentIndex
        {
            get { return _componentIndex; }
        }

        /// <summary>
        /// The vertex index of the vertex the iterator is currently at.
        /// </summary>
        private Int32 VertexIndex
        {
            get { return _vertexIndex; }
        }

        /// <summary>
        /// Gets the <see cref="LineString" /> component the iterator is current at.
        /// </summary>
        private ILineString<TCoordinate> Line
        {
            get { return _currentLine; }
        }

        /// <summary>
        /// Gets the first <see cref="Coordinate" /> of the current segment
        /// (the coordinate of the current vertex).
        /// </summary>
        private TCoordinate SegmentStart
        {
            get { return _currentLine.Coordinates[_vertexIndex]; }
        }

        /// <summary>
        /// Gets the second <see cref="Coordinate" /> of the current segment
        /// (the coordinate of the next vertex).
        /// If the iterator is at the end of a line, <see langword="null" /> is returned.
        /// </summary>
        private TCoordinate SegmentEnd
        {
            get
            {
                if (_vertexIndex < Line.PointCount - 1)
                {
                    return _currentLine.Coordinates[_vertexIndex + 1];
                }

                return default(TCoordinate);
            }
        }

        #region IEnumerator<LinearIterator.LinearElement> Members

        /// <summary>
        /// Gets the <see cref="LinearElement">element</see> in the collection 
        /// at the current position of the enumerator.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="LinearElement">element</see> in the collection 
        /// at the current position of the enumerator.
        /// </returns>
        public LinearElement Current
        {
            get { return _current; }
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Tests whether there are any vertices left to iterator over.
        /// If <see langword="true"/>, then moves the iterator ahead to the next vertex and (possibly) linear component,
        /// so that <see cref="Current" /> exposes the elements.
        /// </summary>
        /// <returns><see langword="true"/> if there are more vertices to scan.</returns>
        public Boolean MoveNext()
        {
            // We must call HasNext() twice because, when in the Next() method
            // another line is loaded, it's necessary to re-ckeck with the new conditions.
            if (HasNext())
            {
                if (_atStart)
                {
                    _atStart = false;
                }
                else
                {
                    Next();
                }
            }
            return HasNext();
        }

        /// <summary>
        /// Gets the <see cref="LinearElement">element</see> in the collection 
        /// at the current position of the enumerator.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="LinearElement">element</see> in the collection 
        /// at the current position of the enumerator.
        /// </returns>
        object IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Sets the enumerator to its initial position, 
        /// which is before the first element in the collection.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// The collection was modified after the enumerator was created. 
        /// </exception>
        public void Reset()
        {
            _lineCount = _linear.NumGeometries;
            _componentIndex = _startComponentIndex;
            _vertexIndex = _startVertexIndex;
            LoadCurrentLine();

            _atStart = true;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(false);
        }

        protected void Dispose(Boolean dispose)
        {
            if (dispose)
            {
                // Dispose unmanaged resources
            }

            // Dispose managed resources
            _current = null;
            _currentLine = null;
        }

        #endregion

        #region IEnumerable<LinearIterator.LinearElement> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Collections.Generic.IEnumerator`1"></see> that can be used 
        /// to iterate through the collection.
        /// </returns>
        public IEnumerator<LinearElement> GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator (of <see cref="LinearElement" />elements) 
        /// that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IEnumerator"></see> object 
        /// that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region LinearElement

        /// <summary>
        /// A class that exposes <see cref="LinearIterator" /> elements.
        /// </summary>
        public class LinearElement
        {
            private readonly LinearIterator<TCoordinate> _iterator = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="LinearElement"/> class.
            /// </summary>
            /// <param name="iterator">The iterator.</param>
            public LinearElement(LinearIterator<TCoordinate> iterator)
            {
                this._iterator = iterator;
            }

            /// <summary>
            /// The component index of the vertex the iterator is currently at.
            /// </summary>
            public Int32 ComponentIndex
            {
                get { return _iterator.ComponentIndex; }
            }

            /// <summary>
            /// The vertex index of the vertex the iterator is currently at.
            /// </summary>
            public Int32 VertexIndex
            {
                get { return _iterator.VertexIndex; }
            }

            /// <summary>
            /// Gets the <see cref="ILineString{TCoordinate}" /> component the iterator is current at.
            /// </summary>
            public ILineString<TCoordinate> Line
            {
                get { return _iterator.Line; }
            }

            /// <summary>
            /// Checks whether the iterator cursor is pointing to the
            /// endpoint of a linestring.
            /// </summary>
            public Boolean IsEndOfLine
            {
                get { return _iterator.IsEndOfLine; }
            }

            /// <summary>
            /// Gets the first <typeparamref name="TCoordinate"/> of the current segment
            /// (the coordinate of the current vertex).
            /// </summary>
            public TCoordinate SegmentStart
            {
                get { return _iterator.SegmentStart; }
            }

            /// <summary>
            /// Gets the second <typeparamref name="TCoordinate"/> of the current segment
            /// (the coordinate of the next vertex).
            /// If the iterator is at the end of a line, <see langword="null" /> is returned.
            /// </summary>
            public TCoordinate SegmentEnd
            {
                get { return _iterator.SegmentEnd; }
            }
        }

        #endregion
    }
}