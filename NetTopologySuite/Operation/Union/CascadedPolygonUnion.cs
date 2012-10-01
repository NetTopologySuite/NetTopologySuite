using IList = System.Collections.Generic.IList<object>;
using System.Collections.Generic;
using System.Threading;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;
#if SILVERLIGHT
using ArrayList = System.Collections.Generic.List<object>;
#endif

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// <see href="http://code.google.com/p/nettopologysuite/issues/detail?id=44"/>
    /// </summary>
    public class CascadedPolygonUnion
    {
        private readonly ICollection<IGeometry> _inputPolys;
        private IGeometryFactory _geomFactory;

        public static IGeometry Union(ICollection<IGeometry> polys)
        {
            var op = new CascadedPolygonUnion(polys);
            return op.Union();
        }

        public CascadedPolygonUnion(ICollection<IGeometry> polys)
        {
            _inputPolys = polys;
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

        public IGeometry Union()
        {
            if (_inputPolys.Count == 0)
                return null;
            _geomFactory = Factory();

            /*
             * A spatial index to organize the collection
             * into groups of close geometries.
             * This makes unioning more efficient, since vertices are more likely 
             * to be eliminated on each round.
             */
            var index = new STRtree(StrtreeNodeCapacity);
            foreach (IGeometry item in _inputPolys)
                index.Insert(item.EnvelopeInternal, item);

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
            foreach (Geometry g in geoms)
            {
                if (union == null)
                     union = (IGeometry) g.Clone();
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
            IGeometry gColl = factory.CreateGeometryCollection(new[] { g0, g1 });
            var unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        //=======================================

        private IGeometry BinaryUnion(IList<IGeometry> geoms)
        {
            return BinaryUnion(geoms, 0, geoms.Count);
        }

        private IGeometry BinaryUnion(IList<IGeometry> geoms, int start, int end)
        {
            IGeometry g0, g1;
            if (end - start <= 1)
            {
                g0 = GetGeometry(geoms, start);
                return UnionSafe(g0, null);
            }
            if (end - start == 2)
                return UnionSafe(GetGeometry(geoms, start), GetGeometry(geoms, start + 1));

#if !WithWorker
            // recurse on both halves of the list
            var mid = (end + start) / 2;
            g0 = BinaryUnion(geoms, start, mid);
            g1 = BinaryUnion(geoms, mid, end);
            return UnionSafe(g0, g1);
#else
            //IGeometry g0 = BinaryUnion(geoms, start, mid);
            var worker0 = new Worker(this, geoms, start, mid);
            var t0 = new Thread(worker0.Execute);

            //IGeometry g1 = BinaryUnion(geoms, mid, end);
            var worker1 = new Worker(this, geoms, mid, end);
            var t1 = new Thread(worker1.Execute);

            t0.Start();
            t1.Start();
            t0.Join();
            t1.Join();

            //return UnionSafe(g0, g1);
            return UnionSafe(worker0.Geometry, worker1.Geometry);
#endif
            }

#if WithWorker
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

        private static IGeometry GetGeometry(IList<IGeometry> list, int index)
        {
            if (index >= list.Count)
                return null;
            return list[index];
        }

        private IList<IGeometry> ReduceToGeometries(IList geomTree)
        {
            IList<IGeometry> geoms = new List<IGeometry>();
            foreach (var o in geomTree)
            {
                IGeometry geom = null;
                if (o is IList)
                    geom = UnionTree((IList) o);
                else if (o is IGeometry)
                    geom = (Geometry) o;
                geoms.Add(geom);
            }
            return geoms;
        }

        private IGeometry UnionSafe(IGeometry g0, IGeometry g1)
        {
            if (g0 == null && g1 == null)
                return null;

            if (g0 == null)
                return (IGeometry)g1.Clone();
            if (g1 == null)
                return (IGeometry)g0.Clone();
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
            for (var i = 0; i < geom.NumGeometries; i++)
            {
                var elem = geom.GetGeometryN(i);
                if (elem.EnvelopeInternal.Intersects(env))
                    intersectingGeoms.Add(elem);
                else
                    disjointGeoms.Add(elem);
            }
            return _geomFactory.BuildGeometry(intersectingGeoms);
        }

        private static IGeometry UnionActual(IGeometry g0, IGeometry g1)
        {
           return g0.Union(g1);
        }
    }
}
