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
using GeoAPI.Coordinates;
using NPack;
#if DOTNET35
    using System.Linq;
#else
    using GeoAPI.DataStructures;
#endif

namespace NetTopologySuite.Coordinates
{
    using ISimpleCoordFactory = ICoordinateFactory<Coordinate>;
    using ISimpleCoordSequence = ICoordinateSequence<Coordinate>;
    using ISimpleCoordSequenceFactory = ICoordinateSequenceFactory<Coordinate>;

    public class CoordinateSequenceFactory : 
        ISimpleCoordSequenceFactory
    {
        private readonly CoordinateFactory _coordFactory;

        public CoordinateSequenceFactory()
            : this(new CoordinateFactory()) { }

        public CoordinateSequenceFactory(CoordinateFactory coordFactory)
        {
            if (coordFactory == null) throw new ArgumentNullException("coordFactory");

            _coordFactory = coordFactory;
        }

        public IComparer<Coordinate> DefaultComparer
        {
            get { return _coordFactory; }
        }

        #region ICoordinateSequenceFactory<Coordinate> Members

        public ISimpleCoordFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public ISimpleCoordSequence Create(CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new CoordinateSequence(this);
        }

        public IPrecisionModel<Coordinate> PrecisionModel
        {
            get { return _coordFactory.PrecisionModel; }
        }

        IPrecisionModel ICoordinateSequenceFactory.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        public ISimpleCoordSequence Create(Int32 size, CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new CoordinateSequence(this, size);
        }

        public ISimpleCoordSequence Create(ISimpleCoordSequence coordSeq)
        {
            ISimpleCoordSequence newSequence = Create(0, CoordinateDimensions.Three);
            newSequence.AddSequence(coordSeq);
            return newSequence;
        }

        public ISimpleCoordSequence Create(Func<Double, Double> componentTransform,
                                             IEnumerable<Coordinate> coordinates,
                                             Boolean allowRepeated,
                                             Boolean direction)
        {
            ISimpleCoordSequence newSequence = Create(CoordinateDimensions.Two);

            Coordinate lastCoord = new Coordinate();

            if (!direction)
            {
                coordinates = Enumerable.Reverse(coordinates);
            }

            foreach (Coordinate coordinate in coordinates)
            {
                Coordinate c = coordinate;

                if (componentTransform != null)
                {
                    Double x = componentTransform(coordinate[Ordinates.X]);
                    Double y = componentTransform(coordinate[Ordinates.Y]);
                    c = _coordFactory.Create(x, y);
                }

                if (!allowRepeated && c.Equals(lastCoord))
                {
                    continue;
                }

                newSequence.Add(c);

                lastCoord = c;
            }

            return newSequence;
        }

        public ISimpleCoordSequence Create(IEnumerable<Coordinate> coordinates,
                                             Boolean allowRepeated,
                                             Boolean direction)
        {
            return Create(null, coordinates, allowRepeated, direction);
        }

        public ISimpleCoordSequence Create(Func<Double, Double> componentTransform,
            IEnumerable<Coordinate> coordinates, Boolean allowRepeated)
        {
            return Create(componentTransform, coordinates, allowRepeated, true);
        }

        public ISimpleCoordSequence Create(IEnumerable<Coordinate> coordinates,
            Boolean allowRepeated)
        {
            return Create(null, coordinates, allowRepeated, true);
        }

        public ISimpleCoordSequence Create(Func<Double, Double> componentTransform,
            IEnumerable<Coordinate> coordinates)
        {
            return Create(componentTransform, coordinates, true, true);
        }

        public ISimpleCoordSequence Create(IEnumerable<Coordinate> coordinates)
        {
            return Create(null, coordinates, true, true);
        }

        public ISimpleCoordSequence Create(ICoordinateSequence coordSeq)
        {
            ISimpleCoordSequence converted = Create(coordSeq.Count, coordSeq.Dimension);

            foreach (ICoordinate coordinate in coordSeq)
            {
                converted.Add(coordinate);
            }

            return converted;
        }

        public ISimpleCoordSequence Create(params Coordinate[] coordinates)
        {
            return Create(null, coordinates, true, true);
        }

        #endregion

        #region ICoordinateSequenceFactory Members

        ICoordinateSequence ICoordinateSequenceFactory.Create(CoordinateDimensions dimension)
        {
            return Create(dimension);
        }

        ICoordinateSequence ICoordinateSequenceFactory.Create(Int32 size, CoordinateDimensions dimension)
        {
            return Create(size, dimension);
        }

        ICoordinateSequence ICoordinateSequenceFactory.Create(ICoordinateSequence coordSeq)
        {
            return (this as ICoordinateSequenceFactory).Create((IEnumerable<ICoordinate>)coordSeq);
        }

        ICoordinateSequence ICoordinateSequenceFactory.Create(IEnumerable<ICoordinate> coordinates)
        {
            return Create(convertCoordinates(coordinates));
        }

        #endregion

        private IEnumerable<Coordinate> convertCoordinates(IEnumerable<ICoordinate> coordinates)
        {
            foreach (ICoordinate coordinate in coordinates)
            {
                yield return CoordinateFactory.Create(coordinate);
            }
        }

        private static void checkDimension(CoordinateDimensions dimension)
        {
            return;

            if (dimension != CoordinateDimensions.Two)
            {
                throw new NotSupportedException("Dimension can only be 2 for " +
                                                "this factory.");
            }
        }


        #region IMatrixFactory<DoubleComponent,Matrix> Member

        public Matrix CreateMatrix(int rowCount, int columnCount)
        {
            throw new NotImplementedException();
        }

        public Matrix CreateMatrix(int rowCount, int columnCount, IEnumerable<DoubleComponent> values)
        {
            throw new NotImplementedException();
        }

        public Matrix CreateMatrix(MatrixFormat format, int rowCount, int columnCount)
        {
            throw new NotImplementedException();
        }

        public Matrix CreateMatrix(Matrix matrix)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVectorFactory<DoubleComponent,Coordinate> Member

        public Coordinate CreateVector(int componentCount)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(IEnumerable<DoubleComponent> values)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(DoubleComponent a, DoubleComponent b)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(DoubleComponent a, DoubleComponent b, DoubleComponent c)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(params DoubleComponent[] components)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(double a, double b)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(double a, double b, double c)
        {
            throw new NotImplementedException();
        }

        public Coordinate CreateVector(params double[] components)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
