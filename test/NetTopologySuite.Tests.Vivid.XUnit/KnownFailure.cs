using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// A corpus case that is known to fail, kept out of CI until it has been investigated.
    /// </summary>
    public sealed class KnownFailure
    {
        public KnownFailure(string file, int index, string description, string reason, string issue)
        {
            File = file;
            Index = index;
            Description = description;
            Reason = reason;
            Issue = issue;
        }

        /// <summary>The corpus-relative path, with forward slashes.</summary>
        public string File { get; }

        /// <summary>The position of the case within the loaded collection for that file.</summary>
        public int Index { get; }

        /// <summary>
        /// The case description as it stood when the entry was written, summarized the same way
        /// test names are. Stored so that an entry cannot go on pointing at a case that has since
        /// moved; see <see cref="KnownFailuresTests"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>Why the case fails, in one line.</summary>
        public string Reason { get; }

        /// <summary>The issue tracking the investigation.</summary>
        public string Issue { get; }

        public override string ToString() => $"{File} #{Index}";
    }

    /// <summary>
    /// The list of corpus cases that fail today. Entries are excluded from CI through the
    /// <c>FailureCase</c> category, the same one the rest of the suite uses, so that the test
    /// story can land ahead of the investigation into why each case fails.
    /// </summary>
    public static class KnownFailures
    {
        // Lazy rather than a static initialiser so that a malformed entry surfaces as a failing
        // test rather than a type initialisation error, and thread-safe because discovery of the
        // two corpus fixtures need not happen on one thread.
        private static readonly Lazy<IReadOnlyList<KnownFailure>> LazyAll =
            new Lazy<IReadOnlyList<KnownFailure>>(KnownFailuresAttributes.Load);

        private static readonly Lazy<HashSet<(string File, int Index)>> LazyIndex =
            new Lazy<HashSet<(string File, int Index)>>(
                () => LazyAll.Value.Select(f => (f.File, f.Index)).ToHashSet());

        public static IReadOnlyList<KnownFailure> All => LazyAll.Value;

        public static bool IsKnown(string file, int index)
        {
            return LazyIndex.Value.Contains((file, index));
        }
    }
}
