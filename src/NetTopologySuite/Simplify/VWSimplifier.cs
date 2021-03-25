using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a <see cref="Geometry"/> using the Visvalingam-Whyatt area-based algorithm.
    /// Ensures that any polygonal geometries returned are valid. Simple lines are not
    /// guaranteed to remain simple after simplification. All geometry types are
    /// handled. Empty and point geometries are returned unchanged. Empty geometry
    /// components are deleted.
    /// The simplification tolerance is specified as a distance.
    /// This is converted to an area tolerance by squaring it.
    /// <para>
    /// <b>Known Bugs</b>
    /// * Not yet optimized for performance.
    /// * Does not simplify the endpoint of rings.
    /// <b>To Do</b>
    /// * Allow specifying desired number of vertices in the output.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Note that in general this algorithm does not preserve topology - e.g. polygons can be split,
    /// collapse to lines or disappear holes can be created or disappear, and lines
    /// can cross.
    /// </remarks>
    /// <version>1.7</version>
    public class VWSimplifier
    {
        /// <summary>
        /// Simplifies a <see cref="Geometry"/> using a given tolerance.
        /// </summary>
        /// <param name="geom">The <see cref="Geometry"/> to simplify.</param>
        /// <param name="distanceTolerance">The tolerance to use.</param>
        /// <returns>A simplified version of the <see cref="Geometry"/>.</returns>
        public static Geometry Simplify(Geometry geom, double distanceTolerance)
        {
            var simp = new VWSimplifier(geom);
            simp.DistanceTolerance = distanceTolerance;
            return simp.GetResultGeometry();
        }

        private readonly Geometry _inputGeom;
        private double _distanceTolerance;
        private bool _isEnsureValidTopology = true;

        /// <summary>
        /// Creates a simplifier for a given <see cref="Geometry"/>.
        /// </summary>
        /// <param name="inputGeom">The <see cref="Geometry"/> to simplify.</param>
        public VWSimplifier(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        /// <summary>
        /// Sets the distance tolerance for the simplification. All vertices in the
        /// simplified <see cref="Geometry"/> will be within this distance of the original geometry.
        /// The tolerance value must be non-negative.
        /// </summary>
        public double DistanceTolerance
        {
            get => _distanceTolerance;
            set
            {
                if (value < 0.0)
                    throw new ArgumentException("Tolerance must be non-negative");
                _distanceTolerance = value;
            }
        }

        /// <summary>
        /// Controls whether simplified polygons will be "fixed" to have valid
        /// topology. The caller may choose to disable this because:
        /// * valid topology is not required.
        /// * fixing topology is a relative expensive operation.
        /// * in some pathological cases the topology fixing operation may either
        /// fail or run for too long.
        /// </summary>
        /// <remarks>The default is to fix polygon topology.</remarks>
        public bool IsEnsureValidTopology
        {
            get => _isEnsureValidTopology;
            set => _isEnsureValidTopology = value;
        }

        /// <summary>
        /// Gets the simplified <see cref="Geometry"/>.
        /// </summary>
        /// <returns>The simplified <see cref="Geometry"/>.</returns>
        public Geometry GetResultGeometry()
        {
            // empty input produces an empty result
            if (_inputGeom.IsEmpty)
                return (Geometry)_inputGeom.Copy();

            var transformer = new VWTransformer(IsEnsureValidTopology, DistanceTolerance);
            return transformer.Transform(_inputGeom);
        }

        class VWTransformer : GeometryTransformer
        {
            private readonly bool _isEnsureValidTopology = true;
            private readonly double _distanceTolerance;

            public VWTransformer(bool isEnsureValidTopology, double distanceTolerance)
            {
                _isEnsureValidTopology = isEnsureValidTopology;
                _distanceTolerance = distanceTolerance;
            }

            protected override CoordinateSequence TransformCoordinates(CoordinateSequence coords, Geometry parent)
            {
                var inputPts = coords.ToCoordinateArray();
                Coordinate[] newPts;
                if (inputPts.Length == 0)
                    newPts = new Coordinate[0];
                else newPts = VWLineSimplifier.Simplify(inputPts, _distanceTolerance);
                return Factory.CoordinateSequenceFactory.Create(newPts);
            }

            /// <summary>
            /// Simplifies a <see cref="Polygon"/>, fixing it if required.
            /// </summary>
            /// <param name="geom"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override Geometry TransformPolygon(Polygon geom, Geometry parent)
            {
                // empty geometries are simply removed
                if (geom.IsEmpty)
                    return null;
                var rawGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is MultiPolygon)
                    return rawGeom;
                return CreateValidArea(rawGeom);
            }

            /// <summary>
            /// Simplifies a <see cref="LinearRing"/>. If the simplification results in a degenerate
            /// ring, remove the component.
            /// </summary>
            /// <param name="geom"></param>
            /// <param name="parent"></param>
            /// <returns><c>null</c> if the simplification results in a degenerate ring.</returns>
            protected override Geometry TransformLinearRing(LinearRing geom, Geometry parent)
            {
                bool removeDegenerateRings = parent is Polygon;
                var simpResult = base.TransformLinearRing(geom, parent);
                if (removeDegenerateRings && !(simpResult is LinearRing))
                    return null;
                return simpResult;
            }

            /// <summary>
            /// Simplifies a <see cref="MultiPolygon"/>, fixing it if required.
            /// </summary>
            /// <param name="geom"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
            {
                var rawGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(rawGeom);
            }

            /// <summary>
            /// Creates a valid area geometry from one that possibly has bad topology
            /// (i.e. self-intersections). Since buffer can handle invalid topology, but
            /// always returns valid geometry, constructing a 0-width buffer "corrects"
            /// the topology. Note this only works for area geometries, since buffer
            /// always returns areas. This also may return empty geometries, if the input
            /// has no actual area.
            /// </summary>
            /// <param name="rawAreaGeom">An area geometry possibly containing self-intersections.</param>
            /// <returns>A valid area geometry.</returns>
            private Geometry CreateValidArea(Geometry rawAreaGeom)
            {
                // if geometry is invalid then make it valid
                if (_isEnsureValidTopology && !rawAreaGeom.IsValid)
                    return rawAreaGeom.Buffer(0.0);
                return rawAreaGeom;
            }
        }
    }
}
