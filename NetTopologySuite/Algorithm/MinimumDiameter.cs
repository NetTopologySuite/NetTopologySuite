using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes the minimum diameter of a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    /// <remarks>
    /// The minimum diameter is defined to be the
    /// width of the smallest band that contains the point,
    /// where a band is a strip of the plane defined
    /// by two parallel lines.
    /// This can be thought of as the smallest hole that the point can be
    /// moved through, with a single rotation.
    /// The first step in the algorithm is computing the convex hull of the Geometry.
    /// If the input Geometry is known to be convex, a hint can be supplied to
    /// avoid this computation.
    /// </remarks>
    public class MinimumDiameter<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometry<TCoordinate> _inputGeom;
        private readonly Boolean _isConvex;

        private LineSegment<TCoordinate>? _minBaseSeg;
        private TCoordinate _minWidthPt = default(TCoordinate);
        private Int32 _minPtIndex;
        private Double _minWidth = 0.0;

        /// <summary> 
        /// Compute a minimum diameter for a giver <see cref="Geometry{TCoordinate}"/>.
        /// </summary>
        /// <param name="inputGeom">a Geometry.</param>
        public MinimumDiameter(IGeometry<TCoordinate> inputGeom)
            : this(inputGeom, false) {}

        /// <summary> 
        /// Compute a minimum diameter for a giver <see cref="Geometry{TCoordinate}"/>,
        /// with a hint if the Geometry is convex
        /// (e.g. a convex Polygon or LinearRing,
        /// or a two-point LineString, or a Point).
        /// </summary>
        /// <param name="inputGeom">a Geometry which is convex.</param>
        /// <param name="isConvex"><see langword="true"/> if the input point is convex.</param>
        public MinimumDiameter(IGeometry<TCoordinate> inputGeom, Boolean isConvex)
        {
            _inputGeom = inputGeom;
            _isConvex = isConvex;
        }

        /// <summary> 
        /// Gets the length of the minimum diameter of the input Geometry.
        /// </summary>
        /// <returns>The length of the minimum diameter.</returns>
        public Double Length
        {
            get
            {
                computeMinimumDiameter();
                return _minWidth;
            }
        }

        /// <summary>
        /// Gets the <typeparamref name="TCoordinate"/> forming one end of the minimum diameter.
        /// </summary>
        /// <returns>A coordinate forming one end of the minimum diameter.</returns>
        public TCoordinate WidthCoordinate
        {
            get
            {
                computeMinimumDiameter();
                return _minWidthPt;
            }
        }

        /// <summary>
        /// Gets the segment forming the base of the minimum diameter.
        /// </summary>
        /// <returns>The segment forming the base of the minimum diameter.</returns>
        public ILineString<TCoordinate> SupportingSegment
        {
            get
            {
                computeMinimumDiameter();
                Debug.Assert(_minBaseSeg.HasValue);
                LineSegment<TCoordinate> seg = _minBaseSeg.Value;
                return _inputGeom.Factory.CreateLineString(seg.P0, seg.P1);
            }
        }

        /// <summary>
        /// Gets a <see cref="ILineString{TCoordinate}"/> which is a minimum diameter.
        /// </summary>
        /// <returns>
        /// A <see cref="ILineString{TCoordinate}"/> which is a minimum diameter.
        /// </returns>
        public ILineString<TCoordinate> Diameter
        {
            get
            {
                computeMinimumDiameter();

                // return empty linearRing if no minimum width calculated
                if (Coordinates<TCoordinate>.IsEmpty(_minWidthPt))
                {
                    return _inputGeom.Factory.CreateLineString();
                }

                ICoordinateFactory<TCoordinate> coordinateFactory = _inputGeom.Factory.CoordinateFactory;
                TCoordinate basePt = _minBaseSeg.Value.Project(_minWidthPt, coordinateFactory);
                return _inputGeom.Factory.CreateLineString(basePt, _minWidthPt);
            }
        }

        private void computeMinimumDiameter()
        {
            // check if computation is cached
            if (Coordinates<TCoordinate>.IsEmpty(_minWidthPt))
            {
                return;
            }

            if (_isConvex)
            {
                computeWidthConvex(_inputGeom);
            }
            else
            {
                //ConvexHull<TCoordinate> hull = new ConvexHull<TCoordinate>(_inputGeom);
                IGeometry<TCoordinate> convexGeom = _inputGeom.ConvexHull();
                computeWidthConvex(convexGeom);
            }
        }

        private void computeWidthConvex(IGeometry<TCoordinate> geom)
        {
            IEnumerable<TCoordinate> pts;

            if (geom is IPolygon<TCoordinate>)
            {
                IPolygon<TCoordinate> poly = geom as IPolygon<TCoordinate>;
                pts = poly.ExteriorRing.Coordinates;
            }
            else
            {
                pts = geom.Coordinates;
            }

            // special cases for lines or points or degenerate rings
            if (!Slice.CountGreaterThan(pts, 0))
            {
                _minWidth = 0.0;
                _minWidthPt = default(TCoordinate);
                _minBaseSeg = null;
            }
            else if (!Slice.CountGreaterThan(pts, 1))
            {
                _minWidth = 0.0;
                TCoordinate point = Slice.GetFirst(pts);
                _minWidthPt = point;
                _minBaseSeg = new LineSegment<TCoordinate>(point, point);
            }
            else if (!Slice.CountGreaterThan(pts, 3))
            {
                _minWidth = 0.0;
                Pair<TCoordinate> pair = Slice.GetPair(pts).Value;
                _minWidthPt = pair.First;
                _minBaseSeg = new LineSegment<TCoordinate>(pair);
            }
            else
            {
                computeConvexRingMinDiameter(pts);
            }
        }

        /// <summary> 
        /// Compute the width information for a ring of <c>Coordinate</c>s.
        /// Leaves the width information in the instance variables.
        /// </summary>
        private void computeConvexRingMinDiameter(IEnumerable<TCoordinate> pts)
        {
            // for each segment in the ring
            _minWidth = Double.MaxValue;
            Int32 currMaxIndex = 1;
            Int32 count = Slice.GetLength(pts);

            LineSegment<TCoordinate> seg = new LineSegment<TCoordinate>();

            // compute the max distance for all segments in the ring, and pick the minimum
            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(pts))
            {
                seg = new LineSegment<TCoordinate>(pair);
                currMaxIndex = findMaxPerpendicularDistance(pts, seg, currMaxIndex, count);
            }
        }

        private Int32 findMaxPerpendicularDistance(IEnumerable<TCoordinate> pts, LineSegment<TCoordinate> seg, Int32 startIndex, Int32 count)
        {
            Double maxPerpDistance = seg.DistancePerpendicular(Slice.GetAt(pts, startIndex));
            Double nextPerpDistance = maxPerpDistance;
            Int32 maxIndex = startIndex;
            Int32 nextIndex = maxIndex;

            while (nextPerpDistance >= maxPerpDistance)
            {
                maxPerpDistance = nextPerpDistance;
                maxIndex = nextIndex;

                nextIndex = computeNextIndex(pts, maxIndex, count);
                nextPerpDistance = seg.DistancePerpendicular(Slice.GetAt(pts, nextIndex));
            }

            // found maximum width for this segment - update global min dist if appropriate
            if (maxPerpDistance < _minWidth)
            {
                _minPtIndex = maxIndex;
                _minWidth = maxPerpDistance;
                _minWidthPt = Slice.GetAt(pts, _minPtIndex);
                _minBaseSeg = new LineSegment<TCoordinate>(seg);
            }

            return maxIndex;
        }

        private static Int32 computeNextIndex(IEnumerable<TCoordinate> pts, Int32 index, Int32 count)
        {
            index++;

            if (index >= count)
            {
                index = 0;
            }

            return index;
        }
    }
}