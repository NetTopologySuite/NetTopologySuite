using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Precision
{
    /// <summary>
    /// Reduces the precision of a <see cref="Geometry"/>
    /// according to the supplied <see cref="PrecisionModel"/>,
    /// ensuring that the result is valid (unless specified otherwise).
    /// <para/>
    /// By default the geometry precision model is not changed.
    /// This can be overridden by using <see cref="ChangePrecisionModel"/>.
    /// <h4>Topological Precision Reduction</h4>
    /// The default mode of operation ensures the reduced result is topologically valid
    /// (i.e. <see cref="Geometry.IsValid"/> is true).
    /// To ensure this polygonal geometry is reduced in a topologically valid fashion
    /// (technically, by using snap-rounding).
    /// Note that this may change polygonal geometry structure
    /// (e.g.two polygons separated by a distance below the specified precision
    /// will be merged into a single polygon).
    /// Duplicate vertices are removed.
    /// This mode is invoked by the static method <see cref="Reduce(Geometry, PrecisionModel)"/>.
    /// <para/>
    /// Normally, collapsed linear components(e.g.lines collapsing to a point)
    /// are not included in the result.
    /// This behavior can be changed
    /// by setting <see cref="RemoveCollapsedComponents"/> to <c>false</c>,
    /// or by using the static method <see cref="ReduceKeepCollapsed(Geometry, PrecisionModel)"/>.
    /// <para/>
    /// In general input must be valid geometry, or an <see cref="ArgumentException"/>
    /// will be thrown. However if the invalidity is "mild" or very small then it
    /// may be eliminated by precision reduction.
    /// <h4> Pointwise Precision Reduction</h4>
    /// Alternatively, geometry can be reduced pointwise by using {@link #setPointwise(boolean)}.
    /// Linear and point geometry are always reduced pointwise(i.e.without further change to
    /// topology or structure), since this does not change validity.
    /// Invalid inputs are allowed.
    /// Duplicate vertices are preserved.
    /// Collapsed components are always included in the result.
    /// The result geometry may be invalid.
    /// <para/>
    /// This mode is invoked by the static method <see cref="ReducePointwise(Geometry, PrecisionModel)"/>.
    /// </summary>
    public class GeometryPrecisionReducer
    {
        /// <summary>
        /// Reduces precision of a geometry, ensuring output geometry is valid.
        /// Collapsed linear and polygonal components are removed.
        /// The geometry precision model is not changed.
        /// Invalid input geometry may cause an error,
        /// unless the invalidity is below the scale of the precision reduction.
        /// </summary>
        /// <param name="g">The geometry to reduce</param>
        /// <param name="precModel">The precision model to use</param>
        /// <returns>The reduced geometry</returns>
        /// <exception cref="ArgumentException">Thrwon if the reduction fails due to invalid input geometry</exception>
        public static Geometry Reduce(Geometry g, PrecisionModel precModel)
        {
            var reducer = new GeometryPrecisionReducer(precModel);
            return reducer.Reduce(g);
        }

        /// <summary>
        /// Reduces precision of a geometry, ensuring output polygonal geometry is valid,
        /// and preserving collapsed linear elements.
        /// The geometry precision model is not changed.
        /// Invalid input geometry may cause an error,
        /// unless the invalidity is below the scale of the precision reduction.
        /// </summary>
        /// <param name="g">The geometry to reduce</param>
        /// <param name="precModel">The precision model to use</param>
        /// <returns>The reduced geometry</returns>
        /// <exception cref="ArgumentException">Thrwon if the reduction fails due to invalid input geometry</exception>
        public static Geometry ReduceKeepCollapsed(Geometry geom, PrecisionModel pm)
        {
            var reducer = new GeometryPrecisionReducer(pm) {
                RemoveCollapsedComponents = false
            };
            return reducer.Reduce(geom);
        }

        /// <summary>
        /// Reduce precision of a geometry in a pointwise way.
        /// All input geometry elements are preserved in the output,
        /// including invalid polygons and collapsed polygons and linestrings.
        /// The output may not be valid, due to collapse or self-intersection.
        /// The geometry precision model is not changed.
        /// Invalid input geometry is allowed.
        /// </summary>
        /// <param name="g">The geometry to reduce</param>
        /// <param name="precModel">The precision model to use</param>
        /// <returns>The reduced geometry</returns>
        public static Geometry ReducePointwise(Geometry g, PrecisionModel precModel)
        {
            var reducer = new GeometryPrecisionReducer(precModel);
            reducer.Pointwise = true;
            return reducer.Reduce(g);
        }

        private readonly PrecisionModel _targetPM;
        private bool _removeCollapsed = true;
        private bool _changePrecisionModel;
        private bool _isPointwise;

        public GeometryPrecisionReducer(PrecisionModel pm)
        {
            _targetPM = pm;
        }

        /// <summary>Gets or sets whether the reduction will result in collapsed components
        /// being removed completely, or simply being collapsed to an (invalid)
        /// Geometry of the same type.
        /// The default is to remove collapsed components.
        /// </summary>
        public bool RemoveCollapsedComponents
        {
            get => _removeCollapsed;
            set => _removeCollapsed = value;
        }

        /// <summary>
        /// Gets or sets whether the <see cref = "PrecisionModel"/> of the new reduced Geometry
        /// will be changed to be the <see cref="PrecisionModel"/> supplied to
        /// specify the precision reduction.
        /// <para/>
        /// The default is to <b>not</b> change the precision model
        /// </summary>
        public bool ChangePrecisionModel
        {
            get => _changePrecisionModel;
            set => _changePrecisionModel = value;
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
            get => _isPointwise;
            set => _isPointwise = value;
        }

        /// <summary>
        /// Reduces the precision of a geometry,
        /// according to the specified strategy of this reducer.
        /// </summary>
        /// <param name="geom">The geometry to reduce</param>
        /// <returns>The precision-reduced geometry</returns>
        /// <exception cref="ArgumentException">if the reduction fails
        /// due to input geometry is invalid.</exception>
        public Geometry Reduce(Geometry geom)
        {
            Geometry reduced;
            if (_isPointwise)
                reduced = PointwisePrecisionReducerTransformer.Reduce(geom, _targetPM);
            else
                reduced = PrecisionReducerTransformer.Reduce(geom, _targetPM, _removeCollapsed);

            // TODO: incorporate this in the Transformer above
            if (_changePrecisionModel)
            {
                return ChangePM(reduced, _targetPM);
            }
            return reduced;
        }

        /// <summary>
        /// Duplicates a geometry to one that uses a different PrecisionModel,
        /// without changing any coordinate values.
        /// </summary>
        /// <param name="geom">The geometry to duplicate</param>
        /// <param name="pm">The precision model to use</param>
        /// <returns>The geometry value with a new precision model</returns>
        private static Geometry ChangePM(Geometry geom, PrecisionModel pm)
        {
            var geomEditor = CreateEditor(geom.Factory, pm);
            // this operation changes the PM for the entire geometry tree
            return geomEditor.Edit(geom, new GeometryEditor.NoOpGeometryOperation());
        }

        private static GeometryEditor CreateEditor(GeometryFactory geomFactory, PrecisionModel newPrecModel)
        {
            // no need to change if precision model is the same
            if (geomFactory.PrecisionModel == newPrecModel)
                return new GeometryEditor();

            // otherwise create a geometry editor which changes PrecisionModel
            var newFactory = CreateFactory(geomFactory, newPrecModel);
            var geomEdit = new GeometryEditor(newFactory);
            return geomEdit;
        }

        private static GeometryFactory CreateFactory(GeometryFactory inputFactory, PrecisionModel pm)
        {
            var newFactory
            = new GeometryFactory(pm,
                    inputFactory.SRID,
                    inputFactory.CoordinateSequenceFactory);
            return newFactory;
        }
    }
}
