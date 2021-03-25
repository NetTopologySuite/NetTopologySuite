using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;

namespace Open.Topology.TestRunner.Functions
{
    public class SpatialIndexFunctions
    {
        public static Geometry KdTreeQuery(Geometry pts, Geometry query, double tolerance)
        {
            var index = BuildKdTree(pts, tolerance);
            var result = index.Query(query.EnvelopeInternal);
            var resultCoords = KdTree<object>.ToCoordinates(result);
            return pts.Factory.CreateMultiPointFromCoords(resultCoords);
        }

        public static Geometry KdTreeQueryRepeated(Geometry pts, Geometry queryEnv, double tolerance)
        {
            var index = BuildKdTree(pts, tolerance);
            var result = index.Query(queryEnv.EnvelopeInternal);
            var resultCoords = KdTree<object>.ToCoordinates(result, true);
            return pts.Factory.CreateMultiPointFromCoords(resultCoords);
        }

        private static KdTree<object> BuildKdTree(Geometry geom, double tolerance)
        {
            var index = new KdTree<object>(tolerance);
            var pt = geom.Coordinates;
            for (int i = 0; i < pt.Length; i++)
            {
                index.Insert(pt[i]);
            }
            return index;
        }

        private class DelegateGeometryFilter : IGeometryFilter
        {
            public Action<Geometry> DoFilter { get; set; }

            public void Filter(Geometry geom)
            {
                DoFilter(geom);
            }
        }

        public static Geometry STRtreeBounds(Geometry geoms)
        {
            var index = BuildSTRtree(geoms);
            var bounds = new List<Geometry>();
            addBounds(index.Root, bounds, geoms.Factory);
            return geoms.Factory.BuildGeometry(bounds);
        }

        private static void addBounds(IBoundable<Envelope, Geometry> bnd, List<Geometry>  bounds,
            GeometryFactory factory)
        {
            // don't include bounds of leaf nodes
            if (!(bnd is AbstractNode<Envelope, Geometry>)) return;

            var env = (Envelope)bnd.Bounds;
            bounds.Add(factory.ToGeometry(env));
            if (bnd is AbstractNode<Envelope, Geometry>) {
                var node = (AbstractNode<Envelope, Geometry>)bnd;
                var children = node.ChildBoundables;
                foreach (var child in children)
                {
                    addBounds(child, bounds, factory);
                }
            }
        }

        public static Geometry STRtreeQuery(Geometry geoms, Geometry queryEnv)
        {
            var index = BuildSTRtree(geoms);
            var result = index.Query(queryEnv.EnvelopeInternal);
            return geoms.Factory.BuildGeometry(result);
        }

        private static STRtree<Geometry> BuildSTRtree(Geometry geom)
        {
            var index = new STRtree<Geometry>();
            geom.Apply(new DelegateGeometryFilter
            {
                DoFilter = delegate(Geometry tmpGeometry)
                {
                    // only insert atomic geometries
                    if (tmpGeometry is GeometryCollection) return;
                    index.Insert(tmpGeometry.EnvelopeInternal, tmpGeometry);
                }
            });

            return index;
        }

        public static Geometry StrTreeNN(Geometry geoms, Geometry geom)
        {
            var index = BuildSTRtree(geoms);
            object result = index.NearestNeighbour(geom.EnvelopeInternal, geom, new GeometryItemDistance());
            return (Geometry)result;
        }

        public static Geometry StrTreeNNInSet(Geometry geoms)
        {
            var index = BuildSTRtree(geoms);
            object[] result = index.NearestNeighbour(new GeometryItemDistance());
            var resultGeoms = new [] { (Geometry)result[0], (Geometry)result[1] };
            return geoms.Factory.CreateGeometryCollection(resultGeoms);
        }

        public static Geometry StrTreeNNk(Geometry geoms, Geometry geom, int k)
        {
            var index = BuildSTRtree(geoms);
            object[] knnObjects = index.NearestNeighbour(geom.EnvelopeInternal, geom, new GeometryItemDistance(), k);
            var geometryCollection = geoms.Factory.BuildGeometry(knnObjects.Cast<Geometry>());
            return geometryCollection;
        }

        public static Geometry QuadTreeQuery(Geometry geoms, Geometry queryEnv)
        {
            var index = BuildQuadtree(geoms);
            var result = index.Query(queryEnv.EnvelopeInternal);
            return geoms.Factory.BuildGeometry(result);
        }

        private static Quadtree<Geometry> BuildQuadtree(Geometry geom)
        {
            var index = new Quadtree<Geometry>();
            geom.Apply(new DelegateGeometryFilter()
            {
                DoFilter = delegate(Geometry tmpGeometry)
                {
                    // only insert atomic geometries
                    if (tmpGeometry is GeometryCollection) return;
                    index.Insert(tmpGeometry.EnvelopeInternal, tmpGeometry);
                }
            });
            return index;
        }

        public static Geometry MonotoneChains(Geometry geom)
        {
            var pts = geom.Coordinates;
            var chains = MonotoneChainBuilder.GetChains(pts);
            var lines = new List<LineString>(chains.Count);
            foreach (var mc in chains)
            {
                var mcPts = mc.Coordinates;
                var line = geom.Factory.CreateLineString(mcPts);
                lines.Add(line);
            }
            return geom.Factory.BuildGeometry(lines);
        }
    }
}
