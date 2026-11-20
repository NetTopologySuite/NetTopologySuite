using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Declares a corpus case that takes long enough to run that it does not belong in the suite
    /// every push waits for.
    /// <para/>
    /// Like <see cref="KnownCorpusFailureAttribute"/>, this is data the case source reads at
    /// discovery time, and it is what puts the <c>LongRunning</c> category on the generated case.
    /// It cannot be that category itself: the cases come from a <c>TestCaseSource</c>, so an
    /// attribute would apply to the method that generates all of them rather than to the one case
    /// meant to be held back.
    /// <para/>
    /// The case is held back, not dropped: a run without the CI filter still executes it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class LongRunningCorpusCaseAttribute : Attribute
    {
        /// <param name="file">The corpus-relative path, with forward slashes.</param>
        /// <param name="index">
        /// The position of the case within the collection loaded for that file, which is the
        /// number the test name carries.
        /// </param>
        /// <param name="description">
        /// The case description as it stood when the entry was written, summarized the same way
        /// test names are. Stored so an entry cannot go on pointing at a case that has since
        /// moved; <see cref="KnownFailuresTests"/> compares it against the case actually there.
        /// </param>
        /// <param name="seconds">Roughly how long the case took when the entry was written.</param>
        /// <param name="reason">What makes it slow, in one line.</param>
        internal LongRunningCorpusCaseAttribute(string file, int index, string description, int seconds, string reason)
        {
            File = file;
            Index = index;
            Description = description;
            Seconds = seconds;
            Reason = reason;
        }

        /// <summary>The corpus-relative path, with forward slashes.</summary>
        internal string File { get; }

        /// <summary>The position of the case within the collection loaded for that file.</summary>
        internal int Index { get; }

        /// <summary>The case description as it stood when the entry was written.</summary>
        internal string Description { get; }

        /// <summary>Roughly how long the case took when the entry was written.</summary>
        internal int Seconds { get; }

        /// <summary>What makes the case slow, in one line.</summary>
        internal string Reason { get; }

        public override string ToString() => $"{File} #{Index}";
    }

    /// <summary>
    /// The corpus cases held back from the suite CI runs, as declared in
    /// <c>LongRunningCasesList.cs</c>.
    /// </summary>
    internal static class LongRunningCases
    {
        // Lazy rather than a static initialiser so that a malformed entry surfaces as a failing
        // test rather than a type initialisation error, and thread-safe because discovery of the
        // corpus fixtures need not happen on one thread.
        private static readonly Lazy<IReadOnlyList<LongRunningCorpusCaseAttribute>> LazyAll =
            new Lazy<IReadOnlyList<LongRunningCorpusCaseAttribute>>(
                () => Assembly.GetExecutingAssembly()
                              .GetCustomAttributes<LongRunningCorpusCaseAttribute>()
                              .ToList());

        private static readonly Lazy<HashSet<(string File, int Index)>> LazyIndex =
            new Lazy<HashSet<(string File, int Index)>>(
                () => LazyAll.Value.Select(c => (c.File, c.Index)).ToHashSet());

        /// <summary>Every declared entry, in declaration order.</summary>
        internal static IReadOnlyList<LongRunningCorpusCaseAttribute> All => LazyAll.Value;

        /// <summary>Whether the case at <paramref name="index"/> of <paramref name="file"/> is declared.</summary>
        internal static bool IsLongRunning(string file, int index) => LazyIndex.Value.Contains((file, index));
    }
}
