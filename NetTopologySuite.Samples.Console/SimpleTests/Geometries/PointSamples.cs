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
    public class PointSamples : BaseSamples
    {
        private IPoint point = null;        

        /// <summary>
        /// 
        /// </summary>
        public PointSamples() : base()
        {            
            point = Factory.CreatePoint(new Coordinate(100, 100));
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            IPoint pInterior = Factory.CreatePoint(new Coordinate(100, 100));
            IPoint pExterior = Factory.CreatePoint(new Coordinate(100, 101));

            try
            {
                Write(point.Area);                
                Write(point.Boundary);
                Write(point.BoundaryDimension);                                
                Write(point.Centroid);
                Write(point.Coordinate);
                Write(point.Coordinates);
                Write(point.CoordinateSequence);
                Write(point.Dimension);
                Write(point.Envelope);
                Write(point.EnvelopeInternal);
                Write(point.Factory);
                Write(point.InteriorPoint);
                Write(point.IsEmpty);
                Write(point.IsSimple);
                Write(point.IsValid);
                Write(point.Length);
                Write(point.NumPoints);
                Write(point.PrecisionModel);                          
                Write(point.X);
                Write(point.Y);                               

                Write(point.Contains(pInterior));
                Write(point.Contains(pExterior));

                Write(point.Buffer(10));
                Write(point.Buffer(10, BufferStyle.CapSquare));
                Write(point.Buffer(10, BufferStyle.CapButt));
                Write(point.Buffer(10, 20));                
                Write(point.Buffer(10, 20, BufferStyle.CapSquare));                
                Write(point.Buffer(10, 20, BufferStyle.CapButt)); 

                Write(point.Crosses(pInterior));
                Write(point.Crosses(pExterior));
                Write(point.Difference(pInterior));
                Write(point.Difference(pExterior));   
                Write(point.Disjoint(pInterior));
                Write(point.Disjoint(pExterior));
                Write(point.Equals(pInterior));
                Write(point.Equals(pExterior));
                Write(point.EqualsExact(pInterior));
                Write(point.EqualsExact(pExterior));
                Write(point.ConvexHull());
                Write(point.Intersection(pInterior)); 
                Write(point.Intersection(pExterior)); 
                Write(point.Intersects(pInterior));
                Write(point.Intersects(pExterior));
                Write(point.IsWithinDistance(pInterior, 0.001));
                Write(point.IsWithinDistance(pExterior, 0.001));                
                Write(point.Overlaps(pInterior));
                Write(point.Overlaps(pExterior));
                Write(point.SymmetricDifference(pInterior));
                Write(point.SymmetricDifference(pExterior));
                Write(point.ToString());
                Write(point.AsText());                
                Write(point.Touches(pInterior));
                Write(point.Touches(pExterior));
                Write(point.Union(pInterior));                 
                Write(point.Union(pExterior));         
                Write(point.Within(pInterior));                        
                Write(point.Within(pExterior));
                                
                string pointstring = "POINT (100.22 100.33)";
                string anotherpointstring = "POINT (12345 3654321)";
                IGeometry geom1 = Reader.Read(pointstring);
                Write(geom1.AsText());
                IGeometry geom2 = Reader.Read(anotherpointstring);
                Write(geom2.AsText());

                byte[] bytes = point.AsBinary();
                IGeometry test1 = new WKBReader().Read(bytes);
                Write(test1.ToString());
                
                bytes = Factory.CreatePoint(new Coordinate(Double.MinValue, Double.MinValue)).AsBinary();
                IGeometry testempty = new WKBReader().Read(bytes);
                Write(testempty);

                bytes = new GDBWriter().Write(geom1);
                test1 = new GDBReader().Read(bytes);
                Write(test1.ToString());                 
   
                // Test Empty Geometries
                Write(Point.Empty);
                Write(LineString.Empty);
                Write(Polygon.Empty);
                Write(MultiPoint.Empty);
                Write(MultiLineString.Empty);
                Write(MultiPolygon.Empty);
                Write(GeometryCollection.Empty);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }       
    }    
}
