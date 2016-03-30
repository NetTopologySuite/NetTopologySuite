using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Converter
{
    internal class SpatialLiteCoordinateListConverter
    {
        readonly ICoordinateSequenceFactory _factory;

        public SpatialLiteCoordinateListConverter()
            : this(GeometryFactory.Default.CoordinateSequenceFactory)
        {}

        public SpatialLiteCoordinateListConverter(ICoordinateSequenceFactory factory)
        {
            _factory = factory;
        }

        public SpatialLite.Core.API.ICoordinateList ToList(ICoordinateSequence seq)
        {
            var res = new SpatialLite.Core.Geometries.CoordinateList();
            if (seq != null)
            {
                for (var i = 0; i < seq.Count; i++)
                    res.Add(ToCoordinate(seq, i));
            }
            return res;
        }

        public ICoordinateSequence ToSequence(SpatialLite.Core.API.ICoordinateList list)
        {
            var c = list[0];
            var kind = Ordinates.XY | (c.Is3D ? Ordinates.Z : Ordinates.None) |
                       (c.IsMeasured ? Ordinates.M : Ordinates.None);
            return ToSequence(list, kind);
        }

        private ICoordinateSequence ToSequence(SpatialLite.Core.API.ICoordinateList list, Ordinates kind)
        {
            var res = _factory.Create(list.Count, kind);
            kind = res.Ordinates;

            for (var i = 0; i < list.Count; i++)
            {
                var c = list[i];
                res.SetOrdinate(i, Ordinate.X, c.X);
                res.SetOrdinate(i, Ordinate.Y, c.Y);
                if ((kind & Ordinates.Z) == Ordinates.Z)
                    res.SetOrdinate(i, Ordinate.Z, c.Z);
                if ((kind & Ordinates.M) == Ordinates.M)
                    res.SetOrdinate(i, Ordinate.M, c.M);
            }
            return res;
        }

        public SpatialLite.Core.API.Coordinate ToCoordinate(ICoordinateSequence seq, int i)
        {
            var c = new SpatialLite.Core.API.Coordinate();
            c.X = seq.GetX(i);
            c.Y = seq.GetY(i);
            if ((seq.Ordinates & Ordinates.Z) == Ordinates.Z)
                c.Z = seq.GetOrdinate(i, Ordinate.Z);
            if ((seq.Ordinates & Ordinates.M) == Ordinates.M)
                c.M = seq.GetOrdinate(i, Ordinate.M);
            return c;
        }
    }
}
