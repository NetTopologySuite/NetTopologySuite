using System;
using System.Collections;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing{TCoordinate}" />s are
    /// nested inside another ring in the set, using a simple O(n^2)
    /// comparison.
    /// </summary>
    public class SimpleNestedRingTester<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private GeometryGraph<TCoordinate> _graph; // used to find non-node vertices
        private List<ILinearRing<TCoordinate>> _rings = new List<ILinearRing<TCoordinate>>();
        private TCoordinate nestedPt;

        public SimpleNestedRingTester(GeometryGraph<TCoordinate> graph)
        {
            _graph = graph;
        }

        public void Add(ILinearRing<TCoordinate> ring)
        {
            _rings.Add(ring);
        }

        public ICoordinate NestedPoint
        {
            get { return nestedPt; }
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