using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class FailureXUnitRunner : XUnitRunner
    {
        protected FailureXUnitRunner(string testFile) : base(testFile) { }

        protected override string TestLocation => "failure";
    }

    public class TestBigNastyBuffer : FailureXUnitRunner
    {
        public TestBigNastyBuffer() : base("TestBigNastyBuffer.xml") { }

        [Test, Category("FailureCase")]
        public override void Test00()
        {
            base.Test00();
        }
    }

    // 1 expected exception thrown
    public class TestOverlayFailure : FailureXUnitRunner
    {
        public TestOverlayFailure() : base("TestOverlay.xml") { }

        [Test, Category("FailureCase")]
        public override void Test00()
        {
            base.Test00();
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
