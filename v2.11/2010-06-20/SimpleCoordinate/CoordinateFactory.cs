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
 *  Author: Felix Obermaier 2009
 *  
 */

#endregion
using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NPack;
using NPack.Interfaces;

// ReSharper disable InconsistentNaming
namespace NetTopologySuite.Coordinates.Simple
{
    using ISimpleCoordFactory = ICoordinateFactory<Coordinate>;
    using ITypedVectorFactory = IVectorFactory<DoubleComponent, Coordinate>;
    using IMatrixFactoryD = IMatrixFactory<DoubleComponent>;
    using IVectorD = IVector<DoubleComponent>;
    using IVectorFactoryD = IVectorFactory<DoubleComponent>;

    public class CoordinateFactory
        : ISimpleCoordFactory,
          ILinearFactory<DoubleComponent, Coordinate, Matrix>,
          ILinearFactory<DoubleComponent>,
          IComparer<Coordinate>

    {
        //public struct CoordinateContext
        //{
        //    private readonly Boolean _hasZ;
        //    private readonly Boolean _hasW;

        //    internal CoordinateContext(Boolean hasZ, Boolean isHomogeneous)
        //    {
        //        _hasZ = hasZ;
        //        _hasW = isHomogeneous;
        //    }

        //    public Boolean HasZ { get { return _hasZ; } }
        //    public Boolean IsHomogeneous { get { return _hasW; } }
        //}

        //private static readonly Object _nonHomogeneous2DContext
        //    = new CoordinateContext(false, false);

        //private static readonly Object _nonHomogeneous3DContext
        //    = new CoordinateContext(true, false);

        //private static readonly Object _homogeneous2DContext
        //    = new CoordinateContext(false, true);

        public CoordinateFactory()
            : this(null) { }

        public CoordinateFactory(Double scale)
            : this(new PrecisionModel(null, scale)) { }

        public CoordinateFactory(PrecisionModelType type)
            : this(new PrecisionModel(null, type)) { }

        private readonly PrecisionModel _precisionModel;
        private readonly IMatrixOperations<DoubleComponent, Coordinate, Matrix> _ops;

        internal static CoordinateFactory CFFloating = new CoordinateFactory();

        public CoordinateFactory(IPrecisionModel precisionModel)
        {
            _precisionModel = new PrecisionModel(this, precisionModel);
            _ops = new ClrMatrixOperations<DoubleComponent, Coordinate, Matrix>(this);
        }

        #region ISimpleCoordFactory Members

        public Coordinate Create(Double x, Double y)
        {
            return new Coordinate(this, x, y);
        }

        public Coordinate Create(Double x, Double y, Double m)
        {
            return new Coordinate(this, x, y, m, OrdinateFlags.XYM);
        }

        ICoordinate ICoordinateFactory.Create(double x, double y, double val, bool valIsM)
        {
            return Create(x, y, val, valIsM);
        }

        public Coordinate Create(Double x, Double y, Double val, bool valIsM)
        {
            OrdinateFlags flag = OrdinateFlags.XY | (valIsM ? OrdinateFlags.M : OrdinateFlags.W);
            return new Coordinate(this, x, y, val, flag);
        }

        public Coordinate Create(params Double[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            Int32 length = coordinates.Length;

            switch (length)
            {
                case 0:
                    return new Coordinate();
                case 1:
                    throw new ArgumentException("Only one coordinate component was provided; " +
                                                "at least 2 are needed.");
                case 2:
                    return Create(coordinates[0], coordinates[1]);
                case 3:
                    return Create(coordinates[0], coordinates[1], coordinates[2]);
                case 4:
                    throw new ArgumentException("M or W component, don't know which");
                case 5:
                    return new Coordinate(this, coordinates[0], coordinates[1], coordinates[2], coordinates[3], coordinates[4]);
                default:
                    throw new ArgumentException("Too many components.");
            }
        }

        public Coordinate Create3D(Double x, Double y, Double z)
        {
            return new Coordinate(this, x, y, z, OrdinateFlags.XYZ);
        }

        public Coordinate Create3D(Double x, Double y, Double z, Double m)
        {
            return new Coordinate(this, x, y, z, m, OrdinateFlags.XYZM);
        }

        ICoordinate ICoordinateFactory.Create3D(double x, double y, double z, double val, bool valIsM)
        {
            return Create3D(x, y, z, val, valIsM);
        }

        public Coordinate Create3D(Double x, Double y, Double z, Double val, bool valIsM)
        {
            OrdinateFlags flags = OrdinateFlags.XYZ | (valIsM ? OrdinateFlags.M : OrdinateFlags.W);
            return new Coordinate(this, x, y, z, val, OrdinateFlags.XYZM);
        }

        public Coordinate Create3D(params Double[] coordinates)
        {
            if (coordinates == null)
            {
                throw new ArgumentNullException("coordinates");
            }

            Int32 length = coordinates.Length;

            switch (length)
            {
                case 0:
                case 1:
                case 2:
                    throw new ArgumentException("Less then 3 coordinate components were provided; " +
                                                "at least 3 are needed.");
                case 3:
                    return Create3D(coordinates[0], coordinates[1], coordinates[2]);
                case 4:
                    return Create3D(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
                case 5:
                    return new Coordinate(this, coordinates[0], coordinates[1], coordinates[2], coordinates[4], coordinates[3]);
                default:
                    throw new ArgumentException("Too many components.");
            }
        }

        public Coordinate Create(Coordinate coordinate)
        {
            if (coordinate.IsEmpty)
            {
                return new Coordinate();
            }

            if (ReferenceEquals(coordinate.CoordinateFactory, this))
            {
                return coordinate;
            }

            return new Coordinate(this, coordinate);
        }

        public Coordinate Create(ICoordinate coordinate)
        {
            if (coordinate is Coordinate)
                return Create((Coordinate)coordinate);

            if (coordinate.IsEmpty)
                return new Coordinate();

            ICoordinate3DM coordinate3DM = coordinate as ICoordinate3DM;
            if (coordinate3DM != null)
                return new Coordinate(this, coordinate3DM.X, coordinate3DM.Y, coordinate3DM.Z, coordinate3DM.M, OrdinateFlags.XYZM);

            ICoordinate3D coordinate3D = coordinate as ICoordinate3D;
            if (coordinate3D != null)
                return new Coordinate(this, coordinate3D.X, coordinate3D.Y, coordinate3D.Z, OrdinateFlags.XYZ);

            ICoordinate2DM coordinate2DM = coordinate as ICoordinate2DM;
            if (coordinate2DM != null)
                return new Coordinate(this, coordinate2DM.X, coordinate2DM.Y, coordinate2DM.M, OrdinateFlags.XYM);

            ICoordinate2D coordinate2D = coordinate as ICoordinate2D;
            if (coordinate2D != null)
                return new Coordinate(this, coordinate2D.X, coordinate2D.Y);

            return new Coordinate();
        }

        public Coordinate Create3D(Coordinate coordinate)
        {
            return Create(coordinate);
        }

        public Coordinate Create3D(ICoordinate coordinate)
        {
            if (coordinate is Coordinate)
            {
                return Create3D((Coordinate)coordinate);
            }

            ICoordinate3D coordinate3D = coordinate as ICoordinate3D;

            if (coordinate3D == null)
            {
                return coordinate.IsEmpty
                    ? new Coordinate()
                    : Create(coordinate[Ordinates.X],
                             coordinate[Ordinates.Y],
                             coordinate[Ordinates.Z],
                             coordinate[Ordinates.W]);

            }

            if (coordinate.IsEmpty)
            {
                return new Coordinate();
            }

            return Create(coordinate3D);
        }

        public Coordinate Homogenize(Coordinate coordinate)
        {
            return Coordinate.Homogenize(coordinate);
        }

        public IEnumerable<Coordinate> Homogenize(IEnumerable<Coordinate> coordinates)
        {
            foreach (Coordinate coordinate in coordinates)
            {
                yield return Homogenize(coordinate);
            }
        }

        public Coordinate Dehomogenize(Coordinate coordinate)
        {
            if (coordinate.HasW)
                return Coordinate.Dehomogenize(coordinate);
            return coordinate;
        }

        public IEnumerable<Coordinate> Dehomogenize(IEnumerable<Coordinate> coordinates)
        {
            foreach (Coordinate coordinate in coordinates)
            {
                yield return Dehomogenize(coordinate);
            }
        }

        public IPrecisionModel<Coordinate> PrecisionModel
        {
            get { return _precisionModel; }
        }

        public IPrecisionModel<Coordinate> CreatePrecisionModel(Double scale)
        {
            return new PrecisionModel(this, scale);
        }

        public IPrecisionModel<Coordinate> CreatePrecisionModel(PrecisionModelType type)
        {
            return new PrecisionModel(this, type);
        }

        #endregion

        #region ICoordinateFactory Members

        ICoordinate ICoordinateFactory.Create(Double x, Double y)
        {
            return Create(x, y);
        }

        ICoordinate ICoordinateFactory.Create(Double x, Double y, Double m)
        {
            return Create(x, y, m);
        }

        ICoordinate ICoordinateFactory.Create(params Double[] coordinates)
        {
            return Create(coordinates);
        }

        ICoordinate ICoordinateFactory.Create3D(Double x, Double y, Double z)
        {
            return Create3D(x, y, z);
        }

        ICoordinate ICoordinateFactory.Create3D(Double x, Double y, Double z, Double m)
        {
            return Create3D(x, y, z, m);
        }

        ICoordinate ICoordinateFactory.Create3D(params Double[] coordinates)
        {
            return Create3D(coordinates);
        }

        IAffineTransformMatrix<DoubleComponent> ICoordinateFactory.CreateTransform(ICoordinate scaleVector, ICoordinate rotationAxis, Double rotation, ICoordinate translateVector)
        {
            throw new NotImplementedException();
        }

        ICoordinate ICoordinateFactory.Homogenize(ICoordinate coordinate)
        {
            if (coordinate == null)
            {
                throw new ArgumentNullException("coordinate");
            }

            return Homogenize(Create(coordinate));
        }

        ICoordinate ICoordinateFactory.Dehomogenize(ICoordinate coordinate)
        {
            if (coordinate.IsEmpty)
            {
                throw new ArgumentNullException("coordinate");
            }

            if (coordinate is Coordinate)
                return Dehomogenize((Coordinate)coordinate);

            return Dehomogenize(Create(coordinate));
        }

        IPrecisionModel ICoordinateFactory.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        IPrecisionModel ICoordinateFactory.CreatePrecisionModel(PrecisionModelType type)
        {
            return CreatePrecisionModel(type);
        }

        IPrecisionModel ICoordinateFactory.CreatePrecisionModel(Double scale)
        {
            return CreatePrecisionModel(scale);
        }

        #endregion

        #region IMatrixFactory<DoubleComponent,BufferedMatrix> Members

        public Matrix CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            if (format == MatrixFormat.RowMajor)
            {
                checkCounts(rowCount, columnCount);
                return new Matrix();
            }

            throw new ArgumentException("Only row-major matrixes are supported");
        }

        public Matrix CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            checkCounts(rowCount, columnCount);

            return new Matrix(this, Enumerable.ToArray(values));
        }

        public Matrix CreateMatrix(Int32 rowCount, Int32 columnCount)
        {
            checkCounts(rowCount, columnCount);

            return new Matrix();
        }

        public Matrix CreateMatrix(Matrix matrix)
        {
            return matrix;
        }

        #endregion

        private static void checkCounts(Int32 rowCount, Int32 columnCount)
        {
            if (rowCount != 3 || rowCount != 4)
            {
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be 3 or 4");
            }

            if (columnCount != 3 || columnCount != 4)
            {
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be 3 or 4");
            }
        }

        #region IMatrixFactoryD Members
        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(Int32 rowCount, Int32 columnCount, IEnumerable<DoubleComponent> values)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(IMatrix<DoubleComponent> matrix)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactoryD.CreateTransformMatrix(Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        ITransformMatrix<DoubleComponent> IMatrixFactoryD.CreateTransformMatrix(MatrixFormat format, Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactoryD.CreateAffineMatrix(Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IAffineTransformMatrix<DoubleComponent> IMatrixFactoryD.CreateAffineMatrix(MatrixFormat format, Int32 rank)
        {
            throw new System.NotImplementedException();
        }

        IMatrix<DoubleComponent> IMatrixFactoryD.CreateMatrix(Int32 rowCount, Int32 columnCount)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region IVectorFactory<DoubleComponent,BufferedCoordinate> Members

        public Coordinate CreateVector(params Double[] components)
        {
            switch (components.Length)
            {
                case 2:
                    return CreateVector(components[0], components[1]);
                case 3:
                    return CreateVector(components[0], components[1], components[2]);
                case 4:
                    //return CreateVector(components[0], components[1], components[2], components[3]);
                    throw new NotImplementedException();
                default:
                    throw new ArgumentException("A BufferedCoordinate must " +
                                                "have only 2, 3 " + /* "or 4 " + */
                                                "components.");
            }
        }

        public Coordinate CreateVector(Double a, Double b, Double c)
        {
            return Create(a, b, c, false);
        }

        public Coordinate CreateVector(Double a, Double b)
        {
            return Create(a, b);
        }

        public Coordinate CreateVector(params DoubleComponent[] components)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return Create((Double)a, (Double)b, (Double)c, false);
        }

        public Coordinate CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return Create((Double)a, (Double)b);
        }
        #endregion

        #region IVectorFactoryD Members
        IVectorD IVectorFactoryD.CreateVector(Int32 componentCount)
        {
            return CreateVector(componentCount);
        }

        IVectorD IVectorFactoryD.CreateVector(IEnumerable<DoubleComponent> values)
        {
            return CreateVector(Enumerable.ToArray(values));
        }

        IVectorD IVectorFactoryD.CreateVector(DoubleComponent a, DoubleComponent b)
        {
            return CreateVector(a, b);
        }

        IVectorD IVectorFactoryD.CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            return CreateVector(a, b, c);
        }

        IVectorD IVectorFactoryD.CreateVector(params DoubleComponent[] components)
        {
            return CreateVector(components);
        }

        IVectorD IVectorFactoryD.CreateVector(Double a, Double b)
        {
            return CreateVector(a, b);
        }

        IVectorD IVectorFactoryD.CreateVector(Double a, Double b, Double c)
        {
            return CreateVector(a, b, c);
        }

        IVectorD IVectorFactoryD.CreateVector(params Double[] components)
        {
            return CreateVector(components);
        }
        #endregion

        internal Coordinate GetZero()
        {
            return Create(0, 0);
        }

        internal Coordinate Add(Coordinate a, Coordinate b)
        {
            return _ops.Add(a, b);
            //return getVertexInternal(a.X + b.X, a.Y + b.Y);
        }

        internal Coordinate Add(Coordinate a, Double b)
        {
            return _ops.ScalarAdd(a, b);
            //return getVertexInternal(a.X + b, a.Y + b);
        }

        internal Coordinate Subtract(Coordinate a, Coordinate b)
        {
            return _ops.Subtract(a, b);
        }

        internal Coordinate Subtract(Coordinate a, Double b)
        {
            return _ops.ScalarSubtract(a, b);
            //return getVertexInternal(a.X + b, a.Y + b);
        }

        //internal static BufferedCoordinate Divide(BufferedCoordinate a, BufferedCoordinate b)
        //{
        //    throw new NotImplementedException();
        //}

        internal Coordinate Divide(Coordinate a, Double b)
        {
            return _ops.ScalarMultiply(a, 1 / b);
        }

        internal Double Distance(Coordinate a, Coordinate b)
        {
            // the Euclidian norm over the vector difference
            return _ops.TwoNorm(_ops.Subtract(a, b));
        }

        internal Coordinate GetOne()
        {
            return Create(1, 1);
        }

        internal Double Dot(Coordinate a, Coordinate b)
        {
            return (Double)_ops.Dot(a, b);
        }

        internal Coordinate Cross(Coordinate a, Coordinate b)
        {
            return _ops.Cross(a, b);
        }

        public Int32 Compare(Coordinate a, Coordinate b)
        {
            return a.CompareTo(b);
        }

        internal Boolean GreaterThan(Coordinate a, Coordinate b)
        {
            return Compare(a, b) > 0;
        }

        internal Boolean GreaterThanOrEqualTo(Coordinate a, Coordinate b)
        {
            return Compare(a, b) >= 0;
        }

        internal Boolean LessThan(Coordinate a, Coordinate b)
        {
            return Compare(a, b) < 0;
        }

        internal Boolean LessThanOrEqualTo(Coordinate a, Coordinate b)
        {
            return Compare(a, b) <= 0;
        }

        //private static void getZW(IVectorD vector,
        //                          Object context,
        //                          out Double z,
        //                          out Double w)
        //{
        //    CoordinateContext? typedContext = (CoordinateContext?)context;

        //    z = Double.NaN;
        //    w = Double.NaN;

        //    if (typedContext == null)
        //    {
        //        return;
        //    }

        //    if (typedContext.Value.HasZ)
        //    {
        //        z = (Double)vector[2];

        //        if (typedContext.Value.IsHomogeneous)
        //        {
        //            w = (Double)vector[3];
        //        }
        //    }
        //    else if (typedContext.Value.IsHomogeneous)
        //    {
        //        w = (Double)vector[2];
        //    }
        //}

        //private void getZW(DoubleComponent component, Object context, out Double z, out Double w)
        //{
        //    CoordinateContext? typedContext = (CoordinateContext?)context;

        //    z = Double.NaN;
        //    w = Double.NaN;

        //    if (typedContext == null)
        //    {
        //        return;
        //    }

        //    if (typedContext.Value.HasZ)
        //    {
        //        z = (Double)component;
        //    }
        //    else if (typedContext.Value.IsHomogeneous)
        //    {
        //        w = (Double)component;
        //    }
        //}

        ICoordinate ICoordinateFactory.Create(IVector<DoubleComponent> coordinate)
        {
            throw new NotImplementedException();
        }


        #region IVectorFactory<DoubleComponent,Coordinate> Member

        Coordinate IVectorFactory<DoubleComponent, Coordinate>.CreateVector(int componentCount)
        {
            throw new NotImplementedException();
        }

        Coordinate IVectorFactory<DoubleComponent, Coordinate>.CreateVector(IEnumerable<DoubleComponent> values)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISimpleCoordFactory<Coordinate> Member


        public Coordinate Create(IVector<DoubleComponent> coordinate)
        {
            switch (coordinate.ComponentCount)
            {
                //case 2:
                //    return Create((Double) coordinate[0], (Double) coordinate[1]);
                case 3:
                    return Create((Double)coordinate[0], (Double)coordinate[1]);
                case 4:
                    return new Coordinate(this, (Double)coordinate[0], (Double)coordinate[1], (Double)coordinate[2],OrdinateFlags.XYZ);
            }
            return new Coordinate();

        }

        #endregion

        #region IComparer<Coordinate> Member

        int IComparer<Coordinate>.Compare(Coordinate x, Coordinate y)
        {
            return x.CompareTo(y);
        }

        #endregion
    }
}