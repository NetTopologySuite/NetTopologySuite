using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Triangulate.Polygon
{
    /**
     * Improves the quality of a triangulation of {@link Tri}s via
     * iterated Delaunay flipping.
     * This produces a Constrained Delaunay Triangulation
     * with the constraints being the boundary of the input triangulation.
     * 
     * @author mdavis
     */
    class TriDelaunayImprover
    {

        /**
         * Improves the quality of a triangulation of {@link Tri}s via
         * iterated Delaunay flipping.
         * The Tris are assumed to be linked into a Triangulation
         * (e.g. via {@link TriangulationBuilder}).
         * 
         * @param triList the list of Tris to flip.
         */
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

        /**
         * Improves a triangulation by examining pairs of adjacent triangles
         * (forming a quadrilateral) and testing if flipping the diagonal of
         * the quadrilateral would produce two new triangles with larger minimum
         * interior angles.
         * 
         * @return the number of flips that were made
         */
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

        /**
         * Does a flip of the common edge of two Tris if the Delaunay condition is not met.
         * 
         * @param tri0 a Tri
         * @param tri1 a Tri
         * @return true if the triangles were flipped
         */
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

        /**
         * Tests if the quadrilateral formed by two adjacent triangles is convex.
         * opp0-adj0-adj1 and opp1-adj1-adj0 are the triangle corners 
         * and hence are known to be convex.
         * The quadrilateral is convex if the other corners opp0-adj0-opp1
         * and opp1-adj1-opp0 have the same orientation (since at least one must be convex).
         * 
         * @param adj0 adjacent edge vertex 0
         * @param adj1 adjacent edge vertex 1
         * @param opp0 corner vertex of triangle 0
         * @param opp1 corner vertex of triangle 1
         * @return true if the quadrilateral is convex
         */
        private static bool IsConvex(Coordinate adj0, Coordinate adj1, Coordinate opp0, Coordinate opp1)
        {
            var dir0 = Orientation.Index(opp0, adj0, opp1);
            var dir1 = Orientation.Index(opp1, adj1, opp0);
            bool isConvex = dir0 == dir1;
            return isConvex;
        }

        /**
         * Tests if either of a pair of adjacent triangles satisfy the Delaunay condition.
         * The triangles are opp0-adj0-adj1 and opp1-adj1-adj0.
         * The Delaunay condition is not met if one opposite vertex 
         * lies is in the circumcircle of the other triangle.
         * 
         * @param adj0 adjacent edge vertex 0
         * @param adj1 adjacent edge vertex 1
         * @param opp0 corner vertex of triangle 0
         * @param opp1 corner vertex of triangle 1
         * @return true if the triangles are Delaunay
         */
        private static bool IsDelaunay(Coordinate adj0, Coordinate adj1, Coordinate opp0, Coordinate opp1)
        {
            if (IsInCircle(adj0, adj1, opp0, opp1)) return false;
            if (IsInCircle(adj1, adj0, opp1, opp0)) return false;
            return true;
        }

        /**
         * Tests whether a point p is in the circumcircle of a triangle abc
         * (oriented clockwise).
         * @param a a vertex of the triangle
         * @param b a vertex of the triangle
         * @param c a vertex of the triangle
         * @param p the point
         * 
         * @return true if the point is in the circumcircle
         */
        private static bool IsInCircle(Coordinate a, Coordinate b, Coordinate c, Coordinate p)
        {
            return TrianglePredicate.IsInCircleRobust(a, c, b, p);
        }

    }
}
