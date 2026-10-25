using System.Collections.Generic;
using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public class GeneralCorpusTests : XmlCorpusRunner
    {
        public static IEnumerable<TestCaseData> Cases => CasesIn("general");

        [TestCaseSource(nameof(Cases))]
        public void Run(string file, int index) => RunCase(file, index);
    }
}
