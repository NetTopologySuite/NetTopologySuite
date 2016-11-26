﻿using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape
{
    public abstract class GeometricShapeBuilder
    {
        protected IGeometryFactory GeomFactory;

        protected GeometricShapeBuilder(IGeometryFactory geomFactory)
        {
            GeomFactory = geomFactory;
        }

        public Envelope Extent { get; set; } = new Envelope(0, 1, 0, 1);

        public Coordinate Centre => Extent.Centre;

        public double Diameter => Math.Min(Extent.Height, Extent.Width);

        public double Radius => Diameter*0.5;

        /// <summary>
        ///     Gets or sets the total number of points in the created <see cref="IGeometry" />.
        ///     The created geometry will have no more than this number of points,
        ///     unless more are needed to create a valid geometry.
        /// </summary>
        public int NumPoints { get; set; }

        public LineSegment GetSquareBaseLine()
        {
            var radius = Radius;

            var centre = Centre;
            var p0 = new Coordinate(centre.X - radius, centre.Y - radius);
            var p1 = new Coordinate(centre.X + radius, centre.Y - radius);

            return new LineSegment(p0, p1);
        }

        public Envelope GetSquareExtent()
        {
            var radius = Radius;

            var centre = Centre;
            return new Envelope(centre.X - radius, centre.X + radius,
                centre.Y - radius, centre.Y + radius);
        }

        public abstract IGeometry GetGeometry();

        protected Coordinate CreateCoord(double x, double y)
        {
            var pt = new Coordinate(x, y);
            GeomFactory.PrecisionModel.MakePrecise(pt);
            return pt;
        }
    }
}