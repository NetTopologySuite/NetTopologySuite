//#define UseWorker

using System;
using IList = System.Collections.Generic.IList<object>;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ConstrainedExecution;
#if UseWorker
using System.Threading;
#endif
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Overlay.Snap;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Provides an efficient method of unioning a collection of
    /// <see cref="IPolygonal"/> geometries.
    /// The geometries are indexed using a spatial index,
    /// and unioned recursively in index order.
    /// For geometries with a high degree of overlap,
    /// this has the effect of reducing the number of vertices
    /// early in the process, which increases speed
    /// and robustness.
    /// <para/>
    /// This algorithm is faster and more robust than
    /// the simple iterated approach of
    /// repeatedly unioning each polygon to a result geometry.
    /// <para/>
    /// The <tt>buffer(0)</tt> trick is sometimes faster, but can be less robust and
    /// can sometimes take a long time to complete.
    /// This is particularly the case where there is a high degree of overlap
    /// between the polygons.  In this case, <tt>buffer(0)</tt> is forced to compute
    /// with <i>all</i> line segments from the outset,
    /// whereas cascading can eliminate many segments
    /// at each stage of processing.
    /// The best situation for using <tt>buffer(0)</tt> is the trivial case
    /// where there is <i>no</i> overlap between the input geometries.
    /// However, this case is likely rare in practice.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso href="http://code.google.com/p/nettopologysuite/issues/detail?id=44"/>
    public class CascadedPolygonUnion
    {
        /// <summary>
        /// A union strategy that uses the classic NTS <see cref="SnapIfNeededOverlayOp"/>,
        /// and for polygonal geometries a robustness fallback using <c>Buffer(0)</c>.
        /// </summary>
        internal static readonly UnionStrategy ClassicUnion = new UnionStrategy(
            (g0, g1) =>
            {
                try
                {
                    return SnapIfNeededOverlayOp.Union(g0, g1);
                }
                catch (TopologyException ex)
                    // union-by-buffer only works for polygons
                    when (g0.Dimension == Dimension.Surface && g1.Dimension == Dimension.Surface)
                {
                    return UnionPolygonsByBuffer(g0, g1);
                }
            }, true);

        /// <summary>
        /// An alternative way of unioning polygonal geometries
        /// by using <c>Buffer(0)</c>.
        /// Only worth using if regular overlay union fails.
        /// </summary>
        /// <param name="g0">A polygonal geometry</param>
        /// <param name="g1">A polygonal geometry</param>
        /// <returns>The union of the geometries</returns>
        private static Geometry UnionPolygonsByBuffer(Geometry g0, Geometry g1)
        {
            //System.out.println("Unioning by buffer");
            var coll = g0.Factory.CreateGeometryCollection(new [] { g0, g1 });
            return coll.Buffer(0);
        }

        /// <summary>
        /// Computes the union of
        /// a collection of <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="Geometry"/>s.</param>
        /// <returns>The union of the <paramref name="polys"/></returns>
        public static Geometry Union(ICollection<Geometry> polys)
        {
            var op = new CascadedPolygonUnion(polys);
            return op.Union();
        }

        /// <summary>
        /// Computes the union of
        /// a collection of <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="Geometry"/>s.</param>
        /// <param name="unionStrategy">A strategy to perform the unioning.</param>
        /// <returns>The union of the <paramref name="polys"/></returns>
        public static Geometry Union(ICollection<Geometry> polys, UnionStrategy unionStrategy)
        {
            var op = new CascadedPolygonUnion(polys, unionStrategy);
            return op.Union();
        }

        private ICollection<Geometry> _inputPolys;
        private GeometryFactory _geomFactory;
        private readonly UnionStrategy _unionStrategy;

        private int _countRemainder = 0;
        private int _countInput = 0;
#if UseWorker
        private int _numThreadsStarted = 0;
#endif

        /// <summary>
        /// Creates a new instance to union
        /// the given collection of <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="Geometry"/>s</param>
        public CascadedPolygonUnion(ICollection<Geometry> polys)
            : this(polys, ClassicUnion)
        {
        }

        /// <summary>
        /// Creates a new instance to union
        /// the given collection of <see cref="Geometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="Geometry"/>s</param>
        /// <param name="unionStrategy"></param>
        public CascadedPolygonUnion(ICollection<Geometry> polys, UnionStrategy unionStrategy)
        {
            _inputPolys = polys;
            _unionStrategy = unionStrategy;
            // guard against null input
            if (_inputPolys == null)
                _inputPolys = new List<Geometry>();
            _countInput = _inputPolys.Count;
            _countRemainder = _countInput;
        }

        /// <summary>
        /// The effectiveness of the index is somewhat sensitive
        /// to the node capacity.
        /// Testing indicates that a smaller capacity is better.
        /// For an STRtree, 4 is probably a good number (since
        /// this produces 2x2 "squares").
        /// </summary>
        private const int STRtreeNodeCapacity = 4;

        private GeometryFactory Factory()
        {
            return _inputPolys.FirstOrDefault()?.Factory;
        }

        /// <summary>
        /// Computes the union of the input geometries.
        /// </summary>
        /// <returns>
        /// <remarks>
        /// This method discards the input geometries as they are processed.
        /// In many input cases this reduces the memory retained
        /// as the operation proceeds.
        /// Optimal memory usage is achieved
        /// by disposing of the original input collection
        /// before calling this method.
        /// </remarks>
        /// The union of the input geometries,
        /// or <c>null</c> if no input geometries were provided
        /// </returns>
        /// <exception cref="InvalidOperationException">if this method is called more than once</exception>
        public Geometry Union()
        {
            if (_inputPolys == null)
                throw new InvalidOperationException("Union() method cannot be called twice");
            if (_inputPolys.Count == 0)
                return null;
            _geomFactory = Factory();

            /*
             * A spatial index to organize the collection
             * into groups of close geometries.
             * This makes unioning more efficient, since vertices are more likely
             * to be eliminated on each round.
             */
            var index = new STRtree<object>(STRtreeNodeCapacity);
            foreach (var item in _inputPolys)
                index.Insert(item.EnvelopeInternal, item);

            // To avoiding holding memory remove references to the input geometries,
            _inputPolys = null;

            var itemTree = index.ItemsTree();
            var unionAll = UnionTree(itemTree);
            return unionAll;
        }

        private Geometry UnionTree(IList geomTree)
        {
            /*
             * Recursively unions all subtrees in the list into single geometries.
             * The result is a list of Geometrys only
             */
            var geoms = ReduceToGeometries(geomTree);
            var union = BinaryUnion(geoms);
            return union;
        }

        //========================================================
        // The following methods are for experimentation only
        /*
        private Geometry RepeatedUnion(IEnumerable<Geometry> geoms)
        {
            Geometry union = null;
            foreach (var g in geoms)
            {
                if (union == null)
                    union = g.Copy();
                else
                    union = _unionStrategy.Union(union, g);
            }

            return union;
        }
         */

        //=======================================

        /// <summary>
        /// Unions a list of geometries
        /// by treating the list as a flattened binary tree,
        /// and performing a cascaded union on the tree.
        /// </summary>
        /// <param name="geoms">The list of geometries to union</param>
        /// <returns>The union of the list</returns>
        private Geometry BinaryUnion(IList<Geometry> geoms)
        {
            return BinaryUnion(geoms, 0, geoms.Count);
        }

        /// <summary>
        /// Unions a section of a list using a recursive binary union on each half
        /// of the section.
        /// </summary>
        /// <param name="geoms">The list of geometries containing the section to union</param>
        /// <param name="start">The start index of the section</param>
        /// <param name="end">The index after the end of the section</param>
        /// <returns>The union of the list section</returns>
        private Geometry BinaryUnion(IList<Geometry> geoms, int start, int end/*, bool multiple = false*/)
        {
            Geometry g0, g1;
            if (end - start <= 1)
            {
                g0 = GetGeometry(geoms, start);
                return UnionSafe(g0, null);
            }

            if (end - start == 2)
                return UnionSafe(GetGeometry(geoms, start), GetGeometry(geoms, start + 1));

            // recurse on both halves of the list
            int mid = (end + start) / 2;

#if !UseWorker
            g0 = BinaryUnion(geoms, start, mid);
            g1 = BinaryUnion(geoms, mid, end);
            return UnionSafe(g0, g1);
#else
            //Geometry g0 = BinaryUnion(geoms, start, mid);
            var worker0 = new Worker(this, geoms, start, mid);
            var t0 = new Thread(worker0.Execute);
            _numThreadsStarted++;
            //Geometry g1 = BinaryUnion(geoms, mid, end);
            var worker1 = new Worker(this, geoms, mid, end);
            var t1 = new Thread(worker1.Execute);
            _numThreadsStarted++;

            t0.Start();
            t1.Start();
            t0.Join();
            t1.Join();

            //return UnionSafe(g0, g1);
            return UnionSafe(worker0.Geometry, worker1.Geometry);
#endif
        }

#if UseWorker
        class Worker
        {
            private readonly CascadedPolygonUnion _tool;
            private readonly IList<Geometry> _geoms;
            private readonly int _start;
            private readonly int _end;

            private Geometry _ret;

            protected internal Worker(CascadedPolygonUnion tool, IList<Geometry> geoms, int start, int end)
            {
                _tool = tool;
                _geoms = geoms;
                _start = start;
                _end = end;
            }

            internal void Execute()
            {
                _ret = _tool.BinaryUnion(_geoms, _start, _end);
            }

            public Geometry Geometry
            {
                get { return _ret; }
            }
        }
#endif

        /// <summary>
        /// Gets the element at a given list index, or
        /// null if the index is out of range.
        /// </summary>
        /// <param name="list">The list of geometries</param>
        /// <param name="index">The index</param>
        /// <returns>The geometry at the given index or
        /// <c>null</c> if the index is out of range</returns>
        private static Geometry GetGeometry(IList<Geometry> list, int index)
        {
            if (index >= list.Count)
                return null;
            return list[index];
        }

        /// <summary>
        /// Reduces a tree of geometries to a list of geometries
        /// by recursively unioning the subtrees in the list.
        /// </summary>
        /// <param name="geomTree">A tree-structured list of geometries</param>
        /// <returns>A list of Geometrys</returns>
        private IList<Geometry> ReduceToGeometries(IList geomTree)
        {
            var geoms = new List<Geometry>();
            foreach (object o in geomTree)
            {
                Geometry geom = null;
                if (o is IList)
                    geom = UnionTree((IList) o);
                else if (o is Geometry)
                    geom = (Geometry) o;
                geoms.Add(geom);
            }

            return geoms;
        }

        /// <summary>
        /// Computes the union of two geometries,
        /// either or both of which may be null.
        /// </summary>
        /// <param name="g0">A Geometry</param>
        /// <param name="g1">A Geometry</param>
        /// <returns>The union of the input(s) or
        /// <c>null</c> if both inputs are null</returns>
        private Geometry UnionSafe(Geometry g0, Geometry g1)
        {
            if (g0 == null && g1 == null)
                return null;
            if (g0 == null)
                return (Geometry) g1.Copy();
            if (g1 == null)
                return (Geometry) g0.Copy();

            _countRemainder--;
            Debug.WriteLine("Remainder: " + _countRemainder + " out of " + _countInput);
            Debug.WriteLine("Union: A: " + g0.NumPoints + " / B: " + g1.NumPoints + "  ---  ");

            var union = UnionActual(g0, g1, _unionStrategy);

            Debug.WriteLine(" Result: " + union.NumPoints);
            //if (TestBuilderProxy.isActive()) TestBuilderProxy.showIndicator(union);

            return union;

        }

        /// <summary>
        /// Encapsulates the actual unioning of two polygonal geometries.
        /// </summary>
        /// <param name="g0">A geometry to union</param>
        /// <param name="g1">A geometry to union</param>
        /// <param name="unionStrategy">A </param>
        /// <returns>The union of the inputs</returns>
        private static Geometry UnionActual(Geometry g0, Geometry g1, UnionStrategy unionStrategy)
        {
            var union = unionStrategy.Union(g0, g1);
            var unionPoly = RestrictToPolygons(union);
            return unionPoly;
        }

        /// <summary> Computes a <see cref="Geometry"/> containing only <see cref="IPolygonal"/> components.
        /// Extracts the <see cref="Polygon"/>s from the input
        /// and returns them as an appropriate <see cref="IPolygonal"/> geometry.
        /// <para/>
        /// If the input is already <tt>Polygonal</tt>, it is returned unchanged.
        /// <para/>
        /// A particular use case is to filter out non-polygonal components
        /// returned from an overlay operation.
        /// </summary>
        /// <param name="g">The geometry to filter</param>
        /// <returns>A polygonal geometry</returns>
        private static Geometry RestrictToPolygons(Geometry g)
        {
            if (g is IPolygonal)
                return g;

            var polygons = PolygonExtracter.GetPolygons(g);
            if (polygons.Count == 1)
                return polygons[0];
            var array = GeometryFactory.ToPolygonArray(polygons);
            return g.Factory.CreateMultiPolygon(array);
        }
    }
}
