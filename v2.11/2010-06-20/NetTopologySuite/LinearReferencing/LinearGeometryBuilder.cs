using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Builds a linear geometry (<see cref="ILineString{TCoordinate}" /> 
    /// or <see cref="IMultiLineString{TCoordinate}" />)
    /// incrementally (point-by-point).
    /// </summary>
    public class LinearGeometryBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private readonly List<IGeometry<TCoordinate>> _lines = new List<IGeometry<TCoordinate>>();
        private List<TCoordinate> _coordList;

        private Boolean _fixInvalidLines;
        private Boolean _ignoreInvalidLines;

        private TCoordinate _lastPt;

        public LinearGeometryBuilder(IGeometryFactory<TCoordinate> geomFact)
        {
            _geometryFactory = geomFact;
        }

        /// <summary>
        /// Allows invalid lines to be fixed rather than causing Exceptions.
        /// An invalid line is one which has only one unique point.
        /// </summary>
        public Boolean FixInvalidLines
        {
            get { return _fixInvalidLines; }
            set { _fixInvalidLines = value; }
        }

        /// <summary>
        /// Allows invalid lines to be ignored rather than causing Exceptions.
        /// An invalid line is one which has only one unique point.
        /// </summary>
        public Boolean IgnoreInvalidLines
        {
            get { return _ignoreInvalidLines; }
            set { _ignoreInvalidLines = value; }
        }

        public TCoordinate LastCoordinate
        {
            get { return _lastPt; }
        }

        /// <summary>
        /// Adds a point to the current line.
        /// </summary>
        /// <param name="pt">The <typeparamref name="TCoordinate"/> to add.</param>
        public void Add(TCoordinate pt)
        {
            Add(pt, true);
        }

        /// <summary>
        /// Adds a point to the current line.
        /// </summary>
        /// <param name="pt">The <typeparamref name="TCoordinate"/> to add.</param>
        /// <param name="allowRepeatedPoints">If <see langword="true"/>, allows the insertions of repeated points.</param>
        public void Add(TCoordinate pt, Boolean allowRepeatedPoints)
        {
            if (_coordList == null)
            {
                _coordList = new List<TCoordinate>();
            }

            if (allowRepeatedPoints || _coordList.IndexOf(pt) < 0)
            {
                _coordList.Add(pt);
            }

            _lastPt = pt;
        }

        /// <summary>
        /// Terminate the current <see cref="ILineString{TCoordinate}" />.
        /// </summary>
        public void EndLine()
        {
            if (_coordList == null)
            {
                return;
            }

            if (_ignoreInvalidLines && _coordList.Count < 2)
            {
                _coordList = null;
                return;
            }

            IEnumerable<TCoordinate> rawPts = _coordList;
            IEnumerable<TCoordinate> pts = rawPts;

            if (FixInvalidLines)
            {
                pts = validCoordinateSequence(rawPts);
            }

            _coordList = null;
            ILineString<TCoordinate> line = null;

            try
            {
                line = _geometryFactory.CreateLineString(pts);
            }
            catch (ArgumentException)
            {
                // exception is due to too few points in line.
                // only propagate if not ignoring short lines
                if (!IgnoreInvalidLines)
                {
                    throw;
                }
            }

            if (line != null)
            {
                _lines.Add(line);
            }
        }

        /// <summary>
        /// Builds and returns the <see cref="Geometry{TCoordinate}" />.
        /// </summary>
        /// <returns></returns>
        public IGeometry<TCoordinate> GetGeometry()
        {
            // end last line in case it was not done by user
            EndLine();

            return _geometryFactory.BuildGeometry(_lines);
        }

        private static IEnumerable<TCoordinate> validCoordinateSequence(IEnumerable<TCoordinate> pts)
        {
            if (Slice.CountGreaterThan(pts, 1))
            {
                return pts;
            }

            TCoordinate coordinate = Slice.GetFirst(pts);
            return Slice.Append(pts, coordinate);
        }
    }
}