using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.Tri
{
    /// <summary>
    /// A memory-efficient representation of a triangle in a triangulation.
    /// Contains three vertices, and links to adjacent <c>Tri</c>s for each edge.
    /// <c>Tri</c>s are constructed independently, and if needed linked
    /// into a triangulation using <see cref="TriangulationBuilder"/>.
    /// <para/>
    /// An edge of a Tri in a triangulation is called a boundary edge
    /// if it has no adjacent triangle.<br/>
    /// The set of Tris containing boundary edges are called the triangulation border.
    /// </summary>
    /// <author>Martin Davis</author>
    public class Tri
    {
        private const string INVALID_TRI_INDEX = "Invalid Tri index: {0}";

        /// <summary>
        /// Creates a <see cref="GeometryCollection"/> of <see cref="Polygon"/>s
        /// representing the triangles in a list.
        /// </summary>
        /// <param name="tris">A collection of <c>Tri</c>s</param>
        /// <param name="geomFact">The GeometryFactory to use</param>
        /// <returns>The <c>Polygon</c>s for the triangles</returns>
        public static Geometry ToGeometry(ICollection<Tri> tris, GeometryFactory geomFact)
        {
            var geoms = new Geometry[tris.Count];
            int i = 0;
            foreach (var tri in tris)
            {
                geoms[i++] = tri.ToPolygon(geomFact);
            }
            return geomFact.CreateGeometryCollection(geoms);
        }

        /// <summary>
        /// Computes the area of a set of Tris.
        /// </summary>
        /// <param name="triList">A set of tris</param>
        /// <returns>The total area of the triangles</returns>
        public static double AreaOf(IEnumerable<Tri> triList)
        {
            double area = 0;
            foreach (var tri in triList)
            {
                area += tri.Area;
            }
            return area;
        }


        /// <summary>
        /// Validates a list of <c>Tri</c>s.
        /// </summary>
        /// <param name="triList">The list of <c>Tri</c>s to validate</param>
        public static void Validate(IEnumerable<Tri> triList)
        {
            foreach (var tri in triList)
            {
                tri.Validate();
            }
        }

        /// <summary>
        /// Creates a triangle with the given vertices.
        /// The vertices should be oriented clockwise.
        /// </summary>
        /// <param name="p0">The first triangle vertex</param>
        /// <param name="p1">The second triangle vertex</param>
        /// <param name="p2">The third triangle vertex</param>
        /// <returns>The created trianlge</returns>
        public static Tri Create(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            return new Tri(p0, p1, p2);
        }

        /// <summary>
        /// Creates a triangle from an array with three vertex coordinates.
        /// The vertices should be oriented clockwise.
        /// </summary>
        /// <param name="pts">The array of vertex coordinates</param>
        /// <returns>The created triangle</returns>
        public static Tri Create(Coordinate[] pts)
        {
            return new Tri(pts[0], pts[1], pts[2]);
        }

        private Coordinate _p0;
        /// <summary>
        /// Gets a value indicating the 1st point of this <c>Tri</c>.
        /// </summary>
        protected Coordinate P0 => _p0;
        private Coordinate _p1;
        /// <summary>
        /// Gets a value indicating the 2nd point of this <c>Tri</c>.
        /// </summary>
        protected Coordinate P1 => _p1;
        private Coordinate _p2;
        /// <summary>
        /// Gets a value indicating the 3rd point of this <c>Tri</c>.
        /// </summary>
        protected Coordinate P2 => _p2;

        /*
         * triN is the adjacent triangle across the edge pN - pNN.
         * pNN is the next vertex CW from pN.
         */
        private Tri _tri0;
        /// <summary>
        /// Gets a value indicating the adjacent <c>Tri</c> across the edge <see cref="P0"/> clockwise towards the next point.
        /// </summary>
        protected Tri Tri0 => _tri0;
        private Tri _tri1;
        /// <summary>
        /// Gets a value indicating the adjacent <c>Tri</c> across the edge <see cref="P1"/> clockwise towards the next point.
        /// </summary>
        protected Tri Tri1 => _tri1;
        private Tri _tri2;
        /// <summary>
        /// Gets a value indicating the adjacent <c>Tri</c> across the edge <see cref="P2"/> clockwise towards the next point.
        /// </summary>
        protected Tri Tri2 => _tri2;

        /// <summary>
        /// Creates a triangle with the given vertices.
        /// The vertices should be oriented clockwise.
        /// </summary>
        /// <param name="p0">The first triangle vertex</param>
        /// <param name="p1">The second triangle vertex</param>
        /// <param name="p2">The third triangle vertex</param>
        public Tri(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            //Assert.isTrue( Orientation.CLOCKWISE != Orientation.index(p0, p1, p2), "Tri is not oriented correctly");
        }

        /// <summary>
        /// Sets the adjacent triangles.<br/>
        /// The vertices of the adjacent triangles are
        /// assumed to match the appropriate vertices in this triangle.
        /// </summary>
        /// <param name="tri0">The triangle adjacent to edge 0</param>
        /// <param name="tri1">The triangle adjacent to edge 1</param>
        /// <param name="tri2">The triangle adjacent to edge 2</param>
        public void SetAdjacent(Tri tri0, Tri tri1, Tri tri2)
        {
            _tri0 = tri0;
            _tri1 = tri1;
            _tri2 = tri2;
        }

        /// <summary>
        /// Sets the triangle adjacent to the edge originating
        /// at a given vertex.<br/>
        /// The vertices of the adjacent triangles are
        /// assumed to match the appropriate vertices in this triangle.
        /// </summary>
        /// <param name="pt">The edge start point</param>
        /// <param name="tri">The adjacent triangle</param>
        public void SetAdjacent(Coordinate pt, Tri tri)
        {
            int index = GetIndex(pt);
            SetTri(index, tri);
            // TODO: validate that tri is adjacent at the edge specified
        }

        /// <summary>
        /// Sets the triangle adjacent to an edge.<br/>
        /// The vertices of the adjacent triangle are
        /// assumed to match the appropriate vertices in this triangle.
        /// </summary>
        /// <param name="edgeIndex">The edge triangle is adjacent to</param>
        /// <param name="tri">The adjacent triangle</param>
        public void SetTri(int edgeIndex, Tri tri)
        {
            switch (edgeIndex)
            {
                case 0: _tri0 = tri; return;
                case 1: _tri1 = tri; return;
                case 2: _tri2 = tri; return;
            }
            throw new ArgumentOutOfRangeException(nameof(edgeIndex), string.Format(INVALID_TRI_INDEX, edgeIndex));
        }

        private void SetCoordinates(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            _p0 = p0;
            _p1 = p1;
            _p2 = p2;
            //Assert.isTrue( Orientation.CLOCKWISE != Orientation.index(p0, p1, p2), "Tri is not oriented correctly");
        }

        /// <summary>
        /// Splits a triangle by a point located inside the triangle.
        /// Creates the three new resulting triangles with adjacent links
        /// set correctly.
        /// Returns the new triangle whose 0'th vertex is the splitting point.
        /// </summary>
        /// <param name="p">The point to insert</param>
        /// <returns>The new triangle whose 0'th vertex is <paramref name="p"/></returns>
        public Tri Split(Coordinate p)
        {
            var tt0 = new Tri(p, _p0, _p1);
            var tt1 = new Tri(p, _p1, _p2);
            var tt2 = new Tri(p, _p2, _p0);
            tt0.SetAdjacent(tt2, _tri0, tt1);
            tt1.SetAdjacent(tt0, _tri1, tt2);
            tt2.SetAdjacent(tt1, _tri2, tt0);
            return tt0;
        }

        /// <summary>
        /// Interchanges the vertices of this triangle and a neighbor
        /// so that their common edge
        /// becomes the the other diagonal of the quadrilateral they form.
        /// Neighbour triangle links are modified accordingly.
        /// </summary>
        /// <param name="index">The index of the adjacent tri to flip with</param>
        public void Flip(int index)
        {
            var tri = GetAdjacent(index);
            int index1 = tri.GetIndex(this);

            var adj0 = GetCoordinate(index);
            var adj1 = GetCoordinate(Next(index));
            var opp0 = GetCoordinate(OppVertex(index));
            var opp1 = tri.GetCoordinate(OppVertex(index1));

            Flip(tri, index, index1, adj0, adj1, opp0, opp1);
        }

        private void Flip(Tri tri, int index0, int index1, Coordinate adj0, Coordinate adj1, Coordinate opp0, Coordinate opp1)
        {
            //System.out.println("Flipping: " + this + " -> " + tri);

            //validate();
            //tri.validate();

            SetCoordinates(opp1, opp0, adj0);
            tri.SetCoordinates(opp0, opp1, adj1);
            /*
             *  Order: 0: opp0-adj0 edge, 1: opp0-adj1 edge, 
             *  2: opp1-adj0 edge, 3: opp1-adj1 edge
             */
            var adjacent = GetAdjacentTris(tri, index0, index1);
            SetAdjacent(tri, adjacent[0], adjacent[2]);
            //--- update the adjacent triangles with new adjacency
            if (adjacent[2] != null)
            {
                adjacent[2].Replace(tri, this);
            }
            tri.SetAdjacent(this, adjacent[3], adjacent[1]);
            if (adjacent[1] != null)
            {
                adjacent[1].Replace(this, tri);
            }
            //validate();
            //tri.validate();
        }

        /// <summary>
        /// Replaces an adjacent triangle with a different one.
        /// </summary>
        /// <param name="triOld">An adjacent triangle</param>
        /// <param name="triNew">The triangle to replace with</param>
        private void Replace(Tri triOld, Tri triNew)
        {
            if (_tri0 != null && _tri0 == triOld)
            {
                _tri0 = triNew;
            }
            else if (_tri1 != null && _tri1 == triOld)
            {
                _tri1 = triNew;
            }
            else if (_tri2 != null && _tri2 == triOld)
            {
                _tri2 = triNew;
            }
        }

        /// <summary>
        /// Computes the degree of a Tri vertex, which is the number of tris containing it.
        /// This must be done by searching the entire triangulation,
        /// since the containing tris may not be adjacent or edge-connected. 
        /// </summary>
        /// <param name="index">The vertex index</param>
        /// <param name="triList">The triangulation</param>
        /// <returns>The degree of the vertex</returns>
        public int Degree(int index, IList<Tri> triList)
        {
            var v = GetCoordinate(index);
            int degree = 0;
            foreach (var tri in triList)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (v.Equals2D(tri.GetCoordinate(i)))
                        degree++;
                }
            }
            return degree;
        }

        /// <summary>
        /// Removes this tri from the triangulation containing it.
        /// All links between the tri and adjacent ones are nulled.
        /// </summary>
        /// <param name="triList">The triangulation</param>
        public void Remove(IList<Tri> triList)
        {
            Remove();
            triList.Remove(this);
        }

        /// <summary>
        /// Removes this triangle from a triangulation.
        /// All adjacent references and the references to this
        /// Tri in the adjacent Tris are set to <c>null</c>.
        /// </summary>
        public void Remove()
        {
            Remove(0);
            Remove(1);
            Remove(2);
        }

        private void Remove(int index)
        {
            var adj = GetAdjacent(index);
            if (adj == null) return;
            adj.SetTri(adj.GetIndex(this), null);
            SetTri(index, null);
        }

        /// <summary>
        /// Gets the triangles adjacent to the quadrilateral
        /// formed by this triangle and an adjacent one.
        /// The triangles are returned in the following order:
        /// <para/>
        /// Order:
        /// <list type="number">
        /// <item><description>opp0-adj0 edge</description></item>
        /// <item><description>opp0-adj1 edge</description></item>
        /// <item><description>opp1-adj0 edge</description></item>
        /// <item><description>opp1-adj1 edge</description></item>
        /// </list>
        /// </summary>
        /// <param name="triAdj">An adjacent triangle</param>
        /// <param name="index">The index of the common edge in this triangle</param>
        /// <param name="indexAdj">The index of the common edge in the adjacent triangle</param>
        /// <returns></returns>
        private Tri[] GetAdjacentTris(Tri triAdj, int index, int indexAdj)
        {
            var adj = new Tri[4];
            adj[0] = GetAdjacent(Prev(index));
            adj[1] = GetAdjacent(Next(index));
            adj[2] = triAdj.GetAdjacent(Next(indexAdj));
            adj[3] = triAdj.GetAdjacent(Prev(indexAdj));
            return adj;
        }

        /// <summary>
        /// Validates that a <see cref="Tri"/> is correct.
        /// Currently just checks that orientation is CW.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if <see cref="Tri"/> is not valid</exception>
        public void Validate()
        {
            if (OrientationIndex.Clockwise != Orientation.Index(_p0, _p1, _p2))
            {
                throw new ArgumentException("Tri is not oriented correctly");
            }

            ValidateAdjacent(0);
            ValidateAdjacent(1);
            ValidateAdjacent(2);
        }


        /// <summary>
        /// Validates that the vertices of an adjacent linked triangle are correct.
        /// </summary>
        /// <param name="index">The index of the adjacent triangle</param>
        public void ValidateAdjacent(int index)
        {
            var tri = GetAdjacent(index);
            if (tri == null) return;

            System.Diagnostics.Debug.Assert(IsAdjacent(tri));
            System.Diagnostics.Debug.Assert(tri.IsAdjacent(this));

            var e0 = GetCoordinate(index);
            var e1 = GetCoordinate(Next(index));
            int indexNeighbor = tri.GetIndex(this);
            var n0 = tri.GetCoordinate(indexNeighbor);
            var n1 = tri.GetCoordinate(Next(indexNeighbor));
            Assert.IsTrue(e0.Equals2D(n1), "Edge coord not equal");
            Assert.IsTrue(e1.Equals2D(n0), "Edge coord not equal");

            //--- check that no edges cross
            var li = new RobustLineIntersector();
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var p00 = GetCoordinate(i);
                    var p01 = GetCoordinate(Next(i));
                    var p10 = tri.GetCoordinate(j);
                    var p11 = tri.GetCoordinate(Next(j));
                    li.ComputeIntersection(p00, p01, p10, p11);
                    System.Diagnostics.Debug.Assert(!li.IsProper);
                }
            }
        }

        /**
         * Gets the start and end vertex of the edge adjacent to another triangle.
         * 
         * @param neighbor
         * @return
         */
        /*
        //TODO: define when needed 
        public Coordinate[] getEdge(Tri neighbor) {
          int index = getIndex(neighbor);
          int next = next(index);

          Coordinate e0 = getCoordinate(index);
          Coordinate e1 = getCoordinate(next);
          assert (neighbor.hasCoordinate(e0));
          assert (neighbor.hasCoordinate(e1));
          int iN = neighbor.getIndex(e0);
          int iNPrev = prev(iN);
          assert (neighbor.getIndex(e1) == iNPrev);

          return new Coordinate[] { getCoordinate(index), getCoordinate(next) };
        }

        public Coordinate getEdgeStart(int i) {
          return getCoordinate(i);
        }

        public Coordinate getEdgeEnd(int i) {
          return getCoordinate(next(i));
        }

        public boolean hasCoordinate(Coordinate v) {
          if ( p0.equals(v) || p1.equals(v) || p2.equals(v) ) {
            return true;
          }
          return false;
        }
         */

        /// <summary>
        /// Gets the coordinate for a vertex.
        /// This is the start vertex of the edge.
        /// </summary>
        /// <param name="index">The vertex (edge) index</param>
        /// <returns>The vertex coordinate</returns>
        public Coordinate GetCoordinate(int index)
        {
            switch (index)
            {
                case 0: return _p0;
                case 1: return _p1;
                case 2: return _p2;
            }
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(INVALID_TRI_INDEX, index));

        }

        /// <summary>
        /// Gets the index of the triangle vertex which has a given coordinate (if any).
        /// This is also the index of the edge which originates at the vertex.
        /// </summary>
        /// <param name="p">The coordinate to find</param>
        /// <returns>The vertex index, or -1 if it is not in the triangle</returns>
        public int GetIndex(Coordinate p)
        {
            if (_p0.Equals2D(p))
                return 0;
            if (_p1.Equals2D(p))
                return 1;
            if (_p2.Equals2D(p))
                return 2;
            return -1;
        }

        /// <summary>
        /// Gets the edge index which a triangle is adjacent to (if any),
        /// based on the adjacent triangle link.
        /// </summary>
        /// <param name="tri">The <c>Tri</c> to find</param>
        /// <returns>The index of the edge adjacent to the triangle, or -1 if not found</returns>
        public int GetIndex(Tri tri)
        {
            if (_tri0 == tri)
                return 0;
            if (_tri1 == tri)
                return 1;
            if (_tri2 == tri)
                return 2;
            return -1;
        }

        /// <summary>
        /// Gets the triangle adjacent to an edge.
        /// </summary>
        /// <param name="index">The edge index</param>
        /// <returns>The adjacent triangle (may be <c>null</c>)</returns>
        public Tri GetAdjacent(int index)
        {
            switch (index)
            {
                case 0: return _tri0;
                case 1: return _tri1;
                case 2: return _tri2;
            }
            throw new ArgumentOutOfRangeException(nameof(index), string.Format(INVALID_TRI_INDEX, index));
        }

        /// <summary>
        /// Tests if this tri has any adjacent tris.
        /// </summary>
        /// <returns><c>true</c> if there is at least one adjacent tri</returns>
        public bool HasAdjacent()
        {
            return HasAdjacent(0)
                || HasAdjacent(1) || HasAdjacent(2);
        }

        /// <summary>
        /// Tests if there is an adjacent triangle to an edge.
        /// </summary>
        /// <param name="index">The edge index</param>
        /// <returns><c>true</c> if there is a triangle adjacent to edge</returns>
        public bool HasAdjacent(int index)
        {
            return null != GetAdjacent(index);
        }

        /// <summary>
        /// Tests if a triangle is adjacent to some edge of this triangle.
        /// </summary>
        /// <param name="tri">The triangle to test</param>
        /// <returns><c>true</c> if the triangle is adjacent</returns>
        /// <see cref="GetIndex(Tri)"/>
        public bool IsAdjacent(Tri tri)
        {
            return GetIndex(tri) >= 0;
        }

        /// <summary>
        /// Computes the number of triangle adjacent to this triangle.
        /// This is a number in the range [0,2].</summary>
        /// <returns>The number of adjacent triangles</returns>
        public int NumAdjacent
        {
            get
            {
                int num = 0;
                if (_tri0 != null)
                    num++;
                if (_tri1 != null)
                    num++;
                if (_tri2 != null)
                    num++;
                return num;
            }
        }

        /// <summary>
        /// Tests if a tri vertex is interior.
        /// A vertex of a triangle is interior if it
        /// is fully surrounded by other triangles.
        /// </summary>
        /// <param name="index">The vertex index</param>
        /// <returns><c>true</c> if the vertex is interior</returns>
        public bool IsInteriorVertex(int index)
        {
            var curr = this;
            int currIndex = index;
            do
            {
                var adj = curr.GetAdjacent(currIndex);
                if (adj == null) return false;
                int adjIndex = adj.GetIndex(curr);
                if (adjIndex < 0)
                {
                    throw new Exception("Inconsistent adjacency - invalid triangulation");
                }
                curr = adj;
                currIndex = Tri.Next(adjIndex);
            }
            while (curr != this);
            return true;
        }

        /// <summary>
        /// Tests if a tri contains a boundary edge,
        /// and thus on the border of the triangulation containing it.
        /// </summary>
        /// <returns><c>true</c> if the tri is on the border of the triangulation</returns>
        public bool IsBorder()
        {
            return IsBoundary(0) || IsBoundary(1) || IsBoundary(2);
        }

        /// <summary>
        /// Tests if an edge is on the boundary of a triangulation.
        /// </summary>
        /// <param name="index">The index of an edge</param>
        /// <returns><c>true</c> if the edge is on the boundary</returns>
        public bool IsBoundary(int index)
        {
            return !HasAdjacent(index);
        }

        /// <summary>
        /// Computes the vertex or edge index which is the next one
        /// (counter-clockwise) around the triangle.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The next index value</returns>
        public static int Next(int index)
        {
            switch (index)
            {
                case 0: return 1;
                case 1: return 2;
                case 2: return 0;
            }
            return -1;
        }

        /// <summary>
        /// Computes the vertex or edge index which is the previous one
        /// (counter-clockwise) around the triangle.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>The previous index value</returns>
        public static int Prev(int index)
        {
            switch (index)
            {
                case 0: return 2;
                case 1: return 0;
                case 2: return 1;
            }
            return -1;
        }

        /// <summary>
        /// Gets the index of the vertex opposite an edge.
        /// </summary>
        /// <param name="edgeIndex">The edge index</param>
        /// <returns>The index of the opposite vertex</returns>
        public static int OppVertex(int edgeIndex)
        {
            return Prev(edgeIndex);
        }

        /// <summary>
        /// Gets the index of the edge opposite a vertex.
        /// </summary>
        /// <param name="vertexIndex">The index of the vertex</param>
        /// <returns>The index of the opposite edge</returns>
        public static int OppEdge(int vertexIndex)
        {
            return Next(vertexIndex);
        }

        /// <summary>
        /// Computes a coordinate for the midpoint of a triangle edge.
        /// </summary>
        /// <param name="edgeIndex">The edge index</param>
        /// <returns>the midpoint of the triangle edge</returns>
        public Coordinate MidPoint(int edgeIndex)
        {
            var p0 = GetCoordinate(edgeIndex);
            var p1 = GetCoordinate(Next(edgeIndex));
            double midX = (p0.X + p1.X) / 2;
            double midY = (p0.Y + p1.Y) / 2;
            return new Coordinate(midX, midY);
        }

        /// <summary>Gets the area of the triangle.</summary>
        /// <returns>The area of the triangle</returns>
        public double Area
        { 
            get => Triangle.Area(_p0, _p1, _p2);
        }

        /// <summary>
        /// Gets the perimeter length of the perimeter of the triangle.
        /// </summary>
        public double Length
        {
            get => Triangle.Length(_p0, _p1, _p2);
        }

        /// <summary>
        /// Gets the length of an edge of the triangle.
        /// </summary>
        /// <param name="edgeIndex">The edge index</param>
        /// <returns>The edge length</returns>
        public double GetLength(int edgeIndex)
        {
            return GetCoordinate(edgeIndex).Distance(GetCoordinate(Next(edgeIndex)));
        }

        /// <summary>
        /// Creates a <see cref="Polygon"/> representing this triangle.
        /// </summary>
        /// <param name="geomFact">The geometry factory</param>
        /// <returns>A polygon</returns>
        public Geometries.Polygon ToPolygon(GeometryFactory geomFact)
        {
            return geomFact.CreatePolygon(
                geomFact.CreateLinearRing(new Coordinate[] { _p0.Copy(), _p1.Copy(), _p2.Copy(), _p0.Copy() }), null);
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return string.Format("POLYGON (({0}, {1}, {2}, {3}))",
                WKTWriter.Format(_p0), WKTWriter.Format(_p1), WKTWriter.Format(_p2),
                WKTWriter.Format(_p0));
        }

    }
}
