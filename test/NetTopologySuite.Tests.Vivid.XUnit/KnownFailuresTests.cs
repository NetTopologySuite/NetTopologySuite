using System;
using System.Collections.Generic;
using System.IO;
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
        [Test]
        public void EveryEntryStillPointsAtItsCase()
        {
            var stale = new List<string>();

            foreach (var failure in KnownFailures.All)
            {
                string path = Path.Combine(CorpusDirectory.Location,
                                           Path.Combine(failure.File.Split('/')));
                if (!File.Exists(path))
                {
                    stale.Add($"{failure}: no such file");
                    continue;
                }

                string[] descriptions;
                try
                {
                    descriptions = XmlCorpusRunner.Descriptions(path);
                }
                catch (Exception ex)
                {
                    stale.Add($"{failure}: the file could not be read ({ex.Message})");
                    continue;
                }

                if (descriptions == null)
                {
                    stale.Add($"{failure}: the file no longer loads");
                    continue;
                }

                if (failure.Index >= descriptions.Length)
                {
                    stale.Add($"{failure}: the file holds {descriptions.Length} case(s)");
                    continue;
                }

                string actual = XmlCorpusRunner.Summarize(descriptions[failure.Index]);
                if (!string.Equals(actual, failure.Description, StringComparison.Ordinal))
                    stale.Add($"{failure}: listed as '{failure.Description}' but now reads '{actual}'");
            }

            Assert.That(stale, Is.Empty,
                "The known-failure list no longer matches the corpus. Cases have probably been "
                + "inserted or removed, which moves the ones listed below onto different cases:"
                + Environment.NewLine + string.Join(Environment.NewLine, stale));
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

        // A pipe anywhere in a cell would start a new column in the rendered table.
        private static string Cell(string value) => value?.Replace("|", "\\|");
    }
}
