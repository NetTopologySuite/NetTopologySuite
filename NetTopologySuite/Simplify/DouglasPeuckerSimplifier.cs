using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a <c>Geometry</c> using the standard Douglas-Peucker algorithm.
    /// </summary>
    /// <remarks>
    /// Ensures that any polygonal geometries returned are valid.
    /// Simple lines are not guaranteed to remain simple after simplification.
    /// Note that in general D-P does not preserve topology -
    /// e.g. polygons can be split, collapse to lines or disappear
    /// holes can be created or disappear,
    /// and lines can cross.
    /// To simplify point while preserving topology use TopologySafeSimplifier.
    /// (However, using D-P is significantly faster).
    /// </remarks>
    public class DouglasPeuckerSimplifier
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        public static IGeometry Simplify(IGeometry geom, double distanceTolerance)
        {
            DouglasPeuckerSimplifier tss = new DouglasPeuckerSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private readonly IGeometry _inputGeom;
        private double _distanceTolerance;
        private bool _isEnsureValidTopology = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputGeom"></param>
        public DouglasPeuckerSimplifier(IGeometry inputGeom)
        {
            _inputGeom = inputGeom;
        }


        /// <summary>
        /// Gets/Sets the distance tolerance for the simplification.
        /// </summary>
        /// <remarks>
        /// All vertices in the simplified geometry will be within this
        /// distance of the original geometry.
        /// The tolerance value must be non-negative. 
        /// </remarks>
        public double DistanceTolerance
        {
            get { return _distanceTolerance; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "value must be non-negative");
                _distanceTolerance = value;
            }
        }

        /// <summary>
        /// Controls whether simplified polygons will be "fixed"
        /// to have valid topology.
        /// </summary>
        /// <remarks>
        /// The caller may choose to disable this because:
        /// <list type="Bullet">
        /// <item>valid topology is not required</item>
        /// <item>fixing topology is a relative expensive operation</item>
        /// <item>in some pathological cases the topology fixing operation may either fail or run for too long</item>
        /// </list>
        /// The default is to fix polygon topology.
        /// </remarks>
        public bool EnsureValidTopology 
        {
            get { return _isEnsureValidTopology; }
            set { _isEnsureValidTopology = value; } 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IGeometry GetResultGeometry()
        {
            return (new DPTransformer(this, EnsureValidTopology)).Transform(_inputGeom);
        }

        /// <summary>
        /// 
        /// </summary>
// ReSharper disable InconsistentNaming
        private class DPTransformer : GeometryTransformer
// ReSharper restore InconsistentNaming
        {
            private readonly DouglasPeuckerSimplifier _container;
            private bool _ensureValidTopology;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="container"></param>
            /// <param name="ensureValidTopology"></param>
            public DPTransformer(DouglasPeuckerSimplifier container, bool ensureValidTopology)
            {
                _container = container;
                _ensureValidTopology = ensureValidTopology;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="coords"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
            {
                Coordinate[] inputPts = coords.ToCoordinateArray();
                Coordinate[] newPts = DouglasPeuckerLineSimplifier.Simplify(inputPts, _container.DistanceTolerance);
                return Factory.CoordinateSequenceFactory.Create(newPts);
            }

            /// <summary>
            /// Simplifies a polygon, fixing it if required.
            /// </summary>
            /// <param name="geom"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override IGeometry TransformPolygon(IPolygon geom, IGeometry parent)
            {
                IGeometry rawGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is IMultiPolygon) 
                    return rawGeom;
                return CreateValidArea(rawGeom);
            }

            ///<summary>
            /// Simplifies a LinearRing.  If the simplification results in a degenerate ring, remove the component.
            ///</summary>
            /// <returns>null if the simplification results in a degenerate ring</returns>
            protected override IGeometry TransformLinearRing(ILinearRing geom, IGeometry parent) 
            {
  	            Boolean removeDegenerateRings = parent is IPolygon;
  	            IGeometry simpResult = base.TransformLinearRing(geom, parent);
  	            
                if (removeDegenerateRings && ! (simpResult is ILinearRing))
  		            return null;
  	            return simpResult;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="geom"></param>
            /// <param name="parent"></param>
            /// <returns></returns>
            protected override IGeometry TransformMultiPolygon(IMultiPolygon geom, IGeometry parent)
            {
                IGeometry roughGeom = base.TransformMultiPolygon(geom, parent);
                return CreateValidArea(roughGeom);
            }

            /// <summary>
            /// Creates a valid area point from one that possibly has
            /// bad topology (i.e. self-intersections).
            /// Since buffer can handle invalid topology, but always returns
            /// valid point, constructing a 0-width buffer "corrects" the
            /// topology.
            /// Note this only works for area geometries, since buffer always returns
            /// areas.  This also may return empty geometries, if the input
            /// has no actual area.
            /// </summary>
            /// <param name="rawAreaGeom">An area point possibly containing self-intersections.</param>
            /// <returns>A valid area point.</returns>
            private IGeometry CreateValidArea(IGeometry rawAreaGeom)
            {
                if (_ensureValidTopology)
                    return rawAreaGeom.Buffer(0.0);
                return rawAreaGeom;
            }
        }
    }
}
