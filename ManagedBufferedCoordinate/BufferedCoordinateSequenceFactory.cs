using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack;
using NPack.Interfaces;

namespace NetTopologySuite.Coordinates
{
    using IBufferedCoordFactory = ICoordinateFactory<BufferedCoordinate>;
    using IBufferedCoordSequence = ICoordinateSequence<BufferedCoordinate>;
    using IBufferedCoordSequenceFactory = ICoordinateSequenceFactory<BufferedCoordinate>;
    using GeoAPI.DataStructures;

    public class BufferedCoordinateSequenceFactory : IBufferedCoordSequenceFactory
    {
        private readonly BufferedCoordinateFactory _coordFactory;
        private readonly IVectorBuffer<DoubleComponent, BufferedCoordinate> _buffer;

        public BufferedCoordinateSequenceFactory()
            : this(new BufferedCoordinateFactory()) { }

        public BufferedCoordinateSequenceFactory(BufferedCoordinateFactory coordFactory)
        {
            if (coordFactory == null) throw new ArgumentNullException("coordFactory"); 
            
            _coordFactory = coordFactory;
            _buffer = _coordFactory.VectorBuffer;
        }

        public IComparer<BufferedCoordinate> DefaultComparer
        {
            get { return _coordFactory.Comparer; }
        }

        #region ICoordinateSequenceFactory<BufferedCoordinate> Members

        public IBufferedCoordFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public IBufferedCoordSequence Create(CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinateSequence(this, _buffer);
        }

        public IPrecisionModel<BufferedCoordinate> PrecisionModel
        {
            get { return _coordFactory.PrecisionModel; }
        }

        IPrecisionModel ICoordinateSequenceFactory.PrecisionModel
        {
            get { return PrecisionModel; }
        }

        public IBufferedCoordSequence Create(Int32 size, CoordinateDimensions dimension)
        {
            checkDimension(dimension);
            return new BufferedCoordinateSequence(size, this, _buffer);
        }

        public IBufferedCoordSequence Create(IBufferedCoordSequence coordSeq)
        {
            IBufferedCoordSequence newSequence = Create(0, CoordinateDimensions.Two);
            newSequence.AddSequence(coordSeq);
            return newSequence;
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
                                             IEnumerable<BufferedCoordinate> coordinates, 
                                             Boolean allowRepeated, 
                                             Boolean direction)
        {
            IBufferedCoordSequence newSequence = Create(CoordinateDimensions.Two);

            BufferedCoordinate lastCoord = new BufferedCoordinate();

            if (!direction)
            {
                coordinates = Enumerable.Reverse(coordinates);
            }

            foreach (BufferedCoordinate coordinate in coordinates)
            {
                BufferedCoordinate c = coordinate;

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

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate> coordinates, 
                                             Boolean allowRepeated, 
                                             Boolean direction)
        {
            return Create(null, coordinates, allowRepeated, direction);
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate> coordinates, Boolean allowRepeated)
        {
            return Create(componentTransform, coordinates, allowRepeated, true);
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate> coordinates, 
            Boolean allowRepeated)
        {
            return Create(null, coordinates, allowRepeated, true);
        }

        public IBufferedCoordSequence Create(Func<Double, Double> componentTransform, 
            IEnumerable<BufferedCoordinate> coordinates)
        {
            return Create(componentTransform, coordinates, true, true);
        }

        public IBufferedCoordSequence Create(IEnumerable<BufferedCoordinate> coordinates)
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

        public IBufferedCoordSequence Create(params BufferedCoordinate[] coordinates)
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

        private IEnumerable<BufferedCoordinate> convertCoordinates(IEnumerable<ICoordinate> coordinates)
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
                throw new NotSupportedException("Dimension can only be 2 for "+
                                                "this factory.");
            }
        }

    }
}
