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
    public class MultiPointSamples : BaseSamples
    {
        private IMultiPoint multiPoint = null;

        /// <summary>
        /// 
        /// </summary>
        public MultiPointSamples() : base()
        {
            ICoordinate[] coordinates = new ICoordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,200),
                new Coordinate(300,300),                
                new Coordinate(400,400),
                new Coordinate(500,500),
            };
            multiPoint = Factory.CreateMultiPoint(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            try
            {
                Write(multiPoint.Area);                
                Write(multiPoint.Boundary);
                Write(multiPoint.BoundaryDimension);
                Write(multiPoint.Centroid);
                Write(multiPoint.Coordinate);
                Write(multiPoint.Coordinates);
                Write(multiPoint.Dimension);
                Write(multiPoint.Envelope);
                Write(multiPoint.EnvelopeInternal);
                Write(multiPoint.Geometries.Length);
                Write(multiPoint.InteriorPoint);
                Write(multiPoint.IsEmpty);
                Write(multiPoint.IsSimple);
                Write(multiPoint.IsValid);
                Write(multiPoint.Length);
                Write(multiPoint.NumGeometries);
                Write(multiPoint.NumPoints);
                
                Write(multiPoint.Buffer(10));
                Write(multiPoint.Buffer(10, BufferStyle.CapButt));
                Write(multiPoint.Buffer(10, BufferStyle.CapSquare));
                Write(multiPoint.Buffer(10, 20));
                Write(multiPoint.Buffer(10, 20, BufferStyle.CapButt));
                Write(multiPoint.Buffer(10, 20, BufferStyle.CapSquare));
                Write(multiPoint.ConvexHull()); 
               
                byte[] bytes = multiPoint.AsBinary();
                IGeometry test1 = new WKBReader().Read(bytes);
                Write(test1.ToString());

                bytes = new GDBWriter().Write(multiPoint);
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
