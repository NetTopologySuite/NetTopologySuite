namespace NetTopologySuite.Tests.XUnit
{
    public abstract class RobustXUnitRunner : XUnitRunner
    {
        protected RobustXUnitRunner(string testFile) : base(testFile) { }

        protected override string TestLocation => "robust";
    }

    // 1 expected exception thrown
    public class ExternalRobustness : RobustXUnitRunner
    {
        public ExternalRobustness() : base("ExternalRobustness.xml") { }
    }

    // 1 expected exception thrown
    public class TestRobustOverlayError : RobustXUnitRunner
    {
        public TestRobustOverlayError() : base("TestRobustOverlayError.xml") { }
    }

    public class TestRobustOverlayFixed : RobustXUnitRunner
    {
        public TestRobustOverlayFixed() : base("TestRobustOverlayFixed.xml") { }
    }

    public class TestRobustOverlayFloat : RobustXUnitRunner
    {
        public TestRobustOverlayFloat() : base("TestRobustOverlayFloat.xml") { }
    }

    public class TestRobustRelate : RobustXUnitRunner
    {
        public TestRobustRelate() : base("TestRobustRelate.xml") { }
    }
}
