using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Triangulate.Polygon
{
    /// <summary>
    /// Improves the quality of a triangulation of <see cref="Tri.Tri"/>s via
    /// iterated Delaunay flipping.
    /// This produces a Constrained Delaunay Triangulation
    /// with the constraints being the boundary of the input triangulation.
    /// </summary>
    /// <author>Martin Davis</author>
    class TriDelaunayImprover
    {
        /// <summary>
        /// Improves the quality of a triangulation of {@link Tri}s via
        /// iterated Delaunay flipping.
        /// The Tris are assumed to be linked into a Triangulation
        /// (e.g. via <see cref="TriangulationBuilder"/>).
        /// </summary>
        /// <param name="triList">The list of <c>Tri</c>s to improve</param>
        public static void Improve(IList<Tri.Tri> triList)
        {
            var improver = new TriDelaunayImprover(triList);
            improver.Improve();
        }

        private const int MaxIteration = 200;
        private readonly IList<Tri.Tri> _triList;

        private TriDelaunayImprover(IList<Tri.Tri> triList)
        {
            _triList = triList;
        }

        private void Improve()
        {
            for (int i = 0; i < MaxIteration; i++)
            {
                int improveCount = ImproveScan(_triList);
                //System.out.println("improve #" + i + " - count = " + improveCount);
                if (improveCount == 0)
                {
                    return;
                }
            }
        }

        /// <summary>Improves a triangulation by examining pairs of adjacent triangles
        /// (forming a quadrilateral) and testing if flipping the diagonal of
        /// the quadrilateral would produce two new triangles with larger minimum
        /// interior angles.
        /// </summary>
        /// <returns>The number of flips that were made</returns>
        private int ImproveScan(IList<Tri.Tri> triList)
        {
            int improveCount = 0;
            for (int i = 0; i < triList.Count - 1; i++)
            {
                var tri = triList[i];
                for (int j = 0; j < 3; j++)
                {
                    //Tri neighb = tri.getAdjacent(j);
                    //tri.validateAdjacent(j);
                    if (ImproveNonDelaunay(tri, j))
                    {
                        // TODO: improve performance by only rescanning tris adjacent to flips?
                        improveCount++;
                    }
                }
            }
            return improveCount;
        }

        /// <summary>
        /// Does a flip of the common edge of two Tris if the Delaunay condition is not met.
        /// </summary>
        /// <param name="tri">A <c>Tri</c></param>
        /// <param name="index">The index of the <paramref name="tri"/></param>
        /// <returns><c>true</c> if the triangles were flipped</returns>
        private bool ImproveNonDelaunay(Tri.Tri tri, int index)
        {
            if (tri == null)
            {
                return false;
            }
            var tri1 = tri.GetAdjacent(index);
            if (tri1 == null)
            {
                return false;
            }
            //tri0.validate();
            //tri1.validate();


            int index1 = tri1.GetIndex(tri);

            var adj0 = tri.GetCoordinate(index);
            var adj1 = tri.GetCoordinate(Tri.Tri.Next(index));
            var opp0 = tri.GetCoordinate(Tri.Tri.OppVertex(index));
            var opp1 = tri1.GetCoordinate(Tri.Tri.OppVertex(index1));

            /*
             * The candidate new edge is opp0 - opp1. 
             * Check if it is inside the quadrilateral formed by the two triangles. 
             * This is the case if the quadrilateral is convex.
             */
            if (!IsConvex(adj0, adj1, opp0, opp1))
            {
                return false;
            }

            /*
             * The candidate edge is inside the quadrilateral. Check to see if the flipping
             * criteria is met. The flipping criteria is to flip if the two triangles are
             * not Delaunay (i.e. one of the opposite vertices is in the circumcircle of the
             * other triangle).
             */
            if (!IsDelaunay(adj0, adj1, opp0, opp1))
            {
                tri.Flip(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests if the quadrilateral formed by two adjacent triangles is convex.
        /// opp0-adj0-adj1 and opp1-adj1-adj0 are the triangle corners
        /// and hence are known to be convex.
        /// The quadrilateral is convex if the other corners opp0-adj0-opp1
        /// and opp1-adj1-opp0 have the same orientation (since at least one must be convex).
        /// </summary>
        /// <param name="adj0">The adjacent edge vertex 0</param>
        /// <param name="adj1">The adjacent edge vertex 1</param>
        /// <param name="opp0">The corner vertex of triangle 0</param>
        /// <param name="opp1">The corner vertex of triangle 1</param>
        /// <returns><c>true</c> if the quadrilateral is convex</returns>
        private static bool IsConvex(Coordinate adj0, Coordinate adj1, Coordinate opp0, Coordinate opp1)
        {
            var dir0 = Orientation.Index(opp0, adj0, opp1);
            var dir1 = Orientation.Index(opp1, adj1, opp0);
            bool isConvex = dir0 == dir1;
            return isConvex;
        }

        /// <summary>
        /// Tests if either of a pair of adjacent triangles satisfy the Delaunay condition.
        /// The triangles are opp0-adj0-adj1 and opp1-adj1-adj0.
        /// The Delaunay condition is not met if one opposite vertex
        /// lies is in the circumcircle of the other triangle.
        /// </summary>
        /// <param name="adj0">The adjacent edge vertex 0</param>
        /// <param name="adj1">The adjacent edge vertex 1</param>
        /// <param name="opp0">The corner vertex of triangle 0</param>
        /// <param name="opp1">The corner vertex of triangle 1</param>
        /// <returns><c>true</c> if the triangles are Delaunay</returns>
        private static bool IsDelaunay(Coordinate adj0, Coordinate adj1, Coordinate opp0, Coordinate opp1)
        {
            if (IsInCircle(adj0, adj1, opp0, opp1)) return false;
            if (IsInCircle(adj1, adj0, opp1, opp0)) return false;
            return true;
        }

        /// <summary>
        /// Tests whether a point p is in the circumcircle of a triangle abc
        /// (oriented clockwise).
        /// </summary>
        /// <param name="a">A vertex of the triangle</param>
        /// <param name="b">A vertex of the triangle</param>
        /// <param name="c">A vertex of the triangle</param>
        /// <param name="p">The point</param>
        /// <returns><c>true</c> if the point is in the circumcircle</returns>
        private static bool IsInCircle(Coordinate a, Coordinate b, Coordinate c, Coordinate p)
        {
            return TrianglePredicate.IsInCircleRobust(a, c, b, p);
        }

    }
}
