using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A mutable list of coordinates.
    /// </summary>
    public sealed class CoordinateBuffer
    {
        private readonly List<double> _xs;

        private readonly List<double> _ys;

        private readonly List<double> _zs;

        private readonly List<double> _ms;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateBuffer"/> class.
        /// </summary>
        /// <param name="ordinates">
        /// The <see cref="Ordinates"/> to store in the list.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ordinates"/> is not at least <see cref="Ordinates.XY"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="ordinates"/> contains more than <see cref="Ordinates.XYZM"/>.
        /// </exception>
        public CoordinateBuffer(Ordinates ordinates)
            : this(ordinates, 4)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoordinateBuffer"/> class.
        /// </summary>
        /// <param name="ordinates">
        /// The <see cref="Ordinates"/> to store in the list.
        /// </param>
        /// <param name="initialCapacity">
        /// The inital number of coordinates to reserve space for.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="ordinates"/> is not at least <see cref="Ordinates.XY"/>, or
        /// <paramref name="initialCapacity"/> is negative.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <paramref name="ordinates"/> contains ordinates other than <see cref="Ordinates.X"/>,
        /// <see cref="Ordinates.Y"/>, <see cref="Ordinates.Z"/>, and <see cref="Ordinates.M"/>.
        /// </exception>
        public CoordinateBuffer(Ordinates ordinates, int initialCapacity)
        {
            if ((ordinates & Ordinates.XY) != Ordinates.XY)
            {
                throw new ArgumentOutOfRangeException(nameof(ordinates), ordinates, "Must store at least X and Y.");
            }

            if ((ordinates & ~Ordinates.XYZM) != Ordinates.None)
            {
                throw new NotSupportedException("Only X, Y, Z, and M ordinates are recognized.");
            }

            if (initialCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCapacity), initialCapacity, "Must be non-negative.");
            }

            _xs = new List<double>(initialCapacity);
            _ys = new List<double>(initialCapacity);

            if ((ordinates & Ordinates.Z) == Ordinates.Z)
            {
                _zs = new List<double>(initialCapacity);
            }

            if ((ordinates & Ordinates.M) == Ordinates.M)
            {
                _ms = new List<double>(initialCapacity);
            }
        }

        /// <summary>
        /// Gets the number of coordinates currently stored in the list.
        /// </summary>
        public int Count => _xs.Count;

        /// <summary>
        /// Gets the <see cref="Ordinates"/> that this list stores.
        /// </summary>
        public Ordinates StoredOrdinates
        {
            get
            {
                var result = Ordinates.XY;

                if (_zs != null)
                {
                    result |= Ordinates.Z;
                }

                if (_ms != null)
                {
                    result |= Ordinates.M;
                }

                return result;
            }
        }

        /// <summary>
        /// Adds a coordinate to this list.
        /// </summary>
        /// <param name="x">
        /// The <see cref="Ordinate.X"/> value of the coordinate to add.
        /// </param>
        /// <param name="y">
        /// The <see cref="Ordinate.Y"/> value of the coordinate to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="StoredOrdinates"/> is not exactly <see cref="Ordinates.XY"/>.
        /// </exception>
        public void AddCoordinateXY(double x, double y)
        {
            this.ValidateProvidedOrdinates(Ordinates.XY);
            _xs.Add(x);
            _ys.Add(y);
        }

        /// <summary>
        /// Adds a coordinate to this list.
        /// </summary>
        /// <param name="x">
        /// The <see cref="Ordinate.X"/> value of the coordinate to add.
        /// </param>
        /// <param name="y">
        /// The <see cref="Ordinate.Y"/> value of the coordinate to add.
        /// </param>
        /// <param name="z">
        /// The <see cref="Ordinate.Z"/> value of the coordinate to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="StoredOrdinates"/> is not exactly <see cref="Ordinates.XYZ"/>.
        /// </exception>
        public void AddCoordinateXYZ(double x, double y, double z)
        {
            this.ValidateProvidedOrdinates(Ordinates.XYZ);
            _xs.Add(x);
            _ys.Add(y);
            _zs.Add(z);
        }

        /// <summary>
        /// Adds a coordinate to this list.
        /// </summary>
        /// <param name="x">
        /// The <see cref="Ordinate.X"/> value of the coordinate to add.
        /// </param>
        /// <param name="y">
        /// The <see cref="Ordinate.Y"/> value of the coordinate to add.
        /// </param>
        /// <param name="m">
        /// The <see cref="Ordinate.M"/> value of the coordinate to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="StoredOrdinates"/> is not exactly <see cref="Ordinates.XYM"/>.
        /// </exception>
        public void AddCoordinateXYM(double x, double y, double m)
        {
            this.ValidateProvidedOrdinates(Ordinates.XYM);
            _xs.Add(x);
            _ys.Add(y);
            _ms.Add(m);
        }

        /// <summary>
        /// Adds a coordinate to this list.
        /// </summary>
        /// <param name="x">
        /// The <see cref="Ordinate.X"/> value of the coordinate to add.
        /// </param>
        /// <param name="y">
        /// The <see cref="Ordinate.Y"/> value of the coordinate to add.
        /// </param>
        /// <param name="z">
        /// The <see cref="Ordinate.Z"/> value of the coordinate to add.
        /// </param>
        /// <param name="m">
        /// The <see cref="Ordinate.M"/> value of the coordinate to add.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <see cref="StoredOrdinates"/> is not exactly <see cref="Ordinates.XYZM"/>.
        /// </exception>
        public void AddCoordinateXYZM(double x, double y, double z, double m)
        {
            this.ValidateProvidedOrdinates(Ordinates.XYZM);
            _xs.Add(x);
            _ys.Add(y);
            _zs.Add(z);
            _ms.Add(m);
        }

        private void ValidateProvidedOrdinates(Ordinates providedOrdinates)
        {
            if (providedOrdinates != this.StoredOrdinates)
            {
                this.ThrowForOrdinateMismatch(providedOrdinates);
            }
        }

        private void ThrowForOrdinateMismatch(Ordinates providedOrdinates)
        {
            bool gaveZ = (providedOrdinates & Ordinates.Z) == Ordinates.Z;
            bool gaveM = (providedOrdinates & Ordinates.M) == Ordinates.M;
            bool needZ = this._zs != null;
            bool needM = this._ms != null;

            bool extraZ = gaveZ && !needZ;
            bool missingZ = needZ && !gaveZ;
            bool extraM = gaveM && !needM;
            bool missingM = needM && !gaveM;

            var messages = new List<string>(3) { "Wrong AddCoordinate method was called for what this list stores." };

            if (gaveZ)
            {
                if (!needZ)
                {
                    messages.Add("Value was provided for Z, but this list does not store Z values.");
                }
            }
            else if (needZ)
            {
                messages.Add("No value was provided for Z, but this list stores Z values.");
            }

            if (gaveM)
            {
                if (!needM)
                {
                    messages.Add("Value was provided for M, but this list does not store M values.");
                }
            }
            else if (needM)
            {
                messages.Add("No value was provided for M, but this list stores M values.");
            }

            throw new InvalidOperationException(string.Join(" ", messages.ToArray()));
        }

        /// <summary>
        /// Copies the coordinates stored in this list to a <see cref="ICoordinateSequence"/>.
        /// </summary>
        /// <param name="coordinateSequence">
        /// The <see cref="ICoordinateSequence"/> to fill.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="coordinateSequence"/> is <see langword="null" />.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="coordinateSequence"/>'s <see cref="ICoordinateSequence.Count"/> is less
        /// than <see cref="Count"/> or its <see cref="ICoordinateSequence.Ordinates"/> demands that
        /// more ordinates be set than what this list has in <see cref="StoredOrdinates"/>.
        /// </exception>
        /// <remarks>
        /// If <paramref name="coordinateSequence"/>'s <see cref="ICoordinateSequence.Ordinates"/>
        /// omits flags that are present in <see cref="StoredOrdinates"/>, then no error is thrown;
        /// instead, values stored in this list are simply not copied to the sequence.
        /// </remarks>
        public void CopyTo(ICoordinateSequence coordinateSequence)
        {
            if (coordinateSequence == null)
            {
                throw new ArgumentNullException(nameof(coordinateSequence));
            }

            if (coordinateSequence.Count < this.Count)
            {
                throw new ArgumentException("Must have enough room for all our coordinates.", nameof(coordinateSequence));
            }

            var inputOrdinates = this.StoredOrdinates;
            var outputOrdinates = coordinateSequence.Ordinates;

            var ordinateColumnList = new List<OrdinateColumn>(4);

            if ((outputOrdinates & Ordinates.X) == Ordinates.X)
            {
                ordinateColumnList.Add(new OrdinateColumn(Ordinate.X, _xs));
            }

            if ((outputOrdinates & Ordinates.Y) == Ordinates.Y)
            {
                ordinateColumnList.Add(new OrdinateColumn(Ordinate.Y, _ys));
            }

            if ((outputOrdinates & Ordinates.Z) == Ordinates.Z)
            {
                ordinateColumnList.Add(new OrdinateColumn(Ordinate.Z, _zs));
            }

            if ((outputOrdinates & Ordinates.M) == Ordinates.M)
            {
                ordinateColumnList.Add(new OrdinateColumn(Ordinate.M, _ms));
            }

            var ordinateColumns = ordinateColumnList.ToArray();
            for (int i = 0, cnt = this.Count; i < cnt; i++)
            {
                foreach (var (ordinate, vals) in ordinateColumns)
                {
                    coordinateSequence.SetOrdinate(i, ordinate, vals?[i] ?? Coordinate.NullOrdinate);
                }
            }
        }

        private struct OrdinateColumn
        {
            private Ordinate _ordinate;

            public List<double> _vals;

            public OrdinateColumn(Ordinate ordinate, List<double> vals)
            {
                _ordinate = ordinate;
                _vals = vals;
            }

            public void Deconstruct(out Ordinate ordinate, out List<double> vals)
            {
                ordinate = _ordinate;
                vals = _vals;
            }
        }
    }
}
