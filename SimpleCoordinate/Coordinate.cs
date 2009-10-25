#region License

/*
 *  The attached / following is part of NetTopologySuite.Coordinates.Simple.
 *  
 *  NetTopologySuite.Coordinates.Simple is free software ? 2009 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  This work was derived from NetTopologySuite.Coordinates.ManagedBufferedCoordinate
 *  by codekaizen
 *  
 *  Author: Felix Obermaier 2009
 *  
 */

#endregion
#define class
using System;
using System.Globalization;
using System.Text;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates.Simple
{
    using IVector2D = IVector<DoubleComponent, Coordinate>;
    using IVectorD = IVector<DoubleComponent>;

    [Flags]
    public enum OrdinateFlags
    {
        None = 0,
        X = 1,
        Y = 2,
        XY = 3,
        Z = 4,
        XYZ = 7,
        M = 8,
        XYM = 11,
        XYZM = 15,
        W = 16,
        XYW = 19,
        XYZW = 23,
        XYMW = 27,
        XYZMW = 31
    }

#if class
    public class Coordinate : ICoordinate3DM,
#else
    public struct Coordinate : ICoordinate3DM,
#endif
                              ICoordinate<Coordinate>,
                              IEquatable<Coordinate>,
                              IComparable<Coordinate>,
                              IComputable<Double, Coordinate>
    {

        public const double CoordinateNullValue = double.NaN;

        private static Boolean IsZ(OrdinateFlags flags)
        {
            return ((flags & OrdinateFlags.Z) == OrdinateFlags.Z);
        }

        private static Boolean IsM(OrdinateFlags flags)
        {
            return ((flags & OrdinateFlags.M) == OrdinateFlags.M);
        }

        private static Boolean IsW(OrdinateFlags flags)
        {
            return ((flags & OrdinateFlags.W) == OrdinateFlags.W);
        }

        private readonly OrdinateFlags _flags;
        private readonly CoordinateFactory _coordFactory;
        private readonly Double _x;
        private readonly Double _y;
        private readonly Double _z;
        private readonly Double _m;
        private readonly Double _w;

#if class
        public Coordinate()
        {
            _coordFactory = null;
            _flags = OrdinateFlags.None;
            _x = CoordinateNullValue;
            _y = CoordinateNullValue;
            _z = CoordinateNullValue;
            _m = CoordinateNullValue;
            _w = CoordinateNullValue;
        }
#endif
        internal Coordinate(CoordinateFactory factory, Double x, Double y)
        {
            IPrecisionModel pm = factory.PrecisionModel;
            _coordFactory = factory;
            _x = pm.MakePrecise(x);
            _y = pm.MakePrecise(y);
            _z = CoordinateNullValue;
            _w = CoordinateNullValue;
            _m = CoordinateNullValue;
            _flags = OrdinateFlags.XY;
        }

        internal Coordinate(CoordinateFactory factory, Double x, Double y, Double zmw, OrdinateFlags flags)
        {
            IPrecisionModel pm = factory.PrecisionModel;
            _coordFactory = factory;
            _x = pm.MakePrecise(x);
            _y = pm.MakePrecise(y);
            if (IsW(flags))
            {
                _w = zmw;
                _z = CoordinateNullValue;
                _m = CoordinateNullValue;
            }
            else if(IsZ(flags))
            {
                _w = CoordinateNullValue;
                _z = pm.MakePrecise(zmw);
                _m = CoordinateNullValue;
            }
            else
            {
                _w = CoordinateNullValue;
                _z = CoordinateNullValue;
                _m = zmw;
            }
            _flags = OrdinateFlags.XY | flags;
        }

        internal Coordinate(CoordinateFactory factory, Double x, Double y, Double z, Double mw, OrdinateFlags flags)
        {
            IPrecisionModel pm = factory.PrecisionModel;
            _coordFactory = factory;
            _x = pm.MakePrecise(x);
            _y = pm.MakePrecise(y);
            _z = z; // MD says its safe not to makeprecise z ordinates
            if (IsW(flags))
            {
                _w = mw;
                _m = CoordinateNullValue;
            }
            else
            {
                _w = CoordinateNullValue;
                _m = mw;
            }
            _flags = OrdinateFlags.XYZ | flags;
        }


        internal Coordinate(CoordinateFactory factory, Coordinate coordinate)
        {
            IPrecisionModel pm = factory.PrecisionModel;
            _coordFactory = factory;
            _x = pm.MakePrecise(coordinate._x);
            _y = pm.MakePrecise(coordinate._y);
            _z = coordinate._z; // MD says its safe not to makeprecise z ordinates
            _m = coordinate._m;
            _w = coordinate._w;
            _flags = coordinate._flags;
        }

        internal Coordinate(CoordinateFactory factory, Double x, Double y, Double z, Double m, Double w)
        {
            IPrecisionModel pm = factory.PrecisionModel;
            _coordFactory = factory;
            _x = pm.MakePrecise(x);
            _y = pm.MakePrecise(y);
            _z = z; // MD says its safe not to makeprecise z ordinates
            _m = m;
            _w = w;
            _flags = OrdinateFlags.XY |
                     (Double.IsNaN(z) ? OrdinateFlags.None : OrdinateFlags.Z) |
                     (Double.IsNaN(m) ? OrdinateFlags.None : OrdinateFlags.M) |
                     (Double.IsNaN(w) ? OrdinateFlags.None : OrdinateFlags.W);
        }

        private DoubleComponent[] getComponents()
        {
            DoubleComponent x, y, z;
            if (!HasZ)
            {
                GetComponents(out x, out y);
                return new[] { x, y, (DoubleComponent)1 };
            }

            GetComponents(out x, out y, out z);
            return new[] { x, y, z, (DoubleComponent)1 };
        }

        public static Coordinate Clone(Coordinate coordinate)
        {
            return new Coordinate(coordinate._coordFactory, coordinate);
        }

        public Double Dot(Coordinate vector)
        {
            return _coordFactory.Dot(this, vector);
        }

        public Coordinate Cross(Coordinate vector)
        {
            return _coordFactory.Homogenize(_coordFactory.Cross(this, vector));
        }

        public Boolean ValueEquals(Coordinate other)
        {
            Boolean e = IsEmpty && other.IsEmpty;
            if (e) return true;

            Boolean xy =  _x == other._x &&
                          _y == other._y;

            if (!xy)
                return false;

            Boolean z = (HasZ || other.HasZ) ? _z == other._z : true;
            Boolean m = (HasM || other.HasM) ? _m == other._m : true;
            Boolean w = (HasW || other.HasW) ? _w == other._w : true;

            return z && m && w;

        }


        public Coordinate One
        {
            get { return _coordFactory.GetOne(); }
        }

        public Coordinate Zero
        {
            get { return _coordFactory.GetZero(); }
        }

        public override Boolean Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Coordinate)
            {
                Coordinate other = (Coordinate)obj;

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

            StringBuilder sb = new StringBuilder(50);
            sb.AppendFormat(CultureInfo.InvariantCulture,"{0} {1}", _x, _y);
            if (HasZ) sb.AppendFormat(CultureInfo.InvariantCulture," {0}", _z);
            if (HasW) sb.AppendFormat(CultureInfo.InvariantCulture," W:{0}", _w);
            if (HasM) sb.AppendFormat(CultureInfo.InvariantCulture," M:{0}", _m);

            return sb.ToString();
        }

        private Boolean HasZ
        { get { return IsZ(_flags); }}

        private Boolean HasM
        { get { return IsM(_flags); } }

        internal Boolean HasW
        { get { return IsW(_flags); } }

        public override Int32 GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode() ^
                   _m.GetHashCode() ^ _w.GetHashCode() ^ HasW.GetHashCode();
        }

        internal CoordinateFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public ICoordinateFactory Factory
        {
            get
            {
                return _coordFactory;
            }
        }


        #region ICoordinate3D Member

        public double Z
        {
            get { return _z; }
        }

        public Double Distance(ICoordinate3D other)
        {
            return Distance(_coordFactory.Create(other));
        }

        public void GetComponents(out double x, out double y, out double z, out double w)
        {
            if (_coordFactory != null)
            {
                x = _x;
                y = _y;
                z = _z;
                w = _w;
                return;
            }

            x = Double.NaN;
            y = Double.NaN;
            z = Double.NaN;
            w = 1d;

        }

        #endregion

        #region ICoordinate2D Member

        public Double X
        {
            get { return IsEmpty ? CoordinateNullValue : _x; }
        }

        public Double Y
        {
            get { return IsEmpty ? CoordinateNullValue : _y; }
        }

        public Double W
        {
            get { return HasW ? _w : 1.0; }
        }

        public void GetComponents(out double x, out double y, out double w)
        {
            DoubleComponent a, b;

            GetComponents(out a, out b);

            x = (Double)a;
            y = (Double)b;
            w = 1d;
        }

        #endregion

        #region ICoordinate Member

        public bool ContainsOrdinate(Ordinates ordinate)
        {
            switch (ordinate)
            {
                case Ordinates.X:
                case Ordinates.Y:
                    return true;
                case Ordinates.W:
                    return HasW;
                case Ordinates.Z:
                    return HasZ;
                case Ordinates.M:
                    return HasM;
                default:
                    return false;
            }
        }

        public Double Distance(ICoordinate other)
        {
            return Distance(_coordFactory.Create(other));
        }

        public Double this[Ordinates ordinate]
        {
            get
            {
                switch (ordinate)
                {
                    case Ordinates.X:
                        return _x;
                    case Ordinates.Y:
                        return _y;
                    case Ordinates.W:
                        return HasW ? _w : 1.0;
                    case Ordinates.Z:
                        return _z;
                    case Ordinates.M:
                        return _m;
                    default:
                        return CoordinateNullValue;
                }
            }
        }

        public bool IsEmpty
        {
            get { return _coordFactory == null; }
        }

        #endregion

        #region IVector<DoubleComponent> Member

        public IVector<DoubleComponent> Clone()
        {
            return Clone(this);
        }

        public int ComponentCount
        {
            get { return 2 + (HasZ ? 1 : 0) + (HasM ? 1 : 0) + (HasW ? 1 : 0); }
        }

        public DoubleComponent[] Components
        {
            get
            {
                DoubleComponent[] ret = new DoubleComponent[ComponentCount];
                Int32 index = 0;
                ret[index++] = _x;
                ret[index++] = _y;
                if (HasZ) ret[index++] = _z;
                if (HasW) ret[index++] = W;
                if (HasM) ret[index] = _m;
                return ret;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public IVector<DoubleComponent> Negative()
        {
            throw new NotSupportedException();
        }

        public DoubleComponent this[int index]
        {
            get
            {
                return this[IndexToOrdinate(index)];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b)
        {
            DoubleComponent c;
            GetComponents(out a, out b, out c);
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c)
        {

            Double w, x, y, z;
            GetComponents(out x, out y, out z, out w);
            a = x;
            b = y;
            c = HasZ ? z : w;
        }

        public void GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c, out DoubleComponent d)
        {
            Double w, x, y, z;
            GetComponents(out x, out y, out z, out w);
            a = x;
            b = y;
            c = z;
            d = w;
        }

        #endregion

        #region IMatrix<DoubleComponent> Member

        public double Determinant
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public int ColumnCount
        {
            get { throw new NotSupportedException(); }
        }

        public MatrixFormat Format
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSingular
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsInvertible
        {
            get { throw new NotSupportedException(); }
        }

        public IMatrix<DoubleComponent> Inverse
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSquare
        {
            get { throw new NotSupportedException(); }
        }

        public bool IsSymmetrical
        {
            get { throw new NotSupportedException(); }
        }

        public int RowCount
        {
            get { throw new NotSupportedException(); }
        }

        public DoubleComponent this[int row, int column]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Clone()
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> GetMatrix(int[] rowIndexes, int startColumn, int endColumn)
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Transpose()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<IMatrix<DoubleComponent>> Member

        public bool Equals(IMatrix<DoubleComponent> other)
        {
            throw new NotSupportedException();
            //if (other is Coordinate)
            //{
            //    return Equals((Coordinate)other);
            //}

            //if (other == null)
            //{
            //    return false;
            //}

            //return other[Ordinates.X] == this[Ordinates.X]
            //    && other[Ordinates.Y] == this[Ordinates.Y];
        }

        #endregion

        #region IComparable<IMatrix<DoubleComponent>> Member

        public int CompareTo(IMatrix<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Abs()
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INegatable<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> INegatable<IMatrix<DoubleComponent>>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Subtract(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasZero<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IHasZero<IMatrix<DoubleComponent>>.Zero
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IAddable<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Add(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Divide(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Multiply(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<IMatrix<DoubleComponent>> Member

        public bool GreaterThan(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool GreaterThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThan(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IExponential<IMatrix<DoubleComponent>> Member

        public IMatrix<DoubleComponent> Power(double exponent)
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Sqrt()
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Log(double newBase)
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Log()
        {
            throw new NotSupportedException();
        }

        public IMatrix<DoubleComponent> Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<DoubleComponent> Member

        public System.Collections.Generic.IEnumerator<DoubleComponent> GetEnumerator()
        {
           foreach(DoubleComponent c in Components)
               yield return c;
        }

        #endregion

        #region IEnumerable Member

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<double,IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IComputable<double, IVector<DoubleComponent>>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IComputable<IVector<DoubleComponent>>.Abs()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent> IComputable<IVector<DoubleComponent>>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Subtract(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasZero<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IHasZero<IVector<DoubleComponent>>.Zero
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IAddable<IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Add(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Divide(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IHasOne<IVector<DoubleComponent>>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Multiply(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<IVector<DoubleComponent>> Member

        public bool GreaterThan(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool GreaterThanOrEqualTo(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThan(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThanOrEqualTo(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IExponential<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IExponential<IVector<DoubleComponent>>.Power(double exponent)
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent> IExponential<IVector<DoubleComponent>>.Sqrt()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent> IExponential<IVector<DoubleComponent>>.Log(double newBase)
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent> IExponential<IVector<DoubleComponent>>.Log()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent> IExponential<IVector<DoubleComponent>>.Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<double,IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Add(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<double,IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Subtract(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<double,IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Multiply(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<double,IVector<DoubleComponent>> Member

        public IVector<DoubleComponent> Divide(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<IVector<DoubleComponent>> Member

        public bool Equals(IVector<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<IVector<DoubleComponent>> Member

        public int CompareTo(IVector<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate> Member

        public int CompareTo(ICoordinate other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate> Member

        public bool Equals(ICoordinate other)
        {
            if (other is Coordinate)
            {
                return Equals((Coordinate)other);
            }

            if (other == null)
            {
                return false;
            }

            return other[Ordinates.X] == this[Ordinates.X]
                && other[Ordinates.Y] == this[Ordinates.Y];
        }

        #endregion

        #region IConvertible Member

        public TypeCode GetTypeCode()
        {
            throw new NotSupportedException();
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public string ToString(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<double,ICoordinate> Member

        ICoordinate IComputable<Double, ICoordinate>.Set(Double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<ICoordinate> Member

        ICoordinate IComputable<ICoordinate>.Abs()
        {
            throw new NotSupportedException();
        }

        ICoordinate IComputable<ICoordinate>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INegatable<ICoordinate> Member

        ICoordinate INegatable<ICoordinate>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<ICoordinate> Member

        public ICoordinate Subtract(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<ICoordinate> Member

        public ICoordinate Add(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<ICoordinate> Member

        public ICoordinate Divide(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<ICoordinate> Member

        ICoordinate IHasOne<ICoordinate>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<ICoordinate> Member

        public ICoordinate Multiply(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<ICoordinate> Member

        public bool GreaterThan(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        public bool GreaterThanOrEqualTo(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        public bool LessThan(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        public bool LessThanOrEqualTo(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IExponential<ICoordinate> Member

        ICoordinate IExponential<ICoordinate>.Power(double exponent)
        {
            throw new NotSupportedException();
        }

        ICoordinate IExponential<ICoordinate>.Sqrt()
        {
            throw new NotSupportedException();
        }

        ICoordinate IExponential<ICoordinate>.Log(double newBase)
        {
            throw new NotSupportedException();
        }

        ICoordinate IExponential<ICoordinate>.Log()
        {
            throw new NotSupportedException();
        }

        ICoordinate IExponential<ICoordinate>.Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<double,ICoordinate> Member

        ICoordinate IAddable<double, ICoordinate>.Add(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<double,ICoordinate> Member

        ICoordinate ISubtractable<double, ICoordinate>.Subtract(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<double,ICoordinate> Member

        ICoordinate IMultipliable<double, ICoordinate>.Multiply(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<double,ICoordinate> Member

        ICoordinate IDivisible<double, ICoordinate>.Divide(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate2D> Member

        public int CompareTo(ICoordinate2D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate2D> Member

        public bool Equals(ICoordinate2D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate3D> Member

        public int CompareTo(ICoordinate3D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate3D> Member

        public bool Equals(ICoordinate3D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        internal static Coordinate Homogenize(Coordinate coordinate)
        {
            if (coordinate.HasW)
                return coordinate;

            return new Coordinate(coordinate._coordFactory, coordinate._x, coordinate._y, coordinate._z, coordinate._m, 1d);
        }

        internal static Coordinate Dehomogenize(Coordinate coordinate)
        {
            if (!coordinate.HasW)
                return coordinate;
            return new Coordinate(coordinate._coordFactory, coordinate._x, coordinate._y, coordinate._z, coordinate._m, CoordinateNullValue);
        }


        //#region ICoordinate2D Member

        //double ICoordinate2D.X
        //{
        //    get { return X; }
        //}

        //double ICoordinate2D.Y
        //{
        //    get { throw new NotSupportedException(); }
        //}

        //double ICoordinate2D.W
        //{
        //    get { throw new NotSupportedException(); }
        //}

        //void ICoordinate2D.GetComponents(out double x, out double y, out double w)
        //{
        //    throw new NotSupportedException();
        //}

        //#endregion

        #region ICoordinate Member

        //bool ICoordinate.ContainsOrdinate(Ordinates ordinate)
        //{
        //    return ContainsOrdinate(ordinate);
        //}

        double ICoordinate.Distance(ICoordinate other)
        {
            throw new NotSupportedException();
        }

        //double ICoordinate.this[Ordinates ordinate]
        //{
        //    get { return this[ordinate]; }
        //}

        //bool ICoordinate.IsEmpty
        //{
        //    get { return IsEmpty; }
        //}

        ICoordinate ICoordinate.Zero
        {
            get { return Zero; }
        }

        //ICoordinateFactory ICoordinate.Factory
        //{
        //    get { throw new NotSupportedException(); }
        //}

        #endregion

        #region IVector<DoubleComponent> Member

        IVector<DoubleComponent> IVector<DoubleComponent>.Clone()
        {
            return Clone();
        }

        //int IVector<DoubleComponent>.ComponentCount
        //{
        //    get { Com; }
        //}

        DoubleComponent[] IVector<DoubleComponent>.Components
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

        IVector<DoubleComponent> IVector<DoubleComponent>.Negative()
        {
            throw new NotSupportedException();
        }

        DoubleComponent IVector<DoubleComponent>.this[int index]
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

        void IVector<DoubleComponent>.GetComponents(out DoubleComponent a, out DoubleComponent b)
        {
            throw new NotSupportedException();
        }

        void IVector<DoubleComponent>.GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c)
        {
            throw new NotSupportedException();
        }

        void IVector<DoubleComponent>.GetComponents(out DoubleComponent a, out DoubleComponent b, out DoubleComponent c, out DoubleComponent d)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMatrix<DoubleComponent> Member

        double IMatrix<DoubleComponent>.Determinant
        {
            get { throw new NotSupportedException(); }
        }

        int IMatrix<DoubleComponent>.ColumnCount
        {
            get { throw new NotSupportedException(); }
        }

        MatrixFormat IMatrix<DoubleComponent>.Format
        {
            get { throw new NotSupportedException(); }
        }

        bool IMatrix<DoubleComponent>.IsSingular
        {
            get { throw new NotSupportedException(); }
        }

        bool IMatrix<DoubleComponent>.IsInvertible
        {
            get { throw new NotSupportedException(); }
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Inverse
        {
            get { throw new NotSupportedException(); }
        }

        bool IMatrix<DoubleComponent>.IsSquare
        {
            get { throw new NotSupportedException(); }
        }

        bool IMatrix<DoubleComponent>.IsSymmetrical
        {
            get { throw new NotSupportedException(); }
        }

        int IMatrix<DoubleComponent>.RowCount
        {
            get { throw new NotSupportedException(); }
        }

        DoubleComponent IMatrix<DoubleComponent>.this[int row, int column]
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.GetMatrix(int[] rowIndexes, int startColumn, int endColumn)
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IMatrix<DoubleComponent>.Transpose()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<IMatrix<DoubleComponent>> Member

        bool IEquatable<IMatrix<DoubleComponent>>.Equals(IMatrix<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<IMatrix<DoubleComponent>> Member

        int IComparable<IMatrix<DoubleComponent>>.CompareTo(IMatrix<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Abs()
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IComputable<IMatrix<DoubleComponent>>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> ISubtractable<IMatrix<DoubleComponent>>.Subtract(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IAddable<IMatrix<DoubleComponent>>.Add(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IDivisible<IMatrix<DoubleComponent>>.Divide(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IHasOne<IMatrix<DoubleComponent>>.One
        {
            get { return One; }
        }

        #endregion

        #region IMultipliable<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IMultipliable<IMatrix<DoubleComponent>>.Multiply(IMatrix<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<IMatrix<DoubleComponent>> Member

        bool IBooleanComparable<IMatrix<DoubleComponent>>.GreaterThan(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IMatrix<DoubleComponent>>.GreaterThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IMatrix<DoubleComponent>>.LessThan(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IMatrix<DoubleComponent>>.LessThanOrEqualTo(IMatrix<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IExponential<IMatrix<DoubleComponent>> Member

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Power(double exponent)
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Sqrt()
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Log(double newBase)
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Log()
        {
            throw new NotSupportedException();
        }

        IMatrix<DoubleComponent> IExponential<IMatrix<DoubleComponent>>.Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<DoubleComponent> Member

        System.Collections.Generic.IEnumerator<DoubleComponent> System.Collections.Generic.IEnumerable<DoubleComponent>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INegatable<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> INegatable<IVector<DoubleComponent>>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> ISubtractable<IVector<DoubleComponent>>.Subtract(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IAddable<IVector<DoubleComponent>>.Add(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IDivisible<IVector<DoubleComponent>>.Divide(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IMultipliable<IVector<DoubleComponent>>.Multiply(IVector<DoubleComponent> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<IVector<DoubleComponent>> Member

        bool IBooleanComparable<IVector<DoubleComponent>>.GreaterThan(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IVector<DoubleComponent>>.GreaterThanOrEqualTo(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IVector<DoubleComponent>>.LessThan(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<IVector<DoubleComponent>>.LessThanOrEqualTo(IVector<DoubleComponent> value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<double,IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IAddable<double, IVector<DoubleComponent>>.Add(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<double,IVector<DoubleComponent>> Member

        IVector<DoubleComponent> ISubtractable<double, IVector<DoubleComponent>>.Subtract(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<double,IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IMultipliable<double, IVector<DoubleComponent>>.Multiply(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<double,IVector<DoubleComponent>> Member

        IVector<DoubleComponent> IDivisible<double, IVector<DoubleComponent>>.Divide(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<IVector<DoubleComponent>> Member

        bool IEquatable<IVector<DoubleComponent>>.Equals(IVector<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<IVector<DoubleComponent>> Member

        int IComparable<IVector<DoubleComponent>>.CompareTo(IVector<DoubleComponent> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate> Member

        int IComparable<ICoordinate>.CompareTo(ICoordinate other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate> Member

        bool IEquatable<ICoordinate>.Equals(ICoordinate other)
        {
            return Equals(other);
        }

        #endregion

        #region IConvertible Member

        TypeCode IConvertible.GetTypeCode()
        {
            throw new NotSupportedException();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<ICoordinate> Member

        ICoordinate ISubtractable<ICoordinate>.Subtract(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasZero<ICoordinate> Member

        ICoordinate IHasZero<ICoordinate>.Zero
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IAddable<ICoordinate> Member

        ICoordinate IAddable<ICoordinate>.Add(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<ICoordinate> Member

        ICoordinate IDivisible<ICoordinate>.Divide(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<ICoordinate> Member

        ICoordinate IMultipliable<ICoordinate>.Multiply(ICoordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<ICoordinate> Member

        bool IBooleanComparable<ICoordinate>.GreaterThan(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<ICoordinate>.GreaterThanOrEqualTo(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<ICoordinate>.LessThan(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<ICoordinate>.LessThanOrEqualTo(ICoordinate value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate2D> Member

        int IComparable<ICoordinate2D>.CompareTo(ICoordinate2D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate2D> Member

        bool IEquatable<ICoordinate2D>.Equals(ICoordinate2D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate3D> Member

        int IComparable<ICoordinate3D>.CompareTo(ICoordinate3D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate3D> Member

        bool IEquatable<ICoordinate3D>.Equals(ICoordinate3D other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<Coordinate> Member

        public bool Equals(Coordinate other)
        {
            
#if class
            if (ReferenceEquals(this, other))
                return true;

            if (ReferenceEquals(other, null))
                return false;
#endif
            return ValueEquals(other) && _coordFactory == other._coordFactory;
        }

        #endregion

        #region IComparable<Coordinate> Member

        public int CompareTo(Coordinate other)
        {
            ////jd: reinstated tests against empty coordinates as many unit tests rely on this
            //if (_id == null && other._id == null)
            //    return 0;

            //// Empty coordinates don't compare
            //if (other._id == null)
            //{
            //    return 1;
            //    //throw new ArgumentException("Cannot compare to the empty coordinate");
            //}

            //if (_id == null)
            //{
            //    return -1;
            //    //throw new InvalidOperationException(
            //    //    "This coordinate is empty and cannot be compared");
            //}

            //// Since the coordinates are stored in lexicograpic order,
            //// the index comparison works to compare coordinates
            //// first by X, then by Y;
            //return _factory.Compare(this, other);
            if (IsEmpty && other.IsEmpty)
                return 0;

            if (IsEmpty && !other.IsEmpty)
                return 1;

            if (!IsEmpty && other.IsEmpty)
                return -1;

            if (_x < other._x) return -1;
            if (_x > other._x) return 1;
            if (_y < other._y) return -1;
            if (_y > other._y) return 1;

            if (HasZ)
            {
                if (!other.HasZ) return 1;
                if (_z < other._z) return -1;
                if (_z > other._z) return 1;
            }

            if (!HasZ && other.HasZ)
                return -1;

            if (HasM)
            {
                if (!other.HasM) return 1;
                if (_m < other._m) return -1;
                if (_m > other._m) return 1;
            }

            if (!HasM && other.HasM)
                return -1;

            if (HasW)
            {
                if (!other.HasW) return 1;
                if (W < other.W) return -1;
                if (W > other.W) return 1;
            }

            if (!HasW && other.HasW)
                return -1;

            return 0;
        }

        #endregion

        #region IComputable<double,Coordinate> Member

        Coordinate IComputable<double, Coordinate>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<Coordinate> Member

        Coordinate IComputable<Coordinate>.Abs()
        {
            throw new NotSupportedException();
        }

        Coordinate IComputable<Coordinate>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INegatable<Coordinate> Member

        Coordinate INegatable<Coordinate>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<Coordinate> Member

        public Coordinate Subtract(Coordinate b)
        {
            return new Coordinate(_coordFactory, _x - b.X, _y - b.Y);
        }

        #endregion

        #region IHasZero<Coordinate> Member

        Coordinate IHasZero<Coordinate>.Zero
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IAddable<Coordinate> Member

        public Coordinate Add(Coordinate b)
        {
            return new Coordinate(_coordFactory, _x + b.X, _y + b.Y);
        }

        #endregion

        #region IDivisible<Coordinate> Member

        public Coordinate Divide(Coordinate b)
        {
            throw  new NotSupportedException();
        }

        #endregion

        #region IMultipliable<Coordinate> Member

        public Coordinate Multiply(Coordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<Coordinate> Member

        public bool GreaterThan(Coordinate value)
        {
            return CompareTo(value) > 0;
        }

        public bool GreaterThanOrEqualTo(Coordinate value)
        {
            return CompareTo(value) >= 0;
        }

        public bool LessThan(Coordinate value)
        {
            return CompareTo(value) < 0;
        }

        public bool LessThanOrEqualTo(Coordinate value)
        {
            return CompareTo(value) <= 0;
        }

        #endregion

        #region IExponential<Coordinate> Member

        Coordinate IExponential<Coordinate>.Power(double exponent)
        {
            throw new NotSupportedException();
        }

        Coordinate IExponential<Coordinate>.Sqrt()
        {
            throw new NotSupportedException();
        }

        Coordinate IExponential<Coordinate>.Log(double newBase)
        {
            throw new NotSupportedException();
        }

        Coordinate IExponential<Coordinate>.Log()
        {
            throw new NotSupportedException();
        }

        Coordinate IExponential<Coordinate>.Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<double,Coordinate> Member

        Coordinate IAddable<double, Coordinate>.Add(double b)
        {
            return new Coordinate(_coordFactory, _x + b, _y + b, HasZ ? _z + b : Double.NaN, HasM ? _m + b : Double.NaN, HasW ? _w + b : Double.NaN);
        }

        #endregion

        #region ISubtractable<double,Coordinate> Member

        Coordinate ISubtractable<double, Coordinate>.Subtract(double b)
        {
            return new Coordinate(_coordFactory, _x - b, _y - b, HasZ ? _z - b : Double.NaN, HasM ? _m - b : Double.NaN, HasW ? _w - b : Double.NaN);
        }

        #endregion

        #region IMultipliable<double,Coordinate> Member

        Coordinate IMultipliable<double, Coordinate>.Multiply(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<double,Coordinate> Member

        Coordinate IDivisible<double, Coordinate>.Divide(double b)
        {
            return _coordFactory.Divide(this, b);
        }

        #endregion

        #region ICoordinate<Coordinate> Member

        public double Distance(Coordinate other)
        {
            Double dx = X - other.X;
            Double dy = Y - other.Y;
            Double dz = HasZ && other.HasZ ? Z - other.Z : 0d;

            return Math.Sqrt(dx*dx + dy*dy + dz*dz);
        }

        Coordinate ICoordinate<Coordinate>.Zero
        {
            get { return Zero; }
        }

        Coordinate ICoordinate<Coordinate>.Clone()
        {
            return Clone(this);
        }

        Coordinate ICoordinate<Coordinate>.Multiply(double factor)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ICoordinate2DM Member

        public double M
        {
            get { return _m; }
        }

        #endregion

        #region IComparable<ICoordinate2DM> Member

        public int CompareTo(ICoordinate2DM other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate2DM> Member

        public bool Equals(ICoordinate2DM other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<ICoordinate3DM> Member

        public int CompareTo(ICoordinate3DM other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<ICoordinate3DM> Member

        public bool Equals(ICoordinate3DM other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IVector<DoubleComponent,Coordinate> Member

        Coordinate IVector<DoubleComponent, Coordinate>.Clone()
        {
            throw new NotSupportedException();
        }

        Coordinate IVector<DoubleComponent, Coordinate>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<double,IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IComputable<double, IVector<DoubleComponent, Coordinate>>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComputable<IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IComputable<IVector<DoubleComponent, Coordinate>>.Abs()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent, Coordinate> IComputable<IVector<DoubleComponent, Coordinate>>.Set(double value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region INegatable<IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> INegatable<IVector<DoubleComponent, Coordinate>>.Negative()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<IVector<DoubleComponent,Coordinate>> Member

        public IVector<DoubleComponent, Coordinate> Subtract(IVector<DoubleComponent, Coordinate> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasZero<IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IHasZero<IVector<DoubleComponent, Coordinate>>.Zero
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IAddable<IVector<DoubleComponent,Coordinate>> Member

        public IVector<DoubleComponent, Coordinate> Add(IVector<DoubleComponent, Coordinate> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<IVector<DoubleComponent,Coordinate>> Member

        public IVector<DoubleComponent, Coordinate> Divide(IVector<DoubleComponent, Coordinate> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IHasOne<IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IHasOne<IVector<DoubleComponent, Coordinate>>.One
        {
            get { throw new NotSupportedException(); }
        }

        #endregion

        #region IMultipliable<IVector<DoubleComponent,Coordinate>> Member

        public IVector<DoubleComponent, Coordinate> Multiply(IVector<DoubleComponent, Coordinate> b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<IVector<DoubleComponent,Coordinate>> Member

        public bool GreaterThan(IVector<DoubleComponent, Coordinate> value)
        {
            throw new NotSupportedException();
        }

        public bool GreaterThanOrEqualTo(IVector<DoubleComponent, Coordinate> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThan(IVector<DoubleComponent, Coordinate> value)
        {
            throw new NotSupportedException();
        }

        public bool LessThanOrEqualTo(IVector<DoubleComponent, Coordinate> value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IExponential<IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IExponential<IVector<DoubleComponent, Coordinate>>.Power(double exponent)
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent, Coordinate> IExponential<IVector<DoubleComponent, Coordinate>>.Sqrt()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent, Coordinate> IExponential<IVector<DoubleComponent, Coordinate>>.Log(double newBase)
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent, Coordinate> IExponential<IVector<DoubleComponent, Coordinate>>.Log()
        {
            throw new NotSupportedException();
        }

        IVector<DoubleComponent, Coordinate> IExponential<IVector<DoubleComponent, Coordinate>>.Exp()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IAddable<double,IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IAddable<double, IVector<DoubleComponent, Coordinate>>.Add(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ISubtractable<double,IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> ISubtractable<double, IVector<DoubleComponent, Coordinate>>.Subtract(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<double,IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IMultipliable<double, IVector<DoubleComponent, Coordinate>>.Multiply(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDivisible<double,IVector<DoubleComponent,Coordinate>> Member

        IVector<DoubleComponent, Coordinate> IDivisible<double, IVector<DoubleComponent, Coordinate>>.Divide(double b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<IVector<DoubleComponent,Coordinate>> Member

        public bool Equals(IVector<DoubleComponent, Coordinate> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<IVector<DoubleComponent,Coordinate>> Member

        public int CompareTo(IVector<DoubleComponent, Coordinate> other)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEquatable<Coordinate> Member

        bool IEquatable<Coordinate>.Equals(Coordinate other)
        {
            return Equals(other);
        }

        #endregion

        #region ISubtractable<Coordinate> Member

        Coordinate ISubtractable<Coordinate>.Subtract(Coordinate b)
        {
            return _coordFactory.Subtract(this, b);
        }

        #endregion

        #region IAddable<Coordinate> Member

        Coordinate IAddable<Coordinate>.Add(Coordinate b)
        {
            return _coordFactory.Add(this, b);
        }

        #endregion

        #region IDivisible<Coordinate> Member

        Coordinate IDivisible<Coordinate>.Divide(Coordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IMultipliable<Coordinate> Member

        Coordinate IMultipliable<Coordinate>.Multiply(Coordinate b)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IBooleanComparable<Coordinate> Member

        bool IBooleanComparable<Coordinate>.GreaterThan(Coordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<Coordinate>.GreaterThanOrEqualTo(Coordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<Coordinate>.LessThan(Coordinate value)
        {
            throw new NotSupportedException();
        }

        bool IBooleanComparable<Coordinate>.LessThanOrEqualTo(Coordinate value)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IComparable<Coordinate> Member

        int IComparable<Coordinate>.CompareTo(Coordinate other)
        {
            return CompareTo(other);
        }

        #endregion

        private Ordinates IndexToOrdinate(Int32 index)
        {
            switch (index)
            {
                case 0:
                    return Ordinates.X;
                case 1:
                    return Ordinates.Y;
                case 2:
                    if (HasZ) return Ordinates.Z;
                    if (HasW) return Ordinates.W;
                    if (HasM) return Ordinates.M;
                    //Should never reach here
                    throw new IndexOutOfRangeException();
                case 3:
                    if (!HasZ)
                    {
                        if (HasW && HasM) return Ordinates.M;
                    }
                    else
                    {
                        if (HasW) return Ordinates.W;
                        if (HasM) return Ordinates.M;
                    }
                    //Should never reach here
                    throw new IndexOutOfRangeException();
                case 4:
                    if (HasZ && HasW && HasM) return Ordinates.M;
                    throw new IndexOutOfRangeException();
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}