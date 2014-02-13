using System;
using System.Reflection;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Utilities;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
using Wintellect.PowerCollections;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Samples.Tests.Various
{
    // see https://code.google.com/p/nettopologysuite/issues/detail?id=174
    [TestFixture]
    public class Issue174TestFixture
    {
        private void AssertStronglyNamedAssembly(Type typeFromAssemblyToCheck)
        {
            Assert.IsNotNull(typeFromAssemblyToCheck, "Cannot determine assembly from null");
            Assembly assembly = typeFromAssemblyToCheck.Assembly;
            StringAssert.DoesNotContain("PublicKeyToken=null", assembly.FullName, "Strongly named assembly should have a PublicKeyToken in fully qualified name");
        }

        [Test, Category("Issue174")]
        public void ensure_GeoAPI_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(IGeometry));
        }

        [Test, Category("Issue174")]
        public void ensure_ProjNet_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(Datum));
        }

        [Test, Category("Issue174")]
        public void ensure_PowerCollections_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(OrderedSet<object>));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(VoronoiDiagramBuilder));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_GDB_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(GDBReader));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_GeoJSON_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(GeoJsonSerializer));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_GeoTools_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(GeoToolsStreamTokenizer));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_MsSqlSpatial_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(MsSqlSpatialReader));
        }

        //// Problem with Oracle.DataAccess...?
        ////
        //// Warning	1	There was a mismatch between the processor architecture of the project
        //// being built "MSIL" and the processor architecture of the reference "Oracle.DataAccess,
        //// Version=2.112.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=x86",
        //// "AMD64". This mismatch may cause runtime failures. Please consider changing the targeted
        //// processor architecture of your project through the Configuration Manager so as to align
        //// the processor architectures between your project and references, or take a dependency on
        //// references with a processor architecture that matches the targeted processor architecture
        //// of your project.	NetTopologySuite.IO.Oracle"
        //
        //[Test, Category("Issue174")]
        //public void ensure_NetTopologySuite_IO_Oracle_assembly_is_strongly_named()
        //{
        //  AssertStronglyNamedAssembly(typeof(NetTopologySuite.IO.OracleGeometryReader));
        //}

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_PostGis_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(PostGisReader));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_ShapeFile_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(ShapeReader));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_SpatialLite_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(GaiaGeoReader));
        }

        [Test, Category("Issue174")]
        public void ensure_NetTopologySuite_IO_SqlServer2008_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(MsSql2008GeometryReader));
        }

    }
}
