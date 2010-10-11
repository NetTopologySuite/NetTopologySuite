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
using System;
using System.Collections.Generic;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    public struct Matrix : IAffineTransformMatrix<DoubleComponent, Coordinate, Matrix>
    {
        private Coordinate _row1;
        private Coordinate _row2;
        private Coordinate _row3;
        private Coordinate _row4;

        public Matrix(CoordinateFactory coordinateFactory, params DoubleComponent[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            _row4 = new Coordinate();

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
                    throw new ArgumentException("Matrix must have 6, 9, 12 or 16 values.");
            }
        }

        #region IAffineTransformMatrix<DoubleComponent,Coordinate,Matrix3> Members

        public Matrix Inverse
        {
            get { throw new NotImplementedException(); }
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void RotateAt(Coordinate point, Coordinate axis, Double radians, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void RotateAt(Coordinate point, Coordinate axis, Double radians)
        {
            throw new NotImplementedException();
        }

        public void Translate(Coordinate translateVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Translate(Coordinate translateVector)
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

        #region ITransformMatrix<DoubleComponent,Coordinate,Matrix3> Members

        public void RotateAlong(Coordinate axis, Double radians, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void RotateAlong(Coordinate axis, Double radians)
        {
            throw new NotImplementedException();
        }

        public void Scale(Coordinate scaleVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Scale(Coordinate scaleVector)
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

        public void Shear(Coordinate shearVector, MatrixOperationOrder order)
        {
            throw new NotImplementedException();
        }

        public void Shear(Coordinate shearVector)
        {
            throw new NotImplementedException();
        }

        public Matrix TransformMatrix(Matrix input)
        {
            throw new NotImplementedException();
        }

        public Coordinate TransformVector(Coordinate input)
        {
            throw new NotImplementedException();
        }

        public void TransformVector(DoubleComponent[] input)
        {
            throw new NotImplementedException();
        }

        public Matrix TransformVector(Matrix input)
        {
            throw new NotImplementedException();
        }

        public void TransformVectors(IEnumerable<DoubleComponent[]> input)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Coordinate> TransformVectors(IEnumerable<Coordinate> input)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMatrix<DoubleComponent,Matrix3> Members

        public Matrix Clone()
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

        public Matrix GetMatrix(Int32 i0, Int32 i1, Int32 j0, Int32 j1)
        {
            throw new NotImplementedException();
        }

        public Matrix GetMatrix(Int32[] rowIndexes, Int32 startColumn, Int32 endColumn)
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

        public Matrix Transpose()
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

        public Boolean Equals(Matrix other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComparable<Matrix3> Members

        public Int32 CompareTo(Matrix other)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IComputable<Matrix3> Members

        public Matrix Abs()
        {
            throw new NotImplementedException();
        }

        public Matrix Set(Double value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INegatable<Matrix3> Members

        public Matrix Negative()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISubtractable<Matrix3> Members

        public Matrix Subtract(Matrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasZero<Matrix3> Members

        public Matrix Zero
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IAddable<Matrix3> Members

        public Matrix Add(Matrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDivisible<Matrix3> Members

        public Matrix Divide(Matrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHasOne<Matrix3> Members

        public Matrix One
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IMultipliable<Matrix3> Members

        public Matrix Multiply(Matrix b)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IBooleanComparable<Matrix3> Members

        public Boolean GreaterThan(Matrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean GreaterThanOrEqualTo(Matrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThan(Matrix value)
        {
            throw new NotImplementedException();
        }

        public Boolean LessThanOrEqualTo(Matrix value)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IExponential<Matrix3> Members

        public Matrix Exp()
        {
            throw new NotImplementedException();
        }

        public Matrix Log()
        {
            throw new NotImplementedException();
        }

        public Matrix Log(Double newBase)
        {
            throw new NotImplementedException();
        }

        public Matrix Power(Double exponent)
        {
            throw new NotImplementedException();
        }

        public Matrix Sqrt()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
