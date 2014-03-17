using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class CoordinateBufferTest
    {
        const int NumCoordinates = 70000;
        private const int NumTests = 1000;

        [TestAttribute]
        public void TestAddCoordinates()
        {
            var buf = new CoordinateBuffer();
            buf.AddCoordinate(0, 0);
            Assert.IsTrue(buf.AddCoordinate(0, 0));

            Assert.AreEqual(2, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);
        }

        [TestAttribute]
        public void TestAddCoordinatesDisallowRepeated()
        {
            var buf = new CoordinateBuffer();
            buf.AddCoordinate(0, 0);
            Assert.IsFalse(buf.AddCoordinate(0, 0, allowRepeated: false));

            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);
        }

        [TestAttribute]
        public void TestInsertCoordinates()
        {
            var buf = new CoordinateBuffer();
            Assert.IsTrue(buf.AddCoordinate(0, 0));
            
            Assert.IsTrue(buf.AddCoordinate(10, 10));
            Assert.AreEqual(2, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            Assert.IsTrue(buf.InsertCoordinate(0, -10d, -10d));
            Assert.AreEqual(3, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);
            
            Assert.IsTrue(buf.InsertCoordinate(0, -10d, -10d));
            Assert.AreEqual(4, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);
        }

        [TestAttribute]
        public void TestInsertCoordinatesDisallowRepeated()
        {
            var buf = new CoordinateBuffer();
            Assert.IsTrue(buf.AddCoordinate(0, 0));
            
            Assert.IsTrue(buf.AddCoordinate(10, 10));
            Assert.IsTrue(buf.InsertCoordinate(0, -10d, -10d, allowRepeated: false));
            Assert.AreEqual(3, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            Assert.IsFalse(buf.InsertCoordinate(0, -10d, -10d, allowRepeated: false));
            Assert.AreEqual(3, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            Assert.IsFalse(buf.InsertCoordinate(1, -10d, -10d, allowRepeated: false));
            Assert.AreEqual(3, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);
        }
        

        [TestAttribute]
        public void TestAddCoordinatesOptionalNaN()
        {
            TestAddCoordinatesOptional();
        }

        [TestAttribute]
        public void TestAddCoordinatesOptionalPosInf()
        {
            TestAddCoordinatesOptional(double.PositiveInfinity);
        }

        [TestAttribute]
        public void TestAddCoordinatesOptionalNegInf()
        {
            TestAddCoordinatesOptional(double.NegativeInfinity);
        }

        [TestAttribute]
        public void TestAddCoordinatesOptionalValue()
        {
            TestAddCoordinatesOptional(32000d);
        }

        [TestAttribute]
        public void TestAddMarkers()
        {
            var cb = new CoordinateBuffer(10);
            for (var i = 0; i < 10; i++)
            {
                if (i > 0 && i % 5 == 0)
                    cb.AddMarker();
                cb.AddCoordinate(i, i);
            }
            //cb.AddMarker();

            var seqs = cb.ToSequences();
            Assert.AreEqual(2, seqs.Length);
            Assert.AreEqual(5, seqs[0].Count);
            Assert.AreEqual(5, seqs[1].Count);
        }


        [TestAttribute]
        public void TestToSequenceMethod()
        {
            //TestToSequenceMethod((ICoordinateSequence)null);
            TestToSequenceMethod(PackedCoordinateSequenceFactory.DoubleFactory);
            TestToSequenceMethod(ToPackedDoubleArray);
            System.Diagnostics.Trace.WriteLine(new string('=', 80));
            TestToSequenceMethod(PackedCoordinateSequenceFactory.FloatFactory);
            TestToSequenceMethod(ToPackedFloatArray);
            System.Diagnostics.Trace.WriteLine(new string('=', 80));
            TestToSequenceMethod(DotSpatialAffineCoordinateSequenceFactory.Instance);
            TestToSequenceMethod(ToDotSpatial);
            System.Diagnostics.Trace.WriteLine(new string('=', 80));
            TestToSequenceMethod(CoordinateArraySequenceFactory.Instance);
            TestToSequenceMethod(ToCoordinateArray);
        }


        [TestAttribute]
        public void TestToSequenceMethodUsingFactory()
        {
            //TestToSequenceMethod((ICoordinateSequence)null);
            TestToSequenceMethod(PackedCoordinateSequenceFactory.DoubleFactory);
            TestToSequenceMethod(PackedCoordinateSequenceFactory.FloatFactory);
            TestToSequenceMethod(DotSpatialAffineCoordinateSequenceFactory.Instance);
            TestToSequenceMethod(CoordinateArraySequenceFactory.Instance);
        }

        [TestAttribute]
        public void TestToSequenceMethodUsingConverter()
        {
            TestToSequenceMethod(ToPackedDoubleArray);
            TestToSequenceMethod(ToPackedFloatArray);
            TestToSequenceMethod(ToDotSpatial);
            TestToSequenceMethod(ToCoordinateArray);
        }

        private static ICoordinateSequence ToDotSpatial(CoordinateBuffer buffer)
        {
            double[] z, m;
            var xy = buffer.ToXYZM(out z, out m);
            return new DotSpatialAffineCoordinateSequence(xy, z, m);
        }

        private static ICoordinateSequence ToCoordinateArray(CoordinateBuffer buffer)
        {
            return new CoordinateArraySequence(buffer.ToCoordinateArray());
        }

        private static ICoordinateSequence ToPackedDoubleArray(CoordinateBuffer buffer)
        {
            double[] pa;
            var dim = buffer.ToPackedArray(out pa);
            return new PackedDoubleCoordinateSequence(pa, dim);
        }

        private static ICoordinateSequence ToPackedFloatArray(CoordinateBuffer buffer)
        {
            float[] pa;
            var dim = buffer.ToPackedArray(out pa);
            return new PackedDoubleCoordinateSequence(pa, dim);
        }

        private static void TestToSequenceMethod(ICoordinateSequenceFactory factory)
        {
            var rnd = new Random(8894);
            var buffer = new CoordinateBuffer(NumCoordinates);

            for (var i = 0; i < NumCoordinates; i++)
                buffer.AddCoordinate(rnd.NextDouble(), rnd.NextDouble());

            System.Diagnostics.Trace.WriteLine(
                string.Format("\nConversion using {0} factory", (factory ?? GeoAPI.GeometryServiceProvider.Instance.DefaultCoordinateSequenceFactory).GetType().Name));

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var seqCold = buffer.ToSequence(factory);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(
                string.Format("  Cold converting sequence of {0} coordinates in {1}ms.", NumCoordinates, sw.ElapsedMilliseconds));
            long total = 0;
            foreach (var rndBuffer in (_randomCoordinateBuffers ?? (_randomCoordinateBuffers = RandomCoordinateBuffers(NumTests))))
            {

                sw.Stop();
                sw.Start();
                var seqWarm = rndBuffer.ToSequence(factory);
                sw.Stop();
                Assert.AreEqual(rndBuffer.Count, seqWarm.Count);
                total += sw.ElapsedTicks;
            }
            System.Diagnostics.Trace.WriteLine(
                string.Format("  Warm converting {0} random coordinate buffers in {1}ticks.", NumTests, total));

        }

        private static IEnumerable<CoordinateBuffer> _randomCoordinateBuffers;

        private static void TestToSequenceMethod(CoordinateBufferToSequenceConverterHandler converter)
        {
            var rnd = new Random(8894);
            var buffer = new CoordinateBuffer();

            for (var i = 0; i < NumCoordinates; i++)
                buffer.AddCoordinate(rnd.NextDouble(), rnd.NextDouble());

            System.Diagnostics.Trace.WriteLine(
                string.Format("\nConversion using {0} method", converter.Method.Name));

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var seqCold = buffer.ToSequence(converter);
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(
                string.Format("  Cold converting sequence of {0} coordinates in {1}ms.", NumCoordinates, sw.ElapsedMilliseconds));
            
            long total = 0;
            foreach (var rndBuffer in (_randomCoordinateBuffers ?? (_randomCoordinateBuffers = RandomCoordinateBuffers(NumTests))))
            {
                sw.Stop();
                sw.Start();
                var seqWarm = rndBuffer.ToSequence(converter);
                sw.Stop();
                Assert.AreEqual(rndBuffer.Count, seqWarm.Count);
                total += sw.ElapsedTicks;
            }
            System.Diagnostics.Trace.WriteLine(
                string.Format("  Warm converting {0} random coordinate buffers in {1}ticks.", NumTests, total));

        }

        #region Static helper methods

        private static void CheckDefinedFlags(CoordinateBuffer buffer, Ordinates defined)
        {
            Assert.AreEqual(defined, buffer.DefinedOrdinates, "Defined flags are not set correctly\nExpected: {0}, Actual: {1}", defined, buffer.DefinedOrdinates);
        }

        private static void TestAddCoordinatesOptional(double noDataValue = double.NaN)
        {
            Assert.IsFalse(0d.Equals(noDataValue), "noDataValue must not be 0");
            Assert.IsFalse(1d.Equals(noDataValue), "noDataValue must not be 1");

            var buf = new CoordinateBuffer(noDataValue);

            buf.AddCoordinate(0, 0, m: noDataValue);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            buf.Clear();
            buf.AddCoordinate(0, 0, m: 1);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XYM);

            buf.Clear();
            buf.AddCoordinate(0, 0, noDataValue);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            buf.Clear();
            buf.AddCoordinate(0, 0, 1);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XYZ);

            buf.Clear();
            buf.AddCoordinate(0, 0, noDataValue, noDataValue);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XY);

            buf.Clear();
            buf.AddCoordinate(0, 0, 1, 1);
            Assert.AreEqual(1, buf.Count);
            CheckDefinedFlags(buf, Ordinates.XYZM);
        }

        private static IEnumerable<CoordinateBuffer> RandomCoordinateBuffers(int numBuffers)
        {
            var rnd = new Random(numBuffers);
            var list = new List<CoordinateBuffer>(numBuffers);
            for (var i = 0; i < numBuffers; i++)
            {
                var numCoordinates = rnd.Next(100, 500);
                var buffer = new CoordinateBuffer(numCoordinates);
                for (var j = 0; j < numCoordinates; j++)
                    buffer.AddCoordinate(rnd.NextDouble(), rnd.NextDouble());
                list.Add(buffer);
            }
            return list;
        }
        #endregion
    }
}