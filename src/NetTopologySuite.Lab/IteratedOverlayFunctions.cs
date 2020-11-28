using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite
{
    public class IteratedOverlayFunctions
    {

        public static Geometry OverlayOld(Geometry coll)
        {
            return Overlay(coll, false, null);
        }

        public static Geometry OverlayNG(Geometry coll)
        {
            return Overlay(coll, true, null);
        }

        public static Geometry OverlaySR(Geometry coll, double scale)
        {
            var pm = new PrecisionModel(scale);
            return Overlay(coll, true, pm);
        }

        private static Geometry Overlay(Geometry coll, bool useNG, PrecisionModel pm)
        {
            var result = new List<Geometry>();
            for (int i = 0; i < coll.NumGeometries; i++)
            {
                var inGeom = coll.GetGeometryN(i);

                int size = result.Count;
                for (int j = 0; j < size; j++)
                {
                    var resGeom = result[j];
                    if (resGeom.IsEmpty) continue;

                    var intGeom = ExtractPolygons(OverlayIntersection(resGeom, inGeom, useNG, pm));
                    if (!intGeom.IsEmpty)
                    {
                        result.Add(intGeom);

                        var resDiff = ExtractPolygons(OverlayDifference(resGeom, intGeom, useNG, pm));
                        result[j] = resDiff;

                        inGeom = ExtractPolygons(OverlayDifference(inGeom, intGeom, useNG, pm));
                    }
                }
                // keep remainder of input (non-overlapped part)
                if (!inGeom.IsEmpty)
                {
                    result.AddRange(PolygonExtracter.GetPolygons(inGeom));
                    //result.add( inGeom );
                }
            }
            // TODO: return only non-empty polygons
            var resultPolys = ExtractPolygonsNonEmpty(result);
            return coll.Factory.BuildGeometry(resultPolys);
        }


        public static Geometry OverlayIndexedNG(Geometry coll)
        {
            return OverlayIndexed(coll, true, null);
        }

        private static Geometry OverlayIndexed(Geometry coll, bool useNG, PrecisionModel pm)
        {
            var tree = new Quadtree<Geometry>();
            for (int i = 0; i < coll.NumGeometries; i++)
            {

                var inGeom = coll.GetGeometryN(i);
                var results = tree.Query(inGeom.EnvelopeInternal);

                foreach (var resPoly in results)
                {

                    var intGeom = ExtractPolygons(OverlayIntersection(resPoly, inGeom, useNG, pm));
                    var intList = PolygonExtracter.GetPolygons(intGeom);

                    // resultant is overlapped by next input
                    if (!intGeom.IsEmpty && intList.Count > 0)
                    {
                        tree.Remove(resPoly.EnvelopeInternal, resPoly);

                        foreach (var intPoly in intList)
                        {
                            tree.Insert(intPoly.EnvelopeInternal, intPoly);
                            var resDiff = OverlayDifference(resPoly, intGeom, useNG, pm);
                            InsertPolys(resDiff, tree);

                            inGeom = ExtractPolygons(OverlayDifference(inGeom, intPoly, useNG, pm));
                        }
                    }
                }
                // keep remainder of input
                InsertPolys(inGeom, tree);
            }
            var result = tree.QueryAll();
            return coll.Factory.BuildGeometry(result);
        }

        private static void InsertPolys(Geometry geom, Quadtree<Geometry> tree)
        {
            if (geom.IsEmpty) return;
            var polyList = PolygonExtracter.GetPolygons(geom);
            foreach (var poly in polyList)
            {
                tree.Insert(poly.EnvelopeInternal, poly);
            }
        }

        private static Geometry OverlayIntersection(Geometry a, Geometry b, bool useNG, PrecisionModel pm)
        {
            if (useNG)
            {
                if (pm == null)
                    return OverlayNGRobust.Overlay(a, b, SpatialFunction.Intersection);
                return Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Intersection, pm);
            }
            return a.Intersection(b);
        }

        private static Geometry OverlayDifference(Geometry a, Geometry b, bool useNG, PrecisionModel pm)
        {
            if (useNG)
            {
                if (pm == null)
                    return OverlayNGRobust.Overlay(a, b, SpatialFunction.Difference);
                return Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Difference, pm);
            }
            return a.Difference(b);
        }

        private static Geometry ExtractPolygons(Geometry geom)
        {
            var polys = PolygonExtracter.GetPolygons(geom);
            return geom.Factory.BuildGeometry(polys);
        }

        private static List<Polygon> ExtractPolygonsNonEmpty(IEnumerable<Geometry> geoms)
        {
            var exPolys = new List<Polygon>();
            foreach (var geom in geoms)
            {
                if (!geom.IsEmpty)
                {
                    if (geom is Polygon) {
                        exPolys.Add((Polygon)geom);
                    }
                    else if (geom is MultiPolygon) {
                        exPolys.AddRange(PolygonExtracter.GetPolygons(geom).Cast<Polygon>());
                    }
                }
            }
            return exPolys;
        }
}}
