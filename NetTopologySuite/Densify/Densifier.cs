using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Densify
{
    /// <summary>
    /// Densifies a geometry by inserting extra vertices along the line segments
    /// contained in the geometry.
    /// All segments in the created densified geometry will be no longer than
    /// than the given distance tolerance.
    /// </summary>
    /// <remarks>
    /// <para>Densified polygonal geometries are guaranteed to be topologically correct.</para>
    /// <para>The coordinates created during densification respect the input geometry's <see cref="IPrecisionModel"/>.</para>
    /// <para><b>Note:</b> At some future point this class will offer a variety of densification strategies.</para>
    /// </remarks>
    /// <author>Martin Davis</author>
    public class Densifier
    {
        /// <summary>
        /// Densifies a geometry using a given distance tolerance, and respecting the input geometry's <see cref="IPrecisionModel"/>.
        /// </summary>
        /// <param name="geom">The geometry densify</param>
        /// <param name="distanceTolerance">The distance tolerance (<see cref="DistanceTolerance"/>)</param>
        /// <returns>The densified geometry</returns>
        public static IGeometry Densify(IGeometry geom, double distanceTolerance)
        {
            var densifier = new Densifier(geom) {DistanceTolerance = distanceTolerance};
            return densifier.GetResultGeometry();
        }

        /// <summary>
        /// Densifies a coordinate sequence.
        /// </summary>
        /// <param name="pts">The coordinate sequence to densify</param>
        /// <param name="distanceTolerance">The distance tolerance (<see cref="DistanceTolerance"/>)</param>
        /// <param name="precModel">The precision model to apply on the new coordinates</param>
        /// <returns>The densified coordinate sequence</returns>
        private static Coordinate[] DensifyPoints(Coordinate[] pts,
                                                   double distanceTolerance, IPrecisionModel precModel)
        {
            var seg = new LineSegment();
            var coordList = new CoordinateList();
            for (int i = 0; i < pts.Length - 1; i++)
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                coordList.Add(seg.P0, false);
                double len = seg.Length;
                int densifiedSegCount = (int) (len/distanceTolerance) + 1;
                if (densifiedSegCount > 1)
                {
                    double densifiedSegLen = len/densifiedSegCount;
                    for (int j = 1; j < densifiedSegCount; j++)
                    {
                        double segFract = (j*densifiedSegLen)/len;
                        var p = seg.PointAlong(segFract);
                        precModel.MakePrecise(p);
                        coordList.Add(p, false);
                    }
                }
            }
            coordList.Add(pts[pts.Length - 1], false);
            return coordList.ToCoordinateArray();
        }

        private readonly IGeometry _inputGeom;
        private double _distanceTolerance;

        ///<summary>Creates a new densifier instance</summary>
        /// <param name="inputGeom">The geometry to densify</param>
        public Densifier(IGeometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        /// <summary>
        /// Gets or sets the distance tolerance for the densification. All line segments
        /// in the densified geometry will be no longer than the distance tolerance.
        /// Simplified geometry will be within this distance of the original geometry.
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
        /// Gets the densified geometry.
        /// </summary>
        /// <returns>The densified geometry</returns>
        public IGeometry GetResultGeometry()
        {
            return (new DensifyTransformer(_distanceTolerance)).Transform(_inputGeom);
        }

        private class DensifyTransformer : GeometryTransformer
        {
            private readonly double _distanceTolerance;

            public DensifyTransformer(double distanceTolerance)
            {
                _distanceTolerance = distanceTolerance;
            }

            protected override ICoordinateSequence TransformCoordinates(
                ICoordinateSequence coords, IGeometry parent)
            {
                var inputPts = coords.ToCoordinateArray();
                var newPts = Densifier
                    .DensifyPoints(inputPts, _distanceTolerance, parent.PrecisionModel);
                // prevent creation of invalid LineStrings
                if (parent is ILineString && newPts.Length == 1)
                {
                    newPts = new Coordinate[0];
                }
                return Factory.CoordinateSequenceFactory.Create(newPts);
            }

            protected override IGeometry TransformPolygon(IPolygon geom, IGeometry parent)
            {
                var roughGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is IMultiPolygon)
                {
                    return roughGeom;
                }
                return CreateValidArea(roughGeom);
            }

            protected override IGeometry TransformMultiPolygon(IMultiPolygon geom, IGeometry parent)
            {
                var roughGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(roughGeom);
            }

            ///<summary>
            /// Creates a valid area geometry from one that possibly has bad topology
            /// (i.e. self-intersections). Since buffer can handle invalid topology, but
            /// always returns valid geometry, constructing a 0-width buffer "corrects"
            /// the topology. Note this only works for area geometries, since buffer
            /// always returns areas. This also may return empty geometries, if the input
            /// has no actual area.
            ///</summary>
            /// <param name="roughAreaGeom">An area geometry possibly containing self-intersections</param>
            /// <returns>A valid area geometry</returns>
            private static IGeometry CreateValidArea(IGeometry roughAreaGeom)
            {
                return roughAreaGeom.Buffer(0.0);
            }
        }

    }
}