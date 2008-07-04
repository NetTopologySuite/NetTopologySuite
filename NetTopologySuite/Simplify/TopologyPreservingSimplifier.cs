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

        private IGeometry inputGeom;
        private TaggedLinesSimplifier lineSimplifier = new TaggedLinesSimplifier();
        private IDictionary lineStringMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputGeom"></param>
        public TopologyPreservingSimplifier(IGeometry inputGeom)
        {
            this.inputGeom = inputGeom;            
        }

        /// <summary>
        /// 
        /// </summary>
        public double DistanceTolerance
        {
            get
            {
                return lineSimplifier.DistanceTolerance;
            }
            set
            {
                lineSimplifier.DistanceTolerance = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IGeometry GetResultGeometry() 
        {
            lineStringMap = new Hashtable();
            inputGeom.Apply(new LineStringMapBuilderFilter(this));
            lineSimplifier.Simplify(new ArrayList(lineStringMap.Values));
            IGeometry result = (new LineStringTransformer(this)).Transform(inputGeom);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        private class LineStringTransformer : GeometryTransformer
        {
            private TopologyPreservingSimplifier container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public LineStringTransformer(TopologyPreservingSimplifier container)            
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coords"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        private class LineStringMapBuilderFilter : IGeometryComponentFilter
        {
            private TopologyPreservingSimplifier container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public LineStringMapBuilderFilter(TopologyPreservingSimplifier container)            
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="geom"></param>
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
