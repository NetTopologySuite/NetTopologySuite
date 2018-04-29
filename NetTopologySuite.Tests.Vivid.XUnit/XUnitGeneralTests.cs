using System;
using System.IO;

namespace NetTopologySuite.Tests.XUnit
{
    public class GeneralXUnitRunner : XUnitRunner
    {
        //protected GeneralXUnitRunner(string testFile) : base(testFile) { }

        private const String testLocation = "..\\..\\..\\NetTopologySuite.TestRunner.Tests\\general";

        public GeneralXUnitRunner() : base(String.Empty)
        {
        }

        protected override string TestLocation { get { return testLocation; } }

        [NUnit.Framework.Ignore]
        public override void Test00() { }
        [NUnit.Framework.Ignore]
        public override void Test01() { }
        [NUnit.Framework.Ignore]
        public override void Test02() { }
        [NUnit.Framework.Ignore]
        public override void Test03() { }
        [NUnit.Framework.Ignore]
        public override void Test04() { }
        [NUnit.Framework.Ignore]
        public override void Test05() { }
        [NUnit.Framework.Ignore]
        public override void Test06() { }
        [NUnit.Framework.Ignore]
        public override void Test07() { }
        [NUnit.Framework.Ignore]
        public override void Test08() { }
        [NUnit.Framework.Ignore]
        public override void Test09() { }
        [NUnit.Framework.Ignore]
        public override void Test10() { }
        [NUnit.Framework.Ignore]
        public override void Test11() { }
        [NUnit.Framework.Ignore]
        public override void Test12() { }
        [NUnit.Framework.Ignore]
        public override void Test13() { }
        [NUnit.Framework.Ignore]
        public override void Test14() { }
        [NUnit.Framework.Ignore]
        public override void Test15() { }
        [NUnit.Framework.Ignore]
        public override void Test16() { }
        [NUnit.Framework.Ignore]
        public override void Test17() { }
        [NUnit.Framework.Ignore]
        public override void Test18() { }
        [NUnit.Framework.Ignore]
        public override void Test19() { }
        [NUnit.Framework.Ignore]
        public override void Test20() { }
        [NUnit.Framework.Ignore]
        public override void TestCountOk() { }

        [NUnit.Framework.Test]
        public void TestAllFiles()
        {
            TestAll();
        }
        protected override void TestAll()
        {
            foreach (string file in Directory.GetFiles(TestLocation))
            {
                var ext = Path.GetExtension(file);
                if (string.IsNullOrEmpty(ext))
                    continue;
                
                if (ext.ToLowerInvariant() != ".xml")
                    continue;

                TestFile = Path.GetFileName(file);
                Console.WriteLine(string.Format("Processing '{0}'", TestFile));
                LoadTests();
                base.TestAll();
            }
        }

    }
}