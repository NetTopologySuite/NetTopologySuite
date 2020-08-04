using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a point and ensures that
    /// the result is a valid point having the
    /// same dimension and number of components as the input,
    /// and with the components having the same topological
    /// relationship.
    /// <para/>
    /// If the input is a polygonal geometry
    /// (<see cref="Polygon"/> or <see cref="MultiPolygon"/>):
    /// <list type="bullet">
    /// <item><description>The result has the same number of shells and holes as the input,
    ///  with the same topological structure</description></item>
    /// <item><description>The result rings touch at no more than the number of touching points in the input
    /// (although they may touch at fewer points).
    /// The key implication of this statement is that if the
    /// input is topologically valid, so is the simplified output.</description></item>
    /// </list>
    /// For linear geometries, if the input does not contain
    /// any intersecting line segments, this property
    /// will be preserved in the output.
    /// <para/>
    /// For all geometry types, the result will contain
    /// enough vertices to ensure validity.  For polygons
    /// and closed linear geometries, the result will have at
    /// least 4 vertices; for open LineStrings the result
    /// will have at least 2 vertices.
    /// <para/>
    /// All geometry types are handled.
    /// Empty and point geometries are returned unchanged.
    /// Empty geometry components are deleted.
    /// <para/>
    /// The simplification uses a maximum-distance difference algorithm
    /// similar to the Douglas-Peucker algorithm.
    /// </summary>
    /// <remarks>
    /// <h3>KNOWN BUGS</h3>
    /// <list type="bullet">
    /// <item><description>May create invalid topology if there are components which are small
    /// relative to the tolerance value.
    /// In particular, if a small hole is very near an edge,
    /// it is possible for the edge to be moved by a relatively large tolerance value
    /// and end up with the hole outside the result shell (or inside another hole).
    /// Similarly, it is possible for a small polygon component to end up inside
    /// a nearby larger polygon.
    /// A workaround is to test for this situation in post-processing and remove
    /// any invalid holes or polygons.</description></item>
    /// </list>
    /// </remarks>
    /// <seealso cref="DouglasPeuckerSimplifier"/>
    public class TopologyPreservingSimplifier
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static Geometry Simplify(Geometry geom, double distanceTolerance)
        {
            var tss = new TopologyPreservingSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private readonly Geometry _inputGeom;
        private readonly TaggedLinesSimplifier _lineSimplifier = new TaggedLinesSimplifier();
        private Dictionary<LineString, TaggedLineString> _lineStringMap;

        /// <summary>
        /// Creates an instance of this class for the provided <paramref name="inputGeom"/> geometry
        /// </summary>
        /// <param name="inputGeom">The geometry to simplify</param>
        public TopologyPreservingSimplifier(Geometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        /// <summary>
        /// Gets or sets the distance tolerance for the simplification.<br/>
        /// Points closer than this tolerance to a simplified segment may
        /// be removed.
        /// </summary>
        public double DistanceTolerance
        {
            get => _lineSimplifier.DistanceTolerance;
            set => _lineSimplifier.DistanceTolerance = value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Geometry GetResultGeometry()
        {
            // empty input produces an empty result
            if (_inputGeom.IsEmpty)
                return (Geometry)_inputGeom.Copy();

            _lineStringMap = new Dictionary<LineString, TaggedLineString>();
            var filter = new LineStringMapBuilderFilter(this);
            _inputGeom.Apply(filter);
            _lineSimplifier.Simplify(_lineStringMap.Values);
            var transformer = new LineStringTransformer(this);
            var result = transformer.Transform(_inputGeom);
            return result;
        }

        /// <summary>
        /// A LineString transformer
        /// </summary>
        private class LineStringTransformer : GeometryTransformer
        {
            private readonly TopologyPreservingSimplifier _container;

            public LineStringTransformer(TopologyPreservingSimplifier container)
            {
                _container = container;
            }

            /// <inheritdoc cref="GeometryTransformer.TransformCoordinates(CoordinateSequence, Geometry)"/>>
            protected override CoordinateSequence TransformCoordinates(CoordinateSequence coords, Geometry parent)
            {
                // for empty coordinate sequences return null
                if (coords.Count == 0)
                    return null;

                // for linear components (including rings), simplify the LineString
                var s = parent as LineString;
                if (s != null)
                {
                    var taggedLine = _container._lineStringMap[s];
                    return CreateCoordinateSequence(taggedLine.ResultCoordinates);
                }
                // for anything else (e.g. points) just copy the coordinates
                return base.TransformCoordinates(coords, parent);
            }
        }

        /// <summary>
        /// A filter to add linear geometries to the LineString map
        /// with the appropriate minimum size constraint.
        /// Closed <see cref="LineString"/>s (including <see cref="LinearRing"/>s
        /// have a minimum output size constraint of 4,
        /// to ensure the output is valid.
        /// For all other LineStrings, the minimum size is 2 points.
        /// </summary>
        /// <author>Martin Davis</author>
        private class LineStringMapBuilderFilter : IGeometryComponentFilter
        {
            private readonly TopologyPreservingSimplifier _container;

            public LineStringMapBuilderFilter(TopologyPreservingSimplifier container)
            {
                _container = container;
            }

            /// <summary>
            /// Filters linear geometries.
            /// </summary>
            /// <param name="geom">A geometry of any type</param>
            public void Filter(Geometry geom)
            {
                var line = geom as LineString;
                if (line == null)
                    return;
                if (line.IsEmpty)
                    return;
                int minSize = line.IsClosed ? 4 : 2;
                var taggedLine = new TaggedLineString(line, minSize);
                _container._lineStringMap.Add(line, taggedLine);
            }
        }
    }
}
