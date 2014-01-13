using System.Collections.Generic;
using GeoAPI.Geometries;
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
    /// (<see cref="IPolygon"/> or <see cref="IMultiPolygon"/>):
    /// <list type="Bullet">
    /// <item>The result has the same number of shells and holes as the input,
    ///  with the same topological structure</item>
    /// <item>The result rings touch at no more than the number of touching points in the input
    /// (although they may touch at fewer points).
    /// The key implication of this statement is that if the
    /// input is topologically valid, so is the simplified output.</item>
    /// </list>
    /// For linear geometries, if the input does not contain
    /// any intersecting line segments, this property
    /// will be preserved in the output.
    /// <para/>
    /// For all geometry types, the result will contain 
    /// enough vertices to ensure validity.  For polygons
    /// and closed linear geometries, the result will have at
    /// least 4 vertices; for open linestrings the result
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
    /// <list type="Bullet">
    /// <item>If a small hole is very near an edge, it is possible for the edge to be moved by
    /// a relatively large tolerance value and end up with the hole outside the result shell.
    /// Similarly, it is possible for a small polygon component to end up inside
    /// a nearby larger polygon.
    ///  A workaround is to test for this situation in post-processing and remove
    /// any invalid holes or polygons.</item>
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
        public static IGeometry Simplify(IGeometry geom, double distanceTolerance)
        {
            TopologyPreservingSimplifier tss = new TopologyPreservingSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private readonly IGeometry _inputGeom;
        private readonly TaggedLinesSimplifier _lineSimplifier = new TaggedLinesSimplifier();
        private Dictionary<ILineString, TaggedLineString> _lineStringMap;

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
            get { return _lineSimplifier.DistanceTolerance; }
            set { _lineSimplifier.DistanceTolerance = value; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IGeometry GetResultGeometry()
        {
            // empty input produces an empty result
            if (_inputGeom.IsEmpty)
                return (IGeometry)_inputGeom.Clone();

            _lineStringMap = new Dictionary<ILineString, TaggedLineString>();
            LineStringMapBuilderFilter filter = new LineStringMapBuilderFilter(this);
            _inputGeom.Apply(filter);
            _lineSimplifier.Simplify(_lineStringMap.Values);
            LineStringTransformer transformer = new LineStringTransformer(this);
            IGeometry result = transformer.Transform(_inputGeom);
            return result;
        }

        /// <summary>
        ///
        /// </summary>
        private class LineStringTransformer : GeometryTransformer
        {
            private readonly TopologyPreservingSimplifier _container;

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
                // for empty coordinate sequences return null
                if (coords.Count == 0)
                    return null;

                // for linear components (including rings), simplify the linestring
                ILineString s = parent as ILineString;
                if (s != null)
                {
                    TaggedLineString taggedLine = _container._lineStringMap[s];
                    return CreateCoordinateSequence(taggedLine.ResultCoordinates);
                }
                // for anything else (e.g. points) just copy the coordinates
                return base.TransformCoordinates(coords, parent);
            }
        }

        /// <summary>
        /// A filter to add linear geometries to the linestring map 
        /// with the appropriate minimum size constraint.
        /// Closed <see cref="ILineString"/>s (including <see cref="ILinearRing"/>s
        /// have a minimum output size constraint of 4, 
        /// to ensure the output is valid.
        /// For all other linestrings, the minimum size is 2 points.
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
            public void Filter(IGeometry geom)
            {
                ILineString line = geom as ILineString;
                if (line == null)
                    return;
                if (line.IsEmpty)
                    return;
                int minSize = line.IsClosed ? 4 : 2;
                TaggedLineString taggedLine = new TaggedLineString(line, minSize);
                _container._lineStringMap.Add(line, taggedLine);
            }
        }
    }
}