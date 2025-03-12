using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.OverlayArea
{
    /// <summary>
    /// Functions to compute the partial area term for an edge vector
    /// starting at an intersection vertex or a contained vertex.
    /// <para/>
    /// An edge vector implicitly defined two derived vectors:
    /// <list type="bullet">
    /// <item><description>A <b>unit tangent vector</b> originating at the start point and parallel to the edge vector</description></item>
    /// <item><description>A <b>unit normal vector</b> originating at the start point and perpendicular to the edge,
    /// pointing into the polygon</description></item>
    /// </list>
    /// Note that an edge vector has no notion of its length.
    /// The terminating coordinate is only provided to establish the direction of the vector.
    /// </summary>
    /// <author>Martin Davix</author>
    internal static class EdgeVector
    {
        /// <summary>
        /// Computes the partial area term for an edge between two points.
        /// </summary>
        /// <param name="p0">The edge start point</param>
        /// <param name="p1">The edge end point</param>
        /// <param name="isInteriorToRight">Flag indicating whether the polygon interior lies to the right of the vector</param>
        /// <returns>The area term</returns>
        public static double Area2Term(Coordinate p0, Coordinate p1, bool isInteriorToRight)
        {
            return Area2Term(p0.X, p0.Y, p0.X, p0.Y, p1.X, p1.Y, isInteriorToRight);
        }

        /// <summary>
        /// Computes the partial area term for an edge between two points.
        /// </summary>
        /// <param name="x0">The start x-ordinate</param>
        /// <param name="y0">The start y-ordinate</param>
        /// <param name="x1">The end x-ordinate</param>
        /// <param name="y1">The end y-ordinate</param>
        /// <param name="isInteriorToRight"></param>
        /// <returns>The area term</returns>
        public static double Area2Term(
            double x0, double y0, double x1, double y1, bool isInteriorToRight)
        {
            return Area2Term(x0, y0, x0, y0, x1, y1, isInteriorToRight);
        }

        /// <summary>
        /// Computes the partial area (doubled) for an edge vector
        /// starting at a given vertex and with a given direction vector and orientation
        /// relative to the parent polygon.
        /// The partial area terms can be summed to determine the total
        /// area of a geometry or an overlay.
        /// <para/>
        /// The edge vector has origin v, and direction vector p0->p1.
        /// The area term sign depends on whether the polygon interior lies to the right or left
        /// of the vector.
        /// </summary>
        /// <param name="v">The edge origin</param>
        /// <param name="d0">The direction vector origin</param>
        /// <param name="d1">The direction vector terminus</param>
        /// <param name="isInteriorToRight">Flag indicating whether the polygon interior lies to the right of the vector</param>
        /// <returns>The area term</returns>
        public static double Area2Term(Coordinate v, Coordinate d0, Coordinate d1, bool isInteriorToRight)
        {
            return Area2Term(v.X, v.Y, d0.X, d0.Y, d1.X, d1.Y, isInteriorToRight);
        }

        /// <summary>
        /// Computes the partial area (doubled) for an edge vector
        /// starting at a given vertex and with a given direction vector and orientation
        /// relative to the parent polygon.
        /// The partial area terms can be summed to determine the total
        /// area of a geometry or an overlay.
        /// <para/>
        /// The edge vector has origin (vx, vy), and direction vector (x0,y0)->(x1,y1).
        /// The area term sign depends on whether the polygon interior lies to the right or left
        /// of the vector.
        /// <para/>
        /// The value returned is twice the actual area term, to reduce arithmetic operations
        /// over many evaluations.
        /// </summary>
        /// <param name="vx">The x-ordinate of the edge origin</param>
        /// <param name="vy">The y-ordinate of the edge origin</param>
        /// <param name="x0">The x-ordinate of the direction vector origin</param>
        /// <param name="y0">The y-ordinate of the direction vector origin</param>
        /// <param name="x1">The x-ordinate of the direction vector terminus</param>
        /// <param name="y1">The x-ordinate of the direction vector terminus</param>
        /// <param name="isInteriorToRight">Flag indicating whether the polygon interior lies to the right of the vector</param>
        /// <returns>The area term</returns>
        public static double Area2Term(
            double vx, double vy, double x0, double y0, double x1, double y1, bool isInteriorToRight)
        {

            double dx = x1 - x0;
            double dy = y1 - y0;
            double len2 = dx * dx + dy * dy;
            if (len2 <= 0) return 0;

            // unit vector in direction of edge
            double len = Math.Sqrt(len2);
            double ux = dx / len;
            double uy = dy / len;

            // normal vector to edge, pointing into polygon
            double nx, ny;
            if (isInteriorToRight)
            {
                nx = uy;
                ny = -ux;
            }
            else
            {
                nx = -uy;
                ny = ux;
            }

            double area2Term = (vx * ux + vy * uy) * (vx * nx + vy * ny);
            //System.out.println(areaTerm);
            return area2Term;
        }


    }
}
