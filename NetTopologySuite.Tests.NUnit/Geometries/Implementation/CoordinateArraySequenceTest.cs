using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    //Tests are exposed by CoordinateSequenceTestBase type
    public class CoordinateArraySequenceTest : CoordinateSequenceTestBase
    {
        public CoordinateArraySequenceTest()
        {
            base.csFactory = CoordinateArraySequenceFactory.Instance;
        }
    }
}