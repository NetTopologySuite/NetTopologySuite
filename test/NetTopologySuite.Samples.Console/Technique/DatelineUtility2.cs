using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace NetTopologySuite.Samples.Technique
{

    /// <summary>
    /// Utility class for dateline handling (IDEA)
    /// </summary>
    public class DatelineUtility2
    {
        [STAThread]
        public static void main()
        {
            //test with a multi-part polygon consisting of three parts. the second part crosses the dateline so the routine will
            //cycle through the parts and stop at the second part for the dateline crossing flag.
            string wkt = "SRID=4326;MULTIPOLYGON (" +
                         "((30 20, 45 40, 10 40, 30 20))," +
                         "((170 3.5, 180 3.5, 180 -5, 188.927777777778 -5, 187.061666666667 -9.46444444444444, " +
                         "185.718888888889 -12.4869444444444, 185.281944444444 -13.4555555555556, 184.472777777778 -15.225, 184.3275 -15.5397222222222, " +
                         "184.171944444444 -15.9, 183.188888888889 -18.1488888888889, 183.019444444444 -18.5302777777778, 181.530555555556 -21.8, " +
                         "180 -25, 177.333333333333 -25, 171.416666666667 -25, 168 -28, 163 -30, 163 -24, 163 -17.6666666666667, 161.25 -14, " +
                         "163 -14, 166.875 -11.8, 170 -10, 170 3.5))," +
                         "((15 5, 40 10, 10 20, 5 10, 15 5))" +
                         ")";
            var rdr = new WKTReader();
            var input = rdr.Read(wkt);
            var result = DatelineUtility2.UnwrapAtDateline(input);
        }

        /// <summary>
        /// Maximum value for <see cref="DateLineGap"/>.
        /// </summary>
        public const double MaxDateLineGap = 0.001;

        private static double _dateLineGap;

        /// <summary>
        /// Tests if a geometry might possibly cross the dateline.
        /// </summary>
        /// <param name="geometry">The geometry to test</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="geometry"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown, if geometry is not geographic (see: <see cref="IsGeographicCRS(Geometry)"/>)</exception>
        public static bool IsCrossingDateline(Geometry geometry)
        {
            // Argument checking
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            // If we don't have a geographic coordinate system 
            if (!IsGeographicCRS(geometry))
                throw new ArgumentException("Geography does not have a geographic coordinate system.", nameof(geometry));

            // Can't do anything with empty geometries
            if (geometry.IsEmpty)
                return false;

            // If we have a geometry that is only consisting of points we don't need to do any
            // dateline handling
            if (geometry is IPuntal)
                return false;

            // if the bounds don't cross the dateline, the geometry doesn't either
            // todo: this might give a false positive
            var env = geometry.EnvelopeInternal;
            if (env.MinX > -180d && env.MaxX < 180)
                return false;

            // Build filter that tests if we need to perform special dateline handling
            var cdf = new IsCrossingDatelineFilter();
            geometry.Apply(cdf);

            // return the result
            // todo: this might give a false positive with checking the EnvelopeInternal.MaxX 
            System.Diagnostics.Debug.Print("cdf.IsCrossesDateline={0}", cdf.IsCrossingDateline);
            if (!cdf.IsCrossingDateline && geometry.EnvelopeInternal.MaxX <= 180)
                return false;

            return true;
        }

        /// <summary>
        /// Gets or sets a value for the dateline gap.
        /// </summary>
        /// <remarks>
        /// The allowed range is [0, <see cref="MaxDateLineGap"/>]
        /// </remarks>
        public static double DateLineGap
        {
            get => _dateLineGap;
            set
            {
                if (value < 0 || value > MaxDateLineGap)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Not in allowed range [0 ... {MaxDateLineGap}]");

                _dateLineGap = value;
            }
        }

        /// <summary>
        /// Unwraps a geometry at the dateline (ie splits geometries if they traverse the dateline).
        /// </summary>
        /// <param name="geometry">The geometry to unwrap</param>
        /// <param name="checkIfCrossing"></param>
        /// <param name="unionResult"></param>
        /// <param name="dateLineGap"></param>
        /// The value used to trim the resulting geometry parts so they do not exactly touch the dateline. A positive value will trim only the
        /// parts that are in the Eastern hemisphere and a negative value will trim only the parts that are in the Western hemisphere.This is
        /// useful if the resultant geometries are to be used as multiple parts of a single feature.Double.NaN for no trim gap.If the resulting
        /// geometry will be inserted into a feature class then consider the tolerance for that feature class. "The default value for the x,y tolerance
        /// is 10 times the default x,y resolution, and this is recommended for most cases" and "If coordinates are in latitude-longitude, the default
        /// x,y resolution is 0.000000001 degrees". Thus a default tolerance = 10 * res = 10 * 0.000000001 degrees = 0.00000001 degrees.
        /// <param name="pm"></param>
        /// <returns>The unwrapped geometry</returns>
        /// <exception cref="ArgumentException">Thrown, if geometry is not geographic (see: <see cref="IsGeographicCRS(Geometry)"/>)</exception>
        public static Geometry UnwrapAtDateline(Geometry geometry, bool checkIfCrossing = true, bool unionResult = true, double dateLineGap = double.NaN, PrecisionModel pm = null)
        {
            try
            {
                // If geometry is not based on a geographic coordinate reference system then throw an exception 
                if (!IsGeographicCRS(geometry))
                    throw new ArgumentException("Geography does not have a geographic coordinate system.", nameof(geometry));

                // Does client want us to check if geometry crosses the dateline
                if (checkIfCrossing)
                {
                    // Test if the geometry has a chance of crossing the dateline
                    if (!IsCrossingDateline(geometry))
                        return geometry;
                }

                // Get number of shifts
                // todo: not sure about the logic here? is it to ensure all coordinates have positive x values?
                double numberOfFullShifts = -Math.Floor((geometry.EnvelopeInternal.MinX + 180) / 360);
                if (numberOfFullShifts > 0)
                {
                    // Move whole geometry by numberOfFullShifts * 360°
                    geometry.Apply(new TranslateXFilter(numberOfFullShifts * 360d));
                }

                // setup a list of geometries to hold our results
                var parts = new List<Geometry>();

                // Adjust gap at dateline
                if (double.IsNaN(dateLineGap))
                    dateLineGap = DateLineGap;

                // Test dateline gap value
                if (dateLineGap < 0 || dateLineGap > MaxDateLineGap)
                    throw new ArgumentOutOfRangeException(nameof(dateLineGap), $"Not in allowed range [0 ... {MaxDateLineGap}]");

                // While input geometry is not empty compute intersection with world bounds and add to list
                // Remove covered part from input geometry and unshift the results by -360°.
                while (!geometry.IsEmpty)
                {
                    // Intersect geometry with default world bounds. We assume it is at -180°. Since we have shifted the
                    // geometry we must also shift the dateline by the same amount.
                    var box = geometry.Factory.ToGeometry(new Envelope(-180, +180 - dateLineGap, -100, +100));
                    //if (numberOfFullShifts > 0)
                    //{
                    //    box.Apply(new TranslateXFilter(numberOfFullShifts * 360d));
                    //}
                    var geom0 = geometry.Intersection(box);

                    // Add part to the results list
                    parts.Add(geom0);

                    // Get remaining geometry and unshift
                    geometry = geometry.Difference(box);
                    geometry.Apply(new TranslateXFilter(-360d));
                }

                // check if the precision model is scaled

                // Union the geometries and return result
                if (unionResult)
                {
                    // Check that the precision model is fixed
                    if (pm != null && pm.IsFloating)
                        throw new ArgumentException("Not a fixed precision model.", nameof(pm));

                    return NetTopologySuite.Operation.OverlayNG.UnaryUnionNG.Union(parts, pm ?? geometry.Factory.PrecisionModel);
                }

                // Build a geometry
                return geometry.Factory.BuildGeometry(parts);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.ToString());
                throw ex;
            }
        }

        /// <summary>
        /// Tests if the geometry has a geographic coordinate reference system
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns><c>true</c> if it has a geographic coordinate system, <c>false</c> otherwise.</returns>
        private static bool IsGeographicCRS(Geometry geometry)
         => _geographicCrs.Value.Contains(geometry.SRID);

        private static Lazy<HashSet<int>> _geographicCrs = new Lazy<HashSet<int>>(ReadGeographicCrs());

        /// <summary>
        /// Read a list of geographic coordinate reference systems from a text file.
        /// </summary>
        /// <remarks>
        /// https://github.com/NetTopologySuite/ProjNet4GeoAPI/tree/develop/test/ProjNet.Tests
        /// There are more geographic coordinate systems than WGS84. A filter of SRID.csv from the ProjNet4GeoAPI resulted
        /// in ~684 geographic coordinate systems with a degree unit and Greenwich as the prime meridian. Those entries are
        /// listed in the embedded resource GeographicCrs.txt.
        /// </remarks>
        /// <returns></returns>
        private static HashSet<int> ReadGeographicCrs()
        {
            var hashSet = new HashSet<int>();
            hashSet.Add(4326);

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            using var strm = asm.GetManifestResourceStream($"{typeof(DatelineUtility2).Namespace}.GeographicCrs.txt");
            using var sr = new System.IO.StreamReader(strm);

            //entries are in this format:
            //4326;GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]]
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("#")) continue;

                string[] parts = line.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;
                    if (int.TryParse(part, out int srid))
                        hashSet.Add(srid);
                }
            }
            System.Diagnostics.Debug.Print("srids={0}",hashSet.Count());
            return hashSet;
        }

        /// <summary>
        /// This is a filter that attempts to determine if the geometry crosses the dateline. Essentially, this is the key to the
        /// entire dateline splitting utility.
        /// </summary>
        /// <remarks>
        /// The IEntireCoordinateSequenceFilter will cycle through all parts of a geometry as long as Done is false. Thus, if any of
        /// the parts of the geometry cross the dateline then Done becomes true and it will jump out of the geometry part loop. this
        /// will ensure we do not get false positives from separate geometries on each side of the dateline.
        /// </remarks>
        private class IsCrossingDatelineFilter : IEntireCoordinateSequenceFilter
        {
            public bool Done => IsCrossingDateline;

            public bool GeometryChanged => false;

            public bool IsCrossingDateline { get; private set; }

            public void Filter(CoordinateSequence seq)
            {
                // At this point we assume we are working on individual geometry parts. For each part we cycle through segments in the
                // coordinate sequence and determine if any cross the dateline. there will be several scenarios that we will look for that
                // determine if a segment traverses the dateline.
                // 1. The first is to check if any longitudes < +180° and also > +180°. this will be an indication that the input coordinates
                // are based on longitude values in the range 0° to +360°. thus any longitudes that traverse the dateline will do so if they
                // are on BOTH sides of the +180° meridian. Note that a segment can end exactly at the 180 longitude so we choose to keep a
                // running count how many coordinates are on each side of the hemisphere. if we have coordinates on both sides then we must
                // have crossed the dateline.
                double xCurr;
                int countLT = 0;
                int countGT = 0;
                for (int i = 0; i < seq.Count; i++)
                {
                    //get the longitude of the current coord
                    xCurr = seq.GetX(i);
                    System.Diagnostics.Debug.Print("  i={0} x={1}", i, xCurr);

                    // keep a count of longitudes < 180 and > 180
                    if (xCurr < 180d) countLT++;
                    else if (xCurr > 180d) countGT++;
                    System.Diagnostics.Debug.Print("  countLT={0} countGT={1}", countLT, countGT);

                    // if we have a non-zero count for both sides then we have crossed the dateline
                    if (countLT > 0 && countGT > 0)
                    {
                        IsCrossingDateline = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// A filter class to translate every x-ordinate by a predefined value.
        /// </summary>
        /// <remarks>Sets <see cref="GeometryChanged"/> to <c>true</c> if the translation value is <c>!= 0d</c></remarks>
        private class TranslateXFilter : IEntireCoordinateSequenceFilter
        {
            private readonly double _translate;

            public TranslateXFilter(double translate)
            {
                _translate = translate;
            }

            public bool Done => false;

            public bool GeometryChanged => _translate != 0d;

            public void Filter(CoordinateSequence seq)
            {
                // translate whole geometry by _translate value
                for (int i = 0; i < seq.Count; i++)
                    seq.SetX(i, seq.GetX(i) + _translate);
            }
        }

    }

    //public class DatelineUtilityTest
    //{
    //    NetTopologySuite.NtsGeometryServices _instance = new NetTopologySuite.NtsGeometryServices(new PrecisionModel(1000));

    //    [TestCase("SRID=4326;LINESTRING EMPTY", "LINESTRING EMPTY")]
    //    [TestCase("SRID=4326;LINESTRING (-179 0, -181 0)", "MULTILINESTRING ((180 0, 179 0), (-179 0, -180 0))")]
    //    [TestCase("SRID=4326;LINESTRING (-541 -20, 0 0, 541 20)",
    //        "MULTILINESTRING ((179 -20, 180 -19.963), (-180 -19.963, 180 -6.654), (-180 -6.654, 0 0, 180 6.654), (-180 6.654, 180 19.963), (-180 19.963, -179 20))")]
    //    [TestCase("SRID=4326;LINESTRING (-179 0, -180 0, -180 2, -179 2)", "LINESTRING(-179 0, -180 0, -180 2, -179 2)")]
    //    [TestCase("SRID=4326;LINESTRING (-179 0, 181 0)", "LINESTRING(-180 0, 180 0)")]
    //    [TestCase("SRID=4326;LINESTRING (-185 20, -175 15, -185 10, -175 5, -180 2.5, -180 -2.5, -175 -5, -185 -10, -175 -15, -185 -20)",
    //        "MULTILINESTRING ((175 20, 180 17.5), (180 12.5, 175 10, 180 7.5), (180 2.5, 180 -2.5), (180 -7.5, 175 -10, 180 -12.5), " +
    //                         "(180 -17.5, 175 -20), (-180 17.5, -175 15, -180 12.5), (-180 7.5, -175 5, -180 2.5), (-180 -2.5, -175 -5, -180 -7.5), " +
    //                         "(-180 -12.5, -175 -15, -180 -17.5))")]
    //    [TestCase("SRID=4326;POLYGON ((-179 1, -181 1, -181 -1, -179 -1, -179 1))",
    //        "MULTIPOLYGON (((-179 1, -179 -1, -180 -1, -180 1, -179 1)), ((179 -1, 179 1, 180 1, 180 -1, 179 -1)))")]
    //    [TestCase("SRID=4326;POLYGON ((-185 0, -185 10, -175 10, -175 0, -185 0),   (-182 3, -178 3, -178 7, -182 7, -182 3)))",
    //        "MULTIPOLYGON (((-175 10, -175 0, -180 0, -180 3, -178 3, -178 7, -180 7, -180 10, -175 10)), ((175 10, 180 10, 180 7, 178 7, 178 3, 180 3, 180 0, 175 0, 175 10)))")]
    //    [TestCase("SRID=4326;LINESTRING (-179 0, -181 0)", "MULTILINESTRING ((180 0, 179 0), (-179 0, -180 0))")]
    //    public void Test(string wktInput, string wktExpected)
    //    {
    //        var rdr = new WKTReader(_instance);
    //        var input = rdr.Read(wktInput);
    //        var actual = DatelineUtility.UnwrapAtDateline(input);
    //        var expected = rdr.Read(wktExpected);

    //        if (actual.IsEmpty && expected.IsEmpty)
    //            Assert.That(actual.OgcGeometryType, Is.EqualTo(expected.OgcGeometryType));
    //        else
    //            Assert.That(actual.Equals(expected), Is.True,
    //                $"Expected: {expected}\nActual: {actual}");
    //    }
    //}

}
