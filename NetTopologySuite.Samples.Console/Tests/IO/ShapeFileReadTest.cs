using System;
using System.Diagnostics;
using System.IO;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Operation.IO
{

    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class ShapeFileDataReaderTest : BaseSamples    
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShapeFileDataReaderTest"/> class.
        /// </summary>
        public ShapeFileDataReaderTest() : base() 
        {
            // Set current dir to shapefiles dir
			Environment.CurrentDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../NetTopologySuite.Samples.Shapefiles");
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestReadingCrustalTestShapeFile()
        {
            // Original file with characters '°' in NAME field.
            using (ShapefileDataReader reader = new ShapefileDataReader("crustal_test_bugged", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {                    
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }

            // Removed NAME field characters
            using (ShapefileDataReader reader = new ShapefileDataReader("crustal_test", Factory))
            {
                int length = reader.DbaseHeader.NumFields;
                while (reader.Read())
                {
                    Debug.WriteLine(reader.GetValue(length - 1));
                }
            }      
        }        
    }
}
