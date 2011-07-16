using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;
using System.Text;

namespace NetTopologySuite.Algorithm.Match
{
    public class AreaSimilarityMeasure<TCoordinate> : ISimilarityMeasure<TCoordinate>
            where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                                IComparable<TCoordinate>, IConvertible,
                                IComputable<Double, TCoordinate>
    {
        /*
        public static double measure(Geometry a, Geometry b)
        {
            AreaSimilarityMeasure gv = new AreaSimilarityMeasure(a, b);
            return gv.measure();
        }

        public AreaSimilarityMeasure()
        {
        }
        */

        public Double Measure(IGeometry<TCoordinate> g1, IGeometry<TCoordinate> g2)
        {
            Double areaInt = GetArea(g1.Intersection(g2));
            Double areaUnion = GetArea(g1.Union(g2));
            return areaInt / areaUnion;
        }

        private Double GetArea(IGeometry<TCoordinate> geom)
        {
            ISurface<TCoordinate> s = geom as ISurface<TCoordinate>;
            if (s != null)
                return s.Area;

            IMultiSurface<TCoordinate> ms = geom as IMultiSurface<TCoordinate>;
            if (ms != null)
                return ms.Area;

            throw new ArgumentException("geom has no area"); ;
        }

    }
}
