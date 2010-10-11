using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace NetTopologySuite.Operation.Predicate
{
    /// <summary>
    /// Optimized implementation of spatial predicate "intersects"
    /// for cases where the first <see cref="IGeometry{TCoordinate}"/> is a rectangle.    
    /// As a further optimization, this class can be used directly 
    /// to test many geometries against a single rectangle.
    /// </summary>
    public class RectangleIntersects<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /// <summary>     
        /// Crossover size at which brute-force intersection scanning
        /// is slower than indexed intersection detection.
        /// Must be determined empirically.  Should err on the
        /// safe side by making value smaller rather than larger.
        /// </summary>
        public const Int32 MaximumScanSegmentCount = 200;

        private readonly IPolygon<TCoordinate> _rectangle;
        private readonly IExtents<TCoordinate> _rectangleExtents;

        /// <summary>
        /// Create a new intersects computer for a rectangle.
        /// </summary>
        /// <param name="rectangle">A rectangular geometry.</param>
        public RectangleIntersects(IPolygon<TCoordinate> rectangle)
        {
            _rectangle = rectangle;
            _rectangleExtents = rectangle.Extents;
        }

        public static Boolean Intersects(IPolygon<TCoordinate> rectangle, IGeometry<TCoordinate> b)
        {
            RectangleIntersects<TCoordinate> rp = new RectangleIntersects<TCoordinate>(rectangle);
            return rp.Intersects(b);
        }

        public Boolean Intersects(IGeometry<TCoordinate> geom)
        {
            if (!_rectangleExtents.Intersects(geom.Extents))
            {
                return false;
            }

            // test envelope relationships
            EnvelopeIntersectsVisitor<TCoordinate> visitor
                = new EnvelopeIntersectsVisitor<TCoordinate>(_rectangleExtents);

            visitor.ApplyTo(geom);

            if (visitor.Intersects)
            {
                return true;
            }

            // test if any rectangle corner is contained in the target
            ContainsPointVisitor<TCoordinate> ecpVisitor = new ContainsPointVisitor<TCoordinate>(_rectangle);
            ecpVisitor.ApplyTo(geom);

            if (ecpVisitor.ContainsPoint)
            {
                return true;
            }

            // test if any lines intersect
            LineIntersectsVisitor<TCoordinate> liVisitor = new LineIntersectsVisitor<TCoordinate>(_rectangle);
            liVisitor.ApplyTo(geom);

            if (liVisitor.Intersects)
            {
                return true;
            }

            return false;
        }
    }

    internal class EnvelopeIntersectsVisitor<TCoordinate> : ShortCircuitedGeometryVisitor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IExtents<TCoordinate> _rectangleExtents;
        private Boolean _intersects;

        public EnvelopeIntersectsVisitor(IExtents<TCoordinate> rectEnv)
        {
            _rectangleExtents = rectEnv;
        }

        public Boolean Intersects
        {
            get { return _intersects; }
        }

        protected override void Visit(IGeometry<TCoordinate> element)
        {
            IExtents<TCoordinate> elementExtents = element.Extents;

            // disjoint
            if (!_rectangleExtents.Intersects(elementExtents))
            {
                return;
            }

            // fully contained - must intersect
            if (_rectangleExtents.Contains(elementExtents))
            {
                _intersects = true;
                return;
            }

            /*
            * Since the envelopes intersect and the test element is connected,
            * if its envelope is completely bisected by an edge of the rectangle
            * the element and the rectangle must touch.
            * (Note it is NOT possible to make this conclusion
            * if the test envelope is "on a corner" of the rectangle
            * envelope)
            */

            if (elementExtents.GetMin(Ordinates.X) >= _rectangleExtents.GetMin(Ordinates.X) &&
                elementExtents.GetMax(Ordinates.X) <= _rectangleExtents.GetMax(Ordinates.X))
            {
                _intersects = true;
                return;
            }

            if (elementExtents.GetMin(Ordinates.Y) >= _rectangleExtents.GetMin(Ordinates.Y) &&
                elementExtents.GetMax(Ordinates.Y) <= _rectangleExtents.GetMax(Ordinates.Y))
            {
                _intersects = true;
                return;
            }
        }

        protected override Boolean IsDone()
        {
            return _intersects;
        }
    }

    internal class ContainsPointVisitor<TCoordinate> : ShortCircuitedGeometryVisitor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IExtents<TCoordinate> _rectangleExtents;
        private readonly ICoordinateSequence<TCoordinate> _rectSeq;
        private Boolean _containsPoint;

        public ContainsPointVisitor(IPolygon<TCoordinate> rectangle)
        {
            _rectSeq = rectangle.ExteriorRing.Coordinates;
            _rectangleExtents = rectangle.Extents;
        }

        public Boolean ContainsPoint
        {
            get { return _containsPoint; }
        }

        protected override void Visit(IGeometry<TCoordinate> geom)
        {
            if (!(geom is IPolygon))
            {
                return;
            }

            IExtents<TCoordinate> elementExtents = geom.Extents;

            if (!_rectangleExtents.Intersects(elementExtents))
            {
                return;
            }

            for (Int32 i = 0; i < 4; i++)
            {
                // test each corner of rectangle for inclusion
                TCoordinate rectPt;

                rectPt = _rectSeq[i];

                if (!elementExtents.Contains(rectPt))
                {
                    continue;
                }

                // check rect point in poly (rect is known not to touch polygon at this point)
                if (SimplePointInAreaLocator<TCoordinate>.ContainsPointInPolygon(rectPt, geom as IPolygon<TCoordinate>))
                {
                    _containsPoint = true;
                    return;
                }
            }
        }

        protected override Boolean IsDone()
        {
            return _containsPoint;
        }
    }

    internal class LineIntersectsVisitor<TCoordinate> : ShortCircuitedGeometryVisitor<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly IPolygon<TCoordinate> _rectangle;
        private readonly IExtents<TCoordinate> _rectangleExtents;
        private readonly ICoordinateSequence<TCoordinate> _rectSeq;
        private Boolean _intersects;

        public LineIntersectsVisitor(IPolygon<TCoordinate> rectangle)
        {
            _rectangle = rectangle;
            _rectSeq = rectangle.ExteriorRing.Coordinates;
            _rectangleExtents = rectangle.Extents;
        }

        public Boolean Intersects
        {
            get { return _intersects; }
        }

        protected override void Visit(IGeometry<TCoordinate> geom)
        {
            IExtents<TCoordinate> elementEnv = geom.Extents;

            if (!_rectangleExtents.Intersects(elementEnv))
            {
                return;
            }

            // check if general relate algorithm should be used, since it's faster for large inputs
            if (geom.PointCount > RectangleIntersects<TCoordinate>.MaximumScanSegmentCount)
            {
                _intersects = _rectangle.Relate(geom).IsIntersects();
                return;
            }

            computeSegmentIntersection(geom);
        }

        private void computeSegmentIntersection(IGeometry<TCoordinate> geom)
        {
            // check segment intersection
            // get all lines from geom (e.g. if it's a multi-ring polygon)
            IEnumerable<ILineString<TCoordinate>> lines =
                GeometryFilter.Filter<ILineString<TCoordinate>, TCoordinate>(geom);

            SegmentIntersectionTester<TCoordinate> si = new SegmentIntersectionTester<TCoordinate>(geom.Factory);
            Boolean hasIntersection = si.HasIntersectionWithLineStrings(_rectSeq, lines);

            if (hasIntersection)
            {
                _intersects = true;
                return;
            }
        }

        protected override Boolean IsDone()
        {
            return _intersects;
        }
    }
}