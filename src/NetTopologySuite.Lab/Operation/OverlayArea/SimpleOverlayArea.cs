using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Operation.OverlayArea
{
    /// <summary>
    /// Computes the result area of an overlay usng the Overlay-Area, for simple polygons.
    /// Simple polygons have no holes.
    /// No indexing is used.
    /// This is faster than <see cref="OverlayArea"/> for polygons with low vertex count.
    /// </summary>
    /// <author>Martin Davis</author>
    public class SimpleOverlayArea
    {
        /// <summary>
        /// Computes the area of intersection of two polygons with no holes.
        /// </summary>
        /// <param name="poly0">A polygon</param>
        /// <param name="poly1">A polygon</param>
        /// <returns>The area of the intersection of the polygons</returns>
        public static double IntersectionArea(Polygon poly0, Polygon poly1)
        {
            var area = new SimpleOverlayArea(poly0, poly1);
            return area.Area;
        }

        private readonly Polygon _geomA;
        private readonly Polygon _geomB;

        public SimpleOverlayArea(Polygon geom0, Polygon geom1)
        {
            _geomA = geom0;
            _geomB = geom1;
            //TODO: error if polygon has holes
        }

        public double Area
        {
            get
            {
                if (_geomA.NumInteriorRings > 0 ||
                    _geomB.NumInteriorRings > 0)
                {
                    throw new InvalidOperationException("Polygons wtih holes are not supported");
                }

                var ringA = GetVertices(_geomA);
                var ringB = GetVertices(_geomB);

                bool isCCWA = Orientation.IsCCW(ringA);
                bool isCCWB = Orientation.IsCCW(ringB);

                double areaInt = areaForIntersections(ringA, isCCWA, ringB, isCCWB);
                double areaVert0 = areaForInteriorVertices(ringA, isCCWA, ringB);
                double areaVert1 = areaForInteriorVertices(ringB, isCCWB, ringA);

                return (areaInt + areaVert1 + areaVert0) / 2;
            }
        }

        private static CoordinateSequence GetVertices(Geometry geom)
        {
            var poly = (Polygon)geom;
            var seq = poly.ExteriorRing.CoordinateSequence;
            return seq;
        }

        private double areaForIntersections(CoordinateSequence ringA, bool isCCWA, CoordinateSequence ringB, bool isCCWB)
        {
            //TODO: use fast intersection computation?

            // Compute rays for all intersections
            LineIntersector li = new RobustLineIntersector();

            double area = 0;
            for (int i = 0; i < ringA.Count - 1; i++)
            {
                var a0 = ringA.GetCoordinate(i);
                var a1 = ringA.GetCoordinate(i + 1);

                if (isCCWA)
                {
                    // flip segment orientation
                    var temp = a0; a0 = a1; a1 = temp;
                }

                for (int j = 0; j < ringB.Count - 1; j++)
                {
                    var b0 = ringB.GetCoordinate(j);
                    var b1 = ringB.GetCoordinate(j + 1);

                    if (isCCWB)
                    {
                        // flip segment orientation
                        var temp = b0; b0 = b1; b1 = temp;
                    }

                    li.ComputeIntersection(a0, a1, b0, b1);
                    if (li.HasIntersection)
                    {

                        /*
                         * With both rings oriented CW (effectively)
                         * There are two situations for segment intersections:
                         * 
                         * 1) A entering B, B exiting A => rays are IP-A1:R, IP-B0:L
                         * 2) A exiting B, B entering A => rays are IP-A0:L, IP-B1:R
                         * (where :L/R indicates result is to the Left or Right).
                         * 
                         * Use full edge to compute direction, for accuracy.
                         */
                        var intPt = li.GetIntersection(0);

                        bool isAenteringB = OrientationIndex.CounterClockwise == Orientation.Index(a0, a1, b1);

                        if (isAenteringB)
                        {
                            area += EdgeVector.Area2Term(intPt, a0, a1, true);
                            area += EdgeVector.Area2Term(intPt, b1, b0, false);
                        }
                        else
                        {
                            area += EdgeVector.Area2Term(intPt, a1, a0, false);
                            area += EdgeVector.Area2Term(intPt, b0, b1, true);
                        }
                    }
                }
            }
            return area;
        }

        private double areaForInteriorVertices(CoordinateSequence ring, bool isCCW, CoordinateSequence ring2)
        {
            double area = 0;
            /*
             * Compute rays originating at vertices inside the resultant
             * (i.e. A vertices inside B, and B vertices inside A)
             */
            for (int i = 0; i < ring.Count - 1; i++)
            {
                var vPrev = i == 0 ? ring.GetCoordinate(ring.Count - 2) : ring.GetCoordinate(i - 1);
                var v = ring.GetCoordinate(i);
                var vNext = ring.GetCoordinate(i + 1);
                var loc = RayCrossingCounter.LocatePointInRing(v, ring2);
                if (loc == Location.Interior)
                {
                    area += EdgeVector.Area2Term(v, vPrev, isCCW);
                    area += EdgeVector.Area2Term(v, vNext, !isCCW);
                }
            }
            return area;
        }

    }

}
