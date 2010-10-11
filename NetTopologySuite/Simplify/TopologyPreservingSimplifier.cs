using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a point, ensuring that
    /// the result is a valid point having the
    /// same dimension and number of components as the input.
    /// </summary>
    /// <remarks>
    /// The simplification uses a maximum distance difference algorithm
    /// similar to the one used in the Douglas-Peucker algorithm.
    /// In particular, if the input is an areal point
    /// ( <see cref="IPolygon{TCoordinate}" /> or <see cref="IMultiPolygon{TCoordinate}"/> )
    /// The result has the same number of shells and holes (rings) as the input,
    /// in the same order
    /// The result rings touch at no more than the number of touching point in the input
    /// (although they may touch at fewer points).
    /// </remarks>
    public class TopologyPreservingSimplifier<TCoordinate>
         where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        private static IGeometryFactory<TCoordinate> _geoFactory;
        public static IGeometryFactory<TCoordinate> GeometryFactory
        {
            get { return _geoFactory; }
            set { _geoFactory = value; }
        }
        
        public static IGeometry<TCoordinate> Simplify(IGeometry<TCoordinate> geom, Double distanceTolerance)
        {
            TopologyPreservingSimplifier<TCoordinate> simplifier 
                = new TopologyPreservingSimplifier<TCoordinate>(geom);
            simplifier.DistanceTolerance = distanceTolerance;
            return simplifier.GetResultGeometry();
        }

        private readonly IGeometry<TCoordinate> _inputGeom;
        private readonly TaggedLinesSimplifier<TCoordinate> _lineSimplifier = new TaggedLinesSimplifier<TCoordinate>();
        private readonly Dictionary<IGeometry<TCoordinate>, TaggedLineString<TCoordinate>> _lineStringMap 
            = new Dictionary<IGeometry<TCoordinate>, TaggedLineString<TCoordinate>>();

        public TopologyPreservingSimplifier(IGeometry<TCoordinate> inputGeom)
        {
            _inputGeom = inputGeom;
        }

        public Double DistanceTolerance
        {
            get { return _lineSimplifier.DistanceTolerance; }
            set { _lineSimplifier.DistanceTolerance = value; }
        }

        public IGeometry<TCoordinate> GetResultGeometry()
        {

            TopologyPreservingSimplifier<TCoordinate> container;
            foreach (ILineString<TCoordinate> lineString in GeometryFilter.Filter<ILineString<TCoordinate>, TCoordinate>(_inputGeom))
            {
                Int32 pointCount = lineString is ILinearRing<TCoordinate> ? 4 : 2;

                TaggedLineString<TCoordinate> taggedLine = new TaggedLineString<TCoordinate>(lineString, pointCount);
                _lineStringMap.Add(lineString, taggedLine);
            }

            //_inputGeom.Apply(new LineStringMapBuilderFilter(this));
            IEnumerable<TaggedLineString<TCoordinate>> lineStrings = _lineStringMap.Values;
            _lineSimplifier.Simplify(lineStrings);
            IGeometry<TCoordinate> result = (new LineStringTransformer(this)).Transform(_inputGeom);
            return result;
        }

        private class LineStringTransformer : GeometryTransformer<TCoordinate>
        {
            private readonly TopologyPreservingSimplifier<TCoordinate> _container = null;

            public LineStringTransformer(TopologyPreservingSimplifier<TCoordinate> container)
            {
                _container = container;
                
            }

            protected override ICoordinateSequence<TCoordinate> TransformCoordinates(ICoordinateSequence<TCoordinate> coords, IGeometry<TCoordinate> parent)
            {
                if (parent is ILineString<TCoordinate>)
                {
                    TaggedLineString<TCoordinate> taggedLine;
                    _container._lineStringMap.TryGetValue(parent, out taggedLine);
                    Debug.Assert(taggedLine != null);
                    return CreateCoordinateSequence(taggedLine.ResultCoordinates);
                }

                // for anything else (e.g. points) just copy the coordinates
                return base.TransformCoordinates(coords, parent);
            }
        }

        /*
        private class LineStringMapBuilderFilter : IGeometryComponentFilter<TCoordinate>
        {
            private readonly TopologyPreservingSimplifier<TCoordinate> _container = null;

            public LineStringMapBuilderFilter(TopologyPreservingSimplifier<TCoordinate> container)
            {
                _container = container;
            }

            public void Filter(IGeometry<TCoordinate> geom)
            {
                ILineString<TCoordinate> line = geom as ILineString<TCoordinate>;
                Int32 pointCount;

                if (geom is ILinearRing)
                {
                    pointCount = 4;
                }
                else if (geom is ILineString)
                {
                    pointCount = 2;
                }
                else
                {
                    return;
                }
                
                TaggedLineString<TCoordinate> taggedLine = new TaggedLineString<TCoordinate>(line, pointCount);
                _container._lineStringMap.Add(geom, taggedLine);
            }
        }
         */
    }
}