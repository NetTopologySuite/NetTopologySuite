using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Index.Strtree;

namespace Open.Topology.TestRunner.Functions
{
    public class SpatialIndexFunctions
    {
        public static IGeometry KdTreeQuery(IGeometry pts, IGeometry query, double tolerance)
        {
            var index = BuildKdTree(pts, tolerance);
            var result = index.Query(query.EnvelopeInternal);
            var resultCoords = KdTree<object>.ToCoordinates(result);
            return pts.Factory.CreateMultiPoint(resultCoords);
        }

        public static IGeometry KdTreeQueryRepeated(IGeometry pts, IGeometry queryEnv, double tolerance)
        {
            var index = BuildKdTree(pts, tolerance);
            var result = index.Query(queryEnv.EnvelopeInternal);
            var resultCoords = KdTree<object>.ToCoordinates(result, true);
            return pts.Factory.CreateMultiPoint(resultCoords);
        }

        private static KdTree<object> BuildKdTree(IGeometry geom, double tolerance)
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
            public Action<IGeometry> DoFilter { get; set; }

            public void Filter(IGeometry geom)
            {
                DoFilter(geom);
            }
        }

        public static IGeometry STRtreeQuery(IGeometry geoms, IGeometry queryEnv)
        {
            var index = BuildSTRtree(geoms);
            var result = index.Query(queryEnv.EnvelopeInternal);
            return geoms.Factory.BuildGeometry(result);
        }

        private static STRtree<IGeometry> BuildSTRtree(IGeometry geom)
        {
            var index = new STRtree<IGeometry>();
            geom.Apply(new DelegateGeometryFilter
            {
                DoFilter = delegate(IGeometry tmpGeometry)
                {
                    // only insert atomic geometries
                    if (tmpGeometry is IGeometryCollection) return;
                    index.Insert(tmpGeometry.EnvelopeInternal, tmpGeometry);
                }
            });

            return index;
        }

        public static IGeometry QuadTreeQuery(IGeometry geoms, IGeometry queryEnv)
        {
            var index = BuildQuadtree(geoms);
            var result = index.Query(queryEnv.EnvelopeInternal);
            return geoms.Factory.BuildGeometry(result);
        }

        private static Quadtree<IGeometry> BuildQuadtree(IGeometry geom)
        {
            var index = new Quadtree<IGeometry>();
            geom.Apply(new DelegateGeometryFilter()
            {
                DoFilter = delegate(IGeometry tmpGeometry)
                {
                    // only insert atomic geometries
                    if (tmpGeometry is IGeometryCollection) return;
                    index.Insert(tmpGeometry.EnvelopeInternal, tmpGeometry);
                }
            });
            return index;
        }
    }
}