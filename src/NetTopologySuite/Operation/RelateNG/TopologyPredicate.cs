using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.RelateNG
{
    /// <summary>
    /// The API for strategy classes implementing
    /// spatial predicates based on the DE-9IM topology model.
    /// Predicate values for specific geometry pairs can be evaluated by <see cref="RelateNG"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public abstract class TopologyPredicate
    {
        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="name">The name of the predicate</param>
        protected TopologyPredicate(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the predicate
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Reports whether this predicate requires self-noding for
        /// geometries which contain crossing edges
        /// (for example, <see cref="LineString"/>s, or <see cref="GeometryCollection"/>s
        /// containing lines or polygons which may self-intersect).
        /// Self-noding ensures that intersections are computed consistently
        /// in cases which contain self-crossings and mutual crossings.
        /// <para/>
        /// Most predicates require this, but it can
        /// be avoided for simple intersection detection
        /// (such as in <see cref="RelatePredicate.Intersects()"/>
        /// and <see cref="RelatePredicate.Disjoint()"/>.
        /// Avoiding self-noding improves performance for polygonal inputs.
        /// </summary>
        public virtual bool RequireSelfNoding() { return true; }

        /// <summary>
        /// Reports whether this predicate requires interaction between
        /// the input geometries.
        /// This is the case if
        /// <code>
        /// IM[I, I] >= 0 or IM[I, B] >= 0 or IM[B, I] >= 0 or IM[B, B] >= 0
        /// </code>
        /// This allows a fast result if
        /// the envelopes of the geometries are disjoint.
        /// </summary>
        /// <returns><c>true</c> if the geometries must interact</returns>
        public virtual bool RequireInteraction()
        {
            return true;
        }

        /// <summary>
        /// Reports whether this predicate requires that the source
        /// cover the target.
        /// This is the case if
        /// <code>
        /// IM[Ext(Src), Int(Tgt)] = F and IM[Ext(Src), Bdy(Tgt)] = F
        /// </code>
        /// If <c>true</c>, this allows a fast result if
        /// the source envelope does not cover the target envelope.
        /// </summary>
        /// <param name="isSourceA">A flag indicating the source input geometry</param>
        /// <returns><c>true</c> if the predicate requires checking whether the source covers the target</returns>
        public virtual bool RequireCovers(bool isSourceA)
        {
            return false;
        }

        /// <summary>
        /// Reports whether this predicate requires checking if the source input intersects
        /// the Exterior of the target input.
        /// This is the case if:
        /// <code>
        /// IM[Int(Src), Ext(Tgt)] >= 0 or IM[Bdy(Src), Ext(Tgt)] >= 0
        /// </code>
        /// If <c>false</c>, this may permit a faster result in some geometric situations.
        /// </summary>
        /// <param name="isSourceA">A flag indicating the source input geometry</param>
        /// <returns><c>true</c> if the predicate requires checking whether the source intersects the target exterior</returns>
        public virtual bool RequireExteriorCheck(bool isSourceA)
        {
            return true;
        }

        /// <summary>
        /// Initializes the predicate for a specific geometric case.
        /// This may allow the predicate result to become known
        /// if it can be inferred from the dimensions.
        /// </summary>
        /// <param name="dimA">The dimension of geometry A</param>
        /// <param name="dimB">The dimension of geometry B</param>
        /// <seealso cref="Dimension"/>
        public virtual void Init(Dimension dimA, Dimension dimB)
        {
            //-- default if dimensions provide no information
        }

        /// <summary>
        /// Initializes the predicate for a specific geometric case.
        /// This may allow the predicate result to become known
        /// if it can be inferred from the envelopes.
        /// </summary>
        /// <param name="envA">The envelope of geometry A</param>
        /// <param name="envB">The envelope of geometry B</param>
        public virtual void Init(Envelope envA, Envelope envB)
        {
            //-- default if envelopes provide no information
        }

        /// <summary>
        /// Updates the entry in the DE-9IM intersection matrix
        /// for given {@link Location}s in the input geometries.
        /// <para/>
        /// If this method is called with a {@link Dimension} value
        /// which is less than the current value for the matrix entry,
        /// the implementing class should avoid changing the entry
        /// if this would cause information loss.
        /// </summary>
        /// <param name="locA">The location on the A axis of the matrix</param>
        /// <param name="locB">The location on the B axis of the matrix</param>
        /// <param name="dimension">The dimension value for the entry</param>
        /// <seealso cref="Location"/>
        /// <seealso cref="Dimension"/>
        public abstract void UpdateDimension(Location locA, Location locB, Dimension dimension);

        /// <summary>
        /// Indicates that the value of the predicate can be finalized
        /// based on its current state.
        /// </summary>
        public abstract void Finish();

        /// <summary>
        /// Tests if the predicate value is known.
        /// </summary>
        public abstract bool IsKnown { get; }

        /// <summary>
        /// Gets the current value of the predicate result.
        /// The value is only valid if <see cref="IsKnown"/> is <c>true</c>.
        /// </summary>
        public abstract bool Value { get; }
    }
}
