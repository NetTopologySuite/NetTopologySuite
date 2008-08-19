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

        private IGeometry LoadGraphResult()
        {
            string path = "graphresult.shp";            
            Assert.IsTrue(Path.GetExtension(path) == shp);

            ShapefileReader reader = new ShapefileReader(path);
            IGeometryCollection coll = reader.ReadAll();
            Assert.AreEqual(1, coll.Count);

            IGeometry geom = coll.GetGeometryN(0);
            Assert.IsInstanceOfType(typeof(IMultiLineString), geom);
            IGeometry str = geom.GetGeometryN(0);
            Assert.IsInstanceOfType(typeof(ILineString), str);
            return str;
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

            // Insert arbitrary userdata
            for (int i = 0; i < count; i++)
            {
                IMultiLineString g = edges.GetGeometryN(i) as IMultiLineString;
                Assert.IsNotNull(g);
                ILineString ls = g.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(ls);

                Assert.IsNull(ls.UserData);
                ls.UserData = i;
                Assert.IsNotNull(ls.UserData);
            }

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

            PathFinder finder = new PathFinder(true);
            foreach (IMultiLineString mlstr in edges.Geometries)
            {
                Assert.AreEqual(1, mlstr.NumGeometries);
                ILineString str = mlstr.GetGeometryN(0) as ILineString;
                Assert.IsNotNull(str);
                Assert.IsNotNull(str.UserData);
                Assert.IsTrue(finder.Add(str));
            }
            finder.Initialize();

            int expectedResultCount = 8;
            IGeometry path = finder.Find(startPoint, endPoint);
            Assert.IsNotNull(path);
            Assert.IsInstanceOfType(typeof (IMultiLineString), path);
            IMultiLineString strings = (IMultiLineString) path;            
            Assert.AreEqual(expectedResultCount, strings.NumGeometries);
            foreach (IGeometry g in strings.Geometries)
            {
                Assert.IsNotNull(g.UserData);
                Console.WriteLine("{0} : {1}", g.UserData, g);
            }

            IGeometry reversedPath = finder.Find(endPoint, startPoint);
            Assert.IsNotNull(reversedPath);
            Assert.IsInstanceOfType(typeof(IMultiLineString), reversedPath);

            IMultiLineString reversedStrings = (IMultiLineString) reversedPath;
            Assert.AreEqual(expectedResultCount, reversedStrings.NumGeometries);
            foreach (IGeometry g in reversedStrings.Geometries)
            {
                Assert.IsNotNull(g.UserData);
                Console.WriteLine("{0} : {1}", g.UserData, g);
            }

            for (int i = 0; i < expectedResultCount; i++)
            {
                IGeometry item = strings.GetGeometryN(i);
                IGeometry itemReversed = strings.GetGeometryN(expectedResultCount - 1 - i);
                Assert.AreNotEqual(item.UserData, itemReversed.UserData);
                Assert.AreNotEqual(item, itemReversed);
            }
        }
    }
}
