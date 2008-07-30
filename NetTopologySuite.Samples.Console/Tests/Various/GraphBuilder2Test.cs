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
            ShapefileReader reader = new ShapefileReader(fileName);
            IGeometryCollection edges = reader.ReadAll();
            return TestGraphBuilder2WithSampleGeometries(edges, src, dst);
        }

        /// <summary>
        /// Uses the passed geometry collection to generate a QuickGraph.
        /// </summary>
        /// <param name="edges"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public ILineString TestGraphBuilder2WithSampleGeometries(IGeometryCollection edges, ICoordinate src, ICoordinate dst)
        {
            GraphBuilder2 builder = new GraphBuilder2(true);            
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

        [Test]
        public void TestGraphBuilder2WithSampleGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            ILineString path = builder.Perform(start, end);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);
        }        

        [Test]
        public void TestBidirectionalGraphBuilder2WithSampleGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2(true);
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();

            ILineString path = builder.Perform(start.Coordinate, end.Coordinate);
            Assert.IsNotNull(path);
            Assert.AreEqual(result, path);

            ILineString revpath = builder.Perform(end, start);
            Assert.IsNotNull(revpath);
            Assert.AreEqual(revresult, revpath);
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingNoGeometries()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingOneGeometry()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingARepeatedGeometry()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsFalse(builder.Add(a));
            Assert.IsFalse(builder.Add(a, a));
            builder.Initialize();
        }

        [Test]
        [ExpectedException(typeof(TopologyException))]
        public void CheckGraphBuilder2ExceptionUsingDifferentFactories()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            Assert.IsTrue(builder.Add(a));
            Assert.IsTrue(builder.Add(b, c));
            Assert.IsTrue(builder.Add(d));
            builder.Add(GeometryFactory.Default.CreateLineString(new ICoordinate[]
            {
                new Coordinate(0 ,0),
                new Coordinate(50 , 50),
            }));
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void CheckGraphBuilder2ExceptionUsingDoubleInitialization()
        {
            GraphBuilder2 builder = new GraphBuilder2();
            builder.Add(a);
            builder.Add(b, c);
            builder.Add(d);
            builder.Add(e);
            builder.Initialize();
            builder.Initialize();
        }

        [Test]
        public void BuildGraphFromMinimalGraphShapefile()
        {
            string shapepath = "minimalgraph.shp";
            int count = 15;

            Assert.IsTrue(File.Exists(shapepath));
            ShapefileReader reader = new ShapefileReader(shapepath);
            IGeometryCollection edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            ILineString startls = edges.GetGeometryN(0).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            ILineString endls = edges.GetGeometryN(5).GetGeometryN(0) as ILineString; ;
            Assert.IsNotNull(endls);

            GraphBuilder2 builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                ILineString str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));
            }
            builder.Initialize();
            
            ILineString path = builder.Perform(startls.StartPoint, endls.EndPoint);
            Assert.IsNotNull(path);
        }

        [Test]
        public void BuildGraphFromCompleteGraphShapefile()
        {
            string shapepath = "graph.shp";
            int count = 1179;

            Assert.IsTrue(File.Exists(shapepath));
            ShapefileReader reader = new ShapefileReader(shapepath);
            IGeometryCollection edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            ILineString startls = edges.GetGeometryN(515).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            IPoint startPoint = startls.EndPoint;
            Assert.AreEqual(2317300d, startPoint.X);
            Assert.AreEqual(4843961d, startPoint.Y);

            ILineString endls = edges.GetGeometryN(141).GetGeometryN(0) as ILineString; ;
            Assert.IsNotNull(endls);
            IPoint endPoint = endls.StartPoint;
            Assert.AreEqual(2322739d, endPoint.X);
            Assert.AreEqual(4844539d, endPoint.Y);

            GraphBuilder2 builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                ILineString str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));
            }
            builder.Initialize();

            ILineString path = builder.Perform(startPoint, endPoint);
            Assert.IsNotNull(path);
            SaveGraphResult(path);

            ILineString reverse = builder.Perform(endPoint, startPoint);
            Assert.IsNotNull(reverse);
            Assert.AreEqual(path, reverse.Reverse());
        }

        private void SaveGraphResult(IGeometry path)
        {
            if (path == null) 
                throw new ArgumentNullException("path");

            string shapepath = "graphresult";
            if (File.Exists(shapepath + shp))
                File.Delete(shapepath + shp);
            Assert.IsFalse(File.Exists(shapepath + shp));
            if (File.Exists(shapepath + shx))
                File.Delete(shapepath + shx);
            Assert.IsFalse(File.Exists(shapepath + shx));
            if (File.Exists(shapepath + dbf))
                File.Delete(shapepath + dbf);
            Assert.IsFalse(File.Exists(shapepath + dbf));

            string field1 = "OBJECTID";            
            Feature feature = new Feature(path, new AttributesTable());
            feature.Attributes.AddAttribute(field1, 0);                        

            DbaseFileHeader header = new DbaseFileHeader();
            header.NumRecords = 1;            
            header.NumFields = 1;
            header.AddColumn(field1, 'N', 5, 0);
            
            ShapefileDataWriter writer = new ShapefileDataWriter(shapepath, factory);
            writer.Header = header;
            writer.Write(new List<Feature>(new Feature[] { feature, }));

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }

        [Ignore]
        [Test]
        public void BuildStradeFixed()
        {
            string path = "strade" + shp;
            Assert.IsTrue(File.Exists(path));
            
            ShapefileDataReader reader = new ShapefileDataReader(path, factory);
            List<Feature> features = new List<Feature>(reader.RecordCount);
            while (reader.Read())
            {
                Feature feature = new Feature(reader.Geometry, new AttributesTable());
                object[] values = new object[reader.FieldCount - 1];
                reader.GetValues(values);
                for (int i = 0; i < values.Length; i++)
                {
                    string name = reader.GetName(i + 1);
                    object value = values[i];
                    feature.Attributes.AddAttribute(name, value);
                }
                features.Add(feature);
            }
            Assert.AreEqual(703, features.Count);

            string shapepath = "strade_fixed";
            if (File.Exists(shapepath + shp))
                File.Delete(shapepath + shp);
            Assert.IsFalse(File.Exists(shapepath + shp));
            if (File.Exists(shapepath + shx))
                File.Delete(shapepath + shx);
            Assert.IsFalse(File.Exists(shapepath + shx));
            if (File.Exists(shapepath + dbf))
                File.Delete(shapepath + dbf);
            Assert.IsFalse(File.Exists(shapepath + dbf));

            DbaseFileHeader header = reader.DbaseHeader;
            
            ShapefileDataWriter writer = new ShapefileDataWriter(shapepath, factory);
            writer.Header = header;
            writer.Write(features);

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }

        [Test]
        public void BuildGraphFromStradeShapefile()
        {
            string shapepath = "strade_fixed.shp";
            int count = 703;

            Assert.IsTrue(File.Exists(shapepath));
            ShapefileReader reader = new ShapefileReader(shapepath);
            IGeometryCollection edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            ICoordinate startCoord = new Coordinate(2317300d, 4843961d);
            ICoordinate endCoord = new Coordinate(2322739d, 4844539d);

            bool startFound = false;
            bool endFound = false;            
            GraphBuilder2 builder = new GraphBuilder2(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                ILineString str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsTrue(builder.Add(str));

                if (!startFound)
                {
                    List<ICoordinate> coords = new List<ICoordinate>(str.Coordinates);
                    if (coords.Contains(startCoord))
                        startFound = true;
                }

                if (!endFound)
                {
                    List<ICoordinate> coords = new List<ICoordinate>(str.Coordinates);
                    if (coords.Contains(endCoord))
                        endFound = true;
                }
            }
            builder.Initialize();
            Assert.IsTrue(startFound);
            Assert.IsTrue(endFound);

            ILineString path = builder.Perform(startCoord, endCoord);
            Assert.IsNotNull(path);
            SaveGraphResult(path);

            ILineString reverse = builder.Perform(startCoord, endCoord);
            Assert.IsNotNull(reverse);
            Assert.AreEqual(path, reverse.Reverse());
        }
    }
}
