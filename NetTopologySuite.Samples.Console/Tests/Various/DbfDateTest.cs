using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;

using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Various
{
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class DbfDateTest : BaseSamples
    {                
        /// <summary>
        /// Initializes a new instance of the <see cref="T:NormalizeTest"/> class.
        /// </summary>
        public DbfDateTest() : base() { }
        
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ReadDbfDate()
        {            
            string file = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), @"..\..\..\NetTopologySuite.Samples.Shapefiles\date.dbf");
            if (!File.Exists(file))
                throw new FileNotFoundException("file not found at " + Path.GetDirectoryName(file));
            
            DbaseFileReader reader = new DbaseFileReader(file);
            DbaseFileHeader header = reader.GetHeader();
            IEnumerator ienum = reader.GetEnumerator();
            ienum.MoveNext();            
            ArrayList items = ienum.Current as ArrayList;

            Assert.IsNotNull(items);
            Assert.AreEqual(2, items.Count);

            foreach (Object item in items)
                Assert.IsNotNull(item);

            DateTime date = (DateTime)items[1];

            Assert.AreEqual(10, date.Day);
            Assert.AreEqual(3, date.Month);
            Assert.AreEqual(2006, date.Year);
        }
    }
}
