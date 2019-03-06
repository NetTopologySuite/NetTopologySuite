using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of the coordinates of a <c>Geometry</c>
    /// according to the supplied {PrecisionModel}, without
    /// attempting to preserve valid topology.
    /// </summary>
    /// <remarks>
    /// In case of <see cref="IPolygonal"/> geometries,
    /// the topology of the resulting geometry may be invalid if
    /// topological collapse occurs due to coordinates being shifted.
    /// It is up to the client to check this and handle it if necessary.
    /// Collapses may not matter for some uses. An example
    /// is simplifying the input to the buffer algorithm.
    /// The buffer algorithm does not depend on the validity of the input point.
    /// </remarks>
    [Obsolete("Use GeometryPrecisionReducer")]
    public class SimpleGeometryPrecisionReducer
    {
        /// <summary>
        /// Convenience method for doing precision reduction on a single geometry,
        /// with collapses removed and keeping the geometry precision model the same.
        /// </summary>
        /// <returns>The reduced geometry</returns>
        public static IGeometry Reduce(IGeometry g, PrecisionModel precModel)
        {
            var reducer = new SimpleGeometryPrecisionReducer(precModel);
            return reducer.Reduce(g);
        }

        private readonly PrecisionModel _newPrecisionModel;
        private bool _removeCollapsed = true;
        private bool _changePrecisionModel;

        /// <summary>
        ///
        /// </summary>
        /// <param name="pm"></param>
        public SimpleGeometryPrecisionReducer(PrecisionModel pm)
        {
            _newPrecisionModel = pm;
        }

        /// <summary>
        /// Sets whether the reduction will result in collapsed components
        /// being removed completely, or simply being collapsed to an (invalid)
        /// Geometry of the same type.
        /// </summary>
        public bool RemoveCollapsedComponents
        {
            get => _removeCollapsed;
            set => _removeCollapsed = value;
        }

        /// <summary>
        /// Gets/Sets whether the PrecisionModel of the new reduced Geometry
        /// will be changed to be the PrecisionModel supplied to
        /// specify the precision reduction.  <para/>
        /// The default is to not change the precision model.
        /// </summary>
        public bool ChangePrecisionModel
        {
            get => _changePrecisionModel;
            set => _changePrecisionModel = value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public IGeometry Reduce(IGeometry geom)
        {
            GeometryEditor geomEdit;
            if (_changePrecisionModel)
            {
                var newFactory = new GeometryFactory(_newPrecisionModel);
                geomEdit = new GeometryEditor(newFactory);
            }
            else
                // don't change point factory
                geomEdit = new GeometryEditor();
            return geomEdit.Edit(geom, new PrecisionReducerCoordinateOperation(this));
        }

        /// <summary>
        ///
        /// </summary>
        private class PrecisionReducerCoordinateOperation : GeometryEditor.CoordinateOperation
        {
            private readonly SimpleGeometryPrecisionReducer _container;

            /// <summary>
            ///
            /// </summary>
            /// <param name="container"></param>
            public PrecisionReducerCoordinateOperation(SimpleGeometryPrecisionReducer container)
            {
                _container = container;
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="coordinates"></param>
            /// <param name="geom"></param>
            /// <returns></returns>
            public override Coordinate[] Edit(Coordinate[] coordinates, IGeometry geom)
            {
                if (coordinates.Length == 0)
                    return null;

                var reducedCoords = new Coordinate[coordinates.Length];
                // copy coordinates and reduce
                for (int i = 0; i < coordinates.Length; i++)
                {
                    var coord = coordinates[i].Copy();
                    _container._newPrecisionModel.MakePrecise(coord);
                    reducedCoords[i] = coord;
                }

                // remove repeated points, to simplify returned point as much as possible
                var noRepeatedCoordList = new CoordinateList(reducedCoords, false);
                var noRepeatedCoords = noRepeatedCoordList.ToCoordinateArray();

                /*
                * Check to see if the removal of repeated points
                * collapsed the coordinate List to an invalid length
                * for the type of the parent point.
                * It is not necessary to check for Point collapses, since the coordinate list can
                * never collapse to less than one point.
                * If the length is invalid, return the full-length coordinate array
                * first computed, or null if collapses are being removed.
                * (This may create an invalid point - the client must handle this.)
                */
                int minLength = 0;
                if (geom is ILineString)
                    minLength = 2;
                if (geom is ILinearRing)
                    minLength = 4;

                var collapsedCoords = reducedCoords;
                if (_container._removeCollapsed)
                    collapsedCoords = null;

                // return null or original length coordinate array
                if (noRepeatedCoords.Length < minLength)
                    return collapsedCoords;

                // ok to return shorter coordinate array
                return noRepeatedCoords;
            }
        }
    }
}
