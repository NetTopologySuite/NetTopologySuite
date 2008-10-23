using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    using IVector2D = IVector<DoubleComponent, BufferedCoordinate>;
    using IVectorD = IVector<DoubleComponent>;

    public struct BufferedCoordinate : ICoordinate3D,
                                       ICoordinate<BufferedCoordinate>,
                                       IBufferedVector<DoubleComponent, BufferedCoordinate>,
                                       IEquatable<BufferedCoordinate>,
                                       IComparable<BufferedCoordinate>,
                                       IComputable<Double, BufferedCoordinate>
    {
        private readonly static Int32[] _ordTable2D = new Int32[] { 0, 1, -1, 2 };
        private readonly Int32? _id;
        private readonly BufferedCoordinateFactory _factory;
        private readonly Boolean _isHomogeneous;
        private readonly Boolean _hasZ;

        //internal BufferedCoordinate(BufferedCoordinateFactory factory, Int32 index, Boolean hasZ)
        //    : this(factory, index, hasZ, false) { }

        internal BufferedCoordinate(BufferedCoordinateFactory factory, Int32 index, Boolean hasZ, Boolean isHomogeneous)
        {
            _factory = factory;
            _id = index;
            _isHomogeneous = isHomogeneous;
            _hasZ = hasZ;
        }

        public BufferedCoordinate Clone()
        {
            return _factory.Create(this);
        }

        public Double Dot(BufferedCoordinate vector)
        {
            return _factory.Dot(this, vector);
        }

        public BufferedCoordinate Cross(BufferedCoordinate vector)
        {
            return _factory.Homogenize(_factory.Cross(this, vector));
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is BufferedCoordinate)
            {
                BufferedCoordinate other = (BufferedCoordinate)obj;

                return Equals(other);
            }

            ICoordinate2D coord2D = obj as ICoordinate2D;

            if (coord2D != null)
            {
                return ((ICoordinate2D)this).Equals(coord2D);
            }

            ICoordinate coord = obj as ICoordinate;

            if (coord != null)
            {
                return ((ICoordinate)this).Equals(coord);
            }

            IVectorD vector = obj as IVectorD;

            if (vector != null)
            {
                return ((IVectorD)this).Equals(coord);
            }

            IMatrix<DoubleComponent> matrix = obj as IMatrix<DoubleComponent>;

            if (matrix != null)
            {
                return ((IMatrix<DoubleComponent>)this).Equals(coord);
            }

            return false;
        }

        public override String ToString()
        {
            if (IsEmpty)
            {
                return "Empty";
            }

            return _isHomogeneous
                       ? (_hasZ
                              ? String.Format("({0}, {1}, {2}, {3})", X, Y, Z, W)
                              : String.Format("({0}, {1}, {2})", X, Y, W))
                       : (_hasZ
                              ? String.Format("({0}, {1}, {2})", X, Y, Z)
                              : String.Format("({0}, {1})", X, Y));
        }

        public override Int32 GetHashCode()
        {
            return _id.GetHashCode()
                ^ _isHomogeneous.GetHashCode()
                ^ _factory.GetHashCode();
        }

        internal BufferedCoordinateFactory Factory
        {
            get { return _factory; }
        }

        internal static BufferedCoordinate Homogenize(BufferedCoordinate coordinate)
        {
            return !coordinate._id.HasValue
                       ? coordinate
                       : new BufferedCoordinate(coordinate._factory, coordinate._id.Value, coordinate._hasZ, true);
        }

        internal static BufferedCoordinate Dehomogenize(BufferedCoordinate coordinate)
        {
            return !coordinate._id.HasValue
                       ? coordinate
                       : new BufferedCoordinate(coordinate._factory, coordinate._id.Value, coordinate._hasZ, false);
        }

        #region IBufferedVector<DoubleComponent> Members

        public IVectorBuffer<DoubleComponent, BufferedCoordinate> GetBuffer()
        {
            return _factory;
        }

        public Int32 Index
        {
            get { return _id.Value; }
        }

        public Boolean ValueEquals(BufferedCoordinate other)
        {
            return IsEmpty == other.IsEmpty &&
                   _isHomogeneous == other._isHomogeneous &&
                   this[Ordinates.X] == other[Ordinates.X] &&
                   this[Ordinates.Y] == other[Ordinates.Y];
        }

        #endregion

        #region ICoordinate3D Members

        public Double Z
        {
            get
            {
                return _id == null || !_hasZ
                    ? Double.NaN
                    : _factory.GetOrdinate(_id.Value, 2);
            }
        }

        public Double Distance(ICoordinate3D other)
        {
            throw new NotImplementedException();
        }

        public void GetComponents(out Double x, out Double y, out Double z, out Double w)
        {
            x = Double.NaN;
            y = Double.NaN;
            z = Double.NaN;
            w = 1.0;

            if (!_id.HasValue)
            {
                return;
            }

            DoubleComponent x1, y1, z1, w1;
            _factory.GetComponents(_id.Value, out x1, out y1, out z1, out w1);

            x = (Double)x1;
            y = (Double)y1;
            z = _hasZ ? (Double)z1 : z;
            w = _isHomogeneous ? (Double)w1 : w;
        }

        #endregion

        #region ICoordinate2D Members

        public Double X
        {
            get
            {
                return _id == null 
                    ? Double.NaN 
                    : _factory.GetOrdinate(_id.Value, 0);
            }
        }

        public Double Y
        {
            get
            {
                return _id == null
                    ? Double.NaN
                    : _factory.GetOrdinate(_id.Value, 1);
            }
        }

        public Double W
        {
            get
            {
                return _id == null || !_isHomogeneous
                    ? Double.NaN
                    : _factory.GetOrdinate(_id.Value, _hasZ ? 3 : 2);
            }
        }

        public void GetComponents(out Double x, out Double y, out Double w)
        {
            DoubleComponent a, b, c;

            GetComponents(out a, out b, out c);

            x = (Double)a;
            y = (Double)b;
            w = (Double)c;
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b)
        {
            DoubleComponent c;
            GetComponents(out a, out b, out c);
        }

        public void GetComponents(out DoubleComponent x, out DoubleComponent y, out DoubleComponent w)
        {
            x = Double.NaN;
            y = Double.NaN;
            w = 1.0;

            if (!_id.HasValue)
            {
                return;
            }

            DoubleComponent x1, y1, w1;
            _factory.GetComponents(_id.Value, out x1, out y1, out w1);

            x = x1;
            y = y1;
            w = _isHomogeneous ? w1 : w;
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c, out DoubleComponent d)
        {
            throw new System.NotImplementedException();
        }

        public Double Distance(ICoordinate2D other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinate Members

        public Boolean ContainsOrdinate(Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                case Ordinates.Y:
                    return true;
                case Ordinates.W:
                    return _isHomogeneous;
                case Ordinates.Z:
                    return _hasZ;
                default:
                    return false;
            }
        }

        public Double Distance(ICoordinate other)
        {
            return Distance(_factory.Create(other));
        }

        public Boolean IsEmpty
        {
            get { return _id == null; }
        }

        public Double this[Ordinates ordinate]
        {
            get
            {
                return _id == null
                    ? Double.NaN
                    : _factory.GetOrdinate(_id.Value, _hasZ ? (Int32)ordinate : _ordTable2D[(Int32)ordinate]);
            }
        }

        ICoordinate ICoordinate.Zero
        {
            get { return _factory.GetZero(); }
        }

        #endregion

        #region IEquatable<ICoordinate> Members

        Boolean IEquatable<ICoordinate>.Equals(ICoordinate other)
        {
            if (other is BufferedCoordinate)
            {
                return Equals((BufferedCoordinate)other);
            }

            if (other == null)
            {
                return false;
            }

            return other[Ordinates.X] == this[Ordinates.X]
                && other[Ordinates.Y] == this[Ordinates.Y];
        }

        #endregion

        #region IComparable<ICoordinate> Members

        Int32 IComparable<ICoordinate>.CompareTo(ICoordinate other)
        {
            if (other == null)
            {
                return 1;
            }

            if (other.ComponentCount > ComponentCount)
            {
                return -1;
            }

            Int32 compare = X.CompareTo(other[Ordinates.X]);

            if (compare == 0)
            {
                compare = Y.CompareTo(other[Ordinates.Y]);
            }

            return compare;
        }

        #endregion

        #region IComparable<ICoordinate2D> Members

        Int32 IComparable<ICoordinate2D>.CompareTo(ICoordinate2D other)
        {
            if (other == null)
            {
                return 1;
            }

            Int32 compare = X.CompareTo(other.X);

            if (compare == 0)
            {
                compare = Y.CompareTo(other.Y);
            }

            return compare;
        }

        #endregion

        #region IEquatable<ICoordinate2D> Members

        Boolean IEquatable<ICoordinate2D>.Equals(ICoordinate2D other)
        {
            if (other == null)
            {
                return false;
            }

            return other.X == X && other.Y == Y;
        }

        #endregion

        #region IEquatable<BufferedCoordinate> Members

        public Boolean Equals(BufferedCoordinate other)
        {
            return _id == other._id && _factory == other._factory;
        }

        #endregion

        #region IComparable<BufferedCoordinate> Members

        public Int32 CompareTo(BufferedCoordinate other)
        {
            // Empty coordinates don't compare
            if (other._id == null)
            {
                throw new ArgumentException("Cannot compare to the empty coordinate");
            }

            if (_id == null)
            {
                throw new InvalidOperationException(
                    "This coordinate is empty and cannot be compared");
            }

            // Since the coordinates are stored in lexicograpic order,
            // the index comparison works to compare coordinates
            // first by X, then by Y;
            return _factory.Compare(this, other);
        }

        #endregion

        #region IComputable<BufferedCoordinate> Members

        public BufferedCoordinate Abs()
        {
            return _factory.Create(Math.Abs(X), Math.Abs(Y));
        }

        public BufferedCoordinate Set(Double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INegatable<BufferedCoordinate> Members

        public BufferedCoordinate Negative()
        {
            return _factory.Create(-X, -Y);
        }

        #endregion

        #region ISubtractable<BufferedCoordinate> Members

        public BufferedCoordinate Subtract(BufferedCoordinate b)
        {
            return Add(b.Negative());
        }

        #endregion

        #region IHasZero<BufferedCoordinate> Members

        public BufferedCoordinate Zero
        {
            get { return _factory.GetZero(); }
        }

        #endregion

        #region IAddable<BufferedCoordinate> Members

        public BufferedCoordinate Add(BufferedCoordinate b)
        {
            return _factory.Add(this, b);
        }

        #endregion

        #region IDivisible<BufferedCoordinate> Members

        public BufferedCoordinate Divide(BufferedCoordinate b)
        {
            throw new NotSupportedException();
            //return BufferedCoordinateFactory.Divide(this, b);
        }

        #endregion

        #region IDivisible<Double, BufferedCoordinate> Members

        public BufferedCoordinate Divide(Double b)
        {
            return _factory.Divide(this, b);
        }

        #endregion

        #region IHasOne<BufferedCoordinate> Members

        public BufferedCoordinate One
        {
            get { return _factory.GetOne(); }
        }

        #endregion

        #region IMultipliable<BufferedCoordinate> Members

        public BufferedCoordinate Multiply(BufferedCoordinate b)
        {
            return _factory.Cross(this, b);
        }

        #endregion

        #region IMultipliable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Multiply(Double b)
        {
            return _factory.Create(X * b, Y * b);
        }

        #endregion

        #region IBooleanComparable<BufferedCoordinate> Members

        public Boolean GreaterThan(BufferedCoordinate value)
        {
            return _factory.GreaterThan(this, value);
        }

        public Boolean GreaterThanOrEqualTo(BufferedCoordinate value)
        {
            return _factory.GreaterThanOrEqualTo(this, value);
        }

        public Boolean LessThan(BufferedCoordinate value)
        {
            return _factory.LessThan(this, value);
        }

        public Boolean LessThanOrEqualTo(BufferedCoordinate value)
        {
            return _factory.LessThanOrEqualTo(this, value);
        }

        #endregion

        #region IExponential<BufferedCoordinate> Members

        public BufferedCoordinate Exp()
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate Log()
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<Double,ICoordinate> Members

        ICoordinate IComputable<Double, ICoordinate>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<ICoordinate> Members

        ICoordinate IComputable<ICoordinate>.Abs()
        {
            throw new NotImplementedException();
        }

        ICoordinate IComputable<ICoordinate>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<ICoordinate> Members

        ICoordinate INegatable<ICoordinate>.Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<ICoordinate> Members

        ICoordinate ISubtractable<ICoordinate>.Subtract(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<ICoordinate> Members

        ICoordinate IHasZero<ICoordinate>.Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAddable<ICoordinate> Members

        ICoordinate IAddable<ICoordinate>.Add(ICoordinate b)
        {
            if (b is BufferedCoordinate)
            {
                return Add((BufferedCoordinate)b);
            }

            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<ICoordinate> Members

        ICoordinate IDivisible<ICoordinate>.Divide(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<ICoordinate> Members

        ICoordinate IHasOne<ICoordinate>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<ICoordinate> Members

        ICoordinate IMultipliable<ICoordinate>.Multiply(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<ICoordinate> Members

        bool IBooleanComparable<ICoordinate>.GreaterThan(ICoordinate value)
        {
            throw new NotImplementedException();
        }

        bool IBooleanComparable<ICoordinate>.GreaterThanOrEqualTo(ICoordinate value)
        {
            throw new NotImplementedException();
        }

        bool IBooleanComparable<ICoordinate>.LessThan(ICoordinate value)
        {
            throw new NotImplementedException();
        }

        bool IBooleanComparable<ICoordinate>.LessThanOrEqualTo(ICoordinate value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<ICoordinate> Members

        ICoordinate IExponential<ICoordinate>.Exp()
        {
            throw new NotImplementedException();
        }

        ICoordinate IExponential<ICoordinate>.Log()
        {
            throw new NotImplementedException();
        }

        ICoordinate IExponential<ICoordinate>.Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        ICoordinate IExponential<ICoordinate>.Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        ICoordinate IExponential<ICoordinate>.Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMultipliable<Double,ICoordinate> Members

        ICoordinate IMultipliable<Double, ICoordinate>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Double,ICoordinate> Members

        ICoordinate IDivisible<Double, ICoordinate>.Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinate Members

        //ICoordinate ICoordinate.Divide(Double value)
        //{
        //    return Divide(value);
        //}

        #endregion

        #region IVectorD Members

        public Int32 ComponentCount
        {
            get { return _isHomogeneous ? (_hasZ ? 4 : 3) : (_hasZ ? 3 : 2); }
        }

        DoubleComponent[] IVectorD.Components
        {
            get
            {
                return getComponents();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        IVectorD IVectorD.Negative()
        {
            return Negative();
        }

        DoubleComponent IVectorD.this[Int32 index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = value;
            }
        }

        #endregion

        #region IMatrix<DoubleComponent> Members

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Clone()
        {
            return ((IVectorD)this).Clone();
        }

        Int32 IMatrix<DoubleComponent>.ColumnCount
        {
            get { throw new NotImplementedException(); }
        }

        Double IMatrix<DoubleComponent>.Determinant
        {
            get { throw new NotImplementedException(); }
        }

        MatrixFormat IMatrix<DoubleComponent>.Format
        {
            get { throw new NotImplementedException(); }
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.GetMatrix(Int32[] rowIndexes, Int32 startColumn, Int32 endColumn)
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Inverse
        {
            get { throw new InvalidOperationException("Inverse doesn't exist for this matrix."); }
        }

        Boolean IMatrix<DoubleComponent>.IsInvertible
        {
            get { return false; }
        }

        Boolean IMatrix<DoubleComponent>.IsSingular
        {
            get { return true; }
        }

        Boolean IMatrix<DoubleComponent>.IsSquare
        {
            get { return false; }
        }

        Boolean IMatrix<DoubleComponent>.IsSymmetrical
        {
            get { return false; }
        }

        Int32 IMatrix<DoubleComponent>.RowCount
        {
            get { throw new NotImplementedException(); }
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Transpose()
        {
            throw new NotImplementedException();
        }

        DoubleComponent IMatrix<DoubleComponent>.this[Int32 row, Int32 column]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region INegatable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> INegatable<IMatrix<DoubleComponent>>.Negative()
        {
            return Negative();
        }

        #endregion

        #region ISubtractable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> ISubtractable<IMatrix<DoubleComponent>>.Subtract(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IHasZero<IMatrix<DoubleComponent>>.Zero
        {
            get { return Zero; }
        }

        #endregion

        #region IAddable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IAddable<IMatrix<DoubleComponent>>.Add(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IDivisible<IMatrix<DoubleComponent>>.Divide(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IHasOne<IMatrix<DoubleComponent>>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IMultipliable<IMatrix<DoubleComponent>>.Multiply(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<IMatrix<DoubleComponent>> Members

        Boolean IEquatable<IMatrix<DoubleComponent>>.Equals(IMatrix<DoubleComponent> other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<DoubleComponent> Members

        public IEnumerator<DoubleComponent> GetEnumerator()
        {
            yield return X;
            yield return Y;

            if (_isHomogeneous)
            {
                yield return this[Ordinates.W];
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INegatable<IVectorD> Members

        IVectorD INegatable<IVectorD>.Negative()
        {
            return _factory.Create(-X, -Y);
        }

        #endregion

        #region ISubtractable<IVectorD> Members

        IVectorD ISubtractable<IVectorD>.Subtract(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<IVectorD> Members

        IVectorD IHasZero<IVectorD>.Zero
        {
            get { return Zero; }
        }

        #endregion

        #region IAddable<IVectorD> Members

        IVectorD IAddable<IVectorD>.Add(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<IVectorD> Members

        IVectorD IDivisible<IVectorD>.Divide(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<IVectorD> Members

        IVectorD IHasOne<IVectorD>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<IVectorD> Members

        IVectorD IMultipliable<IVectorD>.Multiply(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IConvertible Members

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        Boolean IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Int16 IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Int32 IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Int64 IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        SByte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Single IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        String IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        Object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        UInt16 IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        UInt32 IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        UInt64 IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IMatrix<DoubleComponent>> Members

        Int32 IComparable<IMatrix<DoubleComponent>>.CompareTo(IMatrix<DoubleComponent> other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Abs()
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<IMatrix<DoubleComponent>> Members

        Boolean IBooleanComparable<IMatrix<DoubleComponent>>.GreaterThan(IMatrix<DoubleComponent> value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IMatrix<DoubleComponent>>.GreaterThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IMatrix<DoubleComponent>>.LessThan(IMatrix<DoubleComponent> value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IMatrix<DoubleComponent>>.LessThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Exp()
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Log()
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<IVectorD> Members

        IVectorD IComputable<IVectorD>.Abs()
        {
            throw new NotImplementedException();
        }

        IVectorD IComputable<Double, IVectorD>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<IVectorD> Members

        IVectorD IComputable<IVectorD>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<IVectorD> Members

        Boolean IBooleanComparable<IVectorD>.GreaterThan(IVectorD value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVectorD>.GreaterThanOrEqualTo(IVectorD value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVectorD>.LessThan(IVectorD value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVectorD>.LessThanOrEqualTo(IVectorD value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<IVectorD> Members

        IVectorD IExponential<IVectorD>.Exp()
        {
            throw new NotImplementedException();
        }

        IVectorD IExponential<IVectorD>.Log()
        {
            throw new NotImplementedException();
        }

        IVectorD IExponential<IVectorD>.Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        IVectorD IExponential<IVectorD>.Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        IVectorD IExponential<IVectorD>.Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<IVectorD> Members

        Boolean IEquatable<IVectorD>.Equals(IVectorD other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IVectorD> Members

        Int32 IComparable<IVectorD>.CompareTo(IVectorD other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Double,IVectorD> Members

        IVectorD IDivisible<Double, IVectorD>.Divide(Double b)
        {
            return _factory.Divide(this, b);
        }

        #endregion

        #region IMultipliable<Double,IVectorD> Members

        IVectorD IMultipliable<Double, IVectorD>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAddable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Add(Double b)
        {
            return _factory.Add(this, b);
        }

        #endregion

        #region ISubtractable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Subtract(Double b)
        {
            return _factory.Add(this, -b);
        }

        #endregion

        #region IAddable<Double,IVectorD> Members

        IVectorD IAddable<Double, IVectorD>.Add(Double b)
        {
            return Add(b);
        }

        #endregion

        #region ISubtractable<Double,IVectorD> Members

        IVectorD ISubtractable<Double, IVectorD>.Subtract(Double b)
        {
            return Subtract(b);
        }

        #endregion

        #region IAddable<Double, ICoordinate> Members

        ICoordinate IAddable<Double, ICoordinate>.Add(Double b)
        {
            return Add(b);
        }

        #endregion

        #region ISubtractable<Double, ICoordinate> Members

        ICoordinate ISubtractable<Double, ICoordinate>.Subtract(Double b)
        {
            return Subtract(b);
        }

        #endregion

        #region IVector2D Members

        DoubleComponent[] IVector2D.Components
        {
            get
            {
                return getComponents();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public DoubleComponent this[Int32 index]
        {
            get
            {
                return _id == null
                    ? Double.NaN
                    : _factory.GetOrdinate(_id.Value, index);
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region IComputable<Double,IVector2D> Members

        IVector2D IComputable<Double, IVector2D>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<IVector2D> Members

        IVector2D IComputable<IVector2D>.Abs()
        {
            throw new NotImplementedException();
        }

        IVector2D IComputable<IVector2D>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<IVector2D> Members

        IVector2D INegatable<IVector2D>.Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<IVector2D> Members

        public IVector2D Subtract(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<IVector2D> Members

        IVector2D IHasZero<IVector2D>.Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAddable<IVector2D> Members

        public IVector2D Add(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<IVector2D> Members

        IVector2D IDivisible<IVector2D>.Divide(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<IVector2D> Members

        IVector2D IHasOne<IVector2D>.One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IMultipliable<IVector2D> Members

        IVector2D IMultipliable<IVector2D>.Multiply(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<IVector2D> Members

        Boolean IBooleanComparable<IVector2D>.GreaterThan(IVector2D value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVector2D>.GreaterThanOrEqualTo(IVector2D value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVector2D>.LessThan(IVector2D value)
        {
            throw new NotImplementedException();
        }

        Boolean IBooleanComparable<IVector2D>.LessThanOrEqualTo(IVector2D value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<IVector2D> Members

        IVector2D IExponential<IVector2D>.Exp()
        {
            throw new NotImplementedException();
        }

        IVector2D IExponential<IVector2D>.Log()
        {
            throw new NotImplementedException();
        }

        IVector2D IExponential<IVector2D>.Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        IVector2D IExponential<IVector2D>.Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        IVector2D IExponential<IVector2D>.Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IAddable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IAddable<Double, IVector2D>.Add(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D ISubtractable<Double, IVector2D>.Subtract(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMultipliable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IMultipliable<Double, IVector2D>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IDivisible<Double, IVector2D>.Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<IVector2D> Members

        Boolean IEquatable<IVector2D>.Equals(IVector2D other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<IVector2D> Members

        Int32 IComparable<IVector2D>.CompareTo(IVector2D other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVectorD Members

        IVectorD IVectorD.Clone()
        {
            return Clone();
        }

        #endregion

        #region ICoordinate<BufferedCoordinate> Members

        public Double Distance(BufferedCoordinate other)
        {
            return _factory.Distance(this, other);
        }

        DoubleComponent ICoordinate<BufferedCoordinate>.this[Int32 index]
        {
            get { return this[index]; }
        }

        #endregion

        private DoubleComponent[] getComponents()
        {
            return new DoubleComponent[] { X, Y };
        }

        #region IComparable<ICoordinate3D> Members

        public int CompareTo(ICoordinate3D other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEquatable<ICoordinate3D> Members

        public bool Equals(ICoordinate3D other)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
