using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        private IGeometryFactory factory = null;

        protected IGeometryFactory Factory
        {
            get { return factory; }            
        }
        private WKTReader reader = null;

        protected WKTReader Reader
        {
            get { return reader; }            
        }

        /// <summary>
        /// 
        /// </summary>
        protected BaseSamples() : this(new GeometryFactory(), new WKTReader()) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected BaseSamples(IGeometryFactory factory) : this(factory, new WKTReader(factory)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected BaseSamples(IGeometryFactory factory, WKTReader reader)
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
