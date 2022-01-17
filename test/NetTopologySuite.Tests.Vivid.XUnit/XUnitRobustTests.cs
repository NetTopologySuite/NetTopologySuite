using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class RobustXUnitRunner : XUnitRunner
    {
        protected RobustXUnitRunner(string testFile) : base(testFile) { }

        protected override string TestLocation => "robust";
    }

    // 1 expected exception thrown
    public class TestMagnifyTopology : RobustXUnitRunner
    {
        public TestMagnifyTopology() : base("MagnifyTopology.xml") { }
    }

    public class TestBufferJagged2 : RobustXUnitRunner
    {
        public TestBufferJagged2() : base("TestBufferJagged.xml") { }
    }

    public class TestRobustBuffer : RobustXUnitRunner
    {
        public TestRobustBuffer() : base("TestRobustBuffer.xml") { }
    }

    public class TestRobustRelate : RobustXUnitRunner
    {
        public TestRobustRelate() : base("TestRobustRelate.xml") { }
    }

    public class TestRobustRelateFloat : RobustXUnitRunner
    {
        public TestRobustRelateFloat() : base("TestRobustRelateFloat.xml") { }
    }
}
