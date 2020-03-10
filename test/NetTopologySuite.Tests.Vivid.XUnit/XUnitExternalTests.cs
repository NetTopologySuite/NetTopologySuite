using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class ExternalXUnitRunner : XUnitRunner
    {
        protected ExternalXUnitRunner(string testFile) : base(testFile) { }

        protected override string TestLocation => "misc";
    }

    public class TestGeosBuffer : ExternalXUnitRunner
    {
        public TestGeosBuffer() : base("GEOSBuffer.xml") { }
    }

    public class TestBufferExternal : ExternalXUnitRunner
    {
        public TestBufferExternal() : base("TestBufferExternal.xml") { }

        [Test, Category("FailureCase")]
        public override void Test00()
        {
            base.Test00();
        }

        [Test, Category("FailureCase")]
        public override void Test01()
        {
            base.Test01();
        }
    }

    public class TestBufferExternal2 : ExternalXUnitRunner
    {
        public TestBufferExternal2() : base("TestBufferExternal2.xml") { }
    }

    public class TestOverlay : ExternalXUnitRunner
    {
        public TestOverlay() : base("TestOverlay.xml") { }

        [Test, Category("FailureCase")]
        public override void Test01()
        {
            base.Test01();
        }

        [Test, Category("FailureCase")]
        public override void Test02()
        {
            base.Test02();
        }

        [Test, Category("FailureCase")]
        public override void Test03()
        {
            base.Test03();
        }

        [Test, Category("FailureCase")]
        public override void Test04()
        {
            base.Test04();
        }
    }
}
