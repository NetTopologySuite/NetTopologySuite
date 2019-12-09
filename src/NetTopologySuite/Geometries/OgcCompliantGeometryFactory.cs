using System;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// OGC compliant geometry factory
    /// </summary>
    [Obsolete("Use GeometryFactoryEx with OrientationOfExteriorRing = CCW")]
    public class OgcCompliantGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Creates an instance of this class using the default
        /// values for <see cref="GeometryFactory.SRID"/>,
        /// <see cref="GeometryFactory.PrecisionModel"/> and
        /// <see cref="GeometryFactory.CoordinateSequenceFactory"/>.
        /// </summary>
        public OgcCompliantGeometryFactory()
        {}

        /// <summary>
        /// Creates an instance of this class using the default
        /// values for <see cref="GeometryFactory.SRID"/>,
        /// <see cref="GeometryFactory.PrecisionModel"/>,
        /// but the specified <paramref name="factory"/>.
        /// </summary>
        public OgcCompliantGeometryFactory(CoordinateSequenceFactory factory)
            : base(factory)
        { }

        /// Creates an instance of this class using the default
        /// values for <see cref="GeometryFactory.SRID"/>,
        /// <see cref="GeometryFactory.CoordinateSequenceFactory"/> but the
        /// specified <paramref name="pm"/>.
        public OgcCompliantGeometryFactory(PrecisionModel pm)
            :base(pm)
        {}

        public OgcCompliantGeometryFactory(PrecisionModel pm, int srid)
            : base(pm, srid)
        { }

        public OgcCompliantGeometryFactory(PrecisionModel pm, int srid, CoordinateSequenceFactory factory)
            : base(pm, srid, factory)
        { }

        #region Private utility functions
        private static LinearRing ReverseRing(LinearRing ring)
        {
            return (LinearRing)ring.Reverse();
        }

        private LinearRing CreateLinearRing(Coordinate[] coordinates, bool ccw)
        {
            if (coordinates != null && Orientation.IsCCW(coordinates) != ccw)
                Array.Reverse(coordinates);
            return CreateLinearRing(coordinates);
        }

        private LinearRing CreateLinearRing(CoordinateSequence coordinates, bool ccw)
        {
            if (coordinates != null && Orientation.IsCCW(coordinates) != ccw)
            {
                coordinates = coordinates.Copy();
                CoordinateSequences.Reverse(coordinates);
            }
            return CreateLinearRing(coordinates);
        }
        #endregion

        /// <inheritdoc/>
        public override Geometry ToGeometry(Envelope envelope)
        {
            // null envelope - return empty point geometry
            if (envelope.IsNull)
                return CreatePoint((CoordinateSequence)null);

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
        /// The <see cref="Polygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override Polygon CreatePolygon(Coordinate[] coordinates)
        {
            var ring = CreateLinearRing(coordinates, true);
            return base.CreatePolygon(ring);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="Polygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override Polygon CreatePolygon(CoordinateSequence coordinates)
        {
            var ring = CreateLinearRing(coordinates, true);
            return base.CreatePolygon(ring);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="Polygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// </remarks>
        public override Polygon CreatePolygon(LinearRing shell)
        {
            return CreatePolygon(shell, null);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The <see cref="Polygon.ExteriorRing"/> is guaranteed to be orientated counter-clockwise.
        /// <br/>The <see cref="Polygon.InteriorRings"/> are guaranteed to be orientated clockwise.
        /// </remarks>
        public override Polygon CreatePolygon(LinearRing shell, LinearRing[] holes)
        {
            if (shell != null)
            {
                if (!shell.IsCCW)
                    shell = ReverseRing(shell);
            }

            if (holes != null)
            {
                for (int i = 0; i < holes.Length; i++)
                {
                    if (holes[i].IsCCW)
                        holes[i] = ReverseRing(holes[i]);
                }
            }

            return base.CreatePolygon(shell, holes);
        }
    }
}
