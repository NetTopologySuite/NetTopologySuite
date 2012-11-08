using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// OGC compliant geometry factory
    /// </summary>
    public class OgcCompliantGeometryFactory : GeometryFactory
    {
        #region Private utility functions
        private static ILinearRing ReverseRing(ILinearRing ring)
        {
            return (ILinearRing)ring.Reverse();
        }

        private ILinearRing CreateLinearRing(Coordinate[] coordinates, bool ccw)
        {
            if (coordinates != null && Algorithm.CGAlgorithms.IsCCW(coordinates) != ccw) 
                Array.Reverse(coordinates);
            return CreateLinearRing(coordinates);
        }

        private ILinearRing CreateLinearRing(ICoordinateSequence coordinates, bool ccw)
        {
            if (coordinates != null && Algorithm.CGAlgorithms.IsCCW(coordinates) != ccw)
            {
                //CoordinateSequences.Reverse(coordinates);
                coordinates = coordinates.Reversed();
            }
            return CreateLinearRing(coordinates);
        }
        #endregion

        /// <inheritdoc/>
        public override IGeometry ToGeometry(Envelope envelope)
        {
            // null envelope - return empty point geometry
            if (envelope.IsNull)
                return CreatePoint((ICoordinateSequence)null);

            // point?
            if (envelope.MinX == envelope.MaxX && envelope.MinY == envelope.MaxY)
                return CreatePoint(new Coordinate(envelope.MinX, envelope.MinY));

            // vertical or horizontal line?
            if (envelope.MinX == envelope.MaxX
                    || envelope.MinY == envelope.MaxY)
            {
                return CreateLineString(new[] 
                    {
                        new Coordinate(envelope.MinX, envelope.MinY),
                        new Coordinate(envelope.MaxX, envelope.MaxY)
                    });
            }

            // return CCW polygon
            var ring = CreateLinearRing(new[]
            {
                new Coordinate(envelope.MinX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MinY),
                new Coordinate(envelope.MaxX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MaxY),
                new Coordinate(envelope.MinX, envelope.MinY)
            });

            //this is ccw so no need to check that again
            return base.CreatePolygon(ring, null);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="IPolygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override IPolygon CreatePolygon(Coordinate[] coordinates)
        {
            var ring = CreateLinearRing(coordinates, true);
            return base.CreatePolygon(ring);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="IPolygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override IPolygon CreatePolygon(ICoordinateSequence coordinates)
        {
            var ring = CreateLinearRing(coordinates, true);
            return base.CreatePolygon(ring);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="IPolygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override IPolygon CreatePolygon(ILinearRing shell)
        {
            return CreatePolygon(shell, null);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="IPolygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// <br/>The <see cref="IPolygon.InteriorRings"/> are guaranteed to be orientated clockwise.
        /// </remarks>
        public override IPolygon CreatePolygon(ILinearRing shell, ILinearRing[] holes)
        {
            if (shell != null)
            {
                if (!shell.IsCCW)
                    shell = ReverseRing(shell);
            }

            if (holes != null)
            {
                for (var i = 0; i < holes.Length; i++)
                {
                    if (holes[i].IsCCW)
                        holes[i] = ReverseRing(holes[i]);
                }
            }

            return base.CreatePolygon(shell, holes);
        }
    }
}