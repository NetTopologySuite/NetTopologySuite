using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index.Quadtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing{TCoordinate}" />s are
    /// nested inside another ring in the set, using a <see cref="Quadtree{TCoordinate,TItem}"/>
    /// index to speed up the comparisons.
    /// </summary>
    public class QuadtreeNestedRingTester<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private readonly GeometryGraph<TCoordinate> _graph; // used to find non-node vertices
        private readonly List<ILinearRing<TCoordinate>> _rings = new List<ILinearRing<TCoordinate>>();
        private readonly IExtents<TCoordinate> _totalExtents;
        private Quadtree<TCoordinate, ILinearRing<TCoordinate>> _index;
        private TCoordinate _nestedPoint;

        public QuadtreeNestedRingTester(IGeometryFactory<TCoordinate> geoFactory, GeometryGraph<TCoordinate> graph)
        {
            _geoFactory = geoFactory;
            _totalExtents = new Extents<TCoordinate>(geoFactory);
            _graph = graph;
        }

        public TCoordinate NestedPoint
        {
            get { return _nestedPoint; }
        }

        public void Add(ILinearRing<TCoordinate> ring)
        {
            _rings.Add(ring);
            _totalExtents.ExpandToInclude(ring.Extents);
        }

        public Boolean IsNonNested()
        {
            buildQuadtree();

            foreach (ILinearRing<TCoordinate> innerRing in _rings)
            {
                IEnumerable<TCoordinate> innerRingCoordinates = innerRing.Coordinates;

                IEnumerable<ILinearRing<TCoordinate>> results = _index.Query(innerRing.Extents);

                foreach (ILinearRing<TCoordinate> searchRing in results)
                {
                    IEnumerable<TCoordinate> searchRingCoordinates = searchRing.Coordinates;

                    if (innerRing == searchRing)
                    {
                        continue;
                    }

                    if (!innerRing.Extents.Intersects(searchRing.Extents))
                    {
                        continue;
                    }

                    TCoordinate innerRingPt = IsValidOp<TCoordinate>.FindPointNotNode(
                        innerRingCoordinates, searchRing, _graph);

                    Assert.IsTrue(!Coordinates<TCoordinate>.IsEmpty(innerRingPt),
                                  "Unable to find a ring point not a node of the search ring");

                    Boolean isInside = CGAlgorithms<TCoordinate>.IsPointInRing(innerRingPt, searchRingCoordinates);

                    if (isInside)
                    {
                        _nestedPoint = innerRingPt;
                        return false;
                    }
                }
            }

            return true;
        }

        private void buildQuadtree()
        {
            _index = new Quadtree<TCoordinate, ILinearRing<TCoordinate>>(_geoFactory);

            foreach (ILinearRing<TCoordinate> ring in _rings)
            {
                _index.Insert(ring);
            }
        }
    }
}