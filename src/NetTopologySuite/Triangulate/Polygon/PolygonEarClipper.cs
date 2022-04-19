using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index;

using System;
using System.Collections.Generic;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Triangulates a polygon using the Ear-Clipping technique.
    /// The polygon is provided as a closed list of contiguous vertices
    /// defining its boundary.
    /// The vertices must have clockwise orientation.
    /// <para/>
    /// The polygon boundary must not self-cross,
    /// but may self-touch at points or along an edge.
    /// It may contain repeated points, which are treated as a single vertex.
    /// By default every vertex is triangulated,
    /// including ones which are "flat" (the adjacent segments are collinear).
    /// These can be removed by setting <see cref="SkipFlatCorners"/>.
    /// <para/>
    /// The polygon representation does not allow holes.
    /// Polygons with holes can be triangulated by preparing them with <see cref="PolygonHoleJoiner"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    class PolygonEarClipper
    {

        private const int NoVertexIndex = -1;

        /// <summary>
        /// Triangulates a polygon via ear-clipping.
        /// </summary>
        /// <param name="polyShell">The vertices of the polygon</param>
        /// <returns>A list of <c>Tri</c>s</returns>
        public static IList<Tri.Tri> Triangulate(Coordinate[] polyShell)
        {
            var clipper = new PolygonEarClipper(polyShell);
            return clipper.Compute();
        }

        private bool _isFlatCornersSkipped = false;

        /// <summary>
        /// The polygon vertices are provided in CW orientation.
        /// Thus for convex interior angles
        /// the vertices forming the angle are in CW orientation.
        /// </summary>
        private readonly Coordinate[] _vertex;
  
        private readonly int[] _vertexNext;
        private int _vertexSize;
        // first available vertex index
        private int _vertexFirst;

        // indices for current corner
        private int[] _cornerIndex;

        /// <summary>
        /// Indexing vertices improves ear intersection testing performance.
        /// The polyShell vertices are contiguous, so are suitable for an SPRtree.
        /// Note that a KDtree cannot be used because the vertex indices must be stored
        /// and duplicates must be stored.
        /// </summary>
        private readonly VertexSequencePackedRtree _vertexCoordIndex;

        /// <summary>
        /// Creates a new instance of this class
        /// </summary>
        /// <param name="polyShell">The vertices of the polygon to process</param>
        public PolygonEarClipper(Coordinate[] polyShell)
        {
            _vertex = polyShell;

            // init working storage
            _vertexSize = _vertex.Length - 1;
            _vertexNext = CreateNextLinks(_vertexSize);
            _vertexFirst = 0;

            _vertexCoordIndex = new VertexSequencePackedRtree(_vertex);
        }

        private static int[] CreateNextLinks(int size)
        {
            int[] next = new int[size];
            for (int i = 0; i < size; i++)
            {
                next[i] = i + 1;
            }
            next[size - 1] = 0;
            return next;
        }

        /// <summary>
        /// Gets or sets whether flat corners formed by collinear adjacent line segments
        /// are included in the triangulation.
        /// Skipping flat corners reduces the number of triangles in the output.
        /// However, it produces a triangulation which does not include
        /// all input vertices.  This may be undesirable for downstream processes
        /// (such as computing a Constrained Delaunay Triangulation for
        /// purposes of computing the medial axis).
        /// <para/>
        /// The default is to include all vertices in the result triangulation.
        /// This still produces a valid triangulation, with no zero-area triangles.
        /// <para/>
        /// Note that repeated vertices are always skipped.
        /// </summary>
        /// <returns>A flag indicating if flat corners formed by collinear adjacent line segments
        /// are included in the triangulation</returns>
        public bool SkipFlatCorners { get => _isFlatCornersSkipped; set => _isFlatCornersSkipped = value;}

        public IList<Tri.Tri> Compute()
        {
            var triList = new List<Tri.Tri>();

            /*
             * Count scanned corners, to catch infinite loops
             * (which indicate an algorithm bug)
             */
            int cornerScanCount = 0;

            InitCornerIndex();
            var corner = new Coordinate[3];
            FetchCorner(corner);

            /*
             * Scan continuously around vertex ring, 
             * until all ears have been found.
             */
            while (true)
            {
                /*
                 * Non-convex corner- remove if flat, or skip
                 * (a concave corner will turn into a convex corner
                 * after enough ears are removed)
                 */
                if (!IsConvex(corner))
                {
                    // remove the corner if it is invalid flat (if required)
                    bool isCornerRemoved = IsCornerInvalid(corner)
                        || (SkipFlatCorners && IsFlat(corner));
                    if (isCornerRemoved)
                    {
                        //System.Diagnostics.Debug.WriteLine(WKTWriter.ToLineString(corner));
                        RemoveCorner();
                    }
                    cornerScanCount++;
                    if (cornerScanCount > 2 * _vertexSize)
                    {
                        //System.Diagnostics.Debug.WriteLine(ToGeometry());
                        //System.Diagnostics.Debug.WriteLine(WKTWriter.ToLineString(corner));
                        throw new InvalidOperationException("Unable to find a convex corner");
                    }
                }
                /*
                 * Convex corner - check if it is a valid ear
                 */
                else if (IsValidEar(_cornerIndex[1], corner))
                {
                    triList.Add(Tri.Tri.Create(corner));
                    RemoveCorner();
                    cornerScanCount = 0;
                }
                if (cornerScanCount > 2 * _vertexSize)
                {
                    //System.out.println(toGeometry());
                    throw new InvalidOperationException("Unable to find a valid ear");
                }

                //--- done when all corners are processed and removed
                if (_vertexSize < 3)
                {
                    return triList;
                }

                /*
                 * Skip to next corner.
                 * This is done even after an ear is removed, 
                 * since that creates fewer skinny triangles.
                 */
                NextCorner(corner);
            }
        }

        private bool IsValidEar(int cornerIndex, Coordinate[] corner)
        {
            int intApexIndex = FindIntersectingVertex(cornerIndex, corner);
            //--- no intersections found
            if (intApexIndex == NoVertexIndex)
                return true;
            //--- check for duplicate corner apex vertex
            if (_vertex[intApexIndex].Equals2D(corner[1]))
            {
                //--- a duplicate corner vertex requires a full scan
                return IsValidEarScan(cornerIndex, corner);
            }
            //-- vertex is contained in corner, so it is not a valid ear
            return false;
        }

        /// <summary>
        /// Finds a vertex contained in the corner triangle, if any.
        /// Uses the vertex spatial index for efficiency.
        /// <para/>
        /// Also finds any vertex which is a duplicate of the corner apex vertex.
        /// This requires a full scan of the vertices to confirm ear is valid.
        /// This is usually a rare situation, so has little impact on performance.
        /// </summary>
        /// <param name="cornerIndex">The index of the corner apex vertex</param>
        /// <param name="corner">The corner vertices</param>
        /// <returns>The index of an intersecting or duplicate vertex, or <see cref="NoVertexIndex"/> if none</returns>
        private int FindIntersectingVertex(int cornerIndex, Coordinate[] corner)
        {
            var cornerEnv = Envelope(corner);
            int[] result = _vertexCoordIndex.Query(cornerEnv);

            int dupApexIndex = NoVertexIndex;
            //--- check for duplicate vertices
            for (int i = 0; i < result.Length; i++)
            {
                int vertIndex = result[i];

                if (vertIndex == cornerIndex
                    || vertIndex == _vertex.Length - 1
                    || IsRemoved(vertIndex))
                    continue;

                var v = _vertex[vertIndex];
                /*
                 * If the vertex is equal to the corner apex, record it.
                 * This can happen where the polygon ring self-touches,
                 * usually due to hole joining.
                 * This will require a full scan to check the incident segments.
                 * So only report this if no properly intersecting vertex is found,
                 * for efficiency.
                 */
                if (v.Equals2D(corner[1]))
                {
                    dupApexIndex = vertIndex;
                }
                //--- don't need to check other corner vertices 
                else if (v.Equals2D(corner[0]) || v.Equals2D(corner[2]))
                {
                    continue;
                }
                //--- this is a properly intersecting vertex
                else if (Triangle.Intersects(corner[0], corner[1], corner[2], v))
                    return vertIndex;
            }
            if (dupApexIndex != NoVertexIndex)
            {
                return dupApexIndex;
            }
            return NoVertexIndex;
        }

        /// <summary>
        /// Scan all vertices in current ring to check if any are duplicates
        /// of the corner apex vertex, and if so whether the corner ear
        /// intersects the adjacent segments and thus is invalid.
        /// </summary>
        /// <param name="cornerIndex">The index of the corner apex</param>
        /// <param name="corner">The corner vertices</param>
        /// <returns><c>true</c> if the corner ia a valid ear</returns>
        private bool IsValidEarScan(int cornerIndex, Coordinate[] corner)
        {
            double cornerAngle = AngleUtility.AngleBetweenOriented(corner[0], corner[1], corner[2]);

            int currIndex = NextIndex(_vertexFirst);
            int prevIndex = _vertexFirst;
            var vPrev = _vertex[prevIndex];
            for (int i = 0; i < _vertexSize; i++)
            {
                var v = _vertex[currIndex];
                /*
                 * Because of hole-joining vertices can occur more than once.
                 * If vertex is same as corner[1],
                 * check whether either adjacent edge lies inside the ear corner.
                 * If so the ear is invalid.
                 */
                if (currIndex != cornerIndex
                    && v.Equals2D(corner[1]))
                {
                    var vNext = _vertex[NextIndex(currIndex)];

                    //TODO: for robustness use segment orientation instead
                    double aOut = AngleUtility.AngleBetweenOriented(corner[0], corner[1], vNext);
                    double aIn = AngleUtility.AngleBetweenOriented(corner[0], corner[1], vPrev);
                    if (aOut > 0 && aOut < cornerAngle)
                    {
                        return false;
                    }
                    if (aIn > 0 && aIn < cornerAngle)
                    {
                        return false;
                    }
                    if (aOut == 0 && aIn == cornerAngle)
                    {
                        return false;
                    }
                }

                //--- move to next vertex
                vPrev = v;
                prevIndex = currIndex;
                currIndex = NextIndex(currIndex);
            }
            return true;
        }

        private static Envelope Envelope(Coordinate[] corner)
        {
            var cornerEnv = new Envelope(corner[0], corner[1]);
            cornerEnv.ExpandToInclude(corner[2]);
            return cornerEnv;
        }

        /// <summary>
        /// Remove the corner apex vertex and update the candidate corner location.
        /// </summary>
        private void RemoveCorner()
        {
            int cornerApexIndex = _cornerIndex[1];
            if (_vertexFirst == cornerApexIndex)
            {
                _vertexFirst = _vertexNext[cornerApexIndex];
            }
            _vertexNext[_cornerIndex[0]] = _vertexNext[cornerApexIndex];
            _vertexCoordIndex.RemoveAt(cornerApexIndex);
            _vertexNext[cornerApexIndex] = NoVertexIndex;
            _vertexSize--;
            //-- adjust following corner indexes
            _cornerIndex[1] = NextIndex(_cornerIndex[0]);
            _cornerIndex[2] = NextIndex(_cornerIndex[1]);
        }

        private bool IsRemoved(int vertexIndex)
        {
            return NoVertexIndex == _vertexNext[vertexIndex];
        }

        private void InitCornerIndex()
        {
            _cornerIndex = new int[3];
            _cornerIndex[0] = 0;
            _cornerIndex[1] = 1;
            _cornerIndex[2] = 2;
        }

        /// <summary>
        /// Fetch the corner vertices from the indices.
        /// </summary>
        /// <param name="cornerVertex">An array for the corner vertices</param>
        private void FetchCorner(Coordinate[] cornerVertex)
        {
            cornerVertex[0] = _vertex[_cornerIndex[0]];
            cornerVertex[1] = _vertex[_cornerIndex[1]];
            cornerVertex[2] = _vertex[_cornerIndex[2]];
        }

        /// <summary>
        /// Move to next corner.
        /// </summary>
        private void NextCorner(Coordinate[] cornerVertex)
        {
            if (_vertexSize < 3)
            {
                return;
            }
            _cornerIndex[0] = NextIndex(_cornerIndex[0]);
            _cornerIndex[1] = NextIndex(_cornerIndex[0]);
            _cornerIndex[2] = NextIndex(_cornerIndex[1]);
            FetchCorner(cornerVertex);
        }

        /// <summary>
        /// Get the index of the next available shell coordinate starting from the given index.
        /// </summary>
        /// <param name="index">Coordinate position</param>
        /// <returns>Index of the next available shell coordinate</returns>
        private int NextIndex(int index)
        {
            return _vertexNext[index];
        }

        private static bool IsConvex(Coordinate[] pts)
        {
            return OrientationIndex.Clockwise == Orientation.Index(pts[0], pts[1], pts[2]);
        }

        private static bool IsFlat(Coordinate[] pts)
        {
            return OrientationIndex.Collinear == Orientation.Index(pts[0], pts[1], pts[2]);
        }
        /// <summary>
        /// Detects if a corner has repeated points (AAB or ABB), or is collapsed (ABA).
        /// </summary>
        /// <param name="pts">The corner points</param>
        /// <returns><c>true</c> if the corner is flat or collapsed</returns>
        private static bool IsCornerInvalid(Coordinate[] pts)
        {
            return pts[1].Equals2D(pts[0]) || pts[1].Equals2D(pts[2]) || pts[0].Equals2D(pts[2]); ;
        }

        public Geometries.Polygon ToGeometry()
        {
            var fact = new GeometryFactory();
            var coordList = new CoordinateList();
            int index = _vertexFirst;
            for (int i = 0; i < _vertexSize; i++)
            {
                var v = _vertex[index];
                index = NextIndex(index);
                // if (i < shellCoordAvailable.length && shellCoordAvailable.get(i))
                coordList.Add(v, true);
            }
            coordList.CloseRing();
            return fact.CreatePolygon(fact.CreateLinearRing(coordList.ToCoordinateArray()));
        }
    }
}
