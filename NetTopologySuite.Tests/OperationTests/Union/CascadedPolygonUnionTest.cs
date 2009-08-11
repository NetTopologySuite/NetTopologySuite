using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using NetTopologySuite.Coordinates;
using Xunit;

namespace NetTopologySuite.Tests.OperationTests.Union
{
    public class CascadedPolygonUnionTest
    {
        private const string polygonfile = @"sh.txt";

        private static IGeometryFactory<BufferedCoordinate> _geometryFactory =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory());//new BufferedCoordinateFactory(1000) ));

        [Fact]
        public void Test()
        {
            //IGeometry<BufferedCoordinate> geometrySH = _geometryFactory.WktReader.Read(File.ReadAllText(polygonfile));
            IList<IGeometry<BufferedCoordinate>> geoms = new List<IGeometry<BufferedCoordinate>>();//.CreateGeometryCollection();

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

            //DateTime start = DateTime.Now;
            //
            //foreach (var geometry in geoms)
            //{
            //    u1 = u1 == null ? geometry.Clone() : SnapIfNeededOverlayOp<BufferedCoordinate>.Overlay(u1, geometry, SpatialFunctions.Union);
            //}
            //Console.WriteLine(string.Format("PolygonUnion duration: {0}", DateTime.Now.Subtract(start)));
            //Console.WriteLine(u1.ToString());
            //Console.WriteLine(u1.Extents.ToString());

            DateTime start = DateTime.Now;
            IGeometry<BufferedCoordinate> u2 = UnaryUnionOp<BufferedCoordinate>.Union(geoms);
            Console.WriteLine(string.Format("UnaryUnionOp duration: {0}", DateTime.Now.Subtract(start)));
            Console.WriteLine(u2.ToString());
            Console.WriteLine(u2.Extents.ToString());

            IGeometry<BufferedCoordinate> u1 = geoms[0];
            start = DateTime.Now;
            foreach (IPolygonal<BufferedCoordinate> geometry in Enumerable.Skip(geoms,1))
            {
                u1 = SnapIfNeededOverlayOp<BufferedCoordinate>.Overlay(u1, geometry, SpatialFunctions.Union);
            }
            Console.WriteLine(string.Format("SnapIfNeededOverlayOp duration: {0}", DateTime.Now.Subtract(start)));
            Console.WriteLine(u1.ToString());
            Console.WriteLine(u1.Extents.ToString());

            Assert.True(u1.Extents.Equals(u2.Extents));
            //Assert.True(u1.Equals(u2));
// Console.WriteLine("equal");
//else
// Console.WriteLine("shit");

        }
    }
}
