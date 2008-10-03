using System;
using System.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public struct Matrix3 : IAffineTransformMatrix<DoubleComponent, BufferedCoordinate, Matrix3>
    {
        private DoubleComponent _m00, _m01, _m02;
        private DoubleComponent _m10, _m11, _m12;
        private DoubleComponent _m20, _m21, _m22;

        public Matrix3(params DoubleComponent[] values)
        {
            if(values == null)
            {
                throw new ArgumentNullException("values");
            }

            if(values.Length == 6)
            {
                _m00 = values[0];
                _m01 = values[1];
                _m02 = 0;
                _m10 = values[2];
                _m11 = values[3];
                _m12 = 0;
                _m20 = values[4];
                _m21 = values[5];
                _m22 = 1;
            }

            if(values.Length == 9)
            {
                _m00 = values[0];
                _m01 = values[1];
                _m02 = values[2];
                _m10 = values[3];
                _m11 = values[4];
                _m12 = values[5];
                _m20 = values[6];
                _m21 = values[7];
                _m22 = values[8];
            }

            throw new ArgumentException("Matrix3 must have 6 or 9 values.");
        }

        #region IAffineTransformMatrix<DoubleComponent,BufferedCoordinate,Matrix3> Members

        public Matrix3 Inverse
        {
            get { throw new NotImplementedException(); }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void RotateAt(BufferedCoordinate point, BufferedCoordinate axis, Double radians, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void RotateAt(BufferedCoordinate point, BufferedCoordinate axis, Double radians)
        {
            throw new NotImplementedException();
        }

        public void Translate(BufferedCoordinate translateVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Translate(BufferedCoordinate translateVector)
        {
            throw new NotImplementedException();
        }

        public void Translate(DoubleComponent amount, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Translate(DoubleComponent amount)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ITransformMatrix<DoubleComponent,BufferedCoordinate,Matrix3> Members

        public void RotateAlong(BufferedCoordinate axis, Double radians, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void RotateAlong(BufferedCoordinate axis, Double radians)
        {
            throw new NotImplementedException();
        }

        public void Scale(BufferedCoordinate scaleVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Scale(BufferedCoordinate scaleVector)
        {
            throw new NotImplementedException();
        }

        public void Scale(DoubleComponent amount, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Scale(DoubleComponent amount)
        {
            throw new NotImplementedException();
        }

        public void Shear(BufferedCoordinate shearVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Shear(BufferedCoordinate shearVector)
        {
            throw new NotImplementedException();
        }

        public Matrix3 TransformMatrix(Matrix3 input)
        {
            throw new NotImplementedException();
        }

        public BufferedCoordinate TransformVector(BufferedCoordinate input)
        {
            throw new NotImplementedException();
        }

        public void TransformVector(DoubleComponent[] input)
        {
            throw new NotImplementedException();
        }

        public Matrix3 TransformVector(Matrix3 input)
        {
            throw new NotImplementedException();
        }

        public void TransformVectors(IEnumerable<DoubleComponent[]> input)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BufferedCoordinate> TransformVectors(IEnumerable<BufferedCoordinate> input)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMatrix<DoubleComponent,Matrix3> Members

        public Matrix3 Clone()
        {
            throw new NotImplementedException();
        }

        public int ColumnCount
        {
            get { throw new NotImplementedException(); }
        }

        public Double Determinant
        {
            get { throw new NotImplementedException(); }
        }

        public MatrixFormat Format
        {
            get { throw new NotImplementedException(); }
        }

        public Matrix3 GetMatrix(int i0, int i1, int j0, int j1)
        {
            throw new NotImplementedException();
        }

        public Matrix3 GetMatrix(int[] rowIndexes, int startColumn, int endColumn)
        {
            throw new NotImplementedException();
        }

        public bool IsInvertible
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSingular
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSquare
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsSymmetrical
        {
            get { throw new NotImplementedException(); }
        }

        public int RowCount
        {
            get { throw new NotImplementedException(); }
        }

        public Matrix3 Transpose()
        {
            throw new NotImplementedException();
        }

        public DoubleComponent this[int row, int column]
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

        #region IEquatable<Matrix3> Members

        public bool Equals(Matrix3 other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<Matrix3> Members

        public int CompareTo(Matrix3 other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<Matrix3> Members

        public Matrix3 Abs()
        {
            throw new NotImplementedException();
        }

        public Matrix3 Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<Matrix3> Members

        public Matrix3 Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<Matrix3> Members

        public Matrix3 Subtract(Matrix3 b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<Matrix3> Members

        public Matrix3 Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAddable<Matrix3> Members

        public Matrix3 Add(Matrix3 b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Matrix3> Members

        public Matrix3 Divide(Matrix3 b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<Matrix3> Members

        public Matrix3 One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IMultipliable<Matrix3> Members

        public Matrix3 Multiply(Matrix3 b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<Matrix3> Members

        public bool GreaterThan(Matrix3 value)
        {
            throw new NotImplementedException();
        }

        public bool GreaterThanOrEqualTo(Matrix3 value)
        {
            throw new NotImplementedException();
        }

        public bool LessThan(Matrix3 value)
        {
            throw new NotImplementedException();
        }

        public bool LessThanOrEqualTo(Matrix3 value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<Matrix3> Members

        public Matrix3 Exp()
        {
            throw new NotImplementedException();
        }

        public Matrix3 Log()
        {
            throw new NotImplementedException();
        }

        public Matrix3 Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        public Matrix3 Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        public Matrix3 Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
