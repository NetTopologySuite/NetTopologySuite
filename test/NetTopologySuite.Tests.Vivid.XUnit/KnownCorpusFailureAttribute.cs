using System;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Declares a corpus case that fails today, so that it can be excluded from CI while it is
    /// investigated.
    /// <para/>
    /// This is data read at discovery time rather than an NUnit category: the cases come from a
    /// <c>TestCaseSource</c>, and a category attribute would apply to the method that generates
    /// them all rather than to one generated case.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class KnownCorpusFailureAttribute : Attribute
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
        public KnownCorpusFailureAttribute(string file, int index, string description, string reason, string issue)
        {
            File = file;
            Index = index;
            Description = description;
            Reason = reason;
            Issue = issue;
        }

        public string File { get; }

        public int Index { get; }

        public string Description { get; }

        public string Reason { get; }

        public string Issue { get; }

        public override string ToString() => $"{File} #{Index}";
    }
}
