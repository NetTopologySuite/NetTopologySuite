using NetTopologySuite.Geometries;
using System;
using System.Diagnostics;
using System.IO;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// Traces the evaluation of a <see cref="TopologyPredicate"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public static class TopologyPredicateTracer
    {
        /// <summary>
        /// Creates a new predicate tracing the evaluation of a given predicate.
        /// </summary>
        /// <param name="pred">The predicate to trace</param>
        /// <returns>the traceable predicate</returns>
        public static TopologyPredicate Trace(TopologyPredicate pred)
            => Trace(pred, new TextWriterTraceListener().Writer);

        /// <summary>
        /// Creates a new predicate tracing the evaluation of a given predicate.
        /// </summary>
        /// <param name="pred">The predicate to trace</param>
        /// <param name="tw">A text writer</param>
        /// <returns>the traceable predicate</returns>
        public static TopologyPredicate Trace(TopologyPredicate pred, TextWriter tw)
            => new PredicateTracer(pred, tw ?? Console.Out);
        

        private class PredicateTracer : TopologyPredicate
        {
            private readonly TopologyPredicate pred;
            private readonly TextWriter tw;

            public PredicateTracer(TopologyPredicate pred, TextWriter tw) : base($"{pred.Name}(traced)")
            {
                this.pred = pred;
                this.tw = tw;
            }

            public override bool RequireSelfNoding()
                => pred.RequireSelfNoding();

            public override bool RequireInteraction()
                => pred.RequireInteraction();

            public override bool RequireCovers(bool isSourceA)
                    => pred.RequireCovers(isSourceA);

            public override bool RequireExteriorCheck(bool isSourceA)
                    => pred.RequireExteriorCheck(isSourceA);

            public override void Init(Dimension dimA, Dimension dimB)
            {
                pred.Init(dimA, dimB);
                CheckValue("dimensions");
            }

            public override void Init(Envelope envA, Envelope envB)
            {
                pred.Init(envA, envB);
                CheckValue("envelopes");
            }

            public override void UpdateDimension(Location locA, Location locB, Dimension dimension)
            {
                string desc = "A:" + LocationUtility.ToLocationSymbol(locA)
                  + "/B:" + LocationUtility.ToLocationSymbol(locB)
                  + " -> " + dimension;
                string ind = "";
                bool isChanged = IsDimChanged(locA, locB, dimension);
                if (isChanged)
                {
                    ind = " <<< ";
                }
                tw.WriteLine(desc + ind);
                pred.UpdateDimension(locA, locB, dimension);
                if (isChanged)
                {
                    CheckValue("IM entry");
                }
            }

            private bool IsDimChanged(Location locA, Location locB, Dimension dimension)
            {
                if (pred is IMPredicate iMPred)
                {
                    return iMPred.IsDimChanged(locA, locB, dimension);
                }
                return false;
            }

            private void CheckValue(string source)
            {
                if (pred.IsKnown)
                {
                    tw.WriteLine($"{pred.Name} = {pred.Value} based on {source}");
                }
            }

            public override void Finish() => pred.Finish();

            public override bool IsKnown => pred.IsKnown;

            public override bool Value => pred.Value;

            public override string ToString()
            {
                return pred.ToString();
            }
        }
    }
}
