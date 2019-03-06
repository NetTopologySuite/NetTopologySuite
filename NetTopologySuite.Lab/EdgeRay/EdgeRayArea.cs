using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.EdgeRay
{
    public class EdgeRayArea
    {
        public static double GetArea(IGeometry geom)
        {
            var area = new EdgeRayArea(geom);
            return area.Area;
        }

        private readonly IGeometry _geom;

        public EdgeRayArea(IGeometry geom)
        {
            _geom = geom;
        }

        public double Area
        {
            get
            {
                var poly = (IPolygon)_geom;
                var seq = poly.ExteriorRing.CoordinateSequence;
                bool isCW = !Orientation.IsCCW(seq);

                // TODO: for now assume poly is CW

                // scan every segment
                double area = 0;
                for (int i = 1; i < seq.Count; i++)
                {
                    int i0 = i - 1;
                    int i1 = i;
                    /*
                    area += EdgeRay.SreaTermBoth(seq.GetX(i0), seq.GetY(i0),
                        seq.GetX(i1), seq.GetY(i1));
                        */
                    area += EdgeRay.AreaTerm(seq.GetX(i0), seq.GetY(i0),
                        seq.GetX(i1), seq.GetY(i1), isCW);
                    area += EdgeRay.AreaTerm(seq.GetX(i1), seq.GetY(i1),
                        seq.GetX(i0), seq.GetY(i0), !isCW);
                }

                return area;
            }
        }
    }
}
