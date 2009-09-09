#define useWorker
using System;
using System.Collections;
using System.Collections.Generic;
#if useWorker
using System.Threading;
#endif
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using GisSharpBlog.NetTopologySuite.Index.Strtree;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Union
{
    /// <summary>
    /// <see href="http://code.google.com/p/nettopologysuite/issues/detail?id=44"/>
    /// </summary>
    public class CascadedPolygonUnion<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private const int StrTreeNodeCapacity = 4;
        private readonly IEnumerable<IPolygon<TCoordinate>> _inputPolys;
        private IGeometryFactory<TCoordinate> _geomFactory;

        public CascadedPolygonUnion(IEnumerable<IPolygon<TCoordinate>> polys)
        {
            _inputPolys = polys;
        }

        public static IGeometry<TCoordinate> Union(IEnumerable<IPolygon<TCoordinate>> polys)
        {
            CascadedPolygonUnion<TCoordinate> op = new CascadedPolygonUnion<TCoordinate>(polys);
            return op.Union();
        }

        /**
         * The effectiveness of the index is somewhat sensitive
         * to the node capacity.  
         * Testing indicates that a smaller capacity is better.
         * For an STRtree, 4 is probably a good number (since
         * this produces 2x2 "squares").
         */

        private IGeometryFactory<TCoordinate> Factory()
        {
            if ( _inputPolys == null || !Slice.CountGreaterThan(_inputPolys, 0))
                return null;

            return Slice.GetFirst(_inputPolys).Factory;
        }

        public IGeometry<TCoordinate> Union()
        {
            _geomFactory = Factory();
            if (_geomFactory == null) return null;
            /**
             * A spatial index to organize the collection
             * into groups of close geometries.
             * This makes unioning more efficient, since vertices are more likely
             * to be eliminated on each round.
             */
            StrTree<TCoordinate, IGeometry<TCoordinate>> index =
                new StrTree<TCoordinate, IGeometry<TCoordinate>>(_geomFactory, StrTreeNodeCapacity);
            foreach (IPolygon<TCoordinate> item in _inputPolys)
            {
                if (!item.IsEmpty)
                    index.Insert(item);
            }
            IList itemTree = index.ItemsTree();
            IGeometry<TCoordinate> unionAll = UnionTree(itemTree);
            return unionAll;
        }

        private IGeometry<TCoordinate> UnionTree(IList geomTree)
        {
            /**
             * Recursively unions all subtrees in the list into single geometries.
             * The result is a list of Geometrys only
             */
            IList geoms = ReduceToGeometries(geomTree);
            IGeometry<TCoordinate> union = BinaryUnion(geoms);

            return union;
        }

        //========================================================
        // The following methods are for experimentation only

        private IGeometry<TCoordinate> RepeatedUnion(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            IGeometry<TCoordinate> union = null;
            foreach (IGeometry<TCoordinate> g in geoms)
            {
                if (union == null)
                    union = g.Clone();
                else
                    union = union.Union(g);
            }
            return union;
        }

        private IGeometry<TCoordinate> BufferUnion(IEnumerable<IGeometry<TCoordinate>> geoms)
        {
            IGeometryFactory<TCoordinate> factory = Factory();
            IGeometry<TCoordinate> geomColl = factory.BuildGeometry(geoms);
            IGeometry<TCoordinate> unionAll = geomColl.Buffer(0.0d);
            return unionAll;
        }

        private IGeometry<TCoordinate> BufferUnion(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            List<IGeometry<TCoordinate>> lst = new List<IGeometry<TCoordinate>>();
            lst.Add(g0);
            lst.Add(g1);
            return BufferUnion(lst);
        }

        //=======================================

        private IGeometry<TCoordinate> BinaryUnion(IList geoms)
        {
            return BinaryUnion(geoms, 0, geoms.Count);
        }

        private IGeometry<TCoordinate> BinaryUnion(IList geoms, int start, int end)
        {
            IGeometry<TCoordinate> g0;
            if (end - start <= 1)
            {
                g0 = GetGeometry(geoms, start);
                return UnionSafe(g0, null);
            }
            if (end - start == 2)
                return UnionSafe(GetGeometry(geoms, start), GetGeometry(geoms, start + 1));

            // recurse on both halves of the list
            int mid = (end + start) / 2;

#if useWorker
            Worker worker0 = new Worker(this, geoms, start, mid);
            Thread t0 = new Thread(worker0.Execute);

            Worker worker1 = new Worker(this, geoms, mid, end);
            Thread t1 = new Thread(worker1.Execute);

            t0.Start();
            t1.Start();
            t0.Join();
            t1.Join();
            return UnionSafe(worker0.Geometry, worker1.Geometry);
#else
            g0 = BinaryUnion(geoms, start, mid);
            IGeometry<TCoordinate> g1 = BinaryUnion(geoms, mid, end);
            return UnionSafe(g0, g1);
#endif
        }

        private static IGeometry<TCoordinate> GetGeometry(IList list, int index)
        {
            if (index >= list.Count)
                return null;
            return list[index] as IGeometry<TCoordinate>;
        }

        private IList ReduceToGeometries(IList geomTree)
        {
            IList geoms = new ArrayList();
            foreach (Object o in geomTree)
            {
                IGeometry<TCoordinate> geom;
                if (o is IList)
                    geom = UnionTree(o as IList);
                else if (o is IPolygonal<TCoordinate>)
                    geom = (IGeometry<TCoordinate>)o;
                else
                    throw new ArgumentException("Invalid Geometry");
                geoms.Add(geom);
            }
            return geoms;
        }

        private IGeometry<TCoordinate> UnionSafe(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            if (g0 == null && g1 == null)
                return null;


            if (g0 == null)
                return g1.Clone();
            if (g1 == null)
                return g0.Clone();

            return UnionOptimized(g0, g1);
        }

        private IGeometry<TCoordinate> UnionOptimized(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            IExtents<TCoordinate> g0Env = g0.Extents;
            IExtents<TCoordinate> g1Env = g1.Extents;
            if (!g0Env.Intersects(g1Env))
            {
                IGeometry<TCoordinate> combo = GeometryCombiner<TCoordinate>.Combine(g0, g1);
                return combo;
            }

            if (g0 is IHasGeometryComponents<TCoordinate> ||
                g1 is IHasGeometryComponents<TCoordinate>)
            {
                IExtents<TCoordinate> commonEnv = g0Env.Intersection(g1Env);
                return UnionUsingExtentsIntersection(g0, g1, commonEnv);
            }

            return UnionActual(g0, g1);
        }

        private IGeometry<TCoordinate> UnionUsingExtentsIntersection(IGeometry<TCoordinate> g0,
                                                                     IGeometry<TCoordinate> g1,
                                                                     IExtents<TCoordinate> common)
        {
            List<IGeometry<TCoordinate>> disjointPolys = new List<IGeometry<TCoordinate>>();

            IGeometry<TCoordinate> g0Int = ExtractByExtent(common, g0, disjointPolys);
            IGeometry<TCoordinate> g1Int = ExtractByExtent(common, g1, disjointPolys);

            IGeometry<TCoordinate> union = UnionActual(g0Int, g1Int);

            disjointPolys.Add(union);
            IGeometry<TCoordinate> overallUnion = GeometryCombiner<TCoordinate>.Combine(disjointPolys);

            return overallUnion;
        }

        private IEnumerable<IGeometry<TCoordinate>> EnumerateGeometriesElements(IGeometry<TCoordinate> geometry)
        {
            IGeometryCollection<TCoordinate> geomColl = geometry as IGeometryCollection<TCoordinate>;
            if (geomColl != null)
                foreach (IGeometry<TCoordinate> item in geomColl)
                    yield return item;
            else
                yield return geometry;
        }

        private IGeometry<TCoordinate> ExtractByExtent(IExtents<TCoordinate> env, IGeometry<TCoordinate> geom,
                                                       IList<IGeometry<TCoordinate>> disjointGeoms)
        {
            List<IGeometry<TCoordinate>> intersectingGeoms = new List<IGeometry<TCoordinate>>();
            foreach (IGeometry<TCoordinate> elem in EnumerateGeometriesElements(geom))
            {
                if (elem.Extents.Intersects(env))
                    intersectingGeoms.Add(elem);
                else
                    disjointGeoms.Add(elem);
            }
            return _geomFactory.BuildGeometry(intersectingGeoms);
        }

        private IGeometry<TCoordinate> UnionActual(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
        {
            return g0.Union(g1);
        }

        #region Nested type: Worker
#if useWorker

        private class Worker
        {
            private readonly int end;
            private readonly IList geoms;
            private readonly int start;
            private readonly CascadedPolygonUnion<TCoordinate> tool;

            private IGeometry<TCoordinate> ret;

            protected internal Worker(CascadedPolygonUnion<TCoordinate> tool, IList geoms,
                                      int start, int end)
            {
                this.tool = tool;
                this.geoms = geoms;
                this.start = start;
                this.end = end;
            }

            public IGeometry<TCoordinate> Geometry
            {
                get { return ret; }
            }

            internal void Execute()
            {
                ret = tool.BinaryUnion(geoms, start, end);
            }
        }

#endif
        #endregion
    }
}