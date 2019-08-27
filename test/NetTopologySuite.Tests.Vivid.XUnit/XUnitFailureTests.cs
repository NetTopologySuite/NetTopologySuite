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
    }

    // 1 expected exception thrown
    public class TestOverlayFailure : FailureXUnitRunner
    {
        public TestOverlayFailure() : base("TestOverlay.xml") { }
    }
}
