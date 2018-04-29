using System;
using System.Reflection;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Triangulate;
using NUnit.Framework;
using ProjNet.CoordinateSystems;
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
            var assembly = typeFromAssemblyToCheck.Assembly;
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
        public void ensure_NetTopologySuite_assembly_is_strongly_named()
        {
            AssertStronglyNamedAssembly(typeof(VoronoiDiagramBuilder));
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
    }
}
