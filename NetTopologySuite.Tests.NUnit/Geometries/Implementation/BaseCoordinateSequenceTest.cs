using System;
using System.Collections.Generic;

using NetTopologySuite.Geometries;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Implementation
{
    /// <summary>
    /// Test for all the behavior implemented in the base <see cref="CoordinateSequence"/> class,
    /// using a dummy subclass that only implements the minimum required methods using a dictionary
    /// that tracks what the test has set so far.
    /// </summary>
    public class BaseCoordinateSequenceTest
    {
        [TestCase(15, 22, 5)]
        [TestCase(9, 13, 0)]
        [TestCase(600, 3, 1)]
        [TestCase(2, 321, 123)]
        public void TestBaseBehaviorUsingDummySequence(int count, int dimension, int measures)
        {
            int spatial = dimension - measures;
            var cs = new DummyCoordinateSequenceFactory().Create(count, dimension, measures);

            // static checks first
            Assert.That(cs.Count, Is.EqualTo(count));
            Assert.That(cs.Dimension, Is.EqualTo(dimension));
            Assert.That(cs.Measures, Is.EqualTo(measures));
            Assert.That(cs.Spatial, Is.EqualTo(dimension - measures));

            Assert.That(cs.HasZ, Is.EqualTo(spatial > 2));
            Assert.That(cs.HasM, Is.EqualTo(measures > 0));

            // now start with the tests for the actual data access methods
            var random = new Random(8675309);
            var reversed = cs.Reversed();
            var reversed2 = reversed.Reversed();

            var coordinateTemplate = cs.CreateCoordinate();
            var coordToFill = cs.CreateCoordinate();
            var coordToFillReversed = reversed.CreateCoordinate();
            var coordToFillReversed2 = reversed2.CreateCoordinate();

            double minX = double.PositiveInfinity;
            double maxX = double.NegativeInfinity;
            double minY = double.PositiveInfinity;
            double maxY = double.NegativeInfinity;

            for (int i = 0, j = count - 1; i < count; i++, j--)
            {
                for (int dim = 0; dim < dimension; dim++)
                {
                    double expectedOrdinateValue = coordinateTemplate[dim] = random.NextDouble();

                    cs.SetOrdinate(i, dim, expectedOrdinateValue);
                    Assert.That(cs.GetOrdinate(i, dim), Is.EqualTo(expectedOrdinateValue));

                    reversed.SetOrdinate(j, dim, expectedOrdinateValue);
                    Assert.That(reversed.GetOrdinate(j, dim), Is.EqualTo(expectedOrdinateValue));

                    reversed2.SetOrdinate(i, dim, expectedOrdinateValue);
                    Assert.That(reversed2.GetOrdinate(i, dim), Is.EqualTo(expectedOrdinateValue));

                    // everything else in this loop requires an Ordinate flag to match.
                    Ordinate ord;
                    if (dim < spatial)
                    {
                        if (dim > (int)Ordinate.Spatial16)
                        {
                            continue;
                        }

                        ord = Ordinate.Spatial1 + dim;
                    }
                    else
                    {
                        int measureIndex = dim - spatial;
                        if (measureIndex >= measures)
                        {
                            continue;
                        }

                        ord = Ordinate.Measure1 + measureIndex;
                    }

                    var ordinatesFlag = (Ordinates)(1 << (int)ord);
                    Assert.That(cs.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(reversed.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(reversed2.Ordinates.HasFlag(ordinatesFlag));

                    Assert.That(cs.GetOrdinate(i, ord), Is.EqualTo(expectedOrdinateValue));
                    Assert.That(reversed.GetOrdinate(j, ord), Is.EqualTo(expectedOrdinateValue));
                    Assert.That(reversed2.GetOrdinate(i, ord), Is.EqualTo(expectedOrdinateValue));

                    switch (ord)
                    {
                        case Ordinate.X:
                            Assert.That(cs.GetX(i), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed.GetX(j), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed2.GetX(i), Is.EqualTo(expectedOrdinateValue));
                            if (expectedOrdinateValue < minX)
                            {
                                minX = expectedOrdinateValue;
                            }

                            if (expectedOrdinateValue > maxX)
                            {
                                maxX = expectedOrdinateValue;
                            }

                            break;

                        case Ordinate.Y:
                            Assert.That(cs.GetY(i), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed.GetY(j), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed2.GetY(i), Is.EqualTo(expectedOrdinateValue));
                            if (expectedOrdinateValue < minY)
                            {
                                minY = expectedOrdinateValue;
                            }

                            if (expectedOrdinateValue > maxY)
                            {
                                maxY = expectedOrdinateValue;
                            }

                            break;

                        case Ordinate.Z:
                            Assert.That(cs.GetZ(i), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed.GetZ(j), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed2.GetZ(i), Is.EqualTo(expectedOrdinateValue));
                            break;

                        case Ordinate.M:
                            Assert.That(cs.GetM(i), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed.GetM(j), Is.EqualTo(expectedOrdinateValue));
                            Assert.That(reversed2.GetM(i), Is.EqualTo(expectedOrdinateValue));
                            break;
                    }
                }

                // other spatial ordinates should be inaccessible, though no errors should be thrown
                for (var ord = Ordinate.Spatial1 + spatial; ord <= Ordinate.Spatial16; ord++)
                {
                    var ordinatesFlag = (Ordinates)(1 << (int)ord);
                    Assert.That(!cs.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(!reversed.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(!reversed2.Ordinates.HasFlag(ordinatesFlag));

                    cs.SetOrdinate(i, ord, random.NextDouble());
                    reversed.SetOrdinate(j, ord, random.NextDouble());
                    reversed2.SetOrdinate(i, ord, random.NextDouble());

                    Assert.That(cs.GetOrdinate(i, ord), Is.NaN);
                    Assert.That(reversed.GetOrdinate(j, ord), Is.NaN);
                    Assert.That(reversed2.GetOrdinate(i, ord), Is.NaN);
                }

                // other measure ordinates should be inaccessible, though no errors should be thrown
                for (var ord = Ordinate.Measure1 + measures; ord <= Ordinate.Measure16; ord++)
                {
                    var ordinatesFlag = (Ordinates)(1 << (int)ord);
                    Assert.That(!cs.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(!reversed.Ordinates.HasFlag(ordinatesFlag));
                    Assert.That(!reversed2.Ordinates.HasFlag(ordinatesFlag));

                    cs.SetOrdinate(i, ord, random.NextDouble());
                    reversed.SetOrdinate(j, ord, random.NextDouble());
                    reversed2.SetOrdinate(i, ord, random.NextDouble());

                    Assert.That(cs.GetOrdinate(i, ord), Is.NaN);
                    Assert.That(reversed.GetOrdinate(j, ord), Is.NaN);
                    Assert.That(reversed2.GetOrdinate(i, ord), Is.NaN);
                }

                var coord = cs.GetCoordinate(i);
                var coordReversed = reversed.GetCoordinate(j);
                var coordReversed2 = reversed2.GetCoordinate(i);

                var coordCopy = cs.GetCoordinateCopy(i);
                var coordCopyReversed = reversed.GetCoordinateCopy(j);
                var coordCopyReversed2 = reversed2.GetCoordinateCopy(i);

                cs.GetCoordinate(i, coordToFill);
                reversed.GetCoordinate(j, coordToFillReversed);
                reversed2.GetCoordinate(i, coordToFillReversed2);

                for (int dim = 0; dim < cs.Dimension; dim++)
                {
                    coordCopy[dim] += 2;
                    coordCopyReversed[dim] += 2;
                    coordCopyReversed2[dim] += 2;

                    coordToFill[dim] += 3;
                    coordToFillReversed[dim] += 3;
                    coordToFillReversed2[dim] += 3;

                    Assert.That(coord[dim], Is.EqualTo(coordinateTemplate[dim]));
                    Assert.That(coordReversed[dim], Is.EqualTo(coordinateTemplate[dim]));
                    Assert.That(coordReversed2[dim], Is.EqualTo(coordinateTemplate[dim]));

                    Assert.That(coordCopy[dim], Is.EqualTo(coordinateTemplate[dim] + 2));
                    Assert.That(coordCopyReversed[dim], Is.EqualTo(coordinateTemplate[dim] + 2));
                    Assert.That(coordCopyReversed2[dim], Is.EqualTo(coordinateTemplate[dim] + 2));

                    Assert.That(coordToFill[dim], Is.EqualTo(coordinateTemplate[dim] + 3));
                    Assert.That(coordToFillReversed[dim], Is.EqualTo(coordinateTemplate[dim] + 3));
                    Assert.That(coordToFillReversed2[dim], Is.EqualTo(coordinateTemplate[dim] + 3));
                }
            }

            var env1 = new Envelope(minX, maxX, minY, maxY);
            var env2 = cs.ExpandEnvelope(new Envelope());
            var env3 = reversed.ExpandEnvelope(new Envelope());
            var env4 = reversed2.ExpandEnvelope(new Envelope());
            Assert.That(env2, Is.EqualTo(env1));
            Assert.That(env3, Is.EqualTo(env1));
            Assert.That(env4, Is.EqualTo(env1));

            var coordArray = cs.ToCoordinateArray();
            var coordArrayReversed = reversed.ToCoordinateArray();
            var coordArrayReversed2 = reversed2.ToCoordinateArray();

            for (int i = 0, j = count - 1; i < count; i++, j--)
            {
                var c1 = coordArray[i];
                var c2 = coordArrayReversed[j];
                var c3 = coordArrayReversed2[i];

                for (int dim = 0; dim < dimension; dim++)
                {
                    double expectedOrdinateValue = cs.GetOrdinate(i, dim);

                    Assert.That(c1[dim], Is.EqualTo(expectedOrdinateValue));
                    Assert.That(c2[dim], Is.EqualTo(expectedOrdinateValue));
                    Assert.That(c3[dim], Is.EqualTo(expectedOrdinateValue));
                }
            }
        }

        private sealed class DummyCoordinateSequenceFactory : CoordinateSequenceFactory
        {
            public override CoordinateSequence Create(int size, int dimension, int measures) => new DummyCoordinateSequence(size, dimension, measures);
        }

        private sealed class DummyCoordinateSequence : CoordinateSequence
        {
            private readonly Dictionary<(int index, int ordinateIndex), double> _vals = new Dictionary<(int index, int ordinateIndex), double>();

            public DummyCoordinateSequence(int count, int dimension, int measures)
                : base(count, dimension, measures) { }

            public override CoordinateSequence Copy()
            {
                var result = new DummyCoordinateSequence(Count, Dimension, Measures);

                foreach (var kvp in _vals)
                {
                    result._vals.Add(kvp.Key, kvp.Value);
                }

                return result;
            }

            public override double GetOrdinate(int index, int ordinateIndex) =>
                _vals.TryGetValue((index, ordinateIndex), out double value)
                    ? value
                    : throw new AssertionException("GetOrdinate was called for an index / ordinateIndex pair that was never set!");

            public override void SetOrdinate(int index, int ordinateIndex, double value) => _vals[(index, ordinateIndex)] = value;
        }
    }
}
