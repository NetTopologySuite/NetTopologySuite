using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Utility class to build facet sequencs STRtrees-
    /// </summary>
    public static class FacetSequenceTreeBuilder
    {
        // 6 seems to be a good facet sequence size
        private const int FacetSequenceSize = 6;

// ReSharper disable InconsistentNaming

        // Seems to be better to use a minimum node capacity
        private const int STRtreeNodeCapacity = 4;

        /// <summary>
        ///
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public static STRtree<FacetSequence> BuildSTRtree(Geometry g)
// ReSharper restore InconsistentNaming
        {
            var tree = new STRtree<FacetSequence>(STRtreeNodeCapacity);
            var sections = ComputeFacetSequences(g);
            foreach (var section in sections)
            {
                tree.Insert(section.Envelope, section);
            }
            tree.Build();
            return tree;
        }

        /// <summary>
        /// Creates facet sequences from a given geometry
        /// </summary>
        /// <param name="g">The geometry</param>
        /// <returns>A list of <see cref="FacetSequence"/>s</returns>
        private static List<FacetSequence> ComputeFacetSequences(Geometry g)
        {
            var sections = new List<FacetSequence>();

            g.Apply(new GeometryComponentFilter(
                        delegate(Geometry geom)
                            {
                                CoordinateSequence seq;
                                if (geom is LineString ls)
                                {
                                    seq = ls.CoordinateSequence;
                                    AddFacetSequences(geom, seq, sections);
                                }
                                else if (geom is Point pt)
                                {
                                    seq = pt.CoordinateSequence;
                                    AddFacetSequences(geom, seq, sections);
                                }
                            }));
            return sections;
        }

        private static void AddFacetSequences(Geometry geom, CoordinateSequence pts, List<FacetSequence> sections)
        {
            int i = 0;
            int size = pts.Count;
            while (i <= size - 1)
            {
                int end = i + FacetSequenceSize + 1;
                // if only one point remains after this section, include it in this
                // section
                if (end >= size - 1)
                    end = size;
                var sect = new FacetSequence(geom, pts, i, end);
                sections.Add(sect);
                i = i + FacetSequenceSize;
            }
        }
    }
}
