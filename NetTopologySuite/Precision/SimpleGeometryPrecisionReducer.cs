using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of a <see cref="Geometry{TCoordinate}"/>
    /// according to the supplied <see cref="IPrecisionModel"/>, without
    /// attempting to preserve valid topology.
    /// </summary>
    /// <remarks>
    /// The topology of the resulting point may be invalid if
    /// topological collapse occurs due to coordinates being shifted.
    /// It is up to the client to check this and handle it if necessary.
    /// Collapses may not matter for some uses. An example
    /// is simplifying the input to the buffer algorithm.
    /// The buffer algorithm does not depend on the validity of the input point.
    /// </remarks>
    public class SimpleGeometryPrecisionReducer<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IPrecisionModel<TCoordinate> _newPrecisionModel = null;
        private Boolean _removeCollapsed = true;
        private Boolean _changePrecisionModel = false;

        public SimpleGeometryPrecisionReducer(IPrecisionModel<TCoordinate> pm)
        {
            _newPrecisionModel = pm;
        }

        /// <summary>
        /// Sets whether the reduction will result in collapsed components
        /// being removed completely, or simply being collapsed to an (invalid)
        /// Geometry of the same type.
        /// </summary>
        public Boolean RemoveCollapsedComponents
        {
            get { return _removeCollapsed; }
            set { _removeCollapsed = value; }
        }

        /// <summary>
        /// Gets or sets whether the PrecisionModel of the new reduced Geometry
        /// will be changed to be the PrecisionModel supplied to
        /// specify the reduction.  
        /// The default is to not change the precision model.
        /// </summary>
        public Boolean ChangePrecisionModel
        {
            get { return _changePrecisionModel; }
            set { _changePrecisionModel = value; }
        }

        public IGeometry<TCoordinate> Reduce(IGeometry<TCoordinate> geom)
        {
            GeometryEditor<TCoordinate> geometryEditor;

            if (_changePrecisionModel)
            {
                GeometryFactory<TCoordinate> newFactory 
                    = new GeometryFactory<TCoordinate>(_newPrecisionModel);

                geometryEditor = new GeometryEditor<TCoordinate>(newFactory);
            }
            else
            {
                // don't change point factory
                geometryEditor = new GeometryEditor<TCoordinate>();
            }

            return geometryEditor.Edit(geom, new PrecisionReducerCoordinateOperation<TCoordinate>(this));
        }

        private class PrecisionReducerCoordinateOperation<TCoordinate> : GeometryEditor<TCoordinate>.CoordinateOperation
            where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                                IComputable<TCoordinate>, IConvertible
        {
            private readonly SimpleGeometryPrecisionReducer<TCoordinate> _container = null;

            public PrecisionReducerCoordinateOperation(SimpleGeometryPrecisionReducer<TCoordinate> container)
            {
                _container = container;
            }

            public override IEnumerable<TCoordinate> Edit(IEnumerable<TCoordinate> coordinates, IGeometry<TCoordinate> geom)
            {
                if (!Slice.CountGreaterThan(0, coordinates))
                {
                    return null;
                }

                TCoordinate[] reducedCoords = new TCoordinate[coordinates.Length];

                // copy coordinates and reduce
                for (Int32 i = 0; i < coordinates.Length; i++)
                {
                    TCoordinate coord = new Coordinate(coordinates[i]);
                    _container._newPrecisionModel.MakePrecise(coord);
                    reducedCoords[i] = coord;
                }

                // remove repeated points, to simplify returned point as much as possible
                CoordinateList noRepeatedCoordList = new CoordinateList(reducedCoords, false);
                TCoordinate[] noRepeatedCoords = noRepeatedCoordList.ToCoordinateArray();

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
                Int32 minLength = 0;

                if (geom is ILineString)
                {
                    minLength = 2;
                }
                if (geom is ILinearRing)
                {
                    minLength = 4;
                }

                ICoordinate[] collapsedCoords = reducedCoords;

                if (_container._removeCollapsed)
                {
                    collapsedCoords = null;
                }

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
}
