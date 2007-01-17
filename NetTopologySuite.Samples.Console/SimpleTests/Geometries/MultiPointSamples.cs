using System;
using System.Collections;
using System.Text;
using System.Xml;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class MultiPointSamples : BaseSamples
    {
        private MultiPoint multiPoint = null;

        /// <summary>
        /// 
        /// </summary>
        public MultiPointSamples() : base()
        {
            Coordinate[] coordinates = new Coordinate[]
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
                Write(multiPoint.Factory);
                Write(multiPoint.Geometries.Length);
                Write(multiPoint.InteriorPoint);
                Write(multiPoint.IsEmpty);
                Write(multiPoint.IsSimple);
                Write(multiPoint.IsValid);
                Write(multiPoint.Length);
                Write(multiPoint.NumGeometries);
                Write(multiPoint.NumPoints);
                Write(multiPoint.PrecisionModel);        
                
                Write(multiPoint.Buffer(10));
                Write(multiPoint.Buffer(10, BufferStyles.CapButt));
                Write(multiPoint.Buffer(10, BufferStyles.CapSquare));
                Write(multiPoint.Buffer(10, 20));
                Write(multiPoint.Buffer(10, 20, BufferStyles.CapButt));
                Write(multiPoint.Buffer(10, 20, BufferStyles.CapSquare));
                Write(multiPoint.ConvexHull()); 
               
                byte[] bytes = multiPoint.ToBinary();
                Geometry test1 = new WKBReader().Read(bytes);
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
