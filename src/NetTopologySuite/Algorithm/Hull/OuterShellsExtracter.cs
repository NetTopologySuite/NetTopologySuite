using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Algorithm.Hull
{
    /// <summary>
    /// Extracts the rings of outer shells from a polygonal geometry.
    /// Outer shells are the shells of polygon elements which
    /// are not nested inside holes of other polygons.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OuterShellsExtracter
    {

        public static LinearRing[] ExtractShells(Geometry polygons)
        {
            var extracter = new OuterShellsExtracter(polygons);
            return extracter.ExtractShells();
        }

        private readonly Geometry _polygons;

        public OuterShellsExtracter(Geometry polygons)
        {
            _polygons = polygons;
        }

        private LinearRing[] ExtractShells()
        {
            var shells = extractShellRings(_polygons);
            /**
             * sort shells in order of increasing envelope area
             * to ensure that shells are added before any of their inner shells
             */
            Array.Sort(shells, new EnvelopeAreaComparator());
            var outerShells = new List<LinearRing>();
            for (int i = shells.Length - 1; i >= 0; i--)
            {
                var shell = shells[i];
                if (outerShells.Count == 0
                    || IsOuter(shell, outerShells))
                {
                    outerShells.Add(shell);
                }
            }
            return GeometryFactory.ToLinearRingArray(outerShells);
        }

        private bool IsOuter(LinearRing shell, List<LinearRing> outerShells)
        {
            foreach (var outShell in outerShells)
            {
                if (Covers(outShell, shell))
                {
                    return false;
                }
            }
            return true;
        }

        private bool Covers(LinearRing shellA, LinearRing shellB)
        {
            //-- if shellB envelope is not covered then shell is not covered
            if (!shellA.EnvelopeInternal.Covers(shellB.EnvelopeInternal))
                return false;
            //-- if a shellB point lies inside shellA, shell is covered (since shells do not overlap)
            if (IsPointInRing(shellB, shellA))
                return true;
            return false;
        }

        private bool IsPointInRing(LinearRing shell, LinearRing shellRing)
        {
            //TODO: optimize this with cached index
            var pt = shell.Coordinate;
            return PointLocation.IsInRing(pt, shellRing.CoordinateSequence);
        }

        private static LinearRing[] extractShellRings(Geometry polygons)
        {
            var rings = new LinearRing[polygons.NumGeometries];
            for (int i = 0; i < polygons.NumGeometries; i++)
            {
                var consPoly = (Polygon)polygons.GetGeometryN(i);
                rings[i] = (LinearRing)consPoly.ExteriorRing.Copy();
            }
            return rings;
        }

        private class EnvelopeAreaComparator : IComparer<Geometry>
        {

            public int Compare(Geometry o1, Geometry o2)
            {
                return EnvArea(o1).CompareTo(EnvArea(o2));
            }

            private static double EnvArea(Geometry g)
            {
                return g.EnvelopeInternal.Area;
            }

        }
    }
}
