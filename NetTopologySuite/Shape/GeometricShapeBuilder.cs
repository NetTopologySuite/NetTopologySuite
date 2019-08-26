using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape
{
    public abstract class GeometricShapeBuilder
    {
        private Envelope _extent = new Envelope(0, 1, 0, 1);
        protected GeometryFactory GeomFactory;

        protected GeometricShapeBuilder(GeometryFactory geomFactory)
        {
            GeomFactory = geomFactory;
        }

        public Envelope Extent
        {
            get => _extent;
            set => _extent = value;
        }

        public Coordinate Centre => _extent.Centre;

        public double Diameter => Math.Min(_extent.Height, _extent.Width);

        public double Radius => Diameter * 0.5;

        public LineSegment GetSquareBaseLine()
        {
            double radius = Radius;

            var centre = Centre;
            var p0 = new Coordinate(centre.X - radius, centre.Y - radius);
            var p1 = new Coordinate(centre.X + radius, centre.Y - radius);

            return new LineSegment(p0, p1);
        }

        public Envelope GetSquareExtent()
        {
            double radius = Radius;

            var centre = Centre;
            return new Envelope(centre.X - radius, centre.X + radius,
                    centre.Y - radius, centre.Y + radius);
        }

        /// <summary>
        /// Gets or sets the total number of points in the created <see cref="Geometry"/>.
        /// The created geometry will have no more than this number of points,
        /// unless more are needed to create a valid geometry.
        /// </summary>
        public int NumPoints { get; set; }

        public abstract Geometry GetGeometry();

        protected Coordinate CreateCoord(double x, double y)
        {
            var pt = new Coordinate(x, y);
            GeomFactory.PrecisionModel.MakePrecise(pt);
            return pt;
        }
    }
}