using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Computes the outer or inner hull of a ring
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RingHull
    {

        private LinearRing _inputRing;
        private int _targetVertexNum = -1;
        private double _targetAreaDelta = -1;

        /*
         * The ring vertices are oriented so that
         * for corners which are to be kept 
         * the vertices forming the corner are in CW orientation.
         */
        private LinkedRing _vertexRing;
        private double _areaDelta = 0;

        /*
         * Indexing vertices improves corner intersection testing performance.
         * The ring vertices are contiguous, so are suitable for a
         * {@link VertexSequencePackedRtree}.
         */
        private VertexSequencePackedRtree _vertexIndex;

        private PriorityQueue<Corner> _cornerQueue;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="ring">The ring vertices to process</param>
        /// <param name="isOuter">A flag whether the hull is outer or inner</param>
        public RingHull(LinearRing ring, bool isOuter)
        {
            _inputRing = ring;
            Init(ring.Coordinates, isOuter);
        }

        public int MinVertexNum { get => _targetVertexNum; set => _targetVertexNum = value; }

        public double MaxAreaDelta { get => _targetAreaDelta; set => _targetAreaDelta = value; }

        public Envelope Envelope => _inputRing.EnvelopeInternal;

        public VertexSequencePackedRtree VertexIndex => _vertexIndex;

        public LinearRing GetHull(RingHullIndex hullIndex)
        {
            Compute(hullIndex);
            var hullPts = _vertexRing.Coordinates;
            return _inputRing.Factory.CreateLinearRing(hullPts);
        }

        private void Init(Coordinate[] ring, bool isOuter)
        {
            /*
             * Ensure ring is oriented according to outer/inner:
             * - outer, CW
             * - inner: CCW 
             */
            bool orientCW = isOuter;
            if (orientCW == Orientation.IsCCW(ring))
            {
                ring = (Coordinate[])ring.Clone();
                CoordinateArrays.Reverse(ring);
            }

            _vertexRing = new LinkedRing(ring);
            _vertexIndex = new VertexSequencePackedRtree(ring);
            //-- remove duplicate final vertex
            _vertexIndex.RemoveAt(ring.Length - 1);

            _cornerQueue = new PriorityQueue<Corner>();
            for (int i = 0; i < _vertexRing.Count; i++)
            {
                AddCorner(i, _cornerQueue);
            }
        }

        private void AddCorner(int i, PriorityQueue<Corner> cornerQueue)
        {
            //-- convex corners are left untouched
            if (IsConvex(_vertexRing, i))
                return;
            //-- corner is concave or flat - both can be removed
            var corner = new Corner(i,
                _vertexRing.Prev(i),
                _vertexRing.Next(i),
                Area(_vertexRing, i));
            cornerQueue.Add(corner);
        }

        public static bool IsConvex(LinkedRing vertexRing, int index)
        {
            var pp = vertexRing.PrevCoordinate(index);
            var p = vertexRing.GetCoordinate(index);
            var pn = vertexRing.NextCoordinate(index);
            return OrientationIndex.Clockwise == Orientation.Index(pp, p, pn);
        }

        public static double Area(LinkedRing vertexRing, int index)
        {
            var pp = vertexRing.PrevCoordinate(index);
            var p = vertexRing.GetCoordinate(index);
            var pn = vertexRing.NextCoordinate(index);
            return Triangle.Area(pp, p, pn);
        }

        public void Compute(RingHullIndex hullIndex)
        {
            while (!_cornerQueue.IsEmpty()
                && _vertexRing.Count > 3)
            {
                var corner = _cornerQueue.Poll();
                //-- a corner may no longer be valid due to removal of adjacent corners
                if (corner.IsRemoved(_vertexRing))
                    continue;
                if (IsAtTarget(corner))
                    return;
                //System.out.println(corner.toLineString(vertexList));
                /*
                 * Corner is concave or flat - remove it if possible.
                 */
                if (IsRemovable(corner, hullIndex))
                {
                    RemoveCorner(corner, _cornerQueue);
                }
            }
        }

        private bool IsAtTarget(Corner corner)
        {
            if (_targetVertexNum >= 0)
            {
                return _vertexRing.Count < _targetVertexNum;
            }
            if (_targetAreaDelta >= 0)
            {
                //-- include candidate corder to avoid overshooting target
                // (important for very small target area deltas)
                return _areaDelta + corner.Area > _targetAreaDelta;
            }
            //-- no target set
            return true;
        }

        /// <summary>
        /// Removes a corner by removing the apex vertex from the ring.
        /// Two new corners are created with apexes
        /// at the other vertices of the corner
        /// (if they are non-convex and thus removable).
        /// </summary>
        /// <param name="corner">The corner to remove</param>
        /// <param name="cornerQueue">The corner queue</param>
        private void RemoveCorner(Corner corner, PriorityQueue<Corner> cornerQueue)
        {
            int index = corner.Index;
            int prev = _vertexRing.Prev(index);
            int next = _vertexRing.Next(index);
            _vertexRing.RemoveAt(index);
            _vertexIndex.RemoveAt(index);
            _areaDelta += corner.Area;

            //-- potentially add the new corners created
            AddCorner(prev, cornerQueue);
            AddCorner(next, cornerQueue);
        }

        private bool IsRemovable(Corner corner, RingHullIndex hullIndex)
        {
            var cornerEnv = corner.Envelope(_vertexRing);
            if (HasIntersectingVertex(corner, cornerEnv, this))
                return false;
            //-- no other rings to check
            if (hullIndex == null)
                return true;
            //-- check other rings for intersections
            foreach (var hull in hullIndex.Query(cornerEnv))
            {
                //-- this hull was already checked above
                if (hull == this)
                    continue;
                if (HasIntersectingVertex(corner, cornerEnv, hull))
                    return false;
            }
            return true;
        }

        /**
         * Tests if any vertices in a hull intersect the corner triangle.
         * Uses the vertex spatial index for efficiency.
         * 
         * @param corner the corner vertices
         * @param cornerEnv the envelope of the corner
         * @param hull the hull to test
         * @return true if there is an intersecting vertex
         */
        private bool HasIntersectingVertex(Corner corner, Envelope cornerEnv,
            RingHull hull)
        {
            int[] result = hull.Query(cornerEnv);
            for (int i = 0; i < result.Length; i++)
            {
                int index = result[i];
                //-- skip vertices of corner
                if (hull == this && corner.IsVertex(index))
                    continue;

                var v = hull.GetCoordinate(index);
                //--- does corner triangle contain vertex?
                if (corner.Intersects(v, _vertexRing))
                    return true;
            }
            return false;
        }

        private Coordinate GetCoordinate(int index)
        {
            return _vertexRing.GetCoordinate(index);
        }

        private int[] Query(Envelope cornerEnv)
        {
            return _vertexIndex.Query(cornerEnv);
        }

        private void QueryHull(Envelope queryEnv, IList<Coordinate> pts)
        {
            int[] result = _vertexIndex.Query(queryEnv);

            for (int i = 0; i < result.Length; i++)
            {
                int index = result[i];
                //-- skip if already removed
                if (!_vertexRing.HasCoordinate(index))
                    continue;
                var v = _vertexRing.GetCoordinate(index);
                pts.Add(v);
            }

        }

        public Polygon ToGeometry()
        {
            var fact = _inputRing.Factory;
            var coords = _vertexRing.Coordinates;
            return fact.CreatePolygon(fact.CreateLinearRing(coords));
        }

        private class Corner : IComparable<Corner>
        {
            private int index;
            private int prev;
            private int next;
            private double area;

            public Corner(int i, int prev, int next, double area)
            {
                this.index = i;
                this.prev = prev;
                this.next = next;
                this.area = area;
            }

            public bool IsVertex(int index)
            {
                return index == this.index
                    || index == prev
                    || index == next;
            }

            public int Index => index;

            public double Area => area;

            /**
             * Orders corners by increasing area
             */
            public int CompareTo(Corner o)
            {
                return area.CompareTo(o.area);
            }

            public Envelope Envelope(LinkedRing ring)
            {
                var pp = ring.GetCoordinate(prev);
                var p = ring.GetCoordinate(index);
                var pn = ring.GetCoordinate(next);
                var env = new Envelope(pp, pn);
                env.ExpandToInclude(p);
                return env;
            }

            public bool Intersects(Coordinate v, LinkedRing ring)
            {
                var pp = ring.GetCoordinate(prev);
                var p = ring.GetCoordinate(index);
                var pn = ring.GetCoordinate(next);
                return Triangle.Intersects(pp, p, pn, v);
            }

            public bool IsRemoved(LinkedRing ring)
            {
                return ring.Prev(index) != prev || ring.Next(index) != next;
            }

            public LineString ToLineString(LinkedRing ring)
            {
                var pp = ring.GetCoordinate(prev);
                var p = ring.GetCoordinate(index);
                var pn = ring.GetCoordinate(next);
                return NtsGeometryServices.Instance.CreateGeometryFactory().CreateLineString(
                    new Coordinate[] { safeCoord(pp), safeCoord(p), safeCoord(pn) });
            }

            private static Coordinate safeCoord(Coordinate p)
            {
                if (p == null) return new Coordinate(Coordinate.NullOrdinate, Coordinate.NullOrdinate);
                return p;
            }
        }
    }
}
