using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NetTopologySuite.Geometries;

namespace Open.Topology.TestRunner.Operations
{
    /// <summary>
    /// Finds the members of <see cref="Geometry"/> that can carry an operation named the way the
    /// XML corpus names it.
    /// <para/>
    /// The corpus is shared with JTS and names its operations after the Java API, where
    /// <c>getArea</c> and <c>isValid</c> are methods that reflection finds by name. This port
    /// turned those into properties and renamed three of them, so a search over methods alone
    /// misses eight of the operations the corpus asks a geometry for. This class is the one place
    /// that difference is accounted for, which leaves <see cref="GeometryMethodOperation"/> the
    /// literal port of its JTS counterpart that it was.
    /// </summary>
    internal static class GeometryMemberLookup
    {
        // Members this port named differently from JTS. A name that only lost its "get" prefix, or
        // became a property, is handled by the search below and does not belong here.
        private static readonly Dictionary<string, string> RenamedByPort =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["equalsTopo"] = nameof(Geometry.EqualsTopologically),
                ["equalsNorm"] = nameof(Geometry.EqualsNormalized),
                ["symDifference"] = nameof(Geometry.SymmetricDifference),
            };

        private static readonly MethodInfo[] Methods = typeof(Geometry).GetMethods();

        // A property is offered as its getter, so that a caller can invoke whatever it is handed
        // without having to care which kind of member answered.
        private static readonly MethodInfo[] PropertyGetters = typeof(Geometry)
            .GetProperties()
            .Select(p => p.GetMethod)
            .Where(m => m != null && m.IsPublic && m.GetParameters().Length == 0)
            .ToArray();

        /// <summary>
        /// Every member that could serve <paramref name="opName"/>, methods first: a method is
        /// what JTS would have found, and the caller picks between overloads by argument count.
        /// </summary>
        internal static IEnumerable<MethodInfo> Candidates(string opName)
        {
            foreach (string name in NamesToTry(opName))
            {
                foreach (var method in Methods)
                {
                    if (method.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                        yield return method;
                }

                foreach (var getter in PropertyGetters)
                {
                    if (PropertyName(getter).Equals(name, StringComparison.OrdinalIgnoreCase))
                        yield return getter;
                }
            }
        }

        private static IEnumerable<string> NamesToTry(string opName)
        {
            if (RenamedByPort.TryGetValue(opName, out string renamed))
            {
                yield return renamed;
                yield break;
            }

            yield return opName;

            // A Java getter keeps its "get" in the corpus; the member it became here dropped it.
            if (opName.Length > 3 && opName.StartsWith("get", StringComparison.OrdinalIgnoreCase))
                yield return opName.Substring(3);
        }

        private static string PropertyName(MethodInfo getter) => getter.Name.Substring("get_".Length);
    }
}
