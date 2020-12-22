using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Samples.SimpleTests
{
    public class BaseSamples
    {
        protected GeometryFactory Factory { get; }

        protected WKTReader Reader { get; }

        protected BaseSamples() : this(NtsGeometryServices.Instance) { }

        protected BaseSamples(NtsGeometryServices ntsGeometryServices)
        {
            Factory = ntsGeometryServices.CreateGeometryFactory();
            Reader = new WKTReader(ntsGeometryServices);
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
