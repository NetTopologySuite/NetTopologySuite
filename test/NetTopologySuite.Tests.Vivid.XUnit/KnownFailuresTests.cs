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
    /// Guards the known-failure list against drifting away from the corpus.
    /// <para/>
    /// The list points at cases by position, so inserting a case into a file moves every entry
    /// below it onto the wrong case — silently, since the wrong case would simply be excluded from
    /// CI instead. One test validates the whole list, so drift shows up once with everything that
    /// moved, rather than as a scattering of unrelated failures.
    /// </summary>
    [TestFixture]
    public class KnownFailuresTests
    {
        /// <summary>
        /// Both lists point at cases by position, so one source of entries is as liable to drift
        /// as the other and they are checked together.
        /// </summary>
        private static IEnumerable<(string File, int Index, string Description)> AllEntries()
        {
            foreach (var failure in KnownFailures.All)
                yield return (failure.File, failure.Index, failure.Description);

            foreach (var slow in LongRunningCases.All)
                yield return (slow.File, slow.Index, slow.Description);
        }

        [Test]
        public void EveryEntryStillPointsAtItsCase()
        {
            var stale = new List<string>();

            foreach (var failure in AllEntries())
            {
                string path = Path.Combine(CorpusDirectory.Location,
                                           Path.Combine(failure.File.Split('/')));
                if (!File.Exists(path))
                {
                    stale.Add($"{Name(failure)}: no such file");
                    continue;
                }

                string[] descriptions;
                try
                {
                    descriptions = XmlCorpusRunner.Descriptions(path);
                }
                catch (Exception ex)
                {
                    stale.Add($"{Name(failure)}: the file could not be read ({ex.Message})");
                    continue;
                }

                if (descriptions == null)
                {
                    stale.Add($"{Name(failure)}: the file no longer loads");
                    continue;
                }

                if (failure.Index >= descriptions.Length)
                {
                    stale.Add($"{Name(failure)}: the file holds {descriptions.Length} case(s)");
                    continue;
                }

                string actual = XmlCorpusRunner.Summarize(descriptions[failure.Index]);
                if (!string.Equals(actual, failure.Description, StringComparison.Ordinal))
                    stale.Add($"{Name(failure)}: listed as '{failure.Description}' but now reads '{actual}'");
            }

            Assert.That(stale, Is.Empty,
                "The known-failure list no longer matches the corpus. Cases have probably been "
                + "inserted or removed, which moves the ones listed below onto different cases:"
                + Environment.NewLine + string.Join(Environment.NewLine, stale));
        }

        /// <summary>
        /// Checks that every entry names the issue tracking it.
        /// <para/>
        /// The list is generated from a run, and the issue is the one field a generator cannot
        /// fill in, so it is the one that reaches a branch still carrying a placeholder. Nothing
        /// else looks at the field, which is why it needs looking at here.
        /// </summary>
        [Test]
        public void EveryEntryNamesItsIssue()
        {
            var unnamed = new List<string>();

            foreach (var failure in KnownFailures.All)
            {
                string issue = failure.Issue;
                bool named = !string.IsNullOrWhiteSpace(issue)
                             && issue.StartsWith("#", StringComparison.Ordinal)
                             && issue.Length > 1
                             && issue.Skip(1).All(char.IsDigit);

                if (!named)
                    unnamed.Add($"{failure}: '{issue}'");
            }

            Assert.That(unnamed, Is.Empty,
                "These entries do not name the issue tracking them, so nothing says where the "
                + "investigation lives:"
                + Environment.NewLine + string.Join(Environment.NewLine, unnamed));
        }

        /// <summary>
        /// Reports entries whose case has started passing.
        /// <para/>
        /// An excluded case is never run by the suite, so a fix elsewhere would otherwise leave
        /// its entry in place for good and quietly keep the case out of CI. Failing here is the
        /// intended signal: drop the entry and the case rejoins the suite.
        /// </summary>
        [Test]
        public void NoEntryHasStartedPassing()
        {
            var passing = new List<string>();
            var unverified = new List<string>();

            foreach (var failure in KnownFailures.All)
            {
                string path = Path.Combine(CorpusDirectory.Location,
                                           Path.Combine(failure.File.Split('/')));

                // An entry that no longer points at a runnable case says nothing about whether
                // that case passes, and EveryEntryStillPointsAtItsCase reports it already.
                if (!File.Exists(path))
                    continue;

                try
                {
                    var tests = new XmlTestController().Load(path);
                    if (tests == null || failure.Index >= tests.Count)
                        continue;

                    if (tests[failure.Index].RunTest())
                        passing.Add($"{failure}: {failure.Reason}");
                }
                catch (Exception ex)
                {
                    // A case that fails is answered with false and keeps its exception in
                    // XmlTest.Thrown, so nothing the case itself raises arrives here. What does
                    // comes from loading the file, and leaves the entry unverified rather than
                    // confirmed - which is the one thing this test must not pass over.
                    unverified.Add($"{failure}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            Assert.Multiple(() =>
            {
                Assert.That(unverified, Is.Empty,
                    "These entries could not be checked at all, so nothing here says whether the "
                    + "cases they exclude still fail:"
                    + Environment.NewLine + string.Join(Environment.NewLine, unverified));

                Assert.That(passing, Is.Empty,
                    "These cases pass now, so their entries can go and the cases can rejoin CI:"
                    + Environment.NewLine + string.Join(Environment.NewLine, passing));
            });
        }

        /// <summary>
        /// Prints the list as a Markdown table, so the tracking issue can quote the list as it
        /// stands instead of keeping a second copy of it in step by hand.
        /// </summary>
        [Test, Explicit("Prints the list rather than checking anything.")]
        public void WriteTable()
        {
            var table = new StringBuilder()
                .AppendLine("| file | index | description | reason | issue |")
                .AppendLine("|---|---|---|---|---|");

            foreach (var failure in KnownFailures.All)
            {
                table.AppendLine(
                    $"| {Cell(failure.File)} | {failure.Index} | {Cell(failure.Description)} "
                    + $"| {Cell(failure.Reason)} | {Cell(failure.Issue)} |");
            }

            TestContext.Out.Write(table.ToString());
        }

        private static string Name((string File, int Index, string Description) entry) => $"{entry.File} #{entry.Index}";

        // A pipe anywhere in a cell would start a new column in the rendered table.
        private static string Cell(string value) => value?.Replace("|", "\\|");
    }
}
