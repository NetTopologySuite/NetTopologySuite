using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GeoAPI.Operations.Buffer;
using NetTopologySuite.Coordinates;
using NetTopologySuite.IO.WellKnownBinary;

namespace NetTopologySuite.Samples.SimpleTests.Geometries
{
    public class LineStringSamples : BaseSamples
    {
        private readonly ILineString line;

        public LineStringSamples()
        {
            BufferedCoordinateFactory coordFactory = new BufferedCoordinateFactory();

            ICoordinate[] coordinates = new ICoordinate[]
                                            {
                                                coordFactory.Create(10, 10),
                                                coordFactory.Create(20, 20),
                                                coordFactory.Create(20, 10),
                                            };

            line = GeoFactory.CreateLineString(coordinates);
        }

        public override void Start()
        {
            BufferedCoordinateFactory coordFactory = new BufferedCoordinateFactory();

            IPoint pointInLine = GeoFactory.CreatePoint(coordFactory.Create(20, 10));
            IPoint pointOutLine = GeoFactory.CreatePoint(coordFactory.Create(20, 31));
            ICoordinate[] coords = new ICoordinate[]
                                       {
                                           coordFactory.Create(23, 32.2),
                                           coordFactory.Create(922, 11)
                                       };
            ILineString aLine = GeoFactory.CreateLineString(coords);

            coords = new ICoordinate[]
                         {
                             coordFactory.Create(0, 1),
                             coordFactory.Create(30, 30)
                         };
            ILineString anotherLine = GeoFactory.CreateLineString(coords);

            try
            {
                //Write(line.Area);
                Write(line.Boundary);
                Write(line.BoundaryDimension);
                Write(line.Centroid);
                //Write(line.Coordinate);
                Write(line.Coordinates);
                //Write(line.CoordinateSequence);
                Write(line.Dimension);
                Write(line.EndPoint);
                Write(line.Envelope);
                Write(line.Extents);
                //Write(line.EnvelopeInternal);
                //Write(line.InteriorPoint);
                Write(line.IsClosed);
                Write(line.IsEmpty);
                Write(line.IsRing);
                Write(line.IsSimple);
                Write(line.IsValid);
                Write(line.Length);
                Write(line.PointCount);
                Write(line.StartPoint);

                if (line.UserData != null)
                {
                    Write(line.UserData);
                }
                else
                {
                    Write("UserData null");
                }

                Write(line.Buffer(10));
                Write(line.Buffer(10, BufferStyle.Butt));
                Write(line.Buffer(10, BufferStyle.Square));
                Write(line.Buffer(10, 20));
                Write(line.Buffer(10, 20, BufferStyle.Butt));
                Write(line.Buffer(10, 20, BufferStyle.Square));
                Write(line.Contains(pointInLine));
                Write(line.Contains(pointOutLine));
                Write(line.Crosses(pointInLine));
                Write(line.Crosses(pointOutLine));
                Write(line.Difference(pointInLine));
                Write(line.Difference(pointOutLine));
                Write(line.Disjoint(pointInLine));
                Write(line.Disjoint(pointOutLine));
                Write(line.Distance(pointInLine));
                Write(line.Distance(pointOutLine));
                Write(line.Equals(line.Clone() as ILineString));
                //Write(line.EqualsExact(line.Clone() as LineString));
                Write(line.ConvexHull());
                Write(line.Intersection(pointInLine));
                Write(line.Intersection(pointOutLine));
                Write(line.Intersection(aLine));
                Write(line.Intersects(pointInLine));
                Write(line.Intersects(pointOutLine));
                Write(line.Intersects(aLine));
                Write(line.IsWithinDistance(pointOutLine, 2));
                Write(line.IsWithinDistance(pointOutLine, 222));
                Write(line.Overlaps(pointInLine));
                Write(line.Overlaps(pointOutLine));
                Write(line.Overlaps(aLine));
                Write(line.Overlaps(anotherLine));
                Write(line.Relate(pointInLine));
                Write(line.Relate(pointOutLine));
                Write(line.Relate(aLine));
                Write(line.Relate(anotherLine));
                Write(line.SymmetricDifference(pointInLine));
                Write(line.SymmetricDifference(pointOutLine));
                Write(line.SymmetricDifference(aLine));
                Write(line.SymmetricDifference(anotherLine));
                Write(line.ToString());
                Write(line.AsText());
                Write(line.Touches(pointInLine));
                Write(line.Touches(pointOutLine));
                Write(line.Touches(aLine));
                Write(line.Touches(anotherLine));
                Write(line.Union(pointInLine));
                Write(line.Union(pointOutLine));
                Write(line.Union(aLine));
                Write(line.Union(anotherLine));
                Write(line.Within(pointInLine));
                Write(line.Within(pointOutLine));
                Write(line.Within(aLine));
                Write(line.Within(anotherLine));

                String linestring = "LINESTRING (1.2 3.4, 5.6 7.8, 9.1 10.12)";
                String anotherlinestringg = "LINESTRING (12345 3654321, 685 7777.945677, 782 111.1)";
                IGeometry geom1 = Reader.Read(linestring);
                Write(geom1.AsText());
                IGeometry geom2 = Reader.Read(anotherlinestringg);
                Write(geom2.AsText());

                Byte[] bytes = line.AsBinary();
                IGeometry test1 = new WkbReader<BufferedCoordinate>(GeoFactory).Read(bytes);
                Write(test1.ToString());

                //bytes = new GDBWriter().Write(line);
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