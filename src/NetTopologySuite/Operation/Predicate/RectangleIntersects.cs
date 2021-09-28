using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Predicate
{
    /// <summary>I
    /// Implementation of the <tt>Intersects</tt> spatial predicate
    /// optimized for the case where one <see cref="Geometry"/> is a rectangle.
    /// </summary>
    /// <remarks>
    /// This class works for all input geometries, including <see cref="GeometryCollection"/>s.
    /// <para/>
    /// As a further optimization, this class can be used in batch style
    /// to test many geometries against a single rectangle.
    /// </remarks>
    public class RectangleIntersects
    {
        /// <summary>
        /// Crossover size at which brute-force intersection scanning
        /// is slower than indexed intersection detection.
        /// Must be determined empirically.  Should err on the
        /// safe side by making value smaller rather than larger.
        /// </summary>
        public const int MaximumScanSegmentCount = 200;

        /// <summary>
        /// Tests whether a rectangle intersects a given geometry.
        /// </summary>
        /// <param name="rectangle">A rectangular polygon</param>
        /// <param name="b">A geometry of any kind</param>
        /// <returns><c>true</c> if the geometries intersect.</returns>
        public static bool Intersects(Polygon rectangle, Geometry b)
        {
            var rp = new RectangleIntersects(rectangle);
            return rp.Intersects(b);
        }

        private readonly Polygon _rectangle;
        private readonly Envelope _rectEnv;

        /// <summary>
        /// Create a new intersects computer for a rectangle.
        /// </summary>
        /// <param name="rectangle">A rectangular polygon.</param>
        public RectangleIntersects(Polygon rectangle)
        {
            _rectangle = rectangle;
            _rectEnv = rectangle.EnvelopeInternal;
        }

        /// <summary>
        /// Tests whether the given Geometry intersects the query rectangle.
        /// </summary>
        /// <param name="geom">The Geometry to test (may be of any type)</param>
        /// <returns><c>true</c> if an intersection must occur
        /// or <c>false</c> if no conclusion about intersection can be made</returns>
        public bool Intersects(Geometry geom)
        {
            if (!_rectEnv.Intersects(geom.EnvelopeInternal))
                return false;

            /*
             * Test if rectangle envelope intersects any component envelope.
             * This handles Point components as well
             */
            var visitor = new EnvelopeIntersectsVisitor(_rectEnv);
            visitor.ApplyTo(geom);
            if (visitor.Intersects)
                return true;

            /*
             * Test if any rectangle vertex is contained in the target geometry
             */
            var ecpVisitor = new GeometryContainsPointVisitor(_rectangle);
            ecpVisitor.ApplyTo(geom);
            if (ecpVisitor.ContainsPoint)
                return true;

            /*
             * Test if any target geometry line segment intersects the rectangle
             */
            var riVisitor = new RectangleIntersectsSegmentVisitor(_rectangle);
            riVisitor.ApplyTo(geom);
            return riVisitor.Intersects;
        }
    }

    /// <summary>
    /// Tests whether it can be concluded that a rectangle intersects a geometry,
    /// based on the relationship of the envelope(s) of the geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class EnvelopeIntersectsVisitor : ShortCircuitedGeometryVisitor
    {
        private readonly Envelope _rectEnv;

        /// <summary>
        /// Creates an instance of this class using the provided <c>Envelope</c>
        /// </summary>
        /// <param name="rectEnv">The query envelope</param>
        public EnvelopeIntersectsVisitor(Envelope rectEnv)
        {
            _rectEnv = rectEnv;
        }

        /// <summary>
        /// Reports whether it can be concluded that an intersection occurs,
        /// or whether further testing is required.
        /// </summary>
        /// <returns><c>true</c> if an intersection must occur <br/>
        /// or <c>false</c> if no conclusion about intersection can be made</returns>
        public bool Intersects { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        protected override void Visit(Geometry element)
        {
            var elementEnv = element.EnvelopeInternal;

            // disjoint => no intersection
            if (!_rectEnv.Intersects(elementEnv))
                return;

            // rectangle contains target env => must intersect
            if (_rectEnv.Contains(elementEnv))
            {
                Intersects = true;
                return;
            }
            /*
             * Since the envelopes intersect and the test element is connected,
             * if its envelope is completely bisected by an edge of the rectangle
             * the element and the rectangle must touch. (This is basically an application of
             * the Jordan Curve Theorem). The alternative situation is that the test
             * envelope is "on a corner" of the rectangle envelope, i.e. is not
             * completely bisected. In this case it is not possible to make a conclusion
             */
            if (elementEnv.MinX >= _rectEnv.MinX && elementEnv.MaxX <= _rectEnv.MaxX)
            {
                Intersects = true;
                return;
            }
            if (elementEnv.MinY >= _rectEnv.MinY && elementEnv.MaxY <= _rectEnv.MaxY)
            {
                Intersects = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override bool IsDone()
        {
            return Intersects;
        }
    }

    /// <summary>
    /// A visitor which tests whether it can be
    /// concluded that a geometry contains a vertex of
    /// a query geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class GeometryContainsPointVisitor : ShortCircuitedGeometryVisitor
    {
        private readonly CoordinateSequence _rectSeq;
        private readonly Envelope _rectEnv;

        /// <summary>
        ///
        /// </summary>
        /// <param name="rectangle"></param>
        public GeometryContainsPointVisitor(Polygon rectangle)
        {
            _rectSeq = rectangle.ExteriorRing.CoordinateSequence;
            _rectEnv = rectangle.EnvelopeInternal;
        }

        /// <summary>
        /// Gets a value indicating whether it can be concluded that a corner point of the rectangle is
        /// contained in the geometry, or whether further testing is required.
        /// </summary>
        /// <returns><c>true</c> if a corner point is contained
        /// or <c>false</c> if no conclusion about intersection can be made
        /// </returns>
        public bool ContainsPoint { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        protected override void Visit(Geometry geom)
        {
            if (!(geom is Polygon))
                return;

            var elementEnv = geom.EnvelopeInternal;
            if (! _rectEnv.Intersects(elementEnv))
                return;

            // test each corner of rectangle for inclusion
            var rectPt = _rectSeq.CreateCoordinate();
            for (int i = 0; i < 4; i++)
            {
                _rectSeq.GetCoordinate(i, rectPt);
                if (!elementEnv.Contains(rectPt))
                    continue;

                // check rect point in poly (rect is known not to touch polygon at this point)
                if (SimplePointInAreaLocator.ContainsPointInPolygon(rectPt, (Polygon) geom))
                {
                    ContainsPoint = true;
                    return;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        protected override bool IsDone()
        {
            return ContainsPoint;
        }
    }

    /// <summary>
    /// A visitor to test for intersection between the query rectangle and the line segments of the geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class RectangleIntersectsSegmentVisitor : ShortCircuitedGeometryVisitor
    {
        private readonly Envelope _rectEnv;
        private readonly RectangleLineIntersector _rectIntersector;

        private readonly Coordinate _p0 = new Coordinate();
        private readonly Coordinate _p1 = new Coordinate();

        /// <summary>
        /// Creates a visitor for checking rectangle intersection with segments
        /// </summary>
        /// <param name="rectangle">the query rectangle </param>
        public RectangleIntersectsSegmentVisitor(Polygon rectangle)
        {
            _rectEnv = rectangle.EnvelopeInternal;
            _rectIntersector = new RectangleLineIntersector(_rectEnv);
        }

        /// <summary>Reports whether any segment intersection exists.</summary>
        /// <returns>true if a segment intersection exists or
        /// false if no segment intersection exists</returns>
        public bool Intersects { get; private set; }

        protected override void Visit(Geometry geom)
        {
            /*
             * It may be the case that the rectangle and the
             * envelope of the geometry component are disjoint,
             * so it is worth checking this simple condition.
             */
            var elementEnv = geom.EnvelopeInternal;
            if (!_rectEnv.Intersects(elementEnv))
                return;

            // check segment intersections
            // get all lines from geometry component
            // (there may be more than one if it's a multi-ring polygon)
            var lines = LinearComponentExtracter.GetLines(geom);
            CheckIntersectionWithLineStrings(lines);
        }

        private void CheckIntersectionWithLineStrings(IEnumerable<Geometry> lines)
        {
            foreach (LineString testLine in lines)
            {
                CheckIntersectionWithSegments(testLine);
                if (Intersects)
                    return;
            }
        }

        private void CheckIntersectionWithSegments(LineString testLine)
        {
            var seq1 = testLine.CoordinateSequence;
            for (int j = 1; j < seq1.Count; j++)
            {
                _p0.X = seq1.GetX(j - 1);
                _p0.Y = seq1.GetY(j - 1);

                _p1.X = seq1.GetX(j);
                _p1.Y = seq1.GetY(j);

                if (!_rectIntersector.Intersects(_p0, _p1)) continue;
                Intersects = true;
                return;
            }
        }

        protected override bool IsDone()
        {
            return Intersects;
        }
    }
}
