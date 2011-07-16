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

namespace NetTopologySuite.SimpleTests.Geometries
{
    public class MultiPointSamples : BaseSamples
    {
        private readonly IMultiPoint _multiPoint;

        public MultiPointSamples()
        {
            ICoordinate[] coordinates = new ICoordinate[]
                                            {
                                                CoordFactory.Create(100, 100),
                                                CoordFactory.Create(200, 200),
                                                CoordFactory.Create(300, 300),
                                                CoordFactory.Create(400, 400),
                                                CoordFactory.Create(500, 500),
                                            };

            _multiPoint = GeoFactory.CreateMultiPoint(coordinates);
        }

        public override void Start()
        {
            try
            {
                //Write(multiPoint.Area);                
                Write(_multiPoint.Boundary);
                Write(_multiPoint.BoundaryDimension);
                Write(_multiPoint.Centroid);
                //Write(multiPoint.Coordinate);
                Write(_multiPoint.Coordinates);
                Write(_multiPoint.Dimension);
                Write(_multiPoint.Envelope);
                Write(_multiPoint.Extents);
                //Write(multiPoint.EnvelopeInternal);
                //Write(multiPoint.Geometries.Length);
                Write(_multiPoint.Count);
                //Write(multiPoint.InteriorPoint);
                Write(_multiPoint.IsEmpty);
                Write(_multiPoint.IsSimple);
                Write(_multiPoint.IsValid);
                //Write(multiPoint.Length);
                //Write(multiPoint.NumGeometries);
                Write(_multiPoint.PointCount);

                Write(_multiPoint.Buffer(10));
                Write(_multiPoint.Buffer(10, BufferStyle.Butt));
                Write(_multiPoint.Buffer(10, BufferStyle.Square));
                Write(_multiPoint.Buffer(10, 20));
                Write(_multiPoint.Buffer(10, 20, BufferStyle.Butt));
                Write(_multiPoint.Buffer(10, 20, BufferStyle.Square));
                Write(_multiPoint.ConvexHull());

                Byte[] bytes = _multiPoint.AsBinary();
                IGeometry test1 = new WkbReader<coord>(GeoFactory).Read(bytes);
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