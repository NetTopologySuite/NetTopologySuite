using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap;
using GisSharpBlog.NetTopologySuite.Operation.Union;
using NetTopologySuite.Coordinates;
using NPack.Interfaces;
using Xunit;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Tests.OperationTests.Union
{
    public class IOTool<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IDivisible<Double, TCoordinate>, IConvertible
    {
        public static IEnumerable<IGeometry<TCoordinate>> ReadGeometries(IGeometryFactory<TCoordinate> factory, String file)
        {
            using (StreamReader txt = new StreamReader(file))
            {
                while (!txt.EndOfStream)
                {
                    IGeometry<TCoordinate> geom = factory.WktReader.Read(txt.ReadLine());
                    var mp = geom as IMultiPolygon<TCoordinate>;
                    if (mp != null)
                    {
                        foreach (var polygon in mp)
                            yield return polygon;
                        continue;
                    }
                    var p = geom as IPolygon<TCoordinate>;
                    if (p != null)
                        yield return p;
                }
            }
        }
    }
    
    public class CascadedPolygonUnionTest
    {
        private const string polygonfile = @"sh.txt";

        private static IGeometryFactory<coord> _geometryFactory =
            new GeometryFactory<coord>(new coordSeqFac());

        [Fact]
        public void SchleswigHolsteinTest()
        {
            IList<IGeometry<coord>> geoms = new List<IGeometry<coord>>(IOTool<coord>.ReadGeometries(_geometryFactory, polygonfile));

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            //CascadedPolygonUnion<coord> cpu = new CascadedPolygonUnion<coord>(geoms;);
            IGeometry<coord> u2 = UnaryUnionOp<coord>.Union(geoms);

            stopwatch.Stop();
            Console.WriteLine(string.Format("UnaryUnionOp duration: {0}", stopwatch.Elapsed));
            Console.WriteLine(u2.ToString());
            Console.WriteLine(u2.Extents.ToString());

            stopwatch.Stop();
            Console.WriteLine(string.Format("UnaryUnionOp duration: {0}", stopwatch.Elapsed));
            Console.WriteLine(u2.ToString());
            Console.WriteLine(u2.Extents.ToString());

            IGeometry<coord> u1 = geoms[0];
            stopwatch.Reset();
            stopwatch.Start();
            foreach (IPolygonal<coord> geometry in Enumerable.Skip(geoms, 1))
            { 
                u1 = SnapIfNeededOverlayOp<coord>.Overlay(u1, geometry, SpatialFunctions.Union);
            }
            stopwatch.Stop();
            Console.WriteLine(string.Format("SnapIfNeededOverlayOp duration: {0}", stopwatch.Elapsed));
            Console.WriteLine(u1.ToString());
            Console.WriteLine(u1.Extents.ToString());

            Assert.True(u1.Extents.Equals(u2.Extents));

        }
    }
}
