using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Utilities;

namespace Open.Topology.TestRunner.Functions
{
    public class PolygonizeFunctions
    {

        public static IGeometry polygonize(IGeometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            var polygonizer = new Polygonizer();
            polygonizer.Add(CollectionUtil.Cast<ILineString, IGeometry>(lines));
            var polys = polygonizer.GetPolygons();
            var polyArray = GeometryFactory.ToPolygonArray(polys);
            return g.Factory.CreateGeometryCollection(polyArray);
        }
        public static IGeometry polygonizeDangles(IGeometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(CollectionUtil.Cast<ILineString, IGeometry>(lines));
            var geom = polygonizer.GetDangles();
            return g.Factory.BuildGeometry(CollectionUtil.Cast<ILineString, IGeometry>(geom));
        }
        public static IGeometry polygonizeCutEdges(IGeometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(CollectionUtil.Cast<ILineString, IGeometry>(lines));
            var geom = polygonizer.GetCutEdges();
            return g.Factory.BuildGeometry(CollectionUtil.Cast<ILineString, IGeometry>(geom));
        }
        public static IGeometry polygonizeInvalidRingLines(IGeometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(CollectionUtil.Cast<ILineString, IGeometry>(lines));
            var geom = polygonizer.GetInvalidRingLines();
            return g.Factory.BuildGeometry(geom);
        }
        public static IGeometry polygonizeAllErrors(IGeometry g)
        {
            var lines = LineStringExtracter.GetLines(g);
            Polygonizer polygonizer = new Polygonizer();
            polygonizer.Add(CollectionUtil.Cast<ILineString, IGeometry>(lines));
            var errs = new List<ILineString>();
            errs.AddRange(polygonizer.GetDangles());
            errs.AddRange(polygonizer.GetCutEdges());
            errs.AddRange(CollectionUtil.Cast<IGeometry, ILineString>(polygonizer.GetInvalidRingLines()));
            return g.Factory.BuildGeometry(CollectionUtil.Cast<ILineString, IGeometry>(errs));
        }



    }
}