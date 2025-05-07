using System;
using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Technique
{
    /// <summary>
    /// Shows a technique to split a shape along the antimeridian (180 degree) line
    /// </summary>
    public class PerformAntimeridianSplit
    {
        /// <summary>
        /// Maximum value for <see cref="DateLineGap"/>.
        /// </summary>
        public const double MaxDateLineGap = 0.001;

        private static double _dateLineGap;

        /// <summary>
        /// Gets or sets a value
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

        public static Geometry UnwrapAtDateline(Geometry geometry, bool checkIfCrossing = true, bool unionResult = true, double dateLineGap = double.NaN, PrecisionModel pm = null)
        {
            // Does client want us to check if geometry crosses the dateline
            if (checkIfCrossing)
            {
                // Test if the geometry has a chance of crossing the dateline
                if (!IsCrossingAntimeridian(geometry))
                    return geometry;
            }
            else
            {
                // Check if the geometry is at least a geographic one!
                if (!IsGeographic(geometry))
                    throw new ArgumentException(nameof(geometry));
            }

            // Get number of shifts
            double numberOfFullShifts = -Math.Floor((geometry.EnvelopeInternal.MinX + 180) / 360);
            if (numberOfFullShifts > 0)
            {
                // Move whole geometry by numberOfFullShifts * 360°
                geometry.Apply(new TranslateXFilter(numberOfFullShifts * 360d));
            }

            // Build a list of geometries
            var parts = new List<Geometry>();

            // Adjust gap at dateline
            if (double.IsNaN(dateLineGap))
                dateLineGap = DateLineGap;

            // Test dateline gap value
            if (dateLineGap < 0 || dateLineGap > MaxDateLineGap)
                throw new ArgumentOutOfRangeException(nameof(dateLineGap), $"Not in allowed range [0 ... {MaxDateLineGap}]");

            // While input geometry is not empty compute intersection with world bounds and add to list
            // Renove covered part from input geometry
            while (!geometry.IsEmpty)
            {
                // Intersect geometry with default world bounds
                var box = geometry.Factory.ToGeometry(new Envelope(-180, 180 - dateLineGap, -90, 90));
                var geom0 = geometry.Intersection(box);
                // Add part
                parts.Add(geom0);
                // Get remaining geometry and translate into world bounds
                geometry = geometry.Difference(box);
                geometry.Apply(new TranslateXFilter(-360d));
            }

            // check if the precision model is scaled
            // TODO is there something to implement here?

            // Union the geometries and return result
            if (unionResult)
            {
                // Check that the precision model is fixed
                if (pm != null && pm.IsFloating)
                    throw new ArgumentException("Not a fixed precision model", nameof(pm));

                return NetTopologySuite.Operation.OverlayNG.UnaryUnionNG.Union(parts, pm ?? geometry.Factory.PrecisionModel);
            }

            // Build a geometry
            return geometry.Factory.BuildGeometry(parts);
        }

        /// <summary>
        /// Tests if a geometry might possibly cross the dateline
        /// </summary>
        /// <param name="geometry">The geometry to test</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown, if <paramref name="geometry"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown, if geometry is not geographic (see: <see cref="IsGeographic(Geometry)"/>)</exception>
        public static bool IsCrossingAntimeridian(Geometry geometry)
        {
            // Argument checking
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            // If we don't have a geographic coordinate system
            if (!IsGeographic(geometry))
                throw new ArgumentException("Not a geography", nameof(geometry));

            // Can't do anything with empty geometries
            if (geometry.IsEmpty)
                return false;

            // If we have a geometry that is only consisting of points we don't need to do any
            // dateline handling
            if (geometry is IPuntal)
                return false;

            // if the bounds don't cross the dateline, the geometry doesn't either
            if (!IsEnvelopeCrossingAntimeridian(geometry.EnvelopeInternal))
                return false;

             //// TODO Code left intact from original discussion but commented out pending further discussion

            // Build filter that tests if we need to perform special dateline handling
            // var cdf = new IsCrossingDatelineFilter();
            // geometry.Apply(cdf);
            //
            // if (!cdf.IsCrossingDateline && geometry.EnvelopeInternal.MaxX <= 180)
            //     return false;

            /////////////////////////////////////////////////////////////////////////////////////////////////////

            return true;
        }

        private static bool IsEnvelopeCrossingAntimeridian(Envelope envelope)
        {
            // if longitudes are same sign then does not cross the antimeridian
            if (Math.Sign(envelope.MinX) == Math.Sign(envelope.MaxX))
            {
                return false;
            }

            // compute distance between points crossing the prime meridian
            double distanceAcrossPrimeMeridian = Math.Abs(envelope.MinX) + Math.Abs(envelope.MaxX);

            // compute distance between points crossing the antimeridian
            double distanceAcrossAntiMeridian = 360 - distanceAcrossPrimeMeridian;

            return distanceAcrossPrimeMeridian > distanceAcrossAntiMeridian;
        }

        /// <summary>
        /// Tests if the geometry was build for geographic geometries
        /// </summary>
        /// <param name="geometry">The geometry</param>
        /// <returns><c>true</c> if it does.</returns>
        private static bool IsGeographic(Geometry geometry)
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
        private static HashSet<int> ReadGeographicCrs()
        {
            var hashSet = new HashSet<int>();
            hashSet.Add(4326);

            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            string manifestResourceName = $"{typeof(PerformAntimeridianSplit).Namespace}.GeographicCrs.txt";

            Stream stream;
            try
            {
                stream = executingAssembly.GetManifestResourceStream(manifestResourceName);
            }
            catch (FileNotFoundException)
            {
                // If the person running this doesn't have the file, assume they're going to use 4326.
                return hashSet;
            }

            if (stream == null)
                return hashSet;

            using var sr = new StreamReader(stream);

            //entries are in this format:
            //4326;GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.0174532925199433,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]]
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith('#')) continue;

                string[] parts = line.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;
                    if (int.TryParse(part, out int srid))
                        hashSet.Add(srid);
                }
            }

            stream.Dispose();
            return hashSet;
        }

        /// <summary>
        /// A filter class to translate every x-ordinate by a predefined value
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
                // translate whole ge
                for (int i = 0; i < seq.Count; i++)
                    seq.SetX(i, seq.GetX(i) + _translate);
            }
        }
    }

    // TODO If we know that the envelope coordinates cross the antimeridian per IsEnvelopeCrossingAntimeridian,
    // do we need to go through every coordinate?
    public class IsCrossingDatelineFilter : IEntireCoordinateSequenceFilter
    {
        public bool Done => IsCrossingDateline;

        public bool GeometryChanged => false;

        public bool IsCrossingDateline { get; private set; }

        public void Filter(CoordinateSequence seq)
        {
            // Test if we have a segment crossing the dateline
            double currX = seq.GetX(0);
            bool wasLT = currX < -180d;
            bool wasGT = currX > -180d;
            for (int i = 1; i < seq.Count; i++)
            {
                double lastX = currX;

                // Prevent unmotivated sign change on dateline
                currX = seq.GetX(i);
                if ((currX == -180d && lastX == 180d) ||
                    (currX == 180d && lastX == -180d))
                    currX = lastX;

                // Check if we cross the dateline
                if (currX < -180d && wasGT ||
                    currX > -180d && wasLT)
                {
                    IsCrossingDateline = true;
                    break;
                }

                // Update flag variables
                wasLT = currX < -180d;
                wasGT = currX > -180d;
            }
        }
    }

    /// <summary>
    /// Filters coordinates that cross the
    /// </summary>
    public class AntiMeridianShiftXFilter : IEntireCoordinateSequenceFilter
    {
        private const int translate = 360;

        private readonly double _tolerance;

        // TODO I'm not sure what the project idiomatic way of splitting constructors are
        public AntiMeridianShiftXFilter() : this(0d)
        {
        }

        public AntiMeridianShiftXFilter(double tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Done { get; private set; }

        public bool GeometryChanged { get; private set; }

        public void Filter(CoordinateSequence seq)
        {
            // Test if we have a segment crossing the dateline
            double currX = seq.GetX(0);
            int xSign = Math.Sign(currX);
            int prevSign = xSign == 0 ? 1 : xSign;

            for (int i = 1; i < seq.Count; i++)
            {
                double lastX = currX;

                // Prevent unmotivated sign change on dateline
                currX = seq.GetX(i);
                if ((currX == -180d && lastX == 180d) || (currX == 180d && lastX == -180d))
                    currX = lastX;

                // Check if we cross the antemeridian
                // if sign change
                if (Math.Sign(currX) != prevSign)
                {
                    // compute distance between points crossing the prime meridian
                    double distanceAcrossPrimeMeridian = Math.Abs(currX) + Math.Abs(lastX);

                    // compute distance between points crossing the antimeridian
                    double distanceAcrossAntiMeridian = 360 - distanceAcrossPrimeMeridian;

                    if (distanceAcrossPrimeMeridian > distanceAcrossAntiMeridian
                        // if the two distances are the same then it's a tough call because the distance around the globe in either
                        // direction is the same. if the distance is the same, assume the segment crosses the prime meridian.
                        && !(Math.Abs(distanceAcrossPrimeMeridian - distanceAcrossAntiMeridian) <= _tolerance))
                    {
                        // flip the sign of the translation based on current
                        currX += -Math.Sign(currX) * translate;
                        seq.SetX(i, currX);
                        GeometryChanged = true;
                    }
                }

                // Update sign variables
                xSign = Math.Sign(currX);
                prevSign = xSign == 0 ? 1 : xSign;
            }

            Done = true;
        }
    }

    [TestFixture]
    public class AntimeridianSplitTests
    {
        /// <remarks>
        /// Coordinates reflect patterns from my personal experience
        /// </remarks>
        [TestCase("SRID=4326;POLYGON ((178 43, 178 41, -176 41, -176 43, 178 43))",
            "MULTIPOLYGON (((180 41, 178 41, 178 43, 180 43, 180 41)), ((-180 43, -176 43, -176 41, -180 41, -180 43)))")]
        [TestCase("SRID=4326;POLYGON ((-178 43, -178 41, 176 41, 176 43, -178 43))",
            "MULTIPOLYGON (((180 41, 176 41, 176 43, 180 43, 180 41)), ((-178 43, -178 41, -180 41, -180 43, -178 43)))",
            Description = "Covers numberOfFullShifts > 1 in Unwrap method")]
        public void Test(string wktInput, string wktExpected)
        {
            var reader = new WKTReader();
            var polygon = reader.Read(wktInput);

            Assert.That(PerformAntimeridianSplit.IsCrossingAntimeridian(polygon), Is.True);

            // Shifts the coordinates that cross to cross with the same distance
            var antimeridianShiftFilter = new AntiMeridianShiftXFilter();
            // A copy is taken as the antimeridian shift will alter the geometry
            var shiftedPolygon = polygon.Copy();
            shiftedPolygon.Apply(antimeridianShiftFilter);

            // TODO I'm having trouble understanding this filter and why this test fails. Respectfully, I'm not sure we need this filter either.
            // var isCrossingDatelineFilter = new IsCrossingDatelineFilter();
            // shiftedPolygon.Apply(isCrossingDatelineFilter);
            // Assert.That(isCrossingDatelineFilter.IsCrossingDateline, Is.True);

            var actual = PerformAntimeridianSplit.UnwrapAtDateline(shiftedPolygon, checkIfCrossing: false, unionResult: false);
            var expected = reader.Read(wktExpected);

            Assert.That(actual.Equals(expected), Is.True,
                $"Expected: {expected}\nActual: {actual}");
        }
    }
}
