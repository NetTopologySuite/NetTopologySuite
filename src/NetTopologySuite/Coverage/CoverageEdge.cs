using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;

namespace NetTopologySuite.Coverage
{
    /// <summary>
    /// An edge of a polygonal coverage formed from all or a section of a polygon ring.
    /// An edge may be a free ring, which is a ring which has not node points
    /// (i.e.does not touch any other rings in the parent coverage).
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class CoverageEdge
    {

        public static CoverageEdge CreateEdge(LinearRing ring)
        {
            var pts = ExtractEdgePoints(ring, 0, ring.NumPoints - 1);
            return new CoverageEdge(pts, true);
        }

        public static CoverageEdge CreateEdge(LinearRing ring, int start, int end)
        {
            var pts = ExtractEdgePoints(ring, start, end);
            return new CoverageEdge(pts, false);
        }

        private static Coordinate[] ExtractEdgePoints(LinearRing ring, int start, int end)
        {
            int size = start < end
                          ? end - start + 1
                          : ring.NumPoints - start + end;
            var pts = new Coordinate[size];
            int iring = start;
            for (int i = 0; i < size; i++)
            {
                pts[i] = ring.GetCoordinateN(iring).Copy();
                iring += 1;
                if (iring >= ring.NumPoints) iring = 1;
            }
            return pts;
        }

        /// <summary>
        /// Computes a key segment for a ring.
        /// The key is the segment starting at the lowest vertex,
        /// towards the lowest adjacent distinct vertex.
        /// </summary>
        /// <param name="ring">A linear ring</param>
        /// <returns>A LineSegment representing the key</returns>
        public static LineSegment Key(LinearRing ring)
        {
            var pts = ring.Coordinates;
            // find lowest vertex index
            int indexLow = 0;
            for (int i = 1; i < pts.Length - 1; i++)
            {
                if (pts[indexLow].CompareTo(pts[i]) < 0)
                    indexLow = i;
            }
            var key0 = pts[indexLow];
            // find distinct adjacent vertices
            var adj0 = FindDistinctPoint(pts, indexLow, true, key0);
            var adj1 = FindDistinctPoint(pts, indexLow, false, key0);
            var key1 = adj0.CompareTo(adj1) < 0 ? adj0 : adj1;
            return new LineSegment(key0, key1);
        }

        /// <summary>
        /// Computes a distinct key for a section of a linear ring.
        /// </summary>
        /// <param name="ring">A linear ring</param>
        /// <param name="start">The index of the start of the section</param>
        /// <param name="end">The end index of the end of the section</param>
        /// <returns>A LineSegment representing the key</returns>
        public static LineSegment Key(LinearRing ring, int start, int end)
        {
            var pts = ring.Coordinates;
            //-- endpoints are distinct in a line edge
            var end0 = pts[start];
            var end1 = pts[end];
            bool isForward = 0 > end0.CompareTo(end1);
            Coordinate key0, key1;
            if (isForward)
            {
                key0 = end0;
                key1 = FindDistinctPoint(pts, start, true, key0);
            }
            else
            {
                key0 = end1;
                key1 = FindDistinctPoint(pts, end, false, key0);
            }
            return new LineSegment(key0, key1);
        }

        private static Coordinate FindDistinctPoint(Coordinate[] pts, int index, bool isForward, Coordinate pt)
        {
            int inc = isForward ? 1 : -1;
            int i = index;
            do
            {
                if (!pts[i].Equals2D(pt))
                {
                    return pts[i];
                }
                // increment index with wrapping
                i += inc;
                if (i < 0)
                {
                    i = pts.Length - 1;
                }
                else if (i > pts.Length - 1)
                {
                    i = 0;
                }
            } while (i != index);
            throw new ArgumentException("Edge does not contain distinct points");
        }

        private Coordinate[] _pts;
        private int _ringCount = 0;
        private readonly bool _isFreeRing;

        public CoverageEdge(Coordinate[] pts, bool isFreeRing)
        {
            _pts = pts;
            _isFreeRing = isFreeRing;
        }

        public void IncrementRingCount()
        {
            _ringCount++;
        }

        public int RingCount => _ringCount;

        /// <summary>
        /// Gets a value indicating if this edge is a free ring;
        /// i.e.one with no constrained nodes.
        /// </summary>
        public bool IsFreeRing { get => _isFreeRing; }

        public Coordinate[] Coordinates
        {
            get => _pts;
            set => _pts = value;
        }

        public Coordinate EndCoordinate
        {
            get => _pts[_pts.Length - 1];
        }

        public Coordinate StartCoordinate
        {
            get => _pts[0];
        }

        public override string ToString()
        {
            return WKTWriter.ToLineString(_pts);
        }
    }

}
