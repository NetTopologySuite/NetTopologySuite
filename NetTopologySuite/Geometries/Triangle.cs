using System;

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a planar triangle, and provides methods for calculating various
    /// properties of triangles.
    /// </summary>
    public class Triangle
    {
        private ICoordinate p0, p1, p2;

        public ICoordinate P0
        {
            get { return p0; }
            set { p0 = value; }
        }

        public ICoordinate P1
        {
            get { return p1; }
            set { p1 = value; }
        }

        public ICoordinate P2
        {
            get { return p2; }
            set { p2 = value; }
        }

        public Triangle(ICoordinate p0, ICoordinate p1, ICoordinate p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// The inCentre of a triangle is the point which is equidistant
        /// from the sides of the triangle.  
        /// This is also the point at which the bisectors
        /// of the angles meet.
        /// </summary>
        /// <returns>
        /// The point which is the InCentre of the triangle.
        /// </returns>
        public ICoordinate InCentre
        {
            get
            {
                // the lengths of the sides, labelled by their opposite vertex
                Double len0 = P1.Distance(P2);
                Double len1 = P0.Distance(P2);
                Double len2 = P0.Distance(P1);
                Double circum = len0 + len1 + len2;

                Double inCentreX = (len0*P0.X + len1*P1.X + len2*P2.X)/circum;
                Double inCentreY = (len0*P0.Y + len1*P1.Y + len2*P2.Y)/circum;
                return new Coordinate(inCentreX, inCentreY);
            }
        }
    }
}