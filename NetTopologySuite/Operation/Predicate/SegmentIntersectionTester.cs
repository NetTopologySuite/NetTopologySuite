using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Predicate
{
    /// <summary>
    /// Tests if any line segments in two sets of CoordinateSequences intersect.
    /// Optimized for small geometry size.
    /// Short-circuited to return as soon an intersection is found.
    /// </summary>
    public class SegmentIntersectionTester<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        // for purposes of intersection testing, don't need to set precision model
        private readonly LineIntersector<TCoordinate> _li;

        private Boolean _hasIntersection;
        //private TCoordinate pt00;
        //private TCoordinate pt01;
        //private TCoordinate pt10;
        //private TCoordinate pt11;

        public SegmentIntersectionTester(IGeometryFactory<TCoordinate> geoFactory)
        {
            _li = CGAlgorithms<TCoordinate>.CreateRobustLineIntersector(geoFactory);
        }

        public Boolean HasIntersectionWithLineStrings(IEnumerable<TCoordinate> seq,
                                                      IEnumerable<ILineString<TCoordinate>> lines)
        {
            foreach (ILineString<TCoordinate> line in lines)
            {
                HasIntersection(seq, line.Coordinates);

                if (_hasIntersection)
                {
                    break;
                }
            }

            return _hasIntersection;
        }

        public Boolean HasIntersection(IEnumerable<TCoordinate> seq0, IEnumerable<TCoordinate> seq1)
        {
            foreach (Pair<TCoordinate> pair0 in Slice.GetOverlappingPairs(seq0))
            {
                TCoordinate pt00;
                TCoordinate pt01;

                pt00 = pair0.First;
                pt01 = pair0.Second;

                foreach (Pair<TCoordinate> pair1 in Slice.GetOverlappingPairs(seq1))
                {
                    TCoordinate pt10;
                    TCoordinate pt11;

                    pt10 = pair1.First;
                    pt11 = pair1.Second;

                    Intersection<TCoordinate> intersection = _li.ComputeIntersection(pt00, pt01, pt10, pt11);

                    if (intersection.HasIntersection)
                    {
                        _hasIntersection = true;
                        break;
                    }
                }

                if(_hasIntersection)
                {
                    break;
                }
            }

            return _hasIntersection;
        }
    }
}