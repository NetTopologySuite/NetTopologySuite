using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the centroid of a linear point.
    /// Algorithm:
    /// Compute the average of the midpoints
    /// of all line segments weighted by the segment length.
    /// </summary>
    [Obsolete("Use Centroid instead")]
    public class CentroidLine
    {
        private readonly Coordinate _centSum = new Coordinate();
        private double _totalLength;

        /// <summary>
        /// Adds the linear components of by a Geometry to the centroid total.
        /// If the geometry has no linear components it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        public void Add(IGeometry geom)
        {
            if (geom is ILineString)
                Add(geom.Coordinates);

            else if (geom is IPolygon)
            {
                var poly = (IPolygon) geom;
                // add linear components of a polygon
                Add(poly.ExteriorRing.Coordinates);
                for (int i = 0; i < poly.NumInteriorRings; i++)
                {
                    Add(poly.GetInteriorRingN(i).Coordinates);
                }
            }

            else if (geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection)geom;
                foreach (var geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Centroid
        {
            get
            {
                var cent = new Coordinate();
                cent.X = _centSum.X / _totalLength;
                cent.Y = _centSum.Y / _totalLength;
                return cent;
            }
        }

        /// <summary>
        /// Adds the length defined by an array of coordinates.
        /// </summary>
        /// <param name="pts">An array of <c>Coordinate</c>s.</param>
        public void Add(Coordinate[] pts)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                double segmentLen = pts[i].Distance(pts[i + 1]);
                _totalLength += segmentLen;

                double midx = (pts[i].X + pts[i + 1].X) / 2;
                _centSum.X += segmentLen * midx;
                double midy = (pts[i].Y + pts[i + 1].Y) / 2;
                _centSum.Y += segmentLen * midy;
            }
        }
    }
}
