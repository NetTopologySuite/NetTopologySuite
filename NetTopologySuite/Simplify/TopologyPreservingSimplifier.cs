using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a point, ensuring that
    /// the result is a valid point having the
    /// same dimension and number of components as the input.
    /// The simplification uses a maximum distance difference algorithm
    /// similar to the one used in the Douglas-Peucker algorithm.
    /// In particular, if the input is an areal point
    /// ( <c>Polygon</c> or <c>MultiPolygon</c> ):
    /// The result has the same number of shells and holes (rings) as the input,
    /// in the same order
    /// The result rings touch at no more than the number of touching point in the input
    /// (although they may touch at fewer points).
    /// (The key implication of this constraint is that the
    /// output will be topologically valid if the input was.)
    /// </summary>
    /// <remarks>
    /// <h3>KNOWN BUGS</h3>
    /// <list type="Bullet">
    /// <item>If a small hole is very near an edge, it is possible for the edge to be moved by
    /// a relatively large tolerance value and end up with the hole outside the result shell.
    /// Similarly, it is possible for a small polygon component to end up inside
    /// a nearby larger polygon.
    ///  A workaround is to test for this situation in post-processing and remove
    /// any invalid holes or polygons.</item>
    /// </list>
    /// </remarks>
    /// <see cref="DouglasPeuckerSimplifier"/>
    public class TopologyPreservingSimplifier
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static IGeometry Simplify(IGeometry geom, double distanceTolerance)
        {
            TopologyPreservingSimplifier tss = new TopologyPreservingSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private readonly IGeometry _inputGeom;
        private readonly TaggedLinesSimplifier _lineSimplifier = new TaggedLinesSimplifier();
        private IDictionary<ILineString, TaggedLineString> _lineStringMap;

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputGeom"></param>
        public TopologyPreservingSimplifier(IGeometry inputGeom)
        {
            _inputGeom = inputGeom;
        }

        /// <summary>
        ///
        /// </summary>
        public double DistanceTolerance
        {
            get
            {
                return _lineSimplifier.DistanceTolerance;
            }
            set
            {
                _lineSimplifier.DistanceTolerance = value;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IGeometry GetResultGeometry()
        {
            _lineStringMap = new Dictionary<ILineString, TaggedLineString>();
            _inputGeom.Apply(new LineStringMapBuilderFilter(this));
            _lineSimplifier.Simplify(_lineStringMap.Values);
            IGeometry result = (new LineStringTransformer(this)).Transform(_inputGeom);
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        private class LineStringTransformer : GeometryTransformer
        {
            private readonly TopologyPreservingSimplifier _container;

            /// <summary>
            ///
            /// </summary>
            /// <param name="container"></param>
            public LineStringTransformer(TopologyPreservingSimplifier container)
            {
                _container = container;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="coords"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
            {
                // for linear components (including rings), simplify the linestring
                if (parent is ILineString)
                {
                    TaggedLineString taggedLine = _container._lineStringMap[(ILineString)parent];
                    return CreateCoordinateSequence(taggedLine.ResultCoordinates);
                }
                // for anything else (e.g. points) just copy the coordinates
                return base.TransformCoordinates(coords, parent);
            }
        }

        /// <summary>
        ///
        /// </summary>
        private class LineStringMapBuilderFilter : IGeometryComponentFilter
        {
            private readonly TopologyPreservingSimplifier _container;

            /// <summary>
            ///
            /// </summary>
            /// <param name="container"></param>
            public LineStringMapBuilderFilter(TopologyPreservingSimplifier container)
            {
                _container = container;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="geom"></param>
            public void Filter(IGeometry geom)
            {
                if (geom is ILinearRing)
                {
                    TaggedLineString taggedLine = new TaggedLineString((ILineString)geom, 4);
                    _container._lineStringMap.Add((ILineString)geom, taggedLine);
                }
                else if (geom is ILineString)
                {
                    TaggedLineString taggedLine = new TaggedLineString((ILineString)geom, 2);
                    _container._lineStringMap.Add((ILineString)geom, taggedLine);
                }
            }
        }
    }
}