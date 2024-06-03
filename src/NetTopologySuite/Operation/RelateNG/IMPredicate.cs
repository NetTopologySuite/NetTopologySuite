using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// A base class for predicates which are
    /// determined using entries in a <see cref="IntersectionMatrix"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal abstract class IMPredicate : BasicPredicate
    {
        public static bool IsDimsCompatibleWithCovers(Dimension dim0, Dimension dim1)
        {
            //- allow Points coveredBy zero-length Lines
            if (dim0 == Dimension.P && dim1 == Dimension.L)
                return true;
            return dim0 >= dim1;
        }

        const Dimension DIM_UNKNOWN = Dimension.Dontcare;

        /// <summary>
        /// Dimenension of geometry A
        /// </summary>
        protected Dimension dimA;
        /// <summary>
        /// Dimenension of geometry B
        /// </summary>
        protected Dimension dimB;
        /// <summary>
        /// The intersection matrix
        /// </summary>
        protected readonly IntersectionMatrix intMatrix;

        protected IMPredicate(string name) : base(name)
        {
            intMatrix = new IntersectionMatrix();
            //-- E/E is always dim = 2
            intMatrix.Set(Location.Exterior, Location.Exterior, Dimension.A);
        }

        public override void Init(Dimension dimA, Dimension dimB)
        {
            this.dimA = dimA;
            this.dimB = dimB;
        }

        public override void UpdateDimension(Location locA, Location locB, Dimension dimension)
        {
            //-- only record an increased dimension value
            if (IsDimChanged(locA, locB, dimension))
            {
                intMatrix.Set(locA, locB, dimension);
                //-- set value if predicate value can be known
                if (IsDetermined)
                {
                    SetValue(ValueIM);
                }
            }
        }

        public bool IsDimChanged(Location locA, Location locB, Dimension dimension)
        {
            return dimension > intMatrix.Get(locA, locB);
        }

        /// <summary>
        /// Tests whether predicate evaluation can be short-circuited
        /// due to the current state of the matrix providing
        /// enough information to determine the predicate value.
        /// <para/>
        /// If this value is true then <see cref="ValueIM"/>
        /// must provide the correct result of the predicate.
        /// </summary>
        public abstract bool IsDetermined { get; }

        /// <summary>
        /// Tests whether the exterior of the specified input geometry
        /// is intersected by any part of the other input.
        /// </summary>
        /// <param name="isA">A flag defining the input geometry</param>
        /// <returns><c>true</c> if the input geometry exterior is intersected</returns>
        protected bool IntersectsExteriorOf(bool isA)
        {
            if (isA)
            {
                return IsIntersects(Location.Exterior, Location.Interior)
                    || IsIntersects(Location.Exterior, Location.Boundary);
            }
            else
            {
                return IsIntersects(Location.Interior, Location.Exterior)
                    || IsIntersects(Location.Boundary, Location.Exterior);
            }
        }

        protected bool IsIntersects(Location locA, Location locB)
        {
            return intMatrix.Get(locA, locB) >= Dimension.P;
        }

        protected bool IsDimensionKnown(Location locA, Location locB)
        {
            return intMatrix.Get(locA, locB) != DIM_UNKNOWN;
        }

        public bool IsDimension(Location locA, Location locB, Dimension dimension)
        {
            return intMatrix.Get(locA, locB) == dimension;
        }

        public Dimension GetDimension(Location locA, Location locB)
        {
            return intMatrix.Get(locA, locB);
        }

        /// <summary>
        /// Sets the final value based on the state of the IM.
        /// </summary>
        public override void Finish()
        {
            SetValue(ValueIM);
        }

        /// <summary>
        /// Gets the value of the predicate according to the current
        /// intersection matrix state.
        /// </summary>
        public abstract bool ValueIM { get; }

        public override string ToString()
        {
            return $"{Name}: {intMatrix}";
        }

    }
}

