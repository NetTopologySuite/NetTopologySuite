using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.Tri;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace NetTopologySuite.Algorithm.Hull
{
    /// <summary>
    /// Tris which are used to form a concave hull.
    /// If a Tri has an edge (or edges) with no adjacent tri
    /// the tri is on the boundary of the triangulation.
    /// The edge is a boundary edge.
    /// The union of those edges
    /// forms the (linear) boundary of the triangulation.
    /// The triangulation area may be a Polygon or MultiPolygon, and may or may not contain holes.
    /// </summary>
    /// <author>Martin Davis</author>
    class HullTri : Tri, IComparable<HullTri>, IComparable
    {
        public HullTri(Coordinate p0, Coordinate p1, Coordinate p2)
            : base(p0, p1, p2)
        {
            Size = LengthOfLongestEdge;
        }

        public double Size { get; set; }

        /// <summary>
        /// Sets the size to be the length of the boundary edges.
        /// This is used when constructing hull without holes,
        /// by erosion from the triangulation boundary.
        /// </summary>
        public void SetSizeToBoundary()
        {
            Size = LengthOfBoundary;
        }

        public void SetSizeToLongestEdge()
        {
            Size = LengthOfLongestEdge;
        }

        public void SetSizeToCircumradius()
        {
            Size = Triangle.Circumradius(P2, P1, P0);
        }

        public bool IsMarked { get; set; }

        public bool IsRemoved => !HasAdjacent();

        /// <summary>
        /// Gets an index of a boundary edge, if there is one.
        /// </summary>
        /// <returns>A boundary edge index, or -1</returns>
        public int BoundaryIndex
        {
            get
            {
                if (IsBoundary(0)) return 0;
                if (IsBoundary(1)) return 1;
                if (IsBoundary(2)) return 2;
                return -1;
            }
        }

        /// <summary>
        /// Gets the most CCW boundary edge index.
        /// This assumes there is at least one non-boundary edge.
        /// </summary>
        /// <returns>The CCW boundary edge index</returns>
        public int BoundaryIndexCCW
        {
            get
            {
                int index = BoundaryIndex;
                if (index < 0) return -1;
                int prevIndex = Prev(index);
                if (IsBoundary(prevIndex))
                {
                    return prevIndex;
                }
                return index;
            }
        }

        /// <summary>
        /// Gets the most CW boundary edge index.
        /// This assumes there is at least one non-boundary edge.
        /// </summary>
        /// <returns>The CW boundary edge index</returns>
        public int BoundaryIndexCW
        {
            get
            {
                int index = BoundaryIndex;
                if (index < 0) return -1;
                int nextIndex = Next(index);
                if (IsBoundary(nextIndex))
                {
                    return nextIndex;
                }
                return index;
            }
        }

        /// <summary>
        /// Tests if this tri is the only one connecting its 2 adjacents.
        /// Assumes that the tri is on the boundary of the triangulation
        /// and that the triangulation does not contain holes
        /// </summary>
        /// <returns><c>true</c> if the tri is the only connection</returns>
        public bool IsConnecting
        {
            get
            {
                int adj2Index = Adjacent2VertexIndex();
                bool isInterior = IsInteriorVertex(adj2Index);
                return !isInterior;
            }
        }

        /// <summary>
        /// Gets the index of a vertex which is adjacent to two other tris (if any).
        /// </summary>
        /// <returns>The vertex index or -1</returns>
        public int Adjacent2VertexIndex()
        {
            if (HasAdjacent(0) && HasAdjacent(1)) return 1;
            if (HasAdjacent(1) && HasAdjacent(2)) return 2;
            if (HasAdjacent(2) && HasAdjacent(0)) return 0;
            return -1;
        }

        /// <summary>
        /// Tests whether some vertex of this Tri has degree = 1.
        /// In this case it is not in any other Tris.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <returns><c>true</c> if any vertex of this tri has a degree of 1</returns>
        public int IsolatedVertexIndex(IList<Tri> triList)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Degree(i, triList) <= 1)
                    return i;
            }
            return -1;
        }

        public double LengthOfLongestEdge
        {
            get => Triangle.LongestSideLength(P0, P1, P2);
        }

        public double LengthOfBoundary
        {
            get
            {
                double len = 0.0;
                for (int i = 0; i < 3; i++)
                {
                    if (!HasAdjacent(i))
                    {
                        len += GetCoordinate(i).Distance(GetCoordinate(Tri.Next(i)));
                    }
                }
                return len;
            }
        }

        /// <summary>
        /// Sorts tris in decreasing order.
        /// Since PriorityQueues sort in <i>ascending</i> order,
        /// to sort with the largest at the head,
        /// smaller sizes must compare as greater than larger sizes.
        /// (i.e. the normal numeric comparison is reversed).
        /// If the sizes are identical (which should be an infrequent case),
        /// the areas are compared, with larger areas sorting before smaller.
        /// (The rationale is that larger areas indicate an area of lower point density,
        /// which is more likely to be in the exterior of the computed hull.)
        /// This improves the determinism of the queue ordering. 
        /// </summary>
        public int CompareTo(HullTri o)
        {
            /*
             * If size is identical compare areas to ensure a (more) deterministic ordering.
             * Larger areas sort before smaller ones.
             */
            if (Size == o.Size)
            {
                return -Area.CompareTo(o.Area);
            }
            return -Size.CompareTo(o.Size);
        }

        int IComparable.CompareTo(object o)
        {
            return CompareTo((HullTri)o);
        }

        /// <summary>
        /// Tests if this tri has a vertex which is in the boundary,
        /// but not in a boundary edge.
        /// </summary>
        /// <returns><c>true</c> if the tri touches the boundary at a vertex</returns>
        public bool HasBoundaryTouch
        {
            get
            {
                for (int i = 0; i < 3; i++)
                {
                    if (IsBoundaryTouch(i))
                        return true;
                }
                return false;
            }
        }

        private bool IsBoundaryTouch(int index)
        {
            //-- If vertex is in a boundary edge it is not a touch
            if (IsBoundary(index)) return false;
            if (IsBoundary(Prev(index))) return false;
            //-- if vertex is not in interior it is on boundary
            return !IsInteriorVertex(index);
        }

        public static HullTri FindTri(IEnumerable<Tri> triList, Tri exceptTri)
        {
            foreach (HullTri tri in triList)
                if (tri != exceptTri) return tri;

            return null;
        }

        public static bool AreAllMarked(IEnumerable<Tri> triList)
        {
            foreach (HullTri tri in triList)
                if (!tri.IsMarked)
                    return false;
            return true;
        }

        public static void ClearMarks(IEnumerable<Tri> triList)
        {
            foreach (HullTri tri in triList)
                tri.IsMarked = false;
        }

        public static void MarkConnected(HullTri triStart, Tri exceptTri)
        {
            var queue = new Stack<HullTri>();
            queue.Push(triStart);
            while (queue.Count > 0)
            {
                var tri = queue.Pop();
                tri.IsMarked = true;
                for (int i = 0; i < 3; i++)
                {
                    var adj = (HullTri)tri.GetAdjacent(i);
                    //-- don't connect thru this tri
                    if (adj == exceptTri)
                        continue;
                    if (adj != null && !adj.IsMarked)
                    {
                        queue.Push(adj);
                    }
                }
            }
        }

        /// <summary>
        /// Tests if a triangulation is edge-connected, if a triangle is removed.<br/>
        /// NOTE: this is a relatively slow operation.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        /// <param name="exceptTri">The triangle to remove</param>
        /// <returns><c>true</c> if the triangulation is still connected</returns>
        public static bool IsConnected(IList<Tri> triList, HullTri exceptTri)
        {
            if (triList.Count == 0) return false;
            ClearMarks(triList);
            var triStart = FindTri(triList, exceptTri);
            if (triStart == null) return false;
            MarkConnected(triStart, exceptTri);
            exceptTri.IsMarked = true;
            return AreAllMarked(triList);
        }
    }
}
