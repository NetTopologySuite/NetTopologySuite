using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A bounding container for a <see cref="Geometry"/> which is in the shape of a general octagon.
    /// </summary>
    /// <remarks>
    /// The OctagonalEnvelope of a geometric object
    /// is a geometry which is tight bound along the (up to) four extremal rectilinear parallels
    /// and along the (up to) four extremal diagonal parallels.
    /// Depending on the shape of the contained
    /// geometry, the octagon may be degenerate to any extreme
    /// (e.g. it may be a rectangle, a line, or a point).
    /// </remarks>
    public class OctagonalEnvelope
    {
        /// <summary>
        /// Gets the octagonal envelope of a geometry
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <returns>The octagonal envelope of the geometry</returns>
        public static Geometry GetOctagonalEnvelope(Geometry geom)
        {
            return (new OctagonalEnvelope(geom)).ToGeometry(geom.Factory);
        }

        private static double ComputeA(double x, double y)
        {
            return x + y;
        }

        private static double ComputeB(double x, double y)
        {
            return x - y;
        }

        private static readonly double SQRT2 = Math.Sqrt(2.0);

        // initialize in the null state
        private double _minX = double.NaN;
        private double _maxX;
        private double _minY;
        private double _maxY;
        private double _minA;
        private double _maxA;
        private double _minB;
        private double _maxB;

        /// <summary>
        /// Creates a new null bounding octagon
        /// </summary>
        public OctagonalEnvelope()
        {
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding a <see cref="Coordinate" />
        /// </summary>
        /// <param name="p">The coordinate to bound</param>
        public OctagonalEnvelope(Coordinate p)
        {
            ExpandToInclude(p);
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding a pair of <see cref="Coordinate" />s
        /// </summary>
        /// <param name="p0">A coordinate to bound</param>
        /// <param name="p1">A coordinate to bound</param>
        public OctagonalEnvelope(Coordinate p0, Coordinate p1)
        {
            ExpandToInclude(p0);
            ExpandToInclude(p1);
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding an <see cref="Envelope" />
        /// </summary>
        public OctagonalEnvelope(Envelope env)
        {
            ExpandToInclude(env);
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding an <see cref="OctagonalEnvelope" />
        /// (the copy constructor).
        /// </summary>
        public OctagonalEnvelope(OctagonalEnvelope oct)
        {
            ExpandToInclude(oct);
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding a <see cref="Geometry" />
        /// </summary>
        public OctagonalEnvelope(Geometry geom)
        {
            ExpandToInclude(geom);
        }

        /// <summary>
        /// Gets a value indicating the minimal x-ordinate value
        /// </summary>
        public double MinX => _minX;

        /// <summary>
        /// Gets a value indicating the maximal x-ordinate value
        /// </summary>
        public double MaxX => _maxX;

        /// <summary>
        /// Gets a value indicating the minimal y-ordinate value
        /// </summary>
        public double MinY => _minY;

        /// <summary>
        /// Gets a value indicating the maximal y-ordinate value
        /// </summary>
        public double MaxY => _maxY;

        /// <summary>
        /// Gets a value indicating the minimal <c>a</c> value
        /// </summary>
        public double MinA => _minA;

        /// <summary>
        /// Gets a value indicating the maximal <c>a</c> value
        /// </summary>
        public double MaxA => _maxA;

        /// <summary>
        /// Gets a value indicating the minimal <c>b</c> value
        /// </summary>
        public double MinB => _minB;

        /// <summary>
        /// Gets a value indicating the maximal <c>b</c> value
        /// </summary>
        public double MaxB => _maxB;

        /// <summary>
        /// Gets a value indicating that this object is null
        /// </summary>
        public bool IsNull
        {
            get => double.IsNaN(_minX);
            private set
            {
                if (value)
                    _minX = double.NaN;
            }
        }

        /// <summary>
        /// Method to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="g"/> geometry.
        /// </summary>
        /// <param name="g">The geometry</param>
        public void ExpandToInclude(Geometry g)
        {
            g.Apply(new BoundingOctagonComponentFilter(this));
        }

        /// <summary>
        /// Method to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="seq"/> coordinate sequence.
        /// </summary>
        /// <param name="seq">The coordinate sequence</param>
        /// <returns>A reference to <c>this</c> octagonal envelope, expanded by <paramref name="seq"/></returns>
        public OctagonalEnvelope ExpandToInclude(CoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                double x = seq.GetX(i);
                double y = seq.GetY(i);
                ExpandToInclude(x, y);
            }
            return this;
        }

        /// <summary>
        /// Method to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="oct"/> OctogonalEnvelope.
        /// </summary>
        /// <param name="oct">The OctogonalEnvelope</param>
        /// <returns>A reference to <c>this</c> octagonal envelope, expanded by <paramref name="oct"/></returns>
        public OctagonalEnvelope ExpandToInclude(OctagonalEnvelope oct)
        {
            if (oct.IsNull) return this;

            if (IsNull)
            {
                _minX = oct._minX;
                _maxX = oct._maxX;
                _minY = oct._minY;
                _maxY = oct._maxY;
                _minA = oct._minA;
                _maxA = oct._maxA;
                _minB = oct._minB;
                _maxB = oct._maxB;
                return this;
            }
            if (oct._minX < _minX) _minX = oct._minX;
            if (oct._maxX > _maxX) _maxX = oct._maxX;
            if (oct._minY < _minY) _minY = oct._minY;
            if (oct._maxY > _maxY) _maxY = oct._maxY;
            if (oct._minA < _minA) _minA = oct._minA;
            if (oct._maxA > _maxA) _maxA = oct._maxA;
            if (oct._minB < _minB) _minB = oct._minB;
            if (oct._maxB > _maxB) _maxB = oct._maxB;
            return this;
        }

        /// <summary>
        /// Function to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="p"/> coordinate.
        /// </summary>
        /// <param name="p">The coordinate</param>
        /// <returns>A reference to <c>this</c> octagonal envelope, expanded by <paramref name="p"/></returns>
        public OctagonalEnvelope ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
            return this;
        }

        /// <summary>
        /// Function to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="env"/> envelope.
        /// </summary>
        /// <param name="env">The envelope</param>
        /// <returns>A reference to <c>this</c> octagonal envelope, expanded by <paramref name="env"/></returns>
        public OctagonalEnvelope ExpandToInclude(Envelope env)
        {
            ExpandToInclude(env.MinX, env.MinY);
            ExpandToInclude(env.MinX, env.MaxY);
            ExpandToInclude(env.MaxX, env.MinY);
            ExpandToInclude(env.MaxX, env.MaxY);
            return this;
        }

        /// <summary>
        /// Function to expand this <see cref="OctagonalEnvelope"/> to include the provided <paramref name="x"/>- and <paramref name="y"/> ordinates.
        /// </summary>
        /// <param name="x">A x-ordinate value</param>
        /// <param name="y">A y-ordinate value</param>
        /// <returns>A reference to <c>this</c> octagonal envelope, expanded by <paramref name="x"/> and <paramref name="y"/></returns>
        public OctagonalEnvelope ExpandToInclude(double x, double y)
        {
            double A = ComputeA(x, y);
            double B = ComputeB(x, y);

            if (IsNull)
            {
                _minX = x;
                _maxX = x;
                _minY = y;
                _maxY = y;
                _minA = A;
                _maxA = A;
                _minB = B;
                _maxB = B;
            }
            else
            {
                if (x < _minX) _minX = x;
                if (x > _maxX) _maxX = x;
                if (y < _minY) _minY = y;
                if (y > _maxY) _maxY = y;
                if (A < _minA) _minA = A;
                if (A > _maxA) _maxA = A;
                if (B < _minB) _minB = B;
                if (B > _maxB) _maxB = B;
            }
            return this;
        }

        public void ExpandBy(double distance)
        {
            if (IsNull) return;

            double diagonalDistance = SQRT2 * distance;

            _minX -= distance;
            _maxX += distance;
            _minY -= distance;
            _maxY += distance;
            _minA -= diagonalDistance;
            _maxA += diagonalDistance;
            _minB -= diagonalDistance;
            _maxB += diagonalDistance;

            if (!IsValid)
                IsNull = true;
        }

        /// <summary>
        /// Gets a value indicating if the extremal values for this octagon are valid.
        /// </summary>
        /// <returns><c>true</c> if this object has valid values</returns>
        private bool IsValid
        {
            get
            {
                if (IsNull) return true;
                return _minX <= _maxX
                       && _minY <= _maxY
                       && _minA <= _maxA
                       && _minB <= _maxB;
            }
        }

        /// <summary>
        /// Function to test if <c>this</c> octagonal envelope intersects <paramref name="other"/> octagonal envelope .
        /// </summary>
        /// <param name="other">An octagonal envelope </param>
        /// <returns><c>true</c> if <c>this</c> octagonal envelope intersects <paramref name="other"/> octagonal envelope .</returns>
        public bool Intersects(OctagonalEnvelope other)
        {
            if (IsNull || other.IsNull) { return false; }

            if (_minX > other._maxX) return false;
            if (_maxX < other._minX) return false;
            if (_minY > other._maxY) return false;
            if (_maxY < other._minY) return false;
            if (_minA > other._maxA) return false;
            if (_maxA < other._minA) return false;
            if (_minB > other._maxB) return false;
            if (_maxB < other._minB) return false;
            return true;
        }

        /// <summary>
        /// Function to test if <c>this</c> octagonal envelope contains <paramref name="p"/> coordinate.
        /// </summary>
        /// <param name="p">A coordinate</param>
        /// <returns><c>true</c> if <c>this</c> octagonal envelope contains <paramref name="p"/> coordinate.</returns>
        public bool Intersects(Coordinate p)
        {
            if (_minX > p.X) return false;
            if (_maxX < p.X) return false;
            if (_minY > p.Y) return false;
            if (_maxY < p.Y) return false;

            double A = ComputeA(p.X, p.Y);
            double B = ComputeB(p.X, p.Y);
            if (_minA > A) return false;
            if (_maxA < A) return false;
            if (_minB > B) return false;
            if (_maxB < B) return false;
            return true;
        }

        /// <summary>
        /// Function to test if <c>this</c> octagonal envelope contains <paramref name="other"/> octagonal envelope.
        /// </summary>
        /// <param name="other">An octagonal envelope</param>
        /// <returns><c>true</c> if <c>this</c> octagonal envelope contains <paramref name="other"/> octagonal envelope.</returns>
        public bool Contains(OctagonalEnvelope other)
        {
            if (IsNull || other.IsNull) { return false; }

            return other._minX >= _minX
                && other._maxX <= _maxX
                && other._minY >= _minY
                && other._maxY <= _maxY
                && other._minA >= _minA
                && other._maxA <= _maxA
                && other._minB >= _minB
                && other._maxB <= _maxB;
        }

        /// <summary>
        /// Function to convert <c>this</c> octagonal envelope into a geometry
        /// </summary>
        /// <param name="geomFactory">The factory to create the geometry</param>
        /// <returns>A geometry</returns>
        public Geometry ToGeometry(GeometryFactory geomFactory)
        {
            if (geomFactory == null)
                throw new ArgumentNullException(nameof(geomFactory));

            if (IsNull)
            {
                return geomFactory.CreatePoint();
            }

            var px00 = new Coordinate(_minX, _minA - _minX);
            var px01 = new Coordinate(_minX, _minX - _minB);

            var px10 = new Coordinate(_maxX, _maxX - _maxB);
            var px11 = new Coordinate(_maxX, _maxA - _maxX);

            var py00 = new Coordinate(_minA - _minY, _minY);
            var py01 = new Coordinate(_minY + _maxB, _minY);

            var py10 = new Coordinate(_maxY + _minB, _maxY);
            var py11 = new Coordinate(_maxA - _maxY, _maxY);

            var pm = geomFactory.PrecisionModel;
            pm.MakePrecise(px00);
            pm.MakePrecise(px01);
            pm.MakePrecise(px10);
            pm.MakePrecise(px11);
            pm.MakePrecise(py00);
            pm.MakePrecise(py01);
            pm.MakePrecise(py10);
            pm.MakePrecise(py11);

            var coordList = new CoordinateList(9);
            coordList.Add(px00, false);
            coordList.Add(px01, false);
            coordList.Add(py10, false);
            coordList.Add(py11, false);
            coordList.Add(px11, false);
            coordList.Add(px10, false);
            coordList.Add(py01, false);
            coordList.Add(py00, false);

            if (coordList.Count == 1)
            {
                return geomFactory.CreatePoint(px00);
            }
            Coordinate[] pts;
            if (coordList.Count == 2)
            {
                pts = coordList.ToCoordinateArray();
                return geomFactory.CreateLineString(pts);
            }
            // must be a polygon, so add closing point
            coordList.Add(px00, false);
            pts = coordList.ToCoordinateArray();
            return geomFactory.CreatePolygon(geomFactory.CreateLinearRing(pts));
        }

        private class BoundingOctagonComponentFilter : IGeometryComponentFilter
        {
            private readonly OctagonalEnvelope _octogonalEnvelope;

            public BoundingOctagonComponentFilter(OctagonalEnvelope octagonalEnvelope)
            {
                _octogonalEnvelope = octagonalEnvelope;
            }

            public void Filter(Geometry geom)
            {
                if (geom is LineString lgeom)
                {
                    _octogonalEnvelope.ExpandToInclude(lgeom.CoordinateSequence);
                }
                else if (geom is Point pgeom)
                {
                    _octogonalEnvelope.ExpandToInclude(pgeom.CoordinateSequence);
                }
            }
        }
    }
}
