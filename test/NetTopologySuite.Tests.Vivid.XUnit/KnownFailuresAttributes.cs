using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Reads the known-failure list from the assembly's <see cref="KnownCorpusFailureAttribute"/>
    /// declarations, listed in <c>KnownFailuresList.cs</c>.
    /// <para/>
    /// There is no file to parse and no format to get wrong: an entry that is missing an argument
    /// or has them in the wrong order stops the build. The list is code, so the table for the
    /// tracking issue has to be produced from it rather than copied out of it; see
    /// <see cref="KnownFailuresTests.WriteTable"/>.
    /// </summary>
    internal static class KnownFailuresAttributes
    {
        internal static IReadOnlyList<KnownFailure> Load()
        {
            return Assembly.GetExecutingAssembly()
                           .GetCustomAttributes<KnownCorpusFailureAttribute>()
                           .Select(a => new KnownFailure(a.File, a.Index, a.Description, a.Reason, a.Issue))
                           .ToList();
        }
    }
}
