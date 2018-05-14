﻿using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using GeoAPI.Operation.Buffer;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue36Tests
    {
        private readonly IGeometryFactory factory = GeometryFactory.Default;

        private WKTReader reader;        

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            reader = new WKTReader(factory);
        }

        [Test, Category("Issue36")]
        public void Buffer()
        {
            var geometry = reader.Read(
                @"POLYGON((719068.76798974432 6178827.370335687 31.0995,719070.73569863627 
6178830.5852228012 31.0995,719076.87100000086 6178826.8299 31.0995,719078.2722488807 
6178825.9722172953 31.0995,719076.30480000074 6178822.7577000009 
31.0995,719068.76798974432 6178827.370335687 31.0995))");
            Assert.IsNotNull(geometry);
            Assert.IsTrue(geometry.IsValid);

            var buffered = geometry.Buffer(0.01);
            Assert.IsNotNull(buffered);
            Assert.IsTrue(buffered.IsValid);
            Assert.IsFalse(buffered.EqualsExact(geometry));
        }

        [Test, Category("Issue36")]
        public void Buffer2()
        {
            var geometry = reader.Read(
                @"LINESTRING(1250.7665 446.9385,1137.8786 170.4488,1136.3666106287267 
166.74557327980631,1139.485009866369 125.36515638486206,1137.8786 121.7019)");
            Assert.IsNotNull(geometry);
            Assert.IsTrue(geometry.IsValid);

            var buffered = geometry.Buffer(5);
            Assert.IsNotNull(buffered);
            Assert.IsTrue(buffered.IsValid);

            var expected =
                reader.Read(
                    @"POLYGON ((1142.5076334002422 168.55881328590834, 1141.4410130256356 165.94640267866984, 
1144.4708724926961 125.74088750067949, 1144.413756618301 124.52405267228002, 1144.0640803474464 123.35714405190089, 
1142.4576704810775 119.69388766703882, 1141.9779411531201 118.83913872627556, 1141.3406764435852 118.09440409580651, 
1140.5703660778781 117.4883035098286, 1139.696612651358 117.04412907395584, 1138.7529940181994 116.77895016266294, 
1137.7757729132873 116.70295745279682, 1136.8025033956515 116.81907130158044, 1135.8705876670388 117.12282951892244, 
1135.0158387262757 117.60255884687994, 1134.2711040958065 118.23982355641485, 1133.6650035098287 119.01013392212188, 
1133.2208290739559 119.88388734864198, 1132.955650162663 120.82750598180051, 1132.879657452797 121.80472708671265, 
1132.9957713015804 122.7779966043485, 1133.2995295189226 123.70991233296117, 1134.4055086776207 126.23198674214223, 
1131.3807480023995 166.36984216398886, 1131.427488670157 167.52343848559283, 1131.7375772284845 168.63555999389797, 
1133.2495665997578 172.33878671409167, 1246.1374665997578 448.82848671409164, 1246.595130295206 449.6952507660154, 
1247.2130973890783 450.4560744320048, 1247.967619752517 451.0817196852508, 1248.8297015142364 451.5481433295369, 
1249.7662133555946 451.8374209657867, 1250.7411656517302 451.9384358166678, 1251.7170915326622 451.8473059381107, 
1252.6564867140917 451.5675334002421, 1253.5232507660153 451.109869704794, 1254.284074432005 450.4919026109218, 
1254.9097196852508 449.737380247483, 1255.376143329537 448.8752984857635, 1255.6654209657866 447.93878664440524, 
1255.7664358166678 446.9638343482697, 1255.6753059381106 445.98790846733783, 1255.3955334002421 445.0485132859083, 
1142.5076334002422 168.55881328590834))"
                /*
                    @"POLYGON ((1133.2495665997578 172.33878671409167, 1246.1374665997578 
448.82848671409164, 1246.595130295206 449.6952507660154, 1247.2130973890783 450.4560744320048, 
1247.967619752517 451.0817196852508, 1248.8297015142364 451.5481433295369, 1249.7662133555946 451.8374209657867, 
1250.7411656517302 451.9384358166678, 1251.7170915326622 451.8473059381107, 1252.6564867140917 451.5675334002421, 
1253.5232507660153 451.109869704794, 1254.284074432005 450.4919026109218, 1254.9097196852508 449.737380247483, 
1255.376143329537 448.8752984857635, 1255.6654209657866 447.93878664440524, 1255.7664358166678 446.9638343482697, 
1255.6753059381106 445.98790846733783, 1255.3955334002421 445.0485132859083, 1142.5076334002422 168.55881328590834, 
1141.4410130256356 165.94640267866984, 1144.4708724926961 125.74088750067949, 1144.413756618301 124.52405267228002, 
1144.0640803474464 123.35714405190089, 1142.4576704810775 119.69388766703882, 1141.9779411531201 118.83913872627556, 
1141.3406764435852 118.09440409580651, 1140.5703660778781 117.4883035098286, 1139.696612651358 117.04412907395584, 
1138.7529940181994 116.77895016266294, 1137.7757729132873 116.70295745279682, 1136.8025033956515 116.81907130158044, 
1135.8705876670388 117.12282951892244, 1135.0158387262757 117.60255884687994, 1134.2711040958065 118.23982355641485, 
1133.6650035098287 119.01013392212188, 1133.2208290739559 119.88388734864198, 1132.955650162663 120.82750598180051, 
1132.879657452797 121.80472708671265, 1132.9957713015804 122.7779966043485, 1133.2995295189226 123.70991233296117, 
1134.4055086776207 126.23198674214223, 1131.3807480023995 166.36984216398886, 1131.427488670157 167.52343848559283, 
1131.7375772284845 168.63555999389797, 1133.2495665997578 172.33878671409167))"*/);

            var result = buffered.EqualsExact(expected);
            Assert.IsTrue(result);
        }

        [Test, Category("Issue36")]
        public void Buffer3()
        {
            var geometry = reader.Read(
                @"LINESTRING(1250.7665 446.9385,1137.8786 170.4488,1136.3666106287267 
166.74557327980631,1139.485009866369 125.36515638486206,1137.8786 121.7019)");
            Assert.IsNotNull(geometry);
            Assert.IsTrue(geometry.IsValid);

            BufferParameters parameters = new BufferParameters() {EndCapStyle = EndCapStyle.Round};

            var curveBuilder = new OffsetCurveBuilder(
                geometry.PrecisionModel, parameters);
            var curveSetBuilder = new OffsetCurveSetBuilder(geometry, 5, curveBuilder);

            var bufferSegStrList = curveSetBuilder.GetCurves();
            Assert.AreEqual(1, bufferSegStrList.Count);
            
            var segmentString = (NodedSegmentString) bufferSegStrList[0];
            Assert.AreEqual(45, segmentString.Count);

            for (var i = 0; i < segmentString.Coordinates.Length; i++)
            {
                var coord = segmentString.Coordinates[i];
                Debug.WriteLine(String.Format("{1:R} {2:R}", i, coord.X, coord.Y));
            }
        }

        [Test, Category("Issue36")]
        [Ignore("Reevaluate expected geometry")]
        public void TestIsValid()
        {
            var geom1 = reader.Read(
                    @"POLYGON((719068.76798974432 6178827.370335687 31.0995,719070.73569863627 6178830.5852228012 31.0995,719076.87100000086 6178826.8299 31.0995,719078.2722488807 6178825.9722172953 31.0995,719076.30480000074 6178822.7577000009 31.0995,719068.76798974432 6178827.370335687 31.0995))");
            Assert.IsNotNull(geom1);
            Assert.IsTrue(geom1.IsValid);

            var geom2 = reader.Read(
                    @"POINT(719080.36969999934 6178824.6883999994)");
            Assert.IsNotNull(geom2);
            Assert.IsTrue(geom2.IsValid);

            var expected = reader.Read(
                    @"POLYGON ((719068.7579976716 6178827.369937588, 719068.758112008 6178827.371894637, 719068.7586059568 6178827.373791773, 719068.759460535 6178827.375556088, 719070.727169427 6178830.590443202, 719070.7283517772 6178830.592006875, 719070.7298164692 6178830.593309835, 719070.7315072144 6178830.594302007, 719070.733359037 6178830.594945263, 719070.7353007706 6178830.595214883, 719070.7372577946 6178830.595100504, 719070.7391548998 6178830.594606523, 719070.74091918 6178830.593751923, 719070.7409191797 6178830.593751923, 719076.8762205446 6178826.838429122, 719078.2774694242 6178825.980746417, 719078.2774694244 6178825.980746417, 719078.2790330398 6178825.979564076, 719078.2803359522 6178825.97809941, 719078.2813280922 6178825.976408705, 719078.2819713342 6178825.974556932, 719078.2822409592 6178825.97261525, 719078.2821266062 6178825.970658276, 719078.2816326694 6178825.968761212, 719078.2807781298 6178825.966996959, 719076.31332925 6178822.752479665, 719076.3121469484 6178822.750916023, 719076.3106823174 6178822.749613076, 719076.30899164 6178822.748620895, 719076.3071398856 6178822.747977607, 719076.3051982138 6178822.747707932, 719076.3032412394 6178822.747822234, 719076.3013441653 6178822.748316121, 719076.2995798928 6178822.749170613, 719068.7627696363 6178827.361806299, 719068.7627696362 6178827.361806299, 719068.7612059064 6178827.36298861, 719068.759902887 6178827.364453278, 719068.7589106546 6178827.366144013, 719068.7582673416 6178827.36799584, 719068.7579976716 6178827.369937588))");
            Assert.IsNotNull(expected);
            Assert.IsTrue(expected.IsValid);

            var actual = geom1.Buffer(0.01);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.IsValid);
            actual.Normalize();

            if (expected.EqualsExact(actual))
                Assert.IsTrue(true);
            else
            {
                Console.WriteLine(expected.AsText());
                Console.WriteLine(actual.AsText());
                Assert.IsFalse(true);
            }
        }

        [Test, Category("Issue36")]
        public void Buffer4()
        {
            var geom1 = reader.Read( @"LINESTRING (154083.15854208171 181964.499776382, 154083.11547008157 181963.75712038204, 154083.00103808194 181962.7356803827, 154082.70279807597 181961.45196838304, 154082.28526207805 181960.20192038268, 154081.7520140782 181958.99667238072, 154081.10779007524 181957.84684837982, 154080.35822207481 181956.76281637698, 154079.5100940764 181955.7539203763, 154079.37601407617 181955.62188837677, 154078.57095807791 181954.82924837619, 154077.54887807369 181953.99712037668, 154076.45319807529 181953.26457637548, 154075.45979007334 181952.72832037509, 154075.29351807386 181952.63852837682, 154074.23367807269 181952.18208037689, 154073.13979007304 181951.81446437538, 154072.40206207335 181951.63251237571, 154072.0194060728 181951.53811237589, 154070.88007806987 181951.35494437441, 154069.72948607057 181951.266112376, 154068.57556606829 181951.27225637436, 154067.42599806935 181951.3733763732, 154066.28871806711 181951.56870437414, 154064.96468606591 181951.92019237578, 154063.68123006821 181952.39929637685, 154062.45076606423 181953.00153637677, 154061.28519806266 181953.72128037736, 154060.19540606439 181954.55136037618, 154059.19201406092 181955.48409637809, 154058.28462206572 181956.51040037721, 154057.48180606216 181957.62041638047, 154056.83092606068 181958.73350438103, 154056.28513406217 181959.90163237974, 154055.84891006351 181961.11494437978, 154055.52609405667 181962.3632003814, 154055.31924606115 181963.63577638194, 154055.30945406109 181963.777664382, 154055.23028606176 181964.92204838246, 154055.25985406339 181966.2111363858, 154055.3141900599 181966.68147238717, 154055.40782205761 181967.49190438539, 154055.67278206348 181968.75372838602, 154056.05255806446 181969.98585638776, 154056.50350206345 181971.11660838872, 154057.05287805945 181972.20307238773, 154057.69620606303 181973.23654438928, 154058.428494066 181974.20896039158, 154059.24398206174 181975.11276838929, 154060.13639806211 181975.94080039114, 154061.09857406467 181976.68652839214, 154062.12295806408 181977.34419239312, 154063.18113406748 181977.87116839364, 154064.28014206886 181978.30662439391, 154065.41204606742 181978.64748839289, 154066.56878206879 181978.89120039344, 154067.74203006923 181979.03596839309, 154068.92334207147 181979.08089639247, 154070.10414206982 181979.02560039237, 154071.27598207444 181978.87059239298, 154072.55879807472 181978.61894439533, 154073.8126860708 181978.2497923933, 154075.02715007216 181977.76620839164, 154076.19169407338 181977.17241639271, 154077.29633407295 181976.47340839356, 154078.33147007972 181975.67520039156, 154079.28839807957 181974.78470439091, 154080.15879807621 181973.80947238952, 154080.93537408114 181972.75795238838, 154081.61121407896 181971.63897638768, 154082.0496140793 181970.72480038926, 154082.41953407973 181969.78080038726, 154082.71892607957 181968.81209638715, 154082.94612608105 181967.8239363879, 154083.09985408187 181966.82182438672, 154083.17947007716 181965.81100838631, 154083.1749900803 181964.78316838294, 154083.15854208171 181964.499776382)");
            
            var buffered = geom1.Buffer(1.5) as Geometry; 
            Assert.IsNotNull(buffered);
            Assert.IsTrue(buffered.IsValid);
        }
    }
}
