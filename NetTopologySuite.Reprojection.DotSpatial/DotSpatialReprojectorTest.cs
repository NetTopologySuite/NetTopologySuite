
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DotSpatial.Projections;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Reprojection
{
    [TestFixture((string) null)]
    [TestFixture("esriwkt")]
    [TestFixture("proj4")]
    public class OsrReprojectorTest
    {
        private readonly ConsoleTraceListener _ctr = new ConsoleTraceListener();

        public OsrReprojectorTest(string definitionKind)
        {
            Reprojector.Instance = new DotSpatialReprojector(definitionKind);
        }

        [OneTimeSetUp]
        public void SetUp()
        {
            Trace.Listeners.Add(_ctr);
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            Trace.Listeners.Remove(_ctr);
            _ctr.Dispose();
        }

        [Test]
        public void Test()
        {
            var r = Reprojector.Instance;
            var srFrom = r.SpatialReferenceFactory.GetSpatialReference(4326);
            var srTo = r.SpatialReferenceFactory.GetSpatialReference(25832);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(6, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srFrom);
            Console.WriteLine(pt3);

        }

        [Test]
        public void TestOrig()
        {
            var r = Reprojector.Instance = new Reprojector();

            var srFrom = r.SpatialReferenceFactory.GetSpatialReference(4326);
            var srTo = r.SpatialReferenceFactory.GetSpatialReference(31466);

            var pt1 = srFrom.Factory.CreatePoint(new Coordinate(0, 0));
            Console.WriteLine(pt1);
            var pt2 = r.Reproject(pt1, srTo);
            Console.WriteLine(pt2);
            var pt3 = r.Reproject(pt2, srFrom);
            Console.WriteLine(pt3);

        }

        [TestCase(25832)]
        [TestCase(31466)]
        public void TestSpatialReferenceFactory(int srid)
        {
            var srf1 = new EpsgIoSpatialReferenceFactory(DotSpatialAffineCoordinateSequenceFactory.Instance, new PrecisionModel(), "proj4");
            var srf2 = new EpsgIoSpatialReferenceFactory(DotSpatialAffineCoordinateSequenceFactory.Instance, new PrecisionModel(), "esriwkt");
            var sr1 = srf1.GetSpatialReference(srid);
            var sr2 = srf2.GetSpatialReference(srid);
            Console.WriteLine(sr1.Definition);
            Console.WriteLine(ProjectionInfo.FromProj4String(sr1.Definition).ToProj4String());
            Console.WriteLine(ProjectionInfo.FromEsriString(sr2.Definition).ToProj4String());
            Console.WriteLine(DotSpatial.Projections.KnownCoordinateSystems.Projected.NationalGrids.DHDN3DegreeGaussZone2.ToProj4String());
            
        }
    }
}
