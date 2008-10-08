using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NPack.Interfaces;
#if DOTNET35
using System.Linq;
using GeoAPI.DataStructures;
#endif

namespace GisSharpBlog.NetTopologySuite.Geometries
{
    /// <summary>
    /// Basic implementation of <see cref="IMultiPolygon{TCoordinate}"/>
    /// </summary>
    [Serializable]
    public class MultiPolygon<TCoordinate> : GeometryCollection<TCoordinate>, IMultiPolygon<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                    IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Represents an empty <c>MultiPolygon</c>.
        /// </summary>
        //public new static readonly IMultiPolygon<TCoordinate> Empty = new GeometryFactory<TCoordinate>().CreateMultiPolygon();

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <see cref="Polygon{TCoordinate}" />s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Polygon{TCoordinate}" />s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="GeometryFactory{TCoordinate}.PrecisionModel" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public MultiPolygon(params IPolygon<TCoordinate>[] polygons) : this(polygons, ExtractGeometryFactory(polygons)) { }

        /// <summary>
        /// Constructs a <c>MultiPolygon</c>.
        /// </summary>
        /// <param name="polygons">
        /// The <see cref="Polygon{TCoordinate}" />s for this <c>MultiPolygon</c>
        /// , or <see langword="null" /> or an empty array to create the empty point.
        /// Elements may be empty <see cref="Polygon{TCoordinate}" />s, but not <see langword="null" />
        /// s. The polygons must conform to the assertions specified in the 
        /// <see href="http://www.opengis.org/techno/specs.htm"/> OpenGIS Simple Features
        /// Specification for SQL.        
        /// </param>
        /// <remarks>
        /// For create this <see cref="Geometry{TCoordinate}"/> is used a standard <see cref="GeometryFactory{TCoordinate}"/> 
        /// with <see cref="PrecisionModel{TCoordinate}" /> <c> == </c> <see cref="PrecisionModelType.Floating"/>.
        /// </remarks>
        public MultiPolygon(IEnumerable<IPolygon<TCoordinate>> polygons) 
            : this(polygons, ExtractGeometryFactory(Caster.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polygons))) { }

        /// <summary>
        /// Constructs a <see cref="MultiPolygon{TCoordinate}"/>.
        /// </summary>
        /// <param name="polygons">
        /// The <see cref="Polygon{TCoordinate}" />s for this 
        /// <see cref="MultiPolygon{TCoordinate}"/>, or <see langword="null" /> 
        /// or an empty array to create the empty point.     
        /// </param>
        /// <remarks>
        /// Elements may be empty <see cref="Polygon{TCoordinate}" />s, but not 
        /// <see langword="null" />s. The polygons must conform to the assertions 
        /// specified in the <see href="http://www.opengis.org/techno/specs.htm">
        /// OpenGIS Simple Features Specification for SQL</see>.   
        /// </remarks>
        public MultiPolygon(IEnumerable<IPolygon<TCoordinate>> polygons, IGeometryFactory<TCoordinate> factory)
            : base(Caster.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polygons), factory) { }

        public MultiPolygon(ICoordinateSequence<TCoordinate> sequence, IGeometryFactory<TCoordinate> factory)
            : base(factory)
        {
            IEnumerable<ILinearRing<TCoordinate>> rings 
                = Coordinates<TCoordinate>.CreateLinearRings(sequence, factory);

            ILinearRing<TCoordinate> shell = null;
            List<ILinearRing<TCoordinate>> holes = null;

            foreach (ILinearRing<TCoordinate> ring in rings)
            {
                if (!ring.IsCcw)
                {
                    addPolygonFromRings(shell, holes, factory);
                    shell = ring;
                    holes = null;
                }
                else
                {
                    if (shell == null)
                    {
                        throw new TopologyException(
                            "The coordinate sequence specifies holes without a shell.");
                    }

                    if (holes == null)
                    {
                        holes = new List<ILinearRing<TCoordinate>>();
                    }

                    holes.Add(ring);
                }
            }

            addPolygonFromRings(shell, holes, factory);
        }

        public MultiPolygon(IGeometryFactory<TCoordinate> factory)
            : base(factory) { }

        public override IGeometry<TCoordinate> Boundary
        {
            get
            {
                if (IsEmpty)
                {
                    return Factory.CreateGeometryCollection();
                }

                List<ILineString<TCoordinate>> allRings = new List<ILineString<TCoordinate>>();

                foreach (IPolygon<TCoordinate> polygon in GeometriesInternal)
                {
                    IGeometry<TCoordinate> boundary = polygon.Boundary;
                    
                    Debug.Assert(boundary != null);

                    if(boundary is IGeometryCollection)
                    {
                        IMultiLineString<TCoordinate> boundaryLines = boundary as IMultiLineString<TCoordinate>;
                        allRings.AddRange(boundaryLines);
                    }
                }

                return Factory.CreateMultiLineString(allRings.ToArray());
            }
        }

        public override Dimensions BoundaryDimension
        {
            get { return Dimensions.Curve; }
        }

        public override IGeometry<TCoordinate> Clone()
        {
            List<IPolygon<TCoordinate>> polygons = new List<IPolygon<TCoordinate>>();

            foreach (IPolygon<TCoordinate> polygon in this)
            {
                polygons.Add(polygon.Clone() as IPolygon<TCoordinate>);
            }

            return Factory.CreateMultiPolygon(polygons);
        }

        public override Dimensions Dimension
        {
            get { return Dimensions.Surface; }
        }

        public override OgcGeometryType GeometryType
        {
            get { return OgcGeometryType.MultiPolygon; }
        }

        public override Boolean IsSimple
        {
            get { return true; }
        }

        public new IPolygon<TCoordinate> this[Int32 index]
        {
            get { return base[index] as IPolygon<TCoordinate>; }
            set { base[index] = value; }
        }

        public override Boolean Equals(IGeometry<TCoordinate> other, Tolerance tolerance)
        {
            if (!IsEquivalentClass(other))
            {
                return false;
            }

            return base.Equals(other, tolerance);
        }

        public new IEnumerator<IPolygon<TCoordinate>> GetEnumerator()
        {
            foreach (IPolygon<TCoordinate> polygon in GeometriesInternal)
            {
                yield return polygon;
            }
        }

        IPolygon IMultiPolygon.this[Int32 index]
        {
            get { return base[index] as IPolygon; }
            set { base[index] = value as IPolygon<TCoordinate>; }
        }

        IEnumerator<IPolygon> IEnumerable<IPolygon>.GetEnumerator()
        {
            foreach (IPolygon polygon in GeometriesInternal)
            {
                yield return polygon;
            }
        }

        protected override void CheckItemType(IGeometry<TCoordinate> item)
        {
            if (!(item is IPolygon<TCoordinate>))
            {
                throw new InvalidOperationException(String.Format(
                                                        "Cannot add geometry of type {0} " +
                                                        "to a MultiPolygon",
                                                        item.GetType()));
            }
        }

        #region Private helpers

        private void addPolygonFromRings(ILinearRing<TCoordinate> shell, List<ILinearRing<TCoordinate>> holes, IGeometryFactory<TCoordinate> factory)
        {
            if (shell == null) return;
            if (holes == null)
            {
                Add(factory.CreatePolygon(shell));
            }
            else
            {
                Add(factory.CreatePolygon(shell, holes));
            }
        }
        #endregion
    }
}