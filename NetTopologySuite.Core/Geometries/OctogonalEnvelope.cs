using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    ///     A bounding container for a <see cref="IGeometry" /> which is in the shape of a general octagon.
    /// </summary>
    /// <remarks>
    ///     The OctagonalEnvelope of a geometric object
    ///     is a geometry which is tight bound along the (up to) four extremal rectilineal parallels
    ///     and along the (up to) four extremal diagonal parallels.
    ///     Depending on the shape of the contained
    ///     geometry, the octagon may be degenerate to any extreme
    ///     (e.g. it may be a rectangle, a line, or a point).
    /// </remarks>
    public class OctagonalEnvelope
    {
        private static readonly double SQRT2 = Math.Sqrt(2.0);


        // initialize in the null state

        /// <summary>
        ///     Creates a new null bounding octagon
        /// </summary>
        public OctagonalEnvelope()
        {
        }

        /// <summary>
        ///     Creates a new null bounding octagon bounding a <see cref="Coordinate" />
        /// </summary>
        /// <param name="p">The coordinate to bound</param>
        public OctagonalEnvelope(Coordinate p)
        {
            ExpandToInclude(p);
        }

        /// <summary>
        ///     Creates a new null bounding octagon bounding a pair of <see cref="Coordinate" />s
        /// </summary>
        /// <param name="p0">A coordinate to bound</param>
        /// <param name="p1">A coordinate to bound</param>
        public OctagonalEnvelope(Coordinate p0, Coordinate p1)
        {
            ExpandToInclude(p0);
            ExpandToInclude(p1);
        }

        /// <summary>
        ///     Creates a new null bounding octagon bounding an <see cref="Envelope" />
        /// </summary>
        public OctagonalEnvelope(Envelope env)
        {
            ExpandToInclude(env);
        }

        /// <summary>
        ///     Creates a new null bounding octagon bounding an <see cref="OctagonalEnvelope" />
        ///     (the copy constructor).
        /// </summary>
        public OctagonalEnvelope(OctagonalEnvelope oct)
        {
            ExpandToInclude(oct);
        }

        /// <summary>
        ///     Creates a new null bounding octagon bounding a <see cref="IGeometry" />
        /// </summary>
        public OctagonalEnvelope(IGeometry geom)
        {
            ExpandToInclude(geom);
        }


        public double MinX { get; private set; } = double.NaN;
        public double MaxX { get; private set; }
        public double MinY { get; private set; }
        public double MaxY { get; private set; }
        public double MinA { get; private set; }
        public double MaxA { get; private set; }
        public double MinB { get; private set; }
        public double MaxB { get; private set; }

        /// Sets the value of this object to the null value
        public bool IsNull
        {
            get { return double.IsNaN(MinX); }
            private set
            {
                if (value)
                    MinX = double.NaN;
            }
        }

        /// Tests if the extremal values for this octagon are valid.
        /// 
        /// @return
        /// <code>true</code>
        /// if this object has valid values
        private bool IsValid
        {
            get
            {
                if (IsNull) return true;
                return (MinX <= MaxX)
                       && (MinY <= MaxY)
                       && (MinA <= MaxA)
                       && (MinB <= MaxB);
            }
        }

        /// <summary>
        ///     Gets the octagonal envelope of a geometry
        /// </summary>
        /// <param name="geom">The geometry</param>
        /// <returns>The octagonal envelope of the geometry</returns>
        public static IGeometry GetOctagonalEnvelope(IGeometry geom)
        {
            return new OctagonalEnvelope(geom).ToGeometry(geom.Factory);
        }


        private static double ComputeA(double x, double y)
        {
            return x + y;
        }

        private static double ComputeB(double x, double y)
        {
            return x - y;
        }

        public void ExpandToInclude(IGeometry g)
        {
            g.Apply(new BoundingOctagonComponentFilter(this));
        }

        public OctagonalEnvelope ExpandToInclude(ICoordinateSequence seq)
        {
            for (var i = 0; i < seq.Count; i++)
            {
                var x = seq.GetX(i);
                var y = seq.GetY(i);
                ExpandToInclude(x, y);
            }
            return this;
        }

        public OctagonalEnvelope ExpandToInclude(OctagonalEnvelope oct)
        {
            if (oct.IsNull) return this;

            if (IsNull)
            {
                MinX = oct.MinX;
                MaxX = oct.MaxX;
                MinY = oct.MinY;
                MaxY = oct.MaxY;
                MinA = oct.MinA;
                MaxA = oct.MaxA;
                MinB = oct.MinB;
                MaxB = oct.MaxB;
                return this;
            }
            if (oct.MinX < MinX) MinX = oct.MinX;
            if (oct.MaxX > MaxX) MaxX = oct.MaxX;
            if (oct.MinY < MinY) MinY = oct.MinY;
            if (oct.MaxY > MaxY) MaxY = oct.MaxY;
            if (oct.MinA < MinA) MinA = oct.MinA;
            if (oct.MaxA > MaxA) MaxA = oct.MaxA;
            if (oct.MinB < MinB) MinB = oct.MinB;
            if (oct.MaxB > MaxB) MaxB = oct.MaxB;
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
            var A = ComputeA(x, y);
            var B = ComputeB(x, y);

            if (IsNull)
            {
                MinX = x;
                MaxX = x;
                MinY = y;
                MaxY = y;
                MinA = A;
                MaxA = A;
                MinB = B;
                MaxB = B;
            }
            else
            {
                if (x < MinX) MinX = x;
                if (x > MaxX) MaxX = x;
                if (y < MinY) MinY = y;
                if (y > MaxY) MaxY = y;
                if (A < MinA) MinA = A;
                if (A > MaxA) MaxA = A;
                if (B < MinB) MinB = B;
                if (B > MaxB) MaxB = B;
            }
            return this;
        }

        public void ExpandBy(double distance)
        {
            if (IsNull) return;

            var diagonalDistance = SQRT2*distance;

            MinX -= distance;
            MaxX += distance;
            MinY -= distance;
            MaxY += distance;
            MinA -= diagonalDistance;
            MaxA += diagonalDistance;
            MinB -= diagonalDistance;
            MaxB += diagonalDistance;

            if (!IsValid)
                IsNull = true;
        }

        public bool Intersects(OctagonalEnvelope other)
        {
            if (IsNull || other.IsNull) return false;

            if (MinX > other.MaxX) return false;
            if (MaxX < other.MinX) return false;
            if (MinY > other.MaxY) return false;
            if (MaxY < other.MinY) return false;
            if (MinA > other.MaxA) return false;
            if (MaxA < other.MinA) return false;
            if (MinB > other.MaxB) return false;
            if (MaxB < other.MinB) return false;
            return true;
        }

        public bool Intersects(Coordinate p)
        {
            if (MinX > p.X) return false;
            if (MaxX < p.X) return false;
            if (MinY > p.Y) return false;
            if (MaxY < p.Y) return false;

            var A = ComputeA(p.X, p.Y);
            var B = ComputeB(p.X, p.Y);
            if (MinA > A) return false;
            if (MaxA < A) return false;
            if (MinB > B) return false;
            if (MaxB < B) return false;
            return true;
        }

        public bool Contains(OctagonalEnvelope other)
        {
            if (IsNull || other.IsNull) return false;

            return (other.MinX >= MinX)
                   && (other.MaxX <= MaxX)
                   && (other.MinY >= MinY)
                   && (other.MaxY <= MaxY)
                   && (other.MinA >= MinA)
                   && (other.MaxA <= MaxA)
                   && (other.MinB >= MinB)
                   && (other.MaxB <= MaxB);
        }

        public IGeometry ToGeometry(IGeometryFactory geomFactory)
        {
            if (IsNull)
                return geomFactory.CreatePoint((ICoordinateSequence) null);

            var px00 = new Coordinate(MinX, MinA - MinX);
            var px01 = new Coordinate(MinX, MinX - MinB);

            var px10 = new Coordinate(MaxX, MaxX - MaxB);
            var px11 = new Coordinate(MaxX, MaxA - MaxX);

            var py00 = new Coordinate(MinA - MinY, MinY);
            var py01 = new Coordinate(MinY + MaxB, MinY);

            var py10 = new Coordinate(MaxY + MinB, MaxY);
            var py11 = new Coordinate(MaxA - MaxY, MaxY);

            var pm = geomFactory.PrecisionModel;
            pm.MakePrecise(px00);
            pm.MakePrecise(px01);
            pm.MakePrecise(px10);
            pm.MakePrecise(px11);
            pm.MakePrecise(py00);
            pm.MakePrecise(py01);
            pm.MakePrecise(py10);
            pm.MakePrecise(py11);

            var coordList = new CoordinateList();
            coordList.Add(px00, false);
            coordList.Add(px01, false);
            coordList.Add(py10, false);
            coordList.Add(py11, false);
            coordList.Add(px11, false);
            coordList.Add(px10, false);
            coordList.Add(py01, false);
            coordList.Add(py00, false);

            if (coordList.Count == 1)
                return geomFactory.CreatePoint(px00);
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
                    _octogonalEnvelope.ExpandToInclude(((ILineString) geom).CoordinateSequence);
                else if (geom is IPoint)
                    _octogonalEnvelope.ExpandToInclude(((IPoint) geom).CoordinateSequence);
            }
        }
    }
}