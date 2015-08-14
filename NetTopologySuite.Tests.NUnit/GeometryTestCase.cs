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
            Assert.That(equal, Is.True, String.Format("Expected = {0}\nactual   = {1}", expected, actual));
        }

        protected void CheckEqual(ICollection<IGeometry> expected, ICollection<IGeometry> actual)
        {
            CheckEqual(ToGeometryCollection(expected), ToGeometryCollection(actual)); 
        }

        private IGeometryCollection ToGeometryCollection(ICollection<IGeometry> geoms)
        {
            return _geomFactory.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geoms));
        }

        protected IGeometry Read(String wkt)
        {
            return _reader.Read(wkt);
        }

        protected List<IGeometry> ReadList(String[] wkt)
        {
            var geometries = new List<IGeometry>(wkt.Length);
            for (int i = 0; i < wkt.Length; i++)
            {
                geometries.Add(Read(wkt[i]));
            }
            return geometries;
        }

    }
}
