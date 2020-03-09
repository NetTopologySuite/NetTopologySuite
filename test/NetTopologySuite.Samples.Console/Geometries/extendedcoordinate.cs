#nullable disable
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
                    case 2:
                        return Z;
                    case 3:
                        return M;
                }
                throw new ArgumentOutOfRangeException(nameof(ordinateIndex));
            }
            set
            {
                switch (ordinateIndex)
                {
                    case 0:
                        X = value;
                        return;
                    case 1:
                        Y = value;
                        return;
                    case 2:
                        Z = value;
                        return;
                    case 3:
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
