using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Converts the node sections at a polygon node where
    /// a shell and one or more holes touch, or two or more holes touch.
    /// This converts the node topological structure from
    /// the OGC "touching-rings" (AKA "minimal-ring") model to the equivalent "self-touch"
    /// (AKA "inverted/exverted ring" or "maximal ring") model.
    /// In the "self-touch" model the converted NodeSection corners enclose areas
    /// which all lies inside the polygon
    /// (i.e. they does not enclose hole edges).
    /// This allows {@link RelateNode} to use simple area-additive semantics
    /// for adding edges and propagating edge locations.
    /// <para/>
    /// The input node sections are assumed to have canonical orientation
    /// (CW shells and CCW holes).
    /// The arrangement of shells and holes must be topologically valid.
    /// Specifically, the node sections must not cross or be collinear.
    /// <para/>
    /// This supports multiple shell-shell touches
    /// (including ones containing holes), and hole-hole touches,
    /// This generalizes the relate algorithm to support
    /// both the OGC model and the self-touch model.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="RelateNode"/>
    internal class PolygonNodeConverter
    {
        /// <summary>
        /// Converts a list of sections of valid polygon rings
        /// to have "self-touching" structure.
        /// There are the same number of output sections as input ones.
        /// </summary>
        /// <param name="polySections">The original sections</param>
        /// <returns>The converted sections</returns>
        public static IList<NodeSection> Convert(List<NodeSection> polySections)
        {
            polySections.Sort(NodeSection.EdgeAngleComparator.Instance);

            //TODO: move uniquing up to caller
            var sections = ExtractUnique(polySections);
            if (sections.Count == 1)
                return sections;

            //-- find shell section index
            int shellIndex = FindShell(sections);
            if (shellIndex < 0)
            {
                return ConvertHoles(sections);
            }
            //-- at least one shell is present.  Handle multiple ones if present
            var convertedSections = new List<NodeSection>();
            int nextShellIndex = shellIndex;
            do
            {
                nextShellIndex = ConvertShellAndHoles(sections, nextShellIndex, convertedSections);
            } while (nextShellIndex != shellIndex);

            return convertedSections;
        }

        private static int ConvertShellAndHoles(List<NodeSection> sections, int shellIndex,
            List<NodeSection> convertedSections)
        {
            var shellSection = sections[shellIndex];
            var inVertex = shellSection.GetVertex(0);
            int i = Next(sections, shellIndex);
            NodeSection holeSection, ns;
            Coordinate outVertex;
            while (!sections[i].IsShell)
            {
                holeSection = sections[i];
                // Assert: holeSection.isShell() = false
                outVertex = holeSection.GetVertex(1);
                ns = CreateSection(shellSection, inVertex, outVertex);
                convertedSections.Add(ns);

                inVertex = holeSection.GetVertex(0);
                i = Next(sections, i);
            }
            //-- create final section for corner from last hole to shell
            outVertex = shellSection.GetVertex(1);
            ns = CreateSection(shellSection, inVertex, outVertex);
            convertedSections.Add(ns);
            return i;
        }

        private static List<NodeSection> ConvertHoles(List<NodeSection> sections)
        {
            var convertedSections = new List<NodeSection>();
            var copySection = sections[0];
            for (int i = 0; i < sections.Count; i++)
            {
                int inext = Next(sections, i);
                var inVertex = sections[i].GetVertex(0);
                var outVertex = sections[inext].GetVertex(1);
                var ns = CreateSection(copySection, inVertex, outVertex);
                convertedSections.Add(ns);
            }
            return convertedSections;
        }

        private static NodeSection CreateSection(NodeSection ns, Coordinate v0, Coordinate v1)
        {
            return new NodeSection(ns.IsA,
                Dimension.A, ns.Id, 0, ns.Polygonal,
                ns.IsNodeAtVertex,
                v0, ns.NodePt, v1);
        }

        private static List<NodeSection> ExtractUnique(List<NodeSection> sections)
        {
            var uniqueSections = new List<NodeSection>();
            var lastUnique = sections[0];
            uniqueSections.Add(lastUnique);
            foreach (var ns in sections)
            {
                if (0 != lastUnique.CompareTo(ns))
                {
                    uniqueSections.Add(ns);
                    lastUnique = ns;
                }
            }
            return uniqueSections;
        }

        private static int Next(List<NodeSection> ns, int i)
        {
            int next = i + 1;
            if (next >= ns.Count)
                next = 0;
            return next;
        }

        private static int FindShell(List<NodeSection> polySections)
        {
            for (int i = 0; i < polySections.Count; i++)
            {
                if (polySections[i].IsShell)
                    return i;
            }
            return -1;
        }
    }

}
