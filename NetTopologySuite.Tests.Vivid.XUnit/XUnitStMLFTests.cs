using System;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class StMLFXUnitRunner : XUnitRunner
    {
        protected StMLFXUnitRunner(string testFile) : base(testFile) { }

        private const String testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\stmlf";

        protected override string TestLocation { get { return testLocation; } }
    }

    public class TestStMLF20061020 : StMLFXUnitRunner
    {
        public TestStMLF20061020() : base("stmlf-cases-20061020_int.xml") { }
    }

    public class TestStMLF20061018 : StMLFXUnitRunner
    {
        public TestStMLF20061018() : base("stmlf-cases-20061018_int.xml") { }
    }
}