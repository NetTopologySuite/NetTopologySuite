using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Checks if simplifying (flattening) line sections or segments
    /// would cause them to "jump" over other components in the geometry.
    /// </summary>
    /// <author>Martin Davis</author>
    public sealed class ComponentJumpChecker
    {

        //TODO: use a spatial index?
        private readonly ICollection<TaggedLineString> _components;

        internal ComponentJumpChecker(ICollection<TaggedLineString> taggedLines)
        {
            _components = taggedLines;
        }

        /// <summary>
        /// Checks if a line section jumps a component if flattened.
        /// </summary>
        /// <remarks>Assumes <paramref name="start"/> &lt;= <paramref name="end"/></remarks>
        /// <param name="line">The line containing the section being flattened</param>
        /// <param name="start">Start index of the section</param>
        /// <param name="end">End index of the section</param>
        /// <param name="seg">The flattening segment</param>
        /// <returns><c>true</c> if the flattened section jumps a component</returns>
        public bool HasJump(TaggedLineString line, int start, int end, LineSegment seg)
        {
            var sectionEnv = ComputeEnvelope(line, start, end);
            foreach (var comp in _components)
            {
                //-- don't test component against itself
                if (comp == line)
                    continue;

                var compPt = comp.GetComponentPoint();
                if (sectionEnv.Intersects(compPt))
                {
                    if (HasJumpAtComponent(compPt, line, start, end, seg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if two consecutive segments jumps a component if flattened.
        /// The segments are assumed to be consecutive.
        /// (so the seg1.P1 = seg2.P0).
        /// The flattening segment must be the segment between seg1.P0 and seg2.P1.
        /// </summary>
        /// <param name="line">The line containing the section being flattened</param>
        /// <param name="seg1">The first replaced segment</param>
        /// <param name="seg2">The next replaced segment</param>
        /// <param name="seg">The flattening segment</param>
        /// <returns><c>true</c> if the flattened segment jumps a component</returns>
        public bool HasJump(TaggedLineString line, LineSegment seg1, LineSegment seg2, LineSegment seg)
        {
            var sectionEnv = ComputeEnvelope(seg1, seg2);
            foreach (var comp in _components)
            {
                //-- don't test component against itself
                if (comp == line)
                    continue;

                var compPt = comp.GetComponentPoint();
                if (sectionEnv.Intersects(compPt))
                {
                    if (HasJumpAtComponent(compPt, seg1, seg2, seg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool HasJumpAtComponent(Coordinate compPt, TaggedLineString line, int start, int end, LineSegment seg)
        {
            int sectionCount = CrossingCount(compPt, line, start, end);
            int segCount = CrossingCount(compPt, seg);
            bool hasJump = sectionCount % 2 != segCount % 2;
            return hasJump;
        }

        private static bool HasJumpAtComponent(Coordinate compPt, LineSegment seg1, LineSegment seg2, LineSegment seg)
        {
            int sectionCount = CrossingCount(compPt, seg1, seg2);
            int segCount = CrossingCount(compPt, seg);
            bool hasJump = sectionCount % 2 != segCount % 2;
            return hasJump;
        }

        private static int CrossingCount(Coordinate compPt, LineSegment seg)
        {
            var rcc = new RayCrossingCounter(compPt);
            rcc.CountSegment(seg.P0, seg.P1);
            return rcc.Count;
        }

        private static int CrossingCount(Coordinate compPt, LineSegment seg1, LineSegment seg2)
        {
            var rcc = new RayCrossingCounter(compPt);
            rcc.CountSegment(seg1.P0, seg1.P1);
            rcc.CountSegment(seg2.P0, seg2.P1);
            return rcc.Count;
        }

        private static int CrossingCount(Coordinate compPt, TaggedLineString line, int start, int end)
        {
            var rcc = new RayCrossingCounter(compPt);
            for (int i = start; i < end; i++)
            {
                rcc.CountSegment(line.GetCoordinate(i), line.GetCoordinate(i + 1));
            }
            return rcc.Count;
        }

        private static Envelope ComputeEnvelope(LineSegment seg1, LineSegment seg2)
        {
            var env = new Envelope();
            env.ExpandToInclude(seg1.P0);
            env.ExpandToInclude(seg1.P1);
            env.ExpandToInclude(seg2.P0);
            env.ExpandToInclude(seg2.P1);
            return env;
        }

        private static Envelope ComputeEnvelope(TaggedLineString line, int start, int end)
        {
            var env = new Envelope();
            for (int i = start; i <= end; i++)
            {
                env.ExpandToInclude(line.GetCoordinate(i));
            }
            return env;
        }
    }
}
