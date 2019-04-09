using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Samples.Geometries
{
    public class ExtendedCoordinate : CoordinateZ
    {
        // A Coordinate subclass should provide all of these methods

        /// <summary>
        /// Default constructor
        /// </summary>
        public ExtendedCoordinate()
        {
            _m = 0.0;
        }

        public ExtendedCoordinate(double x, double y, double z, double m) : base(x, y, z)
        {
            _m = m;
        }

        public ExtendedCoordinate(Coordinate coord) : base(coord)
        {
            _m = 0.0;
        }

        public ExtendedCoordinate(ExtendedCoordinate coord)
            : base(coord)
        {
            _m = coord._m;
        }

        /// <summary>
        /// An example of extended data.
        /// The m variable holds a measure value for linear referencing
        /// </summary>
        private double _m;

        public override double M
        {
            get => _m;
            set => _m = value;
        }

        public override Coordinate CoordinateValue
        {
            get => this;
            set
            {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                M = value.M;
            }
        }

        /// <inheritdoc />
        public override double this[Ordinate ordinateIndex]
        {
            get
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        return X;
                    case Ordinate.Y:
                        return Y;
                    case Ordinate.Z:
                        return Z;
                    case Ordinate.M:
                        return M;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
            set
            {
                switch (ordinateIndex)
                {
                    case Ordinate.X:
                        X = value;
                        return;
                    case Ordinate.Y:
                        Y = value;
                        return;
                    case Ordinate.Z:
                        Z = value;
                        return;
                    case Ordinate.M:
                        M = value;
                        return;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            string stringRep = "(" + X + "," + Y + "," + Z + " m=" + M + ")";
            return stringRep;
        }
    }
}
