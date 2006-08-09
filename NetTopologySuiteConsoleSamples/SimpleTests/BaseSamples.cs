using System;
using System.Collections;
using System.Text;

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
        private GeometryFactory factory = null;

        protected GeometryFactory Factory
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
        protected BaseSamples(GeometryFactory factory) : this(factory, new WKTReader(factory)) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory"></param>
        protected BaseSamples(GeometryFactory factory, WKTReader reader)
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
