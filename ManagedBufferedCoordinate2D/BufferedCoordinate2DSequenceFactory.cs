using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
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
        {
            _coordFactory = new BufferedCoordinate2DFactory();
            _buffer = _coordFactory.VectorBuffer;
        }

        #region ICoordinateSequenceFactory<BufferedCoordinate2D> Members

        public IBufferedCoordFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public IBufferedCoordSequence Create(Int32 dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinate2DSequence(this, _buffer);
        }

        public IBufferedCoordSequence Create(Int32 size, Int32 dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinate2DSequence(size, this, _buffer);
        }

        public IBufferedCoordSequence Create(IBufferedCoordSequence coordSeq)
        {
            IBufferedCoordSequence newSequence = Create(coordSeq.Count, 2);
            newSequence.AddSequence(coordSeq);
            return newSequence;
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate2D> coordinates, Boolean allowRepeated, Boolean direction)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates, 
            Boolean allowRepeated, Boolean direction)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate2D> coordinates, Boolean allowRepeated)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates, 
            Boolean allowRepeated)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate2D> coordinates)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate2D> coordinates)
        {
            throw new NotImplementedException();
        }

        public IBufferedCoordSequence Create(params BufferedCoordinate2D[] coordinates)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICoordinateSequenceFactory Members

        ICoordinateSequence ICoordinateSequenceFactory.Create(Int32 dimension)
        {
            return Create(dimension);
        }

        ICoordinateSequence ICoordinateSequenceFactory.Create(Int32 size, Int32 dimension)
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

        private static void checkDimension(Int32 dimension)
        {
            if (dimension != 2)
            {
                throw new ArgumentOutOfRangeException("dimension", dimension,
                                                      "Dimension can only be 2 for this factory.");
            }
        }

    }
}
