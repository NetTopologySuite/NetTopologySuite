using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;

namespace SimpleCoordinateTests
{
    internal class SequenceGenerator
    {
        public static readonly Int32 BigMaxLimit = Int32.MaxValue - 2;
        private readonly Random _rnd = new MersenneTwister();
        private readonly Int32 _maxRandomLimit;
        private readonly CoordinateFactory _coordFactory;
        private readonly CoordinateSequenceFactory _seqFactory;
        private readonly ICoordinateSequence<Coordinate> _sequence;
        private readonly List<Coordinate> _main;
        private readonly List<Coordinate> _prepend;
        private readonly List<Coordinate> _append;

        public CoordinateFactory CoordinateFactory
        {
            get { return _coordFactory; }
        }

        public CoordinateSequenceFactory SequenceFactory
        {
            get { return _seqFactory; }
        }

        public ICoordinateSequence<Coordinate> Sequence
        {
            get { return _sequence; }
        }

        public List<Coordinate> MainList
        {
            get { return _main; }
        }

        public List<Coordinate> PrependList
        {
            get { return _prepend; }
        }

        public List<Coordinate> AppendList
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

            _coordFactory = new CoordinateFactory();
            _seqFactory = new CoordinateSequenceFactory(CoordinateFactory);
            _main = new List<Coordinate>(GenerateCoordinates(mainCount));
            _prepend = new List<Coordinate>(GenerateCoordinates(prependCount));
            _append = new List<Coordinate>(GenerateCoordinates(appendCount));

            _sequence = SequenceFactory.Create(MainList);
        }

        public ICoordinateSequence<Coordinate> NewEmptySequence()
        {
            return SequenceFactory.Create();
        }

        public ICoordinateSequence<Coordinate> NewSequence()
        {
            return NewSequence(true);
        }

        public ICoordinateSequence<Coordinate> NewSequence(IEnumerable<Coordinate> coords)
        {
            return NewSequence(coords, true);
        }

        public ICoordinateSequence<Coordinate> NewSequence(Boolean allowRepeated)
        {
            IEnumerable<Coordinate> coords
                = GenerateCoordinates(MainList.Count, _maxRandomLimit);
            return NewSequence(coords, allowRepeated);
        }

        public ICoordinateSequence<Coordinate> NewSequence(IEnumerable<Coordinate> coords, Boolean allowRepeated)
        {
            return SequenceFactory.Create(coords, allowRepeated);
        }

        public Coordinate NewCoordinate(Double x, Double y)
        {
            return CoordinateFactory.Create(x, y);
        }

        public Coordinate RandomCoordinate()
        {
            return RandomCoordinate(_maxRandomLimit);
        }

        public Coordinate RandomCoordinate(Int32 max)
        {
            Double x = _rnd.Next(1, max + 1);
            Double y = _rnd.Next(1, max + 1);
            return NewCoordinate(x, y);
        }

        public IEnumerable<Coordinate> GenerateCoordinates(Int32 count)
        {
            return GenerateCoordinates(count, _maxRandomLimit);
        }

        public IEnumerable<Coordinate> GenerateCoordinates(Int32 count, Int32 max)
        {
            while (count-- > 0)
            {
                yield return RandomCoordinate(max);
            }
        }
    }
}