using System;
using System.IO;
using Xunit;

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

        public override void TestAll()
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