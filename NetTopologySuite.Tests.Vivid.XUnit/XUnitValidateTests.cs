using System;

namespace NetTopologySuite.Tests.XUnit
{
    public abstract class ValidateXUnitRunner : XUnitRunner
    {
        protected ValidateXUnitRunner(string testFile) : base(testFile) { }

        private const String testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\validate";

        protected override string TestLocation { get { return testLocation; } }
    }

    public class TestRelateAA : ValidateXUnitRunner
    {
        public TestRelateAA() : base("TestRelateAA.xml") { }
    }

    public class TestRelateAA_big : ValidateXUnitRunner
    {
        public TestRelateAA_big() : base("TestRelateAA-big.xml") { }
    }

    public class TestRelateAC : ValidateXUnitRunner
    {
        public TestRelateAC() : base("TestRelateAC.xml") { }
    }

    public class TestRelateLA : ValidateXUnitRunner
    {
        public TestRelateLA() : base("TestRelateLA.xml") { }
    }

    public class TestRelateLC : ValidateXUnitRunner
    {
        public TestRelateLC() : base("TestRelateLC.xml") { }
    }

    public class TestRelateLL : ValidateXUnitRunner
    {
        public TestRelateLL() : base("TestRelateLL.xml") { }
    }

    public class TestRelatePA_Vivid : ValidateXUnitRunner
    {
        public TestRelatePA_Vivid() : base("TestRelatePA.xml") { }
    }

    public class TestRelatePL_Vivid : ValidateXUnitRunner
    {
        public TestRelatePL_Vivid() : base("TestRelatePL.xml") { }
    }
    
    public class TestRelatePP_Vivid : ValidateXUnitRunner
    {
        public TestRelatePP_Vivid() : base("TestRelatePP.xml") { }
    }
}
