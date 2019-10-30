using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An extended <see cref="GeometryFactory"/> that is capable of enforcing a ring orientation for polygons.
    /// </summary>
    public class GeometryFactoryEx : GeometryFactory
    {
        /// <summary>
        /// Gets the default polygon shell ring orientation that is used when nothing else has been set.
        /// </summary>
        private const LinearRingOrientation DefaultRingOrientation = LinearRingOrientation.Default;

        /// <summary>
        /// The polygon shell ring orientation enforced by this factory
        /// </summary>
        private LinearRingOrientation? _polygonShellRingOrientation;

        /// <summary>
        /// Gets or sets a value indicating the ring orientation of the
        /// <c>Polygon</c>'s exterior rings.<para>
        /// If its value is <see cref="LinearRingOrientation.DontCare"/>, this
        /// factory behaves just like the base <see cref="GeometryFactory"/>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The setter of this property has to be used prior to any call
        /// to <c>CreatePolygon</c> or <c>"CreateMultiPolygon</c></remarks>
        /// <seealso cref="GeometryFactory.CreatePolygon(Coordinate[])"/>
        /// <seealso cref="GeometryFactory.CreatePolygon(CoordinateSequence)"/>
        /// <seealso cref="GeometryFactory.CreatePolygon(LinearRing)"/>
        /// <seealso cref="GeometryFactory.CreatePolygon(LinearRing, LinearRing[])"/>
        /// <seealso cref="GeometryFactory.CreateMultiPolygon(Polygon[])"/>
        public LinearRingOrientation OrientationOfExteriorRing
        {
            get => _polygonShellRingOrientation ?? (_polygonShellRingOrientation = DefaultRingOrientation).Value;
            set
            {
                if (_polygonShellRingOrientation.HasValue && _polygonShellRingOrientation.Value != value)
                    throw new InvalidOperationException();
                if ((int)value < -1 || (int)value > 1)
                    throw new ArgumentOutOfRangeException();

                _polygonShellRingOrientation = value;
            }
        }

        /// <summary>
        /// Gets a value indicating the ring orientation for the interior rings
        /// </summary>
        /// <remarks>
        /// This value is always opposite of <see cref="OrientationOfExteriorRing"/>,
        /// except when its value is <see cref="LinearRingOrientation.DontCare"/>.
        /// </remarks>
        private LinearRingOrientation OrientationOfInteriorRings
        {
            get
            {
                switch (OrientationOfExteriorRing)
                {
                    case LinearRingOrientation.CCW:
                        return LinearRingOrientation.CW;
                    case LinearRingOrientation.CW:
                        return LinearRingOrientation.CCW;
                    default:
                        return LinearRingOrientation.DontCare;
                }
            }
        }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary and
        /// interior boundaries.
        /// <para/>
        /// The <see cref="OrientationOfExteriorRing"/> is enforced on the constructed polygon.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>, or
        /// <c>null</c> or an empty <c>LinearRing</c> if
        /// the empty point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>, or
        /// <c>null</c> or empty <c>LinearRing</c> s if
        /// the empty point is to be created.
        /// </param>
        /// <returns>A <see cref="Polygon"/> object</returns>
        public override Polygon CreatePolygon(LinearRing shell, LinearRing[] holes)
        {
            shell = EnsureOrientation(shell, OrientationOfExteriorRing);
            if (holes != null && holes.Length > 0)
            {
                var ringOrientation = OrientationOfInteriorRings;
                for (int i = 0; i < holes.Length; i++)
                    holes[i] = EnsureOrientation(holes[i], ringOrientation);
            }
            return new Polygon(shell, holes, this);
        }

        /// <summary>
        /// Creates a <c>MultiPolygon</c> using the given <c>Polygons</c>; a null or empty array
        /// will create an empty Polygon. The polygons must conform to the
        /// assertions specified in the <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.<para/>
        /// The <see cref="OrientationOfExteriorRing"/> is enforced on each polygon.
        /// </summary>
        /// <param name="polygons">Polygons, each of which may be empty but not null.</param>
        /// <returns>A <see cref="MultiPolygon"/> object</returns>
        public override MultiPolygon CreateMultiPolygon(Polygon[] polygons)
        {
            if (polygons == null || polygons.Length == 0)
                return CreateMultiPolygon();

            // Check if polygons were created with a ring orientation enforcing factory and that the ring
            // orientation matches that of this one.
            if (polygons[0]?.Factory is GeometryFactoryEx gfe && gfe.OrientationOfExteriorRing == OrientationOfExteriorRing)
                return new MultiPolygon(polygons, this);

            for (int i = 0; i < polygons.Length; i++)
                polygons[i] = CreatePolygon(polygons[i].Shell, polygons[i].Holes);

            return new MultiPolygon(polygons, this);
        }

        /// <summary>
        /// Utility function to enforce a specific ring orientation on a linear ring
        /// </summary>
        /// <param name="ring">The ring</param>
        /// <param name="ringOrientation">The required orientation</param>
        /// <returns>A ring</returns>
        private static LinearRing EnsureOrientation(LinearRing ring, LinearRingOrientation ringOrientation)
        {
            if (ringOrientation == LinearRingOrientation.DontCare)
                return ring;

            if (ringOrientation == LinearRingOrientation.CCW && !ring.IsCCW ||
                ringOrientation == LinearRingOrientation.CW && ring.IsCCW)
                ring = (LinearRing)ring.Reverse();

            return ring;
        }
    }
}
