using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class MiscXUnitRunner : XUnitRunner
    {
        protected MiscXUnitRunner(string testFile) : base(testFile) { }

        protected override string TestLocation => "misc";
    }

    public class TestGeosBuffer : MiscXUnitRunner
    {
        public TestGeosBuffer() : base("GEOSBuffer.xml") { }
    }

    public class TestGeosBug356Buffer : MiscXUnitRunner
    {
        public TestGeosBug356Buffer() : base("geos-bug356-buffer.xml") { }
    }
    public class TestGeosBug838Union : MiscXUnitRunner
    {
        public TestGeosBug838Union() : base("geos-bug838-union.xml") { }
    }

    public class TestBufferExternal : MiscXUnitRunner
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

    public class TestBufferExternal2 : MiscXUnitRunner
    {
        public TestBufferExternal2() : base("TestBufferExternal2.xml") { }
    }

    public class TestBufferJagged : MiscXUnitRunner
    {
        public TestBufferJagged() : base("TestBufferJagged.xml") { }
    }

    public class TestInvalidA : MiscXUnitRunner
    {
        public TestInvalidA() : base("TestInvalidA.xml") { }
    }

    public class TestOverlay : MiscXUnitRunner
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

    public class TestValid : MiscXUnitRunner
    {
        public TestValid() : base("TestValid.xml") { }
    }
}
