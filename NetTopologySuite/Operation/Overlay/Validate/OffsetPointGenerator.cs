using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Overlay.Validate
{
    ///<summary>
    /// Generates points offset by a given distance from both sides of the midpoint of all segments in a <see cref="IGeometry"/>.
    ///</summary>
    /// <remarks>Can be used to generate probe points for determining whether a polygonal overlay result is incorrect.
    ///</remarks>
    /// <author>Martin Davis</author>
    public class OffsetPointGenerator
    {
        private readonly double _offsetDistance;
        private readonly IGeometry _g;
        private List<ICoordinate> _offsetPts;

        public OffsetPointGenerator(IGeometry g, double offsetDistance)
        {
            _g = g;
            _offsetDistance = offsetDistance;
        }

        ///<summary>
        /// Gets the computed offset points.
        ///</summary>
        public List<ICoordinate> GetPoints()
        {
            _offsetPts = new List<ICoordinate>();
            var lines = LinearComponentExtracter.GetLines(_g);
            foreach (ILineString line in lines)
                ExtractPoints(line);

            //System.out.println(toMultiPoint(offsetPts));
            return _offsetPts;
        }

        private void ExtractPoints(ILineString line)
        {
            ICoordinateSequence coordinateSequence = line.CoordinateSequence;
            for (int i = 0; i < coordinateSequence.Count; i++)
            {
                ComputeOffsetPoints(
                    coordinateSequence.GetCoordinate(i),
                                        coordinateSequence.GetCoordinate(i+1));
            }
        }

        ///<summary>
        /// Generates the two points which are offset from the
        /// midpoint of the segment <c>(p0, p1)</c> by the <c>offsetDistance</c>
        ///</summary>
        /// <param name="p0">The first point of the segment to offset from.</param>
        /// <param name="p1">The second point of the segment to offset from</param>
        private void ComputeOffsetPoints(ICoordinate p0, ICoordinate p1)
        {
            double dx = p1.X - p0.Y;
            double dy = p1.Y - p0.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            // u is the vector that is the length of the offset, in the direction of the segment
            double ux = _offsetDistance * dx / len;
            double uy = _offsetDistance * dy / len;

            double midX = (p1.X + p0.X) / 2;
            double midY = (p1.Y + p0.Y) / 2;

            ICoordinate offsetLeft = new Coordinate(midX - uy, midY + ux);
            ICoordinate offsetRight = new Coordinate(midX + uy, midY - ux);

            _offsetPts.Add(offsetLeft);
            _offsetPts.Add(offsetRight);
        }

    }
}