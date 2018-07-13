using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Shape.Random
{
    /// <summary>
    /// Creates random point sets contained in a
    /// region defined by either a rectangular or a polygonal extent.
    /// </summary>
    /// <author>mbdavis</author>
    public class RandomPointsBuilder : GeometricShapeBuilder
    {
        protected static readonly System.Random Rnd = new System.Random();

        private IGeometry _maskPoly;
        private IPointOnGeometryLocator _extentLocator;

        /// <summary>
        /// Create a shape factory which will create shapes using the default
        /// <see cref="IGeometryFactory"/>.
        /// </summary>
        public RandomPointsBuilder()
            : this(new GeometryFactory())
        {
        }

        /// <summary>
        /// Create a shape factory which will create shapes using the given
        /// <see cref="IGeometryFactory"/>
        /// </summary>
        /// <param name="geomFact">The factory to use</param>
        public RandomPointsBuilder(IGeometryFactory geomFact)
            : base(geomFact)
        {
        }

        /// <summary>
        /// Sets a polygonal mask.
        /// </summary>
        /// <exception cref="ArgumentException">if the mask is not polygonal</exception>
        public void SetExtent(IGeometry mask)
        {
            if (!(mask is IPolygonal))
                throw new ArgumentException("Only polygonal extents are supported");

            _maskPoly = mask;
            Extent = mask.EnvelopeInternal;
            _extentLocator = new IndexedPointInAreaLocator(mask);
        }

        public override IGeometry GetGeometry()
        {
            var pts = new Coordinate[NumPoints];
            int i = 0;
            while (i < NumPoints)
            {
                var p = CreateRandomCoord(Extent);
                if (_extentLocator != null && !IsInExtent(p))
                    continue;
                pts[i++] = p;
            }
            return GeomFactory.CreateMultiPointFromCoords(pts);
        }

        protected bool IsInExtent(Coordinate p)
        {
            if (_extentLocator != null)
                return _extentLocator.Locate(p) != Location.Exterior;
            return Extent.Contains(p);
        }

        /*
         * Same functionality in base class
         *
        protected override Coordinate CreateCoord(double x, double y)
        {
            Coordinate pt = new Coordinate(x, y);
            geomFactory.getPrecisionModel().makePrecise(pt);
            return pt;
        }
        */

        protected Coordinate CreateRandomCoord(Envelope env)
        {
            double x = env.MinX + env.Width * Rnd.NextDouble();
            double y = env.MinY + env.Height * Rnd.NextDouble();

            return CreateCoord(x, y);
        }
    }
}