using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// A predicate that matches a DE-9IM pattern.
    /// <para/>
    /// <h3>FUTURE WORK</h3>
    /// <para/>
    /// Extend the expressiveness of the DE-9IM pattern language to allow:
    /// <list type="bullet">
    /// <item><description>Combining patterns via disjunction using "|".</description></item>
    /// <item><description>Limiting patterns via geometry dimension.
    /// A dimension limit specifies the allowable dimensions
    /// for both or individual geometries as [d] or[ab] or[ab; cd]</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class IMPatternMatcher : IMPredicate
    {
        private readonly string _imPattern;
        private readonly IntersectionMatrix _patternMatrix;

        public IMPatternMatcher(string imPattern) : base("IMPattern")
        {
            _imPattern = imPattern;
            _patternMatrix = new IntersectionMatrix(imPattern);
        }

        //TODO: implement requiresExteriorCheck by inspecting matrix entries for E

        public override void Init(Envelope envA, Envelope envB)
        {
            Init(dimA, dimB);
            //-- if pattern specifies any non-E/non-E interaction, envelopes must not be disjoint
            bool requiresInteractionVal = RequireInteraction(_patternMatrix);
            bool isDisjoint = envA.Disjoint(envB);
            SetValueIf(false, requiresInteractionVal && isDisjoint);
        }

        public override bool RequireInteraction()
        {
            return RequireInteraction(_patternMatrix);
        }

        private static bool RequireInteraction(IntersectionMatrix im)
        {
            bool result =
                IsInteraction(im.Get(Location.Interior, Location.Interior))
                || IsInteraction(im.Get(Location.Interior, Location.Boundary))
                || IsInteraction(im.Get(Location.Boundary, Location.Interior))
                || IsInteraction(im.Get(Location.Boundary, Location.Boundary));
            return result;
        }

        private static bool IsInteraction(Dimension imDim)
        {
            return imDim == Dimension.True || imDim >= Dimension.P;
        }

        public override bool IsDetermined
        {
            get
            {
                /*
                 * Matrix entries only increase in dimension as topology is computed.
                 * The predicate can be short-circuited (as false) if
                 * any computed entry is greater than the mask value. 
                 */
                for (int i = 0; i < 3; i++)
                {
                    var locI = (Location)i;
                    for (int j = 0; j < 3; j++)
                    {
                        var locJ = (Location)j;
                        var patternEntry = _patternMatrix.Get(locI, locJ);

                        if (patternEntry == Dimension.Dontcare)
                            continue;

                        var matrixVal = GetDimension((Location)i, (Location)j);

                        //-- mask entry TRUE requires a known matrix entry
                        if (patternEntry == Dimension.True)
                        {
                            if (matrixVal < 0)
                                return false;
                        }
                        //-- result is known (false) if matrix entry has exceeded mask
                        else if (matrixVal > patternEntry)
                            return true;
                    }
                }
                return false;
            }
        }

        public override bool ValueIM 
        {
            get
            {
                bool val = intMatrix.Matches(_imPattern);
                return val;
            }
        }

        public override string ToString()
        {
            return $"Name ({_imPattern})";
        }
    }

}

