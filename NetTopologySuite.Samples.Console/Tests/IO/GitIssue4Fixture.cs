

using System.Collections.Generic;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Operation.IO
{
    /// <summary>
    /// https://github.com/NetTopologySuite/NetTopologySuite/issues/4
    /// </summary>
    [TestFixture]
    public class GitIssue4Fixture
    {
        private static Coordinate[] CreateCoords()
        {
            IList<Coordinate> coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(0, 0));
            coordinates.Add(new Coordinate(1, 0));
            coordinates.Add(new Coordinate(1, 1));
            coordinates.Add(new Coordinate(0, 1));
            coordinates.Add(new Coordinate(0, 0));
            return coordinates.ToArray();
        }

        private static ICoordinateSequence CopyToSequence(Coordinate[] coords, ICoordinateSequence sequence)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                sequence.SetOrdinate(i, Ordinate.X, coords[i].X);
                sequence.SetOrdinate(i, Ordinate.Y, coords[i].Y);
            }
            return sequence;
        }

        [Test]
        public void create_xyonly_geom_using_sequence_and_dimension_two()
        {
            Coordinate[] coords = CreateCoords();

            CoordinateArraySequenceFactory factory = CoordinateArraySequenceFactory.Instance;
            ICoordinateSequence sequence = CopyToSequence(coords, factory.Create(coords.Length, 2));

            IPolygon polygon = GeometryFactory.Default.CreatePolygon(sequence);
            Assert.That(polygon, Is.Not.Null);
            Assert.That(polygon.Shell, Is.Not.Null);
            Assert.That(polygon.Shell.CoordinateSequence.Dimension, Is.EqualTo(2));
        }

        [Test]
        public void create_xyonly_geom_using_sequence_and_ordinates_xy()
        {
            Coordinate[] coords = CreateCoords();

            CoordinateArraySequenceFactory factory = CoordinateArraySequenceFactory.Instance;
            ICoordinateSequence sequence = CopyToSequence(coords, factory.Create(coords.Length, Ordinates.XY));

            IPolygon polygon = GeometryFactory.Default.CreatePolygon(sequence);
            Assert.That(polygon, Is.Not.Null);
            Assert.That(polygon.Shell, Is.Not.Null);
            Assert.That(polygon.Shell.CoordinateSequence.Dimension, Is.EqualTo(2));
        }        
    }
}