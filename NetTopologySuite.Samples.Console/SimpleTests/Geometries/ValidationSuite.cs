using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidationSuite : BaseSamples
    {
        private GeometryFactory factory = null;
        private WKTReader reader = null;                    
        
        /// <summary>
        /// 
        /// </summary>
        public ValidationSuite() : base()
        {                       
            factory = new GeometryFactory(new PrecisionModel(PrecisionModels.Fixed));                       
            reader = new WKTReader(factory);
        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            IGeometry a = reader.Read("POLYGON ((340 320, 340 200, 200 280, 200 80, 340 200, 340 20, 60 20, 60 340, 340 320))");            
            bool result = a.IsValid;
            
            if(result)
            	  Console.WriteLine("Error!");
             else Console.WriteLine("Work completed!");       
        }
    }
}
