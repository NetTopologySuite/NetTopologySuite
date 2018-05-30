using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit
{
    /// <summary>
    ///  A base class for IGeometry tests which provides various utility methods.
    /// </summary>
    /// <author>Martin Davis</author>
    public abstract class GeometryTestCase
    {
        readonly WKTReader _reader = new WKTReader();
        private readonly IGeometryFactory _geomFactory = new GeometryFactory();

        protected void CheckEqual(IGeometry expected, IGeometry actual)
        {
            var actualNorm = actual.Normalized();
            var expectedNorm = expected.Normalized();
            var equal = actualNorm.EqualsExact(expectedNorm);
            //var writer = new WKTWriter {MaxCoordinatesPerLine };
            Assert.That(equal, Is.True, String.Format("\nExpected = {0}\nactual   = {1}", expected, actual));
        }

        protected void CheckEqual(ICollection<IGeometry> expected, ICollection<IGeometry> actual)
        {
            CheckEqual(ToGeometryCollection(expected), ToGeometryCollection(actual));
        }

        private IGeometryCollection ToGeometryCollection(ICollection<IGeometry> geoms)
        {
            return _geomFactory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
        }

        /// <summary>
        /// Reads a <see cref="IGeometry"/> from a WKT string using a custom <see cref="IGeometryFactory"/>.
        /// </summary>
        /// <param name="geomFactory">The custom factory to use</param>
        /// <param name="wkt">The WKT string</param>
        /// <returns>The geometry read</returns>
        protected IGeometry Read(IGeometryFactory geomFactory, string wkt)
        {
            var reader = new WKTReader(geomFactory);
            try
            {
                return reader.Read(wkt);
            }
            catch (GeoAPI.IO.ParseException e)
            {
                throw new AssertionException(e.Message, e);
            }
        }

        protected IGeometry Read(String wkt)
        {
            try
            {
                return _reader.Read(wkt);
            }
            catch (GeoAPI.IO.ParseException e)
            {
                throw new AssertionException(e.Message, e);
            }
        }

        protected List<IGeometry> ReadList(string[] wkt)
        {
            var geometries = new List<IGeometry>(wkt.Length);
            for (int i = 0; i < wkt.Length; i++)
            {
                geometries.Add(Read(wkt[i]));
            }
            return geometries;
        }

        protected internal static IEqualityComparer<IGeometry> EqualityComparer => new GeometryEqualityComparer();

        private class GeometryEqualityComparer : IEqualityComparer<IGeometry>
        {
            public bool Equals(IGeometry x, IGeometry y)
            {
                if (x == null && y != null)
                    return false;
                if (x != null && y == null)
                    return false;
                return x.EqualsExact(y);
            }

            public int GetHashCode(IGeometry obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}
