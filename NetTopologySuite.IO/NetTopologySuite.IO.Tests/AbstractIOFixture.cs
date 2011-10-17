using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.Tests
{
    [TestFixture]
    public abstract class AbstractIOFixture //<TConnection> 
    //    where TConnection : DbConnection
    {
        protected readonly RandomGeometryHelper RandomGeometryHelper = new RandomGeometryHelper(new GeometryFactory());

        private int _counter;

        public int Counter { get { return ++_counter; } }

        [TestFixtureSetUp]
        public virtual void OnFixtureSetUp()
        {
            CheckAppConfigPresent();
            CreateTestStore();
        }

        private void CheckAppConfigPresent()
        {
            var basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NetTopologySuite.IO.Tests.dll.config");
            if (!File.Exists(basePath))
                CreateAppConfig();
            ReadAppConfig();
        }

        private void CreateAppConfig()
        {
            var throwException = false;

            Configuration config =
                  ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var appSettings = config.AppSettings.Settings;

            appSettings.Add("PrecisionModel", "Floating");
            appSettings.Add("Ordinates", "XY");
            appSettings.Add("MinX", "-180");
            appSettings.Add("MaxX", "180");
            appSettings.Add("MinY", "-90");
            appSettings.Add("MaxY", "90");
            appSettings.Add("Srid", "4326");

            throwException = CreateAppConfigInternal(appSettings);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings;");

            if (throwException)
                throw new ApplicationException("App.config was not present, created on, fill in the blanks!");
            /*
            using (var s = new FileStream(file, FileMode.CreateNew, FileAccess.Write))
            {
                using (var sw = new StreamWriter(s))
                {
                    sw.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8"" ?>");
                    sw.WriteLine("<configuration>");
                    sw.WriteLine("<appSettings>");
                    sw.WriteLine("<add key=\"PrecisionModel\" value=\"Floating\"/>");
                    //sw.WriteLine("<add key\"Ordinates\" value=\"XY\"/>");
                    sw.WriteLine("<add key=\"MinX\" value=\"-180\"/>");
                    sw.WriteLine("<add key=\"MaxX\" value=\"180\"/>");
                    sw.WriteLine("<add key=\"MinY\" value=\"-90\"/>");
                    sw.WriteLine("<add key=\"MaxY\" value=\"90\"/>");
                    sw.WriteLine("<add key=\"Srid\" value=\"4326\"/>");
                    throwException = CreateAppConfigInternal(sw);
                    sw.WriteLine("</appSettings>\n</configuration>");
                }
            }
            if (throwException)
                throw new ApplicationException("App.config was not present, created on, fill in the blanks!");
             */
        }

        protected virtual bool CreateAppConfigInternal(KeyValueConfigurationCollection kvcc)
        {
            return true;
        }

        private void ReadAppConfig()
        {
            var asr = new AppSettingsReader();
            //ConnectionString = (string)asr.GetValue("ConnectionString", typeof (string));
            SRID = (int) asr.GetValue("Srid", typeof (int));
            PrecisionModel = new PrecisionModel((PrecisionModels)Enum.Parse(typeof(PrecisionModels), (string)asr.GetValue("PrecisionModel", typeof(string))));
            MinX = (double)asr.GetValue("MinX", typeof (double));
            MaxX = (double)asr.GetValue("MaxX", typeof(double));
            MinY = (double)asr.GetValue("MinY", typeof(double));
            MaxY = (double)asr.GetValue("MaxY", typeof(double));
            var ordinatesString = (string) asr.GetValue("Ordinates", typeof (string));
            var ordinates = (Ordinates) Enum.Parse(typeof (Ordinates), ordinatesString);
            RandomGeometryHelper.Ordinates = ordinates;
            
            ReadAppConfigInternal(asr);
        }

        protected virtual void ReadAppConfigInternal(AppSettingsReader asr)
        {}

        /// <summary>
        /// Gets the connection string
        /// </summary>
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Gets the spatial reference ID (Srid)
        /// </summary>
        public int SRID
        {
            get { return RandomGeometryHelper.Factory.SRID; }
            protected set 
            {
                var oldPM = new PrecisionModel();
                if (RandomGeometryHelper != null)
                    oldPM = (PrecisionModel)RandomGeometryHelper.Factory.PrecisionModel;
                Debug.Assert(RandomGeometryHelper != null, "RandomGeometryHelper != null");
                RandomGeometryHelper.Factory = new GeometryFactory(oldPM, value);
            }
        }

        public PrecisionModel PrecisionModel
        {
            get
            {
                return (PrecisionModel)RandomGeometryHelper.Factory.PrecisionModel;
            }
            protected set
            {
                if (value == null)
                    return;

                if (value == PrecisionModel)
                    return;

                RandomGeometryHelper.Factory = new GeometryFactory(value);
            }
        }

        /// <summary>
        /// Gets the min x-ordinate value for the <see cref="RandomGeometryHelper"/> function
        /// </summary>
        public double MinX 
        { 
            get { return RandomGeometryHelper.MinX; }
            protected set { RandomGeometryHelper.MinX = value; }
        }
        /// <summary>
        /// Gets the max x-ordinate value for the <see cref="RandomGeometryHelper.CreateCoordinate"/> function
        /// </summary>
        public double MaxX
        {
            get { return RandomGeometryHelper.MaxX; }
            protected set { RandomGeometryHelper.MaxX = value; }
        }

        /// <summary>
        /// Gets the min y-ordinate value for the <see cref="RandomGeometryHelper.CreateCoordinate"/> function
        /// </summary>
        public double MinY
        {
            get { return RandomGeometryHelper.MinY; }
            protected set { RandomGeometryHelper.MinY= value; }
        }

        /// <summary>
        /// Gets the max y-ordinate value for the <see cref="RandomGeometryHelper.CreateCoordinate"/> function
        /// </summary>
        public double MaxY
        {
            get { return RandomGeometryHelper.MaxY; }
            protected set { RandomGeometryHelper.MaxY = value; }
        }

        public Ordinates Ordinates
        {
            get { return RandomGeometryHelper.Ordinates; }
            set
            {
                System.Diagnostics.Debug.Assert((value & Ordinates.XY) == Ordinates.XY);
                RandomGeometryHelper.Ordinates = value;
            }
        }

        /// <summary>
        /// Function to create the test table and add some data
        /// </summary>
        protected abstract void CreateTestStore();

        public void PerformTest(IGeometry gIn)
        {
            var b = Write(gIn);
            var gParsed = Read(b);

            Assert.IsNotNull(gParsed);
            Assert.IsTrue(gIn.EqualsExact(gParsed));
            
        }

        protected abstract IGeometry Read(byte[] b);

        protected abstract byte[] Write(IGeometry gIn);

        [Test]
        public void TestPoint()
        {
            for (var i = 0; i < 5; i++ )
                PerformTest(RandomGeometryHelper.Point);
        }
        [Test]
        public void TestLineString()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.LineString);
        }
        [Test]
        public void TestPolygon()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.Polygon);
        }
        [Test]
        public void TestMultiPoint()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPoint);
        }
        [Test]
        public void TestMultiLineString()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiLineString);
        }

        [Test]
        public void TestMultiPolygon()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPolygon);
        }

        [Test]
        public void TestGeometryCollection()
        {
            for (var i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.GeometryCollection);
        }

        [TestFixtureTearDown]
        public virtual void OnFixtureTearDown()
        { }

    }
}
