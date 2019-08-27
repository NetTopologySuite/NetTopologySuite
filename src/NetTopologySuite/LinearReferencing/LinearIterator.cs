//using System.Collections;

using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// An iterator over the components and coordinates of a linear geometry
    /// (<see cref="LineString" />s and <see cref="MultiLineString" />s.
    /// </summary>
    public class LinearIterator //: IEnumerator<LinearIterator.LinearElement>,
    //    IEnumerable<LinearIterator.LinearElement>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        public static int SegmentEndVertexIndex(LinearLocation loc)
        {
            if (loc.SegmentFraction > 0.0)
                return loc.SegmentIndex + 1;
            return loc.SegmentIndex;
        }

        private readonly Geometry _linearGeom;
        private readonly int _numLines;

        /// <summary>
        /// Invariant: currentLine &lt;&gt; null if the iterator is pointing at a valid coordinate
        /// </summary>
        private LineString _currentLine;
        private int _componentIndex;
        private int _vertexIndex;

        //// Used for avoid the first call to Next() in MoveNext()
        //private bool _atStart;

        //// Returned by Ienumerator.Current
        //private LinearElement _current;

        //// Cached start values - for Reset() call
        //private readonly int _startComponentIndex;
        //private readonly int _startVertexIndex;

        /// <summary>
        /// Creates an iterator initialized to the start of a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to iterate over.</param>
        /// <exception cref="ArgumentException"> if <paramref name="linearGeom"/> is not <see cref="ILineal"/></exception>
        public LinearIterator(Geometry linearGeom) : this(linearGeom, 0, 0) { }

        /// <summary>
        /// Creates an iterator starting at a <see cref="LinearLocation" /> on a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to iterate over.</param>
        /// <param name="start">The location to start at.</param>
        /// <exception cref="ArgumentException"> if <paramref name="linearGeom"/> is not <see cref="ILineal"/></exception>
        public LinearIterator(Geometry linearGeom, LinearLocation start) :
            this(linearGeom, start.ComponentIndex, SegmentEndVertexIndex(start)) { }

        /// <summary>
        /// Creates an iterator starting at
        /// a component and vertex in a linear <see cref="Geometry" />.
        /// </summary>
        /// <param name="linearGeom">The linear geometry to iterate over.</param>
        /// <param name="componentIndex">The component to start at.</param>
        /// <param name="vertexIndex">The vertex to start at.</param>
        /// <exception cref="ArgumentException"> if <paramref name="linearGeom"/> is not <see cref="ILineal"/></exception>
        public LinearIterator(Geometry linearGeom, int componentIndex, int vertexIndex)
        {
            if (!(linearGeom is ILineal))
                throw new ArgumentException("Lineal geometry is required.");
            _linearGeom = linearGeom;
            _numLines = linearGeom.NumGeometries;

            _componentIndex = componentIndex;
            _vertexIndex = vertexIndex;

            LoadCurrentLine();
        }

        /// <summary>
        ///
        /// </summary>
        private void LoadCurrentLine()
        {
            if (_componentIndex >= _numLines)
            {
                _currentLine = null;
                return;
            }
            _currentLine = (LineString)_linearGeom.GetGeometryN(_componentIndex);
        }

        /// <summary>
        /// Tests whether there are any vertices left to iterator over.
        /// Specifically, <c>HasNext()</c> returns <tt>true</tt> if the
        /// current state of the iterator represents a valid location
        /// on the linear geometry.
        /// </summary>
        /// <returns><c>true</c> if there are more vertices to scan.</returns>
        public bool HasNext()
        {
            if (_componentIndex >= _numLines)
                return false;
            if (_componentIndex == _numLines - 1 &&
                _vertexIndex >= _currentLine.NumPoints)
                return false;
            return true;
        }

        /// <summary>
        /// Jump to the next element of the iteration.
        /// </summary>
        public void Next()
        {
            if (!HasNext())
                return;

            _vertexIndex++;
            if (_vertexIndex >= _currentLine.NumPoints)
            {
                _componentIndex++;
                LoadCurrentLine();
                _vertexIndex = 0;
            }
        }

        /// <summary>
        /// Checks whether the iterator cursor is pointing to the
        /// endpoint of a component <see cref="LineString"/>.
        /// </summary>
        public bool IsEndOfLine
        {
            get
            {
                if (_componentIndex >= _numLines)
                    return false;
                if (_vertexIndex < _currentLine.NumPoints - 1)
                    return false;
                return true;
            }
        }

        /// <summary>
        /// The component index of the vertex the iterator is currently at.
        /// </summary>
        public int ComponentIndex => _componentIndex;

        /// <summary>
        /// The vertex index of the vertex the iterator is currently at.
        /// </summary>
        public int VertexIndex => _vertexIndex;

        /// <summary>
        /// Gets the <see cref="LineString" /> component the iterator is current at.
        /// </summary>
        public LineString Line => _currentLine;

        /// <summary>
        /// Gets the first <see cref="Coordinate" /> of the current segment
        /// (the coordinate of the current vertex).
        /// </summary>
        public Coordinate SegmentStart => _currentLine.GetCoordinateN(_vertexIndex);

        /// <summary>
        /// Gets the second <see cref="Coordinate" /> of the current segment
        /// (the coordinate of the next vertex).
        /// If the iterator is at the end of a line, <c>null</c> is returned.
        /// </summary>
        public Coordinate SegmentEnd
        {
            get
            {
                if (_vertexIndex < Line.NumPoints - 1)
                    return _currentLine.GetCoordinateN(_vertexIndex + 1);
                return null;
            }
        }

        //#region IEnumerator<LinearIterator.LinearElement> Members

        ///// <summary>
        ///// Gets the <see cref="LinearElement">element</see> in the collection
        ///// at the current position of the enumerator.
        ///// </summary>
        ///// <value></value>
        ///// <returns>
        ///// The <see cref="LinearElement">element</see> in the collection
        ///// at the current position of the enumerator.
        ///// </returns>
        //public LinearElement Current
        //{
        //    get
        //    {
        //        return _current;
        //    }
        //}

        //#region IEnumerator Members

        ///// <summary>
        ///// Tests whether there are any vertices left to iterator over.
        ///// If <c>true</c>, then moves the iterator ahead to the next vertex and (possibly) linear component,
        ///// so that <see cref="Current" /> exposes the elements.
        ///// </summary>
        ///// <returns><c>true</c> if there are more vertices to scan.</returns>
        //public bool MoveNext()
        //{
        //    // We must call HasNext() twice because, when in the Next() method
        //    // another line is loaded, it's necessary to re-ckeck with the new conditions.
        //    if (HasNext())
        //    {
        //        if (_atStart)
        //            _atStart = false;
        //        else Next();
        //    }
        //    return HasNext();
        //}

        ///// <summary>
        ///// Gets the <see cref="LinearElement">element</see> in the collection
        ///// at the current position of the enumerator.
        ///// </summary>
        ///// <value></value>
        ///// <returns>
        ///// The <see cref="LinearElement">element</see> in the collection
        ///// at the current position of the enumerator.
        ///// </returns>
        //object System.Collections.IEnumerator.Current
        //{
        //    get
        //    {
        //        return Current;
        //    }
        //}

        ///// <summary>
        ///// Sets the enumerator to its initial position,
        ///// which is before the first element in the collection.
        ///// </summary>
        ///// <exception cref="T:System.InvalidOperationException">
        ///// The collection was modified after the enumerator was created.
        ///// </exception>
        //public void Reset()
        //{
        //    _numLines = _linearGeom.NumGeometries;
        //    _componentIndex = _startComponentIndex;
        //    _vertexIndex = _startVertexIndex;
        //    LoadCurrentLine();

        //    _atStart = true;
        //}

        //#endregion IEnumerator Members

        //#region IDisposable Members

        ///// <summary>
        ///// Performs application-defined tasks associated with freeing,
        ///// releasing, or resetting unmanaged resources.
        ///// </summary>
        //public void Dispose()
        //{
        //    Dispose(false);
        //}

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="dispose"></param>
        //protected void Dispose(bool dispose)
        //{
        //    if (dispose)
        //    {
        //        // Dispose unmanaged resources
        //    }

        //    // Dispose managed resources
        //    _current = null;
        //    _currentLine = null;
        //}

        //#endregion IDisposable Members

        //#region IEnumerable<LinearIterator.LinearElement> Members

        ///// <summary>
        ///// Returns an enumerator that iterates through the collection.
        ///// </summary>
        ///// <returns>
        ///// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used
        ///// to iterate through the collection.
        ///// </returns>
        //public IEnumerator<LinearElement> GetEnumerator()
        //{
        //    return this;
        //}

        //#endregion IEnumerable<LinearIterator.LinearElement> Members

        //#region IEnumerable Members

        ///// <summary>
        ///// Returns an enumerator (of <see cref="LinearElement" />elements)
        ///// that iterates through a collection.
        ///// </summary>
        ///// <returns>
        ///// An <see cref="T:System.Collections.IEnumerator"></see> object
        ///// that can be used to iterate through the collection.
        ///// </returns>
        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return GetEnumerator();
        //}

        //#endregion IEnumerable Members

        //#region LinearElement

        ///// <summary>
        ///// A class that exposes <see cref="LinearIterator" /> elements.
        ///// </summary>
        //public class LinearElement
        //{
        //    private readonly LinearIterator _iterator;

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="LinearElement"/> class.
        //    /// </summary>
        //    /// <param name="iterator">The iterator.</param>
        //    public LinearElement(LinearIterator iterator)
        //    {
        //        _iterator = iterator;
        //    }

        //    /// <summary>
        //    /// The component index of the vertex the iterator is currently at.
        //    /// </summary>
        //    public int ComponentIndex
        //    {
        //        get
        //        {
        //            return _iterator.ComponentIndex;
        //        }
        //    }

        //    /// <summary>
        //    /// The vertex index of the vertex the iterator is currently at.
        //    /// </summary>
        //    public int VertexIndex
        //    {
        //        get
        //        {
        //            return _iterator.VertexIndex;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the <see cref="LineString" /> component the iterator is current at.
        //    /// </summary>
        //    public LineString Line
        //    {
        //        get
        //        {
        //            return _iterator.Line;
        //        }
        //    }

        //    /// <summary>
        //    /// Checks whether the iterator cursor is pointing to the
        //    /// endpoint of a linestring.
        //    /// </summary>
        //    public bool IsEndOfLine
        //    {
        //        get
        //        {
        //            return _iterator.IsEndOfLine;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the first <see cref="Coordinate" /> of the current segment
        //    /// (the coordinate of the current vertex).
        //    /// </summary>
        //    public Coordinate SegmentStart
        //    {
        //        get
        //        {
        //            return _iterator.SegmentStart;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the second <see cref="Coordinate" /> of the current segment
        //    /// (the coordinate of the next vertex).
        //    /// If the iterator is at the end of a line, <c>null</c> is returned.
        //    /// </summary>
        //    public Coordinate SegmentEnd
        //    {
        //        get
        //        {
        //            return _iterator.SegmentEnd;
        //        }
        //    }
        //}

        //#endregion LinearElement
    }
}