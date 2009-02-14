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
    public class PathFinderTest
    {
        private const string shp = ".shp";
        private const string shx = ".shx";
        private const string dbf = ".dbf";

        private IGeometryFactory factory;       
       
        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            Environment.CurrentDirectory = Path.Combine(
               AppDomain.CurrentDomain.BaseDirectory,
               @"../../../NetTopologySuite.Samples.Shapefiles");

            factory = GeometryFactory.Fixed;       
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
            
            var writer = new ShapefileDataWriter(shapepath, factory) {Header = header};
            writer.Write(features);

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }       

        private IGeometry LoadGraphResult()
        {
            var path = "graphresult.shp";            
            Assert.IsTrue(Path.GetExtension(path) == shp);

            var reader = new ShapefileReader(path);
            var coll = reader.ReadAll();
            Assert.AreEqual(1, coll.Count);

            var geom = coll.GetGeometryN(0);
            Assert.IsInstanceOfType(typeof(IMultiLineString), geom);
            var str = geom.GetGeometryN(0);
            Assert.IsInstanceOfType(typeof(ILineString), str);
            return str;
        }

        private void SaveGraphResult(IGeometry path)
        {
            if (path == null) 
                throw new ArgumentNullException("path");

            var shapepath = "graphresult";
            if (File.Exists(shapepath + shp))
                File.Delete(shapepath + shp);
            Assert.IsFalse(File.Exists(shapepath + shp));
            if (File.Exists(shapepath + shx))
                File.Delete(shapepath + shx);
            Assert.IsFalse(File.Exists(shapepath + shx));
            if (File.Exists(shapepath + dbf))
                File.Delete(shapepath + dbf);
            Assert.IsFalse(File.Exists(shapepath + dbf));

            var field1 = "OBJECTID";            
            var feature = new Feature(path, new AttributesTable());
            feature.Attributes.AddAttribute(field1, 0);                        

            var header = new DbaseFileHeader {NumRecords = 1, NumFields = 1};
            header.AddColumn(field1, 'N', 5, 0);
            
            var writer = new ShapefileDataWriter(shapepath, factory) {Header = header};
            writer.Write(new List<Feature>(new[] { feature, }));

            Assert.IsTrue(File.Exists(shapepath + shp));
            Assert.IsTrue(File.Exists(shapepath + shx));
            Assert.IsTrue(File.Exists(shapepath + dbf));
        }

        [Test]
        public void BuildGraphFromCompleteGraphShapefile()
        {
            var shapepath = "graph.shp";
            var count = 1179;

            Assert.IsTrue(File.Exists(shapepath));
            var reader = new ShapefileReader(shapepath);
            var edges = reader.ReadAll();
            Assert.IsNotNull(edges);
            Assert.IsInstanceOfType(typeof(GeometryCollection), edges);
            Assert.AreEqual(count, edges.NumGeometries);

            // Insert arbitrary userdata
            for (var i = 0; i < count; i++)
            {
                var g = edges.GetGeometryN(i) as IMultiLineString;
                Assert.IsNotNull(g);
                var ls = g.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(ls);

                Assert.IsNull(ls.UserData);
                ls.UserData = i;
                Assert.IsNotNull(ls.UserData);
            }

            var startls = edges.GetGeometryN(515).GetGeometryN(0) as ILineString;
            Assert.IsNotNull(startls);
            var startPoint = startls.EndPoint;
            Assert.AreEqual(2317300d, startPoint.X);
            Assert.AreEqual(4843961d, startPoint.Y);

            var endls = edges.GetGeometryN(141).GetGeometryN(0) as ILineString; ;
            Assert.IsNotNull(endls);
            var endPoint = endls.StartPoint;
            Assert.AreEqual(2322739d, endPoint.X);
            Assert.AreEqual(4844539d, endPoint.Y);

            var finder = new PathFinder(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                var str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsNotNull(str.UserData);
                Assert.IsTrue(finder.Add(str));
            }
            finder.Initialize();

            var expectedResultCount = 8;
            var path = finder.Find(startPoint, endPoint);
            Assert.IsNotNull(path);
            Assert.IsInstanceOfType(typeof (IMultiLineString), path);
            var strings = (IMultiLineString) path;            
            Assert.AreEqual(expectedResultCount, strings.NumGeometries);
            foreach (var g in strings.Geometries)
            {
                Assert.IsNotNull(g.UserData);
                Console.WriteLine("{0} : {1}", g.UserData, g);
            }

            var reversedPath = finder.Find(endPoint, startPoint);
            Assert.IsNotNull(reversedPath);
            Assert.IsInstanceOfType(typeof(IMultiLineString), reversedPath);

            var reversedStrings = (IMultiLineString) reversedPath;
            Assert.AreEqual(expectedResultCount, reversedStrings.NumGeometries);
            foreach (var g in reversedStrings.Geometries)
            {
                Assert.IsNotNull(g.UserData);
                Console.WriteLine("{0} : {1}", g.UserData, g);
            }

            for (var i = 0; i < expectedResultCount; i++)
            {
                var item = strings.GetGeometryN(i);
                var itemReversed = strings.GetGeometryN(expectedResultCount - 1 - i);
                Assert.AreNotEqual(item.UserData, itemReversed.UserData);
                Assert.AreNotEqual(item, itemReversed);
            }
        }
    }
}
