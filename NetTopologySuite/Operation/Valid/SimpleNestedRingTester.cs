using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Diagnostics;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing{TCoordinate}" />s are
    /// nested inside another ring in the set, using a simple O(n^2)
    /// comparison.
    /// </summary>
    public class SimpleNestedRingTester<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly GeometryGraph<TCoordinate> _graph; // used to find non-node vertices
        private readonly List<ILinearRing<TCoordinate>> _rings = new List<ILinearRing<TCoordinate>>();
        private TCoordinate nestedPt;

        public SimpleNestedRingTester(GeometryGraph<TCoordinate> graph)
        {
            _graph = graph;
        }

        public ICoordinate NestedPoint
        {
            get { return nestedPt; }
        }

        public void Add(ILinearRing<TCoordinate> ring)
        {
            _rings.Add(ring);
        }

        public Boolean IsNonNested()
        {
            foreach (ILinearRing<TCoordinate> innerRing in _rings)
            {
                foreach (ILinearRing<TCoordinate> searchRing in _rings)
                {
                    if (innerRing == searchRing)
                    {
                        continue;
                    }

                    IEnumerable<TCoordinate> innerRingPts = innerRing.Coordinates;
                    IEnumerable<TCoordinate> searchRingPts = searchRing.Coordinates;

                    if (!innerRing.Extents.Intersects(searchRing.Extents))
                    {
                        continue;
                    }

                    TCoordinate innerRingPt = IsValidOp<TCoordinate>.FindPointNotNode(innerRingPts, searchRing, _graph);
                    Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");

                    Boolean isInside = CGAlgorithms<TCoordinate>.IsPointInRing(innerRingPt, searchRingPts);
                    if (isInside)
                    {
                        nestedPt = innerRingPt;
                        return false;
                    }
                }
            }

            return true;
        }
    }
}