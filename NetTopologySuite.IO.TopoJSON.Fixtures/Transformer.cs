using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    public class Transformer
    {
        private readonly IGeometryFactory _factory;
        private readonly Transform _transform;
        private readonly Coordinate[][] _arcs;

        public Transformer(Transform transform, int[][][] arcs)
        {
            if (transform == null)
                throw new ArgumentNullException("transform");

            _transform = transform;
            _arcs = BuildArcs(arcs);

            _factory = GeometryFactory.Default;
        }

        private Coordinate[][] BuildArcs(int[][][] arcs)
        {
            Coordinate[][] list = new Coordinate[arcs.Length][];
            for (int i = 0; i < arcs.Length; i++)
            {
                int[] prev = { 0, 0 };
                int[][] arc = arcs[0];
                Coordinate[] conv = new Coordinate[arc.Length];
                for (int j = 0; j < arc.Length; j++)
                {
                    int[] pt = arc[j];
                    prev[0] += pt[0];
                    prev[1] += pt[1];
                    conv[j] = ConvertPoint(prev);
                }
                list[i] = conv;
            }
            return list;
        }

        private Coordinate ConvertPoint(int[] pt)
        {
            double[] scale = _transform.Scale;
            double[] translate = _transform.Translate;
            double x = pt[0] * scale[0] + translate[0];
            double y = pt[1] * scale[1] + translate[1];
            return new Coordinate(x, y);
        }

        public IGeometry Create(string type, int[] arcs)
        {
            switch (type)
            {
                case "LineString":
                    return CreateLineString(arcs);

                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new NotSupportedException(s);
            }
        }

        public IGeometry Create(string type, int[][] arcs)
        {
            switch (type)
            {
                case "Polygon":
                    return CreatePolygon(arcs);

                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new NotSupportedException(s);
            }
        }

        private IGeometry CreateLineString(int[] arcs)
        {
            Coordinate[] coords = InternalCreateLineString(arcs);
            return _factory.CreateLineString(coords);
            
        }

        private IGeometry CreatePolygon(int[][] arcs)
        {
            Coordinate[] cshell = InternalCreateLineString(arcs[0]);
            ILinearRing shell = _factory.CreateLinearRing(cshell);
            ILinearRing[] holes = new ILinearRing[arcs.Length - 1];
            for (int i = 1; i < arcs.Length; i++)
            {
                Coordinate[] chole = InternalCreateLineString(arcs[i]);
                holes[i - 1] = _factory.CreateLinearRing(chole);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private Coordinate[] InternalCreateLineString(int[] arcs)
        {
            List<Coordinate> list = new List<Coordinate>();
            for (int i = 0; i < arcs.Length; i++)
            {
                int el = arcs[i];
                Coordinate[] coords = _arcs[Math.Abs(el)];
                if (el < 0)
                {
                    /*
                     * https://github.com/topojson/topojson-specification#214-arc-indexes
                     * A negative arc index indicates that the arc 
                     * at the ones’ complement of the index must be reversed 
                     * to reconstruct the geometry: 
                     *   -1 refers to the reversed first arc, 
                     *   -2 refers to the reversed second arc, and so on.
                     */
                    coords = ReverseToCopy(coords);
                }                
                list.AddRange(coords);
            }
            return list.ToArray();
        }

        private static Coordinate[] ReverseToCopy(Coordinate[] arr)
        {
            int len = arr.Length;
            Coordinate[] clone = new Coordinate[len];
            for (int j = 0; j < len - 1; j++)
                clone[len - j - 1] = arr[j];
            return clone;
        }
    }
}