using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NetTopologySuite.Coordinates;
using NPack;

namespace ManagedBufferedCoordinate2DTests
{
    internal class SequenceGenerator
    {
        private Random _rnd = new MersenneTwister();

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
            CoordinateFactory = new BufferedCoordinate2DFactory();
            SequenceFactory = new BufferedCoordinate2DSequenceFactory(CoordinateFactory);
            MainList = generateCoordinates(mainCount, max);
            PrependList = generateCoordinates(prependCount, max);
            AppendList = generateCoordinates(appendCount, max);

            if (MainList != null)
            {
                Sequence = SequenceFactory.Create(MainList);
            }
        }

        private List<BufferedCoordinate2D> generateCoordinates(Int32 count, Int32 max)
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
