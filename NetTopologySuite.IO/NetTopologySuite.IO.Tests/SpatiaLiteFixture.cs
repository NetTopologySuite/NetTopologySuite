using System;

namespace NetTopologySuite.IO.Tests
{
    using System.Configuration;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using GeoAPI.Geometries;
    using NUnit.Framework;

    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class SpatiaLiteFixture : AbstractIOFixture
    {
        public override void OnFixtureSetUp()
        {
            base.OnFixtureSetUp();
            Ordinates = Ordinates.XY;
            Compressed = false;
        }

        public bool HasZ
        {
            get { return (this.Ordinates & Ordinates.Z) == Ordinates.Z; }
        }

        public bool HasM
        {
            get { return (this.Ordinates & Ordinates.M) == Ordinates.M; }
        }

        public bool Compressed { get; set; }

        protected virtual string Name { get { return "SpatiaLite.sqlite"; } }

        protected override void AddAppConfigSpecificItems(KeyValueConfigurationCollection kvcc)
        {
            //kvcc.Add("SpatiaLiteCompressed", "false");
        }

        protected override void ReadAppConfigInternal(AppSettingsReader asr)
        {
            //this.Compressed = (bool)asr.GetValue("SpatiaLiteCompressed", typeof(bool));
        }

        protected override void CreateTestStore()
        {
            if (File.Exists(Name))
                File.Delete(Name);

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=\"" + Name + "\""))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        "CREATE TABLE \"nts_io_spatialite\" (id int primary key, wkt text, the_geom blob);";
                    cmd.ExecuteNonQuery();
                }
            }
        }

        protected override void CheckEquality(IGeometry gIn, IGeometry gParsed, WKTWriter writer)
        {
            var res = gIn.EqualsExact(gParsed);
            if (res) return;

            if (Compressed)
            {
                var discreteHausdorffDistance =
                    Algorithm.Distance.DiscreteHausdorffDistance.Distance(gIn, gParsed);
                if (discreteHausdorffDistance > 0.05)
                {
                    Console.WriteLine();
                    Console.WriteLine(gIn.AsText());
                    Console.WriteLine(gParsed.AsText());
                    Console.WriteLine("DiscreteHausdorffDistance=" + discreteHausdorffDistance);
                }
                Assert.IsTrue(discreteHausdorffDistance < 0.001);
            }
            else
                Assert.IsTrue(false);
        }

        protected override IGeometry Read(byte[] b)
        {
            return new GaiaGeoReader().Read(b);
        }

        protected override byte[] Write(IGeometry gIn)
        {
            var writer = new GaiaGeoWriter();
            writer.HandleOrdinates = Ordinates;
            writer.UseCompressed = Compressed;

            var b = writer.Write(gIn);
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=\"" + Name + "\""))
            {
                conn.Open();
                using (SQLiteCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO \"nts_io_spatialite\" VALUES(@P1, @P3, @P2);";
                    SQLiteParameter p1 = new SQLiteParameter("P1", DbType.Int32) { Value = this.Counter };
                    SQLiteParameter p2 = new SQLiteParameter("P2", DbType.String) { Value = gIn.AsText() };
                    SQLiteParameter p3 = new SQLiteParameter("P3", DbType.Binary) { Value = b };
                    cmd.Parameters.AddRange(new[] { p1, p2, p3 });
                    cmd.ExecuteNonQuery();
                }
            }
            return b;
        }
    }

    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class SpatiaLiteFixtureCompressed : SpatiaLiteFixture
    {
        protected override string Name { get { return "SpatiaLiteCompressed.sqlite"; } }

        public override void OnFixtureSetUp()
        {
            base.OnFixtureSetUp();
            Compressed = true;
        }
    }

    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class SpatiaLiteFixture3D : SpatiaLiteFixture
    {
        protected override string Name { get { return "SpatiaLite3D.sqlite"; } }

        public override void OnFixtureSetUp()
        {
            base.OnFixtureSetUp();
            Ordinates = Ordinates.XYZ;
        }
    }

    [NUnit.Framework.TestFixture]
    [NUnit.Framework.Category("Database.IO")]
    public class SpatiaLiteFixture3DCompressed : SpatiaLiteFixture3D
    {
        protected override string Name { get { return "SpatiaLite3DCompressed.sqlite"; } }

        public override void OnFixtureSetUp()
        {
            base.OnFixtureSetUp();
            Compressed = true;
        }
    }
}