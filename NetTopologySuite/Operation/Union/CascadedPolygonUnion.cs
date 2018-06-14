//#define UseWorker

using System;
using IList = System.Collections.Generic.IList<object>;
using System.Collections.Generic;
#if UseWorker
using System.Threading;
#endif
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;

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
        private ICollection<IGeometry> _inputPolys;
        private IGeometryFactory _geomFactory;
#if UseWorker
        private int _numThreadsStarted = 0;
#endif
        /// <summary>
        /// Computes the union of
        /// a collection of <see cref="IGeometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="IGeometry"/>s.</param>
        /// <returns></returns>
        public static IGeometry Union(ICollection<IGeometry> polys)
        {
            var op = new CascadedPolygonUnion(polys);
            return op.Union();
        }

        /// <summary>
        /// Creates a new instance to union
        /// the given collection of <see cref="IGeometry"/>s.
        /// </summary>
        /// <param name="polys">A collection of <see cref="IPolygonal"/> <see cref="IGeometry"/>s</param>
        public CascadedPolygonUnion(ICollection<IGeometry> polys)
        {
            // guard against null input
            _inputPolys = polys ?? new List<IGeometry>();
        }

        /**
         * The effectiveness of the index is somewhat sensitive
         * to the node capacity.
         * Testing indicates that a smaller capacity is better.
         * For an STRtree, 4 is probably a good number (since
         * this produces 2x2 "squares").
         */
        private const int StrtreeNodeCapacity = 4;

        private IGeometryFactory Factory()
        {
            var geomenumerator = _inputPolys.GetEnumerator();
            geomenumerator.MoveNext();
            return geomenumerator.Current.Factory;
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
        public IGeometry Union()
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
            var index = new STRtree<object>(StrtreeNodeCapacity);
            foreach (var item in _inputPolys)
                index.Insert(item.EnvelopeInternal, item);

            // To avoiding holding memory remove references to the input geometries,
            _inputPolys = null;

            var itemTree = index.ItemsTree();
            var unionAll = UnionTree(itemTree);
            return unionAll;
        }

        private IGeometry UnionTree(IList geomTree)
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

        private IGeometry RepeatedUnion(IEnumerable<IGeometry> geoms)
        {
            IGeometry union = null;
            foreach (var g in geoms)
            {
                if (union == null)
                    union = (IGeometry)g.Copy();
                else union = union.Union(g);
            }
            return union;
        }

        private IGeometry BufferUnion(ICollection<IGeometry> geoms)
        {
            var factory = Factory();
            var gColl = factory.BuildGeometry(geoms);
            var unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        private IGeometry BufferUnion(IGeometry g0, IGeometry g1)
        {
            var factory = g0.Factory;
            var gColl = factory.CreateGeometryCollection(new[] { g0, g1 });
            var unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        //=======================================

        /// <summary>
        /// Unions a list of geometries
        /// by treating the list as a flattened binary tree,
        /// and performing a cascaded union on the tree.
        /// </summary>
        /// <param name="geoms">The list of geometries to union</param>
        /// <returns>The union of the list</returns>
        private IGeometry BinaryUnion(IList<IGeometry> geoms)
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
        private IGeometry BinaryUnion(IList<IGeometry> geoms, int start, int end, bool multiple = false)
        {
            IGeometry g0, g1;
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
            //IGeometry g0 = BinaryUnion(geoms, start, mid);
            var worker0 = new Worker(this, geoms, start, mid);
            var t0 = new Thread(worker0.Execute);
            _numThreadsStarted++;
            //IGeometry g1 = BinaryUnion(geoms, mid, end);
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
            private readonly IList<IGeometry> _geoms;
            private readonly int _start;
            private readonly int _end;

            private IGeometry _ret;

            protected internal Worker(CascadedPolygonUnion tool, IList<IGeometry> geoms, int start, int end)
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

            public IGeometry Geometry
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
        /// <value>null</value> if the index is out of range</returns>
        private static IGeometry GetGeometry(IList<IGeometry> list, int index)
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
        private IList<IGeometry> ReduceToGeometries(IList geomTree)
        {
            var geoms = new List<IGeometry>();
            foreach (object o in geomTree)
            {
                IGeometry geom = null;
                if (o is IList)
                    geom = UnionTree((IList)o);
                else if (o is IGeometry)
                    geom = (IGeometry)o;
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
        /// <value>null</value> if both inputs are null</returns>
        private IGeometry UnionSafe(IGeometry g0, IGeometry g1)
        {
            if (g0 == null && g1 == null)
                return null;
            if (g0 == null)
                return (IGeometry)g1.Copy();
            if (g1 == null)
                return (IGeometry)g0.Copy();
            return UnionOptimized(g0, g1);
        }

        private IGeometry UnionOptimized(IGeometry g0, IGeometry g1)
        {
            var g0Env = g0.EnvelopeInternal;
            var g1Env = g1.EnvelopeInternal;
            if (!g0Env.Intersects(g1Env))
            {
                var combo = GeometryCombiner.Combine(g0, g1);
                return combo;
            }
            if (g0.NumGeometries <= 1 && g1.NumGeometries <= 1)
                return UnionActual(g0, g1);

            var commonEnv = g0Env.Intersection(g1Env);
            return UnionUsingEnvelopeIntersection(g0, g1, commonEnv);
        }

        /// <summary>
        /// Unions two polygonal geometries, restricting computation
        /// to the envelope intersection where possible.
        /// The case of MultiPolygons is optimized to union only
        /// the polygons which lie in the intersection of the two geometry's envelopes.
        /// Polygons outside this region can simply be combined with the union result,
        /// which is potentially much faster.
        /// This case is likely to occur often during cascaded union, and may also
        /// occur in real world data (such as unioning data for parcels on different street blocks).
        /// </summary>
        /// <param name="g0">A polygonal geometry</param>
        /// <param name="g1">A polygonal geometry</param>
        /// <param name="common">The intersection of the envelopes of the inputs</param>
        /// <returns>The union of the inputs</returns>
        private IGeometry UnionUsingEnvelopeIntersection(IGeometry g0, IGeometry g1, Envelope common)
        {
            var disjointPolys = new List<IGeometry>();
            var g0Int = ExtractByEnvelope(common, g0, disjointPolys);
            var g1Int = ExtractByEnvelope(common, g1, disjointPolys);
            var union = UnionActual(g0Int, g1Int);
            disjointPolys.Add(union);
            var overallUnion = GeometryCombiner.Combine(disjointPolys);
            return overallUnion;
        }

        private IGeometry ExtractByEnvelope(Envelope env, IGeometry geom, IList<IGeometry> disjointGeoms)
        {
            var intersectingGeoms = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var elem = geom.GetGeometryN(i);
                if (elem.EnvelopeInternal.Intersects(env))
                    intersectingGeoms.Add(elem);
                else disjointGeoms.Add(elem);
            }
            return _geomFactory.BuildGeometry(intersectingGeoms);
        }

        /// <summary>
        /// Encapsulates the actual unioning of two polygonal geometries.
        /// </summary>
        private static IGeometry UnionActual(IGeometry g0, IGeometry g1)
        {
            return RestrictToPolygons(g0.Union(g1));
        }

        /// <summary> Computes a <see cref="IGeometry"/> containing only <see cref="IPolygonal"/> components.
        /// Extracts the <see cref="IPolygon"/>s from the input
        /// and returns them as an appropriate <see cref="IPolygonal"/> geometry.
        /// <para/>
        /// If the input is already <tt>Polygonal</tt>, it is returned unchanged.
        /// <para/>
        /// A particular use case is to filter out non-polygonal components
        /// returned from an overlay operation.
        /// </summary>
        /// <param name="g">The geometry to filter</param>
        /// <returns>A polygonal geometry</returns>
        private static IGeometry RestrictToPolygons(IGeometry g)
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
