using NetTopologySuite.Geometries;

namespace NetTopologySuite.Elevation
{
    /// <summary>
    /// An elevation model that assigns a const value to z-ordinates
    /// </summary>
    public class ConstElevationModel : BaseElevationModel
    {
        readonly double _z;

        /// <summary>
        /// Creates an instance of this class. The default z-ordinate value is <see cref="Coordinate.NullOrdinate"/>
        /// </summary>
        public ConstElevationModel() : this(0, Coordinate.NullOrdinate)
        { }

        /// <summary>
        /// Creates an instance of this class that assigns <paramref name="z"/> as ordinate value
        /// </summary>
        /// <param name="srid">The id of the spatial reference system in which x- and y-ordinates have to query for z-ordinate values</param>
        /// <param name="z">The z-ordinate value</param>
        public ConstElevationModel(int srid, double z)
            : base(srid, new Envelope(double.NegativeInfinity, double.PositiveInfinity,
                                      double.NegativeInfinity, double.PositiveInfinity))
        {
            _z = z;
        }

        /// <inheritdoc/>
        protected override double GetZValue(double x, double y) => _z;

        /// <inheritdoc/>
        public override void PopulateZ(Geometry geom)
        {
            var filter = new PopulateZFilter(_z);
            geom.Apply(filter);
        }

        private class PopulateZFilter : IEntireCoordinateSequenceFilter
        {
            readonly double _z;

            public PopulateZFilter(double z)
            {
                _z = z;
            }

            public bool Done { get; private set; }

            public bool GeometryChanged
            {
                get => false;
            }

            public void Filter(CoordinateSequence seq)
            {
                if (!seq.HasZ)
                {
                    Done = true;
                    return;
                }

                for (int i = 0; i < seq.Count; i++)
                {
                    if (!IsValidZ(seq.GetZ(i)))
                    {
                        seq.SetOrdinate(i, Ordinate.Z, _z);
                    }
                }
            }
        }
    }
}
