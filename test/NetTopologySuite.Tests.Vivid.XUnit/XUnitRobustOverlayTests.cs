using System.Collections.Generic;
using NUnit.Framework;

namespace NetTopologySuite.Tests.XUnit
{
    public class RobustOverlayCorpusTests : XmlCorpusRunner
    {
        public static IEnumerable<TestCaseData> Cases => CasesIn("robust/overlay");

        [TestCaseSource(nameof(Cases))]
        public void Run(string file, int index) => RunCase(file, index);
    }
}
