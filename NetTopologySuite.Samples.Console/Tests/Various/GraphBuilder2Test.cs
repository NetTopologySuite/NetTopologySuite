using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Features;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.Tests.Various;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class GraphBuilder2Test
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            factory = GeometryFactory.Fixed;

            a = factory.CreateLineString(new ICoordinate[]
                                             {
                                                 new Coordinate(0, 0),
                                                 new Coordinate(100, 0),
                                                 new Coordinate(200, 100),
                                                 new Coordinate(200, 200),
                                             });
            b = factory.CreateLineString(new ICoordinate[]
                                             {
                                                 new Coordinate(0, 0),
                                                 new Coordinate(100, 100),
                                                 new Coordinate(200, 200),
                                             });
            c = factory.CreateLineString(new ICoordinate[]
                                             {
                                                 new Coordinate(0, 0),
                                                 new Coordinate(0, 100),
                                                 new Coordinate(100, 200),
                                                 new Coordinate(200, 200),
                                             });
            d = factory.CreateLineString(new ICoordinate[]
                                             {
                                                 new Coordinate(0, 0),
                                                 new Coordinate(300, 0),
                                                 new Coordinate(300, 200),
                                                 new Coordinate(150, 200),
                                                 new Coordinate(150, 300),
                                             });
            e = factory.CreateLineString(new ICoordinate[]
                                             {
                                                 new Coordinate(100, 300),
                                                 new Coordinate(150, 300),
                                                 new Coordinate(200, 300),
                                             });

            result = factory.CreateLineString(new ICoordinate[]
                                                  {
                                                      new Coordinate(0, 0),
                                                      new Coordinate(300, 0),
                                                      new Coordinate(300, 200),
                                                      new Coordinate(150, 200),
                                                      new Coordinate(150, 300),
                                                  });
            revresult = result.Reverse();

            start = a.StartPoint;
            end = d.EndPoint;
        }

        #endregion

        private const string shp = ".shp";
        private const string shx = ".shx";
        private const string dbf = ".dbf";

        private IGeometryFactory factory;
        private ILineString a, b, c, d, e;
        private ILineString result, revresult;
        private IPoint start, end;

        /// <summary>
        /// Loads the shapefile as a graph allowing SP analysis to be carried out
        /// </summary>
        /// <param name="fileName">The name of the shape file we want to load</param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public ILineString TestGraphBuilder2WithSampleGeometries(string fileName, ICoordinate src, ICoordinate dst)
        {
            var reader = new ShapefileReader(fileName);
            var edges = reader.ReadAll();
            return TestGraphBuilder2WithSampleGeometries(edges, src, dst);
        }

        /// <summary>
        /// Uses the passed geometry collection to generate a QuickGraph.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public ILineString TestGraphBuilder2WithSampleGeometries(IGeometryCollection edges, ICoordinate src,
                                                                 ICoordinate dst)
        {
            var builder = new GraphBuilder2(true);
            foreach (IMultiLineString edge in edges.Geometries)
                foreach (ILineString line in edge.Geometries)
                    builder.Add(line);
            builder.Initialize();

            return builder.Perform(src, dst);
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Environment.CurrentDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"../../../NetTopologySuite.Samples.Shapefiles");
        }

        private void SaveGraphResult(IGeometry path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            const string shapepath = "graphresult";
            if (File.Exists(shapepath + shp))
                File.Delete(shapepath + shp);
            Assert.IsFalse(File.Exists(shapepath + shp));
            if (File.Exists(shapepath + shx))
                File.Delete(shapepath + shx);
            Assert.IsFalse(File.Exists(shapepath + shx));
            if (File.Exists(shapepath + dbf))
                File.Delete(shapepath + dbf);
            Assert.IsFalse(File.Exists(shapepath + dbf));

            const string field1 = "OBJECTID";
            var feature = new Feature(path, new AttributesTable());
            feature.Attributes.AddAttribute(field1, 0);

            var header = new DbaseFileHeader {NumRecords = 1, NumFields = 1};
            header.AddColumn(field1, 'N', 5, 0);

            var writer = new ShapefileDataWriter(shapepath, factory) {Header = header};
            writer.Write(new List<Feature>(new[] {feature,}));

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }

        [Test]
        public void BuildGraphFromCompleteGraphShapefile()
        {
            const string shapepath = "graph.shp";
            const int count = 1179;

            Assert.IsTrue(File.Exists(shapepath));
            var reader = new ShapefileReader(shapepath);
            var edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof (GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            var startls = edges.GetGeometryN(515).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            var startPoint = startls.EndPoint;
            Assert.AreEqual(2317300d, startPoint.X);
            Assert.AreEqual(4843961d, startPoint.Y);

            var endls = edges.GetGeometryN(141).GetGeometryN(0) as ILineString;
            ;
            Assert.IsNotNull(endls);
            var endPoint = endls.StartPoint;
            Assert.AreEqual(2322739d, endPoint.X);
            Assert.AreEqual(4844539d, endPoint.Y);

            var builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                var str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));
            }
            builder.Initialize();

            var path = builder.Perform(startPoint, endPoint);
            Assert.IsNotNull(path);
            SaveGraphResult(path);

            var reverse = builder.Perform(endPoint, startPoint);
            Assert.IsNotNull(reverse);
            Assert.AreEqual(path, reverse.Reverse());
        }

        [Test]
        public void BuildGraphFromMinimalGraphShapefile()
        {
            const string shapepath = "minimalgraph.shp";
            const int count = 15;

            Assert.IsTrue(File.Exists(shapepath));
            var reader = new ShapefileReader(shapepath);
            var edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof (GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            var startls = edges.GetGeometryN(0).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            var endls = edges.GetGeometryN(5).GetGeometryN(0) as ILineString;
            ;
            Assert.IsNotNull(endls);

            var builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                var str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));
            }
            builder.Initialize();

            var path = builder.Perform(startls.StartPoint, endls.EndPoint);
            Assert.IsNotNull(path);
        }

        [Test]
        public void BuildGraphFromStradeShapefile()
        {
            var shapepath = "strade_fixed.shp";
            var count = 703;

            Assert.IsTrue(File.Exists(shapepath));
            var reader = new ShapefileReader(shapepath);
            var edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof (GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            ICoordinate startCoord = new Coordinate(2317300d, 4843961d);
            ICoordinate endCoord = new Coordinate(2322739d, 4844539d);

            var startFound = false;
            var endFound = false;
            var builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                var str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));

                if (!startFound)
                {
                    var coords = new List<ICoordinate>(str.Coordinates);
                    if (coords.Contains(startCoord))
                        startFound = true;
                }

                if (!endFound)
                {
                    var coords = new List<ICoordinate>(str.Coordinates);
                    if (coords.Contains(endCoord))
                        endFound = true;
                }
            }
            builder.Initialize();
            Assert.IsTrue(startFound);
            Assert.IsTrue(endFound);

            var path = builder.Perform(startCoord, endCoord);
            Assert.IsNotNull(path);
            SaveGraphResult(path);

            var reverse = builder.Perform(startCoord, endCoord);
            Assert.IsNotNull(reverse);
            Assert.AreEqual(path, reverse.Reverse());
        }

        [Ignore]
        [Test]
        public void BuildStradeFixed()
        {
            var path = "strade" + shp;
            Assert.IsTrue(File.Exists(path));

            var reader = new ShapefileDataReader(path, factory);
            var features = new List<Feature>(reader.RecordCount);
            while (reader.Read())
            {
                var feature = new Feature(reader.Geometry, new AttributesTable());
                var values = new object[reader.FieldCount - 1];
                reader.GetValues(values);
                for (var i = 0; i < values.Length; i++)
                {
                    var name = reader.GetName(i + 1);
                    var value = values[i];
                    feature.Attributes.AddAttribute(name, value);
                }
                features.Add(feature);
            }
            Assert.AreEqual(703, features.Count);

            var shapepath = "strade_fixed";
            if (File.Exists(shapepath + shp))
                File.Delete(shapepath + shp);
            Assert.IsFalse(File.Exists(shapepath + shp));
            if (File.Exists(shapepath + shx))
                File.Delete(shapepath + shx);
            Assert.IsFalse(File.Exists(shapepath + shx));
            if (File.Exists(shapepath + dbf))
                File.Delete(shapepath + dbf);
            Assert.IsFalse(File.Exists(shapepath + dbf));

            var header = reader.DbaseHeader;

            var writer = new ShapefileDataWriter(shapepath, factory);
            writer.Header = header;
            writer.Write(features);

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }

        [Test]
        [ExpectedException(typeof (TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingARepeatedGeometry()
        {
            var builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsFalse(builder.Add(a));
            Assert.IsFalse(builder.Add(a, a));
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof (TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingDifferentFactories()
        {
            var builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsTrue(builder.Add(b, c));
            Assert.IsTrue(builder.Add(d));
            builder.Add(GeometryFactory.Default.CreateLineString(new ICoordinate[]
                                                                     {
                                                                         new Coordinate(0, 0),
                                                                         new Coordinate(50, 50),
                                                                     }));
        }

        [Test]
        [ExpectedException(typeof (ApplicationException))]
        public void CheckGraphBuilder2ExceptionUsingDoubleInitialization()
        {
            var builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof (TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingNoGeometries()
        {
            var builder = new GraphBuilder2();
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof (TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingOneGeometry()
        {
            var builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            builder.Initialize();
        }

        [Test]
        public void TestBidirectionalGraphBuilder2WithSampleGeometries()
        {
            var builder = new GraphBuilder2(true);
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            var path = builder.Perform(start.Coordinate, end.Coordinate);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);

            var revpath = builder.Perform(end, start);
            Assert.IsNotNull(revpath);
            Assert.AreEqual(revresult, revpath);
        }

        [Test]
        public void TestGraphBuilder2WithSampleGeometries()
        {
            var builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            var path = builder.Perform(start, end);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);
        }
    }
}