using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests
{
    [TestFixture]
    public abstract class AbstractIOFixture
    {
        protected readonly RandomGeometryHelper RandomGeometryHelper;

        protected AbstractIOFixture()
            : this(GeometryFactory.Default)
        {
        }

        protected AbstractIOFixture(IGeometryFactory factory)
        {
            RandomGeometryHelper = new RandomGeometryHelper(factory);
        }

        private int _counter;

        public int Counter { get { return ++_counter; } }

        [TestFixtureSetUp]
        public virtual void OnFixtureSetUp()
        {
            try
            {
                CheckAppConfigPresent();
                CreateTestStore();
            }
            catch (Exception ex)
            {
                throw new IgnoreException("Fixture setup failed", ex);
            }
        }

        [TestFixtureTearDown]
        public virtual void OnFixtureTearDown() { }

        private void CheckAppConfigPresent()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NetTopologySuite.IO.Tests.dll.config");
            if (!File.Exists(path))
                CreateAppConfig();
            UpdateAppConfig();
            ReadAppConfig();
        }

        private void UpdateAppConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            KeyValueConfigurationCollection appSettings = config.AppSettings.Settings;
            AddAppConfigSpecificItems(appSettings);
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");

        }

        private void CreateAppConfig()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            KeyValueConfigurationCollection appSettings = config.AppSettings.Settings;

            appSettings.Add("PrecisionModel", "Floating");
            appSettings.Add("Ordinates", "XY");
            appSettings.Add("MinX", "-180");
            appSettings.Add("MaxX", "180");
            appSettings.Add("MinY", "-90");
            appSettings.Add("MaxY", "90");
            appSettings.Add("Srid", "4326");

            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        protected abstract void AddAppConfigSpecificItems(KeyValueConfigurationCollection kvcc);

        private void ReadAppConfig()
        {
            AppSettingsReader asr = new AppSettingsReader();
            SRID = (int)asr.GetValue("Srid", typeof(int));
            string pm = (string) asr.GetValue("PrecisionModel", typeof (string));
            int scale;
            PrecisionModel = int.TryParse(pm, out scale) 
                ? new PrecisionModel(scale) 
                : new PrecisionModel((PrecisionModels)Enum.Parse(typeof(PrecisionModels), pm));
            MinX = (double)asr.GetValue("MinX", typeof(double));
            MaxX = (double)asr.GetValue("MaxX", typeof(double));
            MinY = (double)asr.GetValue("MinY", typeof(double));
            MaxY = (double)asr.GetValue("MaxY", typeof(double));
            string ordinatesString = (string)asr.GetValue("Ordinates", typeof(string));
            Ordinates ordinates = (Ordinates)Enum.Parse(typeof(Ordinates), ordinatesString);
            RandomGeometryHelper.Ordinates = ordinates;
            ReadAppConfigInternal(asr);
        }

        protected virtual void ReadAppConfigInternal(AppSettingsReader asr) { }

        public string ConnectionString { get; protected set; }

        public int SRID
        {
            get
            {
                return RandomGeometryHelper.Factory.SRID;
            }
            protected set
            {
                PrecisionModel oldPM = new PrecisionModel();
                if (RandomGeometryHelper != null)
                    oldPM = (PrecisionModel)RandomGeometryHelper.Factory.PrecisionModel;
                Debug.Assert(RandomGeometryHelper != null, "RandomGeometryHelper != null");
                if (RandomGeometryHelper.Factory is OgcCompliantGeometryFactory)
                    RandomGeometryHelper.Factory = new OgcCompliantGeometryFactory(oldPM, value);
                else
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

                IGeometryFactory factory = RandomGeometryHelper.Factory;
                int oldSrid = factory != null ? factory.SRID : 0;
                ICoordinateSequenceFactory oldFactory = factory != null
                                     ? factory.CoordinateSequenceFactory
                                     : CoordinateArraySequenceFactory.Instance;
                
                if (RandomGeometryHelper.Factory is OgcCompliantGeometryFactory)
                    RandomGeometryHelper.Factory = new OgcCompliantGeometryFactory(value, oldSrid, oldFactory);
                else
                    RandomGeometryHelper.Factory = new GeometryFactory(value, oldSrid, oldFactory);
            }
        }

        public double MinX
        {
            get { return RandomGeometryHelper.MinX; }
            protected set { RandomGeometryHelper.MinX = value; }
        }

        public double MaxX
        {
            get { return RandomGeometryHelper.MaxX; }
            protected set { RandomGeometryHelper.MaxX = value; }
        }

        public double MinY
        {
            get { return RandomGeometryHelper.MinY; }
            protected set { RandomGeometryHelper.MinY = value; }
        }

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
                Debug.Assert((value & Ordinates.XY) == Ordinates.XY);
                RandomGeometryHelper.Ordinates = value;
            }
        }

        /// <summary>
        /// Function to create the test table and add some data
        /// </summary>
        protected abstract void CreateTestStore();

        public void PerformTest(IGeometry gIn)
        {
            WKTWriter writer = new WKTWriter(2) {EmitSRID = true, MaxCoordinatesPerLine = 3,};
            byte[] b = null;
            Assert.DoesNotThrow(() => b = Write(gIn), "Threw exception during write:\n{0}", writer.WriteFormatted(gIn));

            IGeometry gParsed = null;
            Assert.DoesNotThrow(() => gParsed = Read(b), "Threw exception during read:\n{0}", writer.WriteFormatted(gIn));

            Assert.IsNotNull(gParsed, "Could not be parsed\n{0}", gIn);
            CheckEquality(gIn, gParsed, writer);
        }

        protected virtual void CheckEquality(IGeometry gIn, IGeometry gParsed, WKTWriter writer)
        {
            Assert.IsTrue(gIn.EqualsExact(gParsed), "Instances are not equal\n{0}\n\n{1}", gIn, gParsed);
        }

        protected abstract IGeometry Read(byte[] b);

        protected abstract byte[] Write(IGeometry gIn);

        [Test]
        public virtual void TestPoint()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.Point);
        }
        [Test]
        public virtual void TestLineString()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.LineString);
        }
        [Test]
        public virtual void TestPolygon()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.Polygon);
        }
        [Test]
        public virtual void TestMultiPoint()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPoint);
        }
        [Test]
        public virtual void TestMultiLineString()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiLineString);
        }

        [Test]
        public virtual void TestMultiPolygon()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.MultiPolygon);
        }

        [Test]

        public virtual void TestGeometryCollection()
        {
            for (int i = 0; i < 5; i++)
                PerformTest(RandomGeometryHelper.GeometryCollection);
        }        
    }
}
