//namespace NetTopologySuite.Tests.XUnit
//{
//    using System;
    
//    [Obsolete("XML tests from old JTS versions")]
//    public abstract class VividXUnitRunner : XUnitRunner
//    {
//        protected VividXUnitRunner(string testFile) : base(testFile) { }

//        private const String testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\vivid";

//        protected override string TestLocation { get { return testLocation; } }
//    }

//    public class TestBoundary : VividXUnitRunner
//    {
//        public TestBoundary() : base("TestBoundary.xml") { }
//    }

//    public class TestCentroid : VividXUnitRunner
//    {
//        public TestCentroid() : base("TestCentroid.xml") { }
//    }

//    public class TestConvexHullBig : VividXUnitRunner
//    {
//        public TestConvexHullBig() : base("TestConvexHull-big.xml") { }
//    }

//    public class TestConvexHull : VividXUnitRunner
//    { 
//        public TestConvexHull() : base("TestConvexHull.xml") { }
//    }

//    public class TestFunctionAA : VividXUnitRunner
//    {
//        public TestFunctionAA() : base("TestFunctionAA.xml") { }
//    }

//    public class TestFunctionAAPrec : VividXUnitRunner
//    {
//        public TestFunctionAAPrec() : base("TestFunctionAAPrec.xml") { }
//    }


//    public class TestFunctionLA : VividXUnitRunner
//    {
//        public TestFunctionLA() : base("TestFunctionLA.xml") { }
//    }

//    public class TestFunctionLAPrec : VividXUnitRunner
//    {
//        public TestFunctionLAPrec() : base("TestFunctionLAPrec.xml") { }
//    }


//    public class TestFunctionLL : VividXUnitRunner
//    {
//        public TestFunctionLL() : base("TestFunctionLL.xml") { }
//    }

//    public class TestFunctionLLPrec : VividXUnitRunner
//    {
//        public TestFunctionLLPrec() : base("TestFunctionLLPrec.xml") { }
//    }

//    public class TestFunctionPA : VividXUnitRunner
//    {
//        public TestFunctionPA() : base("TestFunctionPA.xml") { }
//    }

//    public class TestFunctionPL : VividXUnitRunner
//    {
//        public TestFunctionPL() : base("TestFunctionPL.xml") { }
//    }

//    public class TestFunctionPLPrec : VividXUnitRunner
//    {
//        public TestFunctionPLPrec() : base("TestFunctionPLPrec.xml") { }
//    }

//    public class TestFunctionPP : VividXUnitRunner
//    {
//        public TestFunctionPP() : base("TestFunctionPP.xml") { }
//    }

//    public class TestInteriorPoint : VividXUnitRunner
//    {
//        public TestInteriorPoint() : base("TestInteriorPoint.xml") { } 
//    }

//    public class TestRectanglePredicate : VividXUnitRunner
//    {
//        public TestRectanglePredicate() : base("TestRectanglePredicate.xml") { }
//    }

//    public class TestRelateAA_Vivid : VividXUnitRunner
//    {
//        public TestRelateAA_Vivid() : base("TestRelateAA.xml") { }
//    }

//    public class TestRelateAC_Vivid : VividXUnitRunner
//    {
//        public TestRelateAC_Vivid() : base("TestRelateAC.xml") { }
//    }

//    public class TestRelateLA_Vivid : VividXUnitRunner
//    {
//        public TestRelateLA_Vivid() : base("TestRelateLA.xml") { }
//    }

//    public class TestRelateLC_Vivid : VividXUnitRunner
//    {
//        public TestRelateLC_Vivid() : base("TestRelateLC.xml") { }
//    }

//    public class TestRelateLL_Vivid : VividXUnitRunner
//    {
//        public TestRelateLL_Vivid() : base("TestRelateLL.xml") { }
//    }

//    public class TestRelatePA : VividXUnitRunner
//    {
//        public TestRelatePA() : base("TestRelatePA.xml") { }
//    }

//    public class TestRelatePL : VividXUnitRunner 
//    { 
//        public TestRelatePL() : base("TestRelatePL.xml") { }
//    }

//    public class TestRelatePP : VividXUnitRunner
//    {
//        public TestRelatePP() : base("TestRelatePP.xml") { }
//    }

//    public class TestSimple : VividXUnitRunner
//    {
//        public TestSimple() : base("TestSimple.xml") { }
//    }

//    public class TestValid : VividXUnitRunner
//    {
//        public TestValid() : base("TestValid.xml") { }
//    }

//    public class TestValid2Big : VividXUnitRunner
//    {
//        public TestValid2Big() : base("TestValid2-Big.xml") { }
//    }

//    public class TestValid2 : VividXUnitRunner
//    {
//        public TestValid2() : base("TestValid2.xml") { }
//    }

//    public class TestWithinDistance : VividXUnitRunner
//    {
//        public TestWithinDistance() : base("TestWithinDistance.xml") { }
//    }
//}