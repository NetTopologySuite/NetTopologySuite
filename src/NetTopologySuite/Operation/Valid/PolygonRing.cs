using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// A ring of a polygon being analyzed for topological validity.
    /// The shell and hole rings of valid polygons touch only at discrete points.
    /// The "touch" relationship induces a graph over the set of rings.
    /// The interior of a valid polygon must be connected.
    /// This is the case if there is no "chain" of touching rings
    /// (which would partition off part of the interior).
    /// This is equivalent to the touch graph having no cycles.
    /// Thus the touch graph of a valid polygon is a forest - a set of disjoint trees.
    /// <para/>
    /// Also, in a valid polygon two rings can touch only at a single location,
    /// since otherwise they disconnect a portion of the interior between them.
    /// This is checked as the touches relation is built
    /// (so the touch relation representation for a polygon ring does not need to support
    /// more than one touch location for each adjacent ring).
    /// <para/>
    /// The cycle detection algorithm works for polygon rings which also contain self-touches
    /// (inverted shells and exverted holes).
    /// <para/>
    /// Polygons with no holes do not need to be checked for
    /// a connected interior, unless self-touches are allowed.
    /// <para/>
    /// The class also records the topology at self-touch nodes,
    /// to support checking if an invalid self-touch disconnects the polygon.
    /// </summary>
    /// <author>Martin Davis</author>
    class PolygonRing
    {
        /// <summary>
        /// Tests if a polygon ring represents a shell.
        /// </summary>
        /// <param name="polyRing">The ring to test (may be <c>null</c>)</param>
        /// <returns><c>true</c> if the ring represents a shell</returns>
        public static bool IsShell(PolygonRing polyRing)
        {
            if (polyRing == null) return true;
            return polyRing.IsShell();
        }

        /// <summary>
        /// Records a touch location between two rings,
        /// and checks if the rings already touch in a different location.
        /// </summary>
        /// <param name="ring0">A polygon ring</param>
        /// <param name="ring1">A polygon ring</param>
        /// <param name="pt">The location where they touch</param>
        /// <returns><c>true</c> if the polygons already touch</returns>
        public static bool AddTouch(PolygonRing ring0, PolygonRing ring1, Coordinate pt)
        {
            //--- skip if either polygon does not have holes
            if (ring0 == null || ring1 == null)
                return false;

            //--- only record touches within a polygon
            if (!ring0.IsSamePolygon(ring1)) return false;

            if (!ring0.IsOnlyTouch(ring1, pt)) return true;
            if (!ring1.IsOnlyTouch(ring0, pt)) return true;

            ring0.AddTouch(ring1, pt);
            ring1.AddTouch(ring0, pt);
            return false;
        }

        /// <summary>
        /// Finds a location (if any) where a chain of holes forms a cycle
        /// in the ring touch graph.
        /// The shell may form part of the chain as well.
        /// This indicates that a set of holes disconnects the interior of a polygon.
        /// </summary>
        /// <param name="polyRings">The list of rings to check</param>
        /// <returns>A vertex contained in a ring cycle or <c>null</c> if none is found.</returns>
        public static Coordinate FindHoleCycleLocation(IEnumerable<PolygonRing> polyRings)
        {
            foreach (var polyRing in polyRings)
            {
                if (!polyRing.IsInTouchSet)
                {
                    var holeCycleLoc = polyRing.FindHoleCycleLocation();
                    if (holeCycleLoc != null) return holeCycleLoc;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds a location of an interior self-touch in a list of rings,
        /// if one exists.
        /// This indicates that a self-touch disconnects the interior of a polygon,
        /// which is invalid.
        /// </summary>
        /// <param name="polyRings">The list of rings to check</param>
        /// <returns>The location of an interior self-touch node, or <c>null</c> if there are none</returns>
        public static Coordinate FindInteriorSelfNode(IEnumerable<PolygonRing> polyRings)
        {
            foreach (var polyRing in polyRings)
            {
                var interiorSelfNode = polyRing.FindInteriorSelfNode();
                if (interiorSelfNode != null)
                {
                    return interiorSelfNode;
                }
            }
            return null;
        }

        private readonly int _id;
        private readonly PolygonRing _shell;
        private readonly LinearRing _ring;

        /// <summary>
        /// The root of the touch graph tree containing this ring.
        /// Serves as the id for the graph partition induced by the touch relation.
        /// </summary>
        private PolygonRing _touchSetRoot;

        // lazily created

        /// <summary>
        /// The set of <see cref="PolygonRingTouch"/> links
        /// for this ring.
        /// The set of all touches in the rings of a polygon
        /// forms the polygon touch graph.
        /// This supports detecting touch cycles, which
        /// reveal the condition of a disconnected interior.
        /// <para/>
        /// Only a single touch is recorded between any two rings,
        /// since more than one touch between two rings
        /// indicates interior disconnection as well.
        /// </summary>
        private IDictionary<int, PolygonRingTouch> _touches;

        /// <summary>
        /// The set of self-nodes in this ring.
        /// This supports checking valid ring self-touch topology.
        /// </summary>
        private List<PolygonRingSelfNode> _selfNodes;

        /// <summary>
        /// Creates a ring for a polygon shell.
        /// </summary>
        /// <param name="ring">The polygon shell</param>
        public PolygonRing(LinearRing ring)
        {
            _ring = ring;
            _id = -1;
            _shell = this;
        }

        /// <summary>
        /// Creates a ring for a polygon hole.
        /// </summary>
        /// <param name="ring">The ring geometry</param>
        /// <param name="index">The index of the hole</param>
        /// <param name="shell">The parent polygon shell</param>
        public PolygonRing(LinearRing ring, int index, PolygonRing shell)
        {
            _ring = ring;
            _id = index;
            _shell = shell;
        }

        private bool IsSamePolygon(PolygonRing ring)
        {
            return _shell == ring._shell;
        }

        private bool IsShell()
        {
            return _shell == this;
        }

        private bool IsInTouchSet
        {
            get => _touchSetRoot != null;
        }

        private PolygonRing TouchSetRoot
        {
            get => _touchSetRoot;
            set => _touchSetRoot = value;
        }

        private bool HasTouches
        {
            get => _touches != null && _touches.Count != 0;
        }

        private ICollection<PolygonRingTouch> Touches
        {
            get => _touches.Values;
        }

        /// <summary>
        /// Adds a point where a <see cref="PolygonRing"/> touches another one.
        /// </summary>
        /// <param name="ring">The other <see cref="PolygonRing"/></param>
        /// <param name="pt">The touch location</param>
        private void AddTouch(PolygonRing ring, Coordinate pt)
        {
            if (_touches == null)
            {
                _touches = new Dictionary<int, PolygonRingTouch>();
            }

            if (!_touches.TryGetValue(ring._id, out _))
            {
                _touches.Add(ring._id, new PolygonRingTouch(ring, pt));
            }
        }

        /// <summary>
        /// Adds the node (intersection point)
        /// and the endpoints of the four adjacent segments.
        /// </summary>
        /// <param name="origin">The node</param>
        /// <param name="e00">The 1st position of the 1st edge</param>
        /// <param name="e01">The 2nd position of the 1st edge</param>
        /// <param name="e10">The 1st position of the 2nd edge</param>
        /// <param name="e11">The 2nd position of the 2nd edge</param>
        internal void AddSelfTouch(Coordinate origin, Coordinate e00, Coordinate e01, Coordinate e10, Coordinate e11)
        {
            if (_selfNodes == null)
            {
                _selfNodes = new List<PolygonRingSelfNode>();
            }
            _selfNodes.Add(new PolygonRingSelfNode(origin, e00, e01, e10, e11));
        }

        /// <summary>
        /// Tests if this ring touches a given ring at
        /// the single point specified.
        /// </summary>
        /// <param name="ring">The other polygon ring</param>
        /// <param name="pt">The touch point</param>
        /// <returns><c>true</c> if the rings touch only at the given point.</returns>
        private bool IsOnlyTouch(PolygonRing ring, Coordinate pt)
        {
            //--- no touches for this ring
            if (_touches == null) return true;
            //--- no touches for other ring
            if (!_touches.TryGetValue(ring._id, out var touch))
                return true;
            //--- the rings touch - check if point is the same
            return touch.IsAtLocation(pt);
        }

        /// <summary>
        /// Detects whether the subgraph of holes linked by touch to this ring
        /// contains a hole cycle.
        /// If no cycles are detected, the set of touching rings is a tree.
        /// The set is marked using this ring as the root.
        /// </summary>
        /// <returns>A vertex om a hole cycle or <c>null</c> if no cycle found</returns>
        private Coordinate FindHoleCycleLocation()
        {
            //--- the touch set including this ring is already processed
            if (IsInTouchSet) return null;

            //--- scan the touch set tree rooted at this ring
            // Assert: this.touchSetRoot is null
            var root = this;
            root.TouchSetRoot = root;

            if (!HasTouches)
                return null;

            var ringStack = new Queue<PolygonRingTouch>();
            Init(root, ringStack);

            while (ringStack.Count > 0)
            {
                var touch = ringStack.Dequeue();
                var touchCyclePt = ScanForHoleCycle(touch, root, ringStack);
                if (touchCyclePt != null)
                {
                    return touchCyclePt;
                }
            }
            return null;
        }

        private static void Init(PolygonRing root,
            Queue<PolygonRingTouch> touchStack)
        {
            foreach (var touch in root.Touches)
            {
                touch.Ring.TouchSetRoot = root;
                touchStack.Enqueue(touch);
            }
        }

        /// <summary>
        /// Scans for a hole cycle starting at a given touch.
        /// </summary>
        /// <param name="currentTouch">The touch to investigate</param>
        /// <param name="root">The root of the touch subgraph</param>
        /// <param name="touchQueue">The queue of rings to scan</param>
        private Coordinate ScanForHoleCycle(PolygonRingTouch currentTouch, PolygonRing root, Queue<PolygonRingTouch> touchQueue)
        {
            var ring = currentTouch.Ring;
            var currentPt = currentTouch.Coordinate;
            /*
             * Scan the touched rings
             * Either they form a hole cycle, or they are added to the touch set
             * and pushed on the stack for scanning
             */
            foreach(var touch in ring.Touches)
            {
                /*
                 * Don't check touches at the entry point
                 * to avoid trivial cycles.
                 * They will already be processed or on the stack
                 * from the previous ring (which touched
                 * all the rings at that point as well)
                 */
                if (currentPt.Equals2D(touch.Coordinate))
                    continue;

                /*
                 * Test if the touched ring has already been 
                 * reached via a different touch path in the tree.
                 * This is indicated by it already being marked as
                 * part of the touch set.
                 * This indicates a hole cycle has been found.
                 */
                var touchRing = touch.Ring;
                if (touchRing.TouchSetRoot == root)
                    return touch.Coordinate;

                touchRing.TouchSetRoot = root;
                touchQueue.Enqueue(touch);
            }
            return null;
        }

        /// <summary>
        /// Finds the location of an invalid interior self-touch in this ring,
        /// if one exists. 
        /// </summary>
        /// <returns>The location of an interior self-touch node, or <c>null</c> if there are none
        /// </returns>
        public Coordinate FindInteriorSelfNode()
        {
            if (_selfNodes == null) return null;

            /*
             * Determine if the ring interior is on the Right.
             * This is the case if the ring is a shell and is CW,
             * or is a hole and is CCW.
             */
            bool isCCW = Orientation.IsCCW(_ring.CoordinateSequence);
            bool isInteriorOnRight = IsShell() ^ isCCW;

            foreach (var selfNode in _selfNodes)
            {
                if (!selfNode.IsExterior(isInteriorOnRight))
                {
                    return selfNode.Coordinate;
                }
            }
            return null;
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return _ring.ToString();
        }

    }

    /// <summary>
    /// Records a point where a <see cref="PolygonRing"/> touches another one.
    /// This forms an edge in the induced ring touch graph.
    /// </summary>
    /// <author>Martin Davis</author>
    readonly struct PolygonRingTouch
    {
        private readonly PolygonRing _ring;
        private readonly Coordinate _touchPt;

        /// <summary>
        /// Creates an instance of this item
        /// </summary>
        /// <param name="ring">The polygon ring</param>
        /// <param name="pt">The touch position</param>
        public PolygonRingTouch(PolygonRing ring, Coordinate pt)
        {
            _ring = ring;
            _touchPt = pt;
        }

        public Coordinate Coordinate
        {
            get => _touchPt;
        }

        public PolygonRing Ring
        {
            get => _ring;
        }

        public bool IsAtLocation(Coordinate pt)
        {
            return _touchPt.Equals2D(pt);
        }
    }

    /// <summary>
    /// Represents a ring self-touch node, recording the node (intersection point)
    /// and the endpoints of the four adjacent segments.
    /// <para/>
    /// This is used to evaluate validity of self-touching nodes,
    /// when they are allowed.
    /// </summary>
    /// <author>Martin Davis</author>
    readonly struct PolygonRingSelfNode
    {
        private readonly Coordinate _nodePt;
        private readonly Coordinate _e00;
        private readonly Coordinate _e01;
        private readonly Coordinate _e10;
        //private Coordinate e11;

        /// <summary>
        /// Creates an instance of this point
        /// </summary>
        /// <param name="nodePt">The self touch position</param>
        /// <param name="e00">The 1st position of the 1st edge</param>
        /// <param name="e01">The 2nd position of the 1st edge</param>
        /// <param name="e10">The 1st position of the 2nd edge</param>
        /// <param name="e11">The 2nd position of the 2nd edge</param>
        public PolygonRingSelfNode(Coordinate nodePt,
            Coordinate e00, Coordinate e01,
            Coordinate e10, Coordinate e11)
        {
            _nodePt = nodePt;
            _e00 = e00;
            _e01 = e01;
            _e10 = e10;
            //e11 = e11;
        }

        /// <summary>
        /// Gets a value indicating the node point
        /// </summary>
        public Coordinate Coordinate
        {
            get => _nodePt;
        }

        /// <summary>
        /// Tests if a self-touch has the segments of each half of the touch
        /// lying in the exterior of a polygon.
        /// This is a valid self-touch.
        /// It applies to both shells and holes.
        /// Only one of the four possible cases needs to be tested,
        /// since the situation has full symmetry.
        /// </summary>
        /// <param name="isInteriorOnRight">A flag indicating if the interior is to the right of the parent ring</param>
        /// <returns><c>true</c> if the self-touch is on the exterior.</returns>
        public bool IsExterior(bool isInteriorOnRight)
        {
            /*
             * Note that either corner and either of the other edges could be used to test.
             * The situation is fully symmetrical.
             */
            bool isInteriorSeg = PolygonNode.IsInteriorSegment(_nodePt, _e00, _e01, _e10);
            bool isExterior = isInteriorOnRight ? !isInteriorSeg : isInteriorSeg;
            return isExterior;
        }
    }

}
