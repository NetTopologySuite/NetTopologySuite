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

    public class TestBufferFailure : FailureXUnitRunner
    {
        public TestBufferFailure() : base("TestBufferFailure.xml") { }
    }

    public class TestBufferInsideNonEmpty : FailureXUnitRunner
    {
        public TestBufferInsideNonEmpty() : base("TestBufferInsideNonEmpty.xml") { }
    }

    public class TestReducePrecisionFailure : FailureXUnitRunner
    {
        public TestReducePrecisionFailure() : base("TestReducePrecisionFailure.xml") { }
    }
}
