using NetTopologySuite.Geometries;

namespace NetTopologySuite.Elevation
{
    public abstract class ElevationModel
    {
        public abstract double GetZ(double x, double y);

        public double GetZ(Coordinate p)
        {
            return GetZ(p.X, p.Y);
        }


        /// <summary>
        /// Computes Z values for any missing Z values in a geometry,
        /// using the computed model.
        /// If the model has no Z value, or the geometry coordinate dimension
        /// does not include Z, the geometry is not updated.
        /// </summary>
        /// <param name="geom">The geometry to populate Z values for</param>
        public virtual void PopulateZ(Geometry geom)
        {
            var filter = new PopulateZFilter(this);
            geom.Apply(filter);
        }

        public static bool HasValidZ(Coordinate p) => IsValidZ(p.Z);

        public static bool IsValidZ(double z) => !double.IsNaN(z);

        private class PopulateZFilter : IEntireCoordinateSequenceFilter
        {
            private readonly ElevationModel _em;

            public PopulateZFilter(ElevationModel em)
            {
                _em = em;
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
                    // if no Z then short-circuit evaluation
                    Done = true;
                    return;
                }

                for (int i = 0; i < seq.Count; i++)
                {
                    // if Z not populated then assign using model
                    if (!IsValidZ(seq.GetZ(i)))
                    {
                        double z = _em.GetZ(seq.GetX(i), seq.GetY(i));
                        seq.SetOrdinate(i, Ordinate.Z, z);
                    }
                }
            }
        }

    }
}
