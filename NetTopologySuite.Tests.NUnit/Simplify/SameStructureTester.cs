using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Simplify
{
    public class SameStructureTester
    {
        public static Boolean IsSameStructure(IGeometry<Coordinate> g1, IGeometry<Coordinate> g2)
        {
            if (g1.GetType() != g2.GetType())
                return false;
            if (g1 is IGeometryCollection<Coordinate>)
                return IsSameStructureCollection((IGeometryCollection<Coordinate>) g1,
                                                 (IGeometryCollection<Coordinate>) g2);
            if (g1 is IPolygon<Coordinate>)
                return IsSameStructurePolygon(( IPolygon<Coordinate>) g1, ( IPolygon<Coordinate>) g2);
            if (g1 is ILineString<Coordinate>)
                return isSameStructureLineString((ILineString<Coordinate>) g1, (ILineString<Coordinate>) g2);
            if (g1 is IPoint<Coordinate>)
                return isSameStructurePoint((IPoint<Coordinate>) g1, (IPoint<Coordinate>) g2);

            throw new Exception(
                "Unsupported Geometry class: " + g1.GetType().Name);
            return false;
        }

        private static Boolean IsSameStructureCollection(IGeometryCollection<Coordinate> g1, IGeometryCollection<Coordinate> g2)
        {
            if (g1.Count != g2.Count)
                return false;

            IEnumerator<IGeometry<Coordinate>> itg2 = g2.GetEnumerator();
            foreach (IGeometry<Coordinate> geometry in g1)
            {
                itg2.MoveNext();
                if (!IsSameStructure(g1, itg2.Current))
                    return false;
            }
            return true;
        }

        private static Boolean IsSameStructurePolygon(IPolygon<Coordinate> g1, IPolygon<Coordinate> g2)
        {
            if (g1.InteriorRingsCount != g2.InteriorRingsCount)
                return false;
            // could check for both empty or nonempty here
            return true;
        }

        private static Boolean isSameStructureLineString(ILineString<Coordinate> g1, ILineString<Coordinate> g2)
        {
            // could check for both empty or nonempty here
            return true;
        }

        private static Boolean isSameStructurePoint(IPoint<Coordinate> g1, IPoint<Coordinate> g2)
        {
            // could check for both empty or nonempty here
            return true;
        }
        
    }
}