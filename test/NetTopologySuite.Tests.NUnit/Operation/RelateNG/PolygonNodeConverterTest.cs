using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    public class PolygonNodeConverterTest : GeometryTestCase
    {
        [Test]
        public void TestShells()
        {
            CheckConversion(
                Collect(
                    SectionShell(1, 1, 5, 5, 9, 9),
                    SectionShell(8, 9, 5, 5, 6, 9),
                    SectionShell(4, 9, 5, 5, 2, 9)),
                Collect(
                    SectionShell(1, 1, 5, 5, 9, 9),
                    SectionShell(8, 9, 5, 5, 6, 9),
                    SectionShell(4, 9, 5, 5, 2, 9))
                );
        }

        [Test]
        public void TestShellAndHole()
        {
            CheckConversion(
                Collect(
                    SectionShell(1, 1, 5, 5, 9, 9),
                    SectionHole(6, 0, 5, 5, 4, 0)),
                Collect(
                    SectionShell(1, 1, 5, 5, 4, 0),
                    SectionShell(6, 0, 5, 5, 9, 9))
                );
        }

        [Test]
        public void TestShellsAndHoles()
        {
            CheckConversion(
                Collect(
                    SectionShell(1, 1, 5, 5, 9, 9),
                    SectionHole(6, 0, 5, 5, 4, 0),

                    SectionShell(8, 8, 5, 5, 1, 8),
                    SectionHole(4, 8, 5, 5, 6, 8)
                    ),
                Collect(
                    SectionShell(1, 1, 5, 5, 4, 0),
                    SectionShell(6, 0, 5, 5, 9, 9),

                    SectionShell(4, 8, 5, 5, 1, 8),
                    SectionShell(8, 8, 5, 5, 6, 8)
                    )
                );
        }

        [Test]
        public void TestShellAnd2Holes()
        {
            CheckConversion(
                Collect(
                    SectionShell(1, 1, 5, 5, 9, 9),
                    SectionHole(7, 0, 5, 5, 6, 0),
                    SectionHole(4, 0, 5, 5, 3, 0)),
                Collect(
                    SectionShell(1, 1, 5, 5, 3, 0),
                    SectionShell(4, 0, 5, 5, 6, 0),
                    SectionShell(7, 0, 5, 5, 9, 9))
                );
        }

        [Test]
        public void TestHoles()
        {
            CheckConversion(
                Collect(
                    SectionHole(7, 0, 5, 5, 6, 0),
                    SectionHole(4, 0, 5, 5, 3, 0)),
                Collect(
                    SectionShell(4, 0, 5, 5, 6, 0),
                    SectionShell(7, 0, 5, 5, 3, 0))
                );
        }

        private static void CheckConversion(List<NodeSection> input, List<NodeSection> expected)
        {
            var actual = (List<NodeSection>)PolygonNodeConverter.Convert(input);
            bool isEqual = CheckSectionsEqual(actual, expected);
            if (!isEqual)
            {
                TestContext.WriteLine($"Expected: {FormatSections(expected)}");
                TestContext.WriteLine($"Actual: {FormatSections(actual)}");
            }
            Assert.True(isEqual);
        }

        private static string FormatSections(IEnumerable<NodeSection> sections)
        {
            var sb = new StringBuilder();
            foreach (var ns in sections)
            {
                sb.Append(ns + "\n");
            }
            return sb.ToString();
        }

        private static bool CheckSectionsEqual(List<NodeSection> ns1, List<NodeSection> ns2)
        {
            if (ns1.Count != ns2.Count)
                return false;
            Sort(ns1);
            Sort(ns2);
            for (int i = 0; i < ns1.Count; i++)
            {
                int comp = ns1[i].CompareTo(ns2[i]);
                if (comp != 0)
                    return false;
            }
            return true;
        }

        private static void Sort(List<NodeSection> ns)
        {
            ns.Sort(NodeSection.EdgeAngleComparator.Instance);
        }

        private static List<NodeSection> Collect(params NodeSection[] sections)
        {
            var sectionList = new List<NodeSection>();
            foreach (var s in sections)
            {
                sectionList.Add(s);
            }
            return sectionList;
        }

        private static NodeSection SectionHole(double v0x, double v0y, double nx, double ny, double v1x, double v1y)
        {
            return Section(1, v0x, v0y, nx, ny, v1x, v1y);
        }

        private static NodeSection Section(int ringId, double v0x, double v0y, double nx, double ny, double v1x, double v1y)
        {
            return new NodeSection(true, Dimension.A, 1, ringId, null, false,
                new Coordinate(v0x, v0y), new Coordinate(nx, ny), new Coordinate(v1x, v1y));
        }

        private static NodeSection SectionShell(double v0x, double v0y, double nx, double ny, double v1x, double v1y)
        {
            return Section(0, v0x, v0y, nx, ny, v1x, v1y);
        }
    }
}
