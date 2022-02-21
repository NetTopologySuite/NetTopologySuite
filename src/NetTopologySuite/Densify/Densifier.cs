using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Densify
{
    /// <summary>
    /// Densifies a geometry by inserting extra vertices along the line segments
    /// contained in the geometry.
    /// All segments in the created densified geometry will be <b>no longer</b>
    /// than the given distance tolerance
    /// (that is, all segments in the output will have length less than or equal to
    /// the distance tolerance).
    /// </summary>
    /// <remarks>
    /// Densified polygonal geometries are guaranteed to be topologically correct.
    /// <para/>
    /// The coordinates created during densification respect the input geometry's <see cref="PrecisionModel"/>.
    /// <para/>
    /// By default polygonal results are processed to ensure they are valid.
    /// This processing is costly, and it is very rare for results to be invalid.
    /// Validation processing can be disabled by setting the <see cref="Validate"/> property to <c>false</c>.
    /// <para/>
    /// <b>Note:</b> At some future point this class will offer a variety of densification strategies.
    /// </remarks>
    /// <author>Martin Davis</author>
    public class Densifier
    {
        /// <summary>
        /// Densifies a geometry using a given distance tolerance, and respecting the input geometry's <see cref="PrecisionModel"/>.
        /// </summary>
        /// <param name="geom">The geometry densify</param>
        /// <param name="distanceTolerance">The distance tolerance (<see cref="DistanceTolerance"/>)</param>
        /// <returns>The densified geometry</returns>
        public static Geometry Densify(Geometry geom, double distanceTolerance)
        {
            var densifier = new Densifier(geom) {DistanceTolerance = distanceTolerance};
            return densifier.GetResultGeometry();
        }

        /// <summary>
        /// Densifies a list of coordinates.
        /// </summary>
        /// <param name="pts">The coordinate list</param>
        /// <param name="distanceTolerance">The densify tolerance</param>
        /// <param name="precModel">The precision model to apply on the new coordinates</param>
        /// <returns>The densified coordinate sequence</returns>
        private static Coordinate[] DensifyPoints(Coordinate[] pts,
                                                   double distanceTolerance, PrecisionModel precModel)
        {
            var seg = new LineSegment();
            var coordList = new CoordinateList();
            for (int i = 0; i < pts.Length - 1; i++)
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                coordList.Add(seg.P0, false);
                double len = seg.Length;

                // check if no densification is required
                if (len <= distanceTolerance)
                    continue;

                // densify the segment
                int densifiedSegCount = (int) Math.Ceiling(len/distanceTolerance);
                double densifiedSegLen = len/densifiedSegCount;
                for (int j = 1; j < densifiedSegCount; j++)
                {
                    double segFract = (j*densifiedSegLen)/len;
                    var p = seg.PointAlong(segFract);
                    if (p is CoordinateZ pz)
                        pz.Z = seg.P0.Z + segFract * (seg.P1.Z - seg.P0.Z);
                    precModel.MakePrecise(p);
                    coordList.Add(p, false);
                }
            }

            // this check handles empty sequences
            if (pts.Length > 0)
                coordList.Add(pts[pts.Length - 1], false);

            return coordList.ToCoordinateArray();
        }

        private readonly Geometry _inputGeom;
        private double _distanceTolerance;

        /// <summary>
        /// Indicates whether areas should be topologically validated.
        /// <br/><b>Note:</b> JTS name <c>_isValidated</c>
        /// </summary>
        private bool _validate = true;

        /// <summary>Creates a new densifier instance</summary>
        /// <param name="inputGeom">The geometry to densify</param>
        public Densifier(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        /// <summary>
        /// Gets or sets the distance tolerance for the densification. All line segments
        /// in the densified geometry will be no longer than the distance tolerance.
        /// The distance tolerance must be positive.
        /// </summary>
        public double DistanceTolerance
        {
            get => _distanceTolerance;

            set
            {
                if (value <= 0.0)
                    throw new ArgumentException("Tolerance must be positive");
                _distanceTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets whether polygonal results are processed to ensure they are valid.
        /// </summary>
        /// <returns><c>true</c> if polygonal input is validated.</returns>
        public bool Validate
        {
            get => _validate;
            set => _validate = value;
        }

        /// <summary>
        /// Gets the densified geometry.
        /// </summary>
        /// <returns>The densified geometry</returns>
        public Geometry GetResultGeometry()
        {
            return new DensifyTransformer(_distanceTolerance, _validate).Transform(_inputGeom);
        }

        private class DensifyTransformer : GeometryTransformer
        {
            private readonly double _distanceTolerance;

            /// <summary>
            /// Indicates whether areas should be topologically validated.
            /// <br/><b>Note:</b> JTS name <c>_isValidated</c>
            /// </summary>
            private readonly bool _validate;

            public DensifyTransformer(double distanceTolerance, bool validate)
            {
                _distanceTolerance = distanceTolerance;
                _validate = validate;
            }

            protected override CoordinateSequence TransformCoordinates(
                CoordinateSequence coords, Geometry parent)
            {
                var inputPts = coords.ToCoordinateArray();
                var newPts =
                    DensifyPoints(inputPts, _distanceTolerance, parent.PrecisionModel);
                // prevent creation of invalid LineStrings
                if (parent is LineString && newPts.Length == 1)
                {
                    newPts = new Coordinate[0];
                }
                return Factory.CoordinateSequenceFactory.Create(newPts);
            }

            protected override Geometry TransformPolygon(Polygon geom, Geometry parent)
            {
                var roughGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is MultiPolygon)
                {
                    return roughGeom;
                }
                return CreateValidArea(roughGeom);
            }

            protected override Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
            {
                var roughGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(roughGeom);
            }

            /// <summary>
            /// Creates a valid area geometry from one that possibly has bad topology
            /// (i.e. self-intersections). Since buffer can handle invalid topology, but
            /// always returns valid geometry, constructing a 0-width buffer "corrects"
            /// the topology. Note this only works for area geometries, since buffer
            /// always returns areas. This also may return empty geometries, if the input
            /// has no actual area.
            /// </summary>
            /// <param name="roughAreaGeom">An area geometry possibly containing self-intersections</param>
            /// <returns>A valid area geometry</returns>
            private Geometry CreateValidArea(Geometry roughAreaGeom)
            {
                // if valid no need to process to make valid
                if (!_validate || roughAreaGeom.IsValid) return roughAreaGeom;
                return roughAreaGeom.Buffer(0.0);
            }
        }

    }
}
