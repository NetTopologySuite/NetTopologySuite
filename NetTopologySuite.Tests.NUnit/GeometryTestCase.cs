﻿using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    /// <summary>
    ///  A base class for Geometry tests which provides various utility methods.
    /// </summary>
    /// <author>Martin Davis</author>
    public abstract class GeometryTestCase
    {
        readonly WKTReader _reader = new WKTReader();
        private readonly GeometryFactory _geomFactory = new GeometryFactory();

        protected GeometryTestCase()
        {
        }

        protected GeometryTestCase(ICoordinateSequenceFactory coordinateSequenceFactory)
        {
            _geomFactory = new GeometryFactory(coordinateSequenceFactory);
            _reader = new WKTReader(_geomFactory);
        }

        protected void CheckEqual(Geometry expected, Geometry actual)
        {
            var actualNorm = actual.Normalized();
            var expectedNorm = expected.Normalized();
            bool equal = actualNorm.EqualsExact(expectedNorm);
            //var writer = new WKTWriter {MaxCoordinatesPerLine };
            Assert.That(equal, Is.True, string.Format("\nExpected = {0}\nactual   = {1}", expected, actual));
        }

        protected void CheckEqual(ICollection<Geometry> expected, ICollection<Geometry> actual)
        {
            CheckEqual(ToGeometryCollection(expected), ToGeometryCollection(actual));
        }

        private GeometryCollection ToGeometryCollection(ICollection<Geometry> geoms)
        {
            return _geomFactory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
        }

        /// <summary>
        /// Reads a <see cref="Geometry"/> from a WKT string using a custom <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="geomFactory">The custom factory to use</param>
        /// <param name="wkt">The WKT string</param>
        /// <returns>The geometry read</returns>
        protected static Geometry Read(GeometryFactory geomFactory, string wkt)
        {
            var reader = new WKTReader(geomFactory);
            try
            {
                return reader.Read(wkt);
            }
            catch (ParseException e)
            {
                throw new AssertionException(e.Message, e);
            }
        }

        protected Geometry Read(string wkt)
        {
            return Read(_reader, wkt);
        }

        public static Geometry Read(WKTReader reader, string wkt)
        {
            try
            {
                return reader.Read(wkt);
            }
            catch (ParseException e)
            {
                throw new AssertionException(e.Message, e);
            }
        }

        protected List<Geometry> ReadList(string[] wkt)
        {
            var geometries = new List<Geometry>(wkt.Length);
            for (int i = 0; i < wkt.Length; i++)
            {
                geometries.Add(Read(wkt[i]));
            }
            return geometries;
        }

        public static List<Geometry> ReadList(WKTReader reader, string[] wkt)
        {
            var geometries = new List<Geometry>(wkt.Length);
            for (int i = 0; i < wkt.Length; i++)
            {
                geometries.Add(Read(reader, wkt[i]));
            }
            return geometries;
        }

        /// <summary>
        /// Gets a <see cref="WKTReader"/> to read geometries from WKT with expected ordinates.
        /// </summary>
        /// <param name="ordinateFlags">a set of expected ordinates.</param>
        /// <returns>A <see cref="WKTReader"/>.</returns>
        public static WKTReader GetWKTReader(Ordinates ordinateFlags)
        {
            return GetWKTReader(ordinateFlags, new PrecisionModel());
        }

        /// <summary>
        /// Gets a <see cref="WKTReader"/> to read geometries from WKT with expected ordinates.
        /// </summary>
        /// <param name="ordinateFlags">a set of expected ordinates.</param>
        /// <param name="scale">a scale value to create a <see cref="PrecisionModel"/>.</param>
        /// <returns>A <see cref="WKTReader"/>.</returns>
        public static WKTReader GetWKTReader(Ordinates ordinateFlags, double scale)
        {
            return GetWKTReader(ordinateFlags, new PrecisionModel(scale));
        }

        /// <summary>
        /// Gets a <see cref="WKTReader"/> to read geometries from WKT with expected ordinates.
        /// </summary>
        /// <param name="ordinateFlags">a set of expected ordinates.</param>
        /// <param name="precisionModel">a precision model.</param>
        /// <returns>A <see cref="WKTReader"/>.</returns>
        public static WKTReader GetWKTReader(Ordinates ordinateFlags, PrecisionModel precisionModel)
        {
            ordinateFlags |= Ordinates.XY;
            if ((ordinateFlags & Ordinates.XY) == ordinateFlags)
            {
                return new WKTReader(new GeometryFactory(precisionModel, 0, CoordinateArraySequenceFactory.Instance))
                {
                    IsOldNtsCoordinateSyntaxAllowed = false,
                };
            }

            // note: XYZM will go through here too, just like in JTS.
            if (ordinateFlags.HasFlag(Ordinates.Z))
            {
                return new WKTReader(new GeometryFactory(precisionModel, 0, CoordinateArraySequenceFactory.Instance));
            }

            if (ordinateFlags.HasFlag(Ordinates.M))
            {
                return new WKTReader(new GeometryFactory(precisionModel, 0, PackedCoordinateSequenceFactory.DoubleFactory))
                {
                    IsOldNtsCoordinateSyntaxAllowed = false,
                };
            }

            return new WKTReader(new GeometryFactory(precisionModel, 0, CoordinateArraySequenceFactory.Instance));
        }

        /// <summary>
        /// Checks two <see cref="ICoordinateSequence"/>s for equality.  The following items are checked:
        /// <list type="bullet">
        /// <item><description>size</description></item>
        /// <item><description>dimension</description></item>
        /// <item><description>ordinate values</description></item>
        /// </list>
        /// </summary>
        /// <param name="seq1">a sequence</param>
        /// <param name="seq2">another sequence</param>
        /// <returns><see langword="true"/> if both sequences are equal.</returns>
        public static bool CheckEqual(ICoordinateSequence seq1, ICoordinateSequence seq2)
        {
            return CheckEqual(seq1, seq2, 0d);
        }

        /// <summary>
        /// Checks two <see cref="ICoordinateSequence"/>s for equality.  The following items are checked:
        /// <list type="bullet">
        /// <item><description>size</description></item>
        /// <item><description>dimension</description></item>
        /// <item><description>ordinate values with <paramref name="tolerance"/></description></item>
        /// </list>
        /// </summary>
        /// <param name="seq1">a sequence</param>
        /// <param name="seq2">another sequence</param>
        /// <returns><see langword="true"/> if both sequences are equal.</returns>
        public static bool CheckEqual(ICoordinateSequence seq1, ICoordinateSequence seq2, double tolerance)
        {
            if (seq1?.Dimension != seq2?.Dimension)
            {
                return false;
            }

            return CheckEqual(seq1, seq2, seq1.Dimension, tolerance);
        }

        /// <summary>
        /// Checks two <see cref="ICoordinateSequence"/>s for equality.  The following items are checked:
        /// <list type="bullet">
        /// <item><description>size</description></item>
        /// <item><description>dimension up to <paramref name="dimension"/></description></item>
        /// <item><description>ordinate values</description></item>
        /// </list>
        /// </summary>
        /// <param name="seq1">a sequence</param>
        /// <param name="seq2">another sequence</param>
        /// <returns><see langword="true"/> if both sequences are equal.</returns>
        public static bool CheckEqual(ICoordinateSequence seq1, ICoordinateSequence seq2, int dimension)
        {
            return CheckEqual(seq1, seq2, dimension, 0);
        }

        /// <summary>
        /// Checks two <see cref="ICoordinateSequence"/>s for equality.  The following items are checked:
        /// <list type="bullet">
        /// <item><description>size</description></item>
        /// <item><description>dimension up to <paramref name="dimension"/></description></item>
        /// <item><description>ordinate values with <paramref name="tolerance"/></description></item>
        /// </list>
        /// </summary>
        /// <param name="seq1">a sequence</param>
        /// <param name="seq2">another sequence</param>
        /// <returns><see langword="true"/> if both sequences are equal.</returns>
        public static bool CheckEqual(ICoordinateSequence seq1, ICoordinateSequence seq2, int dimension, double tolerance)
        {
            if (ReferenceEquals(seq1, seq2))
            {
                return true;
            }

            if (seq1 is null || seq2 is null)
            {
                return false;
            }

            if (seq1.Count != seq2.Count)
            {
                return false;
            }

            if (seq1.Dimension < dimension)
            {
                throw new ArgumentException("dimension too high for seq1", nameof(seq1));
            }

            if (seq2.Dimension < dimension)
            {
                throw new ArgumentException("dimension too high for seq2", nameof(seq2));
            }

            for (int i = 0, cnt = seq1.Count; i < cnt; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    double val1 = seq1.GetOrdinate(i, j);
                    double val2 = seq2.GetOrdinate(i, j);
                    if (double.IsNaN(val1))
                    {
                        if (!double.IsNaN(val2))
                        {
                            return false;
                        }
                    }
                    else if (Math.Abs(val1 - val2) > tolerance)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a <see cref="ICoordinateSequenceFactory"/> that can create sequences for ordinates
        /// defined in the provided bit pattern.
        /// </summary>
        /// <param name="ordinateFlags">a bit-pattern of ordinates.</param>
        /// <returns>a <see cref="ICoordinateSequenceFactory"/>.</returns>
        public static ICoordinateSequenceFactory GetCSFactory(Ordinates ordinateFlags)
        {
            if (ordinateFlags.HasFlag(Ordinates.M))
            {
                return PackedCoordinateSequenceFactory.DoubleFactory;
            }

            return CoordinateArraySequenceFactory.Instance;
        }

        protected internal static IEqualityComparer<Geometry> EqualityComparer => new GeometryEqualityComparer();

        private class GeometryEqualityComparer : IEqualityComparer<Geometry>
        {
            public bool Equals(Geometry x, Geometry y)
            {
                if (x == null && y != null)
                    return false;
                if (x != null && y == null)
                    return false;
                return x.EqualsExact(y);
            }

            public int GetHashCode(Geometry obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
