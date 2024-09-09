﻿using System;

namespace NetTopologySuite.Geometries
{
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

                // Reset extra values array
                ResetExtraValues();

                // Get number of dimensions and measures for value
                int valDimension = Coordinates.Dimension(value);
                int valMeasures = Coordinates.Measures(value);

                // Get number of values to transfer
                int numSpatialToCopy = -2 + Math.Min(Dimension - Measures, valDimension - valMeasures);
                int numMeasuresToCopy = Math.Min(Measures, valMeasures);

                // Transfer remaining spatial values
                for (int i = 0; i < numSpatialToCopy; i++)
                    _extraValues[i] = value[2 + i];

                // Transfer measure values
                int j1 = Dimension - Measures - 2;
                int j2 = valDimension - valMeasures;
                for (int i = 0; i < numMeasuresToCopy; i++)
                    _extraValues[j1++] = value[j2++];
            }
        }

        /// <summary>
        /// Resets the <see cref="_extraValues"/> array to default values.
        /// </summary>
        private void ResetExtraValues()
        {
            // Get number of spatial dimensions stored in _extraValues
            int numSpatial = Dimension - Measures;

            // Fill extra spatial values with NULL_ORDINATE, measure values 
            int i = 0;
            for (; i < numSpatial - 2; i++) _extraValues[i] = NullOrdinate;
            for (; i < Dimension - 2; i++) _extraValues[i] = 0;
        }

        public override Coordinate Copy() => new ExtraDimensionalCoordinate(X, Y, (double[])_extraValues.Clone(), Measures);
    }
}
