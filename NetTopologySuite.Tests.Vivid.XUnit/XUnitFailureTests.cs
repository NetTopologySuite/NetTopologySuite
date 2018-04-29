using System;
namespace NetTopologySuite.Tests.XUnit
{
    public abstract class FailureXUnitRunner : XUnitRunner
    {
        protected FailureXUnitRunner(string testFile) : base(testFile) { }
        private const string testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\failure";
        protected override string TestLocation => testLocation;
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
