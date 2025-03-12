using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Models a section of a raw offset curve,
    /// starting at a given location along the raw curve.
    /// The location is a decimal number, with the integer part
    /// containing the segment index and the fractional part
    /// giving the fractional distance along the segment.
    /// The location of the last section segment
    /// is also kept, to allow optimizing joining sections together.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OffsetCurveSection : IComparable<OffsetCurveSection>
    {
        public static Geometry ToGeometry(IEnumerable<OffsetCurveSection> sections, GeometryFactory geomFactory)
            => ToGeometry(AsList(sections), geomFactory);

        private static Geometry ToGeometry(List<OffsetCurveSection> sections, GeometryFactory geomFactory)
        {
            if (sections.Count == 0)
                return geomFactory.CreateLineString();
            if (sections.Count == 1)
                return geomFactory.CreateLineString(sections[0].Coordinates);

            //-- sort sections in order along the offset curve
            sections.Sort();
            var lines = new LineString[sections.Count];

            for (int i = 0; i < sections.Count; i++)
                lines[i] = geomFactory.CreateLineString(sections[i].Coordinates);

            return geomFactory.CreateMultiLineString(lines);
        }

        /// <summary>
        /// Joins section coordinates into a LineString.
        /// Join vertices which lie in the same raw curve segment
        /// are removed, to simplify the result linework.
        /// </summary>
        /// <param name="sections">An enumeration of sections to join</param>
        /// <param name="geomFactory">The geometry factory to use</param>
        /// <returns>The simplified linestring for the joined sections</returns>
        public static Geometry ToLine(IEnumerable<OffsetCurveSection> sections, GeometryFactory geomFactory) =>
            ToLine(AsList(sections), geomFactory);

        private static Geometry ToLine(List<OffsetCurveSection> sections, GeometryFactory geomFactory)
        {
            if (sections.Count == 0)
                return geomFactory.CreateLineString();
            if (sections.Count == 1)
                return geomFactory.CreateLineString(sections[0].Coordinates);

            //-- sort sections in order along the offset curve
            sections.Sort();
            
            var pts = new CoordinateList();

            bool removeStartPt = false;
            for (int i = 0; i < sections.Count; i++)
            {
                var section = sections[i];

                bool removeEndPt = false;
                if (i < sections.Count - 1)
                {
                    double nextStartLoc = sections[i + 1].Location;
                    removeEndPt = section.IsEndInSameSegment(nextStartLoc);
                }
                var sectionPts = section.Coordinates;
                for (int j = 0; j < sectionPts.Length; j++)
                {
                    if ((removeStartPt && j == 0) || (removeEndPt && j == sectionPts.Length - 1))
                        continue;
                    pts.Add(sectionPts[j], false);
                }
                removeStartPt = removeEndPt;
            }
            return geomFactory.CreateLineString(pts.ToCoordinateArray());
        }

        private static List<OffsetCurveSection> AsList(IEnumerable<OffsetCurveSection> sections)
        {
            if (sections is List<OffsetCurveSection> lst) return lst;
            return new List<OffsetCurveSection>(sections);
        }

        public static OffsetCurveSection Create(Coordinate[] srcPts, int start, int end, double loc, double locLast)
        {
            int len = end - start + 1;
            if (end <= start)
                len = srcPts.Length - start + end;

            var sectionPts = new Coordinate[len];
            for (int i = 0; i < len; i++)
            {
                int index = (start + i) % (srcPts.Length - 1);
                sectionPts[i] = srcPts[index].Copy();
            }
            return new OffsetCurveSection(sectionPts, loc, locLast);
        }

        private readonly Coordinate[] _sectionPts;
        private double _location;
        private double _locLast;

        OffsetCurveSection(Coordinate[] pts, double loc, double locLast)
        {
            _sectionPts = pts;
            _location = loc;
            _locLast = locLast;
        }

        private Coordinate[] Coordinates => _sectionPts;

        private double Location => _location;

        private bool IsEndInSameSegment(double nextLoc)
        {
            int segIndex = (int)_locLast;
            int nextIndex = (int)nextLoc;
            return segIndex == nextIndex;
        }

        /**
         * Orders sections by their location along the raw offset curve.
         */
        public int CompareTo(OffsetCurveSection section)
        {
            return _location.CompareTo(section._location);
        }

    }
}
