using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace NetTopologySuite.Tests.XUnit
{
    /// <summary>
    /// Locates the XML corpus.
    /// <para/>
    /// The corpus lives in <c>data</c>, outside this project, and is not copied to the output
    /// directory, so there is no path from the assembly to it at run time. What the code does
    /// have is <see cref="CallerFilePathAttribute"/>, which the compiler fills in with the
    /// absolute path of this file as it stood when the assembly was built; the corpus is then
    /// found by walking up to the repository and back down.
    /// <para/>
    /// That makes the path the one of the machine that built the tests, which holds for a build
    /// and a run on the same checkout and not much else. Copying the corpus to the output
    /// directory instead would lift that restriction, at the cost of copying it on every build.
    /// </summary>
    internal static class CorpusDirectory
    {
        internal static readonly string Location = Locate();

        private static string Locate([CallerFilePath] string thisFilePath = null)
        {
            // Hack to debug test built on Windows using WSL
            if (Environment.OSVersion.Platform == PlatformID.Unix && thisFilePath[1] == ':')
            {
                thisFilePath = thisFilePath.Replace('\\', '/');
                thisFilePath = thisFilePath.Replace(thisFilePath.Substring(0, 2), $"/mnt/{thisFilePath.Substring(0, 1).ToLowerInvariant()}");
            }

            var project = new FileInfo(thisFilePath).Directory;          // /test/NetTopologySuite.Tests.Vivid.XUnit
            var repository = project?.Parent?.Parent;                    // /
            var corpus = repository is null
                ? null
                : new DirectoryInfo(Path.Combine(repository.FullName, "data", "NetTopologySuite.TestRunner.Tests"));

            if (corpus is null || !corpus.Exists)
            {
                throw new DirectoryNotFoundException(
                    $"The XML corpus was not found from '{thisFilePath}'. It is located relative to this "
                    + "source file, so a test assembly built on another checkout cannot find it.");
            }

            return corpus.FullName;
        }
    }
}
