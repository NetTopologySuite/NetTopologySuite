namespace NetTopologySuite.Tests.XUnit
{
    using System;

    public abstract class RobustXUnitRunner : XUnitRunner
    {
        protected RobustXUnitRunner(string testFile) : base(testFile) { }

        private const String testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\robust";

        protected override string TestLocation { get { return testLocation; } }
    }

    // Same as JTS: 2 passed, 4 failed, 1 threw exceptions
    public class ExternalRobustness : RobustXUnitRunner
    {
        public ExternalRobustness() : base("ExternalRobustness.xml") { }
    }

    // Same as JTS: 0 passed, 0 failed, 1 threw exceptions
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