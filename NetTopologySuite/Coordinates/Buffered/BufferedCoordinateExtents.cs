using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    internal class BufferedCoordinateExtents : IExtents<BufferedCoordinate>
    {
        private readonly BufferedCoordinateFactory _coordinateFactory;
        private BufferedCoordinate _min;
        private BufferedCoordinate _max;

        public BufferedCoordinateExtents(BufferedCoordinateFactory coordinateFactory)
            : this(coordinateFactory, new BufferedCoordinate(), new BufferedCoordinate()) { }

        public BufferedCoordinateExtents(BufferedCoordinateFactory coordinateFactory, BufferedCoordinate min, BufferedCoordinate max)
        {
            _coordinateFactory = coordinateFactory;
            _min = min;
            _max = max;
        }

        #region Implementation of ICloneable

        public Object Clone()
        {
            return new BufferedCoordinateExtents(_coordinateFactory, _min, _max);
        }

        #endregion

        #region Implementation of IComparable

        Int32 IComparable.CompareTo(Object obj)
        {
            return CompareTo(obj as IExtents<BufferedCoordinate>);
        }

        #endregion

        #region Implementation of IEquatable<IExtents>

        public Boolean Equals(IExtents other)
        {
            return other != null && Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        #endregion

        #region Implementation of IContainable<IExtents>

        public Boolean Contains(IExtents other)
        {
            return other != null && Contains(other.Min) && Contains(other.Max);
        }

        #endregion

        #region Implementation of IIntersectable<IExtents>

        public Boolean Intersects(IExtents other)
        {
            return other != null && (Intersects(other.Min) || Intersects(other.Max));
        }

        #endregion

        public Boolean Intersects(ICoordinate coordinate)
        {
            return coordinate != null &&
                   coordinate[Ordinates.X] >= Min[Ordinates.X] && 
                   coordinate[Ordinates.X] <= Max[Ordinates.X] &&
                   coordinate[Ordinates.Y] >= Min[Ordinates.Y] && 
                   coordinate[Ordinates.Y] <= Max[Ordinates.Y];
        }

        #region Implementation of IComparable<IExtents<BufferedCoordinate>>

        public Int32 CompareTo(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Implementation of IEquatable<IExtents<BufferedCoordinate>>

        public Boolean Equals(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Implementation of IContainable<IExtents<BufferedCoordinate>>

        public Boolean Contains(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Implementation of IIntersectable<IExtents<BufferedCoordinate>>

        public Boolean Intersects(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        #region Implementation of IExtents<BufferedCoordinate>

        public Boolean Borders(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Borders(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Borders(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Borders(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Borders(IExtents other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Borders(IExtents other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        ICoordinate IExtents.Center
        {
            get { return Center; }
        }

        public Boolean Contains(params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(ICoordinate other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(Tolerance tolerance, params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(IExtents other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(ICoordinate other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Double Distance(IExtents extents)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(ICoordinateSequence coordinates)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(IExtents other)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(IGeometry other)
        {
            throw new System.NotImplementedException();
        }

        IGeometryFactory IExtents.Factory
        {
            get { return Factory; }
        }

        public IExtents Intersection(IExtents extents)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(Tolerance tolerance, params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(IExtents other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean IsEmpty
        {
            get { throw new System.NotImplementedException(); }
        }

        ICoordinate IExtents.Max
        {
            get { return Max; }
        }

        ICoordinate IExtents.Min
        {
            get { return Min; }
        }

        public Double GetMax(Ordinates ordinate)
        {
            throw new System.NotImplementedException();
        }

        public Double GetMin(Ordinates ordinate)
        {
            throw new System.NotImplementedException();
        }

        public Double GetSize(Ordinates axis)
        {
            throw new System.NotImplementedException();
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2)
        {
            throw new System.NotImplementedException();
        }

        public Double GetSize(Ordinates axis1, Ordinates axis2, Ordinates axis3)
        {
            throw new System.NotImplementedException();
        }

        public Double GetSize(params Ordinates[] axes)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(params Double[] coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(ICoordinate other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(IExtents other)
        {
            throw new System.NotImplementedException();
        }

        public void Scale(params Double[] vector)
        {
            throw new System.NotImplementedException();
        }

        public void Scale(Double factor)
        {
            throw new System.NotImplementedException();
        }

        public void Scale(Double factor, Ordinates axis)
        {
            throw new System.NotImplementedException();
        }

        public void SetToEmpty()
        {
            throw new System.NotImplementedException();
        }

        IGeometry IExtents.ToGeometry()
        {
            return ToGeometry();
        }

        public void Translate(params Double[] vector)
        {
            throw new System.NotImplementedException();
        }

        public void TranslateRelativeToWidth(params Double[] vector)
        {
            throw new System.NotImplementedException();
        }

        public void Transform(ITransformMatrix<DoubleComponent> transformMatrix)
        {
            throw new System.NotImplementedException();
        }

        public IExtents Union(IPoint point)
        {
            throw new System.NotImplementedException();
        }

        public IExtents Union(IExtents box)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Touches(IExtents a)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Within(IExtents a)
        {
            throw new System.NotImplementedException();
        }

        public BufferedCoordinate Center
        {
            get { throw new System.NotImplementedException(); }
        }

        public Boolean Contains(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Contains(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Double Distance(IExtents<BufferedCoordinate> extents)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(params BufferedCoordinate[] coordinates)
        {
            ExpandToInclude(coordinates as IEnumerable<BufferedCoordinate>);
        }

        public void ExpandToInclude(IEnumerable<BufferedCoordinate> coordinates)
        {
            Boolean minCoordSet = !_min.IsEmpty;
            Boolean maxCoordSet = !_max.IsEmpty;

            foreach (BufferedCoordinate coordinate in coordinates)
            {
                if (!minCoordSet)
                {
                    _min = coordinate;
                    minCoordSet = true;
                }
                else if (coordinate.LessThan(_min))
                {
                    _min = coordinate;
                }

                if (!maxCoordSet)
                {
                    _max = coordinate;
                    maxCoordSet = true;
                }
                else if (coordinate.GreaterThan(_max))
                {
                    _max = coordinate;
                }
            }
        }

        public void ExpandToInclude(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        public void ExpandToInclude(IGeometry<BufferedCoordinate> geometry)
        {
            throw new System.NotImplementedException();
        }

        public Double GetIntersectingArea(IGeometry<BufferedCoordinate> geometry)
        {
            throw new System.NotImplementedException();
        }

        public IExtents<BufferedCoordinate> Intersection(IExtents<BufferedCoordinate> extents)
        {
            throw new System.NotImplementedException();
        }

        public IExtents<BufferedCoordinate> Intersection(IGeometry<BufferedCoordinate> geometry)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Intersects(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public BufferedCoordinate Max
        {
            get { throw new System.NotImplementedException(); }
        }

        public BufferedCoordinate Min
        {
            get { throw new System.NotImplementedException(); }
        }

        public Boolean Overlaps(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Overlaps(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IExtents<BufferedCoordinate>> Split(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public IGeometry<BufferedCoordinate> ToGeometry()
        {
            throw new System.NotImplementedException();
        }

        public Boolean Touches(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Touches(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Touches(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Touches(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public IExtents<BufferedCoordinate> Union(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public IExtents<BufferedCoordinate> Union(IExtents<BufferedCoordinate> box)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Within(BufferedCoordinate coordinate)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Within(IExtents<BufferedCoordinate> other)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Within(BufferedCoordinate coordinate, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public Boolean Within(IExtents<BufferedCoordinate> other, Tolerance tolerance)
        {
            throw new System.NotImplementedException();
        }

        public IGeometryFactory<BufferedCoordinate> Factory
        {
            get { return null; }
        }

        #endregion
    }
}
