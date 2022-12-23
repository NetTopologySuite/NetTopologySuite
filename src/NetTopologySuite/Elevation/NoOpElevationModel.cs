using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm.Elevation
{
    internal class NoOpElevationModel : IElevationModel
    {
        readonly double _z;

        public NoOpElevationModel(double z)
        {
            _z = z;
        }

        public Coordinate CopyWithZ(Coordinate p, double z)
        {
            return ZInterpolate.CopyWithZ(p, z);
        }

        public Coordinate CopyWithZInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return ZInterpolate.CopyWithZInterpolate(p, p1, p2);
        }

        public IElevationModel Create(Geometry geom1, Geometry geom2)
        {
            return this;
        }

        public double GetZ(double x, double y)
        {
            return _z;
        }

        public void PopulateZ(Geometry geom)
        {
            var filter = new PopulateZFilter(_z);
            geom.Apply(filter);
        }

        public double GetZFrom(Coordinate p, Coordinate q)
        {
            return _z;
        }

        public double GetZFromOrInterpolate(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return _z;
        }

        public double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2)
        {
            return _z;
        }

        public double InterpolateZ(Coordinate p, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            return _z;
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
                    if (double.IsNaN(seq.GetZ(i)))
                    {
                        seq.SetOrdinate(i, Ordinate.Z, _z);
                    }
                }
            }
        }
    }
}