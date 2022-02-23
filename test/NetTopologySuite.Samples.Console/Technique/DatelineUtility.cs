using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetTopologySuite.Samples.Technique
{
    public class DatelineUtility
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
            var result = DatelineUtility.UnwrapAtDateline(input);
        }

        /// <summary>
        /// Unwraps a geometry at the dateline
        /// </summary>
        /// <param name="geometry">The geometry to unwrap</param>
        /// <returns>The unwrapped geometry</returns>
        public static Geometry UnwrapAtDateline(Geometry geometry)
        {
            // Test if the geometry has a chance of crossing the dateline
            if (!IsCrossesDateline(geometry))
                return geometry;

            //// Move whole geometry by 360°
            //geometry.Apply(new TranslateXFilter(360d));

            //// Build a list of geometries
            //var parts = new List<Geometry>();

            //// While input geometry is not empty compute intersection with world bounds and add to list
            //// Remove covered part from input geometry, move remains by -360°.
            //while (!geometry.IsEmpty)
            //{
            //    // Intersect geometry with default world bounds
            //    var box = geometry.Factory.ToGeometry(new Envelope(-180, 180, -90, 90));
            //    var geom0 = geometry.Intersection(box);
            //    // Add part
            //    parts.Add(geom0);
            //    // Get remaining geometry and translate into world bounds
            //    geometry = geometry.Difference(box);
            //    geometry.Apply(new TranslateXFilter(-360));
            //}

            //// Union the geometries and return result
            //return NetTopologySuite.Operation.OverlayNG.UnaryUnionNG.Union(parts, geometry.Factory.PrecisionModel);
            return null; //temp
        }

        /// <summary>
        /// Tests if a geometry might possibly cross the dateline.
        /// </summary>
        /// <param name="geometry">The geometry to test</param>
        /// <returns><c>true</c> if the geometry traverses the dateline, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="geometry"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown, if geometry is not geographic (currently just SRID=4326 test)</exception>
        public static bool IsCrossesDateline(Geometry geometry)
        {
            System.Diagnostics.Debug.Print("geomType={0}", geometry.GeometryType);

            // Argument checking
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            // If we don't have a geographic coordinate system 
            if (!IsGeographicCS(geometry))
                throw new ArgumentException("Geography does not have a geographic coordinate system.", nameof(geometry));

            // Can't do anything with empty geometries
            if (geometry.IsEmpty)
                return false;

            // If we have a geometry that is only consisting of points we don't need to do any
            // dateline handling
            if (geometry is IPuntal)
                return false;

            // Build the geometry filter that tests if we need to perform special dateline handling
            var cdf = new IsCrossesDatelineFilter();
            geometry.Apply(cdf);

            // return the result
            System.Diagnostics.Debug.Print("cdf.IsCrossesDateline={0}", cdf.IsCrossesDateline);
            return cdf.IsCrossesDateline;
        }

        /// <summary>
        /// Tests if the geometry has a geographic coordinate system
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns><c>true</c> if it has a geographic coordinate system, <c>false</c> otherwise.</returns>
        private static bool IsGeographicCS(Geometry geometry)
        {
            // TODO: There are more geographic coordinate systems than WGS84. A filter of SRID.csv resulted in ~684
            // geographic coordinate systems with a degree unit and Greenwich as the prime meridian. That is too many
            // to list in this routine so the more popular SRIDs will be added as needed.
            System.Diagnostics.Debug.Print("srid={0}", geometry.SRID);
            return geometry.SRID == 4326;
        }

        /// <summary>
        /// This is a filter that attempts to determine if the geometry crosses the dateline. Essentially, this is the key to the
        /// entire dateline splitting utility.
        /// </summary>
        private class IsCrossesDatelineFilter1 : IGeometryFilter
        /// <remarks>
        /// The IEntireCoordinateSequenceFilter will cycle through all parts of a geometry as long as Done is false. Thus, if any of
        /// the parts of the geometry cross the dateline then Done becomes true and it will jump out of the geometry part loop. this
        /// will ensure we do not get false positives from separate geometries on each side of the dateline.
        /// 
        /// </remarks>
        private class IsCrossesDatelineFilter : IEntireCoordinateSequenceFilter
        {
            public bool Done => IsCrossesDateline;

            public bool GeometryChanged => false;

            public bool IsCrossesDateline { get; private set; } = false;

            public void Filter(CoordinateSequence seq)
            {
                // At this point we assume we are working on individual geometry parts. For each part we cycle through segments in the
                // coordinate sequence and determine if any cross the dateline. there will be several scenarios that we will look for that
                // determine if a segment traverses the dateline. the first is to check if any longitudes < +180° and also > +180°. this
                // will be an indication that the input coordinates are based on longitude values in the range 0° to +360°. thus any
                // longitudes that traverse the dateline will do so if they are on BOTH sides of the +180° meridian. Note that a segment
                // can end exactly at the 180 longitude so we choose to keep a running count how many coordinates are on each side of the
                // hemisphere. if we have coordinates on both sides then we must have crossed the dateline.
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
                        IsCrossesDateline = true;
                        break;
                    }
                }
            }
        }

        ///// <summary>
        ///// A filter class to translate every x-ordinate by a predefined value
        ///// </summary>
        ///// <remarks>Sets <see cref="GeometryChanged"/> to <c>true</c> if the translation value is <c>!= 0d</c></remarks>
        //private class TranslateXFilter : IEntireCoordinateSequenceFilter
        //{
        //    private readonly double _translate;

        //    public TranslateXFilter(double translate)
        //    {
        //        _translate = translate;
        //    }

        //    public bool Done => false;

        //    public bool GeometryChanged => _translate != 0d;

        //    public void Filter(CoordinateSequence seq)
        //    {
        //        // if there is nothing to translate exit now.
        //        if (_translate == 0d) return;

        //        // translate whole by _translate value
        //        for (int i = 0; i < seq.Count; i++)
        //            seq.SetX(i, seq.GetX(i) + _translate);
        //    }
        //}
    }

    //public class DatelineUtilityTest
    //{

    //    [TestCase("SRID=4326;LINESTRING EMPTY", "LINESTRING EMPTY")]
    //    [TestCase("SRID=4326;LINESTRING (-179 0, -181 0)", "MULTILINESTRING ((180 0, 179 0), (-179 0, -180 0))")]
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
    //    public void Test(string wktInput, string wktExpected)
    //    {
    //        var rdr = new WKTReader();
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
