using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies an <see cref="IGeometry{TCoordinate}"/> using the standard Douglas-Peucker algorithm.
    /// Ensures that any polygonal geometries returned are valid.
    /// Simple lines are not guaranteed to remain simple after simplification.
    /// Note that in general D-P does not preserve topology -
    /// e.g. polygons can be split, collapse to lines or disappear
    /// holes can be created or disappear,
    /// and lines can cross.
    /// To simplify point while preserving topology use TopologySafeSimplifier.
    /// (However, using D-P is significantly faster).
    /// </summary>
    public class DouglasPeuckerSimplifier<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static IGeometry<TCoordinate> Simplify(IGeometry<TCoordinate> geom, Double distanceTolerance)
        {
            DouglasPeuckerSimplifier<TCoordinate> tss = new DouglasPeuckerSimplifier<TCoordinate>(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private readonly IGeometry<TCoordinate> _inputGeometry;
        private Double distanceTolerance;

        public DouglasPeuckerSimplifier(IGeometry<TCoordinate> inputGeom)
        {
            _inputGeometry = inputGeom;
        }

        public Double DistanceTolerance
        {
            get { return distanceTolerance; }
            set { distanceTolerance = value; }
        }

        public IGeometry<TCoordinate> GetResultGeometry()
        {
            return (new DPTransformer(this)).Transform(_inputGeometry);
        }

        private class DPTransformer : GeometryTransformer<TCoordinate>
        {
            private DouglasPeuckerSimplifier<TCoordinate> container = null;

            public DPTransformer(DouglasPeuckerSimplifier<TCoordinate> container)
            {
                this.container = container;
            }

            protected override ICoordinateSequence<TCoordinate> TransformCoordinates(ICoordinateSequence<TCoordinate> coords, IGeometry<TCoordinate> parent)
            {
                return DouglasPeuckerLineSimplifier<TCoordinate>.Simplify(
                    coords, container.DistanceTolerance);
            }

            protected override IGeometry<TCoordinate> TransformPolygon(IPolygon<TCoordinate> geom, IGeometry<TCoordinate> parent)
            {
                IGeometry<TCoordinate> roughGeom = base.TransformPolygon(geom, parent);

                // don't try and correct if the parent is going to do this
                if (parent is IMultiPolygon<TCoordinate>)
                {
                    return roughGeom;
                }

                return createValidArea(roughGeom);
            }

            protected override IGeometry<TCoordinate> TransformMultiPolygon(IMultiPolygon<TCoordinate> geom, IGeometry<TCoordinate> parent)
            {
                IGeometry<TCoordinate> roughGeom = base.TransformMultiPolygon(geom, parent);
                return createValidArea(roughGeom);
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
            /// <param name="roughAreaGeom">An area point possibly containing self-intersections.</param>
            /// <returns>A valid area point.</returns>
            private static IGeometry<TCoordinate> createValidArea(IGeometry<TCoordinate> roughAreaGeom)
            {
                if (roughAreaGeom is ISpatialOperator<TCoordinate>)
                {
                    ISpatialOperator<TCoordinate> area = roughAreaGeom as ISpatialOperator<TCoordinate>;
                    return area.Buffer(0.0);
                }

                return null;
            }
        }
    }
}