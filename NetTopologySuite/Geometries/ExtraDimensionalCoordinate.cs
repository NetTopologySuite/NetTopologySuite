using System;

using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Geometries
{
    [Serializable]
    internal sealed class ExtraDimensionalCoordinate : Coordinate
    {
        private readonly PackedDoubleCoordinateSequence _seq;

        public ExtraDimensionalCoordinate(int dimension, int measures)
        {
            _seq = new PackedDoubleCoordinateSequence(1, dimension, measures);
        }

        public int Dimension => _seq.Dimension;

        public int Measures => _seq.Measures;

        public override double Z
        {
            get => _seq.GetZ(0);
            set => _seq.SetOrdinate(0, Ordinate.Z, value);
        }

        public override double M
        {
            get => _seq.GetM(0);
            set => _seq.SetOrdinate(0, Ordinate.M, value);
        }

        public override double this[int ordinateIndex]
        {
            get => _seq.GetOrdinate(0, ordinateIndex);
            set => _seq.SetOrdinate(0, ordinateIndex, value);
        }

        public override double this[Ordinate ordinate]
        {
            get => _seq.GetOrdinate(0, ordinate);
            set => _seq.SetOrdinate(0, ordinate, value);
        }

        public override Coordinate CoordinateValue
        {
            get => this;
            set
            {
                for (int dim = 0, max = Dimension; dim < max; dim++)
                {
                    _seq.SetOrdinate(0, dim, value[dim]);
                }
            }
        }

        public override Coordinate Copy()
        {
            var result = new ExtraDimensionalCoordinate(Dimension, Measures);
            result.CoordinateValue = this;
            return result;
        }
    }
}
