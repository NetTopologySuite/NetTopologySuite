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
            BufferedCoordinate t = _factory.Homogenize(this);
            BufferedCoordinate o = _factory.Homogenize(vector);

            BufferedCoordinate r = _factory.Cross(t, o);
            return r;
            //return
            //return _factory.Dehomogenize( _factory.Cross(_factory.Homogenize(this), _factory.Homogenize(vector)));
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

        internal BufferedCoordinateFactory BufferedCoordinateFactory
        {
            get { return _factory; }
        }

        public ICoordinateFactory Factory
        {
            get
            {
                return _factory;
            }
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
                       : coordinate._factory.Create(coordinate[Ordinates.X] / coordinate[Ordinates.W], coordinate[Ordinates.Y] / coordinate[Ordinates.W]); new BufferedCoordinate(coordinate._factory, coordinate._id.Value, coordinate._hasZ, false);
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

        #endregion IBufferedVector<DoubleComponent> Members

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

        #endregion ICoordinate3D Members

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
            w = _hasZ ? w1 :
                _isHomogeneous ? w1 : w;
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c, out DoubleComponent d)
        {
            throw new System.NotImplementedException();
        }

        public Double Distance(ICoordinate2D other)
        {
            throw new NotImplementedException();
        }

        #endregion ICoordinate2D Members

        #region ICoordinate Members

        public Boolean ContainsOrdinate(Ordinates ordinates)
        {
            switch (ordinates)
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

        public double[] ToArray2D()
        {
            if (!_id.HasValue)
                return new[] { Double.NaN, Double.NaN };

            DoubleComponent x1, y1, w1;
            _factory.GetComponents(_id.Value, out x1, out y1, out w1);
            return new[] { (double)x1, (double)y1 };
        }

        public double[] ToArray(params Ordinates[] ordinates)
        {
            var res = new double[ordinates.Length];
            var i = 0;
            foreach (var ordinate in ordinates)
                res[i++] = this[ordinate];
            return res;
        }

        public Boolean IsEmpty
        {
            get { return _id == null; }
        }

        public Double this[Ordinates ordinates]
        {
            get
            {
                if (_id == null)
                    return Double.NaN;

                if (_hasZ)
                    return _factory.GetOrdinate(_id.Value, (Int32)ordinates);

                return _ordTable2D[(Int32)ordinates] < 0
                           ? Double.NaN
                           : _factory.GetOrdinate(_id.Value, _ordTable2D[(Int32)ordinates]);
            }
        }

        ICoordinate ICoordinate.Zero
        {
            get { return _factory.GetZero(); }
        }

        #endregion ICoordinate Members

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

        #endregion IEquatable<ICoordinate> Members

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

        #endregion IComparable<ICoordinate> Members

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

        #endregion IComparable<ICoordinate2D> Members

        #region IEquatable<ICoordinate2D> Members

        Boolean IEquatable<ICoordinate2D>.Equals(ICoordinate2D other)
        {
            if (other == null)
            {
                return false;
            }

            return other.X == X && other.Y == Y;
        }

        #endregion IEquatable<ICoordinate2D> Members

        #region IEquatable<BufferedCoordinate> Members

        public Boolean Equals(BufferedCoordinate other)
        {
            return _id == other._id && _factory == other._factory;
        }

        #endregion IEquatable<BufferedCoordinate> Members

        #region IComparable<BufferedCoordinate> Members

        public Int32 CompareTo(BufferedCoordinate other)
        {
            //jd: reinstated tests against empty coordinates as many unit tests rely on this
            if (_id == null && other._id == null)
                return 0;

            // Empty coordinates don't compare
            if (other._id == null)
            {
                return 1;
                //throw new ArgumentException("Cannot compare to the empty coordinate");
            }

            if (_id == null)
            {
                return -1;
                //throw new InvalidOperationException(
                //    "This coordinate is empty and cannot be compared");
            }

            // Since the coordinates are stored in lexicograpic order,
            // the index comparison works to compare coordinates
            // first by X, then by Y;
            return _factory.Compare(this, other);
        }

        #endregion IComparable<BufferedCoordinate> Members

        #region IComputable<BufferedCoordinate> Members

        public BufferedCoordinate Abs()
        {
            return _factory.Create(Math.Abs(X), Math.Abs(Y));
        }

        public BufferedCoordinate Set(Double value)
        {
            throw new NotSupportedException();
        }

        #endregion IComputable<BufferedCoordinate> Members

        #region INegatable<BufferedCoordinate> Members

        public BufferedCoordinate Negative()
        {
            return _factory.Create(-X, -Y);
        }

        #endregion INegatable<BufferedCoordinate> Members

        #region ISubtractable<BufferedCoordinate> Members

        public BufferedCoordinate Subtract(BufferedCoordinate b)
        {
            return Add(b.Negative());
        }

        #endregion ISubtractable<BufferedCoordinate> Members

        #region IHasZero<BufferedCoordinate> Members

        public BufferedCoordinate Zero
        {
            get { return _factory.GetZero(); }
        }

        #endregion IHasZero<BufferedCoordinate> Members

        #region IAddable<BufferedCoordinate> Members

        public BufferedCoordinate Add(BufferedCoordinate b)
        {
            return _factory.Add(this, b);
        }

        #endregion IAddable<BufferedCoordinate> Members

        #region IDivisible<BufferedCoordinate> Members

        public BufferedCoordinate Divide(BufferedCoordinate b)
        {
            throw new NotSupportedException();
            //return BufferedCoordinateFactory.Divide(this, b);
        }

        #endregion IDivisible<BufferedCoordinate> Members

        #region IDivisible<Double, BufferedCoordinate> Members

        public BufferedCoordinate Divide(Double b)
        {
            return _factory.Divide(this, b);
        }

        #endregion IDivisible<Double, BufferedCoordinate> Members

        #region IHasOne<BufferedCoordinate> Members

        public BufferedCoordinate One
        {
            get { return _factory.GetOne(); }
        }

        #endregion IHasOne<BufferedCoordinate> Members

        #region IMultipliable<BufferedCoordinate> Members

        public BufferedCoordinate Multiply(BufferedCoordinate b)
        {
            return _factory.Cross(this, b);
        }

        #endregion IMultipliable<BufferedCoordinate> Members

        #region IMultipliable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Multiply(Double b)
        {
            return _factory.Create(X * b, Y * b);
        }

        #endregion IMultipliable<Double,BufferedCoordinate> Members

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

        #endregion IBooleanComparable<BufferedCoordinate> Members

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

        #endregion IExponential<BufferedCoordinate> Members

        #region IComputable<Double,ICoordinate> Members

        ICoordinate IComputable<Double, ICoordinate>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<Double,ICoordinate> Members

        #region IComputable<ICoordinate> Members

        ICoordinate IComputable<ICoordinate>.Abs()
        {
            throw new NotImplementedException();
        }

        ICoordinate IComputable<ICoordinate>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<ICoordinate> Members

        #region INegatable<ICoordinate> Members

        ICoordinate INegatable<ICoordinate>.Negative()
        {
            throw new NotImplementedException();
        }

        #endregion INegatable<ICoordinate> Members

        #region ISubtractable<ICoordinate> Members

        ICoordinate ISubtractable<ICoordinate>.Subtract(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion ISubtractable<ICoordinate> Members

        #region IHasZero<ICoordinate> Members

        ICoordinate IHasZero<ICoordinate>.Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion IHasZero<ICoordinate> Members

        #region IAddable<ICoordinate> Members

        ICoordinate IAddable<ICoordinate>.Add(ICoordinate b)
        {
            if (b is BufferedCoordinate)
            {
                return Add((BufferedCoordinate)b);
            }

            throw new NotImplementedException();
        }

        #endregion IAddable<ICoordinate> Members

        #region IDivisible<ICoordinate> Members

        ICoordinate IDivisible<ICoordinate>.Divide(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<ICoordinate> Members

        #region IHasOne<ICoordinate> Members

        ICoordinate IHasOne<ICoordinate>.One
        {
            get { return One; }
        }

        #endregion IHasOne<ICoordinate> Members

        #region IMultipliable<ICoordinate> Members

        ICoordinate IMultipliable<ICoordinate>.Multiply(ICoordinate b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<ICoordinate> Members

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

        #endregion IBooleanComparable<ICoordinate> Members

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

        #endregion IExponential<ICoordinate> Members

        #region IMultipliable<Double,ICoordinate> Members

        ICoordinate IMultipliable<Double, ICoordinate>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<Double,ICoordinate> Members

        #region IDivisible<Double,ICoordinate> Members

        ICoordinate IDivisible<Double, ICoordinate>.Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<Double,ICoordinate> Members

        #region ICoordinate Members

        //ICoordinate ICoordinate.Divide(Double value)
        //{
        //    return Divide(value);
        //}

        #endregion ICoordinate Members

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

        #endregion IVectorD Members

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

        #endregion IMatrix<DoubleComponent> Members

        #region INegatable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> INegatable<IMatrix<DoubleComponent>>.Negative()
        {
            return Negative();
        }

        #endregion INegatable<IMatrix<DoubleComponent>> Members

        #region ISubtractable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> ISubtractable<IMatrix<DoubleComponent>>.Subtract(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion ISubtractable<IMatrix<DoubleComponent>> Members

        #region IHasZero<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IHasZero<IMatrix<DoubleComponent>>.Zero
        {
            get { return Zero; }
        }

        #endregion IHasZero<IMatrix<DoubleComponent>> Members

        #region IAddable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IAddable<IMatrix<DoubleComponent>>.Add(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion IAddable<IMatrix<DoubleComponent>> Members

        #region IDivisible<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IDivisible<IMatrix<DoubleComponent>>.Divide(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<IMatrix<DoubleComponent>> Members

        #region IHasOne<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IHasOne<IMatrix<DoubleComponent>>.One
        {
            get { return One; }
        }

        #endregion IHasOne<IMatrix<DoubleComponent>> Members

        #region IMultipliable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IMultipliable<IMatrix<DoubleComponent>>.Multiply(IMatrix<DoubleComponent> b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<IMatrix<DoubleComponent>> Members

        #region IEquatable<IMatrix<DoubleComponent>> Members

        Boolean IEquatable<IMatrix<DoubleComponent>>.Equals(IMatrix<DoubleComponent> other)
        {
            throw new NotImplementedException();
        }

        #endregion IEquatable<IMatrix<DoubleComponent>> Members

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

        #endregion IEnumerable<DoubleComponent> Members

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable Members

        #region INegatable<IVectorD> Members

        IVectorD INegatable<IVectorD>.Negative()
        {
            return _factory.Create(-X, -Y);
        }

        #endregion INegatable<IVectorD> Members

        #region ISubtractable<IVectorD> Members

        IVectorD ISubtractable<IVectorD>.Subtract(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion ISubtractable<IVectorD> Members

        #region IHasZero<IVectorD> Members

        IVectorD IHasZero<IVectorD>.Zero
        {
            get { return Zero; }
        }

        #endregion IHasZero<IVectorD> Members

        #region IAddable<IVectorD> Members

        IVectorD IAddable<IVectorD>.Add(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion IAddable<IVectorD> Members

        #region IDivisible<IVectorD> Members

        IVectorD IDivisible<IVectorD>.Divide(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<IVectorD> Members

        #region IHasOne<IVectorD> Members

        IVectorD IHasOne<IVectorD>.One
        {
            get { return One; }
        }

        #endregion IHasOne<IVectorD> Members

        #region IMultipliable<IVectorD> Members

        IVectorD IMultipliable<IVectorD>.Multiply(IVectorD b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<IVectorD> Members

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

        #endregion IConvertible Members

        #region IComparable<IMatrix<DoubleComponent>> Members

        Int32 IComparable<IMatrix<DoubleComponent>>.CompareTo(IMatrix<DoubleComponent> other)
        {
            throw new NotImplementedException();
        }

        #endregion IComparable<IMatrix<DoubleComponent>> Members

        #region IComputable<IMatrix<DoubleComponent>> Members

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Abs()
        {
            throw new NotImplementedException();
        }

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<IMatrix<DoubleComponent>> Members

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

        #endregion IBooleanComparable<IMatrix<DoubleComponent>> Members

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

        #endregion IExponential<IMatrix<DoubleComponent>> Members

        #region IComputable<IVectorD> Members

        IVectorD IComputable<IVectorD>.Abs()
        {
            throw new NotImplementedException();
        }

        IVectorD IComputable<Double, IVectorD>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<IVectorD> Members

        #region IComputable<IVectorD> Members

        IVectorD IComputable<IVectorD>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<IVectorD> Members

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

        #endregion IBooleanComparable<IVectorD> Members

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

        #endregion IExponential<IVectorD> Members

        #region IEquatable<IVectorD> Members

        Boolean IEquatable<IVectorD>.Equals(IVectorD other)
        {
            throw new NotImplementedException();
        }

        #endregion IEquatable<IVectorD> Members

        #region IComparable<IVectorD> Members

        Int32 IComparable<IVectorD>.CompareTo(IVectorD other)
        {
            throw new NotImplementedException();
        }

        #endregion IComparable<IVectorD> Members

        #region IDivisible<Double,IVectorD> Members

        IVectorD IDivisible<Double, IVectorD>.Divide(Double b)
        {
            return _factory.Divide(this, b);
        }

        #endregion IDivisible<Double,IVectorD> Members

        #region IMultipliable<Double,IVectorD> Members

        IVectorD IMultipliable<Double, IVectorD>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<Double,IVectorD> Members

        #region IAddable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Add(Double b)
        {
            return _factory.Add(this, b);
        }

        #endregion IAddable<Double,BufferedCoordinate> Members

        #region ISubtractable<Double,BufferedCoordinate> Members

        public BufferedCoordinate Subtract(Double b)
        {
            return _factory.Add(this, -b);
        }

        #endregion ISubtractable<Double,BufferedCoordinate> Members

        #region IAddable<Double,IVectorD> Members

        IVectorD IAddable<Double, IVectorD>.Add(Double b)
        {
            return Add(b);
        }

        #endregion IAddable<Double,IVectorD> Members

        #region ISubtractable<Double,IVectorD> Members

        IVectorD ISubtractable<Double, IVectorD>.Subtract(Double b)
        {
            return Subtract(b);
        }

        #endregion ISubtractable<Double,IVectorD> Members

        #region IAddable<Double, ICoordinate> Members

        ICoordinate IAddable<Double, ICoordinate>.Add(Double b)
        {
            return Add(b);
        }

        #endregion IAddable<Double, ICoordinate> Members

        #region ISubtractable<Double, ICoordinate> Members

        ICoordinate ISubtractable<Double, ICoordinate>.Subtract(Double b)
        {
            return Subtract(b);
        }

        #endregion ISubtractable<Double, ICoordinate> Members

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

        #endregion IVector2D Members

        #region IComputable<Double,IVector2D> Members

        IVector2D IComputable<Double, IVector2D>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<Double,IVector2D> Members

        #region IComputable<IVector2D> Members

        IVector2D IComputable<IVector2D>.Abs()
        {
            throw new NotImplementedException();
        }

        IVector2D IComputable<IVector2D>.Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion IComputable<IVector2D> Members

        #region INegatable<IVector2D> Members

        IVector2D INegatable<IVector2D>.Negative()
        {
            throw new NotImplementedException();
        }

        #endregion INegatable<IVector2D> Members

        #region ISubtractable<IVector2D> Members

        public IVector2D Subtract(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion ISubtractable<IVector2D> Members

        #region IHasZero<IVector2D> Members

        IVector2D IHasZero<IVector2D>.Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion IHasZero<IVector2D> Members

        #region IAddable<IVector2D> Members

        public IVector2D Add(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion IAddable<IVector2D> Members

        #region IDivisible<IVector2D> Members

        IVector2D IDivisible<IVector2D>.Divide(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<IVector2D> Members

        #region IHasOne<IVector2D> Members

        IVector2D IHasOne<IVector2D>.One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion IHasOne<IVector2D> Members

        #region IMultipliable<IVector2D> Members

        IVector2D IMultipliable<IVector2D>.Multiply(IVector2D b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<IVector2D> Members

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

        #endregion IBooleanComparable<IVector2D> Members

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

        #endregion IExponential<IVector2D> Members

        #region IAddable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IAddable<Double, IVector2D>.Add(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IAddable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        #region ISubtractable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D ISubtractable<Double, IVector2D>.Subtract(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion ISubtractable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        #region IMultipliable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IMultipliable<Double, IVector2D>.Multiply(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IMultipliable<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        #region IDivisible<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        IVector2D IDivisible<Double, IVector2D>.Divide(Double b)
        {
            throw new NotImplementedException();
        }

        #endregion IDivisible<Double, IVector<DoubleComponent, BufferedCoordinate>> Members

        #region IEquatable<IVector2D> Members

        Boolean IEquatable<IVector2D>.Equals(IVector2D other)
        {
            throw new NotImplementedException();
        }

        #endregion IEquatable<IVector2D> Members

        #region IComparable<IVector2D> Members

        Int32 IComparable<IVector2D>.CompareTo(IVector2D other)
        {
            throw new NotImplementedException();
        }

        #endregion IComparable<IVector2D> Members

        #region IVectorD Members

        IVectorD IVectorD.Clone()
        {
            return Clone();
        }

        #endregion IVectorD Members

        #region ICoordinate<BufferedCoordinate> Members

        public Double Distance(BufferedCoordinate other)
        {
            return _factory.Distance(this, other);
        }

        DoubleComponent ICoordinate<BufferedCoordinate>.this[Int32 index]
        {
            get { return this[index]; }
        }

        #endregion ICoordinate<BufferedCoordinate> Members

        private DoubleComponent[] getComponents()
        {
            DoubleComponent x, y, z;
            if (!_hasZ)
            {
                GetComponents(out x, out y);
                return new[] { x, y, (DoubleComponent)1 };
            }

            GetComponents(out x, out y, out z);
            return new[] { x, y, z, (DoubleComponent)1 };
        }

        #region IComparable<ICoordinate3D> Members

        public int CompareTo(ICoordinate3D other)
        {
            throw new NotImplementedException();
        }

        #endregion IComparable<ICoordinate3D> Members

        #region IEquatable<ICoordinate3D> Members

        public bool Equals(ICoordinate3D other)
        {
            throw new NotImplementedException();
        }

        #endregion IEquatable<ICoordinate3D> Members
    }
}