using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;

namespace GisSharpBlog.NetTopologySuite.Geometries
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
    [Serializable]
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
        protected ILinearRing shell = null;

        /// <summary>
        /// The interior boundaries, if any.
        /// </summary>
        protected ILinearRing[] holes; 
        
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Polygon"/> class.
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
            this.shell = shell;
            this.holes = holes;
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICoordinate Coordinate
        {
            get
            {
                return shell.Coordinate;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override ICoordinate[] Coordinates
        {
            get
            {
                if (IsEmpty)
                    return new ICoordinate[] { };
                ICoordinate[] coordinates = new ICoordinate[NumPoints];
                int k = -1;
                ICoordinate[] shellCoordinates = shell.Coordinates;
                for (int x = 0; x < shellCoordinates.Length; x++)
                {
                    k++;
                    coordinates[k] = shellCoordinates[x];
                }
                for (int i = 0; i < holes.Length; i++)
                {
                    ICoordinate[] childCoordinates = holes[i].Coordinates;
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
                int numPoints = shell.NumPoints;
                for (int i = 0; i < holes.Length; i++)
                    numPoints += holes[i].NumPoints;
                return numPoints;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions Dimension
        {
            get
            {
                return Dimensions.Surface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Dimensions BoundaryDimension
        {
            get
            {
                return Dimensions.Curve;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsEmpty
        {
            get
            {
                return shell.IsEmpty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILineString ExteriorRing
        {
            get
            {
                return shell;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumInteriorRings
        {
            get
            {
                return holes.Length;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILineString[] InteriorRings
        {
            get
            {
                return holes;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public ILineString GetInteriorRingN(int n) 
        {
            return holes[n];
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GeometryType
        {
            get
            {
                return "Polygon";
            }
        }

        /// <summary> 
        /// Returns the area of this <c>Polygon</c>
		/// </summary>
		/// <returns></returns>
        public override double Area
        {
            get
            {
                double area = 0.0;
                area += Math.Abs(CGAlgorithms.SignedArea(shell.Coordinates));
                for (int i = 0; i < holes.Length; i++)
                    area -= Math.Abs(CGAlgorithms.SignedArea(holes[i].Coordinates));                
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
                len += shell.Length;
                for (int i = 0; i < holes.Length; i++)
                    len += holes[i].Length;
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
                    return Factory.CreateGeometryCollection(null);
                ILinearRing[] rings = new ILinearRing[holes.Length + 1];
                rings[0] = shell;
                for (int i = 0; i < holes.Length; i++)
                    rings[i + 1] = holes[i];
                if (rings.Length <= 1)
                    return Factory.CreateLinearRing(rings[0].CoordinateSequence);
                return Factory.CreateMultiLineString(rings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override IEnvelope ComputeEnvelopeInternal() 
        {
            return shell.EnvelopeInternal;
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
            IGeometry thisShell = shell;
            IGeometry otherPolygonShell = otherPolygon.Shell;
            if (!thisShell.EqualsExact(otherPolygonShell, tolerance)) 
                return false;
            if (holes.Length != otherPolygon.Holes.Length) 
                return false;
            if (holes.Length != otherPolygon.Holes.Length) 
                return false;
            for (int i = 0; i < holes.Length; i++) 
                if (!(holes[i]).EqualsExact(otherPolygon.Holes[i], tolerance)) 
                    return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public override void Apply(ICoordinateFilter filter)
        {
            shell.Apply(filter);
            for (int i = 0; i < holes.Length; i++) 
                holes[i].Apply(filter);            
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
            shell.Apply(filter);
            for (int i = 0; i < holes.Length; i++) 
                holes[i].Apply(filter);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object Clone() 
        {
            Polygon poly = (Polygon) base.Clone();
            poly.shell = (LinearRing) shell.Clone();
            poly.holes = new ILinearRing[holes.Length];
            for (int i = 0; i < holes.Length; i++) 
                poly.holes[i] = (LinearRing) holes[i].Clone();            
            return poly; 
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
            Normalize(shell, true);
            foreach(ILinearRing hole in Holes)
                Normalize(hole, false);
            Array.Sort(holes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        protected internal override int CompareToSameClass(object o) 
        {   
            LinearRing thisShell = (LinearRing) shell;
            ILinearRing otherShell = ((IPolygon) o).Shell;
            return thisShell.CompareToSameClass(otherShell);
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
            ICoordinate[] uniqueCoordinates = new ICoordinate[ring.Coordinates.Length - 1];
            Array.Copy(ring.Coordinates, 0, uniqueCoordinates, 0, uniqueCoordinates.Length);
            ICoordinate minCoordinate = CoordinateArrays.MinCoordinate(ring.Coordinates);
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
        public Polygon(LinearRing shell, GeometryFactory factory) : this(shell, null, factory) { }

        /// <summary>
        /// Constructs a <c>Polygon</c> with the given exterior boundary.
        /// </summary>
        /// <param name="shell">
        /// The outer boundary of the new <c>Polygon</c>,
        /// or <c>null</c> or an empty <c>LinearRing</c> if the empty
        /// polygon is to be created.
        /// </param>
        public Polygon(LinearRing shell) : this(shell, null, DefaultFactory) { }

        /// <summary>
        /// 
        /// </summary>
        public ILinearRing Shell
        {
            get
            {
                return shell;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ILinearRing[] Holes
        {
            get
            {
                return holes;
            }
        }

        /*END ADDED BY MPAUL42 */

    }
}
