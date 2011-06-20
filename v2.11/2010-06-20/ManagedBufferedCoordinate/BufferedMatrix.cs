using System;
using System.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public struct BufferedMatrix : IAffineTransformMatrix<DoubleComponent, BufferedCoordinate, BufferedMatrix>
    {
        private BufferedCoordinate _row1;
        private BufferedCoordinate _row2;
        private BufferedCoordinate _row3;
        private BufferedCoordinate _row4;

        public BufferedMatrix(BufferedCoordinateFactory coordinateFactory, params DoubleComponent[] values)
        {
            if(values == null)
            {
                throw new ArgumentNullException("values");
            }

            _row4 = new BufferedCoordinate();

            switch (values.Length)
            {
                case 6:
                    _row1 = coordinateFactory.Create((Double)values[0], (Double)values[1], 0.0);
                    _row2 = coordinateFactory.Create((Double)values[2], (Double)values[3], 0.0);
                    _row3 = coordinateFactory.Create((Double)values[4], (Double)values[5], 1.0);
                    break;
                case 9:
                    _row1 = coordinateFactory.Create((Double)values[0], (Double)values[1], (Double)values[2]);
                    _row2 = coordinateFactory.Create((Double)values[3], (Double)values[4], (Double)values[5]);
                    _row3 = coordinateFactory.Create((Double)values[6], (Double)values[7], (Double)values[8]);
                    break;
                case 12:
                    _row1 = coordinateFactory.Create((Double)values[0], (Double)values[1], (Double)values[2], 0.0);
                    _row2 = coordinateFactory.Create((Double)values[3], (Double)values[4], (Double)values[5], 0.0);
                    _row3 = coordinateFactory.Create((Double)values[6], (Double)values[7], (Double)values[8], 0.0);
                    _row4 = coordinateFactory.Create((Double)values[9], (Double)values[10], (Double)values[11], 1.0);
                    break;
                case 16:
                    _row1 = coordinateFactory.Create((Double)values[0], (Double)values[1], (Double)values[2], (Double)values[3]);
                    _row2 = coordinateFactory.Create((Double)values[4], (Double)values[5], (Double)values[6], (Double)values[7]);
                    _row3 = coordinateFactory.Create((Double)values[8], (Double)values[9], (Double)values[10], (Double)values[11]);
                    _row4 = coordinateFactory.Create((Double)values[12], (Double)values[13], (Double)values[14], (Double)values[15]);
                    break;
                default:
                    throw new ArgumentException("BufferedMatrix must have 6, 9, 12 or 16 values.");
            }
        }

        #region IAffineTransformMatrix<DoubleComponent,BufferedCoordinate,Matrix3> Members

        public BufferedMatrix Inverse
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

        public BufferedMatrix TransformMatrix(BufferedMatrix input)
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

        public BufferedMatrix TransformVector(BufferedMatrix input)
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

        public BufferedMatrix Clone()
        {
            throw new NotImplementedException();
        }

        public Int32 ColumnCount
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

        public BufferedMatrix GetMatrix(Int32 i0, Int32 i1, Int32 j0, Int32 j1)
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix GetMatrix(Int32[] rowIndexes, Int32 startColumn, Int32 endColumn)
        {
            throw new NotImplementedException();
        }

        public Boolean IsInvertible
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean IsSingular
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean IsSquare
        {
            get { throw new NotImplementedException(); }
        }

        public Boolean IsSymmetrical
        {
            get { throw new NotImplementedException(); }
        }

        public Int32 RowCount
        {
            get { throw new NotImplementedException(); }
        }

        public BufferedMatrix Transpose()
        {
            throw new NotImplementedException();
        }

        public DoubleComponent this[Int32 row, Int32 column]
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

        public Boolean Equals(BufferedMatrix other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<Matrix3> Members

        public Int32 CompareTo(BufferedMatrix other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<Matrix3> Members

        public BufferedMatrix Abs()
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<Matrix3> Members

        public BufferedMatrix Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<Matrix3> Members

        public BufferedMatrix Subtract(BufferedMatrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<Matrix3> Members

        public BufferedMatrix Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAddable<Matrix3> Members

        public BufferedMatrix Add(BufferedMatrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Matrix3> Members

        public BufferedMatrix Divide(BufferedMatrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<Matrix3> Members

        public BufferedMatrix One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IMultipliable<Matrix3> Members

        public BufferedMatrix Multiply(BufferedMatrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<Matrix3> Members

        public Boolean GreaterThan(BufferedMatrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean GreaterThanOrEqualTo(BufferedMatrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThan(BufferedMatrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThanOrEqualTo(BufferedMatrix value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<Matrix3> Members

        public BufferedMatrix Exp()
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix Log()
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        public BufferedMatrix Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
