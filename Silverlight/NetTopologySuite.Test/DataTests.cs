using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Resources;
using System.Windows;
using GeoAPI.Geometries;
using NetTopologySuite.Data;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GeoTools;
using NetTopologySuite.Shapefile;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
namespace NetTopologySuite.Silverlight.Test
{
    [TestClass]
    public class DataTests
    {
        private const string _testPolygon =
            @"POLYGON((28.7597599029541 -16.537670135498047,28.761909484863281 -16.534400939941406,28.850549697875977 -16.399730682373047,28.858329772949219 -16.0625,28.927219390869141 -15.972220420837402,29.138050079345703 -15.859999656677246,29.319160461425781 -15.74860954284668,29.831380844116211 -15.616109848022461,30.378049850463867 -15.65056037902832,30.41387939453125 -15.633609771728516,30.422769546508789 -16.009170532226562,30.928600311279297 -16.002229690551758,30.984710693359375 -16.059169769287109,31.058050155639648 -16.023059844970703,31.300830841064453 -16.028619766235352,31.423879623413086 -16.161109924316406,31.760549545288086 -16.239999771118164,31.908050537109375 -16.4183292388916,32.062759399414062 -16.449169158935547,32.240261077880859 -16.43889045715332,32.655258178710938 -16.581390380859375,32.708320617675781 -16.607780456542969,32.707759857177734 -16.684169769287109,32.768501281738281 -16.717819213867188,32.982200622558594 -16.708610534667969,32.943870544433594 -16.832780838012695,32.842479705810547 -16.931110382080078,32.968318939208984 -17.147499084472656,33.044921875 -17.346260070800781,32.960269927978516 -17.520839691162109,33.043319702148438 -17.613889694213867,33.018589019775391 -17.723339080810547,32.966091156005859 -17.842229843139648,32.974700927734375 -17.923610687255859,32.946090698242188 -17.975000381469727,32.975811004638672 -18.101390838623047,33.0010986328125 -18.183330535888672,32.966648101806641 -18.23583984375,33.073040008544922 -18.34889030456543,33.001438140869141 -18.405029296875,33.013599395751953 -18.466949462890625,32.888320922851562 -18.530559539794922,32.949710845947266 -18.690280914306641,32.92803955078125 -18.767229080200195,32.899990081787109 -18.791109085083008,32.817211151123047 -18.7791690826416,32.701099395751953 -18.836950302124023,32.720260620117188 -18.882780075073242,32.699150085449219 -18.944450378417969,32.716091156005859 -19.021949768066406,32.845260620117188 -19.037229537963867,32.852210998535156 -19.286670684814453,32.784709930419922 -19.366390228271484,32.783039093017578 -19.4677791595459,32.850540161132812 -19.4938907623291,32.845539093017578 -19.684999465942383,32.912761688232422 -19.6924991607666,32.9536018371582 -19.648609161376953,32.978588104248047 -19.664169311523438,32.975811004638672 -19.736669540405273,33.059429168701172 -19.7802791595459,33.039150238037109 -19.81389045715332,33.026649475097656 -20.031120300292969,32.953041076660156 -20.03639030456543,32.901088714599609 -20.134729385375977,32.665821075439453 -20.557229995727539,32.550819396972656 -20.555000305175781,32.5022087097168 -20.598609924316406,32.483600616455078 -20.661670684814453,32.521369934082031 -20.914169311523438,32.360271453857422 -21.135839462280273,32.492168426513672 -21.346460342407227,32.410259246826172 -21.311119079589844,31.393329620361328 -22.354169845581055,31.297630310058594 -22.416139602661133,31.155830383300781 -22.321670532226562,30.866939544677734 -22.296119689941406,30.294990539550781 -22.343339920043945,30.231100082397461 -22.2922306060791,30.1330509185791 -22.300559997558594,29.767490386962891 -22.136110305786133,29.450820922851562 -22.163339614868164,29.370529174804688 -22.191379547119141,29.258150100708008 -22.066520690917969,29.156660079956055 -22.076950073242188,29.073879241943359 -22.03778076171875,29.037220001220703 -21.974170684814453,29.037769317626953 -21.894729614257812,29.083339691162109 -21.82512092590332,29.080829620361328 -21.820840835571289,28.568050384521484 -21.6311092376709,28.491939544677734 -21.660829544067383,28.357219696044922 -21.603059768676758,28.202770233154297 -21.596670150756836,28.01276969909668 -21.56195068359375,27.908599853515625 -21.31389045715332,27.739429473876953 -21.145559310913086,27.685270309448242 -21.063619613647461,27.719440460205078 -20.81195068359375,27.700820922851562 -20.606670379638672,27.724710464477539 -20.5674991607666,27.713970184326172 -20.509859085083008,27.357219696044922 -20.465000152587891,27.287490844726562 -20.494720458984375,27.297210693359375 -20.302509307861328,27.219989776611328 -20.0916690826416,27.026660919189453 -20.000280380249023,26.969709396362305 -20.009729385375977,26.610820770263672 -19.854450225830078,26.588600158691406 -19.799169540405273,26.441099166870117 -19.731109619140625,26.404439926147461 -19.675830841064453,26.325820922851562 -19.654449462890625,26.348049163818359 -19.597230911254883,26.191940307617188 -19.544729232788086,25.961940765380859 -19.10028076171875,25.973320007324219 -18.945560455322266,25.808330535888672 -18.776950836181641,25.797489166259766 -18.681669235229492,25.659439086914062 -18.531120300292969,25.393329620361328 -18.12251091003418,25.309160232543945 -18.065839767456055,25.236660003662109 -17.894449234008789,25.265750885009766 -17.797660827636719,25.391939163208008 -17.850839614868164,25.599720001220703 -17.841390609741211,25.719989776611328 -17.836389541625977,25.854160308837891 -17.908620834350586,25.859710693359375 -17.973339080810547,25.971660614013672 -18.006389617919922,26.214990615844727 -17.884170532226562,26.314720153808594 -17.935840606689453,26.462770462036133 -17.968610763549805,26.689159393310547 -18.075279235839844,27.020769119262695 -17.964179992675781,27.031410217285156 -17.9554500579834,27.1224308013916 -17.880760192871094,27.146219253540039 -17.845279693603516,27.145820617675781 -17.8044490814209,27.619159698486328 -17.337230682373047,27.638879776000977 -17.224720001220703,27.82526969909668 -16.959169387817383,28.138599395751953 -16.823619842529297,28.259990692138672 -16.724170684814453,28.7597599029541 -16.537670135498047))";

        [TestMethod]
        public void TestValueFactory()
        {
            IValueFactory factory = new ValueFactory();
            if (!factory.HasConverter(typeof(string), typeof(IGeometry)))
                factory.AddConverter(new StringToGeometryConverter());

            Assert.IsInstanceOfType(factory.CreateValue(typeof(int), 1), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<int>((object)1), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<int>("1"), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<double>((object)1), typeof(IValue<double>));

            Assert.IsInstanceOfType(factory.CreateValue<IGeometry>(_testPolygon), typeof(IValue<IGeometry>));
        }

        [TestMethod]
        public void CreateManyValuesKnownType()
        {
            IValueFactory vf = new ValueFactory();
            for (int i = 0; i < 1000000; i++)
            {
                var v = vf.CreateValue<int>(i);
            }
        }

        [TestMethod]
        public void CreateManyValuesForcedType()
        {
            IValueFactory vf = new ValueFactory();
            for (int i = 0; i < 1000000; i++)
            {
                var v = vf.CreateValue(typeof(int), i);
            }
        }

        [TestMethod]
        public void CreateManyValuesConvertedType()
        {
            IValueFactory vf = new ValueFactory();
            for (int i = 0; i < 1000000; i++)
            {
                var v = vf.CreateValue(typeof(int), i.ToString());
            }
        }

        //static void EnsureFile(string fileName, string sourceUri)
        //{
        //    using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
        //    {
        //        if (!isf.FileExists(fileName))
        //        {
        //            using (var f = isf.CreateFile(fileName))
        //            {
        //                using (var res = Application.GetResourceStream(new Uri(sourceUri, UriKind.Relative)).Stream)
        //                {
        //                    if (isf.AvailableFreeSpace < res.Length)
        //                    {
        //                        if (!isf.IncreaseQuotaTo(isf.Quota + 10000000))
        //                            throw new IsolatedStorageException("Isolated storage file quota needs increasing.");
        //                        //the test framework doesn't show the dialog box. todo: needs workaround.

        //                    }

        //                    res.CopyTo(f);
        //                }
        //                f.Flush();
        //                f.Close();
        //            }
        //        }
        //    }
        //}

        //public void EnsureFilesExistInIsolatedStorage()
        //{
        //    EnsureFile("world.shp", "world.shp");
        //    EnsureFile("world.dbf", "world.dbf");
        //    EnsureFile("world.shx", "world.shx");
        //}

        [Ignore]
        [TestMethod]
        public void TestShapefile()
        {
            //EnsureFilesExistInIsolatedStorage();

            IMemoryRecordSet memoryRecordSet =
                IO.Shapefile.CreateDataReader(@"world.shp", new GeometryFactory()).ToInMemorySet();


            IGeometry geometry = new WKTReader().Read(_testPolygon);

            memoryRecordSet.Where(a => a.GetId<uint>() == 1
                && a.GetValue<IGeometry>("Geom").Intersects(geometry));

        }


        [TestMethod]
        public void TestShapefileProvider()
        {

            Debug.WriteLine("**************TestShapefileProvider******************");
            //EnsureFilesExistInIsolatedStorage();
            using (ShapeFileProvider spf = new ShapeFileProvider("world.shp", new GeometryFactory(), new ResourceStorageManager(), new SchemaFactory(new PropertyInfoFactory(new ValueFactory()))))
            {
                spf.Open();
                foreach (var v in spf.GetAllFeatues())
                {
                    Debug.WriteLine(v.ToString());
                }
            }
            Debug.WriteLine("**************End TestShapefileProvider******************");

        }


        [TestMethod]
        public void GetRecordByOID1()
        {
            Debug.WriteLine("**************GetRecordByOID******************");

            //EnsureFilesExistInIsolatedStorage();
            using (ShapeFileProvider spf = new ShapeFileProvider("world.shp", new GeometryFactory(), new ResourceStorageManager(), new SchemaFactory(new PropertyInfoFactory(new ValueFactory()))))
            {
                spf.Open();
                IRecord record = spf.GetAllFeatues().First(a => a.GetId<uint>() == 1);
                Debug.WriteLine(record);
                Assert.AreEqual(record.GetId<uint>(), (uint)1);
            }
            Debug.WriteLine("**************End GetRecordByOID******************");

        }


        [TestMethod]
        public void GetRecordByIntersection()
        {
            Debug.WriteLine("**************GetRecordByIntersection******************");

            IGeometry geometry = new WKTReader().Read(_testPolygon);

            //EnsureFilesExistInIsolatedStorage();
            using (ShapeFileProvider spf = new ShapeFileProvider("world.shp",
                                                                new GeometryFactory(),
                                                                new ResourceStorageManager(),
                                                                new SchemaFactory(
                                                                        new PropertyInfoFactory(
                                                                                new ValueFactory()))))
            {
                spf.Open();
                spf.GetAllFeatues()
                   .Where(a => a.GetValue<IGeometry>("Geom")
                   .Intersects(geometry))
                   .ToList()
                   .ForEach(a => Debug.WriteLine(a));
            }

            Debug.WriteLine("**************End GetRecordbyIntersection******************");

        }

        [TestMethod]
        public void GetRecordByOID2()
        {
            Debug.WriteLine("**************GetRecordByOID******************");

            IGeometry geometry = new WKTReader().Read(_testPolygon);

            //EnsureFilesExistInIsolatedStorage();
            using (ShapeFileProvider spf = new ShapeFileProvider("world.shp", new GeometryFactory(), new ResourceStorageManager(), new SchemaFactory(new PropertyInfoFactory(new ValueFactory()))))
            {
                spf.Open();
                IRecord rec = spf.GetFeatureByOid(3);
                Debug.WriteLine(rec.ToString());
                Assert.AreEqual(rec.GetId<uint>(), (uint)3);
            }

            Debug.WriteLine("**************End GetRecordByOID******************");

        }

        /* 
         * This class has been removed from the source
         * 
        [Ignore]
        [TestMethod]
        public void TestMyShapeReader()
        {
            //currently my shapefile reader internally uses IsolatedStorage perhaps modify to use IStorageManager  or delete completely as it isn't particularly good/featureful

            //EnsureFilesExistInIsolatedStorage();
            using (MyShapeFileReader msfr = new MyShapeFileReader())
            {
                IGeometryCollection geoms = msfr.Read("world.shp");
                Assert.IsNotNull(geoms);
                Assert.IsTrue(geoms.Count > 0);
                foreach (var geom in geoms)
                {
                    Trace.WriteLine(geom.ToString());
                }
            }
        }
         */
    }
}