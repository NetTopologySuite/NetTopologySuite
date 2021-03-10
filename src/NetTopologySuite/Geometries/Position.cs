using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Indicates the position of a location relative to a
    /// node or edge component of a planar topological structure.
    /// </summary>
    public readonly struct Position
    {
        internal const int IndexOn = 0;
        internal const int IndexLeft = 1;
        internal const int IndexRight = 2;
        private const int IndexParallel = -1;

        /// <summary>
        /// Specifies that a location is <c>on</c> a component
        /// </summary>
        /// <value>0</value>
        public static Position On = new Position(IndexOn);

        /// <summary>
        /// Specifies that a location is to the <c>left</c> of a component
        /// </summary>
        /// <value>1</value>
        public static Position Left = new Position(IndexLeft);

        /// <summary>
        /// Specifies that a location is to the <c>right</c> of a component
        /// </summary>
        /// <value>2</value>
        public static Position Right = new Position(IndexRight);

        /// <summary>
        /// Specifies that a location is <c>is parallel to x-axis</c> of a component
        /// </summary>
        /// <value>-1</value>
        public static Position Parallel = new Position(IndexParallel);

        /// <summary>
        /// Creates a new position index
        /// </summary>
        /// <param name="index">A position index</param>
        internal Position(int index)
        {
            if (index < IndexParallel || IndexRight < index)
                throw new ArgumentOutOfRangeException(nameof(index));
            Index = index;
        }

        /// <summary>
        /// Gets a value indicating the position index
        /// </summary>
        internal int Index { get; }

        /// <summary>
        /// Returns <see cref="Position.Left"/> if the position is <see cref="Position.Right"/>,
        /// <see cref="Position.Right"/> if the position is <see cref="Position.Left"/>, or the position
        /// otherwise.
        /// </summary>
        public Position Opposite
        {
            get
            {
                if (this == Position.Left)
                    return Position.Right;
                if (this == Position.Right)
                    return Position.Left;
                return this;
            }
        }

        #region object overrides

        public override bool Equals(object obj)
        {
            if (obj is Position p)
                return Index == p.Index;
            return false;
        }

        public override int GetHashCode()
        {
            return 17 ^ Index.GetHashCode();
        }

        public override string ToString()
        {
            switch (Index)
            {
                case 0:
                    return "On(=0)";
                case 1:
                    return "Left(=1)";
                case 2:
                    return "Right(=2)";
                case -1:
                    return "Parallel(=-1)";
            }
            throw new InvalidOperationException();
        }

        #endregion

        /// <summary>
        /// Equality comparer for <see cref="Position"/> indices
        /// </summary>
        /// <param name="lhs">The position index on the left-hand-side</param>
        /// <param name="rhs">The position index on the right-hand-side</param>
        /// <returns><c>true</c> if both indices are equal.</returns>
        public static bool operator ==(Position lhs, Position rhs)
        {
            return lhs.Index == rhs.Index;
        }

        /// <summary>
        /// Inequality comparer for <see cref="Position"/> indices
        /// </summary>
        /// <param name="lhs">The position index on the left-hand-side</param>
        /// <param name="rhs">The position index on the right-hand-side</param>
        /// <returns><c>true</c> if both indices are <b>not</b> equal.</returns>
        public static bool operator !=(Position lhs, Position rhs)
        {
            return lhs.Index != rhs.Index;
        }

        /// <summary>
        /// Implicit conversion operator for <see cref="Position"/> to <see cref="int"/> conversion.
        /// </summary>
        /// <param name="pos">The position index</param>
        public static implicit operator int(Position pos)
        {
            return pos.Index;
        }
    }
}
