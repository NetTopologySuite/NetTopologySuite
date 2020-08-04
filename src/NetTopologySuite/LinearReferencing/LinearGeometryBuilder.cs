using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.LinearReferencing
{
    /// <summary>
    /// Builds a linear geometry (<see cref="LineString" /> or <see cref="MultiLineString" />)
    /// incrementally (point-by-point).
    /// </summary>
    public class LinearGeometryBuilder
    {
        private readonly GeometryFactory _geomFact;
        private readonly List<Geometry> _lines = new List<Geometry>();
        private CoordinateList _coordList;

        private Coordinate _lastPt;

        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <param name="geomFact">The geometry factory to use.</param>
        public LinearGeometryBuilder(GeometryFactory geomFact)
        {
            _geomFact = geomFact;
        }

        /// <summary>
        /// Allows invalid lines to be fixed rather than causing Exceptions.
        /// An invalid line is one which has only one unique point.
        /// </summary>
        public bool FixInvalidLines { get; set; }

        /// <summary>
        /// Allows invalid lines to be ignored rather than causing Exceptions.
        /// An invalid line is one which has only one unique point.
        /// </summary>
        public bool IgnoreInvalidLines { get; set; }

        /// <summary>
        /// Adds a point to the current line.
        /// </summary>
        /// <param name="pt">The <see cref="Coordinate" /> to add.</param>
        public void Add(Coordinate pt)
        {
            Add(pt, true);
        }

        /// <summary>
        /// Adds a point to the current line.
        /// </summary>
        /// <param name="pt">The <see cref="Coordinate" /> to add.</param>
        /// <param name="allowRepeatedPoints">If <c>true</c>, allows the insertions of repeated points.</param>
        public void Add(Coordinate pt, bool allowRepeatedPoints)
        {
            if (_coordList == null)
                _coordList = new CoordinateList();
            _coordList.Add(pt, allowRepeatedPoints);
            _lastPt = pt;
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate LastCoordinate => _lastPt;

        /// <summary>
        /// Terminate the current <see cref="LineString" />.
        /// </summary>
        public void EndLine()
        {
            if (_coordList == null)
                return;

            if (IgnoreInvalidLines && _coordList.Count < 2)
            {
                _coordList = null;
                return;
            }

            var rawPts = _coordList.ToCoordinateArray();
            var pts = rawPts;
            if (FixInvalidLines)
                pts = ValidCoordinateSequence(rawPts);

            _coordList = null;
            LineString line = null;
            try
            {
                line = _geomFact.CreateLineString(pts);
            }
            catch (ArgumentException ex)
            {
                // exception is due to too few points in line.
                // only propagate if not ignoring short lines
                if (!IgnoreInvalidLines)
                    throw ex;
            }

            if (line != null)
                _lines.Add(line);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        private static Coordinate[] ValidCoordinateSequence(Coordinate[] pts)
        {
            if (pts.Length >= 2)
                return pts;
            var validPts = new[] { pts[0], pts[0] };
            return validPts;
        }

        /// <summary>
        /// Builds and returns the <see cref="Geometry" />.
        /// </summary>
        /// <returns></returns>
        public Geometry GetGeometry()
        {
            // end last line in case it was not done by user
            EndLine();
            return _geomFact.BuildGeometry(_lines);
        }

    }
}
