using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Open.Topology.TestRunner;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Runs every case below a corpus directory as an individually named NUnit test, so that a
    /// behaviour change identifies the case that caused it rather than the directory it lives in.
    /// </summary>
    public abstract class XmlCorpusRunner
    {
        /// <summary>
        /// Index standing in for a file that loaded but produced no case to run. Reported as
        /// ignored: the runner not implementing an operation yet is a known limitation, not a
        /// broken file.
        /// </summary>
        private const int NoCase = -1;

        /// <summary>
        /// Index standing in for a file that could not be read at all. Reported as a failure,
        /// because a corpus file that does not parse is a defect rather than a limitation.
        /// </summary>
        private const int Unreadable = -2;

        /// <summary>
        /// Index standing in for a corpus directory that holds nothing to run, which would
        /// otherwise leave a fixture reporting success over an empty suite.
        /// </summary>
        private const int EmptyLocation = -3;

        // XmlTestController reuses one XmlTestDocument, so loading a file invalidates the
        // collection returned for the previous one. Both discovery and execution walk the corpus
        // file by file, so holding on to the most recent one spares a reload for every case of a
        // file after the first, while bounding memory to a single file.
        //
        // The cache is per thread because that same invalidation makes the controller unsafe to
        // share: were two fixtures ever to run in parallel, one would hand the other a collection
        // belonging to a different file, and the wrong case would run without anything failing.
        [ThreadStatic] private static string _loadedFile;
        [ThreadStatic] private static XmlTestCollection _loadedTests;

        protected static IEnumerable<TestCaseData> CasesIn(string location)
        {
            string directory = Path.Combine(CorpusDirectory.Location, Path.Combine(location.Split('/')));
            if (!Directory.Exists(directory))
            {
                yield return new TestCaseData(directory, EmptyLocation)
                    .SetName($"{location} - directory not found");
                yield break;
            }

            // A directory that yields nothing is the failure this change exists to remove, so it
            // is reported rather than left to look like a fixture that simply had no work.
            bool any = false;

            foreach (string file in Directory.EnumerateFiles(directory, "*.xml")
                                             .OrderBy(f => f, StringComparer.Ordinal))
            {
                any = true;
                string relative = Relative(file);

                string[] descriptions = null;
                string failure = null;
                try
                {
                    descriptions = Descriptions(file);
                }
                catch (Exception ex)
                {
                    failure = ex.Message;
                }

                if (failure != null || descriptions == null)
                {
                    yield return new TestCaseData(file, Unreadable)
                        .SetName(TestName(relative, Unreadable, failure ?? "the document could not be loaded"));
                    continue;
                }

                // A file whose operations the runner does not implement loads fine and comes back
                // empty, so without this it would contribute nothing and go unnoticed - the same
                // silence at file level that this change removes at directory level.
                if (descriptions.Length == 0)
                {
                    yield return new TestCaseData(file, NoCase)
                        .SetName(TestName(relative, NoCase, "no runnable case"));
                    continue;
                }

                for (int i = 0; i < descriptions.Length; i++)
                {
                    var data = new TestCaseData(file, i).SetName(TestName(relative, i, descriptions[i]));

                    // Known failures stay discoverable and keep their name, but carry the category
                    // the CI filter already excludes, so the suite stays green while they are
                    // investigated one at a time. See KnownFailuresList.cs.
                    if (KnownFailures.IsKnown(relative, i))
                        data.SetCategory("FailureCase");

                    yield return data;
                }
            }

            if (!any)
            {
                yield return new TestCaseData(directory, EmptyLocation)
                    .SetName($"{location} - no corpus file");
            }
        }

        protected static void RunCase(string file, int index)
        {
            if (index == EmptyLocation)
                Assert.Fail($"'{file}' holds no corpus file to run.");

            if (index == Unreadable)
                Assert.Fail($"'{Relative(file)}' could not be read.");

            if (index == NoCase)
                Assert.Ignore($"'{Relative(file)}' loaded, but contributed no case to run.");

            var tests = Load(file);
            Assert.That(tests, Is.Not.Null, $"'{Relative(file)}' could not be loaded.");
            Assert.That(index, Is.LessThan(tests.Count),
                $"'{Relative(file)}' holds {tests.Count} case(s), so index {index} is out of range.");

            var test = tests[index];
            Assert.That(test.RunTest(), Is.True, $"Case failed: {test.Description}");
        }

        private static XmlTestCollection Load(string file)
        {
            if (_loadedFile == file)
                return _loadedTests;

            _loadedTests = new XmlTestController().Load(file);
            _loadedFile = file;
            return _loadedTests;
        }

        /// <summary>
        /// The descriptions of a file's cases, or <c>null</c> when the document could not be
        /// loaded at all.
        /// <para/>
        /// They are taken from the loaded collection rather than from the XML, because a case
        /// contributes one entry per contained "test" element; deriving the count from the
        /// markup would misalign the index used to execute the case.
        /// </summary>
        internal static string[] Descriptions(string file)
        {
            // XmlTestDocument reports a document it could not parse by returning false, which
            // reaches here as a null collection. A file the runner simply has no operation for
            // loads fine and comes back empty, and the two deserve different verdicts.
            var tests = Load(file);
            if (tests == null)
                return null;

            string[] descriptions = new string[tests.Count];
            for (int i = 0; i < tests.Count; i++)
                descriptions[i] = tests[i].Description ?? string.Empty;

            return descriptions;
        }

        internal static string Relative(string file)
        {
            return Path.GetRelativePath(CorpusDirectory.Location, file).Replace(Path.DirectorySeparatorChar, '/');
        }

        private static string TestName(string relative, int index, string description)
        {
            string position;
            switch (index)
            {
                case EmptyLocation: position = "empty"; break;
                case Unreadable: position = "unreadable"; break;
                case NoCase: position = "no-case"; break;
                default: position = $"#{index}"; break;
            }

            string summary = Summarize(description);
            return summary.Length == 0
                ? $"{relative} {position}"
                : $"{relative} {position} - {summary}";
        }

        /// <summary>
        /// NUnit treats braces in a test name as format specifiers, and commas and parentheses
        /// interfere with --filter expressions, so they are dropped rather than escaped.
        /// </summary>
        internal static string Summarize(string description)
        {
            const int maxLength = 80;

            var summary = new StringBuilder(Math.Min(description.Length, maxLength));
            bool lastWasSpace = false;
            foreach (char c in description)
            {
                if (summary.Length == maxLength)
                    break;

                if (char.IsControl(c) || char.IsWhiteSpace(c))
                {
                    if (summary.Length > 0 && !lastWasSpace)
                    {
                        summary.Append(' ');
                        lastWasSpace = true;
                    }

                    continue;
                }

                if ("{},()\"".IndexOf(c) >= 0)
                    continue;

                summary.Append(c);
                lastWasSpace = false;
            }

            return summary.ToString().TrimEnd();
        }
    }
}
