using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Simplify;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.NTS
{
    [TestFixture]
    public class Issue60
    {
        [Test]
        public void Test()
        {
            ICoordinateFactory cFactory = new CoordinateFactory();
            ICoordinateSequenceFactory seqFactory = new CoordinateSequenceFactory();
            List<ICoordinate> list = new List<ICoordinate>(
                new ICoordinate[] { cFactory.Create(10, 10), cFactory.Create(10, 20), cFactory.Create(20, 20),
                                    cFactory.Create(20, 10), cFactory.Create(30, 10)});
            ICoordinateSequence coordSeq = seqFactory.Create(list);
            Console.WriteLine(string.Format("Input: {0}", coordSeq));
            try
            {
                ICoordinateSequence<Coordinate> inputCoords = coordSeq as ICoordinateSequence<Coordinate>;
                ICoordinateSequence outputCoords = DouglasPeuckerLineSimplifier<Coordinate>.Simplify(inputCoords, 5);
                Console.WriteLine(string.Format("Output: {0}", outputCoords));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }
        }
    }
}
