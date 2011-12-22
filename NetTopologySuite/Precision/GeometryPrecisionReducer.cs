using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of a <see cref="IGeometry"/>
    /// according to the supplied <see cref="IPrecisionModel"/>,
    /// ensuring that the result is topologically valid.
    /// </summary>
    public class GeometryPrecisionReducer
    {
        /// <summary>
        /// Convenience method for doing precision reduction
        /// on a single geometry,
        /// with collapses removed
        /// and keeping the geometry precision model the same,
        /// and preserving polygonal topology.
        /// </summary>
        /// <param name="g">The geometry to reduce</param>
        /// <param name="precModel">The precision model to use</param>
        /// <returns>The reduced geometry</returns>
        public static IGeometry Reduce(IGeometry g, IPrecisionModel precModel)
        {
            var reducer = new GeometryPrecisionReducer(precModel);
            return reducer.Reduce(g);
        }

        /// <summary>
        /// Convenience method for doing pointwise precision reduction
        /// on a single geometry,
        /// with collapses removed
        /// and keeping the geometry precision model the same,
        /// but NOT preserving valid polygonal topology.
        /// </summary>
        /// <param name="g">The geometry to reduce</param>
        /// <param name="precModel">The precision model to use</param>
        /// <returns>The reduced geometry</returns>
        public static IGeometry ReducePointwise(IGeometry g, IPrecisionModel precModel)
        {
            var reducer = new GeometryPrecisionReducer(precModel);
            reducer.Pointwise = true;
            return reducer.Reduce(g);
        }

        private readonly IPrecisionModel _targetPrecModel;
        private bool _removeCollapsed = true;
        private bool _changePrecisionModel;
        private bool _isPointwise;

        public GeometryPrecisionReducer(IPrecisionModel pm)
        {
            _targetPrecModel = pm;
        }

        /// <summary>Gets or sets whether the reduction will result in collapsed components
        /// being removed completely, or simply being collapsed to an (invalid)
        /// Geometry of the same type.
        /// The default is to remove collapsed components.
        /// </summary>
        public bool RemoveCollapsedComponents
        {
            get { return _removeCollapsed; }
            set { _removeCollapsed = value; }
        }

        /// <summary>
        /// /// Gets or sets whether the <see cref = "IPrecisionModel"/> of the new reduced Geometry
        /// will be changed to be the <see cref="IPrecisionModel"/> supplied to
        /// specify the precision reduction.
        /// <para/>
        /// The default is to <b>not</b> change the precision model
        /// </summary>
        public bool ChangePrecisionModel
        {
            get { return _changePrecisionModel; }
            set { _changePrecisionModel = value; }
        }

        /// <summary>
        /// Gets or sets whether the precision reduction will be done
        /// in pointwise fashion only.
        /// Pointwise precision reduction reduces the precision
        /// of the individual coordinates only, but does
        /// not attempt to recreate valid topology.
        /// This is only relevant for geometries containing polygonal components.
        /// </summary>
        public bool Pointwise
        {
            get { return _isPointwise; }
            set { _isPointwise = value; }
        }

        public IGeometry Reduce(IGeometry geom)
        {
            var reducePointwise = ReducePointwise(geom);
            if (_isPointwise)
                return reducePointwise;

            //TODO: handle GeometryCollections containing polys
            if (!(reducePointwise is IPolygonal))
                return reducePointwise;

            // Geometry is polygonal - test if topology needs to be fixed
            if (reducePointwise.IsValid) return reducePointwise;

            // hack to fix topology.
            // TODO: implement snap-rounding and use that.
            return FixPolygonalTopology(reducePointwise);
        }

        private IGeometry ReducePointwise(IGeometry geom)
        {
            GeometryEditor geomEdit;
            if (_changePrecisionModel)
            {
                var newFactory = CreateFactory(geom.Factory, _targetPrecModel);
                geomEdit = new GeometryEditor(newFactory);
            }
            else
                // don't change geometry factory
                geomEdit = new GeometryEditor();

            /**
             * For polygonal geometries, collapses are always removed, in order
             * to produce correct topology
             */
            bool finalRemoveCollapsed = _removeCollapsed;
            if (geom.Dimension >= Dimension.Surface)
                finalRemoveCollapsed = true;

            var reduceGeom = geomEdit.Edit(geom,
                    new PrecisionReducerCoordinateOperation(_targetPrecModel, finalRemoveCollapsed));

            return reduceGeom;
        }

        private IGeometry FixPolygonalTopology(IGeometry geom)
        {
            /**
             * If precision model was *not* changed, need to flip
             * geometry to targetPM, buffer in that model, then flip back
             */
            var geomToBuffer = geom;
            if (!_changePrecisionModel)
            {
                geomToBuffer = ChangePrecModel(geom, _targetPrecModel);
            }

            var bufGeom = geomToBuffer.Buffer(0);

            var finalGeom = bufGeom;
            if (!_changePrecisionModel)
            {
                var originalPrecModel = geom.Factory.PrecisionModel;
                finalGeom = ChangePrecModel(bufGeom, originalPrecModel);
            }
            return finalGeom;
        }

        private IGeometry ChangePrecModel(IGeometry geom, IPrecisionModel pm)
        {
            var geomEditor = CreateEditor(geom.Factory, pm);
            return geomEditor.Edit(geom, new GeometryEditor.NoOpGeometryOperation());
        }

        private GeometryEditor CreateEditor(IGeometryFactory geomFactory, IPrecisionModel pm)
        {
            if (geomFactory.PrecisionModel == pm)
                return new GeometryEditor();
            // otherwise create a geometry editor which changes PrecisionModel
            var newFactory = CreateFactory(geomFactory, _targetPrecModel);
            var geomEdit = new GeometryEditor(newFactory);
            return geomEdit;
        }

        private static IGeometryFactory CreateFactory(IGeometryFactory inputFactory, IPrecisionModel pm)
        {
            var newFactory
            = new GeometryFactory(pm,
                    inputFactory.SRID,
                    inputFactory.CoordinateSequenceFactory);
            return newFactory;
        }
    }
}