using System;

namespace NetTopologySuite.Geometries
{
    [Serializable]
    internal sealed class ExtraDimensionalCoordinate : Coordinate
    {
        private readonly double[] _values;

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

            _values = new double[dimension];
            Measures = measures;
        }

        private ExtraDimensionalCoordinate(double[] values, int measures)
        {
            _values = values;
            Measures = measures;
        }

        public int Dimension => _values.Length;

        public int Measures { get; }

        public override double Z
        {
            get => (Dimension - Measures > 2) ? _values[2] : NullOrdinate;
            set
            {
                if (Dimension - Measures > 2)
                {
                    _values[2] = value;
                }
            }
        }

        public override double M
        {
            get => Measures > 0 ? _values[Dimension - Measures] : NullOrdinate;
            set
            {
                if (Measures > 0)
                {
                    _values[Dimension - Measures] = value;
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
            if ((uint)ordinateIndex < (uint)_values.Length)
            {
                return ref _values[ordinateIndex];
            }

            throw new ArgumentOutOfRangeException(nameof(ordinateIndex), ordinateIndex, "Ordinate index is not present in this coordinate.");
        }

        public override double this[Ordinate ordinate]
        {
            get => OrdinateRef(ordinate);
            set => OrdinateRef(ordinate) = value;
        }

        private ref double OrdinateRef(Ordinate ordinate)
        {
            // use uint instead of int for comparisons, in order to handle negatives more gracefully
            int spatial = Dimension - Measures;
            if ((uint)ordinate < (uint)Ordinate.Measure1)
            {
                if ((uint)ordinate < (uint)spatial)
                {
                    return ref _values[(int)ordinate];
                }
            }
            else
            {
                ordinate -= Ordinate.Measure1;
                if ((uint)ordinate < (uint)Measures)
                {
                    return ref _values[(int)ordinate + spatial];
                }
            }

            throw new ArgumentOutOfRangeException(nameof(ordinate), ordinate, "Ordinate is not present in this coordinate.");
        }

        public override Coordinate CoordinateValue
        {
            get => this;
            set
            {
                for (int dim = 0; dim < Dimension; dim++)
                {
                    this[dim] = value[dim];
                }
            }
        }

        public override Coordinate Copy() => new ExtraDimensionalCoordinate((double[])_values.Clone(), Measures);
    }
}
