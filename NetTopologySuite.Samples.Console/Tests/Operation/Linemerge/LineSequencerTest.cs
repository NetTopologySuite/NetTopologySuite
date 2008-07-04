using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.Operation.Linemerge;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests;
using NUnit.Framework;

namespace GisSharpBlog.NetTopologySuite.Samples.Tests.Operation.Linemerge
{   
    /// <summary>
    /// 
    /// </summary>
    [TestFixture]
    public class LineSequencerTest : BaseSamples    
    {
        private static WKTReader rdr = new WKTReader(GeometryFactory.Fixed);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public LineSequencerTest() : 
            base(GeometryFactory.Fixed) { }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Simple()      
        {
            String[] wkt =  
            {   
                "LINESTRING ( 0 0, 0 10 )" ,
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 10, 0 20 )",     
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SimpleLoop()      
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 0))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SimpleBigLoop()      
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 30, 0 00 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 0))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SimpleVeryBigLoop()
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 40, 0 00 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 0))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SimpleVeryVeryBigLoop()
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 50, 0 00 )",
                "LINESTRING ( 0 20, 0 30 )",
                "LINESTRING ( 0 30, 0 40 )",
                "LINESTRING ( 0 10, 0 20 )",
            };
            String result = "MULTILINESTRING ((0 50, 0 0), (0 0, 0 10), (0 10, 0 20), (0 20, 0 30), (0 30, 0 40), (0 40, 0 30))";
            RunLineSequencer(wkt, result);
        }
        
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TwoSimpleLoops()      
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 0 0 )",
                "LINESTRING ( 0 0, 0 20 )",
                "LINESTRING ( 0 20, 0 0 )",
            };
            String result = "MULTILINESTRING ((0 10, 0 0), (0 0, 0 20), (0 20, 0 0), (0 0, 0 10))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void Wide8WithTail()      
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 10 0, 10 10 )",
                "LINESTRING ( 0 0, 10 0 )",
                "LINESTRING ( 0 10, 10 10 )",
                "LINESTRING ( 0 10, 0 20 )",
                "LINESTRING ( 10 10, 10 20 )",
                "LINESTRING ( 0 20, 10 20 )",
                "LINESTRING ( 10 20, 30 30 )",
            };
            String result = null;
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SimpleLoopWithTail()      
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10 )",
                "LINESTRING ( 10 10, 10 20, 0 10 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10), (10 10, 10 20, 0 10))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void LineWithRing()
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void MultipleGraphsWithRing()
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void MultipleGraphsWithMultipeRings()
        {
            String[] wkt = 
            {
                "LINESTRING ( 0 0, 0 10 )",
                "LINESTRING ( 0 10, 10 10, 10 20, 0 10 )",
                "LINESTRING ( 0 10, 40 40, 40 20, 0 10 )",
                "LINESTRING ( 0 30, 0 20 )",
                "LINESTRING ( 0 20, 0 10 )",
                "LINESTRING ( 0 60, 0 50 )",
                "LINESTRING ( 0 40, 0 50 )",
            };
            String result = "MULTILINESTRING ((0 0, 0 10), (0 10, 40 40, 40 20, 0 10), (0 10, 10 10, 10 20, 0 10), (0 10, 0 20), (0 20, 0 30), (0 40, 0 50), (0 50, 0 60))";
            RunLineSequencer(wkt, result);
        }        

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void LineSequence()      
        {
            String wkt = "LINESTRING ( 0 0, 0 10 )";
            RunIsSequenced(wkt, true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void SplitLineSequence()      
        { 
            String wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 3, 0 4) )";
            RunIsSequenced(wkt, true);
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void BadLineSequence()
        {
            String wkt = "MULTILINESTRING ((0 0, 0 1), (0 2, 0 3), (0 1, 0 4) )";
            RunIsSequenced(wkt, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputWKT"></param>
        /// <param name="expectedWKT"></param>
        private void RunLineSequencer(String[] inputWKT, String expectedWKT)      
        {
            try
            {
                IList<IGeometry> inputGeoms = FromWKT(inputWKT);
                LineSequencer sequencer = new LineSequencer();
                sequencer.Add(inputGeoms);

                if (!sequencer.IsSequenceable())
                    Assert.IsNull(expectedWKT);
                else
                {
                    IGeometry expected = rdr.Read(expectedWKT);
                    IGeometry result = sequencer.GetSequencedLineStrings();
                    bool isTrue = expected.EqualsExact(result);
                    Assert.IsTrue(isTrue);

                    bool isSequenced = LineSequencer.IsSequenced(result);
                    Assert.IsTrue(isSequenced);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); throw; }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputWKT"></param>
        /// <param name="expected"></param>
        private void RunIsSequenced(String inputWKT, bool expected)
        {
            try
            {
                IGeometry g = rdr.Read(inputWKT);
                bool isSequenced = LineSequencer.IsSequenced(g);
                Assert.IsTrue(isSequenced == expected);
            }
            catch(Exception ex) { Debug.WriteLine(ex.ToString()); throw ex; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wkts"></param>
        /// <returns></returns>
        private IList<IGeometry> FromWKT(String[] wkts)
        {
            IList<IGeometry> geomList = new List<IGeometry>();
            foreach (string wkt in wkts)
            {
                try 
                {
                    geomList.Add(rdr.Read(wkt));
                }
                catch (Exception ex) { Debug.WriteLine(ex.ToString()); throw; }
            }
            return geomList;
        }  
    }
}


