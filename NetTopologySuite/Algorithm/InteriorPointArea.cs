using System;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Computes a point in the interior of an area point.
    /// </summary>
    /// <remarks>
    /// Algorithm:
    /// Find the intersections between the point
    /// and the horizontal bisector of the area's envelope
    /// Pick the midpoint of the largest intersection (the intersections
    /// will be lines and points)
    /// Note: If a fixed precision model is used,
    /// in some cases this method may return a point
    /// which does not lie in the interior.
    /// </remarks>
    public class InteriorPointArea<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly IGeometryFactory<TCoordinate> _factory;
        private TCoordinate _interiorPoint;
        private Double _maxWidth;

        public InteriorPointArea(IGeometry<TCoordinate> g)
        {
            _factory = g.Factory;
            Add(g);
        }

        public TCoordinate InteriorPoint
        {
            get { return _interiorPoint; }
        }

        private static Double Avg(Double a, Double b)
        {
            return (a + b)/2.0;
        }

        /// <summary> 
        /// Tests the interior vertices (if any)
        /// defined by a linear Geometry for the best inside point.
        /// If a Geometry is not of dimension 1 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygon<TCoordinate>)
            {
                AddPolygon(geom);
            }
            else if (geom is IGeometryCollection<TCoordinate>)
            {
                IGeometryCollection<TCoordinate> gc = geom as IGeometryCollection<TCoordinate>;

                foreach (IGeometry<TCoordinate> geometry in gc)
                {
                    Add(geometry);
                }
            }
        }

        /// <summary> 
        /// Finds a reasonable point at which to label a Geometry.
        /// </summary>
        /// <param name="geometry">The point to analyze.</param>
        /// <returns> 
        /// The midpoint of the largest intersection between the point and
        /// a line halfway down its envelope.
        /// </returns>
        public void AddPolygon(IGeometry<TCoordinate> geometry)
        {
            ILineString<TCoordinate> bisector = HorizontalBisector(geometry);

            ISpatialOperator<TCoordinate> intersector = bisector;
            Debug.Assert(intersector != null);
            IGeometry<TCoordinate> intersections = intersector.Intersection(geometry);
            IGeometry<TCoordinate> widestIntersection = WidestGeometry(intersections);

            Double width = widestIntersection.Extents.GetSize(Ordinates.X, Ordinates.Y);

            if (Coordinates<TCoordinate>.IsEmpty(_interiorPoint) || width > _maxWidth)
            {
                _interiorPoint = Center(widestIntersection.Extents);
                _maxWidth = width;
            }
        }

        /// <summary> 
        /// Returns the center point of the <see cref="IExtents{TCoordinate}"/>.
        /// </summary>
        /// <param name="envelope">The extents to analyze.</param>
        /// <returns> The center of the extents.</returns>
        public TCoordinate Center(IExtents<TCoordinate> envelope)
        {
            return _factory.CoordinateFactory.Create(
                Avg(envelope.GetMin(Ordinates.X), envelope.GetMax(Ordinates.X)),
                Avg(envelope.GetMin(Ordinates.Y), envelope.GetMax(Ordinates.Y)));
        }

        /// <returns>
        /// If point is a collection, the widest sub-point; otherwise,
        /// the point itself.
        /// </returns>
        protected static IGeometry<TCoordinate> WidestGeometry(IGeometry<TCoordinate> geometry)
        {
            if (!(geometry is IGeometryCollection<TCoordinate>))
            {
                return geometry;
            }

            return widestGeometry(geometry as IGeometryCollection<TCoordinate>);
        }

        protected ILineString<TCoordinate> HorizontalBisector(IGeometry<TCoordinate> geometry)
        {
            IExtents<TCoordinate> extents = geometry.Extents;

            // Assert: for areas, minx <> maxx
            Double avgY = Avg(extents.GetMin(Ordinates.Y), extents.GetMax(Ordinates.Y));
            return _factory.CreateLineString(
                _factory.CoordinateFactory.Create(extents.GetMin(Ordinates.X), avgY),
                _factory.CoordinateFactory.Create(extents.GetMax(Ordinates.X), avgY));
        }

        private static IGeometry<TCoordinate> widestGeometry(IGeometryCollection<TCoordinate> gc)
        {
            if (gc.IsEmpty)
            {
                return gc;
            }

            IGeometry<TCoordinate> widestGeometry = gc[0];

            for (Int32 i = 1; i < gc.Count; i++) // Start at 1        
            {
                if (gc[i].Extents.GetSize(Ordinates.X, Ordinates.Y) >
                    widestGeometry.Extents.GetSize(Ordinates.X, Ordinates.Y))
                {
                    widestGeometry = gc[i];
                }
            }

            return widestGeometry;
        }
    }
}