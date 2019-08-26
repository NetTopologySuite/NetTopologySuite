using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Experimental code to union MultiPolygons with processing limited to the elements which actually interact.
    /// </summary>
    /// <remarks>Not currently used, since it doesn't seem to offer much of a performance advantage.</remarks>
    /// <author>mbdavis</author>
    public class UnionInteracting
    {
        public static Geometry Union(Geometry g0, Geometry g1)
        {
            var uue = new UnionInteracting(g0, g1);
            return uue.Union();
        }

        private readonly GeometryFactory _geomFactory;

        private readonly Geometry _g0;
        private readonly Geometry _g1;

        private readonly bool[] _interacts0;
        private readonly bool[] _interacts1;

        public UnionInteracting(Geometry g0, Geometry g1)
        {
            _g0 = g0;
            _g1 = g1;
            _geomFactory = g0.Factory;
            _interacts0 = new bool[g0.NumGeometries];
            _interacts1 = new bool[g1.NumGeometries];
        }

        public Geometry Union()
        {
            ComputeInteracting();

            // check for all interacting or none interacting!
            var int0 = ExtractElements(_g0, _interacts0, true);
            var int1 = ExtractElements(_g1, _interacts1, true);

            if (int0.IsEmpty || int1.IsEmpty)
                Debug.WriteLine("found empty!");

            var union = int0.Union(int1);
            var disjoint0 = ExtractElements(_g0, _interacts0, false);
            var disjoint1 = ExtractElements(_g1, _interacts1, false);
            var overallUnion = GeometryCombiner.Combine(union, disjoint0, disjoint1);
            return overallUnion;

        }

        private Geometry BufferUnion(Geometry g0, Geometry g1)
        {
            var factory = g0.Factory;
            var gColl = factory.CreateGeometryCollection(new Geometry[] { g0, g1 });
            var unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        private void ComputeInteracting()
        {
            for (int i = 0; i < _g0.NumGeometries; i++)
            {
                var elem = _g0.GetGeometryN(i);
                _interacts0[i] = ComputeInteracting(elem);
            }
        }

        private bool ComputeInteracting(Geometry elem0)
        {
            bool interactsWithAny = false;
            for (int i = 0; i < _g1.NumGeometries; i++)
            {
                var elem1 = _g1.GetGeometryN(i);
                bool interacts = elem1.EnvelopeInternal.Intersects(elem0.EnvelopeInternal);
                if (interacts) _interacts1[i] = true;
                if (interacts)
                    interactsWithAny = true;
            }
            return interactsWithAny;
        }

        private Geometry ExtractElements(Geometry geom,
              bool[] interacts, bool isInteracting)
        {
            var extractedGeoms = new List<Geometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                var elem = geom.GetGeometryN(i);
                if (interacts[i] == isInteracting)
                    extractedGeoms.Add(elem);
            }
            return _geomFactory.BuildGeometry(extractedGeoms);
        }

    }
}