using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Operation.Union
{
    ///<summary>
    /// Experimental code to union MultiPolygons with processing limited to the elements which actually interact.
    ///</summary>
    /// <remarks>Not currently used, since it doesn't seem to offer much of a performance advantage.</remarks>
    /// <author>mbdavis</author>
    public class UnionInteracting
    {
        public static IGeometry Union(IGeometry g0, IGeometry g1)
        {
            UnionInteracting uue = new UnionInteracting(g0, g1);
            return uue.Union();
        }


        private readonly IGeometryFactory _geomFactory;

        private readonly IGeometry _g0;
        private readonly IGeometry _g1;

        private readonly bool[] _interacts0;
        private readonly bool[] _interacts1;

        public UnionInteracting(IGeometry g0, IGeometry g1)
        {
            _g0 = g0;
            _g1 = g1;
            _geomFactory = g0.Factory;
            _interacts0 = new bool[g0.NumGeometries];
            _interacts1 = new bool[g1.NumGeometries];
        }

        public IGeometry Union()
        {
            ComputeInteracting();

            // check for all interacting or none interacting!

            IGeometry int0 = ExtractElements(_g0, _interacts0, true);
            IGeometry int1 = ExtractElements(_g1, _interacts1, true);

            //		System.out.println(int0);
            //		System.out.println(int1);

            if (int0.IsEmpty || int1.IsEmpty)
            {
                System.Diagnostics.Debug.WriteLine("found empty!");
                //			computeInteracting();
            }
            //		if (! int0.isValid()) {
            //System.out.println(int0);
            //throw new RuntimeException("invalid geom!");
            //		}

            IGeometry union = int0.Union(int1);
            //Geometry union = BufferUnion(int0, int1);

            IGeometry disjoint0 = ExtractElements(_g0, _interacts0, false);
            IGeometry disjoint1 = ExtractElements(_g1, _interacts1, false);

            IGeometry overallUnion = GeometryCombiner.Combine(union, disjoint0, disjoint1);

            return overallUnion;

        }

        private IGeometry BufferUnion(IGeometry g0, IGeometry g1)
        {
            IGeometryFactory factory = g0.Factory;
            IGeometry gColl = factory.CreateGeometryCollection(new IGeometry[] { g0, g1 });
            IGeometry unionAll = gColl.Buffer(0.0);
            return unionAll;
        }

        private void ComputeInteracting()
        {
            for (int i = 0; i < _g0.NumGeometries; i++)
            {
                IGeometry elem = _g0.GetGeometryN(i);
                _interacts0[i] = ComputeInteracting(elem);
            }
        }

        private bool ComputeInteracting(IGeometry elem0)
        {
            bool interactsWithAny = false;
            for (int i = 0; i < _g1.NumGeometries; i++)
            {
                IGeometry elem1 = _g1.GetGeometryN(i);
                bool interacts = elem1.EnvelopeInternal.Intersects(elem0.EnvelopeInternal);
                if (interacts) _interacts1[i] = true;
                if (interacts)
                    interactsWithAny = true;
            }
            return interactsWithAny;
        }

        private IGeometry ExtractElements(IGeometry geom,
              bool[] interacts, bool isInteracting)
        {
            List<IGeometry> extractedGeoms = new List<IGeometry>();
            for (int i = 0; i < geom.NumGeometries; i++)
            {
                IGeometry elem = geom.GetGeometryN(i);
                if (interacts[i] == isInteracting)
                    extractedGeoms.Add(elem);
            }
            return _geomFactory.BuildGeometry(extractedGeoms);
        }


    }
}