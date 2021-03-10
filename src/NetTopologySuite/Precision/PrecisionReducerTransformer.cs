using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Precision
{
    class PrecisionReducerTransformer : GeometryTransformer
    {


        public static Geometry Reduce(Geometry geom, PrecisionModel targetPM)
        {
            return Reduce(geom, targetPM, false);
        }

        public static Geometry Reduce(Geometry geom, PrecisionModel targetPM, bool isPointwise)
        {
            var trans = new PrecisionReducerTransformer(targetPM, isPointwise);
            return trans.Transform(geom);
        }

        private readonly PrecisionModel _targetPm;
        private readonly bool _isPointwise = false;

        PrecisionReducerTransformer(PrecisionModel targetPM)
            : this(targetPM, false)
        {
        }

        PrecisionReducerTransformer(PrecisionModel targetPM, bool isPointwise)
        {
            _targetPm = targetPM;
            _isPointwise = isPointwise;
        }

        protected override CoordinateSequence TransformCoordinates(
            CoordinateSequence coordinates, Geometry parent)
        {
            if (coordinates.Count == 0)
                return null;

            Coordinate[] coordsReduce;
            if (_isPointwise)
            {
                coordsReduce = ReducePointwise(coordinates);
            }
            else
            {
                coordsReduce = ReduceCompress(coordinates);
            }

            /*
             * Check to see if the removal of repeated points collapsed the coordinate
             * List to an invalid length for the type of the parent geometry. It is not
             * necessary to check for Point collapses, since the coordinate list can
             * never collapse to less than one point. If the length is invalid, return
             * the full-length coordinate array first computed, or null if collapses are
             * being removed. (This may create an invalid geometry - the client must
             * handle this.)
             */
            int minLength = 0;
            if (parent is LineString)
                minLength = 2;
            if (parent is LinearRing)
                minLength = 4;

            // collapse - return null so parent is removed or empty
            if (coordsReduce.Length < minLength)
            {
                return null;
            }

            return Factory.CoordinateSequenceFactory.Create(coordsReduce);
        }

        private Coordinate[] ReduceCompress(CoordinateSequence coordinates)
        {
            var noRepeatCoordList = new CoordinateList();
            // copy coordinates and reduce
            for (int i = 0; i < coordinates.Count; i++)
            {
                var coord = coordinates.GetCoordinate(i).Copy();
                _targetPm.MakePrecise(coord);
                noRepeatCoordList.Add(coord, false);
            }

            // remove repeated points, to simplify returned geometry as much as possible
            var noRepeatCoords = noRepeatCoordList.ToCoordinateArray();
            return noRepeatCoords;
        }

        private Coordinate[] ReducePointwise(CoordinateSequence coordinates)
        {
            var coordReduce = new Coordinate[coordinates.Count];
            // copy coordinates and reduce
            for (int i = 0; i < coordinates.Count; i++)
            {
                var coord = coordinates.GetCoordinate(i).Copy();
                _targetPm.MakePrecise(coord);
                coordReduce[i] = coord;
            }

            return coordReduce;
        }

        protected override Geometry TransformPolygon(Polygon geom, Geometry parent)
        {
            if (_isPointwise)
            {
                var trans = base.TransformPolygon(geom, parent);
                /*
                 * For some reason the base transformer may return non-polygonal geoms here.
                 * Check this and return an empty polygon instead.
                 */
                if (trans is Polygon)
                    return trans;
                return Factory.CreatePolygon();
            }

            return ReduceArea(geom);
        }

        protected override Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
        {
            if (_isPointwise)
            {
                return base.TransformMultiPolygon(geom, parent);
            }

            return ReduceArea(geom);
        }

        private Geometry ReduceArea(Geometry geom)
        {
            var reduced = PrecisionReducer.ReducePrecision(geom, _targetPm);
            return reduced;
        }
    }
}
