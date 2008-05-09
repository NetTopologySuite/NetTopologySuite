using System;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{
    public class BaseSamples
    {
        private static BufferedCoordinate2DFactory _coordFactory = new BufferedCoordinate2DFactory();
          
        protected static ICoordinateFactory<BufferedCoordinate2D> CoordFactory
        {
            get { return _coordFactory; }
        }

        private IGeometryFactory<BufferedCoordinate2D> factory = null;

        protected IGeometryFactory<BufferedCoordinate2D> GeoFactory
        {
            get { return factory; }            
        }

        private IWktGeometryReader<BufferedCoordinate2D> reader = null;

        protected IWktGeometryReader<BufferedCoordinate2D> Reader
        {
            get { return reader; }            
        }

        /// <summary>
        /// 
        /// </summary>
        protected BaseSamples() 
            : this(new GeometryFactory<BufferedCoordinate2D>(
                new BufferedCoordinate2DSequenceFactory(_coordFactory))) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected BaseSamples(IGeometryFactory<BufferedCoordinate2D> factory)
            : this(factory, new WktReader<BufferedCoordinate2D>(factory, null)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected BaseSamples(IGeometryFactory<BufferedCoordinate2D> factory,
                              IWktGeometryReader<BufferedCoordinate2D> reader)
        {
            this.factory = factory;
            this.reader = reader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        protected void Write(object o)
        {
            Console.WriteLine(o.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        protected void Write(string s)
        {
            Console.WriteLine(s);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Start() { }
    }
}
