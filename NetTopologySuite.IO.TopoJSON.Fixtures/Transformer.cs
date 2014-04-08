using System;
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

        private IGeometry CreatePolygon(int[][] arcs)
        {
            Coordinate[][] parts = InternalCreateLineString(arcs[0]);
            if (parts.Length > 1)
                throw new ArgumentException("only a single shell compoient expected");

            ILinearRing shell = _factory.CreateLinearRing(parts[0]);
            ILinearRing[] holes = new ILinearRing[arcs.Length - 1];
            for (int i = 1; i < arcs.Length; i++)
            {
                parts = InternalCreateLineString(arcs[i]);
                if (parts.Length > 1)
                    throw new ArgumentException("only a single hole component expected");
                holes[i - 1] = _factory.CreateLinearRing(parts[0]);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private Coordinate[][] InternalCreateLineString(int[] arcs)
        {
            Coordinate[][] list = new Coordinate[arcs.Length][];
            for (int i = 0; i < arcs.Length; i++)
            {
                if (arcs[i] < 0)
                    throw new NotImplementedException("TODO: reverse");
                list[i] = _arcs[arcs[i]];
            }
            return list;
        }

        private ILinearRing Ring(Coordinate[] coords)
        {
            return _factory.CreateLinearRing(coords);
        }
    }
}