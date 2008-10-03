using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;

namespace ManagedBufferedCoordinateTests
{
    internal class SequenceGenerator
    {
        public static readonly Int32 BigMaxLimit = Int32.MaxValue - 2;
        private readonly Random _rnd = new MersenneTwister();
        private readonly Int32 _maxRandomLimit;
        private readonly BufferedCoordinateFactory _coordFactory;
        private readonly BufferedCoordinateSequenceFactory _seqFactory;
        private ICoordinateSequence<BufferedCoordinate> _sequence;
        private List<BufferedCoordinate> _main;
        private List<BufferedCoordinate> _prepend;
        private List<BufferedCoordinate> _append;

        public BufferedCoordinateFactory CoordinateFactory 
        {
            get { return _coordFactory; }
        }

        public BufferedCoordinateSequenceFactory SequenceFactory 
        {
            get { return _seqFactory; }
        }

        public ICoordinateSequence<BufferedCoordinate> Sequence 
        {
            get { return _sequence; }
        }

        public List<BufferedCoordinate> MainList
        {
            get { return _main; }
        }
        
        public List<BufferedCoordinate> PrependList
        {
            get { return _prepend; }
        }
        
        public List<BufferedCoordinate> AppendList
        {
            get { return _append; }
        }

        public SequenceGenerator()
            : this(BigMaxLimit, 0, 0, 0) { }

        public SequenceGenerator(Int32 mainCount)
            : this(BigMaxLimit, mainCount, 0, 0) { }

        public SequenceGenerator(Int32 max, Int32 mainCount)
            : this(max, mainCount, 0, 0) { }
        
        public SequenceGenerator(Int32 mainCount,
                                 Int32 prependCount,
                                 Int32 appendCount)
            : this(BigMaxLimit, mainCount, prependCount, appendCount) { }

        public SequenceGenerator(Int32 max,
                                 Int32 mainCount,
                                 Int32 prependCount,
                                 Int32 appendCount)
        {
            _maxRandomLimit = max;

            _coordFactory = new BufferedCoordinateFactory();
            _seqFactory = new BufferedCoordinateSequenceFactory(CoordinateFactory);
            _main = new List<BufferedCoordinate>(GenerateCoordinates(mainCount));
            _prepend = new List<BufferedCoordinate>(GenerateCoordinates(prependCount));
            _append = new List<BufferedCoordinate>(GenerateCoordinates(appendCount));

            _sequence = SequenceFactory.Create(MainList);
        }

        public ICoordinateSequence<BufferedCoordinate> NewEmptySequence()
        {
            return SequenceFactory.Create();
        }

        public ICoordinateSequence<BufferedCoordinate> NewSequence()
        {
            return NewSequence(true);
        }

        public ICoordinateSequence<BufferedCoordinate> NewSequence(IEnumerable<BufferedCoordinate> coords)
        {
            return NewSequence(coords, true);
        }

        public ICoordinateSequence<BufferedCoordinate> NewSequence(Boolean allowRepeated)
        {
            IEnumerable<BufferedCoordinate> coords
                = GenerateCoordinates(MainList.Count, _maxRandomLimit);
            return NewSequence(coords, allowRepeated);
        }

        public ICoordinateSequence<BufferedCoordinate> NewSequence(IEnumerable<BufferedCoordinate> coords, Boolean allowRepeated)
        {
            return SequenceFactory.Create(coords, allowRepeated);
        }

        public BufferedCoordinate NewCoordinate(Double x, Double y)
        {
            return CoordinateFactory.Create(x, y);
        }

        public BufferedCoordinate RandomCoordinate()
        {
            return RandomCoordinate(_maxRandomLimit);
        }

        public BufferedCoordinate RandomCoordinate(Int32 max)
        {
            Double x = _rnd.Next(1, max + 1);
            Double y = _rnd.Next(1, max + 1);
            return NewCoordinate(x, y);
        }

        public IEnumerable<BufferedCoordinate> GenerateCoordinates(Int32 count)
        {
            return GenerateCoordinates(count, _maxRandomLimit);
        }

        public IEnumerable<BufferedCoordinate> GenerateCoordinates(Int32 count, Int32 max)
        {
            while (count-- > 0)
            {
                yield return RandomCoordinate(max);
            }
        }
    }
}
