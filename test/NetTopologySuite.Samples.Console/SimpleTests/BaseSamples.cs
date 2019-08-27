using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.SimpleTests
{
    public class BaseSamples
    {
        protected GeometryFactory Factory { get; private set; }

        protected WKTReader Reader { get; private set; }

        protected BaseSamples() : this(new GeometryFactory(), new WKTReader()) { }

        protected BaseSamples(GeometryFactory factory) : this(factory, new WKTReader(factory)) { }

        protected BaseSamples(GeometryFactory factory, WKTReader reader)
        {
            Factory = factory;
            Reader = reader;
        }

        protected void Write(object o)
        {
            Console.WriteLine(o.ToString());
        }

        protected void Write(string s)
        {
            Console.WriteLine(s);
        }

        public virtual void Start() { }
    }
}
