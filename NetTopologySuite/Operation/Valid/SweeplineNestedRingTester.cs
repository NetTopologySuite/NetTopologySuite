using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Index.Sweepline;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Tests whether any of a set of <see cref="LinearRing{TCoordinate}"/>s are
    /// nested inside another ring in the set, using a <c>SweepLineIndex</c>
    /// index to speed up the comparisons.
    /// </summary>
    public class SweeplineNestedRingTester<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly GeometryGraph<TCoordinate> _graph; // used to find non-node vertices
        private readonly List<ILinearRing<TCoordinate>> _rings
            = new List<ILinearRing<TCoordinate>>();
        //private IExtents<TCoordinate> _totalExtents = new Extents<TCoordinate>();
        private SweepLineIndex _sweepLine;
        private TCoordinate _nestedPoint = default(TCoordinate);

        public SweeplineNestedRingTester(GeometryGraph<TCoordinate> graph)
        {
            _graph = graph;
        }

        public TCoordinate NestedPoint
        {
            get { return _nestedPoint; }
        }

        public void Add(ILinearRing<TCoordinate> ring)
        {
            _rings.Add(ring);
        }

        public Boolean IsNonNested()
        {
            buildIndex();
            OverlapAction action = new OverlapAction(this);
            _sweepLine.ComputeOverlaps(action);
            return action.IsNonNested;
        }

        private void buildIndex()
        {
            _sweepLine = new SweepLineIndex();
            foreach (ILinearRing<TCoordinate> ring in _rings)
            {
                //Extents env = (Extents) ring.EnvelopeInternal;
                IExtents<TCoordinate> extents = ring.Extents;
                SweepLineInterval sweepInt = new SweepLineInterval(
                    extents.GetMin(Ordinates.X), extents.GetMax(Ordinates.Y), ring);
                _sweepLine.Add(sweepInt);
            }
        }

        private Boolean isInside(ILinearRing<TCoordinate> innerRing, ILinearRing<TCoordinate> searchRing)
        {
            IEnumerable<TCoordinate> innerRingCoordinates = innerRing.Coordinates;
            IEnumerable<TCoordinate> searchRingCoordinates = searchRing.Coordinates;

            if (!innerRing.Extents.Intersects(searchRing.Extents))
            {
                return false;
            }

            TCoordinate innerRingPt = IsValidOp<TCoordinate>.FindPointNotNode(innerRingCoordinates, searchRing, _graph);
            Assert.IsTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
            Boolean isInside = CGAlgorithms<TCoordinate>.IsPointInRing(innerRingPt, searchRingCoordinates);

            if (isInside)
            {
                _nestedPoint = innerRingPt;
                return true;
            }

            return false;
        }

        public class OverlapAction : ISweepLineOverlapAction
        {
            private readonly SweeplineNestedRingTester<TCoordinate> _container = null;
            private Boolean _isNonNested = true;

            public Boolean IsNonNested
            {
                get { return _isNonNested; }
            }

            public OverlapAction(SweeplineNestedRingTester<TCoordinate> container)
            {
                _container = container;
            }

            public void Overlap(SweepLineInterval s0, SweepLineInterval s1)
            {
                ILinearRing<TCoordinate> innerRing = s0.Item as ILinearRing<TCoordinate>;
                ILinearRing<TCoordinate> searchRing = s1.Item as ILinearRing<TCoordinate>;

                if (innerRing == searchRing)
                {
                    return;
                }
                if (_container.isInside(innerRing, searchRing))
                {
                    _isNonNested = false;
                }
            }
        }
    }
}