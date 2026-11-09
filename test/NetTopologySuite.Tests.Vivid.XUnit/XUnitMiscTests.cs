using System.Collections.Generic;
using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public class MiscCorpusTests : XmlCorpusRunner
    {
        public static IEnumerable<TestCaseData> Cases => CasesIn("misc");

        [TestCaseSource(nameof(Cases))]
        public void Run(string file, int index) => RunCase(file, index);
    }
}
