using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    public class PolygonSamples : BaseSamples
    {
        private IPolygon polygon = null;
        private ILinearRing shell = null;
        private ILinearRing hole = null;

        /// <summary>
        /// 
        /// </summary>
        public PolygonSamples() : base(new GeometryFactory(new PrecisionModel(PrecisionModels.Fixed)))
        {
            shell = Factory.CreateLinearRing(new ICoordinate[] { new Coordinate(100,100),
                                                                 new Coordinate(200,100),
                                                                 new Coordinate(200,200),                
                                                                 new Coordinate(100,200),
                                                                 new Coordinate(100,100), });
            hole = Factory.CreateLinearRing(new ICoordinate[] {  new Coordinate(120,120),
                                                                 new Coordinate(180,120),
                                                                 new Coordinate(180,180),                                                                                
                                                                 new Coordinate(120,180),                                                                
                                                                 new Coordinate(120,120), });
            polygon = Factory.CreatePolygon(shell, new ILinearRing[] { hole, });
        }

        public override void Start()
        {
            IPoint interiorPoint = Factory.CreatePoint(new Coordinate(130, 150));
            IPoint exteriorPoint = Factory.CreatePoint(new Coordinate(650, 1500));
            ILineString aLine = Factory.CreateLineString(new ICoordinate[] { new Coordinate(23, 32.2), new Coordinate(10, 222) });
            ILineString anotherLine = Factory.CreateLineString(new ICoordinate[] { new Coordinate(0, 1), new Coordinate(30, 30) });
            ILineString intersectLine = Factory.CreateLineString(new ICoordinate[] { new Coordinate(0, 1), new Coordinate(300, 300) });            

            try
            {               
                Write(polygon.Area);
                Write(polygon.Boundary);
                Write(polygon.BoundaryDimension);
                Write(polygon.Centroid);
                Write(polygon.Coordinate);
                Write(polygon.Coordinates.Length);
                Write(polygon.Dimension);
                Write(polygon.Envelope);
                Write(polygon.EnvelopeInternal);
                Write(polygon.ExteriorRing);
                Write(polygon.InteriorPoint);
                Write(polygon.InteriorRings.Length);
                Write(polygon.IsEmpty);
                Write(polygon.IsSimple);
                Write(polygon.IsValid);
                Write(polygon.Length);
                Write(polygon.NumInteriorRings);
                Write(polygon.NumPoints);                
                if (polygon.UserData != null)
                    Write(polygon.UserData);
                else Write("UserData null");
             
                Write(polygon.Buffer(10));
                Write(polygon.Buffer(10, BufferStyle.CapButt));                
                Write(polygon.Buffer(10, BufferStyle.CapSquare));
                Write(polygon.Buffer(10, 20));
                Write(polygon.Buffer(10, 20, BufferStyle.CapButt));                
                Write(polygon.Buffer(10, 20, BufferStyle.CapSquare));
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

                string aPoly = "POLYGON ((20 20, 100 20, 100 100, 20 100, 20 20))";
                string anotherPoly = "POLYGON ((20 20, 100 20, 100 100, 20 100, 20 20), (50 50, 60 50, 60 60, 50 60, 50 50))";                
                IGeometry geom1 = Reader.Read(aPoly);
                Write(geom1.AsText());                                
                IGeometry geom2 = Reader.Read(anotherPoly);
                Write(geom2.AsText());

                // ExpandToInclude tests
                Envelope envelope = new Envelope(0, 0, 0, 0);
                envelope.ExpandToInclude(geom1.EnvelopeInternal);
                envelope.ExpandToInclude(geom2.EnvelopeInternal);
                Write(envelope.ToString());

                // The polygon is not correctly ordered! Calling normalize we fix the problem...
                polygon.Normalize();

                byte[] bytes = polygon.AsBinary();
                IGeometry test1 = new WKBReader().Read(bytes);
                Write(test1.ToString());

                bytes = new GDBWriter().Write(polygon);
                test1 = new GDBReader().Read(bytes);
                Write(test1.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }        
    }
}
