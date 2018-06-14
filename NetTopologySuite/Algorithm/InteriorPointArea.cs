using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Algorithm
{
    /// <summary>
    /// Computes a point in the interior of an areal geometry.
    /// </summary>
    /// <remarks>
    /// <h2>Algorithm:</h2>
    /// <list type="Bullet">
    /// <item>Find a Y value which is close to the centre of
    /// the geometry's vertical extent but is different
    /// to any of it's Y ordinates.</item>
    /// <item>Create a horizontal bisector line using the Y value
    /// and the geometry's horizontal extent</item>
    /// <item>Find the intersection between the geometry
    /// and the horizontal bisector line.
    /// The intersection is a collection of lines and points.</item>
    /// <item>Pick the midpoint of the largest intersection geometry</item>
    /// </list>
    /// <h3>KNOWN BUGS</h3>
    /// <list type="Bullet">
    /// <item>If a fixed precision model is used,
    /// in some cases this method may return a point
    /// which does not lie in the interior.</item></list></remarks>
    public class InteriorPointArea
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double Avg(double a, double b)
        {
            return (a + b)/2.0;
        }

        private readonly IGeometryFactory _factory;
        private Coordinate _interiorPoint;
        private double _maxWidth;

        /// <summary>
        /// Creates a new interior point finder
        /// for an areal geometry.
        /// </summary>
        /// <param name="g">An areal geometry</param>
        public InteriorPointArea(IGeometry g)
        {
            _factory = g.Factory;
            Add(g);
        }

        /// <summary>
        /// Gets the computed interior point.
        /// </summary>
        public Coordinate InteriorPoint => _interiorPoint;

        /// <summary>
        /// Tests the interior vertices (if any)
        /// defined by an areal Geometry for the best inside point.
        /// If a component Geometry is not of dimension 2 it is not tested.
        /// </summary>
        /// <param name="geom">The point to add.</param>
        private void Add(IGeometry geom)
        {
            if (geom is Polygon)
                AddPolygon(geom);
            else if (geom is IGeometryCollection)
            {
                var gc = (IGeometryCollection) geom;
                foreach (var geometry in gc.Geometries)
                    Add(geometry);
            }
        }

        /// <summary>
        /// Finds an interior point of a Polygon.
        /// </summary>
        /// <param name="geometry">The geometry to analyze.</param>
        private void AddPolygon(IGeometry geometry)
        {
            if (geometry.IsEmpty)
                return;

            Coordinate intPt;
            double width;

            var bisector = HorizontalBisector(geometry);
            if (bisector.Length == 0.0)
            {
                width = 0;
                intPt = bisector.Coordinate;
            }
            else
            {
                var intersections = bisector.Intersection(geometry);
                var widestIntersection = WidestGeometry(intersections);
                width = widestIntersection.EnvelopeInternal.Width;
                intPt = Centre(widestIntersection.EnvelopeInternal);
            }
            if (_interiorPoint == null || width > _maxWidth)
            {
                _interiorPoint = intPt;
                _maxWidth = width;
            }
        }

        /// <returns>
        /// If point is a collection, the widest sub-point; otherwise,
        /// the point itself.
        /// </returns>
        private static IGeometry WidestGeometry(IGeometry geometry)
        {
            if (!(geometry is IGeometryCollection))
                return geometry;
            return WidestGeometry((IGeometryCollection) geometry);
        }

        private static IGeometry WidestGeometry(IGeometryCollection gc)
        {
            if (gc.IsEmpty)
                return gc;

            var widestGeometry = gc.GetGeometryN(0);
            // scan remaining geom components to see if any are wider
            for (int i = 1; i < gc.NumGeometries; i++) //Start at 1
                if (gc.GetGeometryN(i).EnvelopeInternal.Width > widestGeometry.EnvelopeInternal.Width)
                    widestGeometry = gc.GetGeometryN(i);
            return widestGeometry;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected ILineString HorizontalBisector(IGeometry geometry)
        {
            var envelope = geometry.EnvelopeInternal;

            /**
             * Original algorithm.  Fails when geometry contains a horizontal
             * segment at the Y midpoint.
             */
            // Assert: for areas, minx <> maxx
            //double avgY = Avg(envelope.MinY, envelope.MaxY);
            double bisectY = SafeBisectorFinder.GetBisectorY((IPolygon) geometry);

            return _factory.CreateLineString(
                new[] {new Coordinate(envelope.MinX, bisectY), new Coordinate(envelope.MaxX, bisectY)});
        }

        /// <summary>
        /// Returns the centre point of the envelope.
        /// </summary>
        /// <param name="envelope">The envelope to analyze.</param>
        /// <returns> The centre of the envelope.</returns>
        public static Coordinate Centre(Envelope envelope)
        {
            return new Coordinate(Avg(envelope.MinX, envelope.MaxX), Avg(envelope.MinY, envelope.MaxY));
        }

        /// <summary>
        /// Finds a safe bisector Y ordinate
        /// by projecting to the Y axis
        /// and finding the Y-ordinate interval
        /// which contains the centre of the Y extent.
        /// The centre of this interval is returned as the bisector Y-ordinate.
        /// </summary>
        /// <author>Martin Davis</author>
        private class SafeBisectorFinder
        {
            public static double GetBisectorY(IPolygon poly)
            {
                var finder = new SafeBisectorFinder(poly);
                return finder.GetBisectorY();
            }

            private readonly IPolygon _poly;

            private readonly double _centreY;
            private double _hiY;// = double.MaxValue;
            private double _loY;// = -double.MaxValue;

            private SafeBisectorFinder(IPolygon poly)
            {
                _poly = poly;

                // initialize using extremal values
                var env = poly.EnvelopeInternal;
                _hiY = env.MaxY;
                _loY = env.MinY;
                _centreY = Avg(_loY, _hiY);
            }

            private double GetBisectorY()
            {
                Process(_poly.ExteriorRing);
                for (int i = 0; i < _poly.NumInteriorRings; i++)
                {
                    Process(_poly.GetInteriorRingN(i));
                }
                double bisectY = Avg(_hiY, _loY);
                return bisectY;
            }

            private void Process(ILineString line)
            {
                var seq = line.CoordinateSequence;
                for (int i = 0; i < seq.Count; i++)
                {
                    double y = seq.GetY(i);
                    UpdateInterval(y);
                }
            }

            private void UpdateInterval(double y)
            {
                if (y <= _centreY)
                {
                    if (y > _loY)
                        _loY = y;
                }
                else if (y > _centreY)
                {
                    if (y < _hiY)
                    {
                        _hiY = y;
                    }
                }

            }
        }
    }
}
