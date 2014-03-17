using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    // Copyright 2012 - Felix Obermaier (www.ivv-aachen.de)
    //
    // This file is part of NetTopologySuite.Tests.NUnit.
    // SharpMap is free software; you can redistribute it and/or modify
    // it under the terms of the GNU Lesser General Public License as published by
    // the Free Software Foundation; either version 2 of the License, or
    // (at your option) any later version.
    //
    // SharpMap is distributed in the hope that it will be useful,
    // but WITHOUT ANY WARRANTY; without even the implied warranty of
    // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    // GNU Lesser General Public License for more details.
    //
    // You should have received a copy of the GNU Lesser General Public License
    // along with SharpMap; if not, write to the Free Software
    // Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

    [TestFixtureAttribute]
    public class BasicCoordinateSequenceTest
    {
        [TestAttribute]
        public void TestClone()
        {
            ICoordinateSequence s1 = CoordinateArraySequenceFactory.Instance.Create(
                new Coordinate[] { new Coordinate(1, 2), new Coordinate(3, 4) });
            ICoordinateSequence s2 = (ICoordinateSequence)s1.Clone();
            Assert.IsTrue(s1.GetCoordinate(0).Equals(s2.GetCoordinate(0)));
            Assert.IsTrue(s1.GetCoordinate(0) != s2.GetCoordinate(0));
        }
    }
}