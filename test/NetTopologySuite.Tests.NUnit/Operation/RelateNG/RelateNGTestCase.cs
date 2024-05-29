using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    public abstract class RelateNGTestCase : GeometryTestCase
    {

        private bool _isTrace = false;

        protected void CheckIntersectsDisjoint(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Intersects(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.Intersects(), wktb, wkta, expectedValue);
            CheckPredicate(RelatePredicate.Disjoint(), wkta, wktb, !expectedValue);
            CheckPredicate(RelatePredicate.Disjoint(), wktb, wkta, !expectedValue);
        }

        protected void CheckContainsWithin(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Contains(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.Within(), wktb, wkta, expectedValue);
        }

        protected void CheckCoversCoveredBy(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Covers(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.CoveredBy(), wktb, wkta, expectedValue);
        }

        protected void CheckCrosses(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Crosses(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.Crosses(), wktb, wkta, expectedValue);
        }

        protected void CheckOverlaps(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Overlaps(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.Overlaps(), wktb, wkta, expectedValue);
        }

        protected void CheckTouches(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.Touches(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.Touches(), wktb, wkta, expectedValue);
        }

        protected void CheckEquals(string wkta, string wktb, bool expectedValue)
        {
            CheckPredicate(RelatePredicate.EqualsTopo(), wkta, wktb, expectedValue);
            CheckPredicate(RelatePredicate.EqualsTopo(), wktb, wkta, expectedValue);
        }

        protected void CheckRelate(string wkta, string wktb, string expectedValue)
        {
            var a = Read(wkta);
            var b = Read(wktb);
            var pred = new RelateMatrixPredicate();
            var predTrace = Trace(pred);
            NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, predTrace);
            string actualVal = pred.IM.ToString();
            Assert.That(actualVal, Is.EqualTo(expectedValue));
        }

        protected void CheckRelateMatches(string wkta, string wktb, string pattern, bool expectedValue)
        {
            var pred = RelatePredicate.Matches(pattern);
            CheckPredicate(pred, wkta, wktb, expectedValue);
        }

        protected void CheckPredicate(TopologyPredicate pred, string wkta, string wktb, bool expectedValue)
        {
            var a = Read(wkta);
            var b = Read(wktb);
            var predTrace = Trace(pred);
            bool actualVal = NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, predTrace);
            Assert.That(actualVal, Is.EqualTo(expectedValue));
        }

        private TopologyPredicate Trace(TopologyPredicate pred)
        {
            if (!_isTrace)
                return pred;

            TestContext.WriteLine($"----------- Pred: {pred.Name}");

            return TopologyPredicateTracer.Trace(pred);
        }
    }
}
