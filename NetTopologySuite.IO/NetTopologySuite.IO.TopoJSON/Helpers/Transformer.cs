using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Geometries;

namespace NetTopologySuite.IO.Helpers
{
    public class Transformer : ITransformer
    {
        private static readonly IGeometryFactory DefaultFactory = GeometryFactory.Default;

        private readonly IGeometryFactory _factory;
        private readonly ITransform _transform;
        private readonly Coordinate[][] _arcs;

        public Transformer(double[][][] arcs) :
            this(arcs, DefaultFactory) { }

        public Transformer(double[][][] arcs, IGeometryFactory factory) :
            this(new Transform(), arcs, factory) { }

        public Transformer(ITransform transform, double[][][] arcs) :
            this(transform, arcs, DefaultFactory) { }

        public Transformer(ITransform transform, double[][][] arcs, IGeometryFactory factory)
        {
            if (transform == null)
                throw new ArgumentNullException("transform");

            _transform = transform;
            _arcs = BuildArcs(arcs);
            _factory = factory;
        }

        private Coordinate[][] BuildArcs(double[][][] arcs)
        {
            Coordinate[][] list = new Coordinate[arcs.Length][];
            for (int i = 0; i < arcs.Length; i++)
            {
                /*
                 * https://github.com/topojson/topojson-specification/blob/master/README.md#213-arcs
                 * If a topology is quantized, the positions of each arc 
                 * in the topology which are quantized must be delta-encoded. 
                 */
                double[] prev = { 0, 0 };
                double[][] arc = arcs[i];
                Coordinate[] conv = new Coordinate[arc.Length];
                for (int j = 0; j < arc.Length; j++)
                {
                    double[] pt = arc[j];
                    bool quantized = _transform.Quantized;
                    if (quantized)
                    {
                        prev[0] += pt[0];
                        prev[1] += pt[1];
                        conv[j] = ConvertPoint(prev);
                    }
                    else conv[j] = ConvertPoint(pt);
                }
                list[i] = conv;
            }
            return list;
        }

        private Coordinate ConvertPoint(double[] pt)
        {
            double[] scale = _transform.Scale;
            double[] translate = _transform.Translate;
            double x = pt[0] * scale[0] + translate[0];
            double y = pt[1] * scale[1] + translate[1];
            Coordinate c = new Coordinate(x, y);
            return c;
        }

        public FeatureCollection Create(TopoObject data)
        {
            if (data == null)
                throw new ArgumentNullException("data");
            string type = data.Type;
            if (String.IsNullOrEmpty(type))
                throw new ArgumentException("type undefined", "data");

            if (String.Equals("GeometryCollection", type))
            {
                // a TopoJSON "GeometryCollection" actually is an IFeature array
                // so we handle this stuff as a special case
                TopoCollection coll = (TopoCollection)data;
                return CreateCollection(coll.Geometries);
            }

            IGeometry geometry;
            switch (type)
            {
                case "Point":
                    TopoPoint point = (TopoPoint)data;
                    geometry = CreatePoint(point.Coordinates);
                    break;
                case "MultiPoint":
                    TopoMultiPoint mpoint = (TopoMultiPoint)data;
                    geometry = CreateMultiPoint(mpoint.Coordinates);
                    break;
                case "LineString":
                    TopoLineString lstring = (TopoLineString)data;
                    geometry = CreateLineString(lstring.Arcs);
                    break;
                case "MultiLineString":
                    TopoMultiLineString mlstring = (TopoMultiLineString)data;
                    geometry = CreateMultiLineString(mlstring.Arcs);
                    break;
                case "Polygon":
                    TopoPolygon poly = (TopoPolygon)data;
                    geometry = CreatePolygon(poly.Arcs);
                    break;
                case "MultiPolygon":
                    TopoMultiPolygon mpoly = (TopoMultiPolygon)data;
                    geometry = CreateMultiPolygon(mpoly.Arcs);
                    break;
                default:
                    string s = string.Format("type unsupported: {0}", type);
                    throw new NotSupportedException(s);
            }

            IAttributesTable properties = data.Properties;
            Feature feature = new Feature(geometry, properties);
            Collection<IFeature> collection = new Collection<IFeature> { feature };
            return new FeatureCollection(collection);
        }

        private IPoint CreatePoint(double[] data)
        {
            Coordinate coordinate = ConvertPoint(data);
            return _factory.CreatePoint(coordinate);
        }

        private IMultiPoint CreateMultiPoint(double[][] data)
        {
            IPoint[] list = new IPoint[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = CreatePoint(data[i]);
            return _factory.CreateMultiPoint(list);
        }

        private ILineString CreateLineString(int[] data)
        {
            Coordinate[] coords = InternalCreateLineString(data);
            return _factory.CreateLineString(coords);
        }

        private IMultiLineString CreateMultiLineString(int[][] data)
        {
            ILineString[] list = new ILineString[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = CreateLineString(data[i]);
            return _factory.CreateMultiLineString(list);
        }

        private IPolygon CreatePolygon(int[][] data)
        {
            Coordinate[] cshell = InternalCreateLineString(data[0]);
            ILinearRing shell = _factory.CreateLinearRing(cshell);
            ILinearRing[] holes = new ILinearRing[data.Length - 1];
            for (int i = 1; i < data.Length; i++)
            {
                Coordinate[] chole = InternalCreateLineString(data[i]);
                holes[i - 1] = _factory.CreateLinearRing(chole);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private IMultiPolygon CreateMultiPolygon(int[][][] data)
        {
            IPolygon[] list = new IPolygon[data.Length];
            for (int i = 0; i < data.Length; i++)
                list[i] = CreatePolygon(data[i]);
            return _factory.CreateMultiPolygon(list);
        }

        private FeatureCollection CreateCollection(TopoObject[] data)
        {
            IFeature[] features = new IFeature[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                TopoObject component = data[i];
                FeatureCollection coll = Create(component);
                features[i] = coll[0];
            }

            FeatureCollection collection = new FeatureCollection();
            foreach (IFeature feature in features)
                collection.Add(feature);
            return collection;
        }

        private Coordinate[] InternalCreateLineString(int[] data)
        {
            List<Coordinate> list = new List<Coordinate>();
            for (int i = 0; i < data.Length; i++)
            {
                int el = data[i];
                Coordinate[] coords;
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
                    int idx = Math.Abs(el) - 1;
                    Coordinate[] temp = _arcs[idx];
                    coords = MakeCopyReversed(temp);
                }
                else coords = _arcs[el];

                if (i > 0)
                {
                    /*
                     * https://github.com/topojson/topojson-specification#214-arc-indexes
                     * If more than one arc is referenced to construct a LineString or LinearRing, 
                     * the first position of a subsequent arc must be equal to the last position of the previous arc.
                     * Then, when reconstructing the geometry, the first position of each arc except the first 
                     * may be dropped; equivalently, the last position of each arc except the last may be dropped.
                     */
                    Coordinate last = list[list.Count - 1];
                    if (!coords[0].Equals(last))
                        throw new ArgumentException("invalid arc continuation");
                    /* no need to fix anything: NTS can handle stuff like this easily
                    Coordinate[] temp = new Coordinate[coords.Length - 1];
                    Array.Copy(coords, 1, temp, 0, temp.Length);
                    coords = temp;
                    */
                }
                list.AddRange(coords);
            }
            return list.ToArray();
        }

        private static Coordinate[] MakeCopyReversed(Coordinate[] arr)
        {
            int len = arr.Length;
            Coordinate[] clone = new Coordinate[len];
            for (int j = 0; j < len; j++)
                clone[len - j - 1] = arr[j];
            return clone;
        }
    }
}