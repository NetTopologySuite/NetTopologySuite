using System;

namespace NetTopologySuite.Geometries
{
    // Please treat this internal class, ExtraDimensionalCoordinate, as an **implementation detail**
    // of the various methods on the Coordinates static class. The use cases for this type are hard
    // to quantify. It was originally created to let Coordinates.Create return a Coordinate that
    // behaves as requested instead of either silently deciding to give the user something that they
    // didn't ask for (JTS behavior) or throwing an exception (a perhaps-reasonable alternative).
    [Serializable]
    internal sealed class ExtraDimensionalCoordinate : Coordinate
    {
        private readonly double[] _extraValues;

        public ExtraDimensionalCoordinate(int dimension, int measures)
        {
            if (dimension < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Must be non-negative.");
            }

            if (measures < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension), dimension, "Must be non-negative.");
            }

            if (dimension - measures < 2)
            {
                throw new ArgumentException("Must have at least two spatial dimensions.");
            }

            _extraValues = new double[dimension - 2];
            Measures = measures;
        }

        private ExtraDimensionalCoordinate(double x, double y, double[] extraValues, int measures)
        {
            X = x;
            Y = y;
            _extraValues = extraValues;
            Measures = measures;
        }

        public int Dimension => _extraValues.Length + 2;

        public int Measures { get; }

        public override double Z
        {
            get
            {
                if (Dimension - Measures > 2)
                {
                    return _extraValues[0];
                }
                else
                {
                    // inherit whatever the base class does when Z is missing.
                    return base.Z;
                }
            }

            set
            {
                if (Dimension - Measures > 2)
                {
                    _extraValues[0] = value;
                }
                else
                {
                    // inherit whatever the base class does when Z is missing.
                    base.Z = value;
                }
            }
        }

        public override double M
        {
            get
            {
                if (Measures > 0)
                {
                    return _extraValues[_extraValues.Length - Measures];
                }
                else
                {
                    // inherit whatever the base class does when M is missing.
                    return base.M;
                }
            }

            set
            {
                if (Measures > 0)
                {
                    _extraValues[_extraValues.Length - Measures] = value;
                }
                else
                {
                    // inherit whatever the base class does when M is missing.
                    base.M = value;
                }
            }
        }

        public override double this[int ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case 0:
                        return X;

                    case 1:
                        return Y;

                    default:
                        // use uint instead of int, to handle negatives more gracefully
                        if ((uint)(ordinateIndex - 2) < (uint)_extraValues.Length)
                        {
                            return _extraValues[ordinateIndex - 2];
                        }

                        // inherit whatever the base class does when an ordinate is missing.
                        return base[ordinateIndex];
                }
            }

            set
            {
                switch (ordinateIndex)
                {
                    case 0:
                        X = value;
                        break;

                    case 1:
                        Y = value;
                        break;

                    default:
                        // use uint instead of int, to handle negatives more gracefully
                        if ((uint)(ordinateIndex - 2) < (uint)_extraValues.Length)
                        {
                            _extraValues[ordinateIndex - 2] = value;
                            break;
                        }

                        // inherit whatever the base class does when an ordinate is missing.
                        base[ordinateIndex] = value;
                        break;
                }
            }
        }

        public override double this[Ordinate ordinate]
        {
            get
            {
                switch (ordinate)
                {
                    case Ordinate.X:
                        return X;

                    case Ordinate.Y:
                        return Y;

                    default:
                        int spatial = Dimension - Measures;

                        // use uint instead of int, to handle negatives more gracefully
                        if ((uint)ordinate < (uint)Ordinate.Measure1)
                        {
                            if ((uint)ordinate < (uint)spatial)
                            {
                                return _extraValues[(int)(ordinate - 2)];
                            }
                        }
                        else if ((uint)ordinate <= (uint)Ordinate.Measure16)
                        {
                            if ((uint)(ordinate - Ordinate.Measure1) < (uint)Measures)
                            {
                                return _extraValues[spatial + (ordinate - Ordinate.Measure1) - 2];
                            }
                        }

                        // inherit whatever the base class does when an ordinate is missing.
                        return base[ordinate];
                }
            }

            set
            {
                switch (ordinate)
                {
                    case Ordinate.X:
                        X = value;
                        break;

                    case Ordinate.Y:
                        Y = value;
                        break;

                    default:
                        int spatial = Dimension - Measures;

                        // use uint instead of int, to handle negatives more gracefully
                        if ((uint)ordinate < (uint)Ordinate.Measure1)
                        {
                            if ((uint)ordinate < (uint)spatial)
                            {
                                _extraValues[(int)(ordinate - 2)] = value;
                                break;
                            }
                        }
                        else if ((uint)ordinate <= (uint)Ordinate.Measure16)
                        {
                            if ((uint)(ordinate - Ordinate.Measure1) < (uint)Measures)
                            {
                                _extraValues[spatial + (ordinate - Ordinate.Measure1) - 2] = value;
                                break;
                            }
                        }

                        // inherit whatever the base class does when an ordinate is missing.
                        base[ordinate] = value;
                        break;
                }
            }
        }

        public override Coordinate CoordinateValue
        {
            get => this;
            set
            {
                X = value.X;
                Y = value.Y;

                // Get number of dimensions and measures for value
                int valDimension = Coordinates.Dimension(value);
                int valSpatial = Coordinates.SpatialDimension(value);

                // Get number of spatial dimensions
                int thisSpatial = Dimension - Measures;

                // fill in values for spatial dimensions until we reach the end of either one.
                int thisI = 2, valI = 2;
                while (thisI < thisSpatial & valI < valSpatial)
                {
                    this[thisI++] = value[valI++];
                }
                // fill in remaining spatial dimensions that weren't present in the source.
                while (thisI < thisSpatial)
                {
                    this[thisI++] = Coordinate.NullOrdinate;
                }
                // fill in values for measures until we reach the end of either one.
                valI = valSpatial;
                while (thisI < Dimension && valI < valDimension)
                {
                    this[thisI++] = value[valI++];
                }
                // fill in remaining measures that weren't present in the source.
                while (thisI < Dimension)
                {
                    this[thisI++] = 0;
                }
            }
        }

        public override Coordinate Copy() => new ExtraDimensionalCoordinate(X, Y, (double[])_extraValues.Clone(), Measures);
    }
}
