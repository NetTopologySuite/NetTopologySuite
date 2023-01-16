using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayArea
{
    /// <summary>
    /// Computes the area of a geometry using the <see cref="EdgeVector"/> summing
    /// approach.
    /// This provides a validation of the correctness of the <see cref="OverlayArea"/> approach.
    /// It is not intended to replace the standard polygon area computation.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryArea
    {

        public static double Compute(Geometry geom)
        {
            var area = new GeometryArea(geom);
            return area.Area;
        }

        private readonly Geometry _geom;

        public GeometryArea(Geometry geom)
        {
            _geom = geom;
        }

        private double Area
        {
            get
            {
                var filter = new PolygonAreaFilter();
                _geom.Apply(filter);
                return filter.Area;
            }
        }

        private class PolygonAreaFilter : IGeometryFilter
        {
            double _area = 0;
        public void Filter(Geometry geom)
        {
            if (geom is Polygon poly) {
                _area += AreaPolygon(poly);
            }
        }

            public double Area => _area;
    }

    private static double AreaPolygon(Polygon geom)
    {
        double area = AreaRing(geom.ExteriorRing);
        for (int i = 0; i < geom.NumInteriorRings; i++)
        {
            var hole = geom.GetInteriorRingN(i);
            area -= AreaRing(hole);
        }
        return area;
    }

    private static double AreaRing(LineString ring)
    {
        // TODO: handle hole rings, multiPolygons
        var seq = ring.CoordinateSequence;
        bool isCW = !Orientation.IsCCW(seq);

        // scan every segment
        double area = 0;
        for (int i = 1; i < seq.Count; i++)
        {
            int i0 = i - 1;
            int i1 = i;
            /*
             * Sum the partial areas for the two
             * opposing SegmentVectors representing the edge.
             * If the ring is oriented CW then the interior is to the right of the vector,
             * and the opposing vector is opposite.
             */
            area += EdgeVector.Area2Term(
                seq.GetX(i0), seq.GetY(i0),
                seq.GetX(i1), seq.GetY(i1),
                isCW)
                + EdgeVector.Area2Term(
                    seq.GetX(i1), seq.GetY(i1),
                    seq.GetX(i0), seq.GetY(i0),
                    !isCW);
        }
        return area / 2;
    }
}}
