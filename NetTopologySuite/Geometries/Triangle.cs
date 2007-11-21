using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a planar triangle, and provides methods for calculating various
    /// properties of triangles.
    /// </summary>
    public class Triangle<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private TCoordinate p0, p1, p2;

        public TCoordinate P0
        {
            get { return p0; }
            set { p0 = value; }
        }

        public TCoordinate P1
        {
            get { return p1; }
            set { p1 = value; }
        }

        public TCoordinate P2
        {
            get { return p2; }
            set { p2 = value; }
        }

        public Triangle(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// The InCenter of a triangle is the point which is equidistant
        /// from the sides of the triangle.  
        /// This is also the point at which the bisectors
        /// of the angles meet.
        /// </summary>
        /// <returns>
        /// The point which is the InCenter of the triangle.
        /// </returns>
        public TCoordinate InCenter
        {
            get
            {
                // the lengths of the sides, labeled by their opposite vertex
                Double len0 = P1.Distance(P2);
                Double len1 = P0.Distance(P2);
                Double len2 = P0.Distance(P1);
                Double circum = len0 + len1 + len2;

                Double inCenterX = (len0 * P0[Ordinates.X] + len1 * P1[Ordinates.X] + len2 * P2[Ordinates.X]) / circum;
                Double inCenterY = (len0 * P0[Ordinates.Y] + len1 * P1[Ordinates.Y] + len2 * P2[Ordinates.Y]) / circum;
                return new TCoordinate(inCenterX, inCenterY);
            }
        }
    }
}