using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes the centroid of a linear point.
    /// Algorithm:
    /// Compute the average of the midpoints
    /// of all line segments weighted by the segment length.
    /// </summary>
    public class CentroidLine<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                             IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly ICoordinateFactory<TCoordinate> _factory;
        private TCoordinate _centSum;
        private Double totalLength = 0.0;

        public CentroidLine(ICoordinateFactory<TCoordinate> factory)
        {
            _factory = factory;
        }

        /// <summary> 
        /// Adds the linestring(s) defined by a Geometry to the centroid total.
        /// If the geometry is not linear it does not contribute to the centroid.
        /// </summary>
        /// <param name="geom">The geometry to add.</param>
        public void Add(IGeometry<TCoordinate> geom)
        {
            if (geom is ILineString<TCoordinate>)
            {
                Add(geom.Coordinates);
            }

            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                Debug.Assert(gc != null);

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    Add(geometry);
                }
            }
        }

        public TCoordinate Centroid
        {
            get
            {
                Double x = _centSum[Ordinates.X] / totalLength;
                Double y = _centSum[Ordinates.Y] / totalLength;
                return _factory.Create(x, y);
            }
        }

        /// <summary> 
        /// Adds the length defined by a set of coordinates.
        /// </summary>
        /// <param name="points">A set of <typeparamref name="TCoordinate"/>s.</param>
        public void Add(IEnumerable<TCoordinate> points)
        {
            Double x = _centSum[Ordinates.X];
            Double y = _centSum[Ordinates.Y];

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(points))
            {
                TCoordinate point1 = pair.First;
                TCoordinate point2 = pair.Second;

                Double segmentLen = point1.Distance(point2);
                totalLength += segmentLen;

                Double midx = (point1[Ordinates.X] + point2[Ordinates.X]) / 2;
                x += segmentLen * midx;

                Double midy = (point1[Ordinates.Y] + point2[Ordinates.Y]) / 2;
                y += segmentLen * midy;
            }

            _centSum = _factory.Create(x, y);
        }
    }
}