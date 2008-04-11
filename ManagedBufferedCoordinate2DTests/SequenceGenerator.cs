using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;

namespace ManagedBufferedCoordinate2DTests
{
    internal class SequenceGenerator
    {
        private readonly Random _rnd = new MersenneTwister();
        private readonly Int32 _maxRandomLimit;

        public BufferedCoordinate2DFactory CoordinateFactory { get; private set; }
        public BufferedCoordinate2DSequenceFactory SequenceFactory { get; private set; }
        public ICoordinateSequence<BufferedCoordinate2D> Sequence { get; private set; }

        public List<BufferedCoordinate2D> MainList { get; private set; }
        public List<BufferedCoordinate2D> PrependList { get; private set; }
        public List<BufferedCoordinate2D> AppendList { get; private set; }

        public SequenceGenerator(Int32 max, Int32 mainCount)
            : this(max, mainCount, 0, 0)
        {
        }

        public SequenceGenerator(Int32 max,
            Int32 mainCount,
            Int32 prependCount,
            Int32 appendCount)
        {
            _maxRandomLimit = max;

            CoordinateFactory = new BufferedCoordinate2DFactory();
            SequenceFactory = new BufferedCoordinate2DSequenceFactory(CoordinateFactory);
            MainList = GenerateCoordinates(mainCount);
            PrependList = GenerateCoordinates(prependCount);
            AppendList = GenerateCoordinates(appendCount);

            if (MainList != null)
            {
                Sequence = SequenceFactory.Create(MainList);
            }
        }

        public BufferedCoordinate2D RandomCoordinate()
        {
            return RandomCoordinate(_maxRandomLimit);
        }

        public BufferedCoordinate2D RandomCoordinate(Int32 max)
        {
            return CoordinateFactory.Create(_rnd.Next(1, max + 1), _rnd.Next(1, max + 1));
        }

        public List<BufferedCoordinate2D> GenerateCoordinates(Int32 count)
        {
            return GenerateCoordinates(count, _maxRandomLimit);
        }

        public List<BufferedCoordinate2D> GenerateCoordinates(Int32 count, Int32 max)
        {
            if (count <= 0)
            {
                return null;
            }

            List<BufferedCoordinate2D> newList = new List<BufferedCoordinate2D>(count);
            while (count-- > 0)
            {
                newList.Add(CoordinateFactory.Create(_rnd.Next(1, max + 1), _rnd.Next(1, max + 1)));
            }
            return newList;
        }
    }
}
