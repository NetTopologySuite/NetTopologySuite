using System;
using System.Collections;
using System.Text;
using System.Xml;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownBinary;
using GeoAPI.Operations.Buffer;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Coordinates;

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
                CoordFactory.Create(100,100),
                CoordFactory.Create(200,200),
                CoordFactory.Create(300,300),                
                CoordFactory.Create(400,400),
                CoordFactory.Create(500,500),
            };

            multiPoint = GeoFactory.CreateMultiPoint(coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            try
            {
                //Write(multiPoint.Area);                
                Write(multiPoint.Boundary);
                Write(multiPoint.BoundaryDimension);
                Write(multiPoint.Centroid);
                //Write(multiPoint.Coordinate);
                Write(multiPoint.Coordinates);
                Write(multiPoint.Dimension);
                Write(multiPoint.Envelope);
                Write(multiPoint.Extents);
                //Write(multiPoint.EnvelopeInternal);
                //Write(multiPoint.Geometries.Length);
                Write(multiPoint.Count);
                //Write(multiPoint.InteriorPoint);
                Write(multiPoint.IsEmpty);
                Write(multiPoint.IsSimple);
                Write(multiPoint.IsValid);
                //Write(multiPoint.Length);
                //Write(multiPoint.NumGeometries);
                Write(multiPoint.PointCount);
                
                Write(multiPoint.Buffer(10));
                Write(multiPoint.Buffer(10, BufferStyle.Butt));
                Write(multiPoint.Buffer(10, BufferStyle.Square));
                Write(multiPoint.Buffer(10, 20));
                Write(multiPoint.Buffer(10, 20, BufferStyle.Butt));
                Write(multiPoint.Buffer(10, 20, BufferStyle.Square));
                Write(multiPoint.ConvexHull()); 
               
                byte[] bytes = multiPoint.AsBinary();
                IGeometry test1 = new WkbReader<BufferedCoordinate2D>(GeoFactory).Read(bytes);
                Write(test1.ToString());

                //bytes = new GDBWriter().Write(multiPoint);
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
