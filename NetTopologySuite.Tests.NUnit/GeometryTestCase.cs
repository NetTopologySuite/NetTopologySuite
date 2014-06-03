using System;
using GeoAPI.Geometries;
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

        protected void CheckEqual(IGeometry expected, IGeometry actual)
        {
            IGeometry actualNorm = actual.Normalized();       
            IGeometry expectedNorm = expected.Normalized();                 
            bool equal = actualNorm.EqualsExact(expectedNorm);
            Assert.That(equal, Is.True, String.Format("Expected = {0} actual = {1}", expected, actual));
        }

        protected IGeometry Read(String wkt)
        {
            return _reader.Read(wkt);
        }
    }
}
