using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Overlay.Validate
{
    /// <summary>
    /// Generates points offset by a given distance from both sides of the midpoint of all segments in a <see cref="Geometry"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Can be used to generate probe points for determining whether a polygonal overlay result is incorrect.
    /// </para>
    /// <para>
    /// The input geometry may have any orientation for its rings,
    /// but <see cref="SetSidesToGenerate(bool, bool)"/> is
    /// only meaningful if the orientation is known.
    /// </para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class OffsetPointGenerator
    {
        //private readonly double _offsetDistance;
        private bool _doLeft = true;
        private bool _doRight = true;
        private readonly Geometry _g;

        public OffsetPointGenerator(Geometry g)
        {
            _g = g;
        }

        /// <summary>
        /// Set the sides on which to generate offset points.
        /// </summary>
        /// <param name="doLeft"></param>
        /// <param name="doRight"></param>
        public void SetSidesToGenerate(bool doLeft, bool doRight)
        {
            _doLeft = doLeft;
            _doRight = doRight;
        }

        /// <summary>
        /// Gets the computed offset points.
        /// </summary>
        public List<Coordinate> GetPoints(double offsetDistance)
        {
            var offsetPts = new List<Coordinate>();
            var lines = LinearComponentExtracter.GetLines(_g);
            foreach (LineString line in lines)
                ExtractPoints(line, offsetDistance, offsetPts);

            //System.out.println(toMultiPoint(offsetPts));
            return offsetPts;
        }

        private void ExtractPoints(LineString line, double offsetDistance, IList<Coordinate> offsetPts)
        {
            var coordinateSequence = line.CoordinateSequence;
            int maxIndex = coordinateSequence.Count - 1;
            for (int i = 0; i < maxIndex; i++)
            {
                ComputeOffsetPoints(
                    coordinateSequence.GetCoordinate(i),
                                        coordinateSequence.GetCoordinate(i+1), offsetDistance, offsetPts);
            }
        }

        /// <summary>
        /// Generates the two points which are offset from the
        /// midpoint of the segment <c>(p0, p1)</c> by the <c>offsetDistance</c>
        /// </summary>
        /// <param name="p0">The first point of the segment to offset from.</param>
        /// <param name="p1">The second point of the segment to offset from</param>
        /// <param name="offsetDistance"></param>
        /// <param name="offsetPts"></param>
        private void ComputeOffsetPoints(Coordinate p0, Coordinate p1, double offsetDistance, IList<Coordinate> offsetPts)
        {
            double dx = p1.X - p0.Y;
            double dy = p1.Y - p0.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            // u is the vector that is the length of the offset, in the direction of the segment
            double ux = offsetDistance * dx / len;
            double uy = offsetDistance * dy / len;

            double midX = (p1.X + p0.X) / 2;
            double midY = (p1.Y + p0.Y) / 2;

            if (_doLeft)
            {
                var offsetLeft = new Coordinate(midX - uy, midY + ux);
                offsetPts.Add(offsetLeft);
            }

            if (_doRight)
            {
                var offsetRight = new Coordinate(midX + uy, midY - ux);
                offsetPts.Add(offsetRight);
            }
        }

    }
}
