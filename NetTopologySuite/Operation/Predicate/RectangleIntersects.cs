using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Predicate
{
    /// <summary>
    /// Optimized implementation of spatial predicate "intersects"
    /// for cases where the first {@link Geometry} is a rectangle.    
    /// As a further optimization,
    /// this class can be used directly to test many geometries against a single
    /// rectangle.
    /// </summary>
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
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Intersects(IPolygon rectangle, IGeometry b)
        {
            RectangleIntersects rp = new RectangleIntersects(rectangle);
            return rp.Intersects(b);
        }

        private IPolygon rectangle;
        private IEnvelope rectEnv;

        /// <summary>
        /// Create a new intersects computer for a rectangle.
        /// </summary>
        /// <param name="rectangle">A rectangular geometry.</param>
        public RectangleIntersects(IPolygon rectangle) 
        {
            this.rectangle = rectangle;
            rectEnv = rectangle.EnvelopeInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public bool Intersects(IGeometry geom)
        {
            if (!rectEnv.Intersects(geom.EnvelopeInternal))
                return false;
            // test envelope relationships
            EnvelopeIntersectsVisitor visitor = new EnvelopeIntersectsVisitor(rectEnv);
            visitor.ApplyTo(geom);
            if (visitor.Intersects())
                return true;

            // test if any rectangle corner is contained in the target
            ContainsPointVisitor ecpVisitor = new ContainsPointVisitor(rectangle);
            ecpVisitor.ApplyTo(geom);
            if (ecpVisitor.ContainsPoint())
                return true;

            // test if any lines intersect
            LineIntersectsVisitor liVisitor = new LineIntersectsVisitor(rectangle);
            liVisitor.ApplyTo(geom);
            if (liVisitor.Intersects())
                return true;

            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class EnvelopeIntersectsVisitor : ShortCircuitedGeometryVisitor
    {
        private IEnvelope rectEnv;
        private bool intersects = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectEnv"></param>
        public EnvelopeIntersectsVisitor(IEnvelope rectEnv)
        {
            this.rectEnv = rectEnv;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Intersects() 
        { 
            return intersects; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        protected override void Visit(IGeometry element)
        {
            IEnvelope elementEnv = element.EnvelopeInternal;
            // disjoint
            if (!rectEnv.Intersects(elementEnv))
                return;            
            // fully contained - must intersect
            if (rectEnv.Contains(elementEnv)) 
            {
                intersects = true;
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
            if (elementEnv.MinX >= rectEnv.MinX && elementEnv.MaxX <= rectEnv.MaxX)
            {
                intersects = true;
                return;
            }
            if (elementEnv.MinY >= rectEnv.MinY && elementEnv.MaxY <= rectEnv.MaxY)
            {
                intersects = true;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsDone() 
        {
            return intersects == true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class ContainsPointVisitor : ShortCircuitedGeometryVisitor
        {
        private ICoordinateSequence rectSeq;
        private IEnvelope rectEnv;
        private bool containsPoint = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        public ContainsPointVisitor(IPolygon rectangle)
        {
            this.rectSeq = rectangle.ExteriorRing.CoordinateSequence;
            rectEnv = rectangle.EnvelopeInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ContainsPoint() { return containsPoint; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        protected override void Visit(IGeometry geom)
        {
            if (!(geom is IPolygon))
                return;
            IEnvelope elementEnv = geom.EnvelopeInternal;
            if (! rectEnv.Intersects(elementEnv))
                return;
            // test each corner of rectangle for inclusion
            ICoordinate rectPt = new Coordinate();
            for (int i = 0; i < 4; i++) 
            {
                rectSeq.GetCoordinate(i, rectPt);
                if (!elementEnv.Contains(rectPt))
                    continue;
                // check rect point in poly (rect is known not to touch polygon at this point)
                if (SimplePointInAreaLocator.ContainsPointInPolygon(rectPt, (IPolygon) geom)) 
                {
                    containsPoint = true;
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
            return containsPoint == true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class LineIntersectsVisitor : ShortCircuitedGeometryVisitor
    {
        private IPolygon rectangle;
        private ICoordinateSequence rectSeq;
        private IEnvelope rectEnv;
        private bool intersects = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectangle"></param>
        public LineIntersectsVisitor(IPolygon rectangle)
        {
            this.rectangle = rectangle;
            this.rectSeq = rectangle.ExteriorRing.CoordinateSequence;
            rectEnv = rectangle.EnvelopeInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Intersects() 
        { 
            return intersects; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        protected override void Visit(IGeometry geom)
        {
            IEnvelope elementEnv = geom.EnvelopeInternal;
            if (!rectEnv.Intersects(elementEnv))
                return;
            // check if general relate algorithm should be used, since it's faster for large inputs
            if (geom.NumPoints > RectangleIntersects.MaximumScanSegmentCount) 
            {
                intersects = rectangle.Relate(geom).IsIntersects();
                return;
            }
            ComputeSegmentIntersection(geom);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        private void ComputeSegmentIntersection(IGeometry geom)
        {
            // check segment intersection
            // get all lines from geom (e.g. if it's a multi-ring polygon)
            IList lines = LinearComponentExtracter.GetLines(geom);
            SegmentIntersectionTester si = new SegmentIntersectionTester();
            bool hasIntersection = si.HasIntersectionWithLineStrings(rectSeq, lines);
            if (hasIntersection) 
            {
                intersects = true;
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool IsDone() 
        {
            return intersects == true;
        }
    }
}
