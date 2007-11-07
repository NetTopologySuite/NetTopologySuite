using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    /// <summary>
    /// Simplifies a <c>Geometry</c> using the standard Douglas-Peucker algorithm.
    /// Ensures that any polygonal geometries returned are valid.
    /// Simple lines are not guaranteed to remain simple after simplification.
    /// Note that in general D-P does not preserve topology -
    /// e.g. polygons can be split, collapse to lines or disappear
    /// holes can be created or disappear,
    /// and lines can cross.
    /// To simplify point while preserving topology use TopologySafeSimplifier.
    /// (However, using D-P is significantly faster).
    /// </summary>
    public class DouglasPeuckerSimplifier
    {
        public static IGeometry Simplify(IGeometry geom, Double distanceTolerance)
        {
            DouglasPeuckerSimplifier tss = new DouglasPeuckerSimplifier(geom);
            tss.DistanceTolerance = distanceTolerance;
            return tss.GetResultGeometry();
        }

        private IGeometry inputGeom;
        private Double distanceTolerance;

        public DouglasPeuckerSimplifier(IGeometry inputGeom)
        {
            this.inputGeom = inputGeom;
        }

        public Double DistanceTolerance
        {
            get { return distanceTolerance; }
            set { distanceTolerance = value; }
        }

        public IGeometry GetResultGeometry()
        {
            return (new DPTransformer(this)).Transform(inputGeom);
        }

        private class DPTransformer : GeometryTransformer
        {
            private DouglasPeuckerSimplifier container = null;

            public DPTransformer(DouglasPeuckerSimplifier container)
            {
                this.container = container;
            }

            protected override ICoordinateSequence TransformCoordinates(ICoordinateSequence coords, IGeometry parent)
            {
                ICoordinate[] inputPts = coords.ToCoordinateArray();
                ICoordinate[] newPts = DouglasPeuckerLineSimplifier.Simplify(inputPts, container.DistanceTolerance);
                return factory.CoordinateSequenceFactory.Create(newPts);
            }

            protected override IGeometry TransformPolygon(IPolygon geom, IGeometry parent)
            {
                IGeometry roughGeom = base.TransformPolygon(geom, parent);
                // don't try and correct if the parent is going to do this
                if (parent is IMultiPolygon)
                {
                    return roughGeom;
                }
                return CreateValidArea(roughGeom);
            }

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
            /// <param name="roughAreaGeom">An area point possibly containing self-intersections.</param>
            /// <returns>A valid area point.</returns>
            private IGeometry CreateValidArea(IGeometry roughAreaGeom)
            {
                return roughAreaGeom.Buffer(0.0);
            }
        }
    }
}