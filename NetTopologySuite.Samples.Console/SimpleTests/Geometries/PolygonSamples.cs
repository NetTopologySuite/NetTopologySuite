using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GeoAPI.Operations.Buffer;
#if BUFFERED
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#else
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
#endif
using NetTopologySuite.IO.WellKnownBinary;

namespace NetTopologySuite.Samples.SimpleTests.Geometries
{
    public class PolygonSamples : BaseSamples
    {
        private readonly ILinearRing hole;
        private readonly IPolygon polygon;
        private readonly ILinearRing shell;

        public PolygonSamples()
            : base(GeometryServices.GetGeometryFactory(PrecisionModelType.Fixed))
        {
            shell = GeoFactory.CreateLinearRing(new ICoordinate[]
                                                    {
                                                        CoordFactory.Create(100, 100),
                                                        CoordFactory.Create(200, 100),
                                                        CoordFactory.Create(200, 200),
                                                        CoordFactory.Create(100, 200),
                                                        CoordFactory.Create(100, 100),
                                                    });

            hole = GeoFactory.CreateLinearRing(new ICoordinate[]
                                                   {
                                                       CoordFactory.Create(120, 120),
                                                       CoordFactory.Create(180, 120),
                                                       CoordFactory.Create(180, 180),
                                                       CoordFactory.Create(120, 180),
                                                       CoordFactory.Create(120, 120),
                                                   });

            polygon = GeoFactory.CreatePolygon(shell, new[] {hole,});
        }

        public override void Start()
        {
            IPoint interiorPoint = GeoFactory.CreatePoint(CoordFactory.Create(130, 150));
            IPoint exteriorPoint = GeoFactory.CreatePoint(CoordFactory.Create(650, 1500));
            ILineString aLine =
                GeoFactory.CreateLineString(new ICoordinate[]
                                                {
                                                    CoordFactory.Create(23, 32.2),
                                                    CoordFactory.Create(10, 222)
                                                });
            ILineString anotherLine =
                GeoFactory.CreateLineString(new ICoordinate[] {CoordFactory.Create(0, 1), CoordFactory.Create(30, 30)});
            ILineString intersectLine =
                GeoFactory.CreateLineString(new ICoordinate[]
                                                {
                                                    CoordFactory.Create(0, 1),
                                                    CoordFactory.Create(300, 300)
                                                });

            try
            {
                Write(polygon.Area);
                Write(polygon.Boundary);
                Write(polygon.BoundaryDimension);
                Write(polygon.Centroid);
                //Write(polygon.Coordinate);
                Write(polygon.Coordinates.Count);
                Write(polygon.Dimension);
                Write(polygon.Envelope);
                Write(polygon.Extents);
                //Write(polygon.EnvelopeInternal);
                Write(polygon.ExteriorRing);
                Write(polygon.PointOnSurface);
                Write(polygon.InteriorRingsCount);
                Write(polygon.IsEmpty);
                Write(polygon.IsSimple);
                Write(polygon.IsValid);
                //Write(polygon.Length);
                //Write(polygon.NumInteriorRings);
                Write(polygon.PointCount);
                if (polygon.UserData != null)
                {
                    Write(polygon.UserData);
                }
                else
                {
                    Write("UserData null");
                }

                Write(polygon.Buffer(10));
                Write(polygon.Buffer(10, BufferStyle.Butt));
                Write(polygon.Buffer(10, BufferStyle.Square));
                Write(polygon.Buffer(10, 20));
                Write(polygon.Buffer(10, 20, BufferStyle.Butt));
                Write(polygon.Buffer(10, 20, BufferStyle.Square));
                Write(polygon.Contains(interiorPoint));
                Write(polygon.Contains(exteriorPoint));
                Write(polygon.Contains(aLine));
                Write(polygon.Contains(anotherLine));
                Write(polygon.Crosses(interiorPoint));
                Write(polygon.Crosses(exteriorPoint));
                Write(polygon.Crosses(aLine));
                Write(polygon.Crosses(anotherLine));
                Write(polygon.Difference(interiorPoint));
                Write(polygon.Difference(exteriorPoint));
                Write(polygon.Difference(aLine));
                Write(polygon.Difference(anotherLine));
                Write(polygon.Disjoint(interiorPoint));
                Write(polygon.Disjoint(exteriorPoint));
                Write(polygon.Disjoint(aLine));
                Write(polygon.Disjoint(anotherLine));
                Write(polygon.Distance(interiorPoint));
                Write(polygon.Distance(exteriorPoint));
                Write(polygon.Distance(aLine));
                Write(polygon.Distance(anotherLine));
                Write(polygon.Intersection(interiorPoint));
                Write(polygon.Intersection(exteriorPoint));
                Write(polygon.Intersection(aLine));
                Write(polygon.Intersection(anotherLine));
                Write(polygon.Intersects(interiorPoint));
                Write(polygon.Intersects(exteriorPoint));
                Write(polygon.Intersects(aLine));
                Write(polygon.Intersects(anotherLine));
                Write(polygon.IsWithinDistance(interiorPoint, 300));
                Write(polygon.IsWithinDistance(exteriorPoint, 300));
                Write(polygon.IsWithinDistance(aLine, 300));
                Write(polygon.IsWithinDistance(anotherLine, 300));
                Write(polygon.Overlaps(interiorPoint));
                Write(polygon.Overlaps(exteriorPoint));
                Write(polygon.Overlaps(aLine));
                Write(polygon.Overlaps(anotherLine));
                Write(polygon.Relate(interiorPoint));
                Write(polygon.Relate(exteriorPoint));
                Write(polygon.Relate(aLine));
                Write(polygon.Relate(anotherLine));
                Write(polygon.SymmetricDifference(interiorPoint));
                Write(polygon.SymmetricDifference(exteriorPoint));
                Write(polygon.SymmetricDifference(aLine));
                Write(polygon.SymmetricDifference(anotherLine));
                Write(polygon.ToString());
                Write(polygon.AsText());
                Write(polygon.Touches(interiorPoint));
                Write(polygon.Touches(exteriorPoint));
                Write(polygon.Touches(aLine));
                Write(polygon.Touches(anotherLine));
                Write(polygon.Union(interiorPoint));
                Write(polygon.Union(exteriorPoint));
                Write(polygon.Union(aLine));
                Write(polygon.Union(anotherLine));

                String aPoly = "POLYGON ((20 20, 100 20, 100 100, 20 100, 20 20))";
                String anotherPoly =
                    "POLYGON ((20 20, 100 20, 100 100, 20 100, 20 20), (50 50, 60 50, 60 60, 50 60, 50 50))";
                IGeometry geom1 = Reader.Read(aPoly);
                Write(geom1.AsText());
                IGeometry geom2 = Reader.Read(anotherPoly);
                Write(geom2.AsText());

                // ExpandToInclude tests
                IExtents<coord> extents = GeoFactory.CreateExtents(
                    CoordFactory.Create(0, 0),
                    CoordFactory.Create(0, 0));
                extents.ExpandToInclude(geom1.Extents);
                extents.ExpandToInclude(geom2.Extents);
                Write(extents.ToString());

                // The polygon is not correctly ordered! Calling normalize we fix the problem...
                polygon.Normalize();

                Byte[] bytes = polygon.AsBinary();
                IGeometry test1 = new WkbReader<coord>(GeoFactory).Read(bytes);
                Write(test1.ToString());

                //bytes = new GDBWriter().Write(polygon);
                //test1 = new GDBReader().Read(bytes);
                Write(test1.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}