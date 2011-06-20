using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    public class IndexedNestedRingTester<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible

    {
        private GeometryGraph<TCoordinate> _graph;  // used to find non-node vertices
        private List<ILinearRing<TCoordinate>> _rings = new List<ILinearRing<TCoordinate>>();
        private IExtents<TCoordinate> _totalEnv;
        private StrTree<TCoordinate, ILinearRing<TCoordinate>> _index;
        private TCoordinate _nestedPt;

        public IndexedNestedRingTester(IGeometryFactory<TCoordinate> geoFactory , GeometryGraph<TCoordinate> graph)
        {
            _graph = graph;
            _totalEnv = geoFactory.CreateExtents();
            _index = new StrTree<TCoordinate, ILinearRing<TCoordinate>>(geoFactory);
        }

        public TCoordinate NestedPoint
        {
            get { return _nestedPt; }
        }

        public void Add(ILinearRing<TCoordinate> ring)
        {
            if ( ring.IsEmpty )
                return;
            _rings.Add(ring);
            _totalEnv.ExpandToInclude(ring.Extents);
            _index.Insert(ring);
        }

        public Boolean IsNonNested()
        {

            foreach (ILinearRing<TCoordinate> innerRing in _rings)
            {
                ICoordinateSequence<TCoordinate> innerRingPts =
                    innerRing.Coordinates;
                foreach (ILinearRing<TCoordinate> searchRing in _index.Query(innerRing.Extents))
                {
                    if (innerRing == searchRing)
                        continue;
                    if (!innerRing.Extents.Intersects(searchRing.Extents))
                        continue;

                    TCoordinate innerRingPt = IsValidOp<TCoordinate>.FindPointNotNode(innerRingPts, searchRing, _graph);
                    Assert.IsTrue(!Coordinates<TCoordinate>.IsEmpty(innerRingPt),
                              "Unable to find a ring point not a node of the search ring");

                    Boolean isInside = CGAlgorithms<TCoordinate>.IsPointInRing(innerRingPt, searchRing.Coordinates);
                    if ( isInside )
                    {
                        _nestedPt = innerRingPt;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
