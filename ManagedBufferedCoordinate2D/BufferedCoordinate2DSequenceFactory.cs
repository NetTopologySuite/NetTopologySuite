using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Utilities;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate2D>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate2D>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate2D>;

    public class BufferedCoordinate2DSequenceFactory : IBufferedCoordSequenceFactory
    {
        private readonly BufferedCoordinate2DFactory _coordFactory;
        private readonly IVectorBuffer<BufferedCoordinate2D, DoubleComponent> _buffer;

        public BufferedCoordinate2DSequenceFactory()
            : this(new BufferedCoordinate2DFactory()) { }

        public BufferedCoordinate2DSequenceFactory(BufferedCoordinate2DFactory coordFactory)
        {
            if (coordFactory == null) throw new ArgumentNullException("coordFactory"); 
            
            _coordFactory = coordFactory;
            _buffer = _coordFactory.VectorBuffer;
        }

        #region ICoordinateSequenceFactory<BufferedCoordinate2D> Members

        public IBufferedCoordFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public IBufferedCoordSequence Create(CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinate2DSequence(this, _buffer);
        }

        public IBufferedCoordSequence Create(Int32 size, CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinate2DSequence(size, this, _buffer);
        }

        public IBufferedCoordSequence Create(IBufferedCoordSequence coordSeq)
        {
            IBufferedCoordSequence newSequence = Create(coordSeq.Count, CoordinateDimensions.Two);
            newSequence.AddSequence(coordSeq);
            return newSequence;
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
                                             IEnumerable<BufferedCoordinate2D> coordinates, 
                                             Boolean allowRepeated, 
                                             Boolean direction)
        {
            IBufferedCoordSequence newSequence = Create(CoordinateDimensions.Two);

            BufferedCoordinate2D lastCoord = new BufferedCoordinate2D();

            if (!direction)
            {
                coordinates = Enumerable.Reverse(coordinates);
            }

            foreach (BufferedCoordinate2D coordinate in coordinates)
            {
                BufferedCoordinate2D c = coordinate;

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
            }

            return newSequence;
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates, 
                                             Boolean allowRepeated, 
                                             Boolean direction)
        {
            return Create(null, coordinates, allowRepeated, direction);
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate2D> coordinates, Boolean allowRepeated)
        {
            return Create(componentTransform, coordinates, allowRepeated, true);
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates, 
            Boolean allowRepeated)
        {
            return Create(null, coordinates, allowRepeated, true);
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate2D> coordinates)
        {
            return Create(componentTransform, coordinates, true, true);
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            return Create(null, coordinates, true, true);
        }

        public IBufferedCoordSequence Create(ICoordinateSequence coordSeq)
        {
            IBufferedCoordSequence converted = Create(coordSeq.Count, coordSeq.Dimension);

            foreach (ICoordinate coordinate in coordSeq)
            {
                converted.Add(coordinate);
            }

            return converted;
        }

        public IBufferedCoordSequence Create(params BufferedCoordinate2D[] coordinates)
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
            return (this as ICoordinateSequenceFactory).Create((IEnumerable<ICoordinate>) coordSeq);
        }

        ICoordinateSequence ICoordinateSequenceFactory.Create(IEnumerable<ICoordinate> coordinates)
        {
            return Create(convertCoordinates(coordinates));
        }

        #endregion

        private IEnumerable<BufferedCoordinate2D> convertCoordinates(IEnumerable<ICoordinate> coordinates)
        {
            foreach (ICoordinate coordinate in coordinates)
            {
                yield return CoordinateFactory.Create(coordinate);
            }
        }

        private static void checkDimension(CoordinateDimensions dimension)
        {
            if (dimension != CoordinateDimensions.Two)
            {
                throw new ArgumentOutOfRangeException("dimension", dimension,
                                                      "Dimension can only be 2 for "+
                                                      "this factory.");
            }
        }

    }
}
