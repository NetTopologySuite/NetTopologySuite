using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Declares a corpus case that fails today, so that it can be excluded from CI while it is
    /// investigated.
    /// <para/>
    /// The declaration is data the case source reads at discovery time, and it is what puts the
    /// <c>FailureCase</c> category on the generated case. It cannot be that category itself: the
    /// cases come from a <c>TestCaseSource</c>, so an attribute would apply to the method that
    /// generates all of them rather than to the one case meant to be excluded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    internal sealed class KnownCorpusFailureAttribute : Attribute
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
        /// <param name="reason">Why the case fails, in one line.</param>
        /// <param name="issue">The issue tracking the investigation.</param>
        internal KnownCorpusFailureAttribute(string file, int index, string description, string reason, string issue)
        {
            File = file;
            Index = index;
            Description = description;
            Reason = reason;
            Issue = issue;
        }

        /// <summary>The corpus-relative path, with forward slashes.</summary>
        internal string File { get; }

        /// <summary>The position of the case within the collection loaded for that file.</summary>
        internal int Index { get; }

        /// <summary>The case description as it stood when the entry was written.</summary>
        internal string Description { get; }

        /// <summary>Why the case fails, in one line.</summary>
        internal string Reason { get; }

        /// <summary>The issue tracking the investigation.</summary>
        internal string Issue { get; }

        public override string ToString() => $"{File} #{Index}";
    }

    /// <summary>
    /// The corpus cases that fail today, as declared in <c>KnownFailuresList.cs</c>. They are
    /// excluded from CI through the <c>FailureCase</c> category, the same one the rest of the
    /// suite uses, so that the test story can land ahead of the investigation into why each
    /// case fails.
    /// </summary>
    internal static class KnownFailures
    {
        // Lazy rather than a static initialiser so that a malformed entry surfaces as a failing
        // test rather than a type initialisation error, and thread-safe because discovery of the
        // two corpus fixtures need not happen on one thread.
        private static readonly Lazy<IReadOnlyList<KnownCorpusFailureAttribute>> LazyAll =
            new Lazy<IReadOnlyList<KnownCorpusFailureAttribute>>(
                () => Assembly.GetExecutingAssembly()
                              .GetCustomAttributes<KnownCorpusFailureAttribute>()
                              .ToList());

        private static readonly Lazy<HashSet<(string File, int Index)>> LazyIndex =
            new Lazy<HashSet<(string File, int Index)>>(
                () => LazyAll.Value.Select(f => (f.File, f.Index)).ToHashSet());

        /// <summary>Every declared entry, in declaration order.</summary>
        internal static IReadOnlyList<KnownCorpusFailureAttribute> All => LazyAll.Value;

        /// <summary>Whether the case at <paramref name="index"/> of <paramref name="file"/> is declared.</summary>
        internal static bool IsKnown(string file, int index) => LazyIndex.Value.Contains((file, index));
    }
}
