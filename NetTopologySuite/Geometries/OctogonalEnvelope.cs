using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A Bounding Container which is in the shape of an octagon.
    /// </summary>
    /// <remarks>
    /// The OctagonalEnvelope of a geometric object
    /// is tight along the four extremal rectilineal parallels
    /// and along the four extremal diagonal parallels.
    /// Depending on the shape of the contained
    /// geometry, the octagon may be degenerate to any extreme
    /// (e.g. it may be a rectangle, a line, or a point).
    /// </remarks>
    public class OctagonalEnvelope
    {
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
        private double _minX = Double.NaN;
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
        public OctagonalEnvelope(Coordinate p)
        {
            ExpandToInclude(p);
        }

        /// <summary>
        /// Creates a new null bounding octagon bounding a pair of <see cref="Coordinate" />s
        /// </summary>
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
        /// Creates a new null bounding octagon bounding a <see cref="IGeometry" />
        /// </summary>
        public OctagonalEnvelope(IGeometry geom)
        {
            ExpandToInclude(geom);
        }


        public double MinX { get { return _minX; } }
        public double MaxX { get { return _maxX; } }
        public double MinY { get { return _minY; } }
        public double MaxY { get { return _maxY; } }
        public double MinA { get { return _minA; } }
        public double MaxA { get { return _maxA; } }
        public double MinB { get { return _minB; } }
        public double MaxB { get { return _maxB; } }

        ///
        ///  Sets the value of this object to the null value
        ///
        public Boolean IsNull
        {
            get { return Double.IsNaN(_minX); }
            private set
            {
                if (value)
                    _minX = Double.NaN;
            }
        }

        public void ExpandToInclude(IGeometry g)
        {
            g.Apply(new BoundingOctagonComponentFilter(this));
        }

        public OctagonalEnvelope ExpandToInclude(ICoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
            {
                double x = seq.GetX(i);
                double y = seq.GetY(i);
                ExpandToInclude(x, y);
            }
            return this;
        }

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

        public OctagonalEnvelope ExpandToInclude(Coordinate p)
        {
            ExpandToInclude(p.X, p.Y);
            return this;
        }

        public OctagonalEnvelope ExpandToInclude(Envelope env)
        {
            ExpandToInclude(env.MinX, env.MinY);
            ExpandToInclude(env.MinX, env.MaxY);
            ExpandToInclude(env.MaxX, env.MinY);
            ExpandToInclude(env.MaxX, env.MaxY);
            return this;
        }

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

        ///
        /// Tests if the extremal values for this octagon are valid.
        ///
        /// @return <code>true</code> if this object has valid values
        ///
        private Boolean IsValid
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

        public Boolean Intersects(OctagonalEnvelope other)
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

        public Boolean Intersects(Coordinate p)
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

        public Boolean Contains(OctagonalEnvelope other)
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

        public IGeometry ToGeometry(IGeometryFactory geomFactory)
        {
            if (IsNull)
            {
                return geomFactory.CreatePoint((ICoordinateSequence)null);
            }

            Coordinate px00 = new Coordinate(_minX, _minA - _minX);
            Coordinate px01 = new Coordinate(_minX, _minX - _minB);

            Coordinate px10 = new Coordinate(_maxX, _maxX - _maxB);
            Coordinate px11 = new Coordinate(_maxX, _maxA - _maxX);

            Coordinate py00 = new Coordinate(_minA - _minY, _minY);
            Coordinate py01 = new Coordinate(_minY + _maxB, _minY);

            Coordinate py10 = new Coordinate(_maxY + _minB, _maxY);
            Coordinate py11 = new Coordinate(_maxA - _maxY, _maxY);

            IPrecisionModel pm = geomFactory.PrecisionModel;
            pm.MakePrecise(px00);
            pm.MakePrecise(px01);
            pm.MakePrecise(px10);
            pm.MakePrecise(px11);
            pm.MakePrecise(py00);
            pm.MakePrecise(py01);
            pm.MakePrecise(py10);
            pm.MakePrecise(py11);

            CoordinateList coordList = new CoordinateList();
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
            return geomFactory.CreatePolygon(geomFactory.CreateLinearRing(pts), null);
        }

        private class BoundingOctagonComponentFilter : IGeometryComponentFilter
        {
            private readonly OctagonalEnvelope _octogonalEnvelope;

            public BoundingOctagonComponentFilter(OctagonalEnvelope octagonalEnvelope)
            {
                _octogonalEnvelope = octagonalEnvelope;
            }

            public void Filter(IGeometry geom)
            {
                if (geom is ILineString)
                {
                    _octogonalEnvelope.ExpandToInclude(((ILineString)geom).CoordinateSequence);
                }
                else if (geom is IPoint)
                {
                    _octogonalEnvelope.ExpandToInclude(((IPoint)geom).CoordinateSequence);
                }
            }
        }
    }
}
