using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class LineStringSamples : BaseSamples
    {
        private ILineString line = null;       

        /// <summary>
        /// 
        /// </summary>
        public LineStringSamples() : base()            
        {
            ICoordinate[] coordinates = new ICoordinate[]
            {
                 new Coordinate(10, 10),
                 new Coordinate(20, 20),
                 new Coordinate(20, 10),                 
            };
            line = Factory.CreateLineString(coordinates);
        }        

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            IPoint pointInLine = Factory.CreatePoint(new Coordinate(20, 10));
            IPoint pointOutLine = Factory.CreatePoint(new Coordinate(20, 31));
            ILineString aLine = Factory.CreateLineString(new ICoordinate[] { new Coordinate(23, 32.2), new Coordinate(922, 11) });
            ILineString anotherLine = Factory.CreateLineString(new ICoordinate[] { new Coordinate(0, 1), new Coordinate(30, 30) });

            try
            {
                Write(line.Area);                
                Write(line.Boundary);
                Write(line.BoundaryDimension);                
                Write(line.Centroid);
                Write(line.Coordinate);
                Write(line.Coordinates);
                Write(line.CoordinateSequence);
                Write(line.Dimension);
                Write(line.EndPoint);
                Write(line.Envelope);
                Write(line.EnvelopeInternal);                
                Write(line.InteriorPoint);
                Write(line.IsClosed);
                Write(line.IsEmpty);
                Write(line.IsRing);
                Write(line.IsSimple);
                Write(line.IsValid);
                Write(line.Length);
                Write(line.NumPoints);                
                Write(line.StartPoint);
                if (line.UserData != null)
                    Write(line.UserData);
                else Write("UserData null");                 

                Write(line.Buffer(10));
                Write(line.Buffer(10, BufferStyle.CapButt));
                Write(line.Buffer(10, BufferStyle.CapSquare));
                Write(line.Buffer(10, 20));                
                Write(line.Buffer(10, 20, BufferStyle.CapButt));                
                Write(line.Buffer(10, 20, BufferStyle.CapSquare));   
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
                Write(line.Equals(line.Clone() as LineString));
                Write(line.EqualsExact(line.Clone() as LineString));
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
                
                string linestring = "LINESTRING (1.2 3.4, 5.6 7.8, 9.1 10.12)";
                string anotherlinestringg = "LINESTRING (12345 3654321, 685 7777.945677, 782 111.1)";
                IGeometry geom1 = Reader.Read(linestring);
                Write(geom1.AsText());
                IGeometry geom2 = Reader.Read(anotherlinestringg);
                Write(geom2.AsText());

                byte[] bytes = line.AsBinary();
                IGeometry test1 = new WKBReader().Read(bytes);
                Write(test1.ToString());

                bytes = new GDBWriter().Write(line);
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
