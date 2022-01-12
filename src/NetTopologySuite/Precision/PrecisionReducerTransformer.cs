using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// A transformer to reduce the precision of geometry in a
    /// topologically valid way.<br/>
    /// Repeated points are removed.
    /// If geometry elements collapse below their valid length,
    /// they may be removed
    /// by specifying <c>isRemoveCollapsed</c> as <c>true</c>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class PrecisionReducerTransformer : GeometryTransformer
    {


        public static Geometry Reduce(Geometry geom, PrecisionModel targetPM, bool isRemoveCollapsed)
        {
            var trans = new PrecisionReducerTransformer(targetPM, isRemoveCollapsed);
            return trans.Transform(geom);
        }

        private readonly PrecisionModel _targetPm;
        private readonly bool _isRemoveCollapsed;

        PrecisionReducerTransformer(PrecisionModel targetPM, bool isRemoveCollapsed)
        {
            _targetPm = targetPM;
            _isRemoveCollapsed = isRemoveCollapsed;
        }

        protected override CoordinateSequence TransformCoordinates(
            CoordinateSequence coordinates, Geometry parent)
        {
            if (coordinates.Count == 0)
                return null;

            var coordsReduce = ReduceCompress(coordinates);

            /*
             * Check if the removal of repeated points collapsed the coordinate
             * list to an invalid size for the type of the parent geometry. It is not
             * necessary to check for Point collapses, since the coordinate list can
             * never collapse to less than one point. If the size is invalid, return
             * the full-size coordinate array first computed, or null if collapses are
             * being removed. (This may create an invalid geometry - the client must
             * handle this.)
             */
            int minSize = 0;
            if (parent is LineString)
                minSize = 2;
            if (parent is LinearRing)
                minSize = LinearRing.MinimumValidSize;

            /*
             * Handle collapse. If specified return null so parent geometry is removed or empty,
             * otherwise extend to required length.
             */
            if (coordsReduce.Length < minSize)
            {
                if (_isRemoveCollapsed)
                {
                    return null;
                }

                coordsReduce = Extend(coordsReduce, minSize);
            }
            return Factory.CoordinateSequenceFactory.Create(coordsReduce);

        }

        private Coordinate[] Extend(Coordinate[] coords, int minLength)
        {
            if (coords.Length >= minLength)
                return coords;
            var exCoords = new Coordinate[minLength];
            for (int i = 0; i < exCoords.Length; i++)
            {
                int iSrc = i < coords.Length ? i : coords.Length - 1;
                exCoords[i] = coords[iSrc].Copy();
            }
            return exCoords;
        }

        private Coordinate[] ReduceCompress(CoordinateSequence coordinates)
        {
            var noRepeatCoordList = new CoordinateList(coordinates.Count);
            // copy coordinates and reduce
            for (int i = 0; i < coordinates.Count; i++)
            {
                var coord = coordinates.GetCoordinate(i).Copy();
                _targetPm.MakePrecise(coord);
                noRepeatCoordList.Add(coord, false);
            }

            // remove repeated points, to simplify geometry as much as possible
            var noRepeatCoords = noRepeatCoordList.ToCoordinateArray();
            return noRepeatCoords;
        }

        /// <inheritdoc cref="GeometryTransformer.TransformPolygon"/>
        protected override Geometry TransformPolygon(Polygon geom, Geometry parent)
        {
            return ReduceArea(geom);
        }

        /// <inheritdoc cref="GeometryTransformer.TransformMultiPolygon"/>
        protected override Geometry TransformMultiPolygon(MultiPolygon geom, Geometry parent)
        {
            return ReduceArea(geom);
        }

        private Geometry ReduceArea(Geometry geom)
        {
            var reduced = PrecisionReducer.ReducePrecision(geom, _targetPm);
            return reduced;
        }
    }
}
