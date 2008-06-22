using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;

namespace ManagedBufferedCoordinate2DTests
{
    internal class SequenceGenerator
    {
        public static readonly Int32 BigMaxLimit = Int32.MaxValue - 2;
        private readonly Random _rnd = new MersenneTwister();
        private readonly Int32 _maxRandomLimit;
        private readonly BufferedCoordinate2DFactory _coordFactory;
        private readonly BufferedCoordinate2DSequenceFactory _seqFactory;
        private ICoordinateSequence<BufferedCoordinate2D> _sequence;
        private List<BufferedCoordinate2D> _main;
        private List<BufferedCoordinate2D> _prepend;
        private List<BufferedCoordinate2D> _append;

        public BufferedCoordinate2DFactory CoordinateFactory 
        {
            get { return _coordFactory; }
        }

        public BufferedCoordinate2DSequenceFactory SequenceFactory 
        {
            get { return _seqFactory; }
        }

        public ICoordinateSequence<BufferedCoordinate2D> Sequence 
        {
            get { return _sequence; }
        }

        public List<BufferedCoordinate2D> MainList
        {
            get { return _main; }
        }
        
        public List<BufferedCoordinate2D> PrependList
        {
            get { return _prepend; }
        }
        
        public List<BufferedCoordinate2D> AppendList
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

            _coordFactory = new BufferedCoordinate2DFactory();
            _seqFactory = new BufferedCoordinate2DSequenceFactory(CoordinateFactory);
            _main = new List<BufferedCoordinate2D>(GenerateCoordinates(mainCount));
            _prepend = new List<BufferedCoordinate2D>(GenerateCoordinates(prependCount));
            _append = new List<BufferedCoordinate2D>(GenerateCoordinates(appendCount));

            _sequence = SequenceFactory.Create(MainList);
        }

        public ICoordinateSequence<BufferedCoordinate2D> NewEmptySequence()
        {
            return SequenceFactory.Create();
        }

        public ICoordinateSequence<BufferedCoordinate2D> NewSequence()
        {
            return NewSequence(true);
        }

        public ICoordinateSequence<BufferedCoordinate2D> NewSequence(IEnumerable<BufferedCoordinate2D> coords)
        {
            return NewSequence(coords, true);
        }

        public ICoordinateSequence<BufferedCoordinate2D> NewSequence(Boolean allowRepeated)
        {
            IEnumerable<BufferedCoordinate2D> coords
                = GenerateCoordinates(MainList.Count, _maxRandomLimit);
            return NewSequence(coords, allowRepeated);
        }

        public ICoordinateSequence<BufferedCoordinate2D> NewSequence(IEnumerable<BufferedCoordinate2D> coords, Boolean allowRepeated)
        {
            return SequenceFactory.Create(coords, allowRepeated);
        }

        public BufferedCoordinate2D NewCoordinate(Double x, Double y)
        {
            return CoordinateFactory.Create(x, y);
        }

        public BufferedCoordinate2D RandomCoordinate()
        {
            return RandomCoordinate(_maxRandomLimit);
        }

        public BufferedCoordinate2D RandomCoordinate(Int32 max)
        {
            Double x = _rnd.Next(1, max + 1);
            Double y = _rnd.Next(1, max + 1);
            return NewCoordinate(x, y);
        }

        public IEnumerable<BufferedCoordinate2D> GenerateCoordinates(Int32 count)
        {
            return GenerateCoordinates(count, _maxRandomLimit);
        }

        public IEnumerable<BufferedCoordinate2D> GenerateCoordinates(Int32 count, Int32 max)
        {
            while (count-- > 0)
            {
                yield return RandomCoordinate(max);
            }
        }
    }
}
