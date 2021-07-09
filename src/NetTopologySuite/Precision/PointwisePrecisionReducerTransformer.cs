using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// A transformer to reduce the precision of a geometry pointwise.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class PointwisePrecisionReducerTransformer : GeometryTransformer
    {


        public static Geometry Reduce(Geometry geom, PrecisionModel targetPM)
        {
            var trans = new PointwisePrecisionReducerTransformer(targetPM);
            return trans.Transform(geom);
        }

        private readonly PrecisionModel _targetPm;

        private PointwisePrecisionReducerTransformer(PrecisionModel targetPM)
        {
            this._targetPm = targetPM;
        }

        protected override CoordinateSequence TransformCoordinates(
            CoordinateSequence coordinates, Geometry parent)
        {
            if (coordinates.Count == 0)
                return null;

            var coordsReduce = ReducePointwise(coordinates);
            return Factory.CoordinateSequenceFactory.Create(coordsReduce);
        }

        private Coordinate[] ReducePointwise(CoordinateSequence coordinates)
        {
            var coordReduce = new Coordinate[coordinates.Count];
            // copy coordinates and reduce
            for (int i = 0; i < coordinates.Count; i++)
            {
                var coord = coordinates.GetCoordinateCopy(i);
                _targetPm.MakePrecise(coord);
                coordReduce[i] = coord;
            }

            return coordReduce;
        }
    }
}
