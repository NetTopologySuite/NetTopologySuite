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
            IGeometry current = null;
            IGeometry result = null;
            using (ShapefileDataReader reader = new ShapefileDataReader("sa_region", Factory))
            {                
                while (reader.Read())
                {
                    try
                    {
                        current = reader.Geometry;
                        if (result == null)
                             result = current;
                        else result = result.Union(current);
                        Debug.WriteLine("Iteration => " + ++count);
                    }
                    catch (TopologyException ex)
                    {
                        Debug.WriteLine(count + ": " + ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        Debug.WriteLine(String.Empty);
                        Debug.WriteLine("--- BEGIN RESULT ---");
                        Debug.WriteLine(result);
                        Debug.WriteLine("--- END RESULT ---");
                        Debug.WriteLine(String.Empty);
                        Debug.WriteLine("--- BEGIN CURRENT ---");
                        Debug.WriteLine(current);
                        Debug.WriteLine("--- END CURRENT ---");
                        throw ex;
                    }                    
                }
            }
            Debug.WriteLine(result);
        }
    }
}
