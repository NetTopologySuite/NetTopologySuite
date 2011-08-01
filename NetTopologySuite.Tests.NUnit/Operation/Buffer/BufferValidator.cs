using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Buffer
{
    public class BufferValidator
    {
        private IGeometry original;
        private double bufferDistance;
        private Dictionary<string, Test> nameToTestMap = new Dictionary<string, Test>();
        private IGeometry buffer;
        private static int QUADRANT_SEGMENTS_1 = 100;
        private static int QUADRANT_SEGMENTS_2 = 50;
        private String wkt;
        private GeometryFactory geomFact = new GeometryFactory();
        private WKTWriter wktWriter = new WKTWriter();
        private WKTReader wktReader;

        public BufferValidator()
        {
            IGeometry g =
                new WKTReader().Read(
                "MULTILINESTRING (( 635074.5418406526 6184832.4888257105, 635074.5681951842 6184832.571842485, 635074.6472587794 6184832.575795664 ), ( 635074.6657069515 6184832.53889932, 635074.6933792098 6184832.451929366, 635074.5642420045 6184832.474330718 ))");
            Console.WriteLine(g);
            Console.WriteLine(g.Buffer(0.01, 100));
            Console.WriteLine("END");
        }

        public BufferValidator(double bufferDistance, String wkt)
            : this()
        {
            // SRID = 888 is to test that SRID is preserved in computed buffers
            SetFactory(new PrecisionModel(), 888);
            this.bufferDistance = bufferDistance;
            this.wkt = wkt;
            //    addBufferResultValidatorTest();
        }

        private String Supplement(String message)
        {
            String newMessage = "\n" + message + "\n";
            newMessage += "Original: " + wktWriter.WriteFormatted(GetOriginal()) + "\n";
            newMessage += "Buffer Distance: " + bufferDistance + "\n";
            newMessage += "Buffer: " + wktWriter.WriteFormatted(GetBuffer()) + "\n";
            return newMessage.Substring(0, newMessage.Length - 1);
        }

        public BufferValidator TestExpectedArea(double expectedArea)
        {
            try
            {
                double tolerance =
                    Math.Abs(
                        GetBuffer().Area
                        - GetOriginal()
                        .Buffer(
                            bufferDistance,
                            QUADRANT_SEGMENTS_1 - QUADRANT_SEGMENTS_2)
                        .Area);

                Assert.AreEqual(expectedArea, GetBuffer().Area, tolerance, "Area Test");
            }
            catch (Exception e)
            {
                throw new Exception(
                Supplement(e.ToString()) + e.StackTrace);
            }

            return this;
        }

        public BufferValidator TestEmptyBufferExpected(bool emptyBufferExpected)
        {
            Assert.IsTrue(
                emptyBufferExpected == GetBuffer().IsEmpty,
                Supplement(
                    "Expected buffer "
                    + (emptyBufferExpected ? "" : "not ")
                    + "to be empty")
                );

             return this;
        }

        public BufferValidator TestBufferHolesExpected(bool bufferHolesExpected)
        {
            Assert.IsTrue(
                HasHoles(GetBuffer()) == bufferHolesExpected,
                Supplement(
                        "Expected buffer "
                            + (bufferHolesExpected ? "" : "not ")
                            + "to have holes")
                );

            return this;
        }

        private bool HasHoles(IGeometry buffer)
        {
            if (buffer.IsEmpty) {
                return false;
            }
            if (buffer is Polygon) {
                return ((Polygon) buffer).NumInteriorRings > 0;
            }
            MultiPolygon multiPolygon = (MultiPolygon) buffer;
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                if (HasHoles(multiPolygon.GetGeometryN(i)))
                {
                    return true;
                }
            }
            return false;
        }

        private IGeometry GetOriginal()
        {
            if (original == null) {
                original = wktReader.Read(wkt);
            }
            return original;
        }


        public BufferValidator SetPrecisionModel(PrecisionModel precisionModel)
        {
            wktReader = new WKTReader(new GeometryFactory(precisionModel));
            return this;
        }

        public BufferValidator SetFactory(PrecisionModel precisionModel, int srid)
        {
            wktReader = new WKTReader(new GeometryFactory(precisionModel, srid));
            return this;
        }

        private IGeometry GetBuffer()
        {
            if (buffer == null) {
                buffer = GetOriginal().Buffer(bufferDistance, QUADRANT_SEGMENTS_1);
                if (GetBuffer() is GeometryCollection && GetBuffer().IsEmpty)
                {
                    try
                    {
                        //#contains doesn't work with GeometryCollections [Jon Aquino
                        // 10/29/2003]
                        buffer = wktReader.Read("POINT EMPTY");
                    }
                    catch (ParseException e)
                    {
                        NetTopologySuite.Utilities.Assert.ShouldNeverReachHere();
                    }
                }
            }
            return buffer;
        }

        public void TestContains()
        {
            if (GetOriginal() is GeometryCollection) {
                return;
            }
            Assert.IsTrue(GetOriginal().IsValid);
            if (bufferDistance > 0)
            {
                Assert.IsTrue(
                    Contains(GetBuffer(), GetOriginal()),
                    Supplement("Expected buffer to contain original"));
            }
            else
            {
                Assert.IsTrue(
                    Contains(GetOriginal(), GetBuffer()),
                    Supplement("Expected original to contain buffer"));
            }

        }

        private bool Contains(IGeometry a, IGeometry b)
        {
            //JTS doesn't currently handle empty geometries correctly [Jon Aquino
            // 10/29/2003]
            if (b.IsEmpty) {
                return true;
            }
            bool isContained = a.Contains(b);
            return isContained;
        }

        //private void AddBufferResultValidatorTest()
        //{
        //    if (GetOriginal() is GeometryCollection) {
        //        return;
        //    }
        //        Assert.Assert.IsTrue(
        //            BufferResultValidator.IsValid(GetOriginal(), bufferDistance, GetBuffer()),
        //            Supplement("BufferResultValidator failure"));
        //    }
        //}
    }
}
