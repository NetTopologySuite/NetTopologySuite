using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Coverage
{
    internal class CoverageRing : BasicSegmentString
    {
        public static List<CoverageRing> CreateRings(Geometry geom)
        {
            var polygons = Extracter.GetPolygons(geom);
            return CreateRings(polygons);
        }

        public static List<CoverageRing> CreateRings(IList<Polygon> polygons)
        {
            var rings = new List<CoverageRing>();
            foreach (var poly in polygons)
            {
                CreateRings(poly, rings);
            }
            return rings;
        }

        private static void CreateRings(Polygon poly, IList<CoverageRing> rings)
        {
            rings.Add(CreateRing(poly.ExteriorRing, true));
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                rings.Add(CreateRing(poly.GetInteriorRingN(i), false));
            }
        }

        private static CoverageRing CreateRing(LineString ring, bool isShell)
        {
            var pts = ring.Coordinates;
            bool isCCW = Orientation.IsCCW(pts);
            bool isInteriorOnRight = isShell ? !isCCW : isCCW;
            return new CoverageRing(pts, isInteriorOnRight);
        }

        /// <remarks>Named <c>isValid</c> in JTS</remarks>
        public static bool AllRingsValid(IList<CoverageRing> rings)
        {
            foreach (var ring in rings)
            {
                if (!ring.IsValid)
                    return false;
            }
            return true;
        }

        private readonly bool _isInteriorOnRight;
        private readonly bool[] _isInvalid;
        private readonly bool[] _isValid;

        public CoverageRing(Coordinate[] pts, bool isInteriorOnRight)
                : base(pts, null)
        {
            _isInteriorOnRight = isInteriorOnRight;
            _isInvalid = new bool[Count - 1];
            _isValid = new bool[Count - 1];
        }

        public bool IsInteriorOnRight => _isInteriorOnRight;

        /// <summary>
        /// Tests if a segment is marked valid.
        /// </summary>
        /// <param name="index">The segment index</param>
        /// <returns><c>true</c> if the segment is valid.</returns>
        /// <remarks>Named <c>isValid</c> in JTS</remarks>
        public bool IsValidAt(int index)
        {
            return _isValid[index];
        }

        /// <summary>
        /// Tests if a segment is marked invalid.
        /// </summary>
        /// <param name="index">The segment index</param>
        /// <returns><c>true</c> if the segment is invalid.</returns>
        /// <remarks>Named <c>isInvalid</c> in JTS</remarks>
        public bool IsInvalidAt(int index)
        {
            return _isInvalid[index];
        }

        /// <summary>
        /// Gets a value indicating whether all segments are valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                for (int i = 0; i < _isValid.Length; i++)
                {
                    if (!_isValid[i])
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all segments are invalid.
        /// </summary>
        public bool IsInvalid
        {
            get
            {
                for (int i = 0; i < _isInvalid.Length; i++)
                {
                    if (!_isInvalid[i])
                        return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether any segment is invalid.
        /// </summary>
        public bool HasInvalid
        {
            get
            {
                for (int i = 0; i < _isInvalid.Length; i++)
                {
                    if (_isInvalid[i])
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Tests whether the validity state of a ring segment is known.
        /// </summary>
        /// <param name="i">The index of the ring segment</param>
        /// <returns><c>true</c> if the segment validity state is known.</returns>
        public bool IsKnown(int i)
        {
            return _isValid[i] || _isInvalid[i];
        }

        /// <summary>
        /// Finds the previous vertex in the ring which is distinct from a given coordinate value.
        /// </summary>
        /// <param name="index">The index to start the search</param>
        /// <param name="pt">A coordinate value (which may not be a ring vertex)</param>
        /// <returns>The previous distinct vertex in the ring</returns>
        public Coordinate FindVertexPrev(int index, Coordinate pt)
        {
            int iPrev = index;
            var prev = Coordinates[iPrev];
            while (pt.Equals2D(prev))
            {
                iPrev = Prev(iPrev);
                prev = Coordinates[iPrev];
            }
            return prev;
        }

        /// <summary>
        /// Finds the next vertex in the ring which is distinct from a given coordinate value.
        /// </summary>
        /// <param name="index">The index to start the search</param>
        /// <param name="pt">A coordinate value (which may not be a ring vertex)</param>
        /// <returns>The next distinct vertex in the ring</returns>
        public Coordinate FindVertexNext(int index, Coordinate pt)
        {
            //-- safe, since index is always the start of a segment
            int iNext = index + 1;
            var next = Coordinates[iNext];
            while (pt.Equals2D(next))
            {
                iNext = Next(iNext);
                next = Coordinates[iNext];
            }
            return next;
        }

        /// <summary>
        /// Gets the index of the previous segment in the ring.
        /// </summary>
        /// <param name="index">A segment index</param>
        /// <returns>The index of the previous segment</returns>
        public int Prev(int index)
        {
            if (index == 0)
                return Count - 2;
            return index - 1;
        }

        /// <summary>
        /// Gets the index of the next segment in the ring.
        /// </summary>
        /// <param name="index">A segment index</param>
        /// <returns>The index of the next segment</returns>
        public int Next(int index)
        {
            if (index < Count - 2)
                return index + 1;
            return 0;
        }

        /// <summary>
        /// Marks a segment as invalid.
        /// </summary>
        /// <param name="i">The segment index</param>
        public void MarkInvalid(int i)
        {
            if (_isValid[i])
                throw new InvalidOperationException("Setting valid edge to invalid");
            _isInvalid[i] = true;
        }

        /// <summary>
        /// Marks a segment as valid.
        /// </summary>
        /// <param name="i">The segment index</param>
        public void MarkValid(int i)
        {
            if (_isInvalid[i])
                throw new InvalidOperationException("Setting invalid edge to valid");
            _isValid[i] = true;
        }

        public void CreateInvalidLines(GeometryFactory geomFactory, List<LineString> lines)
        {
            //-- empty case
            if (!HasInvalid)
            {
                return;
            }
            //-- entire ring case
            if (IsInvalid)
            {
                var line = CreateLine(0, Count - 1, geomFactory);
                lines.Add(line);
                return;
            }

            //-- find first end after index 0, to allow wrap-around
            int startIndex = FindInvalidStart(0);
            int firstEndIndex = FindInvalidEnd(startIndex);
            int endIndex = firstEndIndex;
            while (true)
            {
                startIndex = FindInvalidStart(endIndex);
                endIndex = FindInvalidEnd(startIndex);
                var line = CreateLine(startIndex, endIndex, geomFactory);
                lines.Add(line);
                if (endIndex == firstEndIndex)
                    break;
            }
        }

        private int FindInvalidStart(int index)
        {
            while (!IsInvalidAt(index))
            {
                index = NextMarkIndex(index);
            }
            return index;
        }

        private int FindInvalidEnd(int index)
        {
            index = NextMarkIndex(index);
            while (IsInvalidAt(index))
            {
                index = NextMarkIndex(index);
            }
            return index;
        }

        private int NextMarkIndex(int index)
        {
            if (index >= _isInvalid.Length - 1)
            {
                return 0;
            }
            return index + 1;
        }

        /// <summary>
        /// Creates a line from a sequence of ring segments between startIndex and endIndex (inclusive).
        /// If the endIndex &lt; startIndex the sequence wraps around the ring endpoint.</summary>
        /// <returns>A line representing the section</returns>
        private LineString CreateLine(int startIndex, int endIndex, GeometryFactory geomFactory)
        {
            var pts = endIndex < startIndex ?
                  ExtractSectionWrap(startIndex, endIndex)
                : ExtractSection(startIndex, endIndex);
            return geomFactory.CreateLineString(pts);
        }

        private Coordinate[] ExtractSection(int startIndex, int endIndex)
        {
            int size = endIndex - startIndex + 1;
            var pts = new Coordinate[size];
            int ipts = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                pts[ipts++] = Coordinates[i].Copy();
            }
            return pts;
        }

        private Coordinate[] ExtractSectionWrap(int startIndex, int endIndex)
        {
            int size = endIndex + (Count - startIndex);
            var pts = new Coordinate[size];
            int index = startIndex;
            for (int i = 0; i < size; i++)
            {
                pts[i] = Coordinates[index].Copy();
                index = NextMarkIndex(index);
            }
            return pts;
        }

    }
}
