using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a point, ensuring that
    /// the result is a valid point having the
    /// same dimension and number of components as the input.
    /// The simplification uses a maximum distance difference algorithm
    /// similar to the one used in the Douglas-Peucker algorithm.
    /// In particular, if the input is an areal point
    /// ( <c>Polygon</c> or <c>MultiPolygon</c> )
    /// The result has the same number of shells and holes (rings) as the input,
    /// in the same order
    /// The result rings touch at no more than the number of touching point in the input
    /// (although they may touch at fewer points).
    /// </summary>
    public class TopologyPreservingSimplifier
    {
        public static IGeometry Simplify(IGeometry geom, Double distanceTolerance)
        {
            TopologyPreservingSimplifier tss = new TopologyPreservingSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private IGeometry inputGeom;
        private TaggedLinesSimplifier lineSimplifier = new TaggedLinesSimplifier();
        private IDictionary lineStringMap;

        public TopologyPreservingSimplifier(IGeometry inputGeom)
        {
            this.inputGeom = inputGeom;
        }

        public Double DistanceTolerance
        {
            get { return lineSimplifier.DistanceTolerance; }
            set { lineSimplifier.DistanceTolerance = value; }
        }

        public IGeometry GetResultGeometry()
        {
            lineStringMap = new Hashtable();
            inputGeom.Apply(new LineStringMapBuilderFilter(this));
            lineSimplifier.Simplify(new ArrayList(lineStringMap.Values));
            IGeometry result = (new LineStringTransformer(this)).Transform(inputGeom);
            return result;
        }

        private class LineStringTransformer : GeometryTransformer
        {
            private TopologyPreservingSimplifier container = null;

            public LineStringTransformer(TopologyPreservingSimplifier container)
            {
                this.container = container;
            }

            protected override ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
            {
                if (parent is ILineString)
                {
                    TaggedLineString taggedLine = (TaggedLineString) container.lineStringMap[parent];
                    return CreateCoordinateSequence(taggedLine.ResultCoordinates);
                }
                // for anything else (e.g. points) just copy the coordinates
                return base.TransformCoordinates(coords, parent);
            }
        }

        private class LineStringMapBuilderFilter : IGeometryComponentFilter
        {
            private TopologyPreservingSimplifier container = null;

            public LineStringMapBuilderFilter(TopologyPreservingSimplifier container)
            {
                this.container = container;
            }

            public void Filter(IGeometry geom)
            {
                if (geom is ILinearRing)
                {
                    TaggedLineString taggedLine = new TaggedLineString((ILineString) geom, 4);
                    container.lineStringMap.Add(geom, taggedLine);
                }
                else if (geom is ILineString)
                {
                    TaggedLineString taggedLine = new TaggedLineString((ILineString) geom, 2);
                    container.lineStringMap.Add(geom, taggedLine);
                }
            }
        }
    }
}