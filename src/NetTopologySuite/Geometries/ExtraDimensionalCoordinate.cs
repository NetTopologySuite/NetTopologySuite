using System;

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
            get => Dimension - Measures > 2 ? _extraValues[0] : NullOrdinate;
            set
            {
                if (Dimension - Measures > 2)
                {
                    _extraValues[0] = value;
                }
            }
        }

        public override double M
        {
            get => Measures > 0 ? _extraValues[_extraValues.Length - Measures] : NullOrdinate;
            set
            {
                if (Measures > 0)
                {
                    _extraValues[_extraValues.Length - Measures] = value;
                }
            }
        }

        public override double this[int ordinateIndex]
        {
            get => OrdinateRef(ordinateIndex);
            set => OrdinateRef(ordinateIndex) = value;
        }

        private ref double OrdinateRef(int ordinateIndex)
        {
            if (ordinateIndex == 0)
            {
                return ref _x;
            }

            if (ordinateIndex == 1)
            {
                return ref _y;
            }

            ordinateIndex -= 2;

            // use uint instead of int for comparisons, in order to handle negatives more gracefully
            if ((uint)ordinateIndex >= (uint)_extraValues.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex), ordinateIndex + 2, "Not present in this coordinate.");
            }

            return ref _extraValues[ordinateIndex];
        }

        public override double this[Ordinate ordinate]
        {
            get => OrdinateRef(ordinate);
            set => OrdinateRef(ordinate) = value;
        }

        private ref double OrdinateRef(Ordinate ordinate)
        {
            if (ordinate == Ordinate.X)
            {
                return ref _x;
            }

            if (ordinate == Ordinate.Y)
            {
                return ref _y;
            }

            int spatial = Dimension - Measures;

            // use uint instead of int for comparisons, in order to handle negatives more gracefully
            if ((uint)ordinate < (uint)Ordinate.Measure1)
            {
                if ((uint)ordinate < (uint)spatial)
                {
                    return ref _extraValues[(int)(ordinate - 2)];
                }
            }
            else if ((uint)ordinate <= (uint)Ordinate.Measure16)
            {
                ordinate -= Ordinate.Measure1;
                if ((uint)ordinate < (uint)Measures)
                {
                    return ref _extraValues[(int)(ordinate - 2) + spatial];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(ordinate), ordinate, "Not present in this coordinate.");
        }

        public override Coordinate CoordinateValue
        {
            get => this;
            set
            {
                X = value.X;
                Y = value.Y;

                int maxInputDim = Coordinates.Dimension(value);
                int maxOutputDim = Dimension;
                if (maxInputDim > maxOutputDim)
                {
                    maxInputDim = maxOutputDim;
                }

                int dim;
                for (dim = 2; dim < maxInputDim; dim++)
                {
                    _extraValues[dim - 2] = value[dim];
                }

                for (; dim < maxOutputDim; dim++)
                {
                    _extraValues[dim - 2] = NullOrdinate;
                }
            }
        }

        public override Coordinate Copy() => new ExtraDimensionalCoordinate(X, Y, (double[])_extraValues.Clone(), Measures);
    }
}
