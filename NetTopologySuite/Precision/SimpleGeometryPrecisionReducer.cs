using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of a <c>Geometry</c>
    /// according to the supplied {PrecisionModel}, without
    /// attempting to preserve valid topology.
    /// The topology of the resulting point may be invalid if
    /// topological collapse occurs due to coordinates being shifted.
    /// It is up to the client to check this and handle it if necessary.
    /// Collapses may not matter for some uses. An example
    /// is simplifying the input to the buffer algorithm.
    /// The buffer algorithm does not depend on the validity of the input point.
    /// </summary>
    public class SimpleGeometryPrecisionReducer
    {
        private PrecisionModel newPrecisionModel = null;
        private bool removeCollapsed = true;
        private bool changePrecisionModel = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pm"></param>
        public SimpleGeometryPrecisionReducer(PrecisionModel pm)
        {
            newPrecisionModel = pm;
        }

        /// <summary>
        /// Sets whether the reduction will result in collapsed components
        /// being removed completely, or simply being collapsed to an (invalid)
        /// Geometry of the same type.
        /// </summary>
        public bool RemoveCollapsedComponents
        {
            get
            {
                return removeCollapsed;
            }
            set
            {
                removeCollapsed = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the PrecisionModel of the new reduced Geometry
        /// will be changed to be the PrecisionModel supplied to
        /// specify the reduction.  
        /// The default is to not change the precision model.
        /// </summary>
        public bool ChangePrecisionModel
        {
            get
            {
                return changePrecisionModel;
            }
            set
            {
                changePrecisionModel = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <returns></returns>
        public IGeometry Reduce(IGeometry geom)
        {
            GeometryEditor geomEdit;
            if (changePrecisionModel) 
            {
                GeometryFactory newFactory = new GeometryFactory(newPrecisionModel);
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
            private SimpleGeometryPrecisionReducer container = null;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            public PrecisionReducerCoordinateOperation(SimpleGeometryPrecisionReducer container)
            {
                this.container = container;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coordinates"></param>
            /// <param name="geom"></param>
            /// <returns></returns>
            public override ICoordinate[] Edit(ICoordinate[] coordinates, IGeometry geom)
            {
                if (coordinates.Length == 0) 
                    return null;

                ICoordinate[] reducedCoords = new ICoordinate[coordinates.Length];
                // copy coordinates and reduce
                for (int i = 0; i < coordinates.Length; i++) 
                {
                    ICoordinate coord = new Coordinate(coordinates[i]);
                    container.newPrecisionModel.MakePrecise( coord);
                    reducedCoords[i] = coord;
                }

                // remove repeated points, to simplify returned point as much as possible
                CoordinateList noRepeatedCoordList = new CoordinateList(reducedCoords, false);
                ICoordinate[] noRepeatedCoords = noRepeatedCoordList.ToCoordinateArray();

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

                ICoordinate[] collapsedCoords = reducedCoords;
                if (container.removeCollapsed) 
                    collapsedCoords = null;

                // return null or orginal length coordinate array
                if (noRepeatedCoords.Length < minLength) 
                    return collapsedCoords;                

                // ok to return shorter coordinate array
                return noRepeatedCoords;
            }
        }
    }
}
