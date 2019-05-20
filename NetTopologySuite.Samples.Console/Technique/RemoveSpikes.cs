using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Technique
{
    public class RemoveSpikes
    {
        public class SpikeFixFilter : IGeometryFilter
        {
            private readonly double _spikeThreshold;

            public SpikeFixFilter()
                :this(1e-7)
            {
            }

            public SpikeFixFilter(double spikeThreshold)
            {
                _spikeThreshold = spikeThreshold;
            }

            public void Filter(Geometry geom)
            {
                if (geom is Point)
                    return;
                if (geom is LineString)
                    geom.Apply(new SpikeFixSequenceFilter(_spikeThreshold));
                if (geom is Polygon)
                {
                    var poly = (Polygon) geom;
                    poly.ExteriorRing.Apply(new SpikeFixSequenceFilter(_spikeThreshold));
                    for (int i = 0; i < poly.NumInteriorRings; i++)
                        poly.GetInteriorRingN(i).Apply(new SpikeFixSequenceFilter(_spikeThreshold));
                }
                if (geom is GeometryCollection)
                {
                    for (int i = 0; i < geom.NumGeometries; i++)
                        geom.GetGeometryN(i).Apply(new SpikeFixFilter(_spikeThreshold));
                }
            }

            private class SpikeFixSequenceFilter : ICoordinateSequenceFilter
            {
                private readonly double _spikeThreshold = 1e-7;

                private bool _changed;
                private LineSegment _lastSegment;

                public SpikeFixSequenceFilter(double spikeThreshold)
                {
                    _spikeThreshold = spikeThreshold;
                }

                public void Filter(CoordinateSequence seq, int i)
                {
                    if (seq == null)
                        throw new ArgumentNullException();

                    if (seq.Count == 1)
                        return;

                    if (i > seq.Count-2)
                        return;

                    LineSegment currentSegment;
                    SpikeFix spikeFix;
                    int index;
                    if (i == 0)
                    {
                        currentSegment = new LineSegment(seq.GetCoordinate(0), seq.GetCoordinate(1));
                        if (IsClosed(seq))
                        {
                            _lastSegment = new LineSegment(seq.GetCoordinate(seq.Count-2), seq.GetCoordinate(0));
                            spikeFix = CheckSpike(_lastSegment, currentSegment);
                            if (spikeFix != SpikeFix.NoSpike)
                            {
                                index = spikeFix == SpikeFix.First ? seq.Count - 2 : 1;
                                FixSpike(seq, seq.Count-1, index);
                                FixSpike(seq, 0, index);
                                _changed = true;
                            }
                        }
                        _lastSegment = currentSegment;
                        return;
                    }

                    currentSegment = new LineSegment(_lastSegment.P1, seq.GetCoordinate(i + 1));

                    spikeFix = CheckSpike(_lastSegment, currentSegment);
                    if (spikeFix != SpikeFix.NoSpike)
                    {

                        index = i + (int) spikeFix;
                        FixSpike(seq, i, index);

                        _changed = true;
                    }
                    _lastSegment = new LineSegment(seq.GetCoordinate(i), seq.GetCoordinate(i+1));
                }

                private static void FixSpike(CoordinateSequence seq, int fixIndex, int fixWithIndex)
                {
                    seq.SetOrdinate(fixIndex, Ordinate.X, seq.GetOrdinate(fixWithIndex, Ordinate.X));
                    seq.SetOrdinate(fixIndex, Ordinate.Y, seq.GetOrdinate(fixWithIndex, Ordinate.Y));

                    if ((seq.Ordinates & Ordinates.Z) == Ordinates.Z)
                        seq.SetOrdinate(fixIndex, Ordinate.Z, seq.GetOrdinate(fixWithIndex, Ordinate.Z));
                    if ((seq.Ordinates & Ordinates.M) == Ordinates.M)
                        seq.SetOrdinate(fixIndex, Ordinate.M, seq.GetOrdinate(fixWithIndex, Ordinate.M));
                }

                private enum SpikeFix
                {
                    NoSpike = 0,
                    First = -1,
                    Second = 1
                }

                private static bool IsClosed(CoordinateSequence seq)
                {
                    return seq.GetCoordinate(seq.Count - 1).Equals2D(seq.GetCoordinate(0));
                }

                private SpikeFix CheckSpike(LineSegment l1, LineSegment l2)
                {
                    if (l1.Length == 0 || l2.Length == 0)
                        return SpikeFix.NoSpike;
                    var l3 = new LineSegment(l1.P0, l2.P1);
                    if ((l1.Length + l3.Length) - l2.Length < _spikeThreshold)
                        return SpikeFix.First;

                    if ((l2.Length + l3.Length) -l1.Length < _spikeThreshold)
                        return SpikeFix.Second;

                    return SpikeFix.NoSpike;
                }

                public bool Done => false;

                public bool GeometryChanged => _changed;
            }
        }

        public class SpikeRemovingUtility
        {
            public static Geometry RemoveSpikes(Geometry geom, double spikeThreshold)
            {
                var filter = new SpikeFixFilter(spikeThreshold);
                var res = (Geometry) geom.Copy();
                res.Apply(filter);
                return res;
            }

            [TestCase(
                "POLYGON ((194908.68715217288 586962.86751464731, 194881.30215215127 586952.0195146437, 194879.05315214754 586952.15151464322, 194877.20115214764 586954.13551464747, 194831.95715210476 587019.88551468146, 194760.91615204382 587122.9405147346, 194857.09315212426 587178.23851475632, 194858.9451521278 587178.63551475492, 194860.26815212792 587177.44451475318, 194873.10015214139 587158.65951475059, 194925.75215218749 587081.797514704, 194953.13715220965 587042.10951468861, 194962.79415221984 587027.42551468522, 194985.68115224215 586994.0885146677, 194986.21015224193 586991.70651466073, 194985.54815224075 586989.32551465672, 194908.68715217288 586962.86751464731))",
                "POLYGON ((194886.66904433916 586975.65752607386, 194901.74579137619 586981.629866985, 194928.53316474415 586990.85093296878, 194935.04290785809 586971.94000373222, 194908.68715217288 586962.86751464731, 194881.30215215127 586952.0195146437, 194879.05315214754 586952.15151464322, 194877.20115214764 586954.13551464747, 194831.95715210476 587019.88551468146, 194821.8332618672 587034.57164674706, 194838.29986314307 587045.92290405277, 194848.42375338063 587031.23677198717, 194886.66904433916 586975.65752607386))"
                )]

            public static void Test(string wkt1, string wkt2)
            {
                var wktReader = new WKTReader(new GeometryFactory(new PrecisionModel(), 28992));
                var diff = wktReader.Read(wkt1).Difference(wktReader.Read(wkt2));

                Console.WriteLine(diff);
                var diffRs = RemoveSpikes(diff, 1e-5);
                Console.WriteLine(diffRs);
            }
        }

    }

    public class SpikeRemovingOperation : GeometryEditor.IGeometryEditorOperation
    {
        private readonly double _spikeThreshold;

        public SpikeRemovingOperation()
            :this(1e-7)
        {
        }

        public SpikeRemovingOperation(double spikeThreshold)
        {
            _spikeThreshold = spikeThreshold;
        }

        public Geometry Edit(Geometry geom, GeometryFactory factory)
        {
            factory = factory ?? geom.Factory;

            if (geom is Point)
                return factory.CreatePoint(((Point) geom).CoordinateSequence);

            if (geom is LineString)
            {
                return RemoveSpikesFromLineString((LineString) geom, factory);
            }
            if (geom is Polygon)
            {
                var poly = (Polygon)geom;
                var exteriorRing = RemoveSpikesFromLineString(poly.ExteriorRing, factory, true);
                var interiorRings = new List<LinearRing>(poly.NumInteriorRings);
                for (int i = 0; i < poly.NumInteriorRings; i++)
                    interiorRings.Add((LinearRing)RemoveSpikesFromLineString(poly.GetInteriorRingN(i), factory, true));
                return factory.CreatePolygon((LinearRing)exteriorRing, interiorRings.ToArray());
            }

            if (geom is GeometryCollection)
            {
                var lst = new List<Geometry>();
                for (int i = 0; i < geom.NumGeometries; i++)
                    lst.Add(Edit(geom.GetGeometryN(i), geom.Factory));
                return factory.CreateGeometryCollection(lst.ToArray());
            }
            Utilities.Assert.ShouldNeverReachHere("Unhandled geometry type");
            return null;
        }

        private LineString RemoveSpikesFromLineString(LineString geom, GeometryFactory factory, bool ring = false)
        {
            var seq = geom.CoordinateSequence;
            if (geom.Length < 3)
                return factory.CreateLineString(seq);

            //seq = RemoveSpikesFromSequence(geom.CoordinateSequence, factory.CoordinateSequenceFactory);
            return ring ? factory.CreateLinearRing(seq)
                        : factory.CreateLineString(seq);
        }
        /*
        private CoordinateSequence RemoveSpikesFromSequence(CoordinateSequence seq, CoordinateSequenceFactory factory)
        {
            LineSegment s1, s2;
            var res = factory.Create(seq.Count, seq.Ordinates);
            var newIndex = 0;
            if (IsClosed(seq))
            {
                s1 = new LineSegment(seq.GetCoordinate(seq.Count - 2), seq.GetCoordinate(0));
                s2 = new LineSegment(seq.GetCoordinate(0), seq.GetCoordinate(1));

                var spikeFix = CheckSpike(s1, s2);
                if (spikeFix != SpikeFix.NoSpike)
                {
                    res.SetOrdinate(0, Ordinate.X, s2.P1.X);
                    res.SetOrdinate(0, Ordinate.X, s2.P1.Y);
                    if (res.HasZ()) res.SetOrdinate(0, Ordinate.Z, seq.GetOrdinate(1, Ordinate.Z));
                    if (res.HasM()) res.SetOrdinate(0, Ordinate.M, seq.GetOrdinate(1, Ordinate.M));
                }
            }
            else
            {
                s1 = new LineSegment(seq.GetCoordinate(0), seq.GetCoordinate(1));
            }

            for (var i = 1; i < seq.Count; i++)
            {
                s2 = new LineSegment(seq.GetCoordinate(i), seq.GetCoordinate(i + 1));
                var spikeFix = CheckSpike(s1, s2);

                s1 = s2;
            }

        }

        private void CheckSpike(CoordinateSequence seq, int si1, int si2)
        {

        }

        private void CheckSpike(CoordinateSequence seq, int si1, int si2)
        {

        }*/

        private enum SpikeFix
        {
            NoSpike = 0,
            First = -1,
            Second = 1
        }

        private static bool IsClosed(CoordinateSequence seq)
        {
            return seq.GetCoordinate(seq.Count - 1).Equals2D(seq.GetCoordinate(0));
        }

        private SpikeFix CheckSpike(LineSegment l1, LineSegment l2)
        {
            if (l1.Length == 0 || l2.Length == 0)
                return SpikeFix.NoSpike;

            var l3 = new LineSegment(l1.P0, l2.P1);
            if ((l1.Length + l3.Length) - l2.Length < _spikeThreshold)
                return SpikeFix.Second;

            if ((l2.Length + l3.Length) - l1.Length < _spikeThreshold)
                return SpikeFix.First;

            return SpikeFix.NoSpike;
        }

    }
}
