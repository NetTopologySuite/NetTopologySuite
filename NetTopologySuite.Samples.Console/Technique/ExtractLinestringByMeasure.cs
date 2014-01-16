using System;
using System.Collections.Generic;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Technique
{
    public class ExtractLinestringByMeasure
    {
        private readonly IGeometryFactory _factory = new GeometryFactory(DotSpatialAffineCoordinateSequenceFactory.Instance);

        [Test]
        public void Test1()
        {
            var seq = _factory.CoordinateSequenceFactory.Create(3, Ordinates.XYZM);

            seq.SetOrdinate(0, Ordinate.X, 0);
            seq.SetOrdinate(0, Ordinate.Y, 0);
            seq.SetOrdinate(0, Ordinate.Z, 0);
            seq.SetOrdinate(0, Ordinate.M, 0);

            seq.SetOrdinate(1, Ordinate.X, 1000);
            seq.SetOrdinate(1, Ordinate.Y, 0);
            seq.SetOrdinate(1, Ordinate.Z, 0);
            seq.SetOrdinate(1, Ordinate.M, 100);

            seq.SetOrdinate(2, Ordinate.X, 2000);
            seq.SetOrdinate(2, Ordinate.Y, 0);
            seq.SetOrdinate(2, Ordinate.Z, 0);
            seq.SetOrdinate(2, Ordinate.M, 0);

            var l = _factory.CreateLineString(seq);

            var lmf = new LinestringMeasureFilter(Interval.Create(25, 75));
            l.Apply(lmf);

            Assert.IsTrue(lmf.Filtered.Count == 2);
            var lf = (ILineString)lmf.Filtered[0];
            Assert.AreEqual(250, lf.StartPoint.X, 1e-7);
            Assert.AreEqual(750, lf.EndPoint.X, 1e-7);

            lf = (ILineString)lmf.Filtered[1];
            Assert.AreEqual(1250, lf.StartPoint.X, 1e-7);
            Assert.AreEqual(1750, lf.EndPoint.X, 1e-7);
        }
    }

    public class LinestringMeasureFilter : IGeometryFilter
    {
        private Interval _measureInterval;
        private readonly List<IGeometry> _geoms = new List<IGeometry>();

        public LinestringMeasureFilter()
            :this(Interval.Create(double.MinValue, double.MaxValue))
        {}

        public LinestringMeasureFilter(Interval create)
        {
            _measureInterval = create;
        }

        public void Filter(IGeometry geom)
        {
            if (!(geom is ILineString))
                return;

            Filter(((ILineString)geom));
        }

        private void Filter(ILineString lineString)
        {
            if (!IsValidLinestring(lineString))
                return;

            var tuples = new List<Tuple<Coordinate, double>>();

            var sequence = lineString.CoordinateSequence;
            var startMeasure = sequence.GetOrdinate(0, Ordinate.M);
            var startRelation = _measureInterval.Relation(startMeasure);
            if (startRelation == IntervalRelation.In)
            {
                tuples.Add(Tuple.Create(sequence.GetCoordinate(0), startMeasure));
            }


            for (var i = 1; i < sequence.Count; i++)
            {
                var measure = sequence.GetOrdinate(i, Ordinate.M);
                var relation = _measureInterval.Relation(measure);

                if (relation == IntervalRelation.In)
                {
                    tuples.Add(Tuple.Create(sequence.GetCoordinate(0), startMeasure));
                }
                else
                {
                    var segment = new LineSegment(sequence.GetCoordinate(i - 1), sequence.GetCoordinate(i));
                    if (startRelation == IntervalRelation.Above)
                    {
                        switch (relation)
                        {
                            case IntervalRelation.Above:
                                break;
                            case IntervalRelation.In:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                break;
                            case IntervalRelation.Below:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Min, measure, segment));
                                _geoms.Add(ToLinestring(lineString.Factory, sequence.Ordinates, tuples));
                                break;
                        }
                    } 
                    else if (startRelation == IntervalRelation.Below)
                    {
                        switch (relation)
                        {
                            case IntervalRelation.Above:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Min, measure, segment));
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                _geoms.Add(ToLinestring(lineString.Factory, sequence.Ordinates, tuples));
                                break;
                            case IntervalRelation.In:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                break;
                            case IntervalRelation.Below:
                                break;
                        }
                    }
                    else
                    {

                        switch (relation)
                        {
                            case IntervalRelation.Above:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                break;
                            case IntervalRelation.In:
                                break;
                            case IntervalRelation.Below:
                                tuples.Add(Interpolate(startMeasure, _measureInterval.Max, measure, segment));
                                break;
                        }
                    }
                }

                if (startRelation == IntervalRelation.In && relation != IntervalRelation.In)
                {
                    _geoms.Add(ToLinestring(lineString.Factory, sequence.Ordinates, tuples));
                }

                startMeasure = measure;
                startRelation = relation;
            }

            Utilities.Assert.IsTrue(tuples.Count == 0);
        }

        private static IGeometry ToLinestring(IGeometryFactory factory, Ordinates ordinates,
            List<Tuple<Coordinate, double>> tuples)
        {
            var seq = factory.CoordinateSequenceFactory.Create(tuples.Count, ordinates);
            for (var i = 0; i < tuples.Count; i++)
            {
                seq.SetOrdinate(i, Ordinate.X, tuples[i].Item1.X);
                seq.SetOrdinate(i, Ordinate.Y, tuples[i].Item1.Y);
                seq.SetOrdinate(i, Ordinate.Z, tuples[i].Item1.Z);
                seq.SetOrdinate(i, Ordinate.M, tuples[i].Item2);
            }
            var lineString = factory.CreateLineString(seq);
            tuples.Clear();
            return lineString;
        }

        private static Tuple<Coordinate, double> Interpolate(double m1, double measure, double m2, LineSegment segment)
        {
            var measureFraction = (measure - m1) / (m2 - m1);
            return Tuple.Create(segment.PointAlong(measureFraction), measure);
        }

        private static bool IsValidLinestring(ILineString lineString)
        {
            if (lineString == null)
            {
                return false;
                throw new ArgumentNullException("lineString");
            }

            if (lineString.IsEmpty)
            {
                return false;
                throw new ArgumentException("Linestring is empty", "lineString");
            }

            var sequence = lineString.CoordinateSequence;
            if ((sequence.Ordinates & Ordinates.M) == Ordinates.None)
            {
                return false;
                throw new ArgumentException("No Measure value");
            }

            return true;

        }

        public IList<IGeometry> Filtered { get { return _geoms.AsReadOnly(); }}
    }

    internal enum IntervalRelation
    {
        Below = -1,
        In = 0,
        Above = 1
    }

    internal static class IntervalExtensions
    {
        public static IntervalRelation Relation(this Interval self, double value)
        {
            if (value < self.Min) return IntervalRelation.Below;
            if (value < self.Max) return IntervalRelation.In;
            return IntervalRelation.Above;
        }
    }
}