using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using NUnit.Framework;

using GeoAPI.Geometries;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;


namespace  GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class UnionAggregateTest : BaseSamples
    {
        /// <summary>
        /// 
        /// </summary>
        public UnionAggregateTest() : base() 
        {
            // Set current dir to shapefiles dir
            Environment.CurrentDirectory = @"../../../NetTopologySuite.Samples.Shapefiles";
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void PerformUnionAggregateTest()
        {
            int count = 0;
            IGeometry result = null;
            using (ShapefileDataReader reader = new ShapefileDataReader("sa_region", Factory))
            {                
                while (reader.Read())
                {
                    try
                    {
                        if (result == null)
                            result = reader.Geometry;
                        else result = result.Union(reader.Geometry);
                        Debug.WriteLine("Iteration => " + ++count);
                    }
                    catch (TopologyException ex)
                    {
                        Debug.WriteLine(count + ": " + ex.ToString());
                        Debug.WriteLine(ex.StackTrace);
                        throw ex;
                    }                    
                }
            }
            Debug.WriteLine(result);
        }
    }
}
