using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests.Union
{
    public class CascadedPolygonUnionTest
    {
        private const string polygonfile = @"sh.txt";

        private static IGeometryFactory<BufferedCoordinate> _geometryFactory =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory( ));

        [Fact]
        public void Test()
        {
            //IGeometry<BufferedCoordinate> geometrySH = _geometryFactory.WktReader.Read(File.ReadAllText(polygonfile));
            IList<IPolygon<BufferedCoordinate>> geoms = new List<IPolygon<BufferedCoordinate>>();//.CreateGeometryCollection();

            using (StreamReader txt = new StreamReader(polygonfile))
            {
                while (!txt.EndOfStream)
                {
                    IGeometry<BufferedCoordinate> geom = _geometryFactory.WktReader.Read(txt.ReadLine());
                    var mp =  geom as IMultiPolygon<BufferedCoordinate>;
                    if (mp != null)
                    {
                        foreach (var polygon in mp)
                            geoms.Add(polygon);
                        continue;
                    }
                    var p =  geom as IPolygon<BufferedCoordinate>;
                    if ( p != null )
                        geoms.Add(p);
                }
                //geoms.Add(_geometryFactory.WktReader.Read());
            }

            DateTime start = DateTime.Now;
            //IGeometry<BufferedCoordinate> u1 = null;
            //foreach (var geometry in geoms)
            //{
            //    u1 = u1 == null ? geometry : OverlayOp<BufferedCoordinate>.Overlay(u1, geometry, SpatialFunctions.Union);
            //}
            //Console.WriteLine(string.Format("PolygonUnion duration: {0}", DateTime.Now.Subtract(start)));

            start = DateTime.Now;
            IGeometry<BufferedCoordinate> u2 = CascadedPolygonUnion<BufferedCoordinate>.Union(geoms);
            Console.WriteLine(string.Format("CascadedPolygonUnion duration: {0}", DateTime.Now.Subtract(start)));
            Console.WriteLine(u2.ToString());
            //Assert.True(u1.Equals(u2));
// Console.WriteLine("equal");
//else
// Console.WriteLine("shit");

        }
    }
}
