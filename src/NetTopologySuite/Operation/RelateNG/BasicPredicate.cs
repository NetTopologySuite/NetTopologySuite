using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// The base class for relate topological predicates
    /// with a boolean value.
    /// Implements tri-state logic for the predicate value,
    /// to detect when the final value has been determined.
    /// </summary>
    /// <author>Martin Davis</author>
    internal abstract class BasicPredicate : TopologyPredicate
    {

        private const int UNKNOWN = -1;
        private const int FALSE = 0;
        private const int TRUE = 1;

        private static bool IsKnownCheck(int value)
        {
            return value > UNKNOWN;
        }

        private static bool ToBoolean(int value)
        {
            return value == TRUE;
        }

        private static int ToValue(bool val)
        {
            return val ? TRUE : FALSE;
        }

        /// <summary>
        /// Tests if two geometries intersect
        /// based on an interaction at given locations.
        /// </summary>
        /// <param name="locA">The location on geometry A</param>
        /// <param name="locB">The location on geometry B</param>
        /// <returns><c>true</c> if the geometries intersect</returns>
        public static bool IsIntersection(Location locA, Location locB)
        {
            //-- i.e. some location on both geometries intersects
            return locA != Location.Exterior && locB != Location.Exterior;
        }

        private int _value = UNKNOWN;

        protected BasicPredicate(string name) : base(name) { }

        /*
        public bool IsSelfNodingRequired => false;
         */

        public override bool IsKnown => IsKnownCheck(_value);

        public override bool Value => ToBoolean(_value);

        /// <summary>
        /// Updates the predicate value to the given state
        /// if it is currently unknown.
        /// </summary>
        /// <param name="val">The predicate value to update</param>
        protected void SetValue(bool val)
        {
            //-- don't change already-known value
            if (IsKnown)
                return;
            _value = ToValue(val);
        }

        protected void SetValue(int val)
        {
            //-- don't change already-known value
            if (IsKnown)
                return;
            _value = val;
        }

        protected void SetValueIf(bool value, bool cond)
        {
            if (cond)
                SetValue(value);
        }

        protected void Require(bool cond)
        {
            if (!cond)
                SetValue(false);
        }

        protected void RequireCovers(Envelope a, Envelope b)
        {
            Require(a.Covers(b));
        }
    }
}
