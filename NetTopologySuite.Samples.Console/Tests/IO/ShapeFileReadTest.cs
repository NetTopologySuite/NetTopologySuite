using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.IO
{
    [TestFixture]
    public class ShapeFileDataReaderTest : BaseSamples
    {  
        [SetUp]
        public void SetUp()
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory = 
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar));
        }

        // see https://code.google.com/p/nettopologysuite/issues/detail?id=175
        [Test]
        public void TestIssue175_ReadingShapeFileUsingShpExtension()
        {
            using (ShapefileDataReader reader = new ShapefileDataReader("crustal_test.shp", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }
        }

        [Test]
        public void TestReadingCrustalTestShapeFile()
        {
            // Original file with characters '°' in NAME field.
            using (ShapefileDataReader reader = new ShapefileDataReader("crustal_test_bugged", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }

            // Removed NAME field characters
            using (ShapefileDataReader reader = new ShapefileDataReader("crustal_test", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }
        }

        [Test, ExpectedException(typeof(FileNotFoundException))]
        [Ignore("File aaa.shp not exists")]
        public void TestReadingAaaShapeFile()
        {
            using (ShapefileDataReader reader = new ShapefileDataReader("aaa", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }
            Assert.Fail();
        }

        [Test]
        public void TestReadingShapeFileWithNulls()
        {
            using (ShapefileDataReader reader = new ShapefileDataReader("AllNulls", Factory))
            {
                while (reader.Read())
                {
                    IGeometry geom = reader.Geometry;
                    Assert.IsNotNull(geom);

                    object[] values = new object[5];
                    int result = reader.GetValues(values);
                    Assert.IsNotNull(values);
                }
            }
        }
        [Test]
        public void TestReadingShapeFileZ()
        {
            //Use a factory with a coordinate sequence factor that can handle measure values
            GeometryFactory factory = new GeometryFactory(DotSpatialAffineCoordinateSequenceFactory.Instance);
             
            const int distance = 500;
            using (ShapefileDataReader reader = new ShapefileDataReader("with_M", factory))
            { // ""
                int index = 0;

                reader.Read();
                IGeometry geom = reader.Geometry;
                double firstM = geom.GetOrdinates(Ordinate.M).First();
                Assert.AreEqual(400, firstM);

                while (reader.Read())
                {
                    geom = reader.Geometry;
                    Assert.IsNotNull(geom);
                    Assert.IsTrue(geom.IsValid);
                    Debug.WriteLine(String.Format("Geom {0}: {1}", index++, geom));

                    IGeometry buff = geom.Buffer(distance);
                    Assert.IsNotNull(buff);
                
                    foreach (double m in geom.GetOrdinates(Ordinate.M))
                    {                       
                        Assert.IsFalse(Double.IsNaN(m));
                    }
                }
            }
        }

        [Test]
        public void TestReadingShapeFileAfvalbakken()
        {
            IGeometryFactory factory = GeometryFactory.Default;
            List<IPolygon> polys = new List<IPolygon>();
            const int distance = 500;
            using (ShapefileDataReader reader = new ShapefileDataReader("afvalbakken", factory))
            {
                int index = 0;
                while (reader.Read())
                {                    
                    IGeometry geom = reader.Geometry;
                    Assert.IsNotNull(geom);
                    Assert.IsTrue(geom.IsValid);
                    Debug.WriteLine(String.Format("Geom {0}: {1}", index++, geom));
                    
                    IGeometry buff = geom.Buffer(distance);
                    Assert.IsNotNull(buff);

                    polys.Add((IPolygon) geom);
                }
            }

            IMultiPolygon multiPolygon = factory.CreateMultiPolygon(polys.ToArray());
            Assert.IsNotNull(multiPolygon);
            Assert.IsTrue(multiPolygon.IsValid);

            IMultiPolygon multiBuffer = (IMultiPolygon) multiPolygon.Buffer(distance);
            Assert.IsNotNull(multiBuffer);            
            Assert.IsTrue(multiBuffer.IsValid);

            ShapefileWriter writer = new ShapefileWriter(factory); 
            writer.Write(@"test_buffer", multiBuffer); 
            ShapefileWriter.WriteDummyDbf(@"test_buffer.dbf", multiBuffer.NumGeometries);        
        }

        [Test]
        public void TestSeptPolygones()
        {
            const string wktGeom9 =
                "MULTIPOLYGON ( "+
                   "((-73.8706030450129 45.425307895968558, -73.8691180248536 45.425712901466682, -73.862907940551338 45.425949154673731, -73.862739188260548 45.423181617104319, -73.864662964375952 45.423384119853267, -73.8654729753718 45.42220285381751, -73.865979232244342 45.421730347403241, -73.866822993698463 45.42088658594912, -73.866485489116826 45.420481580450996, -73.865202971706537 45.42041407953468, -73.864629213917681 45.421325341905117, -73.864156707503412 45.422236604275611, -73.863481698340081 45.422405356566514, -73.863414197423765 45.421899099693974, -73.863414197423765 45.421190340072485, -73.8635491992564 45.4200765749531, -73.864122957045254 45.419165312582606, -73.864797966208585 45.419064061208076, -73.866316736825922 45.419030310749974, -73.867092997363727 45.419266563957194, -73.867295500112789 45.419536567622515, -73.867396751487263 45.420751584116317, -73.867092997363727 45.421527844654122, -73.866384237742238 45.422506607941045, -73.866046733160658 45.423215367562364, -73.8669579955311 45.423721624434904, -73.868881771646556 45.423485371227684, -73.8694555294353 45.423417870311312, -73.8700630376822 45.423991628100168, -73.870434292722109 45.424497884972709, -73.8706030450129 45.425307895968558), "+
                    "(-73.86921927622808 45.425139143677825, -73.868983023020974 45.424464134514437, -73.868544267064863 45.423991628100168, -73.86813926156691 45.423991628100168, -73.867092997363727 45.423991628100168, -73.86533797353917 45.423620373060317, -73.864966718499375 45.424059129016484, -73.864966718499375 45.424497884972709, -73.865304223081068 45.42534164642683, -73.866451738658668 45.425409147343146, -73.86756550377811 45.425274145510514, -73.86921927622808 45.425139143677825), "+
                    "(-73.865937695291677 45.419884197388171, -73.865599517078863 45.419585804847259, -73.86432637557175 45.4198046260438, -73.864167232883347 45.4205605538138, -73.864565089604355 45.420500875305606, -73.865937695291677 45.419884197388171)), "+
                   "((-73.868038010192436 45.424869140012561, -73.866620490949458 45.424869140012561, -73.865844230411653 45.424970391386921, -73.865742979037179 45.42436288314002, -73.865979232244342 45.42402537855844, -73.866687991865717 45.424295382223704, -73.867869257901532 45.424396633598121, -73.868038010192436 45.424869140012561), "+
                    "(-73.86744733356926 45.424703767127937, -73.867371896498639 45.42446991220919, -73.867002254852821 45.424454824795021, -73.866232796733016 45.424432193673908, -73.866345952338861 45.4246509611786, -73.86744733356926 45.424703767127937)), "+
                   "((-73.86512208901371 45.419923983060187, -73.864604875276427 45.420301946945074, -73.8644059469159 45.420043340076518, -73.86512208901371 45.419923983060187)))";

            IGeometryFactory factory = GeometryFactory.Default; //new GeometryFactory(new PrecisionModel(Math.Pow(10, 13)));
            WKTReader wktReader = new WKTReader(factory);
            List<IGeometry> polys = new List<IGeometry>();
            using (ShapefileDataReader reader = new ShapefileDataReader("sept_polygones", factory))
            {
                int index = 0;
                while (reader.Read())
                {
                    IGeometry geom = reader.Geometry;
                    Assert.IsNotNull(geom);
                    Assert.IsTrue(geom.IsValid);
                    geom.Normalize();
                    Debug.WriteLine(String.Format("Geom {0}: {1}", ++index, geom));
                    polys.Add(geom);
                }
            }

            IGeometry expected = wktReader.Read(wktGeom9);
            expected.Normalize();

            Envelope e1 = expected.EnvelopeInternal;
            Envelope e2 = polys[8].EnvelopeInternal;
            Assert.IsTrue(e1.Equals(e2), string.Format("{0}\ndoes not match\n{1}", e1, e2));
            Assert.IsTrue(expected.EqualsTopologically(polys[8]), string.Format("{0}\ndoes not match\n{1}", expected, polys[8]));
        }
        
        [Test]
        // see https://code.google.com/p/nettopologysuite/issues/detail?id=167
        public void Issue167_EnsureAllBinaryContentIsReaded()
        {
            int i = 0;
            ShapefileReader reader = new ShapefileReader("Issue167.shp");
            foreach (IGeometry geom in reader)
            {
                Assert.That(geom, Is.Not.Null, "geom null");
                Console.WriteLine("geom {0}: {1}", ++i, geom);
            }
            Assert.That(i, Is.EqualTo(201));
        }
    }
}
