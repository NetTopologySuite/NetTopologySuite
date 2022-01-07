using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Triangulate.Tri
{
    /// <summary>
    /// Represents an edge in a <see cref="Tri"/>,
    /// to be used as a key for looking up Tris
    /// while building a triangulation.
    /// The edge value is normalized to allow lookup
    /// of adjacent triangles.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class TriEdge
    {
        /// <summary>
        /// Gets or sets a value indicating the start point of this <see cref="TriEdge"/>
        /// </summary>
        public Coordinate P0 { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating the end point of this <see cref="TriEdge"/>
        /// </summary>
        public Coordinate P1 { get; private set; }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="a">A coordinate</param>
        /// <param name="b">A coordinate</param>
        public TriEdge(Coordinate a, Coordinate b)
        {
            P0 = a;
            P1 = b;
            Normalize();
        }

        private void Normalize()
        {
            if (P0.CompareTo(P1) < 0)
            {
                var tmp = P0;
                P0 = P1;
                P1 = tmp;
            }
        }

        public override int GetHashCode()
        {
            int result = 17;
            result = 37 * result + P0.X.GetHashCode();
            result = 37 * result + P1.X.GetHashCode();
            result = 37 * result + P0.Y.GetHashCode();
            result = 37 * result + P1.Y.GetHashCode();
            return result;
        }

        public override bool Equals(object arg)
        {
            if (!(arg is TriEdge other))
                return false;
            if (P0.Equals(other.P0) && P1.Equals(other.P1))
                return true;
            return false;
        }

        public override string ToString()
        {
            return WKTWriter.ToLineString(new Coordinate[] { P0, P1 });
        }
    }

}
