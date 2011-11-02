using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Geometries
{
    /// <summary> 
    /// Represents a linear polygon, which may include holes.
    /// The shell and holes of the polygon are represented by {LinearRing}s.
    /// In a valid polygon, holes may touch the shell or other holes at a single point.
    /// However, no sequence of touching holes may split the polygon into two pieces.
    /// The orientation of the rings in the polygon does not matter.
    /// The shell and holes must conform to the assertions specified in the
    /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features Specification for SQL.     
    /// </summary>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    public class Polygon : Geometry, IPolygon
    {
        /// <summary>
        /// Represents an empty <c>Polygon</c>.
        /// </summary>
        public static readonly IPolygon Empty = new GeometryFactory().CreatePolygon(null, null);

        /// <summary>
        /// The exterior boundary, or <c>null</c> if this <c>Polygon</c>
        /// is the empty point.
        /// </summary>
        private ILinearRing _shell;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        private ILinearRing[] _holes; 
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Polygon"/> class.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>
        /// , or <c>null</c> or empty <c>LinearRing</c>s if the empty
        /// point is to be created.
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        public Polygon(ILinearRing shell, ILinearRing[] holes) : this(shell, holes, DefaultFactory) { }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary and
        /// interior boundaries.
        /// </summary>       
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// point is to be created.
        /// </param>
        /// <param name="holes">
        /// The inner boundaries of the new <c>Polygon</c>
        /// , or <c>null</c> or empty <c>LinearRing</c>s if the empty
        /// point is to be created.
        /// </param>
        /// <param name="factory"></param>
        public Polygon(ILinearRing shell, ILinearRing[] holes, IGeometryFactory factory) : base(factory)
        {        
            if (shell == null) 
                shell = Factory.CreateLinearRing((ICoordinateSequence) null);            
            if (holes == null) 
                holes = new ILinearRing[] { };
            if (HasNullElements(holes)) 
                throw new ArgumentException("holes must not contain null elements");
            if (shell.IsEmpty && HasNonEmptyElements(holes)) 
                throw new ArgumentException("shell is empty but holes are not");
            _shell = shell;
            _holes = holes;
        }

        /// <summary>
        /// 
        /// </summary>
        public override Coordinate Coordinate
        {
            get
            {
                return _shell.Coordinate;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Coordinate[] Coordinates
        {
            get
            {
                if (IsEmpty)
                    return new Coordinate[] { };
                Coordinate[] coordinates = new Coordinate[NumPoints];
                int k = -1;
                Coordinate[] shellCoordinates = _shell.Coordinates;
                for (int x = 0; x < shellCoordinates.Length; x++)
                {
                    k++;
                    coordinates[k] = shellCoordinates[x];
                }
                for (int i = 0; i < _holes.Length; i++)
                {
                    Coordinate[] childCoordinates = _holes[i].Coordinates;
                    for (int j = 0; j < childCoordinates.Length; j++)
                    {
                        k++;
                        coordinates[k] = childCoordinates[j];
                    }
                }
                return coordinates;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int NumPoints
        {
            get
            {
                int numPoints = _shell.NumPoints;
                for (int i = 0; i < _holes.Length; i++)
                    numPoints += _holes[i].NumPoints;
                return numPoints;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimension Dimension
        {
            get
            {
                return Dimension.Surface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimension BoundaryDimension
        {
            get
            {
                return Dimension.Curve;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                return _shell.IsEmpty;
            }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public override bool IsSimple
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        public ILineString ExteriorRing
        {
            get
            {
                return _shell;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumInteriorRings
        {
            get
            {
                return _holes.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILineString[] InteriorRings
        {
            get
            {
                return _holes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public ILineString GetInteriorRingN(int n) 
        {
            return _holes[n];
        }

        /// <summary>  
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"Polygon"</returns>
        public override string GeometryType
        {
            get
            {
                return "Polygon";
            }
        }

        public override OgcGeometryType OgcGeometryType
        {
            get { return OgcGeometryType.Polygon; }
        }
        /// <summary> 
        /// Returns the area of this <c>Polygon</c>
		/// </summary>
		/// <returns></returns>
        public override double Area
        {
            get
            {
                var area = 0.0;
                area += Math.Abs(CGAlgorithms.SignedArea(_shell.CoordinateSequence));
                for (int i = 0; i < _holes.Length; i++)
                    area -= Math.Abs(CGAlgorithms.SignedArea(_holes[i].CoordinateSequence));                
                return area;
            }
        }

        /// <summary>
        /// Returns the perimeter of this <c>Polygon</c>.
		/// </summary>
		/// <returns></returns>
        public override double Length
        {
            get
            {
                double len = 0.0;
                len += _shell.Length;
                for (int i = 0; i < _holes.Length; i++)
                    len += _holes[i].Length;
                return len;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override IGeometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreateMultiLineString(null);
                ILinearRing[] rings = new ILinearRing[_holes.Length + 1];
                rings[0] = _shell;
                for (int i = 0; i < _holes.Length; i++)
                    rings[i + 1] = _holes[i];
                // create LineString or MultiLineString as appropriate
                if (rings.Length <= 1)
                    return Factory.CreateLinearRing(rings[0].CoordinateSequence);
                return Factory.CreateMultiLineString(rings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override Envelope ComputeEnvelopeInternal() 
        {
            return _shell.EnvelopeInternal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public override bool EqualsExact(IGeometry other, double tolerance) 
        {
            if (!IsEquivalentClass(other)) 
                return false;

            IPolygon otherPolygon = (IPolygon) other;
            IGeometry thisShell = _shell;
            IGeometry otherPolygonShell = otherPolygon.Shell;
            if (!thisShell.EqualsExact(otherPolygonShell, tolerance)) 
                return false;
            if (_holes.Length != otherPolygon.Holes.Length) 
                return false;
            if (_holes.Length != otherPolygon.Holes.Length) 
                return false;
            for (int i = 0; i < _holes.Length; i++) 
                if (!(_holes[i]).EqualsExact(otherPolygon.Holes[i], tolerance)) 
                    return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(ICoordinateFilter filter)
        {
            _shell.Apply(filter);
            for (int i = 0; i < _holes.Length; i++) 
                _holes[i].Apply(filter);            
        }

        public override void Apply(ICoordinateSequenceFilter filter)
        {
            ((LinearRing)_shell).Apply(filter);
            if (!filter.Done)
            {
                for (int i = 0; i < _holes.Length; i++)
                {
                    ((LinearRing)_holes[i]).Apply(filter);
                    if (filter.Done)
                        break;
                }
            }
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryFilter filter) 
        {
            filter.Filter(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(IGeometryComponentFilter filter) 
        {
            filter.Filter(this);
            _shell.Apply(filter);
            for (int i = 0; i < _holes.Length; i++) 
                _holes[i].Apply(filter);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone() 
        {
            Polygon poly = (Polygon) base.Clone();
            poly._shell = (LinearRing) _shell.Clone();
            poly._holes = new ILinearRing[_holes.Length];
            for (int i = 0; i < _holes.Length; i++) 
                poly._holes[i] = (LinearRing) _holes[i].Clone();            
            return poly; 
        }

        internal override int GetHashCodeInternal(int baseValue, Func<int, int> operation)
        {
            if (!IsEmpty)
            {
                baseValue = _shell.CoordinateSequence.GetHashCode(baseValue, operation);
                foreach(var ring in _holes)
                    baseValue = ring.CoordinateSequence.GetHashCode(baseValue, operation);
            }
            return baseValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IGeometry ConvexHull()
        {            
            return ExteriorRing.ConvexHull();         
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Normalize() 
        {
            Normalize(_shell, true);
            foreach(ILinearRing hole in Holes)
                Normalize(hole, false);
            Array.Sort(_holes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o) 
        {   
            LinearRing thisShell = (LinearRing) _shell;
            ILinearRing otherShell = ((IPolygon) o).Shell;
            return thisShell.CompareToSameClass(otherShell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="comparer"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object other, IComparer<ICoordinateSequence> comparer)
        {
            IPolygon poly = (IPolygon)other;

            LinearRing thisShell = (LinearRing)_shell;
            LinearRing otherShell = (LinearRing)poly.Shell;
            int shellComp = thisShell.CompareToSameClass(otherShell, comparer);
            if (shellComp != 0) return shellComp;

            int nHole1 = NumInteriorRings;
            int nHole2 = poly.NumInteriorRings;
            int i = 0;
            while (i < nHole1 && i < nHole2)
            {
                LinearRing thisHole = (LinearRing)GetInteriorRingN(i);
                LinearRing otherHole = (LinearRing)poly.GetInteriorRingN(i);
                int holeComp = thisHole.CompareToSameClass(otherHole, comparer);
                if (holeComp != 0) return holeComp;
                i++;
            }
            if (i < nHole1) return 1;
            if (i < nHole2) return -1;
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="clockwise"></param>        
        private void Normalize(ILinearRing ring, bool clockwise) 
        {
            if (ring.IsEmpty) 
                return;            
            Coordinate[] uniqueCoordinates = new Coordinate[ring.Coordinates.Length - 1];
            Array.Copy(ring.Coordinates, 0, uniqueCoordinates, 0, uniqueCoordinates.Length);
            Coordinate minCoordinate = CoordinateArrays.MinCoordinate(ring.Coordinates);
            CoordinateArrays.Scroll(uniqueCoordinates, minCoordinate);
            Array.Copy(uniqueCoordinates, 0, ring.Coordinates, 0, uniqueCoordinates.Length);
            ring.Coordinates[uniqueCoordinates.Length] = uniqueCoordinates[0];
            if (CGAlgorithms.IsCCW(ring.Coordinates) == clockwise) 
                CoordinateArrays.Reverse(ring.Coordinates);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsRectangle
        {
            get
            {
                if (NumInteriorRings != 0) return false;
                if (Shell == null) return false;
                if (Shell.NumPoints != 5) return false;

                // check vertices have correct values
                ICoordinateSequence seq = Shell.CoordinateSequence;                
                Envelope env = (Envelope) EnvelopeInternal;
                for (int i = 0; i < 5; i++)
                {
                    double x = seq.GetX(i);
                    if (!(x == env.MinX || x == env.MaxX)) 
                        return false;
                    
                    double y = seq.GetY(i);
                    if (!(y == env.MinY || y == env.MaxY))
                        return false;
                }

                // check vertices are in right order
                double prevX = seq.GetX(0);
                double prevY = seq.GetY(0);
                for (int i = 1; i <= 4; i++)
                {
                    double x = seq.GetX(i);
                    double y = seq.GetY(i);

                    bool xChanged = x != prevX;
                    bool yChanged = y != prevY;
                    
                    if (xChanged == yChanged)
                        return false;
                    
                    prevX = x;
                    prevY = y;
                }
                return true;
            }
        }

        public override IGeometry Reverse()
        {
            Polygon poly = (Polygon)Clone();
            poly.Shell = (LinearRing)((LinearRing)_shell.Clone()).Reverse();
            poly.Holes = new LinearRing[_holes.Length];
            for (int i = 0; i < _holes.Length; i++)
            {
                poly.Holes[i] = (LinearRing)((LinearRing)_holes[i].Clone()).Reverse();
            }
            return poly;// return the clone
        }


        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// polygon is to be created.
        /// </param>
        /// <param name="factory"></param>
        public Polygon(ILinearRing shell, IGeometryFactory factory) : this(shell, null, factory) { }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(ILinearRing shell) : this(shell, null, DefaultFactory) { }

        /// <summary>
        /// 
        /// </summary>
        public ILinearRing Shell
        {
            get
            {
                return _shell;
            }
            private set { _shell = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILinearRing[] Holes
        {
            get
            {
                return _holes;
            }
            private set { _holes = value; }
        }

        /*END ADDED BY MPAUL42 */

    }

    public static class CoordinateSequenceEx
    {
        public static int GetHashCode(this ICoordinateSequence sequence, int baseValue, Func<int, int> operation)
        {
            if (sequence!=null && sequence.Count > 0)
            {
                for (var i = 0; i < sequence.Count; i++)
                    baseValue = operation(baseValue) + sequence.GetX(i).GetHashCode();
            }
            return baseValue;
        }
        
    }
}
