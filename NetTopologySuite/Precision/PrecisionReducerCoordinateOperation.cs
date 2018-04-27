﻿using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Precision
{
    public class PrecisionReducerCoordinateOperation : GeometryEditor.CoordinateOperation
    {
        private readonly IPrecisionModel _targetPrecModel;
        private readonly bool _removeCollapsed = true;

        public PrecisionReducerCoordinateOperation(IPrecisionModel targetPrecModel, bool removeCollapsed)
        {
            _targetPrecModel = targetPrecModel;
            _removeCollapsed = removeCollapsed;
        }

        public override Coordinate[] Edit(Coordinate[] coordinates, IGeometry geom)
        {
            if (coordinates.Length == 0)
                return null;

            var reducedCoords = new Coordinate[coordinates.Length];
            // copy coordinates and reduce
            for (int i = 0; i < coordinates.Length; i++)
            {
                var coord = new Coordinate(coordinates[i]);
                _targetPrecModel.MakePrecise(coord);
                reducedCoords[i] = coord;
            }
            // remove repeated points, to simplify returned geometry as much as possible
            var noRepeatedCoordList = new CoordinateList(reducedCoords,
                    false);
            var noRepeatedCoords = noRepeatedCoordList.ToCoordinateArray();

            /**
             * Check to see if the removal of repeated points collapsed the coordinate
             * List to an invalid length for the type of the parent geometry. It is not
             * necessary to check for Point collapses, since the coordinate list can
             * never collapse to less than one point. If the length is invalid, return
             * the full-length coordinate array first computed, or null if collapses are
             * being removed. (This may create an invalid geometry - the client must
             * handle this.)
             */
            int minLength = 0;
            if (geom is ILineString)
                minLength = 2;
            if (geom is ILinearRing)
                minLength = LinearRing.MinimumValidSize;

            Coordinate[] collapsedCoords = reducedCoords;
            if (_removeCollapsed)
                collapsedCoords = null;

            // return null or orginal length coordinate array
            if (noRepeatedCoords.Length < minLength)
            {
                return collapsedCoords;
            }

            // ok to return shorter coordinate array
            return noRepeatedCoords;
        }
    }
}